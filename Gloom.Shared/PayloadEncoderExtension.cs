namespace Gloom;

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
