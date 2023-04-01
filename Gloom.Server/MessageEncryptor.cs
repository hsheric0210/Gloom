using NSec.Cryptography;
using System.Security.Cryptography;

namespace Gloom.Server
{
	internal class MessageEncryptor
	{
		private readonly byte[] serverRandom;
		private readonly Key serverKey;

		private AesGcm? aes;

		public MessageEncryptor()
		{
			serverRandom = RandomNumberGenerator.GetBytes(64);
			serverKey = Key.Create(KeyAgreementAlgorithm.X25519, new KeyCreationParameters { ExportPolicy = KeyExportPolicies.AllowPlaintextExport });
		}

		public void ReceiveClientHello(OpStructs.ClientHello clientHello)
		{
			var clientKey = PublicKey.Import(KeyAgreementAlgorithm.X25519, clientHello.DHParameter, KeyBlobFormat.PkixPublicKey);
			var agreedKey = KeyAgreementAlgorithm.X25519.Agree(serverKey, clientKey, new SharedSecretCreationParameters { ExportPolicy = KeyExportPolicies.AllowPlaintextExport });
			if (agreedKey == null)
				throw new CryptographicException("Key agreement failed");

			var key = agreedKey.Export(SharedSecretBlobFormat.NSecSharedSecret);

			using var stream = new MemoryStream(64);
			stream.Write(key);
			stream.Write(clientHello.ClientRandom);
			stream.Write(serverRandom);
			var argon2 = new Konscious.Security.Cryptography.Argon2id(stream.GetBuffer())
			{
				Salt = clientHello.ClientRandom[0..16],
				Iterations = Math.Min(clientHello.KDFIterations, CryptoLimits.MaxKDFIterations),
				MemorySize = Math.Min(clientHello.KDFMemorySize, CryptoLimits.MaxKDFMemorySize),
				DegreeOfParallelism = Math.Clamp(clientHello.KDFParallelism, CryptoLimits.MinKDFParallelism, CryptoLimits.MaxKDFParallelism)
			};
			aes = new AesGcm(argon2.GetBytes(32));
		}

		public OpStructs.ServerHello MakeServerHello() => new OpStructs.ServerHello
		{
			ServerRandom = serverRandom,
			DHParameter = serverKey.Export(KeyBlobFormat.PkixPublicKey)
		};

		public byte[] Encrypt(byte[] plaintext) => aes.EncryptSimple(plaintext);

		public byte[] Decrypt(byte[] ciphertext) => aes.DecryptSimple(ciphertext);
	}
}
