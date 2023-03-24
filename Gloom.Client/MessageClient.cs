using System.Buffers.Text;
using System.Net.WebSockets;
using System.Text;

namespace Gloom
{
	internal class MessageClient
	{
		private ClientWebSocket socket;
		private readonly CancellationTokenSource cancel;
		private MessageDecryptor? decryptor;

		public MessageClient(Uri address)
		{
			cancel = new CancellationTokenSource();
			socket = new ClientWebSocket();
			Task.Run(async () =>
			{
				while (!cancel.IsCancellationRequested)
				{
					Console.WriteLine("Trying to connect...");
					try
					{
						await socket.ConnectAsync(address, cancel.Token);

						// receive test message
						await Handshake();
						while (!cancel.IsCancellationRequested)
						{
							await ProcessMessages(socket);
							await Task.Delay(1000, cancel.Token);
						}
					}
					catch (Exception ex)
					{
						Console.WriteLine("Exception on main: " + ex);
					}
					socket = new ClientWebSocket(); // it automatically destroy itself when fails
					await Task.Delay(1000); // Try to re-connect every 10 sec
				}
			}).Wait();
		}

		public async Task ProcessMessages(ClientWebSocket socket)
		{
			if (socket.State == WebSocketState.Open)
			{
				try
				{
					var incoming = new byte[8192];
					await socket.ReceiveAsync(incoming, cancel.Token);
				}
				catch (Exception e)
				{
					Console.WriteLine("Error processing received message: " + e);
				}
			}
		}

		private async Task Handshake()
		{
			// create my message encryptor
			decryptor = new MessageDecryptor();

			// send my public key
			await socket.SendAsync(OpFrame.ClientHandshake.CreateOp(decryptor.ExportPublic()), WebSocketMessageType.Binary, true, cancel.Token);

			// receive encrypted shared secret
			var secretKeyBuffer = new byte[4096];
			await socket.ReceiveAsync(secretKeyBuffer, cancel.Token);
			decryptor.SetSecretKey(OpFrame.GetData(secretKeyBuffer));
		}

		public async void Finish()
		{
			cancel.Cancel();
			await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", cancel.Token);
			socket.Dispose();
		}
	}
}
