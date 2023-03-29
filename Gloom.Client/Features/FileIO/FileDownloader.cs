using System.Buffers;

namespace Gloom.Client.Features.FileIO;
internal class FileDownloader : FeatureBase
{
	public override Guid[] AcceptedOps => new Guid[] { OpCodes.DownloadFileRequest };

	public FileDownloader(IMessageSender sender) : base(sender)
	{
	}

	public override async Task HandleAsync(Guid op, byte[] data)
	{
		var req = StructConvert.Bytes2Struct<OpStructs.DownloadFileRequest>(data);
		var info = new FileInfo(req.Source);
		if (!info.Exists)
		{
			await SendAsync(OpCodes.UploadFileResponse, new OpStructs.DownloadFileResponse { Ident = req.Ident, TotalChunkCount = -1 }, true);
			return;
		}

		var buffer = ArrayPool<byte>.Shared.Rent(163840);
		long index = 0;
		try
		{
			using FileStream fs = info.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			var taskList = new List<Task>();
			for (int bytesRead; (bytesRead = fs.Read(buffer, 0, buffer.Length)) != 0; index++)
				taskList.Add(Task.Run(async () => await SendAsync(OpCodes.DownloadFileChunkResponse, new OpStructs.DownloadFileChunkResponse { ChunkIndex = index, Data = buffer[..bytesRead] }, false)));
			await Task.WhenAll(taskList);
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(buffer);
			await SendAsync(OpCodes.DownloadFileResponse, new OpStructs.DownloadFileResponse { Ident = req.Ident, TotalChunkCount = index }, true);
		}
	}
}
