using System.Reflection;

namespace Gloom
{
	/// <summary>
	/// to-markdown-table
	/// Original author is Jeff (@jpierson)
	/// https://github.com/jpierson/to-markdown-table/blob/develop/src/ToMarkdownTable/LinqMarkdownTableExtensions.cs
	/// </summary>
	public static class LinqMarkdownTableExtensions
	{
		public static string ToMarkdownTableSingleton<T>(this T source) => ToMarkdownTable(new T[1] { source });

		public static string ToMarkdownTable<T>(this IEnumerable<T> source)
		{
			IEnumerable<PropertyInfo> properties = typeof(T).GetRuntimeProperties();
			IEnumerable<FieldInfo> fields = typeof(T)
				.GetRuntimeFields()
				.Where(f => f.IsPublic);

			var gettables = properties.Select(p => new { p.Name, GetValue = (Func<object, object?>)p.GetValue, Type = p.PropertyType }).Union(
				fields.Select(p => new { p.Name, GetValue = (Func<object, object?>)p.GetValue, Type = p.FieldType }));

			IEnumerable<string> columnNames = gettables.Select(p => p.Name);

			var headerLine = "| " + string.Join(" | ", columnNames) + " |";

			var isNumeric = new Func<Type, bool>(type =>
				type == typeof(byte) ||
				type == typeof(sbyte) ||
				type == typeof(ushort) ||
				type == typeof(uint) ||
				type == typeof(ulong) ||
				type == typeof(short) ||
				type == typeof(int) ||
				type == typeof(long) ||
				type == typeof(decimal) ||
				type == typeof(double) ||
				type == typeof(float));

			var rightAlign = new Func<Type, char>(type => isNumeric(type) ? ':' : ' ');

			var headerDataDividerLine =
				"| " +
				 string.Join(
					 "| ",
					 gettables.Select((g, _) => "---" + rightAlign(g.Type))) +
				"|";

			IEnumerable<string> lines = new[]
				{
					headerLine,
					headerDataDividerLine,
				}.Union(source
				.Select(s => "| " + string.Join(" | ", gettables.Select((n, _) => ToString(n.GetValue(s)) ?? "")) + " |"));

			return lines.Aggregate((p, c) => p + Environment.NewLine + c);
		}

		private static string? ToString(object? thing)
		{
			return thing is DateTime dt ? dt.ToString("yyyy-MM-dd HH:mm:ss.ffff") : (thing?.ToString());
		}
	}
}