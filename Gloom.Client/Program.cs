namespace Gloom
{
	internal class Program
	{
		public const string CommandServer = "ws://127.0.0.1:8683";


		static void Main(string[] args)
		{
			_ = new MessageClient(new Uri(CommandServer));
		}
	}
}
