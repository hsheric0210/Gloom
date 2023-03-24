using Fleck;

namespace Gloom
{
	internal class MessageServer
	{
		private readonly List<IWebSocketConnection> sockets = new List<IWebSocketConnection>();
		private readonly WebSocketServer server;
		private readonly CancellationTokenSource cancel;
		private MessageEncryptor? encryptor;

		public MessageServer(string address)
		{
			cancel = new CancellationTokenSource();
			encryptor = new MessageEncryptor();

			server = new WebSocketServer(address);
			server.ListenerSocket.NoDelay = true;
			server.Start(Configure);
		}

		public void Configure(IWebSocketConnection socket)
		{
			FleckLog.Level = LogLevel.Debug;
			socket.OnOpen = () => OnOpen(socket);
			socket.OnBinary = data => OnMessage(socket, data);
			socket.OnClose = () => OnClose(socket);
			socket.OnError = ex => Console.WriteLine("Socket error: " + ex);
		}

		public void OnOpen(IWebSocketConnection socket)
		{
			Console.WriteLine("A pc connected.");
			sockets.Add(socket);
		}

		public void OnMessage(IWebSocketConnection socket, byte[] frame)
		{
			var guid = OpFrame.GetGuid(frame);
			Console.WriteLine("Received message. Guid is " + guid);
			if (guid == OpFrame.ClientHandshake)
			{
				encryptor.SetPublicKey(OpFrame.GetData(frame));
#if DEBUG
				Console.WriteLine("Key exchange successful.");
#endif
				var exported = encryptor.ExportEncryptedSecret();
				var encsecret = OpFrame.ServerHandshake.CreateOp(exported);
				socket.Send(encsecret);
			}
		}

		public void OnClose(IWebSocketConnection socket)
		{
			Console.WriteLine("A pc disconnected.");
			sockets.Remove(socket);
		}

		public async void Finish()
		{
			server.Dispose();
			cancel.Cancel();
		}
	}
}
