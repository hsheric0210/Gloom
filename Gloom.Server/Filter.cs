using System.Text.RegularExpressions;

namespace Gloom.Server;
public class Filter
{
	private readonly FilterType type;
	private readonly string expression;
	private Regex? regex;

	public Filter(FilterType type, string expression)
	{
		this.type = type;
		this.expression = expression;
	}

	public bool IsMatch(string str)
	{
		switch (type)
		{
			case FilterType.Equals:
				return string.Equals(str, expression, StringComparison.OrdinalIgnoreCase);
			case FilterType.Contains:
				return str.Contains(expression, StringComparison.OrdinalIgnoreCase);
			case FilterType.StartsWith:
				return str.StartsWith(expression, StringComparison.OrdinalIgnoreCase);
			case FilterType.EndsWith:
				return str.EndsWith(expression, StringComparison.OrdinalIgnoreCase);
			case FilterType.Regex:
				return (regex ??= new Regex(expression, RegexOptions.Compiled)).IsMatch(str); // Cache compiled regex
		}
		return true;
	}

	public static Filter Parse(string expression)
	{
		if (expression.StartsWith('*'))
			return new Filter(FilterType.All, "");

		FilterType _type = FilterType.Equals;
		var offset = 0; // To detach type prefix
		if (expression.Length > 1)
		{
			offset = 1;
			switch (expression[0])
			{
				case '=':
					break;
				case '~':
					_type = FilterType.Contains;
					break;
				case '[':
					_type = FilterType.StartsWith;
					break;
				case ']':
					_type = FilterType.EndsWith;
					break;
				case '/':
					_type = FilterType.Regex;
					break;
				default: // Unrecognized prefix
					offset = 0;
					break;
			}
		}
		return new Filter(_type, expression[offset..]);
	}
}

public enum FilterType
{
	All,
	Equals,
	Contains,
	StartsWith,
	EndsWith,
	Regex
}
