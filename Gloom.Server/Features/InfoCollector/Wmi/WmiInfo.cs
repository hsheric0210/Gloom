namespace Gloom.Server.Features.InfoCollector.Wmi;
internal abstract class WmiInfo
{
	internal string Command { get; }
	internal Guid WmiOp { get; }

	protected WmiInfo(string command, Guid wmiOp)
	{
		Command = command;
		WmiOp = wmiOp;
	}

	public abstract Task Handle(string from, byte[] data);
}