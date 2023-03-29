using Gloom.WmiOps;

namespace Gloom.Client.Features.InfoCollector.Wmi;
internal class WmiProcessList : WmiInfo
{
	public WmiProcessList() : base(WmiOpCodes.ProcessList)
	{
	}

	public override object Collect()
	{
		var ret = new ProcessListResponse
		{
			List = Crawl<Win32Process>("Win32_Process")
		};
		return ret;
	}
}
