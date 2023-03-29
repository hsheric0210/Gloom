using Fleck;
using Serilog;
using System.Collections.Immutable;

namespace Gloom.Server
{
	internal class MessageServer : IMessageSender
	{
		private readonly ISet<IMessageHandler> handlerRegistry = new HashSet<IMessageHandler>();
		private readonly IList<IWebSocketConnection> sockets = new List<IWebSocketConnection>();
		private readonly IDictionary<IWebSocketConnection, MessageEncryptor> encryptors = new Dictionary<IWebSocketConnection, MessageEncryptor>();
		private readonly WebSocketServer server;

		public async Task<int> SendAsync<T>(Filter filter, Guid opCode, T data, bool eom) where T : struct
		{
			var targets = sockets.Where(sock => filter.IsMatch(sock.ConnectionInfo.Host)).ToImmutableList();
			var tasks = new List<Task>(targets.Count);

			foreach (IWebSocketConnection socket in targets)
				tasks.Add(socket.Send(encryptors[socket].Encrypt(opCode.NewPayload(StructConvert.Struct2Bytes(data)))));

			await Task.WhenAll(tasks);
			return targets.Count;
		}

		public MessageServer(string address)
		{
			server = new WebSocketServer(address);
			server.ListenerSocket.NoDelay = true;
			server.Start(Configure);
		}

		public void Configure(IWebSocketConnection socket)
		{
			//FleckLog.Level = LogLevel.Debug;
			socket.OnOpen = () => OnOpen(socket);
			socket.OnBinary = data => OnMessage(socket, data);
			socket.OnClose = () => OnClose(socket);
			socket.OnError = ex => OnError(socket, ex);
		}

		public void OnOpen(IWebSocketConnection socket)
		{
			Console.WriteLine("A pc connected.");
			encryptors[socket] = new MessageEncryptor();
			sockets.Add(socket);
		}

		public void OnError(IWebSocketConnection socket, Exception ex)
		{
			Console.WriteLine("Socket error: " + ex);
			sockets.Remove(socket);
			encryptors.Remove(socket);
		}

		public void OnMessage(IWebSocketConnection socket, byte[] payload)
		{
			var host = socket.ConnectionInfo.Host;
			Guid guid = payload.GetGuid();
			if (guid == OpCodes.ClientHello)
			{
				OpStructs.ClientHello hs = StructConvert.Bytes2Struct<OpStructs.ClientHello>(payload.GetData());
				Log.Information("Client {client} ({id}) is trying to connect.", host, hs.Identifier);
				encryptors[socket].ReceiveClientHello(hs);
				socket.Send(OpCodes.ServerHello.NewPayload(StructConvert.Struct2Bytes(encryptors[socket].MakeServerHello())));
			}
			else
			{
				var decryptedPayload = encryptors[socket].Decrypt(payload);
				Guid opcode = decryptedPayload.GetGuid();
				var handlers = handlerRegistry.Where(handler => handler.AcceptedOps.Any(op => op == opcode)).ToList();
				var data = decryptedPayload.GetData();

				try
				{
					SimpleParallel.ForEach(handlers, async handler => await handler.HandleAsync(host, opcode, data)); // Run all associated handlers in parallel
				}
				catch (Exception ex)
				{
					Log.Error(ex, "Exception thrown during message processing.");
				}
			}
		}

		public void OnClose(IWebSocketConnection socket)
		{
			Log.Information("Client {client} disconnected.", socket.ConnectionInfo.Host);
			sockets.Remove(socket);
		}

		public void RegisterHandler(IMessageHandler handler) => handlerRegistry.Add(handler);

		public void Dispose()
		{
			foreach (IWebSocketConnection socket in sockets)
				socket.Close();
			server.Dispose();
		}
	}
}
