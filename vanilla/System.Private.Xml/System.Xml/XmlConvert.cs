using System.CodeDom.Compiler;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.RegularExpressions.Generated;
using System.Xml.Schema;

namespace System.Xml;

public class XmlConvert
{
	private static volatile string[] s_allDateTimeFormats;

	internal static readonly char[] WhitespaceChars = new char[4] { ' ', '\t', '\n', '\r' };

	private static string[] AllDateTimeFormats
	{
		get
		{
			if (s_allDateTimeFormats == null)
			{
				CreateAllDateTimeFormats();
			}
			return s_allDateTimeFormats;
		}
	}

	[return: NotNullIfNotNull("name")]
	public static string? EncodeName(string? name)
	{
		return EncodeName(name, first: true, local: false);
	}

	[return: NotNullIfNotNull("name")]
	public static string? EncodeNmToken(string? name)
	{
		return EncodeName(name, first: false, local: false);
	}

	[return: NotNullIfNotNull("name")]
	public static string? EncodeLocalName(string? name)
	{
		return EncodeName(name, first: true, local: true);
	}

	[return: NotNullIfNotNull("name")]
	public static string? DecodeName(string? name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return name;
		}
		int num = name.IndexOf('_');
		if (num < 0)
		{
			return name;
		}
		Regex.ValueMatchEnumerator valueMatchEnumerator = DecodeCharRegex().EnumerateMatches(name.AsSpan(num));
		int num2 = -1;
		if (valueMatchEnumerator.MoveNext())
		{
			num2 = num + valueMatchEnumerator.Current.Index;
		}
		StringBuilder stringBuilder = null;
		int length = name.Length;
		int num3 = 0;
		for (int i = 0; i < length - 7 + 1; i++)
		{
			if (i != num2)
			{
				continue;
			}
			if (valueMatchEnumerator.MoveNext())
			{
				num2 = num + valueMatchEnumerator.Current.Index;
			}
			if (stringBuilder == null)
			{
				stringBuilder = new StringBuilder(length + 20);
			}
			stringBuilder.Append(name, num3, i - num3);
			if (name[i + 6] != '_')
			{
				int num4 = FromHex(name[i + 2]) * 268435456 + FromHex(name[i + 3]) * 16777216 + FromHex(name[i + 4]) * 1048576 + FromHex(name[i + 5]) * 65536 + FromHex(name[i + 6]) * 4096 + FromHex(name[i + 7]) * 256 + FromHex(name[i + 8]) * 16 + FromHex(name[i + 9]);
				if (num4 >= 65536)
				{
					if (num4 <= 1114111)
					{
						num3 = i + 7 + 4;
						XmlCharType.SplitSurrogateChar(num4, out var lowChar, out var highChar);
						stringBuilder.Append(highChar);
						stringBuilder.Append(lowChar);
					}
				}
				else
				{
					num3 = i + 7 + 4;
					stringBuilder.Append((char)num4);
				}
				i += 10;
			}
			else
			{
				num3 = i + 7;
				stringBuilder.Append((char)(FromHex(name[i + 2]) * 4096 + FromHex(name[i + 3]) * 256 + FromHex(name[i + 4]) * 16 + FromHex(name[i + 5])));
				i += 6;
			}
		}
		if (num3 == 0)
		{
			return name;
		}
		if (num3 < length)
		{
			stringBuilder.Append(name, num3, length - num3);
		}
		return stringBuilder.ToString();
	}

	[return: NotNullIfNotNull("name")]
	private static string EncodeName(string name, bool first, bool local)
	{
		if (string.IsNullOrEmpty(name))
		{
			return name;
		}
		StringBuilder stringBuilder = null;
		int length = name.Length;
		int num = 0;
		int i = 0;
		int num2 = name.IndexOf('_');
		IEnumerator enumerator = null;
		if (num2 >= 0)
		{
			MatchCollection matchCollection = EncodeCharRegex().Matches(name, num2);
			enumerator = matchCollection.GetEnumerator();
		}
		int num3 = -1;
		if (enumerator != null && enumerator.MoveNext())
		{
			Match match = (Match)enumerator.Current;
			num3 = match.Index - 1;
		}
		if (first && ((!XmlCharType.IsStartNCNameCharXml4e(name[0]) && (local || name[0] != ':')) || num3 == 0))
		{
			if (stringBuilder == null)
			{
				stringBuilder = new StringBuilder(length + 20);
			}
			stringBuilder.Append("_x");
			if (length > 1 && XmlCharType.IsHighSurrogate(name[0]) && XmlCharType.IsLowSurrogate(name[1]))
			{
				int highChar = name[0];
				int lowChar = name[1];
				int value = XmlCharType.CombineSurrogateChar(lowChar, highChar);
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder3 = stringBuilder2;
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(0, 1, stringBuilder2);
				handler.AppendFormatted(value, "X8");
				stringBuilder3.Append(ref handler);
				i++;
				num = 2;
			}
			else
			{
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder4 = stringBuilder2;
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(0, 1, stringBuilder2);
				handler.AppendFormatted((int)name[0], "X4");
				stringBuilder4.Append(ref handler);
				num = 1;
			}
			stringBuilder.Append('_');
			i++;
			if (num3 == 0 && enumerator.MoveNext())
			{
				Match match2 = (Match)enumerator.Current;
				num3 = match2.Index - 1;
			}
		}
		for (; i < length; i++)
		{
			if ((local && !XmlCharType.IsNCNameCharXml4e(name[i])) || (!local && !XmlCharType.IsNameCharXml4e(name[i])) || num3 == i)
			{
				if (stringBuilder == null)
				{
					stringBuilder = new StringBuilder(length + 20);
				}
				if (num3 == i && enumerator.MoveNext())
				{
					Match match3 = (Match)enumerator.Current;
					num3 = match3.Index - 1;
				}
				stringBuilder.Append(name, num, i - num);
				stringBuilder.Append("_x");
				if (length > i + 1 && XmlCharType.IsHighSurrogate(name[i]) && XmlCharType.IsLowSurrogate(name[i + 1]))
				{
					int highChar2 = name[i];
					int lowChar2 = name[i + 1];
					int value2 = XmlCharType.CombineSurrogateChar(lowChar2, highChar2);
					StringBuilder stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder5 = stringBuilder2;
					StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(0, 1, stringBuilder2);
					handler.AppendFormatted(value2, "X8");
					stringBuilder5.Append(ref handler);
					num = i + 2;
					i++;
				}
				else
				{
					StringBuilder stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder6 = stringBuilder2;
					StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(0, 1, stringBuilder2);
					handler.AppendFormatted((int)name[i], "X4");
					stringBuilder6.Append(ref handler);
					num = i + 1;
				}
				stringBuilder.Append('_');
			}
		}
		if (num == 0)
		{
			return name;
		}
		if (num < length)
		{
			stringBuilder.Append(name, num, length - num);
		}
		return stringBuilder.ToString();
	}

	[GeneratedRegex("_[Xx][0-9a-fA-F]{4}(?:_|[0-9a-fA-F]{4}_)")]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "8.0.10.36612")]
	private static Regex DecodeCharRegex()
	{
		return _003CRegexGenerator_g_003EF74B1AE921BCEFE4BA601AA541C7A23B1CA9711EA81E8FE504B5B6446748E035A__DecodeCharRegex_0.Instance;
	}

	[GeneratedRegex("(?<=_)[Xx][0-9a-fA-F]{4}(?:_|[0-9a-fA-F]{4}_)")]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "8.0.10.36612")]
	private static Regex EncodeCharRegex()
	{
		return _003CRegexGenerator_g_003EF74B1AE921BCEFE4BA601AA541C7A23B1CA9711EA81E8FE504B5B6446748E035A__EncodeCharRegex_1.Instance;
	}

	private static int FromHex(char digit)
	{
		return System.HexConverter.FromChar(digit);
	}

	internal static byte[] FromBinHexString(ReadOnlySpan<char> s, bool allowOddCount)
	{
		return BinHexDecoder.Decode(s, allowOddCount);
	}

	internal static string ToBinHexString(byte[] inArray)
	{
		ArgumentNullException.ThrowIfNull(inArray, "inArray");
		return BinHexEncoder.Encode(inArray, 0, inArray.Length);
	}

	public static string VerifyName(string name)
	{
		ArgumentException.ThrowIfNullOrEmpty(name, "name");
		int num = ValidateNames.ParseNameNoNamespaces(name, 0);
		if (num != name.Length)
		{
			throw CreateInvalidNameCharException(name, num, ExceptionType.XmlException);
		}
		return name;
	}

	internal static Exception TryVerifyName(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return new XmlException(System.SR.Xml_EmptyName, string.Empty);
		}
		int num = ValidateNames.ParseNameNoNamespaces(name, 0);
		if (num != name.Length)
		{
			return new XmlException((num == 0) ? System.SR.Xml_BadStartNameChar : System.SR.Xml_BadNameChar, XmlException.BuildCharExceptionArgs(name, num));
		}
		return null;
	}

	internal static string VerifyQName(string name, ExceptionType exceptionType)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		int colonOffset;
		int num = ValidateNames.ParseQName(name, 0, out colonOffset);
		if (num != name.Length)
		{
			throw CreateException(System.SR.Xml_BadNameChar, XmlException.BuildCharExceptionArgs(name, num), exceptionType, 0, num + 1);
		}
		return name;
	}

	public static string VerifyNCName(string name)
	{
		return VerifyNCName(name, ExceptionType.XmlException);
	}

	internal static string VerifyNCName(string name, ExceptionType exceptionType)
	{
		ArgumentException.ThrowIfNullOrEmpty(name, "name");
		int num = ValidateNames.ParseNCName(name, 0);
		if (num != name.Length)
		{
			throw CreateInvalidNameCharException(name, num, exceptionType);
		}
		return name;
	}

	internal static Exception TryVerifyNCName(string name)
	{
		int num = ValidateNames.ParseNCName(name);
		if (num == 0 || num != name.Length)
		{
			return ValidateNames.GetInvalidNameException(name, 0, num);
		}
		return null;
	}

	[return: NotNullIfNotNull("token")]
	public static string? VerifyTOKEN(string? token)
	{
		if (string.IsNullOrEmpty(token))
		{
			return token;
		}
		if (token.StartsWith(' ') || token.EndsWith(' ') || token.AsSpan().ContainsAny("\t\n\r") || token.Contains("  "))
		{
			throw new XmlException(System.SR.Sch_NotTokenString, token);
		}
		return token;
	}

	internal static Exception TryVerifyTOKEN(string token)
	{
		if (string.IsNullOrEmpty(token))
		{
			return null;
		}
		if (token.StartsWith(' ') || token.EndsWith(' ') || token.AsSpan().ContainsAny("\t\n\r") || token.Contains("  "))
		{
			return new XmlException(System.SR.Sch_NotTokenString, token);
		}
		return null;
	}

	public static string VerifyNMTOKEN(string name)
	{
		return VerifyNMTOKEN(name, ExceptionType.XmlException);
	}

	internal static string VerifyNMTOKEN(string name, ExceptionType exceptionType)
	{
		ArgumentNullException.ThrowIfNull(name, "name");
		if (name.Length == 0)
		{
			throw CreateException(System.SR.Xml_InvalidNmToken, name, exceptionType);
		}
		int num = ValidateNames.ParseNmtokenNoNamespaces(name, 0);
		if (num != name.Length)
		{
			throw CreateException(System.SR.Xml_BadNameChar, XmlException.BuildCharExceptionArgs(name, num), exceptionType, 0, num + 1);
		}
		return name;
	}

	internal static Exception TryVerifyNMTOKEN(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return new XmlException(System.SR.Xml_EmptyName, string.Empty);
		}
		int num = ValidateNames.ParseNmtokenNoNamespaces(name, 0);
		if (num != name.Length)
		{
			return new XmlException(System.SR.Xml_BadNameChar, XmlException.BuildCharExceptionArgs(name, num));
		}
		return null;
	}

	internal static Exception TryVerifyNormalizedString(string str)
	{
		if (str.AsSpan().ContainsAny("\t\n\r"))
		{
			return new XmlSchemaException(System.SR.Sch_NotNormalizedString, str);
		}
		return null;
	}

	public static string VerifyXmlChars(string content)
	{
		ArgumentNullException.ThrowIfNull(content, "content");
		VerifyCharData(content, ExceptionType.XmlException);
		return content;
	}

	public static string VerifyPublicId(string publicId)
	{
		ArgumentNullException.ThrowIfNull(publicId, "publicId");
		int num = XmlCharType.IsPublicId(publicId);
		if (num >= 0)
		{
			throw CreateInvalidCharException(publicId, num, ExceptionType.XmlException);
		}
		return publicId;
	}

	public static string VerifyWhitespace(string content)
	{
		ArgumentNullException.ThrowIfNull(content, "content");
		int num = XmlCharType.IsOnlyWhitespaceWithPos(content);
		if (num != -1)
		{
			throw new XmlException(System.SR.Xml_InvalidWhitespaceCharacter, XmlException.BuildCharExceptionArgs(content, num), 0, num + 1);
		}
		return content;
	}

	public static bool IsStartNCNameChar(char ch)
	{
		return XmlCharType.IsStartNCNameSingleChar(ch);
	}

	public static bool IsNCNameChar(char ch)
	{
		return XmlCharType.IsNCNameSingleChar(ch);
	}

	public static bool IsXmlChar(char ch)
	{
		return XmlCharType.IsCharData(ch);
	}

	public static bool IsXmlSurrogatePair(char lowChar, char highChar)
	{
		if (XmlCharType.IsHighSurrogate(highChar))
		{
			return XmlCharType.IsLowSurrogate(lowChar);
		}
		return false;
	}

	public static bool IsPublicIdChar(char ch)
	{
		return XmlCharType.IsPubidChar(ch);
	}

	public static bool IsWhitespaceChar(char ch)
	{
		return XmlCharType.IsWhiteSpace(ch);
	}

	public static string ToString(bool value)
	{
		if (!value)
		{
			return "false";
		}
		return "true";
	}

	public static string ToString(char value)
	{
		return value.ToString();
	}

	public static string ToString(decimal value)
	{
		return value.ToString(null, NumberFormatInfo.InvariantInfo);
	}

	[CLSCompliant(false)]
	public static string ToString(sbyte value)
	{
		return value.ToString(null, CultureInfo.InvariantCulture);
	}

	public static string ToString(short value)
	{
		return value.ToString(null, CultureInfo.InvariantCulture);
	}

	public static string ToString(int value)
	{
		return value.ToString(null, CultureInfo.InvariantCulture);
	}

	public static string ToString(long value)
	{
		return value.ToString(null, CultureInfo.InvariantCulture);
	}

	public static string ToString(byte value)
	{
		return value.ToString(null, CultureInfo.InvariantCulture);
	}

	[CLSCompliant(false)]
	public static string ToString(ushort value)
	{
		return value.ToString(null, CultureInfo.InvariantCulture);
	}

	[CLSCompliant(false)]
	public static string ToString(uint value)
	{
		return value.ToString(null, CultureInfo.InvariantCulture);
	}

	[CLSCompliant(false)]
	public static string ToString(ulong value)
	{
		return value.ToString(null, CultureInfo.InvariantCulture);
	}

	public static string ToString(float value)
	{
		if (float.IsNegativeInfinity(value))
		{
			return "-INF";
		}
		if (float.IsPositiveInfinity(value))
		{
			return "INF";
		}
		if (IsNegativeZero(value))
		{
			return "-0";
		}
		return value.ToString("R", NumberFormatInfo.InvariantInfo);
	}

	public static string ToString(double value)
	{
		if (double.IsNegativeInfinity(value))
		{
			return "-INF";
		}
		if (double.IsPositiveInfinity(value))
		{
			return "INF";
		}
		if (IsNegativeZero(value))
		{
			return "-0";
		}
		return value.ToString("R", NumberFormatInfo.InvariantInfo);
	}

	public static string ToString(TimeSpan value)
	{
		return new XsdDuration(value).ToString();
	}

	[Obsolete("Use XmlConvert.ToString() that accepts an XmlDateTimeSerializationMode instead.")]
	public static string ToString(DateTime value)
	{
		return ToString(value, "yyyy-MM-ddTHH:mm:ss.fffffffzzzzzz");
	}

	public static string ToString(DateTime value, [StringSyntax("DateTimeFormat")] string format)
	{
		return value.ToString(format, DateTimeFormatInfo.InvariantInfo);
	}

	public static string ToString(DateTime value, XmlDateTimeSerializationMode dateTimeOption)
	{
		switch (dateTimeOption)
		{
		case XmlDateTimeSerializationMode.Local:
			value = SwitchToLocalTime(value);
			break;
		case XmlDateTimeSerializationMode.Utc:
			value = SwitchToUtcTime(value);
			break;
		case XmlDateTimeSerializationMode.Unspecified:
			value = new DateTime(value.Ticks, DateTimeKind.Unspecified);
			break;
		default:
			throw new ArgumentException(System.SR.Format(System.SR.Sch_InvalidDateTimeOption, dateTimeOption, "dateTimeOption"));
		case XmlDateTimeSerializationMode.RoundtripKind:
			break;
		}
		return new XsdDateTime(value, XsdDateTimeFlags.DateTime).ToString();
	}

	public static string ToString(DateTimeOffset value)
	{
		return new XsdDateTime(value).ToString();
	}

	public static string ToString(DateTimeOffset value, [StringSyntax("DateTimeFormat")] string format)
	{
		return value.ToString(format, DateTimeFormatInfo.InvariantInfo);
	}

	public static string ToString(Guid value)
	{
		return value.ToString();
	}

	public static bool ToBoolean(string s)
	{
		switch (s.AsSpan().Trim(WhitespaceChars))
		{
		case "1":
		case "true":
			return true;
		case "0":
		case "false":
			return false;
		default:
			throw new FormatException(System.SR.Format(System.SR.XmlConvert_BadFormat, s, "Boolean"));
		}
	}

	internal static Exception TryToBoolean(string s, out bool result)
	{
		switch (s.AsSpan().Trim(WhitespaceChars))
		{
		case "0":
		case "false":
			result = false;
			return null;
		case "1":
		case "true":
			result = true;
			return null;
		default:
			result = false;
			return new FormatException(System.SR.Format(System.SR.XmlConvert_BadFormat, s, "Boolean"));
		}
	}

	public static char ToChar(string s)
	{
		ArgumentNullException.ThrowIfNull(s, "s");
		if (s.Length != 1)
		{
			throw new FormatException(System.SR.XmlConvert_NotOneCharString);
		}
		return s[0];
	}

	internal static Exception TryToChar(string s, out char result)
	{
		if (!char.TryParse(s, out result))
		{
			return new FormatException(System.SR.Format(System.SR.XmlConvert_BadFormat, s, "Char"));
		}
		return null;
	}

	public static decimal ToDecimal(string s)
	{
		return decimal.Parse(s, NumberStyles.Integer | NumberStyles.AllowDecimalPoint, NumberFormatInfo.InvariantInfo);
	}

	internal static Exception TryToDecimal(string s, out decimal result)
	{
		if (!decimal.TryParse(s, NumberStyles.Integer | NumberStyles.AllowDecimalPoint, NumberFormatInfo.InvariantInfo, out result))
		{
			return new FormatException(System.SR.Format(System.SR.XmlConvert_BadFormat, s, "Decimal"));
		}
		return null;
	}

	internal static decimal ToInteger(string s)
	{
		return decimal.Parse(s, NumberStyles.Integer, NumberFormatInfo.InvariantInfo);
	}

	internal static Exception TryToInteger(string s, out decimal result)
	{
		if (!decimal.TryParse(s, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out result))
		{
			return new FormatException(System.SR.Format(System.SR.XmlConvert_BadFormat, s, "Integer"));
		}
		return null;
	}

	[CLSCompliant(false)]
	public static sbyte ToSByte(string s)
	{
		return sbyte.Parse(s, NumberStyles.Integer, NumberFormatInfo.InvariantInfo);
	}

	internal static Exception TryToSByte(string s, out sbyte result)
	{
		if (!sbyte.TryParse(s, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out result))
		{
			return new FormatException(System.SR.Format(System.SR.XmlConvert_BadFormat, s, "SByte"));
		}
		return null;
	}

	public static short ToInt16(string s)
	{
		return short.Parse(s, NumberStyles.Integer, NumberFormatInfo.InvariantInfo);
	}

	internal static Exception TryToInt16(string s, out short result)
	{
		if (!short.TryParse(s, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out result))
		{
			return new FormatException(System.SR.Format(System.SR.XmlConvert_BadFormat, s, "Int16"));
		}
		return null;
	}

	public static int ToInt32(string s)
	{
		return int.Parse(s, NumberStyles.Integer, NumberFormatInfo.InvariantInfo);
	}

	internal static Exception TryToInt32(string s, out int result)
	{
		if (!int.TryParse(s, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out result))
		{
			return new FormatException(System.SR.Format(System.SR.XmlConvert_BadFormat, s, "Int32"));
		}
		return null;
	}

	public static long ToInt64(string s)
	{
		return long.Parse(s, NumberStyles.Integer, NumberFormatInfo.InvariantInfo);
	}

	internal static Exception TryToInt64(string s, out long result)
	{
		if (!long.TryParse(s, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out result))
		{
			return new FormatException(System.SR.Format(System.SR.XmlConvert_BadFormat, s, "Int64"));
		}
		return null;
	}

	public static byte ToByte(string s)
	{
		return byte.Parse(s, NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite, NumberFormatInfo.InvariantInfo);
	}

	internal static Exception TryToByte(string s, out byte result)
	{
		if (!byte.TryParse(s, NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite, NumberFormatInfo.InvariantInfo, out result))
		{
			return new FormatException(System.SR.Format(System.SR.XmlConvert_BadFormat, s, "Byte"));
		}
		return null;
	}

	[CLSCompliant(false)]
	public static ushort ToUInt16(string s)
	{
		return ushort.Parse(s, NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite, NumberFormatInfo.InvariantInfo);
	}

	internal static Exception TryToUInt16(string s, out ushort result)
	{
		if (!ushort.TryParse(s, NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite, NumberFormatInfo.InvariantInfo, out result))
		{
			return new FormatException(System.SR.Format(System.SR.XmlConvert_BadFormat, s, "UInt16"));
		}
		return null;
	}

	[CLSCompliant(false)]
	public static uint ToUInt32(string s)
	{
		return uint.Parse(s, NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite, NumberFormatInfo.InvariantInfo);
	}

	internal static Exception TryToUInt32(string s, out uint result)
	{
		if (!uint.TryParse(s, NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite, NumberFormatInfo.InvariantInfo, out result))
		{
			return new FormatException(System.SR.Format(System.SR.XmlConvert_BadFormat, s, "UInt32"));
		}
		return null;
	}

	[CLSCompliant(false)]
	public static ulong ToUInt64(string s)
	{
		return ulong.Parse(s, NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite, NumberFormatInfo.InvariantInfo);
	}

	internal static Exception TryToUInt64(string s, out ulong result)
	{
		if (!ulong.TryParse(s, NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite, NumberFormatInfo.InvariantInfo, out result))
		{
			return new FormatException(System.SR.Format(System.SR.XmlConvert_BadFormat, s, "UInt64"));
		}
		return null;
	}

	public static float ToSingle(string s)
	{
		ArgumentNullException.ThrowIfNull(s, "s");
		ReadOnlySpan<char> readOnlySpan = s.AsSpan().Trim(WhitespaceChars);
		if (!readOnlySpan.SequenceEqual("-INF".AsSpan()))
		{
			if (readOnlySpan.SequenceEqual("INF".AsSpan()))
			{
				return float.PositiveInfinity;
			}
			float num = float.Parse(readOnlySpan, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, NumberFormatInfo.InvariantInfo);
			if (num == 0f && readOnlySpan[0] == '-')
			{
				return -0f;
			}
			return num;
		}
		return float.NegativeInfinity;
	}

	internal static Exception TryToSingle(string s, out float result)
	{
		ReadOnlySpan<char> readOnlySpan = s.AsSpan().Trim(WhitespaceChars);
		if (!readOnlySpan.SequenceEqual("-INF".AsSpan()))
		{
			if (readOnlySpan.SequenceEqual("INF".AsSpan()))
			{
				result = float.PositiveInfinity;
				return null;
			}
			if (!float.TryParse(readOnlySpan, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, NumberFormatInfo.InvariantInfo, out result))
			{
				return new FormatException(System.SR.Format(System.SR.XmlConvert_BadFormat, s, "Single"));
			}
			if (result == 0f && readOnlySpan[0] == '-')
			{
				result = -0f;
			}
			return null;
		}
		result = float.NegativeInfinity;
		return null;
	}

	public static double ToDouble(string s)
	{
		ArgumentNullException.ThrowIfNull(s, "s");
		ReadOnlySpan<char> readOnlySpan = s.AsSpan().Trim(WhitespaceChars);
		if (!readOnlySpan.SequenceEqual("-INF".AsSpan()))
		{
			if (readOnlySpan.SequenceEqual("INF".AsSpan()))
			{
				return double.PositiveInfinity;
			}
			double num = double.Parse(readOnlySpan, NumberStyles.Float, NumberFormatInfo.InvariantInfo);
			if (num == 0.0 && readOnlySpan[0] == '-')
			{
				return -0.0;
			}
			return num;
		}
		return double.NegativeInfinity;
	}

	internal static Exception TryToDouble(string s, out double result)
	{
		ReadOnlySpan<char> readOnlySpan = s.AsSpan().Trim(WhitespaceChars);
		if (!readOnlySpan.SequenceEqual("-INF".AsSpan()))
		{
			if (readOnlySpan.SequenceEqual("INF".AsSpan()))
			{
				result = double.PositiveInfinity;
				return null;
			}
			if (!double.TryParse(readOnlySpan, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, NumberFormatInfo.InvariantInfo, out result))
			{
				return new FormatException(System.SR.Format(System.SR.XmlConvert_BadFormat, s, "Double"));
			}
			if (result == 0.0 && readOnlySpan[0] == '-')
			{
				result = -0.0;
			}
			return null;
		}
		result = double.NegativeInfinity;
		return null;
	}

	internal static double ToXPathDouble(object o)
	{
		if (!(o is string text))
		{
			if (o is double)
			{
				return (double)o;
			}
			if (o is bool)
			{
				if (!(bool)o)
				{
					return 0.0;
				}
				return 1.0;
			}
			try
			{
				return Convert.ToDouble(o, NumberFormatInfo.InvariantInfo);
			}
			catch (FormatException)
			{
			}
			catch (OverflowException)
			{
			}
			catch (ArgumentNullException)
			{
			}
			return double.NaN;
		}
		ArgumentNullException.ThrowIfNull(text, "str");
		ReadOnlySpan<char> s = text.AsSpan().Trim(WhitespaceChars);
		if (s.Length != 0 && s[0] != '+' && double.TryParse(s, NumberStyles.AllowTrailingWhite | NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, NumberFormatInfo.InvariantInfo, out var result))
		{
			return result;
		}
		return double.NaN;
	}

	internal static string ToXPathString(object value)
	{
		if (!(value is string result))
		{
			if (!(value is double num))
			{
				if (value is bool)
				{
					if (!(bool)value)
					{
						return "false";
					}
					return "true";
				}
				return Convert.ToString(value, NumberFormatInfo.InvariantInfo);
			}
			return num.ToString("R", NumberFormatInfo.InvariantInfo);
		}
		return result;
	}

	internal static double XPathRound(double value)
	{
		double num = Math.Round(value);
		if (value - num != 0.5)
		{
			return num;
		}
		return num + 1.0;
	}

	public static TimeSpan ToTimeSpan(string s)
	{
		XsdDuration xsdDuration;
		try
		{
			xsdDuration = new XsdDuration(s);
		}
		catch (Exception)
		{
			throw new FormatException(System.SR.Format(System.SR.XmlConvert_BadFormat, s, "TimeSpan"));
		}
		return xsdDuration.ToTimeSpan();
	}

	internal static Exception TryToTimeSpan(string s, out TimeSpan result)
	{
		XsdDuration result2;
		Exception ex = XsdDuration.TryParse(s, out result2);
		if (ex != null)
		{
			result = TimeSpan.MinValue;
			return ex;
		}
		return result2.TryToTimeSpan(out result);
	}

	private static void CreateAllDateTimeFormats()
	{
		if (s_allDateTimeFormats == null)
		{
			s_allDateTimeFormats = new string[24]
			{
				"yyyy-MM-ddTHH:mm:ss.FFFFFFFzzzzzz", "yyyy-MM-ddTHH:mm:ss.FFFFFFF", "yyyy-MM-ddTHH:mm:ss.FFFFFFFZ", "HH:mm:ss.FFFFFFF", "HH:mm:ss.FFFFFFFZ", "HH:mm:ss.FFFFFFFzzzzzz", "yyyy-MM-dd", "yyyy-MM-ddZ", "yyyy-MM-ddzzzzzz", "yyyy-MM",
				"yyyy-MMZ", "yyyy-MMzzzzzz", "yyyy", "yyyyZ", "yyyyzzzzzz", "--MM-dd", "--MM-ddZ", "--MM-ddzzzzzz", "---dd", "---ddZ",
				"---ddzzzzzz", "--MM--", "--MM--Z", "--MM--zzzzzz"
			};
		}
	}

	[Obsolete("Use XmlConvert.ToDateTime() that accepts an XmlDateTimeSerializationMode instead.")]
	public static DateTime ToDateTime(string s)
	{
		return ToDateTime(s, AllDateTimeFormats);
	}

	public static DateTime ToDateTime(string s, [StringSyntax("DateTimeFormat")] string format)
	{
		return DateTime.ParseExact(s, format, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AllowLeadingWhite | DateTimeStyles.AllowTrailingWhite);
	}

	public static DateTime ToDateTime(string s, [StringSyntax("DateTimeFormat")] string[] formats)
	{
		return DateTime.ParseExact(s, formats, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AllowLeadingWhite | DateTimeStyles.AllowTrailingWhite);
	}

	public static DateTime ToDateTime(string s, XmlDateTimeSerializationMode dateTimeOption)
	{
		XsdDateTime xsdDateTime = new XsdDateTime(s, XsdDateTimeFlags.AllXsd);
		DateTime dateTime = xsdDateTime;
		switch (dateTimeOption)
		{
		case XmlDateTimeSerializationMode.Local:
			dateTime = SwitchToLocalTime(dateTime);
			break;
		case XmlDateTimeSerializationMode.Utc:
			dateTime = SwitchToUtcTime(dateTime);
			break;
		case XmlDateTimeSerializationMode.Unspecified:
			dateTime = new DateTime(dateTime.Ticks, DateTimeKind.Unspecified);
			break;
		default:
			throw new ArgumentException(System.SR.Format(System.SR.Sch_InvalidDateTimeOption, dateTimeOption, "dateTimeOption"));
		case XmlDateTimeSerializationMode.RoundtripKind:
			break;
		}
		return dateTime;
	}

	public static DateTimeOffset ToDateTimeOffset(string s)
	{
		ArgumentNullException.ThrowIfNull(s, "s");
		XsdDateTime xsdDateTime = new XsdDateTime(s, XsdDateTimeFlags.AllXsd);
		return xsdDateTime;
	}

	public static DateTimeOffset ToDateTimeOffset(string s, [StringSyntax("DateTimeFormat")] string format)
	{
		ArgumentNullException.ThrowIfNull(s, "s");
		return DateTimeOffset.ParseExact(s, format, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AllowLeadingWhite | DateTimeStyles.AllowTrailingWhite);
	}

	public static DateTimeOffset ToDateTimeOffset(string s, [StringSyntax("DateTimeFormat")] string[] formats)
	{
		ArgumentNullException.ThrowIfNull(s, "s");
		return DateTimeOffset.ParseExact(s, formats, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AllowLeadingWhite | DateTimeStyles.AllowTrailingWhite);
	}

	public static Guid ToGuid(string s)
	{
		return new Guid(s);
	}

	internal static Exception TryToGuid(string s, out Guid result)
	{
		Exception result2 = null;
		result = Guid.Empty;
		try
		{
			result = new Guid(s);
		}
		catch (ArgumentException)
		{
			result2 = new FormatException(System.SR.Format(System.SR.XmlConvert_BadFormat, s, "Guid"));
		}
		catch (FormatException)
		{
			result2 = new FormatException(System.SR.Format(System.SR.XmlConvert_BadFormat, s, "Guid"));
		}
		return result2;
	}

	private static DateTime SwitchToLocalTime(DateTime value)
	{
		return value.Kind switch
		{
			DateTimeKind.Local => value, 
			DateTimeKind.Unspecified => new DateTime(value.Ticks, DateTimeKind.Local), 
			DateTimeKind.Utc => value.ToLocalTime(), 
			_ => value, 
		};
	}

	private static DateTime SwitchToUtcTime(DateTime value)
	{
		return value.Kind switch
		{
			DateTimeKind.Utc => value, 
			DateTimeKind.Unspecified => new DateTime(value.Ticks, DateTimeKind.Utc), 
			DateTimeKind.Local => value.ToUniversalTime(), 
			_ => value, 
		};
	}

	internal static Uri ToUri(string s)
	{
		if (!string.IsNullOrEmpty(s))
		{
			s = TrimString(s);
			if (s.Length == 0 || s.Contains("##"))
			{
				throw new FormatException(System.SR.Format(System.SR.XmlConvert_BadFormat, s, "Uri"));
			}
		}
		if (!Uri.TryCreate(s, UriKind.RelativeOrAbsolute, out Uri result))
		{
			throw new FormatException(System.SR.Format(System.SR.XmlConvert_BadFormat, s, "Uri"));
		}
		return result;
	}

	internal static Exception TryToUri(string s, out Uri result)
	{
		result = null;
		if (s != null && s.Length > 0)
		{
			s = TrimString(s);
			if (s.Length == 0 || s.Contains("##"))
			{
				return new FormatException(System.SR.Format(System.SR.XmlConvert_BadFormat, s, "Uri"));
			}
		}
		if (!Uri.TryCreate(s, UriKind.RelativeOrAbsolute, out result))
		{
			return new FormatException(System.SR.Format(System.SR.XmlConvert_BadFormat, s, "Uri"));
		}
		return null;
	}

	internal static string TrimString(string value)
	{
		return value.Trim(WhitespaceChars);
	}

	internal static string TrimStringStart(string value)
	{
		return value.TrimStart(WhitespaceChars);
	}

	internal static string TrimStringEnd(string value)
	{
		return value.TrimEnd(WhitespaceChars);
	}

	internal static string[] SplitString(string value)
	{
		return value.Split(WhitespaceChars, StringSplitOptions.RemoveEmptyEntries);
	}

	internal static string[] SplitString(string value, StringSplitOptions splitStringOptions)
	{
		return value.Split(WhitespaceChars, splitStringOptions);
	}

	internal static bool IsNegativeZero(double value)
	{
		if (value == 0.0 && BitConverter.DoubleToInt64Bits(value) == BitConverter.DoubleToInt64Bits(-0.0))
		{
			return true;
		}
		return false;
	}

	internal static void VerifyCharData(string data, ExceptionType exceptionType)
	{
		VerifyCharData(data, exceptionType, exceptionType);
	}

	internal static void VerifyCharData(string data, ExceptionType invCharExceptionType, ExceptionType invSurrogateExceptionType)
	{
		if (string.IsNullOrEmpty(data))
		{
			return;
		}
		int num = 0;
		int length = data.Length;
		while (true)
		{
			if (num < length && XmlCharType.IsCharData(data[num]))
			{
				num++;
				continue;
			}
			if (num == length)
			{
				return;
			}
			char ch = data[num];
			if (!XmlCharType.IsHighSurrogate(ch))
			{
				break;
			}
			if (num + 1 == length)
			{
				throw CreateException(System.SR.Xml_InvalidSurrogateMissingLowChar, invSurrogateExceptionType, 0, num + 1);
			}
			ch = data[num + 1];
			if (XmlCharType.IsLowSurrogate(ch))
			{
				num += 2;
				continue;
			}
			throw CreateInvalidSurrogatePairException(data[num + 1], data[num], invSurrogateExceptionType, 0, num + 1);
		}
		throw CreateInvalidCharException(data, num, invCharExceptionType);
	}

	internal static void VerifyCharData(char[] data, int offset, int len, ExceptionType exceptionType)
	{
		if (data == null || len == 0)
		{
			return;
		}
		int num = offset;
		int num2 = offset + len;
		while (true)
		{
			if (num < num2 && XmlCharType.IsCharData(data[num]))
			{
				num++;
				continue;
			}
			if (num == num2)
			{
				return;
			}
			char ch = data[num];
			if (!XmlCharType.IsHighSurrogate(ch))
			{
				break;
			}
			if (num + 1 == num2)
			{
				throw CreateException(System.SR.Xml_InvalidSurrogateMissingLowChar, exceptionType, 0, offset - num + 1);
			}
			ch = data[num + 1];
			if (XmlCharType.IsLowSurrogate(ch))
			{
				num += 2;
				continue;
			}
			throw CreateInvalidSurrogatePairException(data[num + 1], data[num], exceptionType, 0, offset - num + 1);
		}
		throw CreateInvalidCharException(data, len, num, exceptionType);
	}

	internal static string EscapeValueForDebuggerDisplay(string value)
	{
		StringBuilder stringBuilder = null;
		int i = 0;
		int num = 0;
		for (; i < value.Length; i++)
		{
			char c = value[i];
			if (c < ' ' || c == '"')
			{
				if (stringBuilder == null)
				{
					stringBuilder = new StringBuilder(value.Length + 4);
				}
				if (i - num > 0)
				{
					stringBuilder.Append(value, num, i - num);
				}
				num = i + 1;
				switch (c)
				{
				case '"':
					stringBuilder.Append("\\\"");
					break;
				case '\r':
					stringBuilder.Append("\\r");
					break;
				case '\n':
					stringBuilder.Append("\\n");
					break;
				case '\t':
					stringBuilder.Append("\\t");
					break;
				default:
					stringBuilder.Append(c);
					break;
				}
			}
		}
		if (stringBuilder == null)
		{
			return value;
		}
		if (i - num > 0)
		{
			stringBuilder.Append(value, num, i - num);
		}
		return stringBuilder.ToString();
	}

	internal static Exception CreateException(string res, ExceptionType exceptionType, int lineNo, int linePos)
	{
		return exceptionType switch
		{
			ExceptionType.ArgumentException => new ArgumentException(res), 
			_ => new XmlException(res, string.Empty, lineNo, linePos), 
		};
	}

	internal static Exception CreateException(string res, string arg, ExceptionType exceptionType)
	{
		return CreateException(res, arg, exceptionType, 0, 0);
	}

	internal static Exception CreateException(string res, string arg, ExceptionType exceptionType, int lineNo, int linePos)
	{
		return exceptionType switch
		{
			ExceptionType.ArgumentException => new ArgumentException(string.Format(res, arg)), 
			_ => new XmlException(res, arg, lineNo, linePos), 
		};
	}

	internal static Exception CreateException(string res, string[] args, ExceptionType exceptionType)
	{
		return CreateException(res, args, exceptionType, 0, 0);
	}

	internal static Exception CreateException(string res, string[] args, ExceptionType exceptionType, int lineNo, int linePos)
	{
		switch (exceptionType)
		{
		case ExceptionType.ArgumentException:
			return new ArgumentException(string.Format(res, args));
		default:
			return new XmlException(res, args, lineNo, linePos);
		}
	}

	internal static Exception CreateInvalidSurrogatePairException(char low, char hi)
	{
		return CreateInvalidSurrogatePairException(low, hi, ExceptionType.ArgumentException);
	}

	internal static Exception CreateInvalidSurrogatePairException(char low, char hi, ExceptionType exceptionType)
	{
		return CreateInvalidSurrogatePairException(low, hi, exceptionType, 0, 0);
	}

	internal static Exception CreateInvalidSurrogatePairException(char low, char hi, ExceptionType exceptionType, int lineNo, int linePos)
	{
		string[] array = new string[2];
		uint num = hi;
		array[0] = num.ToString("X", CultureInfo.InvariantCulture);
		num = low;
		array[1] = num.ToString("X", CultureInfo.InvariantCulture);
		string[] args = array;
		return CreateException(System.SR.Xml_InvalidSurrogatePairWithArgs, args, exceptionType, lineNo, linePos);
	}

	internal static Exception CreateInvalidHighSurrogateCharException(char hi)
	{
		return CreateInvalidHighSurrogateCharException(hi, ExceptionType.ArgumentException);
	}

	internal static Exception CreateInvalidHighSurrogateCharException(char hi, ExceptionType exceptionType)
	{
		return CreateInvalidHighSurrogateCharException(hi, exceptionType, 0, 0);
	}

	internal static Exception CreateInvalidHighSurrogateCharException(char hi, ExceptionType exceptionType, int lineNo, int linePos)
	{
		string xml_InvalidSurrogateHighChar = System.SR.Xml_InvalidSurrogateHighChar;
		uint num = hi;
		return CreateException(xml_InvalidSurrogateHighChar, num.ToString("X", CultureInfo.InvariantCulture), exceptionType, lineNo, linePos);
	}

	internal static Exception CreateInvalidCharException(char[] data, int length, int invCharPos, ExceptionType exceptionType)
	{
		return CreateException(System.SR.Xml_InvalidCharacter, XmlException.BuildCharExceptionArgs(data, length, invCharPos), exceptionType, 0, invCharPos + 1);
	}

	internal static Exception CreateInvalidCharException(string data, int invCharPos)
	{
		return CreateInvalidCharException(data, invCharPos, ExceptionType.ArgumentException);
	}

	internal static Exception CreateInvalidCharException(string data, int invCharPos, ExceptionType exceptionType)
	{
		return CreateException(System.SR.Xml_InvalidCharacter, XmlException.BuildCharExceptionArgs(data, invCharPos), exceptionType, 0, invCharPos + 1);
	}

	internal static Exception CreateInvalidCharException(char invChar, char nextChar)
	{
		return CreateInvalidCharException(invChar, nextChar, ExceptionType.ArgumentException);
	}

	internal static Exception CreateInvalidCharException(char invChar, char nextChar, ExceptionType exceptionType)
	{
		return CreateException(System.SR.Xml_InvalidCharacter, XmlException.BuildCharExceptionArgs(invChar, nextChar), exceptionType);
	}

	internal static Exception CreateInvalidNameCharException(string name, int index, ExceptionType exceptionType)
	{
		return CreateException((index == 0) ? System.SR.Xml_BadStartNameChar : System.SR.Xml_BadNameChar, XmlException.BuildCharExceptionArgs(name, index), exceptionType, 0, index + 1);
	}

	internal static bool TryFormat(bool value, Span<char> destination, out int charsWritten)
	{
		string text = (value ? "true" : "false");
		charsWritten = text.Length;
		return text.TryCopyTo(destination);
	}

	internal static bool TryFormat(char value, Span<char> destination, out int charsWritten)
	{
		charsWritten = 1;
		if (destination.Length < 1)
		{
			return false;
		}
		destination[0] = value;
		return true;
	}

	internal static bool TryFormat(decimal value, Span<char> destination, out int charsWritten)
	{
		return value.TryFormat(destination, out charsWritten, default(ReadOnlySpan<char>), NumberFormatInfo.InvariantInfo);
	}

	internal static bool TryFormat(sbyte value, Span<char> destination, out int charsWritten)
	{
		return value.TryFormat(destination, out charsWritten, default(ReadOnlySpan<char>), CultureInfo.InvariantCulture);
	}

	internal static bool TryFormat(short value, Span<char> destination, out int charsWritten)
	{
		return value.TryFormat(destination, out charsWritten, default(ReadOnlySpan<char>), CultureInfo.InvariantCulture);
	}

	internal static bool TryFormat(int value, Span<char> destination, out int charsWritten)
	{
		return value.TryFormat(destination, out charsWritten, default(ReadOnlySpan<char>), CultureInfo.InvariantCulture);
	}

	internal static bool TryFormat(long value, Span<char> destination, out int charsWritten)
	{
		return value.TryFormat(destination, out charsWritten, default(ReadOnlySpan<char>), CultureInfo.InvariantCulture);
	}

	internal static bool TryFormat(byte value, Span<char> destination, out int charsWritten)
	{
		return value.TryFormat(destination, out charsWritten, default(ReadOnlySpan<char>), CultureInfo.InvariantCulture);
	}

	internal static bool TryFormat(ushort value, Span<char> destination, out int charsWritten)
	{
		return value.TryFormat(destination, out charsWritten, default(ReadOnlySpan<char>), CultureInfo.InvariantCulture);
	}

	internal static bool TryFormat(uint value, Span<char> destination, out int charsWritten)
	{
		return value.TryFormat(destination, out charsWritten, default(ReadOnlySpan<char>), CultureInfo.InvariantCulture);
	}

	internal static bool TryFormat(ulong value, Span<char> destination, out int charsWritten)
	{
		return value.TryFormat(destination, out charsWritten, default(ReadOnlySpan<char>), CultureInfo.InvariantCulture);
	}

	internal static bool TryFormat(float value, Span<char> destination, out int charsWritten)
	{
		ReadOnlySpan<char> readOnlySpan;
		if (!float.IsFinite(value))
		{
			readOnlySpan = ((!float.IsNaN(value)) ? ((ReadOnlySpan<char>)(float.IsNegative(value) ? "-INF" : "INF")) : ((ReadOnlySpan<char>)"NaN"));
		}
		else
		{
			if (!IsNegativeZero(value))
			{
				return value.TryFormat(destination, out charsWritten, "R", NumberFormatInfo.InvariantInfo);
			}
			readOnlySpan = "-0";
		}
		charsWritten = readOnlySpan.Length;
		return readOnlySpan.TryCopyTo(destination);
	}

	internal static bool TryFormat(double value, Span<char> destination, out int charsWritten)
	{
		ReadOnlySpan<char> readOnlySpan;
		if (!double.IsFinite(value))
		{
			readOnlySpan = ((!double.IsNaN(value)) ? ((ReadOnlySpan<char>)(double.IsNegative(value) ? "-INF" : "INF")) : ((ReadOnlySpan<char>)"NaN"));
		}
		else
		{
			if (!IsNegativeZero(value))
			{
				return value.TryFormat(destination, out charsWritten, "R", NumberFormatInfo.InvariantInfo);
			}
			readOnlySpan = "-0";
		}
		charsWritten = readOnlySpan.Length;
		return readOnlySpan.TryCopyTo(destination);
	}

	internal static bool TryFormat(TimeSpan value, Span<char> destination, out int charsWritten)
	{
		return new XsdDuration(value).TryFormat(destination, out charsWritten);
	}

	internal static bool TryFormat(DateTime value, [StringSyntax("DateTimeFormat")] string format, Span<char> destination, out int charsWritten)
	{
		return value.TryFormat(destination, out charsWritten, format, DateTimeFormatInfo.InvariantInfo);
	}

	internal static bool TryFormat(DateTime value, Span<char> destination, out int charsWritten)
	{
		return TryFormat(value, XmlDateTimeSerializationMode.RoundtripKind, destination, out charsWritten);
	}

	internal static bool TryFormat(DateTime value, XmlDateTimeSerializationMode dateTimeOption, Span<char> destination, out int charsWritten)
	{
		switch (dateTimeOption)
		{
		case XmlDateTimeSerializationMode.Local:
			value = SwitchToLocalTime(value);
			break;
		case XmlDateTimeSerializationMode.Utc:
			value = SwitchToUtcTime(value);
			break;
		case XmlDateTimeSerializationMode.Unspecified:
			value = new DateTime(value.Ticks, DateTimeKind.Unspecified);
			break;
		default:
			throw new ArgumentException(System.SR.Format(System.SR.Sch_InvalidDateTimeOption, dateTimeOption, "dateTimeOption"));
		case XmlDateTimeSerializationMode.RoundtripKind:
			break;
		}
		return new XsdDateTime(value, XsdDateTimeFlags.DateTime).TryFormat(destination, out charsWritten);
	}

	internal static bool TryFormat(DateTimeOffset value, Span<char> destination, out int charsWritten)
	{
		return new XsdDateTime(value).TryFormat(destination, out charsWritten);
	}

	internal static bool TryFormat(DateTimeOffset value, [StringSyntax("DateTimeFormat")] string format, Span<char> destination, out int charsWritten)
	{
		return value.TryFormat(destination, out charsWritten, format, DateTimeFormatInfo.InvariantInfo);
	}

	internal static bool TryFormat(Guid value, Span<char> destination, out int charsWritten)
	{
		return value.TryFormat(destination, out charsWritten);
	}
}
