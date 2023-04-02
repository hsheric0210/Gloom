namespace Gloom.Client.Features
{
	internal class SelfUpdater : FeatureBase
	{
		public override Guid[] AcceptedOps => throw new NotImplementedException();

		public SelfUpdater(IMessageSender sender) : base(sender)
		{
		}

		public override Task HandleAsync(Guid op, byte[] data) => throw new NotImplementedException();
	}
}