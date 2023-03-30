using System.Buffers;
using System.Security.Cryptography;

namespace Gloom.Client.Features.FileIO;
internal class FileUploader : FeatureBase
{
	public override Guid[] AcceptedOps => new Guid[] { OpCodes.UploadFileRequest, OpCodes.UploadFileChunkRequest };
	private string[]? chunks;
	private CountdownEvent? counter;

	public FileUploader(IMessageSender sender) : base(sender)
	{
	}

	public override async Task HandleAsync(Guid op, byte[] data)
	{
		// UploadRequest(EoT=false) -> UploadChunkRequest(s) -> UploadRequest(EoT=true)
		if (op == OpCodes.UploadFileChunkRequest && chunks != null && counter != null)
		{
			OpStructs.UploadFileChunkRequest req = StructConvert.Bytes2Struct<OpStructs.UploadFileChunkRequest>(data);
#if DEBUG
			Console.WriteLine($"Received chunk #{req.ChunkIndex}");
#endif
			await File.WriteAllBytesAsync(chunks[req.ChunkIndex], req.Data);
#if DEBUG
			Console.WriteLine($"Successfully received file chunk #{req.ChunkIndex} from connection {req.Ident} hash={Convert.ToHexString(SHA512.HashData(req.Data))}");
#endif
			counter?.Signal();
		}
		else
		{
			OpStructs.UploadFileRequest req = StructConvert.Bytes2Struct<OpStructs.UploadFileRequest>(data);
			if (req.EoT && chunks != null)
			{
				// Verify chunks and concat & delete temp files
				counter?.Wait(); // Wait for all chunks to finish


				var buffer = ArrayPool<byte>.Shared.Rent(req.BufferSize);
				try
				{
					using var ihash = IncrementalHash.CreateHash(HashAlgorithmName.SHA512);
					using (FileStream stream = File.Open(req.Destination, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
					{
						// Combine chunks
						for (var i = 0; i < req.TotalChunkCount; i++)
						{
							try
							{
								using (FileStream chunkStream = File.Open(chunks[i], FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
								{
									for (int bytesRead; (bytesRead = chunkStream.Read(buffer, 0, buffer.Length)) != 0;)
									{
										stream.Write(buffer, 0, bytesRead);
										ihash.AppendData(buffer, 0, bytesRead);
									}
								}
#if DEBUG
								Console.WriteLine($"Combined chunk #{i}");
#endif
								File.Delete(chunks[i]);
							}
							catch (Exception ex)
							{
#if DEBUG
								Console.WriteLine($"Error combining chunk #{i}: {ex}");
#endif
								await SendError(req.Ident, FileIOError.ChunkCombiningFailed);
							}
						}
					}

					await SendAsync(OpCodes.UploadFileResponse, new OpStructs.UploadFileResponse
					{
						Ident = req.Ident.ToByteArray(),
						ErrorCode = 0,
						Sha512Hash = ihash.GetCurrentHash()
					}, true);
				}
				catch (Exception ex)
				{
#if DEBUG
					Console.WriteLine($"Error writing: {ex}");
#endif
					await SendError(req.Ident, FileIOError.WritingFailed);
				}
				finally
				{
					ArrayPool<byte>.Shared.Return(buffer);
				}

				chunks = null;
			}
			else
			{
				// Initiate transaction

				try
				{
					Path.GetFullPath(req.Destination); // Pre-verify target path
				}
				catch
				{
					await SendError(req.Ident, FileIOError.InvalidPath);
					return;
				}

				counter = new CountdownEvent((int)req.TotalChunkCount);
				chunks = new string[req.TotalChunkCount];
				var chunkFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
				Directory.CreateDirectory(chunkFolder);
				for (var i = 0; i < req.TotalChunkCount; i++)
					chunks[i] = Path.Combine(chunkFolder, Path.GetRandomFileName());
			}
		}
	}

	private async Task SendError(Guid ident, int errorCode) => await SendAsync(OpCodes.UploadFileResponse, new OpStructs.UploadFileResponse { Ident = ident.ToByteArray(), ErrorCode = errorCode, Sha512Hash = Array.Empty<byte>() }, true);
}
