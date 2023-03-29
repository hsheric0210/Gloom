using Gloom.Server.Features.InfoCollector.Wmi;
using Microsoft.Win32;
using Serilog;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gloom.Server.Features;
internal class FileUploader : FeatureBase
{
	private IDictionary<Guid, OpStructs.UploadFileRequest> registry = new Dictionary<Guid, OpStructs.UploadFileRequest>();

	public FileUploader(IMessageSender sender) : base(sender, "up")
	{
	}

	public override Guid[] AcceptedOps => new Guid[] { OpCodes.UploadFileResponse };

	public override async Task HandleAsync(string from, Guid op, byte[] data)
	{
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

		var ident = Guid.NewGuid();
		var dst = args[2];

		var buffer = ArrayPool<byte>.Shared.Rent(163840);
		long index = 0;
		try
		{
			using FileStream fs = File.Open(dst, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			var taskList = new List<Task>();
			for (int bytesRead; (bytesRead = fs.Read(buffer, 0, buffer.Length)) != 0; index++)
				taskList.Add(Task.Run(async () => await SendAsync(filter, OpCodes.UploadFileChunkRequest, new OpStructs.UploadFileChunkResponse { Ident = ident, ChunkIndex = index, Data = buffer[..bytesRead] }, false)));
			await Task.WhenAll(taskList);
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(buffer);
			await SendAsync(filter, OpCodes.UploadFileRequest, new OpStructs.UploadFileRequest { Ident = ident, TotalChunkCount = index }, true);
		}

		Log.Information("Sent environment variable list request to clients.");
		return true;
	}
}
