namespace Gloom.Server.Features;
public abstract class FeatureBase : IMessageHandler
{
	private readonly IMessageSender sender;
	internal string CommandPrefix { get; }

	public abstract Guid[] AcceptedOps { get; }

	protected FeatureBase(IMessageSender sender, string commandPrefix)
	{
		this.sender = sender;
		CommandPrefix = commandPrefix;
	}

	public abstract Task HandleAsync(Client client, Guid op, byte[] data);

	public abstract Task<bool> HandleCommandAsync(string[] args);

	protected async Task<int> SendAsync<T>(Filter filter, Guid op, T data) where T : struct => await sender.SendAsync(filter, op, data);

	protected async Task SendAsync<T>(string to, Guid op, T data) where T : struct => await sender.SendAsync(new Filter(FilterType.Equals, to), op, data);
}
