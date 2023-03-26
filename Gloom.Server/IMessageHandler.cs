namespace Gloom.Server;
internal interface IMessageHandler
{
	public Guid[] AcceptedOps { get; }
	public Task HandleAsync(string from, Guid op, byte[] data);
}
