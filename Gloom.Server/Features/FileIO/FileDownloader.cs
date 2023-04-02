using Serilog;
using System.Buffers;
using System.Security.Cryptography;

namespace Gloom.Server.Features.FileIO;
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

	private bool SingleSessionRunning() => sessions.Count <= 1;

	private string AppendSuffixes(string original, string from) => SingleSessionRunning() ? original : Path.GetFileNameWithoutExtension(original) + $".{from}" + Path.GetExtension(original);

	public override async Task HandleAsync(Client client, Guid op, byte[] data)
	{
		if (op == OpCodes.DownloadFilePreResponse)
		{
			var req = data.Deserialize<OpStructs.DownloadFilePreResponse>();

			if (req.ErrorCode != 0)
			{
				Log.Error("{client} returned download initiation error: {code}", client, req.ErrorCode);
				return;
			}

			if (sessions.ContainsKey(req.Sid))
			{
				Log.Warning("{client} tried to re-open existing download session {sid}.", client, req.Sid);
				return;
			}

			var chunkFiles = new string[req.TotalChunkCount];
			var chunkFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(chunkFolder);
			for (var i = 0; i < req.TotalChunkCount; i++)
				chunkFiles[i] = Path.Combine(chunkFolder, Path.GetRandomFileName());
			sessions[req.Sid] = new DownloadSession(chunkFiles, new CountdownEvent((int)req.TotalChunkCount));
		}
		else if (op == OpCodes.DownloadFileChunkResponse)
		{
			var req = data.Deserialize<OpStructs.DownloadFileChunkResponse>();
			if (!sessions.TryGetValue(req.Sid, out var session))
			{
				Log.Warning("{client} tried to send file chunk response of unauthorized session {sid}.", client, req.Sid);
				return;
			}
			if (session.Chunks.Length <= req.ChunkIndex)
			{
				Log.Warning("{client} responded out-of-bounds file chunk (index: {index}, total: {total}) on download session {sid}.", client, req.ChunkIndex, session.Chunks.Length, req.Sid);
				return;
			}

			await File.WriteAllBytesAsync(session.Chunks[req.ChunkIndex], req.Data);

			if (SingleSessionRunning()) // On multiple-session mode, process-bar may be broke
			{
				Console.CursorLeft = 0;
				Console.Write($"{req.ChunkIndex} / {session.Chunks.Length} chunks received");
			}

			session.Counter.Signal();
		}
		else if (op == OpCodes.DownloadFilePostResponse)
		{
			var req = data.Deserialize<OpStructs.DownloadFilePostResponse>();
			var sid = req.Sid;

			if (req.ErrorCode != 0)
			{
				Log.Error("{client} returned error download cleanup error: {code}", client, req.ErrorCode);
				return;
			}

			if (!configs.TryGetValue(sid, out var config))
			{
				Log.Warning("{client} tried to send download finish response of unconfirmed session {sid}.", client, sid);
				return;
			}
			if (!sessions.TryGetValue(sid, out var session))
			{
				Log.Warning("{client} tried to send download finish response of unauthorized session {sid}.", client, sid);
				return;
			}
			session.Counter.Wait();
			session.Counter.Dispose();

			if (SingleSessionRunning()) // On multiple-session mode, process-bar may be broke
				Console.WriteLine();

			Log.Information("Finished receiving file chunks from {client}.", client);

			// Hash and Combine
			var buffer = ArrayPool<byte>.Shared.Rent(config.BufferSize);
			try
			{
				using var ihash = IncrementalHash.CreateHash(HashAlgorithmName.SHA512);
				using (var stream = File.Open(AppendSuffixes(config.SaveLocation, client.Name), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
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
							Log.Error(ex, "Exception during writing chunk #{index} received from ", i, client);
						}
					}
				}

				Log.Information("Finished combining file chunks from {client}.", client);

				var hash = ihash.GetCurrentHash();
				if (hash.SequenceEqual(req.Sha512Hash))
					Log.Information("Hash of the file received from {client} matches: {hash}", client, req.Sha512Hash);
				else
					Log.Error("Hash of the file received from {client} mismatches: remote={remoteHash}, local={localHash}", client, req.Sha512Hash, hash);

				// Delete chunks
				Directory.Delete(Path.GetDirectoryName(session.Chunks[0])!, true);
				configs.Remove(sid);
				sessions.Remove(sid);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Exception during processing {client}, session {sid}", client, sid);
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
		if (!(args.Length >= 4 && int.TryParse(args[3], out var bufferSize) && bufferSize > 0))
			bufferSize = 8388608; // 8MB by default

		try
		{
			await File.WriteAllBytesAsync(dst, new byte[] { 0x13, 0x37, 0x69, 0x74 });
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Local file {file} is not writable!", dst);
			return true;
		}

		var sid = Guid.NewGuid();
		configs[sid] = new DownloadConfig(dst, bufferSize);
		await SendAsync(filter, OpCodes.DownloadFileRequest, new OpStructs.DownloadFileRequest
		{
			Sid = sid,
			Source = src,
			BufferSize = bufferSize
		});

		Log.Information("Sent the download file request to the clients. Session {sid}", sid);
		return true;
	}
}
