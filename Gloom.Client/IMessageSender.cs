namespace Gloom;
public interface IMessageSender
{
	public Task SendAsync<T>(Guid opCode, T data, bool eom) where T : struct;
}
