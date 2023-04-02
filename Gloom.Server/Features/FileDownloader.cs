using Serilog;
using System.Buffers;
using System.Security.Cryptography;

namespace Gloom.Server.Features;
internal class FileDownloader : FeatureBase
{
	private IDictionary<Guid, DownloadConfig> configs = new Dictionary<Guid, DownloadConfig>();
	private IDictionary<Guid, DownloadSession> sessions = new Dictionary<Guid, DownloadSession>();

	private sealed record DownloadConfig(string SaveLocation, int BufferSize);
	private sealed record DownloadSession(string[] Chunks, CountdownEvent Counter);

	public FileDownloader(IMessageSender sender) : base(sender, "dl")
	{
	}

	public override Guid[] AcceptedOps => new Guid[] { OpCodes.DownloadFilePreResponse, OpCodes.DownloadFileChunkResponse, OpCodes.DownloadFilePostResponse };

	public override async Task HandleAsync(string from, Guid op, byte[] data)
	{
		if (op == OpCodes.DownloadFilePreResponse)
		{
			var req = StructConvert.Bytes2Struct<OpStructs.DownloadFilePreResponse>(data);

			if (req.ErrorCode != 0)
			{
				Log.Error("Error code: {code}", req.ErrorCode);
				return;
			}

			if (sessions.ContainsKey(req.Ident))
			{
				Log.Warning("Tried to re-start existing download session: {ident}", req.Ident);
				return;
			}

			var chunkFiles = new string[req.TotalChunkCount];
			var chunkFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(chunkFolder);
			for (var i = 0; i < req.TotalChunkCount; i++)
				chunkFiles[i] = Path.Combine(chunkFolder, Path.GetRandomFileName());
			sessions[req.Ident] = new DownloadSession(chunkFiles, new CountdownEvent((int)req.TotalChunkCount));
		}
		else if (op == OpCodes.DownloadFileChunkResponse)
		{
			var req = StructConvert.Bytes2Struct<OpStructs.DownloadFileChunkResponse>(data);
			if (!sessions.TryGetValue(req.Ident, out var session))
			{
				Log.Warning("Unauthorized file chunk received: {ident}", req.Ident);
				return;
			}
			if (session.Chunks.Length <= req.ChunkIndex)
			{
				Log.Warning("Out-of-index file chunk response received: {index} with {ident}", req.ChunkIndex, req.Ident);
				return;
			}

			await File.WriteAllBytesAsync(session.Chunks[req.ChunkIndex], req.Data);
			Console.CursorLeft = 0;
			Console.Write($"{req.ChunkIndex} / {session.Chunks.Length} chunks received");
			session.Counter.Signal();
		}
		else if (op == OpCodes.DownloadFilePostResponse)
		{
			var req = StructConvert.Bytes2Struct<OpStructs.DownloadFilePostResponse>(data);
			var ident = req.Ident;

			if (req.ErrorCode != 0)
			{
				Log.Error("Error code {code}", req.ErrorCode);
				return;
			}

			if (!configs.TryGetValue(ident, out var config))
			{
				Log.Warning("Unconfigured file chunk received: {ident}", ident);
				return;
			}
			if (!sessions.TryGetValue(ident, out var session))
			{
				Log.Warning("Unauthorized file download response received: {ident}", ident);
				return;
			}
			session.Counter.Wait();
			session.Counter.Dispose();

			Console.WriteLine();
			Log.Information("Finished receiving file chunks.");

			// Hash and Combine
			var buffer = ArrayPool<byte>.Shared.Rent(config.BufferSize);
			try
			{
				using var ihash = IncrementalHash.CreateHash(HashAlgorithmName.SHA512);
				using (var stream = File.Open(config.SaveLocation, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
				{
					for (var i = 0; i < session.Chunks.Length; i++)
					{
						try
						{
							using var chunkStream = File.Open(session.Chunks[i], FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
							for (int bytesRead; (bytesRead = chunkStream.Read(buffer, 0, buffer.Length)) != 0;)
							{
								stream.Write(buffer, 0, bytesRead);
								ihash.AppendData(buffer, 0, bytesRead);
							}
						}
						catch (Exception ex)
						{
							Log.Error(ex, "Exception during writing chunk #{index}", i);
						}
					}
				}

				Log.Information("Finished combining file chunks.");

				var hash = ihash.GetCurrentHash();
				if (hash.SequenceEqual(req.Sha512Hash))
					Log.Information("Hash matches: {hash}", req.Sha512Hash);
				else
					Log.Error("Hash mismatches: remote={remoteHash}, local={localHash}", req.Sha512Hash, hash);

				// Delete chunks
				Directory.Delete(Path.GetDirectoryName(session.Chunks[0])!, true);
				configs.Remove(ident);
				sessions.Remove(ident);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Exception during writing");
			}
			finally
			{
				ArrayPool<byte>.Shared.Return(buffer);
			}
		}
	}

	public override async Task<bool> HandleCommandAsync(string[] args)
	{
		if (args.Length < 3)
			return false;
		var filter = Filter.Parse(args[0]);
		var src = args[1];
		var dst = args[2];
		try
		{
			await File.WriteAllBytesAsync(dst, new byte[] { 0x13, 0x37, 0x69, 0x74 });
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Local file {file} is not writable!", dst);
			return true;
		}

		const int bufferSize = 8388608; // 8MB

		var ident = Guid.NewGuid();
		configs[ident] = new DownloadConfig(dst, bufferSize);
		await SendAsync(filter, OpCodes.DownloadFileRequest, new OpStructs.DownloadFileRequest
		{
			Ident = ident,
			Source = src,
			BufferSize = bufferSize
		}, true);

		Log.Information("Sent the download file request to the clients. Ident={ident}");
		return true;
	}
}
