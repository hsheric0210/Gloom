using Serilog;
using System.Buffers;
using System.Security.Cryptography;

namespace Gloom.Server.Features;
internal class FileUploader : FeatureBase
{
	private IDictionary<Guid, byte[]> hashRegistry = new Dictionary<Guid, byte[]>();

	public FileUploader(IMessageSender sender) : base(sender, "up")
	{
	}

	public override Guid[] AcceptedOps => new Guid[] { OpCodes.UploadFileResponse };

	public override async Task HandleAsync(string from, Guid op, byte[] data)
	{
		var rsp = StructConvert.Bytes2Struct<OpStructs.UploadFileResponse>(data);
		var ident = new Guid(rsp.Ident);
		if (rsp.ErrorCode != 0)
		{
			Log.Error("Transfer failed with error code: {code}", rsp.ErrorCode);
			return;
		}

		if (hashRegistry.ContainsKey(ident) && rsp.Sha512Hash.Length > 0)
		{
			var hash = hashRegistry[ident];
			if (hash.SequenceEqual(rsp.Sha512Hash))
				Log.Information("Hash matches: {hash}", hash);
			else
				Log.Error("Hash mismatches: local={myhash} vs remote={remotehash}", hash, rsp.Sha512Hash);
		}
	}

	public override async Task<bool> HandleCommandAsync(string[] args)
	{
		if (args.Length < 3)
			return false;
		var filter = Filter.Parse(args[0]);
		var src = args[1];
		if (!File.Exists(src))
		{
			Log.Warning("Local file {file} does not exists!", src);
			return true;
		}

		const int bufferSize = 33554432; // 32MB

		var ident = Guid.NewGuid();
		var dst = args[2];

		var buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
		long index = 0;
		try
		{
			var fi = new FileInfo(src);
			var size = fi.Length;
			await SendAsync(filter, OpCodes.UploadFileRequest, new OpStructs.UploadFileRequest
			{
				Ident = ident,
				Destination = dst,
				TotalChunkCount = ((size - (size % bufferSize)) / bufferSize) + (size % bufferSize > 0 ? 1 : 0),
				BufferSize = bufferSize,
				EoT = false
			}, true);

			using var ihash = IncrementalHash.CreateHash(HashAlgorithmName.SHA512);
			using FileStream fs = fi.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			for (int bytesRead; (bytesRead = fs.Read(buffer, 0, buffer.Length)) != 0; index++)
			{
				ihash.AppendData(buffer, 0, bytesRead);
				SendAsync(filter, OpCodes.UploadFileChunkRequest, new OpStructs.UploadFileChunkRequest { Ident = ident, ChunkIndex = index, Data = buffer[..bytesRead] }, false);
			}
			hashRegistry[ident] = ihash.GetCurrentHash();
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(buffer);
			await SendAsync(filter, OpCodes.UploadFileRequest, new OpStructs.UploadFileRequest
			{
				Ident = ident,
				Destination = dst,
				TotalChunkCount = index,
				BufferSize = bufferSize,
				EoT = true
			}, true);
		}

		Log.Information("Sent environment variable list request to clients.");
		return true;
	}
}
