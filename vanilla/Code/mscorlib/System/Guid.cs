using System.Globalization;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;

namespace System;

/// <summary>Represents a globally unique identifier (GUID).</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[ComVisible(true)]
public struct Guid : IFormattable, IComparable, IComparable<Guid>, IEquatable<Guid>
{
	[Flags]
	private enum GuidStyles
	{
		None = 0,
		AllowParenthesis = 1,
		AllowBraces = 2,
		AllowDashes = 4,
		AllowHexPrefix = 8,
		RequireParenthesis = 0x10,
		RequireBraces = 0x20,
		RequireDashes = 0x40,
		RequireHexPrefix = 0x80,
		HexFormat = 0xA0,
		NumberFormat = 0,
		DigitFormat = 0x40,
		BraceFormat = 0x60,
		ParenthesisFormat = 0x50,
		Any = 0xF
	}

	private enum GuidParseThrowStyle
	{
		None,
		All,
		AllButOverflow
	}

	private enum ParseFailureKind
	{
		None,
		ArgumentNull,
		Format,
		FormatWithParameter,
		NativeException,
		FormatWithInnerException
	}

	private struct GuidResult
	{
		internal Guid parsedGuid;

		internal GuidParseThrowStyle throwStyle;

		internal ParseFailureKind m_failure;

		internal string m_failureMessageID;

		internal object m_failureMessageFormatArgument;

		internal string m_failureArgumentName;

		internal Exception m_innerException;

		internal void Init(GuidParseThrowStyle canThrow)
		{
			parsedGuid = Empty;
			throwStyle = canThrow;
		}

		internal void SetFailure(Exception nativeException)
		{
			m_failure = ParseFailureKind.NativeException;
			m_innerException = nativeException;
		}

		internal void SetFailure(ParseFailureKind failure, string failureMessageID)
		{
			SetFailure(failure, failureMessageID, null, null, null);
		}

		internal void SetFailure(ParseFailureKind failure, string failureMessageID, object failureMessageFormatArgument)
		{
			SetFailure(failure, failureMessageID, failureMessageFormatArgument, null, null);
		}

		internal void SetFailure(ParseFailureKind failure, string failureMessageID, object failureMessageFormatArgument, string failureArgumentName, Exception innerException)
		{
			m_failure = failure;
			m_failureMessageID = failureMessageID;
			m_failureMessageFormatArgument = failureMessageFormatArgument;
			m_failureArgumentName = failureArgumentName;
			m_innerException = innerException;
			if (throwStyle != 0)
			{
				throw GetGuidParseException();
			}
		}

		internal Exception GetGuidParseException()
		{
			return m_failure switch
			{
				ParseFailureKind.ArgumentNull => new ArgumentNullException(m_failureArgumentName, Environment.GetResourceString(m_failureMessageID)), 
				ParseFailureKind.FormatWithInnerException => new FormatException(Environment.GetResourceString(m_failureMessageID), m_innerException), 
				ParseFailureKind.FormatWithParameter => new FormatException(Environment.GetResourceString(m_failureMessageID, m_failureMessageFormatArgument)), 
				ParseFailureKind.Format => new FormatException(Environment.GetResourceString(m_failureMessageID)), 
				ParseFailureKind.NativeException => m_innerException, 
				_ => new FormatException(Environment.GetResourceString("Unrecognized Guid format.")), 
			};
		}
	}

	/// <summary>A read-only instance of the <see cref="T:System.Guid" /> structure whose value is all zeros.</summary>
	/// <filterpriority>1</filterpriority>
	public static readonly Guid Empty = default(Guid);

	private int _a;

	private short _b;

	private short _c;

	private byte _d;

	private byte _e;

	private byte _f;

	private byte _g;

	private byte _h;

	private byte _i;

	private byte _j;

	private byte _k;

	private static object _rngAccess = new object();

	private static RandomNumberGenerator _rng;

	private static RandomNumberGenerator _fastRng;

	/// <summary>Initializes a new instance of the <see cref="T:System.Guid" /> structure by using the specified array of bytes.</summary>
	/// <param name="b">A 16-element byte array containing values with which to initialize the GUID. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="b" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="b" /> is not 16 bytes long. </exception>
	public Guid(byte[] b)
	{
		if (b == null)
		{
			throw new ArgumentNullException("b");
		}
		if (b.Length != 16)
		{
			throw new ArgumentException(Environment.GetResourceString("Byte array for GUID must be exactly {0} bytes long.", "16"));
		}
		_a = (b[3] << 24) | (b[2] << 16) | (b[1] << 8) | b[0];
		_b = (short)((b[5] << 8) | b[4]);
		_c = (short)((b[7] << 8) | b[6]);
		_d = b[8];
		_e = b[9];
		_f = b[10];
		_g = b[11];
		_h = b[12];
		_i = b[13];
		_j = b[14];
		_k = b[15];
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Guid" /> structure by using the specified unsigned integers and bytes.</summary>
	/// <param name="a">The first 4 bytes of the GUID. </param>
	/// <param name="b">The next 2 bytes of the GUID. </param>
	/// <param name="c">The next 2 bytes of the GUID. </param>
	/// <param name="d">The next byte of the GUID. </param>
	/// <param name="e">The next byte of the GUID. </param>
	/// <param name="f">The next byte of the GUID. </param>
	/// <param name="g">The next byte of the GUID. </param>
	/// <param name="h">The next byte of the GUID. </param>
	/// <param name="i">The next byte of the GUID. </param>
	/// <param name="j">The next byte of the GUID. </param>
	/// <param name="k">The next byte of the GUID. </param>
	[CLSCompliant(false)]
	public Guid(uint a, ushort b, ushort c, byte d, byte e, byte f, byte g, byte h, byte i, byte j, byte k)
	{
		_a = (int)a;
		_b = (short)b;
		_c = (short)c;
		_d = d;
		_e = e;
		_f = f;
		_g = g;
		_h = h;
		_i = i;
		_j = j;
		_k = k;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Guid" /> structure by using the specified integers and byte array.</summary>
	/// <param name="a">The first 4 bytes of the GUID. </param>
	/// <param name="b">The next 2 bytes of the GUID. </param>
	/// <param name="c">The next 2 bytes of the GUID. </param>
	/// <param name="d">The remaining 8 bytes of the GUID. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="d" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="d" /> is not 8 bytes long. </exception>
	public Guid(int a, short b, short c, byte[] d)
	{
		if (d == null)
		{
			throw new ArgumentNullException("d");
		}
		if (d.Length != 8)
		{
			throw new ArgumentException(Environment.GetResourceString("Byte array for GUID must be exactly {0} bytes long.", "8"));
		}
		_a = a;
		_b = b;
		_c = c;
		_d = d[0];
		_e = d[1];
		_f = d[2];
		_g = d[3];
		_h = d[4];
		_i = d[5];
		_j = d[6];
		_k = d[7];
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Guid" /> structure by using the specified integers and bytes.</summary>
	/// <param name="a">The first 4 bytes of the GUID. </param>
	/// <param name="b">The next 2 bytes of the GUID. </param>
	/// <param name="c">The next 2 bytes of the GUID. </param>
	/// <param name="d">The next byte of the GUID. </param>
	/// <param name="e">The next byte of the GUID. </param>
	/// <param name="f">The next byte of the GUID. </param>
	/// <param name="g">The next byte of the GUID. </param>
	/// <param name="h">The next byte of the GUID. </param>
	/// <param name="i">The next byte of the GUID. </param>
	/// <param name="j">The next byte of the GUID. </param>
	/// <param name="k">The next byte of the GUID. </param>
	public Guid(int a, short b, short c, byte d, byte e, byte f, byte g, byte h, byte i, byte j, byte k)
	{
		_a = a;
		_b = b;
		_c = c;
		_d = d;
		_e = e;
		_f = f;
		_g = g;
		_h = h;
		_i = i;
		_j = j;
		_k = k;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Guid" /> structure by using the value represented by the specified string.</summary>
	/// <param name="g">A string that contains a GUID in one of the following formats ("d" represents a hexadecimal digit whose case is ignored): 32 contiguous digits: dddddddddddddddddddddddddddddddd -or- Groups of 8, 4, 4, 4, and 12 digits with hyphens between the groups. The entire GUID can optionally be enclosed in matching braces or parentheses: dddddddd-dddd-dddd-dddd-dddddddddddd -or- {dddddddd-dddd-dddd-dddd-dddddddddddd} -or- (dddddddd-dddd-dddd-dddd-dddddddddddd) -or- Groups of 8, 4, and 4 digits, and a subset of eight groups of 2 digits, with each group prefixed by "0x" or "0X", and separated by commas. The entire GUID, as well as the subset, is enclosed in matching braces: {0xdddddddd, 0xdddd, 0xdddd,{0xdd,0xdd,0xdd,0xdd,0xdd,0xdd,0xdd,0xdd}} All braces, commas, and "0x" prefixes are required. All embedded spaces are ignored. All leading zeros in a group are ignored.The digits shown in a group are the maximum number of meaningful digits that can appear in that group. You can specify from 1 to the number of digits shown for a group. The specified digits are assumed to be the low-order digits of the group. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="g" /> is null. </exception>
	/// <exception cref="T:System.FormatException">The format of <paramref name="g" /> is invalid. </exception>
	/// <exception cref="T:System.OverflowException">The format of <paramref name="g" /> is invalid. </exception>
	public Guid(string g)
	{
		if (g == null)
		{
			throw new ArgumentNullException("g");
		}
		this = Empty;
		GuidResult result = default(GuidResult);
		result.Init(GuidParseThrowStyle.All);
		if (TryParseGuid(g, GuidStyles.Any, ref result))
		{
			this = result.parsedGuid;
			return;
		}
		throw result.GetGuidParseException();
	}

	/// <summary>Converts the string representation of a GUID to the equivalent <see cref="T:System.Guid" /> structure.</summary>
	/// <returns>A structure that contains the value that was parsed.</returns>
	/// <param name="input">The GUID to convert.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="input" /> is null.</exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="input" /> is not in a recognized format.</exception>
	public static Guid Parse(string input)
	{
		if (input == null)
		{
			throw new ArgumentNullException("input");
		}
		GuidResult result = default(GuidResult);
		result.Init(GuidParseThrowStyle.AllButOverflow);
		if (TryParseGuid(input, GuidStyles.Any, ref result))
		{
			return result.parsedGuid;
		}
		throw result.GetGuidParseException();
	}

	/// <summary>Converts the string representation of a GUID to the equivalent <see cref="T:System.Guid" /> structure. </summary>
	/// <returns>true if the parse operation was successful; otherwise, false.</returns>
	/// <param name="input">The GUID to convert.</param>
	/// <param name="result">The structure that will contain the parsed value. If the method returns true, <paramref name="result" /> contains a valid <see cref="T:System.Guid" />. If the method returns false, <paramref name="result" /> equals <see cref="F:System.Guid.Empty" />. </param>
	public static bool TryParse(string input, out Guid result)
	{
		GuidResult result2 = default(GuidResult);
		result2.Init(GuidParseThrowStyle.None);
		if (TryParseGuid(input, GuidStyles.Any, ref result2))
		{
			result = result2.parsedGuid;
			return true;
		}
		result = Empty;
		return false;
	}

	/// <summary>Converts the string representation of a GUID to the equivalent <see cref="T:System.Guid" /> structure, provided that the string is in the specified format.</summary>
	/// <returns>A structure that contains the value that was parsed.</returns>
	/// <param name="input">The GUID to convert.</param>
	/// <param name="format">One of the following specifiers that indicates the exact format to use when interpreting <paramref name="input" />: "N", "D", "B", "P", or "X".</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="input" /> or <paramref name="format" /> is null.</exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="input" /> is not in a recognized format.</exception>
	public static Guid ParseExact(string input, string format)
	{
		if (input == null)
		{
			throw new ArgumentNullException("input");
		}
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		if (format.Length != 1)
		{
			throw new FormatException(Environment.GetResourceString("Format String can be only \"D\", \"d\", \"N\", \"n\", \"P\", \"p\", \"B\", \"b\", \"X\" or \"x\"."));
		}
		GuidStyles flags;
		switch (format[0])
		{
		case 'D':
		case 'd':
			flags = GuidStyles.RequireDashes;
			break;
		case 'N':
		case 'n':
			flags = GuidStyles.None;
			break;
		case 'B':
		case 'b':
			flags = GuidStyles.BraceFormat;
			break;
		case 'P':
		case 'p':
			flags = GuidStyles.ParenthesisFormat;
			break;
		case 'X':
		case 'x':
			flags = GuidStyles.HexFormat;
			break;
		default:
			throw new FormatException(Environment.GetResourceString("Format String can be only \"D\", \"d\", \"N\", \"n\", \"P\", \"p\", \"B\", \"b\", \"X\" or \"x\"."));
		}
		GuidResult result = default(GuidResult);
		result.Init(GuidParseThrowStyle.AllButOverflow);
		if (TryParseGuid(input, flags, ref result))
		{
			return result.parsedGuid;
		}
		throw result.GetGuidParseException();
	}

	/// <summary>Converts the string representation of a GUID to the equivalent <see cref="T:System.Guid" /> structure, provided that the string is in the specified format.</summary>
	/// <returns>true if the parse operation was successful; otherwise, false.</returns>
	/// <param name="input">The GUID to convert.</param>
	/// <param name="format">One of the following specifiers that indicates the exact format to use when interpreting <paramref name="input" />: "N", "D", "B", "P", or "X".</param>
	/// <param name="result">The structure that will contain the parsed value. If the method returns true, <paramref name="result" /> contains a valid <see cref="T:System.Guid" />. If the method returns false, <paramref name="result" /> equals <see cref="F:System.Guid.Empty" />.</param>
	public static bool TryParseExact(string input, string format, out Guid result)
	{
		if (format == null || format.Length != 1)
		{
			result = Empty;
			return false;
		}
		GuidStyles flags;
		switch (format[0])
		{
		case 'D':
		case 'd':
			flags = GuidStyles.RequireDashes;
			break;
		case 'N':
		case 'n':
			flags = GuidStyles.None;
			break;
		case 'B':
		case 'b':
			flags = GuidStyles.BraceFormat;
			break;
		case 'P':
		case 'p':
			flags = GuidStyles.ParenthesisFormat;
			break;
		case 'X':
		case 'x':
			flags = GuidStyles.HexFormat;
			break;
		default:
			result = Empty;
			return false;
		}
		GuidResult result2 = default(GuidResult);
		result2.Init(GuidParseThrowStyle.None);
		if (TryParseGuid(input, flags, ref result2))
		{
			result = result2.parsedGuid;
			return true;
		}
		result = Empty;
		return false;
	}

	private static bool TryParseGuid(string g, GuidStyles flags, ref GuidResult result)
	{
		if (g == null)
		{
			result.SetFailure(ParseFailureKind.Format, "Unrecognized Guid format.");
			return false;
		}
		string text = g.Trim();
		if (text.Length == 0)
		{
			result.SetFailure(ParseFailureKind.Format, "Unrecognized Guid format.");
			return false;
		}
		bool flag = text.IndexOf('-', 0) >= 0;
		if (flag)
		{
			if ((flags & (GuidStyles.AllowDashes | GuidStyles.RequireDashes)) == 0)
			{
				result.SetFailure(ParseFailureKind.Format, "Unrecognized Guid format.");
				return false;
			}
		}
		else if ((flags & GuidStyles.RequireDashes) != 0)
		{
			result.SetFailure(ParseFailureKind.Format, "Unrecognized Guid format.");
			return false;
		}
		bool flag2 = text.IndexOf('{', 0) >= 0;
		if (flag2)
		{
			if ((flags & (GuidStyles.AllowBraces | GuidStyles.RequireBraces)) == 0)
			{
				result.SetFailure(ParseFailureKind.Format, "Unrecognized Guid format.");
				return false;
			}
		}
		else if ((flags & GuidStyles.RequireBraces) != 0)
		{
			result.SetFailure(ParseFailureKind.Format, "Unrecognized Guid format.");
			return false;
		}
		if (text.IndexOf('(', 0) >= 0)
		{
			if ((flags & (GuidStyles.AllowParenthesis | GuidStyles.RequireParenthesis)) == 0)
			{
				result.SetFailure(ParseFailureKind.Format, "Unrecognized Guid format.");
				return false;
			}
		}
		else if ((flags & GuidStyles.RequireParenthesis) != 0)
		{
			result.SetFailure(ParseFailureKind.Format, "Unrecognized Guid format.");
			return false;
		}
		try
		{
			if (flag)
			{
				return TryParseGuidWithDashes(text, ref result);
			}
			if (flag2)
			{
				return TryParseGuidWithHexPrefix(text, ref result);
			}
			return TryParseGuidWithNoStyle(text, ref result);
		}
		catch (IndexOutOfRangeException innerException)
		{
			result.SetFailure(ParseFailureKind.FormatWithInnerException, "Unrecognized Guid format.", null, null, innerException);
			return false;
		}
		catch (ArgumentException innerException2)
		{
			result.SetFailure(ParseFailureKind.FormatWithInnerException, "Unrecognized Guid format.", null, null, innerException2);
			return false;
		}
	}

	private static bool TryParseGuidWithHexPrefix(string guidString, ref GuidResult result)
	{
		int num = 0;
		int num2 = 0;
		guidString = EatAllWhitespace(guidString);
		if (string.IsNullOrEmpty(guidString) || guidString[0] != '{')
		{
			result.SetFailure(ParseFailureKind.Format, "Expected {0xdddddddd, etc}.");
			return false;
		}
		if (!IsHexPrefix(guidString, 1))
		{
			result.SetFailure(ParseFailureKind.Format, "Expected hex 0x in '{0}'.", "{0xdddddddd, etc}");
			return false;
		}
		num = 3;
		num2 = guidString.IndexOf(',', num) - num;
		if (num2 <= 0)
		{
			result.SetFailure(ParseFailureKind.Format, "Could not find a comma, or the length between the previous token and the comma was zero (i.e., '0x,'etc.).");
			return false;
		}
		if (!StringToInt(guidString.Substring(num, num2), -1, 4096, out result.parsedGuid._a, ref result))
		{
			return false;
		}
		if (!IsHexPrefix(guidString, num + num2 + 1))
		{
			result.SetFailure(ParseFailureKind.Format, "Expected hex 0x in '{0}'.", "{0xdddddddd, 0xdddd, etc}");
			return false;
		}
		num = num + num2 + 3;
		num2 = guidString.IndexOf(',', num) - num;
		if (num2 <= 0)
		{
			result.SetFailure(ParseFailureKind.Format, "Could not find a comma, or the length between the previous token and the comma was zero (i.e., '0x,'etc.).");
			return false;
		}
		if (!StringToShort(guidString.Substring(num, num2), -1, 4096, out result.parsedGuid._b, ref result))
		{
			return false;
		}
		if (!IsHexPrefix(guidString, num + num2 + 1))
		{
			result.SetFailure(ParseFailureKind.Format, "Expected hex 0x in '{0}'.", "{0xdddddddd, 0xdddd, 0xdddd, etc}");
			return false;
		}
		num = num + num2 + 3;
		num2 = guidString.IndexOf(',', num) - num;
		if (num2 <= 0)
		{
			result.SetFailure(ParseFailureKind.Format, "Could not find a comma, or the length between the previous token and the comma was zero (i.e., '0x,'etc.).");
			return false;
		}
		if (!StringToShort(guidString.Substring(num, num2), -1, 4096, out result.parsedGuid._c, ref result))
		{
			return false;
		}
		if (guidString.Length <= num + num2 + 1 || guidString[num + num2 + 1] != '{')
		{
			result.SetFailure(ParseFailureKind.Format, "Expected {0xdddddddd, etc}.");
			return false;
		}
		num2++;
		byte[] array = new byte[8];
		for (int i = 0; i < 8; i++)
		{
			if (!IsHexPrefix(guidString, num + num2 + 1))
			{
				result.SetFailure(ParseFailureKind.Format, "Expected hex 0x in '{0}'.", "{... { ... 0xdd, ...}}");
				return false;
			}
			num = num + num2 + 3;
			if (i < 7)
			{
				num2 = guidString.IndexOf(',', num) - num;
				if (num2 <= 0)
				{
					result.SetFailure(ParseFailureKind.Format, "Could not find a comma, or the length between the previous token and the comma was zero (i.e., '0x,'etc.).");
					return false;
				}
			}
			else
			{
				num2 = guidString.IndexOf('}', num) - num;
				if (num2 <= 0)
				{
					result.SetFailure(ParseFailureKind.Format, "Could not find a brace, or the length between the previous token and the brace was zero (i.e., '0x,'etc.).");
					return false;
				}
			}
			uint num3 = (uint)Convert.ToInt32(guidString.Substring(num, num2), 16);
			if (num3 > 255)
			{
				result.SetFailure(ParseFailureKind.Format, "Value was either too large or too small for an unsigned byte.");
				return false;
			}
			array[i] = (byte)num3;
		}
		result.parsedGuid._d = array[0];
		result.parsedGuid._e = array[1];
		result.parsedGuid._f = array[2];
		result.parsedGuid._g = array[3];
		result.parsedGuid._h = array[4];
		result.parsedGuid._i = array[5];
		result.parsedGuid._j = array[6];
		result.parsedGuid._k = array[7];
		if (num + num2 + 1 >= guidString.Length || guidString[num + num2 + 1] != '}')
		{
			result.SetFailure(ParseFailureKind.Format, "Could not find the ending brace.");
			return false;
		}
		if (num + num2 + 1 != guidString.Length - 1)
		{
			result.SetFailure(ParseFailureKind.Format, "Additional non-parsable characters are at the end of the string.");
			return false;
		}
		return true;
	}

	private static bool TryParseGuidWithNoStyle(string guidString, ref GuidResult result)
	{
		int num = 0;
		int num2 = 0;
		if (guidString.Length != 32)
		{
			result.SetFailure(ParseFailureKind.Format, "Guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).");
			return false;
		}
		foreach (char c in guidString)
		{
			if (c < '0' || c > '9')
			{
				char c2 = char.ToUpper(c, CultureInfo.InvariantCulture);
				if (c2 < 'A' || c2 > 'F')
				{
					result.SetFailure(ParseFailureKind.Format, "Guid string should only contain hexadecimal characters.");
					return false;
				}
			}
		}
		if (!StringToInt(guidString.Substring(num, 8), -1, 4096, out result.parsedGuid._a, ref result))
		{
			return false;
		}
		num += 8;
		if (!StringToShort(guidString.Substring(num, 4), -1, 4096, out result.parsedGuid._b, ref result))
		{
			return false;
		}
		num += 4;
		if (!StringToShort(guidString.Substring(num, 4), -1, 4096, out result.parsedGuid._c, ref result))
		{
			return false;
		}
		num += 4;
		if (!StringToInt(guidString.Substring(num, 4), -1, 4096, out var result2, ref result))
		{
			return false;
		}
		num += 4;
		num2 = num;
		if (!StringToLong(guidString, ref num2, 8192, out var result3, ref result))
		{
			return false;
		}
		if (num2 - num != 12)
		{
			result.SetFailure(ParseFailureKind.Format, "Guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).");
			return false;
		}
		result.parsedGuid._d = (byte)(result2 >> 8);
		result.parsedGuid._e = (byte)result2;
		result2 = (int)(result3 >> 32);
		result.parsedGuid._f = (byte)(result2 >> 8);
		result.parsedGuid._g = (byte)result2;
		result2 = (int)result3;
		result.parsedGuid._h = (byte)(result2 >> 24);
		result.parsedGuid._i = (byte)(result2 >> 16);
		result.parsedGuid._j = (byte)(result2 >> 8);
		result.parsedGuid._k = (byte)result2;
		return true;
	}

	private static bool TryParseGuidWithDashes(string guidString, ref GuidResult result)
	{
		int num = 0;
		int num2 = 0;
		if (guidString[0] == '{')
		{
			if (guidString.Length != 38 || guidString[37] != '}')
			{
				result.SetFailure(ParseFailureKind.Format, "Guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).");
				return false;
			}
			num = 1;
		}
		else if (guidString[0] == '(')
		{
			if (guidString.Length != 38 || guidString[37] != ')')
			{
				result.SetFailure(ParseFailureKind.Format, "Guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).");
				return false;
			}
			num = 1;
		}
		else if (guidString.Length != 36)
		{
			result.SetFailure(ParseFailureKind.Format, "Guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).");
			return false;
		}
		if (guidString[8 + num] != '-' || guidString[13 + num] != '-' || guidString[18 + num] != '-' || guidString[23 + num] != '-')
		{
			result.SetFailure(ParseFailureKind.Format, "Dashes are in the wrong position for GUID parsing.");
			return false;
		}
		num2 = num;
		if (!StringToInt(guidString, ref num2, 8, 8192, out var result2, ref result))
		{
			return false;
		}
		result.parsedGuid._a = result2;
		num2++;
		if (!StringToInt(guidString, ref num2, 4, 8192, out result2, ref result))
		{
			return false;
		}
		result.parsedGuid._b = (short)result2;
		num2++;
		if (!StringToInt(guidString, ref num2, 4, 8192, out result2, ref result))
		{
			return false;
		}
		result.parsedGuid._c = (short)result2;
		num2++;
		if (!StringToInt(guidString, ref num2, 4, 8192, out result2, ref result))
		{
			return false;
		}
		num2++;
		num = num2;
		if (!StringToLong(guidString, ref num2, 8192, out var result3, ref result))
		{
			return false;
		}
		if (num2 - num != 12)
		{
			result.SetFailure(ParseFailureKind.Format, "Guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).");
			return false;
		}
		result.parsedGuid._d = (byte)(result2 >> 8);
		result.parsedGuid._e = (byte)result2;
		result2 = (int)(result3 >> 32);
		result.parsedGuid._f = (byte)(result2 >> 8);
		result.parsedGuid._g = (byte)result2;
		result2 = (int)result3;
		result.parsedGuid._h = (byte)(result2 >> 24);
		result.parsedGuid._i = (byte)(result2 >> 16);
		result.parsedGuid._j = (byte)(result2 >> 8);
		result.parsedGuid._k = (byte)result2;
		return true;
	}

	[SecuritySafeCritical]
	private unsafe static bool StringToShort(string str, int requiredLength, int flags, out short result, ref GuidResult parseResult)
	{
		return StringToShort(str, null, requiredLength, flags, out result, ref parseResult);
	}

	[SecuritySafeCritical]
	private unsafe static bool StringToShort(string str, ref int parsePos, int requiredLength, int flags, out short result, ref GuidResult parseResult)
	{
		fixed (int* parsePos2 = &parsePos)
		{
			return StringToShort(str, parsePos2, requiredLength, flags, out result, ref parseResult);
		}
	}

	[SecurityCritical]
	private unsafe static bool StringToShort(string str, int* parsePos, int requiredLength, int flags, out short result, ref GuidResult parseResult)
	{
		result = 0;
		int result2;
		bool result3 = StringToInt(str, parsePos, requiredLength, flags, out result2, ref parseResult);
		result = (short)result2;
		return result3;
	}

	[SecuritySafeCritical]
	private unsafe static bool StringToInt(string str, int requiredLength, int flags, out int result, ref GuidResult parseResult)
	{
		return StringToInt(str, null, requiredLength, flags, out result, ref parseResult);
	}

	[SecuritySafeCritical]
	private unsafe static bool StringToInt(string str, ref int parsePos, int requiredLength, int flags, out int result, ref GuidResult parseResult)
	{
		fixed (int* parsePos2 = &parsePos)
		{
			return StringToInt(str, parsePos2, requiredLength, flags, out result, ref parseResult);
		}
	}

	[SecurityCritical]
	private unsafe static bool StringToInt(string str, int* parsePos, int requiredLength, int flags, out int result, ref GuidResult parseResult)
	{
		result = 0;
		int num = ((parsePos != null) ? (*parsePos) : 0);
		try
		{
			result = ParseNumbers.StringToInt(str, 16, flags, parsePos);
		}
		catch (OverflowException ex)
		{
			if (parseResult.throwStyle == GuidParseThrowStyle.All)
			{
				throw;
			}
			if (parseResult.throwStyle == GuidParseThrowStyle.AllButOverflow)
			{
				throw new FormatException(Environment.GetResourceString("Unrecognized Guid format."), ex);
			}
			parseResult.SetFailure(ex);
			return false;
		}
		catch (Exception failure)
		{
			if (parseResult.throwStyle == GuidParseThrowStyle.None)
			{
				parseResult.SetFailure(failure);
				return false;
			}
			throw;
		}
		if (requiredLength != -1 && parsePos != null && *parsePos - num != requiredLength)
		{
			parseResult.SetFailure(ParseFailureKind.Format, "Guid string should only contain hexadecimal characters.");
			return false;
		}
		return true;
	}

	[SecuritySafeCritical]
	private unsafe static bool StringToLong(string str, int flags, out long result, ref GuidResult parseResult)
	{
		return StringToLong(str, null, flags, out result, ref parseResult);
	}

	[SecuritySafeCritical]
	private unsafe static bool StringToLong(string str, ref int parsePos, int flags, out long result, ref GuidResult parseResult)
	{
		fixed (int* parsePos2 = &parsePos)
		{
			return StringToLong(str, parsePos2, flags, out result, ref parseResult);
		}
	}

	[SecuritySafeCritical]
	private unsafe static bool StringToLong(string str, int* parsePos, int flags, out long result, ref GuidResult parseResult)
	{
		result = 0L;
		try
		{
			result = ParseNumbers.StringToLong(str, 16, flags, parsePos);
		}
		catch (OverflowException ex)
		{
			if (parseResult.throwStyle == GuidParseThrowStyle.All)
			{
				throw;
			}
			if (parseResult.throwStyle == GuidParseThrowStyle.AllButOverflow)
			{
				throw new FormatException(Environment.GetResourceString("Unrecognized Guid format."), ex);
			}
			parseResult.SetFailure(ex);
			return false;
		}
		catch (Exception failure)
		{
			if (parseResult.throwStyle == GuidParseThrowStyle.None)
			{
				parseResult.SetFailure(failure);
				return false;
			}
			throw;
		}
		return true;
	}

	private static string EatAllWhitespace(string str)
	{
		int length = 0;
		char[] array = new char[str.Length];
		foreach (char c in str)
		{
			if (!char.IsWhiteSpace(c))
			{
				array[length++] = c;
			}
		}
		return new string(array, 0, length);
	}

	private static bool IsHexPrefix(string str, int i)
	{
		if (str.Length > i + 1 && str[i] == '0' && char.ToLower(str[i + 1], CultureInfo.InvariantCulture) == 'x')
		{
			return true;
		}
		return false;
	}

	/// <summary>Returns a 16-element byte array that contains the value of this instance.</summary>
	/// <returns>A 16-element byte array.</returns>
	/// <filterpriority>2</filterpriority>
	public byte[] ToByteArray()
	{
		return new byte[16]
		{
			(byte)_a,
			(byte)(_a >> 8),
			(byte)(_a >> 16),
			(byte)(_a >> 24),
			(byte)_b,
			(byte)(_b >> 8),
			(byte)_c,
			(byte)(_c >> 8),
			_d,
			_e,
			_f,
			_g,
			_h,
			_i,
			_j,
			_k
		};
	}

	/// <summary>Returns a string representation of the value of this instance in registry format.</summary>
	/// <returns>The value of this <see cref="T:System.Guid" />, formatted by using the "D" format specifier as follows: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx where the value of the GUID is represented as a series of lowercase hexadecimal digits in groups of 8, 4, 4, 4, and 12 digits and separated by hyphens. An example of a return value is "382c74c3-721d-4f34-80e5-57657b6cbc27". To convert the hexadecimal digits from a through f to uppercase, call the <see cref="M:System.String.ToString" /> method on the returned string.</returns>
	/// <filterpriority>1</filterpriority>
	public override string ToString()
	{
		return ToString("D", null);
	}

	/// <summary>Returns the hash code for this instance.</summary>
	/// <returns>The hash code for this instance.</returns>
	/// <filterpriority>2</filterpriority>
	public override int GetHashCode()
	{
		return _a ^ ((_b << 16) | (ushort)_c) ^ ((_f << 24) | _k);
	}

	/// <summary>Returns a value that indicates whether this instance is equal to a specified object.</summary>
	/// <returns>true if <paramref name="o" /> is a <see cref="T:System.Guid" /> that has the same value as this instance; otherwise, false.</returns>
	/// <param name="o">The object to compare with this instance. </param>
	/// <filterpriority>2</filterpriority>
	public override bool Equals(object o)
	{
		if (o == null || !(o is Guid guid))
		{
			return false;
		}
		if (guid._a != _a)
		{
			return false;
		}
		if (guid._b != _b)
		{
			return false;
		}
		if (guid._c != _c)
		{
			return false;
		}
		if (guid._d != _d)
		{
			return false;
		}
		if (guid._e != _e)
		{
			return false;
		}
		if (guid._f != _f)
		{
			return false;
		}
		if (guid._g != _g)
		{
			return false;
		}
		if (guid._h != _h)
		{
			return false;
		}
		if (guid._i != _i)
		{
			return false;
		}
		if (guid._j != _j)
		{
			return false;
		}
		if (guid._k != _k)
		{
			return false;
		}
		return true;
	}

	/// <summary>Returns a value indicating whether this instance and a specified <see cref="T:System.Guid" /> object represent the same value.</summary>
	/// <returns>true if <paramref name="g" /> is equal to this instance; otherwise, false.</returns>
	/// <param name="g">An object to compare to this instance.</param>
	/// <filterpriority>2</filterpriority>
	public bool Equals(Guid g)
	{
		if (g._a != _a)
		{
			return false;
		}
		if (g._b != _b)
		{
			return false;
		}
		if (g._c != _c)
		{
			return false;
		}
		if (g._d != _d)
		{
			return false;
		}
		if (g._e != _e)
		{
			return false;
		}
		if (g._f != _f)
		{
			return false;
		}
		if (g._g != _g)
		{
			return false;
		}
		if (g._h != _h)
		{
			return false;
		}
		if (g._i != _i)
		{
			return false;
		}
		if (g._j != _j)
		{
			return false;
		}
		if (g._k != _k)
		{
			return false;
		}
		return true;
	}

	private int GetResult(uint me, uint them)
	{
		if (me < them)
		{
			return -1;
		}
		return 1;
	}

	/// <summary>Compares this instance to a specified object and returns an indication of their relative values.</summary>
	/// <returns>A signed number indicating the relative values of this instance and <paramref name="value" />.Return value Description A negative integer This instance is less than <paramref name="value" />. Zero This instance is equal to <paramref name="value" />. A positive integer This instance is greater than <paramref name="value" />, or <paramref name="value" /> is null. </returns>
	/// <param name="value">An object to compare, or null. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is not a <see cref="T:System.Guid" />. </exception>
	/// <filterpriority>2</filterpriority>
	public int CompareTo(object value)
	{
		if (value == null)
		{
			return 1;
		}
		if (!(value is Guid guid))
		{
			throw new ArgumentException(Environment.GetResourceString("Object must be of type GUID."));
		}
		if (guid._a != _a)
		{
			return GetResult((uint)_a, (uint)guid._a);
		}
		if (guid._b != _b)
		{
			return GetResult((uint)_b, (uint)guid._b);
		}
		if (guid._c != _c)
		{
			return GetResult((uint)_c, (uint)guid._c);
		}
		if (guid._d != _d)
		{
			return GetResult(_d, guid._d);
		}
		if (guid._e != _e)
		{
			return GetResult(_e, guid._e);
		}
		if (guid._f != _f)
		{
			return GetResult(_f, guid._f);
		}
		if (guid._g != _g)
		{
			return GetResult(_g, guid._g);
		}
		if (guid._h != _h)
		{
			return GetResult(_h, guid._h);
		}
		if (guid._i != _i)
		{
			return GetResult(_i, guid._i);
		}
		if (guid._j != _j)
		{
			return GetResult(_j, guid._j);
		}
		if (guid._k != _k)
		{
			return GetResult(_k, guid._k);
		}
		return 0;
	}

	/// <summary>Compares this instance to a specified <see cref="T:System.Guid" /> object and returns an indication of their relative values.</summary>
	/// <returns>A signed number indicating the relative values of this instance and <paramref name="value" />.Return value Description A negative integer This instance is less than <paramref name="value" />. Zero This instance is equal to <paramref name="value" />. A positive integer This instance is greater than <paramref name="value" />. </returns>
	/// <param name="value">An object to compare to this instance.</param>
	/// <filterpriority>2</filterpriority>
	public int CompareTo(Guid value)
	{
		if (value._a != _a)
		{
			return GetResult((uint)_a, (uint)value._a);
		}
		if (value._b != _b)
		{
			return GetResult((uint)_b, (uint)value._b);
		}
		if (value._c != _c)
		{
			return GetResult((uint)_c, (uint)value._c);
		}
		if (value._d != _d)
		{
			return GetResult(_d, value._d);
		}
		if (value._e != _e)
		{
			return GetResult(_e, value._e);
		}
		if (value._f != _f)
		{
			return GetResult(_f, value._f);
		}
		if (value._g != _g)
		{
			return GetResult(_g, value._g);
		}
		if (value._h != _h)
		{
			return GetResult(_h, value._h);
		}
		if (value._i != _i)
		{
			return GetResult(_i, value._i);
		}
		if (value._j != _j)
		{
			return GetResult(_j, value._j);
		}
		if (value._k != _k)
		{
			return GetResult(_k, value._k);
		}
		return 0;
	}

	/// <summary>Indicates whether the values of two specified <see cref="T:System.Guid" /> objects are equal.</summary>
	/// <returns>true if <paramref name="a" /> and <paramref name="b" /> are equal; otherwise, false.</returns>
	/// <param name="a">The first object to compare. </param>
	/// <param name="b">The second object to compare. </param>
	/// <filterpriority>3</filterpriority>
	public static bool operator ==(Guid a, Guid b)
	{
		if (a._a != b._a)
		{
			return false;
		}
		if (a._b != b._b)
		{
			return false;
		}
		if (a._c != b._c)
		{
			return false;
		}
		if (a._d != b._d)
		{
			return false;
		}
		if (a._e != b._e)
		{
			return false;
		}
		if (a._f != b._f)
		{
			return false;
		}
		if (a._g != b._g)
		{
			return false;
		}
		if (a._h != b._h)
		{
			return false;
		}
		if (a._i != b._i)
		{
			return false;
		}
		if (a._j != b._j)
		{
			return false;
		}
		if (a._k != b._k)
		{
			return false;
		}
		return true;
	}

	/// <summary>Indicates whether the values of two specified <see cref="T:System.Guid" /> objects are not equal.</summary>
	/// <returns>true if <paramref name="a" /> and <paramref name="b" /> are not equal; otherwise, false.</returns>
	/// <param name="a">The first object to compare. </param>
	/// <param name="b">The second object to compare. </param>
	/// <filterpriority>3</filterpriority>
	public static bool operator !=(Guid a, Guid b)
	{
		return !(a == b);
	}

	/// <summary>Returns a string representation of the value of this <see cref="T:System.Guid" /> instance, according to the provided format specifier.</summary>
	/// <returns>The value of this <see cref="T:System.Guid" />, represented as a series of lowercase hexadecimal digits in the specified format. </returns>
	/// <param name="format">A single format specifier that indicates how to format the value of this <see cref="T:System.Guid" />. The <paramref name="format" /> parameter can be "N", "D", "B", "P", or "X". If <paramref name="format" /> is null or an empty string (""), "D" is used. </param>
	/// <exception cref="T:System.FormatException">The value of <paramref name="format" /> is not null, an empty string (""), "N", "D", "B", "P", or "X". </exception>
	/// <filterpriority>1</filterpriority>
	public string ToString(string format)
	{
		return ToString(format, null);
	}

	private static char HexToChar(int a)
	{
		a &= 0xF;
		return (char)((a > 9) ? (a - 10 + 97) : (a + 48));
	}

	[SecurityCritical]
	private unsafe static int HexsToChars(char* guidChars, int offset, int a, int b)
	{
		return HexsToChars(guidChars, offset, a, b, hex: false);
	}

	[SecurityCritical]
	private unsafe static int HexsToChars(char* guidChars, int offset, int a, int b, bool hex)
	{
		if (hex)
		{
			guidChars[offset++] = '0';
			guidChars[offset++] = 'x';
		}
		guidChars[offset++] = HexToChar(a >> 4);
		guidChars[offset++] = HexToChar(a);
		if (hex)
		{
			guidChars[offset++] = ',';
			guidChars[offset++] = '0';
			guidChars[offset++] = 'x';
		}
		guidChars[offset++] = HexToChar(b >> 4);
		guidChars[offset++] = HexToChar(b);
		return offset;
	}

	/// <summary>Returns a string representation of the value of this instance of the <see cref="T:System.Guid" /> class, according to the provided format specifier and culture-specific format information.</summary>
	/// <returns>The value of this <see cref="T:System.Guid" />, represented as a series of lowercase hexadecimal digits in the specified format.</returns>
	/// <param name="format">A single format specifier that indicates how to format the value of this <see cref="T:System.Guid" />. The <paramref name="format" /> parameter can be "N", "D", "B", "P", or "X". If <paramref name="format" /> is null or an empty string (""), "D" is used. </param>
	/// <param name="provider">(Reserved) An object that supplies culture-specific formatting information. </param>
	/// <exception cref="T:System.FormatException">The value of <paramref name="format" /> is not null, an empty string (""), "N", "D", "B", "P", or "X". </exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public unsafe string ToString(string format, IFormatProvider provider)
	{
		if (format == null || format.Length == 0)
		{
			format = "D";
		}
		int offset = 0;
		bool flag = true;
		bool flag2 = false;
		if (format.Length != 1)
		{
			throw new FormatException(Environment.GetResourceString("Format String can be only \"D\", \"d\", \"N\", \"n\", \"P\", \"p\", \"B\", \"b\", \"X\" or \"x\"."));
		}
		string text;
		switch (format[0])
		{
		case 'D':
		case 'd':
			text = string.FastAllocateString(36);
			break;
		case 'N':
		case 'n':
			text = string.FastAllocateString(32);
			flag = false;
			break;
		case 'B':
		case 'b':
			text = string.FastAllocateString(38);
			fixed (char* ptr3 = text)
			{
				ptr3[offset++] = '{';
				ptr3[37] = '}';
			}
			break;
		case 'P':
		case 'p':
			text = string.FastAllocateString(38);
			fixed (char* ptr2 = text)
			{
				ptr2[offset++] = '(';
				ptr2[37] = ')';
			}
			break;
		case 'X':
		case 'x':
			text = string.FastAllocateString(68);
			fixed (char* ptr = text)
			{
				ptr[offset++] = '{';
				ptr[67] = '}';
			}
			flag = false;
			flag2 = true;
			break;
		default:
			throw new FormatException(Environment.GetResourceString("Format String can be only \"D\", \"d\", \"N\", \"n\", \"P\", \"p\", \"B\", \"b\", \"X\" or \"x\"."));
		}
		fixed (char* ptr4 = text)
		{
			if (flag2)
			{
				ptr4[offset++] = '0';
				ptr4[offset++] = 'x';
				offset = HexsToChars(ptr4, offset, _a >> 24, _a >> 16);
				offset = HexsToChars(ptr4, offset, _a >> 8, _a);
				ptr4[offset++] = ',';
				ptr4[offset++] = '0';
				ptr4[offset++] = 'x';
				offset = HexsToChars(ptr4, offset, _b >> 8, _b);
				ptr4[offset++] = ',';
				ptr4[offset++] = '0';
				ptr4[offset++] = 'x';
				offset = HexsToChars(ptr4, offset, _c >> 8, _c);
				ptr4[offset++] = ',';
				ptr4[offset++] = '{';
				offset = HexsToChars(ptr4, offset, _d, _e, hex: true);
				ptr4[offset++] = ',';
				offset = HexsToChars(ptr4, offset, _f, _g, hex: true);
				ptr4[offset++] = ',';
				offset = HexsToChars(ptr4, offset, _h, _i, hex: true);
				ptr4[offset++] = ',';
				offset = HexsToChars(ptr4, offset, _j, _k, hex: true);
				ptr4[offset++] = '}';
			}
			else
			{
				offset = HexsToChars(ptr4, offset, _a >> 24, _a >> 16);
				offset = HexsToChars(ptr4, offset, _a >> 8, _a);
				if (flag)
				{
					ptr4[offset++] = '-';
				}
				offset = HexsToChars(ptr4, offset, _b >> 8, _b);
				if (flag)
				{
					ptr4[offset++] = '-';
				}
				offset = HexsToChars(ptr4, offset, _c >> 8, _c);
				if (flag)
				{
					ptr4[offset++] = '-';
				}
				offset = HexsToChars(ptr4, offset, _d, _e);
				if (flag)
				{
					ptr4[offset++] = '-';
				}
				offset = HexsToChars(ptr4, offset, _f, _g);
				offset = HexsToChars(ptr4, offset, _h, _i);
				offset = HexsToChars(ptr4, offset, _j, _k);
			}
		}
		return text;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Guid" /> structure.</summary>
	/// <returns>A new GUID object.</returns>
	/// <filterpriority>1</filterpriority>
	public static Guid NewGuid()
	{
		byte[] array = new byte[16];
		lock (_rngAccess)
		{
			if (_rng == null)
			{
				_rng = RandomNumberGenerator.Create();
			}
			_rng.GetBytes(array);
		}
		Guid result = new Guid(array);
		result._d = (byte)((result._d & 0x3F) | 0x80);
		result._c = (short)(((ulong)result._c & 0xFFFuL) | 0x4000);
		return result;
	}

	internal static byte[] FastNewGuidArray()
	{
		byte[] array = new byte[16];
		lock (_rngAccess)
		{
			if (_rng != null)
			{
				_fastRng = _rng;
			}
			if (_fastRng == null)
			{
				_fastRng = new RNGCryptoServiceProvider();
			}
			_fastRng.GetBytes(array);
		}
		array[8] = (byte)((array[8] & 0x3F) | 0x80);
		array[7] = (byte)((array[7] & 0xF) | 0x40);
		return array;
	}
}
