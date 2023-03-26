namespace Gloom.Client.Features.Stealer.ClipboardLogger;
internal class ClipboardLoggerBase : FeatureBase
{
	public override Guid[] AcceptedOps => new Guid[] { OpCodes.ClipboardLoggerSettingRequest, OpCodes.ClipboardLogRequest };

	public ClipboardLoggerBase(IMessageSender sender) : base(sender)
	{
	}

	public override Task HandleAsync(Guid op, byte[] data) => throw new NotImplementedException();
}
