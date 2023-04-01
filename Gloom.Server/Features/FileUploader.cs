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
		if (rsp.ErrorCode != 0)
		{
			Log.Error("Transfer failed with error code: {code}", rsp.ErrorCode);
			return;
		}

		if (hashRegistry.ContainsKey(rsp.Ident) && rsp.Sha512Hash.Length > 0)
		{
			var hash = hashRegistry[rsp.Ident];
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

		const int bufferSize = 8388608; // 8MB

		var ident = Guid.NewGuid();
		var dst = args[2];

		var buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
		try
		{
			var info = new FileInfo(src);
			var size = info.Length;
			var expectedTotalChunks = (size - size % bufferSize) / bufferSize + (size % bufferSize > 0 ? 1 : 0);
			await SendAsync(filter, OpCodes.UploadFilePreRequest, new OpStructs.UploadFilePreRequest
			{
				Ident = ident,
				Destination = dst,
				TotalChunkCount = expectedTotalChunks
			}, true);

			using var ihash = IncrementalHash.CreateHash(HashAlgorithmName.SHA512);
			using var fs = info.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			long index = 0;
			for (int bytesRead; (bytesRead = fs.Read(buffer, 0, buffer.Length)) != 0; index++)
			{
				ihash.AppendData(buffer, 0, bytesRead);
				await SendAsync(filter, OpCodes.UploadFileChunkRequest, new OpStructs.UploadFileChunkRequest { Ident = ident, ChunkIndex = index, Data = buffer[..bytesRead] }, false);

				// Print progress
				if (!Console.IsOutputRedirected)
				{
					Console.CursorLeft = 0;
					Console.Write($"Sending chunks: {(index + 1) / expectedTotalChunks}% [{index + 1} / {expectedTotalChunks}]");
				}
			}
			Console.WriteLine(" ... All chunks have been sent.");
			hashRegistry[ident] = ihash.GetCurrentHash();
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(buffer);
			await SendAsync(filter, OpCodes.UploadFilePostRequest, new OpStructs.UploadFilePostRequest
			{
				Ident = ident,
				Destination = dst,
				BufferSize = bufferSize
			}, true);
		}

		Log.Information("Sent the specified file to the clients clients.");
		return true;
	}
}
