using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MS.Internal.Globalization;

internal static class BamlResourceContentUtil
{
	private static Regex UnescapePattern = new Regex("(\\\\.?|&lt;|&gt;|&quot;|&apos;|&amp;)", RegexOptions.Compiled | RegexOptions.CultureInvariant);

	private static MatchEvaluator UnescapeMatchEvaluator = UnescapeMatch;

	internal static string EscapeString(string content)
	{
		if (content == null)
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < content.Length; i++)
		{
			switch (content[i])
			{
			case '#':
			case ';':
			case '\\':
				stringBuilder.Append('\\');
				stringBuilder.Append(content[i]);
				break;
			case '&':
				stringBuilder.Append("&amp;");
				break;
			case '<':
				stringBuilder.Append("&lt;");
				break;
			case '>':
				stringBuilder.Append("&gt;");
				break;
			case '\'':
				stringBuilder.Append("&apos;");
				break;
			case '"':
				stringBuilder.Append("&quot;");
				break;
			default:
				stringBuilder.Append(content[i]);
				break;
			}
		}
		return stringBuilder.ToString();
	}

	internal static string UnescapeString(string content)
	{
		return UnescapePattern.Replace(content, UnescapeMatchEvaluator);
	}

	private static string UnescapeMatch(Match match)
	{
		switch (match.Value)
		{
		case "&lt;":
			return "<";
		case "&gt;":
			return ">";
		case "&amp;":
			return "&";
		case "&apos;":
			return "'";
		case "&quot;":
			return "\"";
		default:
			if (match.Value.Length == 2)
			{
				return match.Value[1].ToString();
			}
			return string.Empty;
		}
	}

	internal static BamlStringToken[] ParseChildPlaceholder(string input)
	{
		if (input == null)
		{
			return null;
		}
		List<BamlStringToken> list = new List<BamlStringToken>(8);
		int num = 0;
		bool flag = false;
		for (int i = 0; i < input.Length; i++)
		{
			if (input[i] == '#')
			{
				if (i == 0 || input[i - 1] != '\\')
				{
					if (flag)
					{
						return null;
					}
					flag = true;
					if (num < i)
					{
						list.Add(new BamlStringToken(BamlStringToken.TokenType.Text, UnescapeString(input.Substring(num, i - num))));
						num = i;
					}
				}
			}
			else if (input[i] == ';' && i > 0 && input[i - 1] != '\\' && flag)
			{
				list.Add(new BamlStringToken(BamlStringToken.TokenType.ChildPlaceHolder, UnescapeString(input.Substring(num + 1, i - num - 1))));
				num = i + 1;
				flag = false;
			}
		}
		if (flag)
		{
			return null;
		}
		if (num < input.Length)
		{
			list.Add(new BamlStringToken(BamlStringToken.TokenType.Text, UnescapeString(input.Substring(num))));
		}
		return list.ToArray();
	}
}
