using System.Buffers;
using System.Net.WebSockets;

namespace Gloom.Client
{
	internal class MessageClient : IMessageSender
	{
		private const int MessageLoopDelay = 1000;
		private const int ReconnectDelay = 10000;
		private const int ReadBufferSize = 8192;

		private readonly ISet<IMessageHandler> handlerRegistry = new HashSet<IMessageHandler>();
		private readonly Uri address;
		private readonly CancellationTokenSource cancel;
		private ClientWebSocket socket;
		private MessageEncryptor? encryptor;

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
					Console.WriteLine("Connected 2 " + address);
					await SendClientHello();
					while (!cancel.IsCancellationRequested && socket.State == WebSocketState.Open)
					{
						await ProcessMessages(socket);
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

		public async Task SendAsync(Guid opCode, object data, bool eom)
		{
			if (encryptor == null)
				throw new InvalidOperationException("Client handshake not finished yet. (Message encryptor not available)");
			await socket.SendAsync(encryptor.Encrypt(opCode.NewPayload(data.Serialize())), WebSocketMessageType.Binary, eom, cancel.Token);
		}

		private async Task ProcessMessages(ClientWebSocket socket)
		{
			if (socket.State == WebSocketState.Open && encryptor is not null)
			{
				try
				{
					byte[] payload;
					using (var encryptedStream = new MemoryStream(ReadBufferSize))
					{
						// Read all data from socket & Dump it into one stream
						var buffer = ArrayPool<byte>.Shared.Rent(ReadBufferSize);
						try
						{
							//https://stackoverflow.com/questions/28512360/what-is-the-proper-way-to-use-websocketclient-receiveasync-and-buffer
							while (true)
							{
								var result = await socket.ReceiveAsync(buffer, cancel.Token);
								encryptedStream.Write(buffer, 0, result.Count);
								if (result.EndOfMessage)
									break;
							}
						}
						finally
						{
							ArrayPool<byte>.Shared.Return(buffer, true);
						}

						payload = encryptor.Decrypt(encryptedStream.ToArray());
					}

					var opcode = payload.GetGuid();
					var data = payload.GetData();
					try
					{
						await SimpleParallel.ForEachAsync(handlerRegistry.Where(handler => handler.AcceptedOps.Any(op => op == opcode)), async handler => await handler.HandleAsync(opcode, data));
					}
					catch (Exception e)
					{
#if DEBUG
						Console.WriteLine("Exception thrown from message handler: " + e);
#endif
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

		private async Task SendClientHello()
		{
			// create my message encryptor
			encryptor = new MessageEncryptor();

			// Send client Handshake
			var id = Environment.UserName + '@' + Environment.MachineName;
			await socket.SendAsync(OpCodes.ClientHello.NewPayload(encryptor.MakeClientHello(id).Serialize()), WebSocketMessageType.Binary, true, cancel.Token);

			// Receive server handshake
			var serverHs = new byte[8192]; // FIXME: Variable length buffer
			await socket.ReceiveAsync(serverHs, cancel.Token);
			encryptor.ReceiveServerHello(serverHs.GetData().Deserialize<OpStructs.ServerHello>());
		}

		public async Task Finish()
		{
			cancel.Cancel();
			await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", cancel.Token);
			socket.Dispose();
		}
	}
}
