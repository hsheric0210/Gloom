using System.Security.Cryptography;

namespace Gloom
{
	internal class MessageEncryptor
	{
		private readonly Aes aes;
		private readonly RSA rsa = RSA.Create(4096);

		public MessageEncryptor()
		{
			aes = Aes.Create();
			aes.Mode = CipherMode.CBC;
			aes.Padding = PaddingMode.ISO10126;
			aes.KeySize = 256; // AES-256
		}

		public byte[] ExportPublic() => rsa.ExportRSAPublicKey();

		public void SetPublicKey(byte[] publicKey) => rsa.ImportRSAPublicKey(publicKey, out _);

		public byte[] ExportEncryptedSecret()
		{
			var secret = new byte[446];
			aes.Key.CopyTo(secret, 0);
			aes.IV.CopyTo(secret, 32);
			return rsa.Encrypt(secret, RSAEncryptionPadding.Pkcs1);
		}

		public byte[] Encrypt(byte[] plain) => aes.CreateEncryptor().TransformFinalBlock(plain, 0, plain.Length);

		public byte[] Decrypt(byte[] encrypted) => aes.CreateDecryptor().TransformFinalBlock(encrypted, 0, encrypted.Length);
	}
}
