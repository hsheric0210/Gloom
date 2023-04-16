using System.IO.Pipes;

namespace Gloom.Client.Native
{
	public class NativeConnection
	{
		private string pipeName;
		private Thread pipeThread;

		public NativeConnection(string pipeName)
		{
			this.pipeName = pipeName;
			pipeThread = new Thread(PipeThreadProc);

		}

		private void PipeThreadProc(object tdata)
		{
			var stream = new NamedPipeServerStream(pipeName);
#if DEBUG
			Console.WriteLine("Waiting native client to connect the pipe.");
			stream.ReadMode = PipeTransmissionMode.Message;
#endif
			stream.WaitForConnection();
#if DEBUG
			Console.WriteLine("Native client connected the pipe.");
#endif
		}

		public async void SendAsync(byte[] data)
		{

		}
	}
}
