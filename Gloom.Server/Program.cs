using Gloom.Server.Features;
using Gloom.Server.Features.InfoCollector;
using Gloom.Server.Features.InfoCollector.Wmi;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using System.Text;

namespace Gloom.Server
{
	internal class Program
	{
		private const string LogFileName = "GloomLog.log";
		private const string LogDbName = "GloomLogDb.sqlite";

		private readonly ISet<FeatureBase> Features;
		private readonly IMessageSender sender;

		static void Main(string[] args)
		{
			Log.Logger = new LoggerConfiguration()
				.WriteTo.Console(theme: AnsiConsoleTheme.Code)
				.WriteTo.Async(a => a.File(LogFileName, fileSizeLimitBytes: 134217728 /*128MB*/, rollOnFileSizeLimit: true, encoding: new UTF8Encoding(false)), 16384, true)
				.WriteTo.Async(a => a.SQLite(LogDbName)).CreateLogger();
			new Program(new MessageServer("ws://0.0.0.0:8683"));
		}

		public Program(MessageServer server)
		{
			sender = server;
			Features = new HashSet<FeatureBase>()
			{
				new EnvVarsCollector(server),
				new WmiInfoCollector(server),
				new FileUploader(server),
				new FileDownloader(server)
			};
			foreach (var feature in Features)
				server.RegisterHandler(feature);
			CommandLoop();
		}

		private void CommandLoop()
		{
			while (true)
			{
				var cmd = Console.ReadLine();
				var split = cmd.Trim().SplitOutsideQuotes(' ');
				if (split.Length == 0)
					continue;

				// Handle exit
				if (string.Equals(split[0], "exit", StringComparison.OrdinalIgnoreCase))
					break;

				Task.Run(async () =>
				{
					var param = split.Length > 1 ? split[1..] : Array.Empty<string>();
					var features = Features.Where(f => string.Equals(f.CommandPrefix, split[0], StringComparison.OrdinalIgnoreCase)).ToList();
					Log.Information("There're {count} features matching command prefix {cmd}.", features.Count, split[0]);
					foreach (var feature in features)
					{
						try
						{
							if (!await feature.HandleCommandAsync(param))
								Log.Error("Invalid syntax.");
						}
						catch (Exception ex)
						{
							Log.Error(ex, "Exception thrown from feature with command {name} during command handling.", feature.CommandPrefix);
						}
					}
				});
			}
			try
			{
				sender.Dispose();
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Exception during message handler disposition.");
			}
		}
	}
}