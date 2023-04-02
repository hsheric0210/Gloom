namespace Gloom.Server;
internal interface IMessageHandler
{
	public Guid[] AcceptedOps { get; }
	public Task HandleAsync(Client sender, Guid op, byte[] data);
}
