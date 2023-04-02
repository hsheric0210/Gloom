using Serilog;
using System.Buffers;
using System.Security.Cryptography;

namespace Gloom.Server.Features.FileIO;
internal class FileDeleter : FeatureBase
{
	public FileDeleter(IMessageSender sender) : base(sender, "rm")
	{
	}

	public override Guid[] AcceptedOps => new Guid[] { OpCodes.DeleteFileResponse };

	public override async Task HandleAsync(Client client, Guid op, byte[] data)
	{
		if (op != OpCodes.DeleteFileResponse)
			return;

		var req = data.Deserialize<OpStructs.DeleteFileResponse>();
		if (req.ErrorCode != 0)
		{
			Log.Error("{client} returned deletion error: {error}", client, req.ErrorCode);
			return;
		}

		Log.Information("{client} successfully deleted the specified file.", client);
	}

	public override async Task<bool> HandleCommandAsync(string[] args)
	{
		if (args.Length < 2)
			return false;
		var filter = Filter.Parse(args[0]);
		var target = args[1];
		var count = await SendAsync(filter, OpCodes.DeleteFileRequest, new OpStructs.DeleteFileRequest
		{
			FilePath = target
		});

		Log.Information("Sent the file deletion request to the {count} clients.", count);
		return true;
	}
}
