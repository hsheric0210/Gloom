using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gloom.Server.Features
{
	public class Shell : FeatureBase
	{
		public override Guid[] AcceptedOps => new Guid[] { OpCodes.ShellOutResponse, OpCodes.ShellErrResponse, OpCodes.ShellExitResponse };

		public Shell(IMessageSender sender, string commandPrefix) : base(sender, commandPrefix)
		{
		}

		public override async Task HandleAsync(Client client, Guid op, byte[] data)
		{

		}

		public override Task<bool> HandleCommandAsync(string[] args)
		{

		}
	}
}
