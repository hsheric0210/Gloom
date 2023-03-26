using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Threading.Channels;

namespace Gloom.Client
{
	internal class MessageEncryptor : IDisposable
	{
		private readonly Aes aes = Aes.Create();
		private readonly RSA rsa;
		private bool disposedValue;

		internal ICryptoTransform Decryptor { get; private set; }

		public MessageEncryptor()
		{
			rsa = RSA.Create(4096); // generate keypair
		}

		public byte[] ExportPublic() => rsa.ExportRSAPublicKey();

		public void SetSecretKey(byte[] encryptedSecret)
		{
			var secret = rsa.Decrypt(encryptedSecret, RSAEncryptionPadding.Pkcs1);
			if (secret.Length < 48)
			{
#if DEBUG
				Console.WriteLine("Secret too short - expected at least 48, received " + secret.Length);
#endif
				return;
			}
			aes.Mode = CipherMode.CBC;
			aes.Padding = PaddingMode.ISO10126;
			aes.KeySize = 256; // AES-256
			aes.Key = secret[0..32];
			aes.IV = secret[32..48];
			Decryptor = aes.CreateDecryptor();
		}

		public byte[] Encrypt(byte[] plain) => aes.CreateEncryptor().TransformFinalBlock(plain, 0, plain.Length);

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
					Decryptor?.Dispose();
				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
