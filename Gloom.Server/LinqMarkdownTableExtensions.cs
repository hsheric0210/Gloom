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
			var properties = typeof(T).GetRuntimeProperties();
			var fields = typeof(T)
				.GetRuntimeFields()
				.Where(f => f.IsPublic);

			var gettables = properties.Select(p => new { p.Name, GetValue = (Func<object, object?>)p.GetValue, Type = p.PropertyType }).Union(
				fields.Select(p => new { p.Name, GetValue = (Func<object, object?>)p.GetValue, Type = p.FieldType }));

			var maxColumnValues = source
				.Select(x => gettables.Select(p => ToString(p.GetValue(x))?.Length ?? 0))
				.Union(new[] { gettables.Select(p => p.Name.Length) }) // Include header in column sizes
				.Aggregate(
					new int[gettables.Count()].AsEnumerable(),
					(accumulate, x) => accumulate.Zip(x, Math.Max))
				.ToArray();

			var columnNames = gettables.Select(p => p.Name);

			var headerLine = "| " + string.Join(" | ", columnNames.Select((n, i) => n.PadRight(maxColumnValues[i]))) + " |";

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

			var headerDataDividerLine = "| " + string.Join("| ", gettables.Select((g, i) => new string('-', maxColumnValues[i]) + rightAlign(g.Type))) + "|";

			var lines = new[] { headerLine, headerDataDividerLine, }.Union(source.Select(s => "| " + string.Join(" | ", gettables.Select((n, i) => (ToString(n.GetValue(s)) ?? "").PadRight(maxColumnValues[i]))) + " |"));

			return lines.Aggregate((p, c) => p + Environment.NewLine + c);
		}

		private static string? ToString(object? thing) => thing is DateTime dt ? dt.ToString("yyyy-MM-dd HH:mm:ss.ffff") : (thing?.ToString());
	}
}