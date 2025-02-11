using System;
using System.Collections.Generic;
using System.Text;
using MS.Internal.WindowsBase;

namespace MS.Internal;

[MS.Internal.WindowsBase.FriendAccessAllowed]
internal sealed class ContentType
{
	internal class StrongComparer : IEqualityComparer<ContentType>
	{
		public bool Equals(ContentType x, ContentType y)
		{
			return x?.AreTypeAndSubTypeEqual(y) ?? (y == null);
		}

		public int GetHashCode(ContentType obj)
		{
			return obj.ToString().ToUpperInvariant().GetHashCode();
		}
	}

	internal class WeakComparer : IEqualityComparer<ContentType>
	{
		public bool Equals(ContentType x, ContentType y)
		{
			return x?.AreTypeAndSubTypeEqual(y, allowParameterValuePairs: true) ?? (y == null);
		}

		public int GetHashCode(ContentType obj)
		{
			return obj._type.ToUpperInvariant().GetHashCode() ^ obj._subType.ToUpperInvariant().GetHashCode();
		}
	}

	private string _contentType;

	private string _type = string.Empty;

	private string _subType = string.Empty;

	private string _originalString;

	private Dictionary<string, string> _parameterDictionary;

	private bool _isInitialized;

	private const string _quote = "\"";

	private const char _semicolonSeparator = ';';

	private const char _equalSeparator = '=';

	private static readonly char[] _allowedCharacters = new char[15]
	{
		'!', '#', '$', '%', '&', '\'', '*', '+', '-', '.',
		'^', '_', '`', '|', '~'
	};

	private static readonly char[] _LinearWhiteSpaceChars = new char[4] { ' ', '\n', '\r', '\t' };

	private static readonly ContentType _emptyContentType = new ContentType("");

	internal string TypeComponent => _type;

	internal string SubTypeComponent => _subType;

	internal Dictionary<string, string>.Enumerator ParameterValuePairs
	{
		get
		{
			EnsureParameterDictionary();
			return _parameterDictionary.GetEnumerator();
		}
	}

	internal static ContentType Empty => _emptyContentType;

	internal string OriginalString => _originalString;

	internal ContentType(string contentType)
	{
		if (contentType == null)
		{
			throw new ArgumentNullException("contentType");
		}
		if (contentType.Length == 0)
		{
			_contentType = string.Empty;
		}
		else
		{
			if (IsLinearWhiteSpaceChar(contentType[0]) || IsLinearWhiteSpaceChar(contentType[contentType.Length - 1]))
			{
				throw new ArgumentException(SR.ContentTypeCannotHaveLeadingTrailingLWS);
			}
			ValidateCarriageReturns(contentType);
			int num = contentType.IndexOf(';');
			if (num == -1)
			{
				ParseTypeAndSubType(contentType);
			}
			else
			{
				ParseTypeAndSubType(contentType.AsSpan(0, num));
				ParseParameterAndValue(contentType.Substring(num));
			}
		}
		_originalString = contentType;
		_isInitialized = true;
	}

	internal bool AreTypeAndSubTypeEqual(ContentType contentType)
	{
		return AreTypeAndSubTypeEqual(contentType, allowParameterValuePairs: false);
	}

	internal bool AreTypeAndSubTypeEqual(ContentType contentType, bool allowParameterValuePairs)
	{
		bool result = false;
		if (contentType != null)
		{
			if (!allowParameterValuePairs)
			{
				if (_parameterDictionary != null && _parameterDictionary.Count > 0)
				{
					return false;
				}
				Dictionary<string, string>.Enumerator parameterValuePairs = contentType.ParameterValuePairs;
				parameterValuePairs.MoveNext();
				if (parameterValuePairs.Current.Key != null)
				{
					return false;
				}
			}
			result = string.Compare(_type, contentType.TypeComponent, StringComparison.OrdinalIgnoreCase) == 0 && string.Compare(_subType, contentType.SubTypeComponent, StringComparison.OrdinalIgnoreCase) == 0;
		}
		return result;
	}

	public override string ToString()
	{
		if (_contentType == null)
		{
			if (!_isInitialized)
			{
				return string.Empty;
			}
			StringBuilder stringBuilder = new StringBuilder(_type);
			stringBuilder.Append('/');
			stringBuilder.Append(_subType);
			if (_parameterDictionary != null && _parameterDictionary.Count > 0)
			{
				foreach (string key in _parameterDictionary.Keys)
				{
					stringBuilder.Append(_LinearWhiteSpaceChars[0]);
					stringBuilder.Append(';');
					stringBuilder.Append(_LinearWhiteSpaceChars[0]);
					stringBuilder.Append(key);
					stringBuilder.Append('=');
					stringBuilder.Append(_parameterDictionary[key]);
				}
			}
			_contentType = stringBuilder.ToString();
		}
		return _contentType;
	}

	private static void ValidateCarriageReturns(string contentType)
	{
		int num = contentType.IndexOf(_LinearWhiteSpaceChars[2]);
		while (num != -1)
		{
			if (contentType[num - 1] == _LinearWhiteSpaceChars[1] || contentType[num + 1] == _LinearWhiteSpaceChars[1])
			{
				num = contentType.IndexOf(_LinearWhiteSpaceChars[2], ++num);
				continue;
			}
			throw new ArgumentException(SR.InvalidLinearWhiteSpaceCharacter);
		}
	}

	private void ParseTypeAndSubType(ReadOnlySpan<char> typeAndSubType)
	{
		typeAndSubType = typeAndSubType.TrimEnd(_LinearWhiteSpaceChars);
		int num = typeAndSubType.IndexOf('/');
		if (num < 0 || typeAndSubType.Slice(num + 1).IndexOf('/') >= 0)
		{
			throw new ArgumentException(SR.InvalidTypeSubType);
		}
		_type = ValidateToken(typeAndSubType.Slice(0, num).ToString());
		_subType = ValidateToken(typeAndSubType.Slice(num + 1).ToString());
	}

	private void ParseParameterAndValue(ReadOnlySpan<char> parameterAndValue)
	{
		while (!parameterAndValue.IsEmpty)
		{
			if (parameterAndValue[0] != ';')
			{
				throw new ArgumentException(SR.ExpectingSemicolon);
			}
			if (parameterAndValue.Length == 1)
			{
				throw new ArgumentException(SR.ExpectingParameterValuePairs);
			}
			parameterAndValue = parameterAndValue.Slice(1);
			parameterAndValue = parameterAndValue.TrimStart(_LinearWhiteSpaceChars);
			int num = parameterAndValue.IndexOf('=');
			if (num <= 0 || num == parameterAndValue.Length - 1)
			{
				throw new ArgumentException(SR.InvalidParameterValuePair);
			}
			int num2 = num + 1;
			int lengthOfParameterValue = GetLengthOfParameterValue(parameterAndValue, num2);
			EnsureParameterDictionary();
			_parameterDictionary.Add(ValidateToken(parameterAndValue.Slice(0, num).ToString()), ValidateQuotedStringOrToken(parameterAndValue.Slice(num2, lengthOfParameterValue).ToString()));
			parameterAndValue = parameterAndValue.Slice(num2 + lengthOfParameterValue).TrimStart(_LinearWhiteSpaceChars);
		}
	}

	private static int GetLengthOfParameterValue(ReadOnlySpan<char> s, int startIndex)
	{
		int num3;
		if (s[startIndex] != '"')
		{
			int num = s.Slice(startIndex).IndexOf(';');
			if (num != -1)
			{
				int num2 = s.Slice(startIndex).IndexOfAny(_LinearWhiteSpaceChars);
				num3 = ((num2 != -1 && num2 < num) ? num2 : num);
				num3 += startIndex;
			}
			else
			{
				num3 = s.Length;
			}
		}
		else
		{
			bool flag = false;
			num3 = startIndex;
			while (!flag)
			{
				int num4 = ++num3;
				num3 = s.Slice(num4).IndexOf('"');
				if (num3 == -1)
				{
					throw new ArgumentException(SR.InvalidParameterValue);
				}
				num3 += num4;
				if (s[num3 - 1] != '\\')
				{
					flag = true;
					num3++;
				}
			}
		}
		return num3 - startIndex;
	}

	private static string ValidateToken(string token)
	{
		if (string.IsNullOrEmpty(token))
		{
			throw new ArgumentException(SR.InvalidToken);
		}
		for (int i = 0; i < token.Length; i++)
		{
			if (!IsAsciiLetterOrDigit(token[i]) && !IsAllowedCharacter(token[i]))
			{
				throw new ArgumentException(SR.InvalidToken);
			}
		}
		return token;
	}

	private static string ValidateQuotedStringOrToken(string parameterValue)
	{
		if (string.IsNullOrEmpty(parameterValue))
		{
			throw new ArgumentException(SR.InvalidParameterValue);
		}
		if (parameterValue.Length >= 2 && parameterValue.StartsWith("\"", StringComparison.Ordinal) && parameterValue.EndsWith("\"", StringComparison.Ordinal))
		{
			ValidateQuotedText(parameterValue.AsSpan(1, parameterValue.Length - 2));
		}
		else
		{
			ValidateToken(parameterValue);
		}
		return parameterValue;
	}

	private static void ValidateQuotedText(ReadOnlySpan<char> quotedText)
	{
		for (int i = 0; i < quotedText.Length; i++)
		{
			if (!IsLinearWhiteSpaceChar(quotedText[i]))
			{
				if (quotedText[i] <= ' ' || quotedText[i] >= 'ÿ')
				{
					throw new ArgumentException(SR.InvalidParameterValue);
				}
				if (quotedText[i] == '"' && (i == 0 || quotedText[i - 1] != '\\'))
				{
					throw new ArgumentException(SR.InvalidParameterValue);
				}
			}
		}
	}

	private static bool IsAllowedCharacter(char character)
	{
		char[] allowedCharacters = _allowedCharacters;
		for (int i = 0; i < allowedCharacters.Length; i++)
		{
			if (allowedCharacters[i] == character)
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsAsciiLetterOrDigit(char character)
	{
		if (((uint)(character - 65) & -33) >= 26)
		{
			return (uint)(character - 48) < 10u;
		}
		return true;
	}

	private static bool IsLinearWhiteSpaceChar(char ch)
	{
		if (ch > ' ')
		{
			return false;
		}
		char[] linearWhiteSpaceChars = _LinearWhiteSpaceChars;
		foreach (char c in linearWhiteSpaceChars)
		{
			if (ch == c)
			{
				return true;
			}
		}
		return false;
	}

	private void EnsureParameterDictionary()
	{
		if (_parameterDictionary == null)
		{
			_parameterDictionary = new Dictionary<string, string>();
		}
	}
}
