using System.Text;
using System.Xml;
using System.Xml.Serialization;
namespace Gloom;

/// <summary>
/// Simple .NET struct encoder/decoder.
/// https://kdsoft-zeros.tistory.com/25
/// </summary>
public static class SerializerExtension
{
	public static byte[] Serialize(this object obj)
	{
		using var ms = new MemoryStream();
		using (var xw = XmlWriter.Create(ms, new XmlWriterSettings { Encoding = new UTF8Encoding(false), Indent = false, OmitXmlDeclaration = true }))
		{
			var serializer = new XmlSerializer(obj.GetType());
			serializer.Serialize(xw, obj);
		}
		return ms.ToArray();
	}

	public static T Deserialize<T>(this byte[] buffer)
	{
		using var ms = new MemoryStream(buffer);
		var serializer = new XmlSerializer(typeof(T));
		return (T)serializer.Deserialize(ms)!;
	}
}
