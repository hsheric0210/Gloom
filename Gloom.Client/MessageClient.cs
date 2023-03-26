using System.Buffers;
using System.Net.WebSockets;
using System.Reflection;
using System.Security.Cryptography;
using System;

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
		private MessageEncryptor? decryptor;

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
			await socket.SendAsync(decryptor.Encrypt(opCode.NewPayload(StructConvert.Struct2Bytes(data))), WebSocketMessageType.Binary, eom, cancel.Token);
		}

		private async Task ProcessMessages(ClientWebSocket socket)
		{
			if (socket.State == WebSocketState.Open)
			{
				try
				{
					using var decryptedStream = new MemoryStream();
					using (var encryptedStream = new MemoryStream())
					{
						// Read all data from socket & Dump it into one stream
						var buffer = ArrayPool<byte>.Shared.Rent(ReadBufferSize);
						try
						{
							//https://stackoverflow.com/questions/28512360/what-is-the-proper-way-to-use-websocketclient-receiveasync-and-buffer
							while (true)
							{
								WebSocketReceiveResult result = await socket.ReceiveAsync(buffer, cancel.Token);
								encryptedStream.Write(buffer, 0, result.Count);
								if (result.EndOfMessage)
									break;
							}
						}
						finally
						{
							ArrayPool<byte>.Shared.Return(buffer, true);
						}

						// Perform decryption
						encryptedStream.Position = 0; // Reset cursor
						using var cryptoStream = new CryptoStream(encryptedStream, decryptor!.Decryptor, CryptoStreamMode.Read);
						await cryptoStream.CopyToAsync(decryptedStream, ReadBufferSize);
					}

					var payload = decryptedStream.GetBuffer();
					Guid opcode = payload.GetGuid();
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

		private async Task Handshake()
		{
			// create my message encryptor
			decryptor = new MessageEncryptor();

			// Send client Handshake
			var clientHandshake = new OpStructs.ClientHandshake
			{
				PcName = Environment.MachineName,
				UserName = Environment.UserName,
				PublicKey = decryptor.ExportPublic()
			};
			await socket.SendAsync(OpCodes.ClientHandshake.NewPayload(StructConvert.Struct2Bytes(clientHandshake)), WebSocketMessageType.Binary, true, cancel.Token);

			// Receive server handshake
			var serverHs = new byte[8192]; // FIXME: Variable length buffer
			await socket.ReceiveAsync(serverHs, cancel.Token);
			decryptor.SetSecretKey(StructConvert.Bytes2Struct<OpStructs.ServerHandshake>(serverHs.GetData()).EncryptedSecret);
		}

		public async Task Finish()
		{
			cancel.Cancel();
			await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", cancel.Token);
			socket.Dispose();
		}
	}
}
