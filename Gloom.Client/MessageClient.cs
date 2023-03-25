using System.Net.WebSockets;
using static Gloom.HandshakeData;

namespace Gloom
{
	internal class MessageClient : IMessageSender
	{
		private const int MessageLoopDelay = 1000;
		private const int ReconnectDelay = 10000;

		private readonly ISet<IMessageHandler> handlerRegistry = new HashSet<IMessageHandler>();
		private readonly Uri address;
		private readonly CancellationTokenSource cancel;
		private ClientWebSocket socket;
		private MessageDecryptor? decryptor;

		public MessageClient(Uri address)
		{
			this.address = address;
			cancel = new CancellationTokenSource();
			socket = new ClientWebSocket();
		}

		public async Task Run()
		{
			while (!cancel.IsCancellationRequested)
			{
#if DEBUG
				Console.WriteLine("Trying to connect...");
#endif
				try
				{
					await socket.ConnectAsync(address, cancel.Token);
					await Handshake();
					while (!cancel.IsCancellationRequested && socket.State == WebSocketState.Open)
					{
						await ProcessMessages(socket);
						await Task.Delay(MessageLoopDelay, cancel.Token);
					}
				}
				catch (Exception ex) when (ex is not TaskCanceledException)
				{
#if DEBUG
					Console.WriteLine("Exception on main: " + ex);
#endif
				}
				socket = new ClientWebSocket(); // it automatically destroy itself when connection fails
				await Task.Delay(ReconnectDelay, cancel.Token); // Try to re-connect every 10 sec
			}
		}

		public void RegisterHandler(IMessageHandler handler) => handlerRegistry.Add(handler);

		public async Task SendAsync<T>(Guid opCode, T data, bool eom) where T : struct
		{
			if (decryptor == null)
				throw new InvalidOperationException("Client handshake not finished yet. (Message encryptor not available)");
			await socket.SendAsync(opCode.NewPayload(decryptor.Encrypt(StructConvert.Struct2Bytes(data))), WebSocketMessageType.Binary, eom, cancel.Token);
		}

		private async Task ProcessMessages(ClientWebSocket socket)
		{
			if (socket.State == WebSocketState.Open)
			{
				try
				{
					var incoming = new byte[8192];
					await socket.ReceiveAsync(incoming, cancel.Token);

					Guid opcode = incoming.GetGuid();
					var data = incoming.GetData();
					foreach (IMessageHandler handler in handlerRegistry.Where(handler => handler.AcceptedOps.Any(op => op == opcode)))
					{
						try
						{
							await handler.HandleAsync(opcode, data);
						}
						catch (Exception e)
						{
#if DEBUG
							Console.WriteLine("Exception thrown from message handler: " + e);
#endif
						}
					}
				}
				catch (Exception e)
				{
#if DEBUG
					Console.WriteLine("Error processing received message: " + e);
#endif
				}
			}
		}

		private async Task Handshake()
		{
			// create my message encryptor
			decryptor = new MessageDecryptor();

			var exportedPublic = decryptor.ExportPublic();
			var buffer = new byte[1024];
			Buffer.BlockCopy(exportedPublic, 0, buffer, 0, exportedPublic.Length);
			var clientHandshake = new ClientHandshake
			{
				PcName = Environment.MachineName,
				UserName = Environment.UserName,
				PublicKeyLength = exportedPublic.Length,
				PublicKeySpec = buffer
			};
			socket.SendAsync(OpCodes.ClientHandshake.NewPayload(StructConvert.Struct2Bytes(clientHandshake)), WebSocketMessageType.Binary, true, cancel.Token);

			// receive encrypted shared secret
			var secretKeyBuffer = new byte[4096];
			await socket.ReceiveAsync(secretKeyBuffer, cancel.Token);
			decryptor.SetSecretKey(secretKeyBuffer.GetData());

#if DEBUG
			Console.WriteLine("Handshake was successful.");
#endif
		}

		public async Task Finish()
		{
			cancel.Cancel();
			await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", cancel.Token);
			socket.Dispose();
		}
	}
}
