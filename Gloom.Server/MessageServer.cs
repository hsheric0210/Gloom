using Fleck;
using Serilog;
using System.Collections.Immutable;

namespace Gloom.Server
{
	public sealed record Client(string Address, string Name)
	{
		internal IMessageSender msgProcessor = null!;

		public async Task<int> SendAsync<T>(Guid opCode, T data) => await msgProcessor.SendAsync(new Filter(FilterType.Equals, Address), opCode, data);

		public override string ToString() => $"{Name} ({Address})";
	}

	internal class MessageServer : IMessageSender
	{
		private readonly ISet<IMessageHandler> handlerRegistry = new HashSet<IMessageHandler>();
		private readonly IList<IWebSocketConnection> sockets = new List<IWebSocketConnection>();
		private readonly IDictionary<IWebSocketConnection, MessageEncryptor> encryptors = new Dictionary<IWebSocketConnection, MessageEncryptor>();
		private readonly IDictionary<string, string> clientNameMap = new Dictionary<string, string>();
		private readonly WebSocketServer server;

		public async Task<int> SendAsync<T>(Filter filter, Guid opCode, T data)
		{
			var targets = sockets.Where(sock => filter.IsMatch(sock.ConnectionInfo.Host)).ToImmutableList();
			var tasks = new List<Task>(targets.Count);

			foreach (var socket in targets)
				tasks.Add(socket.Send(encryptors[socket].Encrypt(opCode.NewPayload(data.Serialize()))));

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
			var guid = payload.GetGuid();
			if (guid == OpCodes.ClientHello)
			{
				var hs = payload.GetData().Deserialize<OpStructs.ClientHello>();
				Log.Information("Client {client} ({name}) is trying to connect.", host, hs.Name);
				encryptors[socket].ReceiveClientHello(hs);
				socket.Send(OpCodes.ServerHello.NewPayload(encryptors[socket].MakeServerHello().Serialize()));
				Log.Information("Client {client} ({name}) is connected.", host, hs.Name);
				var name = hs.Name;
				foreach (var invchr in Path.GetInvalidFileNameChars())
					name = name.Replace(invchr, '_');
				if (!string.Equals(hs.Name, name, StringComparison.OrdinalIgnoreCase))
					Log.Warning("Invalid characters in client name have been replaced: {prev} -> {new}", hs.Name, name);
				clientNameMap[host] = name;
			}
			else
			{
				var decryptedPayload = encryptors[socket].Decrypt(payload);
				var opcode = decryptedPayload.GetGuid();
				var handlers = handlerRegistry.Where(handler => handler.AcceptedOps.Any(op => op == opcode)).ToList();
				var data = decryptedPayload.GetData();

				try
				{
					SimpleParallel.ForEach(handlers, async handler => await handler.HandleAsync(new Client(host, clientNameMap[host]) { msgProcessor = this }, opcode, data)); // Run all associated handlers in parallel
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
			foreach (var socket in sockets)
				socket.Close();
			server.Dispose();
		}
	}
}
