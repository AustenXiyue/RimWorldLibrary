using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;
using MS.Internal.Text;

namespace System.Windows.Documents;

internal class RtfToXamlLexer
{
	private byte[] _rtfBytes;

	private int _rtfIndex;

	private int _rtfLastIndex;

	private int _currentCodePage;

	private Encoding _currentEncoding;

	private static object _rtfControlTableMutex = new object();

	private static Hashtable _rtfControlTable = null;

	private const int MAX_CONTROL_LENGTH = 32;

	private const int MAX_PARAM_LENGTH = 10;

	internal int CodePage
	{
		set
		{
			if (_currentCodePage != value)
			{
				_currentCodePage = value;
				_currentEncoding = InternalEncoding.GetEncoding(_currentCodePage);
			}
		}
	}

	internal Encoding CurrentEncoding => _currentEncoding;

	internal byte CurByte => _rtfBytes[_rtfIndex];

	internal RtfToXamlLexer(byte[] rtfBytes)
	{
		_rtfBytes = rtfBytes;
		_currentCodePage = CultureInfo.CurrentCulture.TextInfo.ANSICodePage;
		_currentEncoding = InternalEncoding.GetEncoding(_currentCodePage);
	}

	internal RtfToXamlError Next(RtfToken token, FormatState formatState)
	{
		RtfToXamlError result = RtfToXamlError.None;
		_rtfLastIndex = _rtfIndex;
		token.Empty();
		if (_rtfIndex >= _rtfBytes.Length)
		{
			token.Type = RtfTokenType.TokenEOF;
			return result;
		}
		int rtfIndex = _rtfIndex;
		switch (_rtfBytes[_rtfIndex++])
		{
		case 123:
			token.Type = RtfTokenType.TokenGroupStart;
			break;
		case 125:
			token.Type = RtfTokenType.TokenGroupEnd;
			break;
		case 10:
		case 13:
			token.Type = RtfTokenType.TokenNewline;
			break;
		case 0:
			token.Type = RtfTokenType.TokenNullChar;
			break;
		case 92:
			if (_rtfIndex >= _rtfBytes.Length)
			{
				token.Type = RtfTokenType.TokenInvalid;
				break;
			}
			if (IsControlCharValid(CurByte))
			{
				int rtfIndex2 = _rtfIndex;
				SetRtfIndex(token, rtfIndex2);
				token.Text = CurrentEncoding.GetString(_rtfBytes, rtfIndex2 - 1, _rtfIndex - rtfIndex);
				break;
			}
			if (CurByte == 39)
			{
				_rtfIndex--;
				return NextText(token);
			}
			if (CurByte == 42)
			{
				_rtfIndex++;
				token.Type = RtfTokenType.TokenDestination;
			}
			else
			{
				token.Type = RtfTokenType.TokenTextSymbol;
				token.Text = CurrentEncoding.GetString(_rtfBytes, _rtfIndex, 1);
				_rtfIndex++;
			}
			break;
		default:
			_rtfIndex--;
			if (formatState != null && formatState.RtfDestination == RtfDestination.DestPicture)
			{
				token.Type = RtfTokenType.TokenPictureData;
				break;
			}
			return NextText(token);
		}
		return result;
	}

	internal RtfToXamlError AdvanceForUnicode(long nSkip)
	{
		RtfToXamlError rtfToXamlError = RtfToXamlError.None;
		RtfToken rtfToken = new RtfToken();
		while (nSkip > 0 && rtfToXamlError == RtfToXamlError.None)
		{
			rtfToXamlError = Next(rtfToken, null);
			if (rtfToXamlError != 0)
			{
				break;
			}
			switch (rtfToken.Type)
			{
			default:
				Backup();
				nSkip = 0L;
				break;
			case RtfTokenType.TokenControl:
				if (rtfToken.RtfControlWordInfo != null && rtfToken.RtfControlWordInfo.Control == RtfControlWord.Ctrl_BIN)
				{
					AdvanceForBinary((int)rtfToken.Parameter);
				}
				nSkip--;
				break;
			case RtfTokenType.TokenText:
			{
				int rtfIndex = _rtfIndex;
				Backup();
				while (nSkip > 0 && _rtfIndex < rtfIndex)
				{
					if (CurByte == 92)
					{
						_rtfIndex += 4;
					}
					else
					{
						_rtfIndex++;
					}
					nSkip--;
				}
				break;
			}
			case RtfTokenType.TokenNewline:
			case RtfTokenType.TokenNullChar:
				break;
			}
		}
		return rtfToXamlError;
	}

	internal void AdvanceForBinary(int skip)
	{
		if (_rtfIndex + skip < _rtfBytes.Length)
		{
			_rtfIndex += skip;
		}
		else
		{
			_rtfIndex = _rtfBytes.Length - 1;
		}
	}

	internal void AdvanceForImageData()
	{
		for (byte b = _rtfBytes[_rtfIndex]; b != 125; b = _rtfBytes[_rtfIndex++])
		{
		}
		_rtfIndex--;
	}

	internal void WriteImageData(Stream imageStream, bool isBinary)
	{
		byte b = _rtfBytes[_rtfIndex];
		while (b != 123 && b != 125 && b != 92)
		{
			if (isBinary)
			{
				imageStream.WriteByte(b);
			}
			else
			{
				byte b2 = _rtfBytes[_rtfIndex + 1];
				if (IsHex(b) && IsHex(b2))
				{
					byte b3 = HexToByte(b);
					byte b4 = HexToByte(b2);
					imageStream.WriteByte((byte)((b3 << 4) | b4));
					_rtfIndex++;
				}
			}
			_rtfIndex++;
			b = _rtfBytes[_rtfIndex];
		}
	}

	private RtfToXamlError NextText(RtfToken token)
	{
		RtfToXamlError result = RtfToXamlError.None;
		_rtfLastIndex = _rtfIndex;
		token.Empty();
		token.Type = RtfTokenType.TokenText;
		int num = _rtfIndex;
		int num2 = num;
		bool flag = false;
		while (num2 < _rtfBytes.Length)
		{
			if (IsControl(_rtfBytes[num2]))
			{
				if (_rtfBytes[num2] != 92 || num2 + 3 >= _rtfBytes.Length || _rtfBytes[num2 + 1] != 39 || !IsHex(_rtfBytes[num2 + 2]) || !IsHex(_rtfBytes[num2 + 3]))
				{
					break;
				}
				num2 += 4;
				flag = true;
			}
			else
			{
				if (_rtfBytes[num2] == 13 || _rtfBytes[num2] == 10 || _rtfBytes[num2] == 0)
				{
					break;
				}
				num2++;
			}
		}
		if (num == num2)
		{
			token.Type = RtfTokenType.TokenInvalid;
		}
		else
		{
			_rtfIndex = num2;
			if (flag)
			{
				int count = 0;
				byte[] array = new byte[num2 - num];
				while (num < num2)
				{
					if (_rtfBytes[num] == 92)
					{
						array[count++] = (byte)((byte)(HexToByte(_rtfBytes[num + 2]) << 4) + HexToByte(_rtfBytes[num + 3]));
						num += 4;
					}
					else
					{
						array[count++] = _rtfBytes[num++];
					}
				}
				token.Text = CurrentEncoding.GetString(array, 0, count);
			}
			else
			{
				token.Text = CurrentEncoding.GetString(_rtfBytes, num, num2 - num);
			}
		}
		return result;
	}

	private RtfToXamlError Backup()
	{
		if (_rtfLastIndex == 0)
		{
			return RtfToXamlError.InvalidFormat;
		}
		_rtfIndex = _rtfLastIndex;
		_rtfLastIndex = 0;
		return RtfToXamlError.None;
	}

	private void SetRtfIndex(RtfToken token, int controlStartIndex)
	{
		while (_rtfIndex < _rtfBytes.Length && IsControlCharValid(CurByte))
		{
			_rtfIndex++;
		}
		int num = _rtfIndex - controlStartIndex;
		string @string = CurrentEncoding.GetString(_rtfBytes, controlStartIndex, num);
		if (num > 32)
		{
			token.Type = RtfTokenType.TokenInvalid;
			return;
		}
		token.Type = RtfTokenType.TokenControl;
		token.RtfControlWordInfo = RtfControlWordLookup(@string);
		if (_rtfIndex >= _rtfBytes.Length)
		{
			return;
		}
		if (CurByte == 32)
		{
			_rtfIndex++;
		}
		else if (IsParameterStart(CurByte))
		{
			bool flag = false;
			if (CurByte == 45)
			{
				flag = true;
				_rtfIndex++;
			}
			long num2 = 0L;
			int rtfIndex = _rtfIndex;
			while (_rtfIndex < _rtfBytes.Length && IsParameterFollow(CurByte))
			{
				num2 = num2 * 10 + (CurByte - 48);
				_rtfIndex++;
			}
			int num3 = _rtfIndex - rtfIndex;
			if (_rtfIndex < _rtfBytes.Length && CurByte == 32)
			{
				_rtfIndex++;
			}
			if (flag)
			{
				num2 = -num2;
			}
			if (num3 > 10)
			{
				token.Type = RtfTokenType.TokenInvalid;
			}
			else
			{
				token.Parameter = num2;
			}
		}
	}

	private bool IsControl(byte controlChar)
	{
		if (controlChar != 92 && controlChar != 123)
		{
			return controlChar == 125;
		}
		return true;
	}

	private bool IsControlCharValid(byte controlChar)
	{
		if (controlChar < 97 || controlChar > 122)
		{
			if (controlChar >= 65)
			{
				return controlChar <= 90;
			}
			return false;
		}
		return true;
	}

	private bool IsParameterStart(byte controlChar)
	{
		if (controlChar != 45)
		{
			if (controlChar >= 48)
			{
				return controlChar <= 57;
			}
			return false;
		}
		return true;
	}

	private bool IsParameterFollow(byte controlChar)
	{
		if (controlChar >= 48)
		{
			return controlChar <= 57;
		}
		return false;
	}

	private bool IsHex(byte controlChar)
	{
		if ((controlChar < 48 || controlChar > 57) && (controlChar < 97 || controlChar > 102))
		{
			if (controlChar >= 65)
			{
				return controlChar <= 70;
			}
			return false;
		}
		return true;
	}

	private byte HexToByte(byte hexByte)
	{
		if (hexByte >= 48 && hexByte <= 57)
		{
			return (byte)(hexByte - 48);
		}
		if (hexByte >= 97 && hexByte <= 102)
		{
			return (byte)(10 + hexByte - 97);
		}
		if (hexByte >= 65 && hexByte <= 70)
		{
			return (byte)(10 + hexByte - 65);
		}
		return 0;
	}

	private static RtfControlWordInfo RtfControlWordLookup(string controlName)
	{
		lock (_rtfControlTableMutex)
		{
			if (_rtfControlTable == null)
			{
				RtfControlWordInfo[] controlTable = RtfControls.ControlTable;
				_rtfControlTable = new Hashtable(controlTable.Length);
				for (int i = 0; i < controlTable.Length; i++)
				{
					_rtfControlTable.Add(controlTable[i].ControlName, controlTable[i]);
				}
			}
		}
		RtfControlWordInfo rtfControlWordInfo = (RtfControlWordInfo)_rtfControlTable[controlName];
		if (rtfControlWordInfo == null)
		{
			controlName = controlName.ToLower(CultureInfo.InvariantCulture);
			rtfControlWordInfo = (RtfControlWordInfo)_rtfControlTable[controlName];
		}
		return rtfControlWordInfo;
	}
}
