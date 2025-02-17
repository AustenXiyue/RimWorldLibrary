using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace HarmonyLib;

public static class GeneralExtensions
{
	public static string Join<T>(this IEnumerable<T> enumeration, Func<T, string> converter = null, string delimiter = ", ")
	{
		if (converter == null)
		{
			converter = (T t) => t.ToString();
		}
		return enumeration.Aggregate("", (string prev, T curr) => prev + ((prev.Length > 0) ? delimiter : "") + converter(curr));
	}

	public static string Description(this Type[] parameters)
	{
		if (parameters == null)
		{
			return "NULL";
		}
		return "(" + parameters.Join((Type p) => p.FullDescription()) + ")";
	}

	public static string FullDescription(this Type type)
	{
		if ((object)type == null)
		{
			return "null";
		}
		string text = type.Namespace;
		if (!string.IsNullOrEmpty(text))
		{
			text += ".";
		}
		string text2 = text + type.Name;
		if (type.IsGenericType)
		{
			text2 += "<";
			Type[] genericArguments = type.GetGenericArguments();
			for (int i = 0; i < genericArguments.Length; i++)
			{
				if (!text2.EndsWith("<", StringComparison.Ordinal))
				{
					text2 += ", ";
				}
				text2 += genericArguments[i].FullDescription();
			}
			text2 += ">";
		}
		return text2;
	}

	public static string FullDescription(this MethodBase member)
	{
		if ((object)member == null)
		{
			return "null";
		}
		Type returnedType = AccessTools.GetReturnedType(member);
		StringBuilder stringBuilder = new StringBuilder();
		if (member.IsStatic)
		{
			stringBuilder.Append("static ");
		}
		if (member.IsAbstract)
		{
			stringBuilder.Append("abstract ");
		}
		if (member.IsVirtual)
		{
			stringBuilder.Append("virtual ");
		}
		stringBuilder.Append(returnedType.FullDescription() + " ");
		if ((object)member.DeclaringType != null)
		{
			stringBuilder.Append(member.DeclaringType.FullDescription() + "::");
		}
		string text = member.GetParameters().Join((ParameterInfo p) => p.ParameterType.FullDescription() + " " + p.Name);
		stringBuilder.Append(member.Name + "(" + text + ")");
		return stringBuilder.ToString();
	}

	public static Type[] Types(this ParameterInfo[] pinfo)
	{
		return pinfo.Select((ParameterInfo pi) => pi.ParameterType).ToArray();
	}

	public static T GetValueSafe<S, T>(this Dictionary<S, T> dictionary, S key)
	{
		if (dictionary.TryGetValue(key, out var value))
		{
			return value;
		}
		return default(T);
	}

	public static T GetTypedValue<T>(this Dictionary<string, object> dictionary, string key)
	{
		if (dictionary.TryGetValue(key, out var value) && value is T)
		{
			return (T)value;
		}
		return default(T);
	}

	public static string ToLiteral(this string input, string quoteChar = "\"")
	{
		StringBuilder stringBuilder = new StringBuilder(input.Length + 2);
		stringBuilder.Append(quoteChar);
		foreach (char c in input)
		{
			switch (c)
			{
			case '\'':
				stringBuilder.Append("\\'");
				continue;
			case '"':
				stringBuilder.Append("\\\"");
				continue;
			case '\\':
				stringBuilder.Append("\\\\");
				continue;
			case '\0':
				stringBuilder.Append("\\0");
				continue;
			case '\a':
				stringBuilder.Append("\\a");
				continue;
			case '\b':
				stringBuilder.Append("\\b");
				continue;
			case '\f':
				stringBuilder.Append("\\f");
				continue;
			case '\n':
				stringBuilder.Append("\\n");
				continue;
			case '\r':
				stringBuilder.Append("\\r");
				continue;
			case '\t':
				stringBuilder.Append("\\t");
				continue;
			case '\v':
				stringBuilder.Append("\\v");
				continue;
			}
			if (c >= ' ' && c <= '~')
			{
				stringBuilder.Append(c);
				continue;
			}
			stringBuilder.Append("\\u");
			int num = c;
			stringBuilder.Append(num.ToString("x4"));
		}
		stringBuilder.Append(quoteChar);
		return stringBuilder.ToString();
	}
}
