namespace Gloom
{
	public static class CryptoLimits
	{
		public const int MaxKDFIterations = 100;
		public const int MaxKDFMemorySize = 2097152; // 2GB
		public const int MaxKDFParallelism = 16;
		public const int MinKDFParallelism = 2;
	}
}
