namespace Gloom.Server;
public interface IMessageSender : IDisposable
{
	public Task<int> SendAsync<T>(Filter filter, Guid opCode, T data);
}
