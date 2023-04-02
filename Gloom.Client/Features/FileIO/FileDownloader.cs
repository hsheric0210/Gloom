using System.Buffers;
using System.Security.Cryptography;

namespace Gloom.Client.Features.FileIO;
internal class FileDownloader : FeatureBase
{
	public override Guid[] AcceptedOps => new Guid[] { OpCodes.DownloadFileRequest };

	public FileDownloader(IMessageSender sender) : base(sender)
	{
	}

	public override async Task HandleAsync(Guid op, byte[] data)
	{
		var req = data.Deserialize<OpStructs.DownloadFileRequest>();
		var ident = req.Sid;
		var bufferSize = req.BufferSize;

		// Acquire file informations
		FileInfo info;
		try
		{
			info = new FileInfo(req.Source);
		}
		catch (Exception ex)
		{
#if DEBUG
			Console.WriteLine("Exception reading file: " + ex);
#endif
			await SendAsync(OpCodes.DownloadFilePreResponse, new OpStructs.DownloadFilePreResponse
			{
				Sid = ident,
				ErrorCode = FileIOError.ReadingFailed,
				TotalChunkCount = -1
			}, true);
			return;
		}

		// Check if the file really exists
		if (!info.Exists)
		{
			await SendAsync(OpCodes.DownloadFilePreResponse, new OpStructs.DownloadFilePreResponse { Sid = ident, ErrorCode = FileIOError.InvalidPath, TotalChunkCount = -1 }, true);
			return;
		}

		var buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
		using var ihash = IncrementalHash.CreateHash(HashAlgorithmName.SHA512);
		try
		{
			var size = info.Length;
			var expectedTotalChunks = (size - size % bufferSize) / bufferSize + (size % bufferSize > 0 ? 1 : 0);
			await SendAsync(OpCodes.DownloadFilePreResponse, new OpStructs.DownloadFilePreResponse
			{
				Sid = ident,
				ErrorCode = 0,
				TotalChunkCount = expectedTotalChunks
			}, true);

			using var fs = info.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			long index = 0;
			for (int bytesRead; (bytesRead = fs.Read(buffer, 0, buffer.Length)) != 0; index++)
			{
				ihash.AppendData(buffer, 0, bytesRead);
				await SendAsync(OpCodes.DownloadFileChunkResponse, new OpStructs.DownloadFileChunkResponse { Sid = ident, ChunkIndex = index, Data = buffer[..bytesRead] }, true);
			}
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(buffer);
			await SendAsync(OpCodes.DownloadFilePostResponse, new OpStructs.DownloadFilePostResponse { Sid = ident, ErrorCode = 0, Sha512Hash = ihash.GetCurrentHash() }, true);
		}
	}
}
