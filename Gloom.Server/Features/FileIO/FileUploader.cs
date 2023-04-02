using Serilog;
using System.Buffers;
using System.Security.Cryptography;

namespace Gloom.Server.Features.FileIO
{
	internal class FileUploader : FeatureBase
	{
		private readonly IDictionary<Guid, byte[]> hashRegistry = new Dictionary<Guid, byte[]>();

		public FileUploader(IMessageSender sender) : base(sender, "up")
		{
		}

		public override Guid[] AcceptedOps => new Guid[] { OpCodes.UploadFileResponse };

		public override async Task HandleAsync(Client client, Guid op, byte[] data)
		{
			var rsp = data.Deserialize<OpStructs.UploadFileResponse>();
			if (rsp.ErrorCode != 0)
			{
				Log.Error("{client} returned uploading error: {error}", rsp.ErrorCode);
				return;
			}

			if (hashRegistry.ContainsKey(rsp.Sid) && rsp.Sha512Hash.Length > 0)
			{
				var hash = hashRegistry[rsp.Sid];
				if (hash.SequenceEqual(rsp.Sha512Hash))
					Log.Information("Hash of the file sent to {client} matches: {hash}", client, hash);
				else
					Log.Error("Hash of the file sent to {client} mismatches: local={myhash} vs remote={remotehash}", client, hash, rsp.Sha512Hash);
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
			int count;
			try
			{
				var info = new FileInfo(src);
				var size = info.Length;
				var expectedTotalChunks = (size - size % bufferSize) / bufferSize + (size % bufferSize > 0 ? 1 : 0);
				await SendAsync(filter, OpCodes.UploadFilePreRequest, new OpStructs.UploadFilePreRequest
				{
					Sid = ident,
					Destination = dst,
					TotalChunkCount = expectedTotalChunks
				});

				using var ihash = IncrementalHash.CreateHash(HashAlgorithmName.SHA512);
				using var fs = info.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				long index = 0;
				for (int bytesRead; (bytesRead = fs.Read(buffer, 0, buffer.Length)) != 0; index++)
				{
					ihash.AppendData(buffer, 0, bytesRead);
					await SendAsync(filter, OpCodes.UploadFileChunkRequest, new OpStructs.UploadFileChunkRequest { Sid = ident, ChunkIndex = index, Data = buffer[..bytesRead] });

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
				count = await SendAsync(filter, OpCodes.UploadFilePostRequest, new OpStructs.UploadFilePostRequest
				{
					Sid = ident,
					Destination = dst,
					BufferSize = bufferSize
				});
			}

			Log.Information("Sent the specified file to the {count} clients.", count);
			return true;
		}
	}
}