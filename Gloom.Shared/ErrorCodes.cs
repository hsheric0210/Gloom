namespace Gloom
{
	public static class FileIOError
	{
		public const int InvalidPath = 0x1;
		public const int ChunkCombiningFailed = 0x2;
		public const int WritingFailed = 0x3;
		public const int ReadingFailed = 0x4;
		public const int DeletionFailed = 0x5;
	}
}
