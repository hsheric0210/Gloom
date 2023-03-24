namespace Gloom;
internal static class OpFrame
{
	public readonly static Guid HelloWorld = Guid.Parse("6694af3e-f1c6-4f05-a1ad-7fa6f12d2117");
	public readonly static Guid ClientHandshake = Guid.Parse("033f8e43-ddee-4536-936e-71f94c599033");
	public readonly static Guid ServerHandshake = Guid.Parse("77b710eb-6507-4034-9ac6-c17d50e46b26");
	public readonly static Guid Test = Guid.Parse("09a3e64a-f87a-48d6-9760-fd2d61bd31e5");

	public static byte[] CreateOp(this Guid opGuid, byte[] data)
	{
		var payload = new byte[data.Length + 24];
		var lengthBytes = BitConverter.GetBytes(data.LongLength);
		Buffer.BlockCopy(lengthBytes, 0, payload, 0, 8);
		opGuid.TryWriteBytes(payload.AsSpan()[8..]);
		Buffer.BlockCopy(data, 0, payload, 24, data.Length);
		return payload;
	}

	public static long GetContentLength(byte[] payload) => BitConverter.ToInt64(payload.AsSpan()[..8]);

	public static Guid GetGuid(byte[] payload) => new(payload[8..24]);

	public static byte[] GetData(byte[] payload) => payload[24..((int)GetContentLength(payload) + 24)];
}
