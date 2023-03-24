using System.Security.Cryptography;

namespace Gloom
{


	internal class MessageDecryptor
	{
		private readonly Aes aes = Aes.Create();
		private readonly RSA rsa;

		public MessageDecryptor() => rsa = RSA.Create(4096); // generate keypair

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
		}

		public byte[] Encrypt(byte[] plain) => aes.CreateEncryptor().TransformFinalBlock(plain, 0, plain.Length);

		public byte[] Decrypt(byte[] encrypted) => aes.CreateDecryptor().TransformFinalBlock(encrypted, 0, encrypted.Length);
	}
}
