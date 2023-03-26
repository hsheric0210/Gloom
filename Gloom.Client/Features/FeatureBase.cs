namespace Gloom.Client.Features;
public abstract class FeatureBase : IMessageHandler
{
	private readonly IMessageSender sender;
	public abstract Guid[] AcceptedOps { get; }

	protected FeatureBase(IMessageSender sender) => this.sender = sender;

	public abstract Task HandleAsync(Guid op, byte[] data);

	protected async Task SendAsync<T>(Guid op, T data, bool eom) where T : struct => await sender.SendAsync(op, data, eom);
}
