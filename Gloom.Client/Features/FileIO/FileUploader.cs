namespace Gloom.Client.Features.FileIO;
internal class FileUploader : FeatureBase
{
	public override Guid[] AcceptedOps => new Guid[] { OpCodes.UploadFileRequest };

	public FileUploader(IMessageSender sender) : base(sender)
	{
	}

	public override async Task HandleAsync(Guid op, byte[] data)
	{
		var req = StructConvert.Bytes2Struct<OpStructs.UploadFileRequest>(data);
		try
		{
			Path.GetFullPath(req.Destination);
		}
		catch
		{
			await SendAsync(OpCodes.UploadFileResponse, new OpStructs.UploadFileResponse { }, true);
			return;
		}
		File.WriteAllBytes(req.Destination, data);
		await SendAsync(OpCodes.UploadFileResponse, new OpStructs.UploadFileResponse { }, true);
	}
}
