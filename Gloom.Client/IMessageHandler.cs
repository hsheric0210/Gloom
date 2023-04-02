namespace Gloom.Client
{
	internal interface IMessageHandler
	{
		public Guid[] AcceptedOps { get; }
		public Task HandleAsync(Guid op, byte[] data);
	}
}