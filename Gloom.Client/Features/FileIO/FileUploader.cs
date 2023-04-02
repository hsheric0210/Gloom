using System.Buffers;
using System.Security.Cryptography;

namespace Gloom.Client.Features.FileIO;
internal class FileUploader : FeatureBase
{
	public override Guid[] AcceptedOps => new Guid[] { OpCodes.UploadFilePreRequest, OpCodes.UploadFileChunkRequest, OpCodes.UploadFilePostRequest };
	private string[]? chunks;
	private CountdownEvent? counter;

	public FileUploader(IMessageSender sender) : base(sender)
	{
	}

	public override async Task HandleAsync(Guid op, byte[] data)
	{
		if (op == OpCodes.UploadFilePreRequest)
		{
			// Initiate uploading
			var req = data.Deserialize<OpStructs.UploadFilePreRequest>();
			try
			{
				File.WriteAllBytes(req.Destination, new byte[] { 0x13, 0x37 });
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Following path is unwritable '{req.Destination}' : {ex}");
				await SendError(req.Sid, FileIOError.InvalidPath);
				return;
			}

			counter = new CountdownEvent((int)req.TotalChunkCount);
			chunks = new string[req.TotalChunkCount];
			var chunkFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(chunkFolder);
			for (var i = 0; i < req.TotalChunkCount; i++)
				chunks[i] = Path.Combine(chunkFolder, Path.GetRandomFileName());
		}
		else if (op == OpCodes.UploadFileChunkRequest && chunks != null && counter != null)
		{
			// Receive file chunks
			var req = data.Deserialize<OpStructs.UploadFileChunkRequest>();
			if (chunks.Length <= req.ChunkIndex)
			{
#if DEBUG
				Console.WriteLine("Out-of-index chunk received: #" + req.ChunkIndex);
#endif
				return;
			}

			await File.WriteAllBytesAsync(chunks[req.ChunkIndex], req.Data);
			counter.Signal();
		}
		else if (op == OpCodes.UploadFilePostRequest && chunks?.Length > 0 && counter != null)
		{
			// Finish uploading
			var req = data.Deserialize<OpStructs.UploadFilePostRequest>();
			counter.Wait(); // Wait for all chunks to finish

			// After all chunk finished, dispose the counter
			counter.Dispose();
			counter = null;

			// Hash and Combine
			var buffer = ArrayPool<byte>.Shared.Rent(req.BufferSize);
			try
			{
				using var ihash = IncrementalHash.CreateHash(HashAlgorithmName.SHA512);
				using (var stream = File.Open(req.Destination, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
				{
					for (var i = 0; i < chunks.Length; i++)
					{
						try
						{
							using var chunkStream = File.Open(chunks[i], FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
							for (int bytesRead; (bytesRead = chunkStream.Read(buffer, 0, buffer.Length)) != 0;)
							{
								stream.Write(buffer, 0, bytesRead);
								ihash.AppendData(buffer, 0, bytesRead);
							}
						}
						catch (Exception ex)
						{
#if DEBUG
							Console.WriteLine($"Error combining chunk #{i}: {ex}");
#endif
							await SendError(req.Sid, FileIOError.ChunkCombiningFailed);
						}
					}
				}

				await SendAsync(OpCodes.UploadFileResponse, new OpStructs.UploadFileResponse
				{
					Sid = req.Sid,
					ErrorCode = 0,
					Sha512Hash = ihash.GetCurrentHash()
				}, true);

				// Delete chunks
				Directory.Delete(Path.GetDirectoryName(chunks[0])!, true);
			}
			catch (Exception ex)
			{
#if DEBUG
				Console.WriteLine($"Error writing: {ex}");
#endif
				await SendError(req.Sid, FileIOError.WritingFailed);
			}
			finally
			{
				ArrayPool<byte>.Shared.Return(buffer, true);
			}

			chunks = null;
		}
	}

	private async Task SendError(Guid ident, int errorCode) => await SendAsync(OpCodes.UploadFileResponse, new OpStructs.UploadFileResponse { Sid = ident, ErrorCode = errorCode, Sha512Hash = Array.Empty<byte>() }, true);
}
