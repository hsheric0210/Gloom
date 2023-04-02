using Gloom.WmiOps;
using Serilog;
using System.Text;

namespace Gloom.Server.Features.InfoCollector.Wmi;
internal class WmiHardwareInfo : WmiInfo
{
	public WmiHardwareInfo() : base("hw", WmiOpCodes.HardwareInfo)
	{
	}

	public override async Task Handle(Client client, byte[] data)
	{
		var rsp = data.Deserialize<HardwareInfoResponse>();
		var fileName = $"Hardware information of {client.Name} at {DateTime.Now:yyyy-MM-dd-HH-mm-ss-ffff}.md";
		try
		{
			using var fw = new StreamWriter(fileName, false, new UTF8Encoding(false), 8192);

			fw.WriteLine("# BIOS");
			fw.WriteLine(rsp.Bios.ToMarkdownTableSingleton());

			fw.WriteLine("# Mainboard");
			fw.WriteLine(rsp.BaseBoard.ToMarkdownTableSingleton());

			fw.WriteLine("# Keyboards");
			fw.WriteLine(rsp.Keyboards.ToMarkdownTable());

			fw.WriteLine("# Network adapters");
			fw.WriteLine(rsp.NetworkAdapters.ToMarkdownTable());

			fw.WriteLine("# Physical memories");
			fw.WriteLine(rsp.PhysicalMemories.ToMarkdownTable());

			fw.WriteLine("# Pointing devices");
			fw.WriteLine(rsp.PointingDevices.ToMarkdownTable());

			fw.WriteLine("# Video controllers");
			fw.WriteLine(rsp.VideoControllers.ToMarkdownTable());

			Log.Information("Hardware info of {client} written to {path}.", client, fileName);
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Exception during handling of wmi hadware info response.");
		}
	}
}
