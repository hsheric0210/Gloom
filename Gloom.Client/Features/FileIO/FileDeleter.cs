namespace Gloom.Client.Features.FileIO
{
	internal class FileDeleter : FeatureBase
	{
		public override Guid[] AcceptedOps => new Guid[] { OpCodes.DeleteFileRequest };

		public FileDeleter(IMessageSender sender) : base(sender)
		{
		}

		public override async Task HandleAsync(Guid op, byte[] data)
		{
			if (op != OpCodes.DeleteFileRequest)
				return;

			var req = data.Deserialize<OpStructs.DeleteFileRequest>();
			try
			{
				var info = new FileInfo(req.FilePath);
				if (!info.Exists)
				{
					await Respond(FileIOError.InvalidPath);
					return;
				}
				info.Delete();
				await Respond(0);
			}
			catch (Exception ex)
			{
#if DEBUG
				Console.WriteLine("Deletion failed with error: " + ex);
#endif
				await Respond(FileIOError.DeletionFailed);
			}

			// NOTE: This is local function, not an weird expression, for those whom doesn't know latest C# features.
			async Task Respond(int errorCode) => await SendAsync(OpCodes.DeleteFileResponse, new OpStructs.DeleteFileResponse { ErrorCode = errorCode }, true);
		}
	}
}