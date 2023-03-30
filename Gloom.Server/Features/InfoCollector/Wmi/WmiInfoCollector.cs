﻿using Gloom.WmiOps;
using Serilog;

namespace Gloom.Server.Features.InfoCollector.Wmi;
internal class WmiInfoCollector : FeatureBase
{
	public override Guid[] AcceptedOps => new Guid[] { OpCodes.WmiInfoResponse };
	private readonly IReadOnlySet<WmiInfo> registry = new HashSet<WmiInfo>()
	{
		new WmiProcessList(),
		new WmiServiceList(),
		new WmiHardwareInfo(),
		new WmiDiskInfo()
	};

	public WmiInfoCollector(IMessageSender sender) : base(sender, "wmi")
	{

	}

	public override async Task HandleAsync(string from, Guid op, byte[] data)
	{
		WmiInfoResponse rsp = StructConvert.Bytes2Struct<WmiInfoResponse>(data);
		foreach (WmiInfo wi in registry.Where(r => r.WmiOp == rsp.WmiOp))
			await wi.Handle(from, rsp.Data);
	}

	public override async Task<bool> HandleCommandAsync(string[] args)
	{
		if (args.Length < 2)
			return false;
		var filter = Filter.Parse(args[0]);
		var count = 0;
		foreach (WmiInfo wi in registry.Where(r => string.Equals(r.Command, args[1])))
			count += await SendAsync(filter, OpCodes.WmiInfoRequest, new WmiInfoRequest { WmiOp = wi.WmiOp }, true);
		Log.Information("Sent environment variable list request to total {count} clients.", count);
		return true;
	}
}