using System.Management;

namespace Gloom.Client.Features.InfoCollector.Wmi;
internal abstract class WmiInfo
{
	internal Guid WmiOp { get; }
	protected WmiInfo(Guid wmiOp) => WmiOp = wmiOp;

	public abstract object Collect();

#pragma warning disable CA1416 // Validate platform compatibility
	protected static T[] Crawl<T>(string wmiClass) where T : struct
	{
		if (Environment.OSVersion.Platform != PlatformID.Win32NT) // WMI is not supported on other OS's
			return Array.Empty<T>();

		var cls = new ManagementClass(wmiClass);
		var list = new List<T>();
		try
		{
			foreach (var obj in cls.GetInstances())
			{
				object instance = new T();
				foreach (var prop in instance.GetType().GetProperties())
				{
					try
					{
						var data = obj[prop.Name];
						if (prop.PropertyType == typeof(DateTime) && data is string sdata)
							data = ManagementDateTimeConverter.ToDateTime(sdata);
						prop.SetValue(instance, data);
					}
					catch (Exception ex)
					{
#if DEBUG
						Console.WriteLine($"Exception during WMI class '{wmiClass}' property '{prop.Name}' -> {ex}");
#endif
					}
				}
				list.Add((T)instance);
			}
		}
		catch (Exception ex)
		{
#if DEBUG
			Console.WriteLine($"Exception during cim instance '{wmiClass}' init: {ex}");
#endif
		}
		return list.ToArray();
	}
#pragma warning restore CA1416 // Validate platform compatibility
}
