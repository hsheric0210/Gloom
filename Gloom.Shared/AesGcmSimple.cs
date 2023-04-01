using System.Security.Cryptography;

namespace Gloom
{
	public static class AesGcmSimple
	{
		public static byte[] EncryptSimple(this AesGcm? aes, byte[] plaintext)
		{
			if (aes is null)
				ThrowCipherNotInitialized();
			var tagLen = AesGcm.TagByteSizes.MaxSize;
			var nonce = RandomNumberGenerator.GetBytes(AesGcm.NonceByteSizes.MaxSize);
			var buffer = new byte[nonce.Length + tagLen + plaintext.Length]; // <NONCE><TAG><CIPHERTEXT>
			Buffer.BlockCopy(nonce, 0, buffer, 0, nonce.Length);
			aes!.Encrypt(nonce, plaintext, buffer.AsSpan(nonce.Length + tagLen, plaintext.Length), buffer.AsSpan(nonce.Length, tagLen));
			return buffer;
		}

		public static byte[] DecryptSimple(this AesGcm? aes, byte[] ciphertext)
		{
			if (aes is null)
				ThrowCipherNotInitialized();
			var metaTagLength = AesGcm.NonceByteSizes.MaxSize + AesGcm.TagByteSizes.MaxSize;
			var buffer = new byte[ciphertext.Length - metaTagLength];
			// <NONCE><TAG><CIPHERTEXT>
			var nonce = ciphertext.AsSpan(0, AesGcm.NonceByteSizes.MaxSize);
			var cipherTexxt = ciphertext.AsSpan(metaTagLength, ciphertext.Length - metaTagLength);
			var tag = ciphertext.AsSpan(AesGcm.NonceByteSizes.MaxSize, AesGcm.TagByteSizes.MaxSize);
			aes!.Decrypt(nonce, cipherTexxt, tag, buffer);
			return buffer;
		}

		private static void ThrowCipherNotInitialized() => throw new InvalidOperationException("AES cipher not initialized yet");
	}
}
