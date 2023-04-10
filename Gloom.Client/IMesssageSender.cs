namespace Gloom.Client
{
	public interface IMessageSender
	{
		public Task SendAsync(Guid opCode, object data);
	}
}