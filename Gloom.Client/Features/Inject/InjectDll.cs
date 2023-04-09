using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gloom.Client.Features.Inject
{
	internal class InjectDll : FeatureBase
	{
		public override Guid[] AcceptedOps => throw new NotImplementedException();

		public InjectDll(IMessageSender sender) : base(sender)
		{
		}

		public override Task HandleAsync(Guid op, byte[] data) => throw new NotImplementedException();
	}
}
