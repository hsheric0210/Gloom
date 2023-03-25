namespace Gloom.Features;
public abstract class BaseFeature : IMessageHandler
{
	private readonly IMessageSender sender;
	public abstract Guid[] AcceptedOps { get; }

	protected BaseFeature(IMessageSender sender) => this.sender = sender;

	public async virtual Task HandleAsync(Guid op, byte[] data) { }

	public async Task SendAsync<T>(Guid op, T data, bool eom) where T : struct => await sender.SendAsync(op, data, eom);
}
