using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
namespace Gloom;

/// <summary>
/// Simple .NET struct encoder/decoder.
/// https://kdsoft-zeros.tistory.com/25
/// </summary>
public static class StructConvert
{
	public static byte[] Struct2Bytes<T>(T obj) => !typeof(T).IsValueType || IsVariableSize<T>() ? VariableStruct2Bytes(obj) : FixedStruct2Bytes(obj);

	public static T Bytes2Struct<T>(byte[] buffer) => !typeof(T).IsValueType || IsVariableSize<T>() ? Bytes2VariableStruct<T>(buffer) : Bytes2FixedStruct<T>(buffer);

	private static bool IsVariableSize<T>() => Attribute.IsDefined(typeof(T), typeof(SerializableAttribute));

	#region Fixed-size struct (de-)serializer - Fast but the size is fixed
	private static byte[] FixedStruct2Bytes(object obj)
	{
		var sz = Marshal.SizeOf(obj);
		var arr = new byte[sz];
		IntPtr ptr = Marshal.AllocHGlobal(sz);
		Marshal.StructureToPtr(obj, ptr, false);
		Marshal.Copy(ptr, arr, 0, sz);
		Marshal.FreeHGlobal(ptr);
		return arr;
	}

	private static T Bytes2FixedStruct<T>(byte[] buffer)
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
	private static byte[] VariableStruct2Bytes(object obj)
	{
		using var ms = new MemoryStream();
		new BinaryFormatter().Serialize(ms, obj);
		return ms.ToArray();
	}

	private static T Bytes2VariableStruct<T>(byte[] buffer)
	{
		using var ms = new MemoryStream(buffer);
		return (T)new BinaryFormatter().Deserialize(ms)!;
	}
#else
	private static byte[] VariableStruct2Bytes(object obj)
	{
		using var ms = new MemoryStream();
		using (var xw = XmlWriter.Create(ms, new XmlWriterSettings { Encoding = new UTF8Encoding(false), Indent = false, OmitXmlDeclaration = true }))
		{
			var serializer = new XmlSerializer(obj.GetType());
			serializer.Serialize(xw, obj);
		}
		return ms.ToArray();
	}

	private static T Bytes2VariableStruct<T>(byte[] buffer)
	{
		using var ms = new MemoryStream(buffer);
		var serializer = new XmlSerializer(typeof(T));
		return (T)serializer.Deserialize(ms)!;
	}
#endif
	#endregion
}
