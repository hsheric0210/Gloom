using NSec.Cryptography;
using System.Security.Cryptography;

namespace Gloom.Client
{
	internal class MessageEncryptor
	{
		private readonly byte[] clientRandom;
		private readonly Key clientKey;
		private readonly int kdfIterations;
		private readonly int kdfMemorySize;
		private readonly int kdfParallelism;

		private AesGcm? aes;

		public MessageEncryptor()
		{
			clientRandom = RandomNumberGenerator.GetBytes(64);
			clientKey = Key.Create(KeyAgreementAlgorithm.X25519, new KeyCreationParameters { ExportPolicy = KeyExportPolicies.AllowPlaintextExport });

			var threads = Environment.ProcessorCount;
			kdfIterations = Math.Min(24, CryptoLimits.MaxKDFIterations);
			kdfMemorySize = Math.Min(RandomNumberGenerator.GetInt32(64, 256) * 1024, CryptoLimits.MaxKDFMemorySize); // 32 ~ 128MB
			kdfParallelism = Math.Clamp(RandomNumberGenerator.GetInt32(100) * threads / 100, CryptoLimits.MinKDFParallelism, CryptoLimits.MaxKDFParallelism);
		}

		public OpStructs.ClientHello MakeClientHello(string identifier) => new()
		{
			Name = identifier,
			ClientRandom = clientRandom,
			DHParameter = clientKey.Export(KeyBlobFormat.PkixPublicKey),
			KDFIterations = kdfIterations,
			KDFMemorySize = kdfMemorySize,
			KDFParallelism = kdfParallelism
		};

		public void ReceiveServerHello(OpStructs.ServerHello serverHello)
		{
			var serverKey = PublicKey.Import(KeyAgreementAlgorithm.X25519, serverHello.DHParameter, KeyBlobFormat.PkixPublicKey);
			var agreedKey = KeyAgreementAlgorithm.X25519.Agree(clientKey, serverKey, new SharedSecretCreationParameters { ExportPolicy = KeyExportPolicies.AllowPlaintextExport });
			if (agreedKey is null)
				throw new CryptographicException("Key agreement failed");

			var key = agreedKey.Export(SharedSecretBlobFormat.NSecSharedSecret);
			using var stream = new MemoryStream(64);
			stream.Write(key);
			stream.Write(clientRandom);
			stream.Write(serverHello.ServerRandom);

			var argon2 = new Konscious.Security.Cryptography.Argon2id(stream.GetBuffer())
			{
				Salt = clientRandom[0..16],
				Iterations = Math.Min(kdfIterations, CryptoLimits.MaxKDFIterations),
				MemorySize = Math.Min(kdfMemorySize, CryptoLimits.MaxKDFMemorySize),
				DegreeOfParallelism = Math.Clamp(kdfParallelism, CryptoLimits.MinKDFParallelism, CryptoLimits.MaxKDFParallelism)
			};
			aes = new AesGcm(argon2.GetBytes(32));
		}

		public byte[] Encrypt(byte[] plaintext) => aes.EncryptSimple(plaintext);

		public byte[] Decrypt(byte[] ciphertext) => aes.DecryptSimple(ciphertext);
	}
}
