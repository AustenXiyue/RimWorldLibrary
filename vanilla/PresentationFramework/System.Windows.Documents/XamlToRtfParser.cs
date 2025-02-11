using System.Collections;

namespace System.Windows.Documents;

internal class XamlToRtfParser
{
	internal class XamlLexer
	{
		private string _xaml;

		private int _xamlIndex;

		internal XamlLexer(string xaml)
		{
			_xaml = xaml;
		}

		internal XamlToRtfError Next(XamlToken token)
		{
			XamlToRtfError result = XamlToRtfError.None;
			int xamlIndex = _xamlIndex;
			if (_xamlIndex < _xaml.Length)
			{
				switch (_xaml[_xamlIndex])
				{
				case '\t':
				case '\n':
				case '\r':
				case ' ':
					token.TokenType = XamlTokenType.XTokWS;
					_xamlIndex++;
					while (IsCharsAvailable(1) && IsSpace(_xaml[_xamlIndex]))
					{
						_xamlIndex++;
					}
					break;
				case '<':
					NextLessThanToken(token);
					break;
				case '&':
					token.TokenType = XamlTokenType.XTokInvalid;
					_xamlIndex++;
					while (IsCharsAvailable(1))
					{
						if (_xaml[_xamlIndex] == ';')
						{
							_xamlIndex++;
							token.TokenType = XamlTokenType.XTokEntity;
							break;
						}
						_xamlIndex++;
					}
					break;
				default:
					token.TokenType = XamlTokenType.XTokCharacters;
					_xamlIndex++;
					while (IsCharsAvailable(1) && _xaml[_xamlIndex] != '&' && _xaml[_xamlIndex] != '<')
					{
						_xamlIndex++;
					}
					break;
				}
			}
			token.Text = _xaml.Substring(xamlIndex, _xamlIndex - xamlIndex);
			if (token.Text.Length == 0)
			{
				token.TokenType = XamlTokenType.XTokEOF;
			}
			return result;
		}

		private bool IsSpace(char character)
		{
			if (character != ' ' && character != '\t' && character != '\n')
			{
				return character == '\r';
			}
			return true;
		}

		private bool IsCharsAvailable(int index)
		{
			return _xamlIndex + index <= _xaml.Length;
		}

		private void NextLessThanToken(XamlToken token)
		{
			_xamlIndex++;
			if (!IsCharsAvailable(1))
			{
				token.TokenType = XamlTokenType.XTokInvalid;
				return;
			}
			token.TokenType = XamlTokenType.XTokInvalid;
			switch (_xaml[_xamlIndex])
			{
			case '?':
				_xamlIndex++;
				while (IsCharsAvailable(2))
				{
					if (_xaml[_xamlIndex] == '?' && _xaml[_xamlIndex + 1] == '>')
					{
						_xamlIndex += 2;
						token.TokenType = XamlTokenType.XTokPI;
						break;
					}
					_xamlIndex++;
				}
				return;
			case '!':
				_xamlIndex++;
				while (IsCharsAvailable(3))
				{
					if (_xaml[_xamlIndex] == '-' && _xaml[_xamlIndex + 1] == '-' && _xaml[_xamlIndex + 2] == '>')
					{
						_xamlIndex += 3;
						token.TokenType = XamlTokenType.XTokComment;
						break;
					}
					_xamlIndex++;
				}
				return;
			case '>':
				_xamlIndex++;
				token.TokenType = XamlTokenType.XTokInvalid;
				return;
			case '/':
				_xamlIndex++;
				while (IsCharsAvailable(1))
				{
					if (_xaml[_xamlIndex] == '>')
					{
						_xamlIndex++;
						token.TokenType = XamlTokenType.XTokEndElement;
						break;
					}
					_xamlIndex++;
				}
				return;
			}
			char c = '\0';
			while (IsCharsAvailable(1))
			{
				if (c != 0)
				{
					if (_xaml[_xamlIndex] == c)
					{
						c = '\0';
					}
				}
				else if (_xaml[_xamlIndex] == '"' || _xaml[_xamlIndex] == '\'')
				{
					c = _xaml[_xamlIndex];
				}
				else if (_xaml[_xamlIndex] == '>')
				{
					_xamlIndex++;
					token.TokenType = XamlTokenType.XTokStartElement;
					break;
				}
				_xamlIndex++;
			}
		}
	}

	internal class XamlTagStack : ArrayList
	{
		internal XamlTagStack()
			: base(10)
		{
		}

		internal RtfToXamlError Push(string xamlTag)
		{
			Add(xamlTag);
			return RtfToXamlError.None;
		}

		internal void Pop()
		{
			if (Count > 0)
			{
				RemoveAt(Count - 1);
			}
		}

		internal bool IsMatchTop(string xamlTag)
		{
			if (Count == 0)
			{
				return false;
			}
			string text = (string)this[Count - 1];
			if (text.Length == 0)
			{
				return false;
			}
			if (string.Compare(xamlTag, xamlTag.Length, text, text.Length, text.Length, StringComparison.OrdinalIgnoreCase) == 0)
			{
				return true;
			}
			return false;
		}
	}

	internal class XamlAttributes : IXamlAttributes
	{
		private XamlParsePoints _xamlParsePoints;

		internal bool IsEmpty => _xamlParsePoints.IsEmpty;

		internal XamlAttributes(string xaml)
		{
			_xamlParsePoints = new XamlParsePoints();
		}

		internal XamlToRtfError Init(string xaml)
		{
			return _xamlParsePoints.Init(xaml);
		}

		internal XamlToRtfError GetTag(ref string xamlTag)
		{
			XamlToRtfError result = XamlToRtfError.None;
			if (!_xamlParsePoints.IsValid)
			{
				return XamlToRtfError.Unknown;
			}
			xamlTag = (string)_xamlParsePoints[0];
			return result;
		}

		XamlToRtfError IXamlAttributes.GetLength(ref int length)
		{
			XamlToRtfError result = XamlToRtfError.None;
			if (_xamlParsePoints.IsValid)
			{
				length = (_xamlParsePoints.Count - 1) / 2;
				return result;
			}
			return XamlToRtfError.Unknown;
		}

		XamlToRtfError IXamlAttributes.GetUri(int index, ref string uri)
		{
			return XamlToRtfError.None;
		}

		XamlToRtfError IXamlAttributes.GetLocalName(int index, ref string localName)
		{
			return XamlToRtfError.None;
		}

		XamlToRtfError IXamlAttributes.GetQName(int index, ref string qName)
		{
			return XamlToRtfError.None;
		}

		XamlToRtfError IXamlAttributes.GetName(int index, ref string uri, ref string localName, ref string qName)
		{
			XamlToRtfError result = XamlToRtfError.None;
			int num = (_xamlParsePoints.Count - 1) / 2;
			if (index < 0 || index > num - 1)
			{
				return XamlToRtfError.Unknown;
			}
			localName = (string)_xamlParsePoints[index * 2 + 1];
			qName = (string)_xamlParsePoints[index * 2 + 2];
			return result;
		}

		XamlToRtfError IXamlAttributes.GetIndexFromName(string uri, string localName, ref int index)
		{
			return XamlToRtfError.None;
		}

		XamlToRtfError IXamlAttributes.GetIndexFromQName(string qName, ref int index)
		{
			return XamlToRtfError.None;
		}

		XamlToRtfError IXamlAttributes.GetType(int index, ref string typeName)
		{
			return XamlToRtfError.None;
		}

		XamlToRtfError IXamlAttributes.GetTypeFromName(string uri, string localName, ref string typeName)
		{
			return XamlToRtfError.None;
		}

		XamlToRtfError IXamlAttributes.GetValue(int index, ref string valueName)
		{
			XamlToRtfError result = XamlToRtfError.None;
			int num = (_xamlParsePoints.Count - 1) / 2;
			if (index < 0 || index > num - 1)
			{
				return XamlToRtfError.OutOfRange;
			}
			valueName = (string)_xamlParsePoints[index * 2 + 2];
			return result;
		}

		XamlToRtfError IXamlAttributes.GetValueFromName(string uri, string localName, ref string valueName)
		{
			return XamlToRtfError.None;
		}

		XamlToRtfError IXamlAttributes.GetValueFromQName(string qName, ref string valueName)
		{
			return XamlToRtfError.None;
		}

		XamlToRtfError IXamlAttributes.GetTypeFromQName(string qName, ref string typeName)
		{
			return XamlToRtfError.None;
		}
	}

	internal class XamlParsePoints : ArrayList
	{
		private bool _empty;

		private bool _valid;

		internal bool IsEmpty => _empty;

		internal bool IsValid => _valid;

		internal XamlParsePoints()
			: base(10)
		{
		}

		internal XamlToRtfError Init(string xaml)
		{
			XamlToRtfError result = XamlToRtfError.None;
			_empty = false;
			_valid = false;
			Clear();
			int num = 0;
			if (xaml.Length < 2 || xaml[0] != '<' || xaml[xaml.Length - 1] != '>')
			{
				return XamlToRtfError.Unknown;
			}
			num++;
			if (IsSpace(xaml[num]))
			{
				return XamlToRtfError.Unknown;
			}
			if (xaml[num] == '/')
			{
				return HandleEndTag(xaml, num);
			}
			int num2 = num;
			for (num++; IsNameChar(xaml[num]); num++)
			{
			}
			AddParseData(xaml.Substring(num2, num - num2));
			while (num < xaml.Length)
			{
				for (; IsSpace(xaml[num]); num++)
				{
				}
				if (num == xaml.Length - 1)
				{
					break;
				}
				if (xaml[num] == '/')
				{
					if (num == xaml.Length - 2)
					{
						_empty = true;
						break;
					}
					return XamlToRtfError.Unknown;
				}
				num2 = num;
				for (num++; IsNameChar(xaml[num]); num++)
				{
				}
				AddParseData(xaml.Substring(num2, num - num2));
				if (num < xaml.Length)
				{
					for (; IsSpace(xaml[num]); num++)
					{
					}
				}
				if (num == xaml.Length || xaml[num] != '=')
				{
					return XamlToRtfError.Unknown;
				}
				for (num++; IsSpace(xaml[num]); num++)
				{
				}
				if (xaml[num] != '\'' && xaml[num] != '"')
				{
					return XamlToRtfError.Unknown;
				}
				char c = xaml[num++];
				num2 = num;
				for (; num < xaml.Length && xaml[num] != c; num++)
				{
				}
				if (num == xaml.Length)
				{
					return XamlToRtfError.Unknown;
				}
				AddParseData(xaml.Substring(num2, num - num2));
				num++;
			}
			_valid = true;
			return result;
		}

		internal void AddParseData(string parseData)
		{
			Add(parseData);
		}

		private bool IsSpace(char character)
		{
			if (character != ' ' && character != '\t' && character != '\n')
			{
				return character == '\r';
			}
			return true;
		}

		private bool IsNameChar(char character)
		{
			if (!IsSpace(character) && character != '=' && character != '>')
			{
				return character != '/';
			}
			return false;
		}

		private XamlToRtfError HandleEndTag(string xaml, int xamlIndex)
		{
			xamlIndex++;
			while (IsSpace(xaml[xamlIndex]))
			{
				xamlIndex++;
			}
			int num = xamlIndex;
			xamlIndex++;
			while (IsNameChar(xaml[xamlIndex]))
			{
				xamlIndex++;
			}
			AddParseData(xaml.Substring(num, xamlIndex - num));
			while (IsSpace(xaml[xamlIndex]))
			{
				xamlIndex++;
			}
			if (xamlIndex == xaml.Length - 1)
			{
				_valid = true;
				return XamlToRtfError.None;
			}
			return XamlToRtfError.Unknown;
		}
	}

	internal class XamlToken
	{
		private XamlTokenType _tokenType;

		private string _text;

		internal XamlTokenType TokenType
		{
			get
			{
				return _tokenType;
			}
			set
			{
				_tokenType = value;
			}
		}

		internal string Text
		{
			get
			{
				return _text;
			}
			set
			{
				_text = value;
			}
		}

		internal XamlToken()
		{
		}
	}

	private string _xaml;

	private XamlLexer _xamlLexer;

	private XamlTagStack _xamlTagStack;

	private XamlAttributes _xamlAttributes;

	private IXamlContentHandler _xamlContent;

	private IXamlErrorHandler _xamlError;

	internal XamlToRtfParser(string xaml)
	{
		_xaml = xaml;
		_xamlLexer = new XamlLexer(_xaml);
		_xamlTagStack = new XamlTagStack();
		_xamlAttributes = new XamlAttributes(_xaml);
	}

	internal XamlToRtfError Parse()
	{
		if (_xamlContent == null || _xamlError == null)
		{
			return XamlToRtfError.Unknown;
		}
		XamlToRtfError xamlToRtfError = XamlToRtfError.None;
		XamlToken xamlToken = new XamlToken();
		string name = string.Empty;
		xamlToRtfError = _xamlContent.StartDocument();
		while (xamlToRtfError == XamlToRtfError.None)
		{
			xamlToRtfError = _xamlLexer.Next(xamlToken);
			if (xamlToRtfError != 0 || xamlToken.TokenType == XamlTokenType.XTokEOF)
			{
				break;
			}
			switch (xamlToken.TokenType)
			{
			case XamlTokenType.XTokInvalid:
				xamlToRtfError = XamlToRtfError.Unknown;
				break;
			case XamlTokenType.XTokCharacters:
				xamlToRtfError = _xamlContent.Characters(xamlToken.Text);
				break;
			case XamlTokenType.XTokEntity:
				xamlToRtfError = _xamlContent.SkippedEntity(xamlToken.Text);
				break;
			case XamlTokenType.XTokStartElement:
				xamlToRtfError = ParseXTokStartElement(xamlToken, ref name);
				break;
			case XamlTokenType.XTokEndElement:
				xamlToRtfError = ParseXTokEndElement(xamlToken, ref name);
				break;
			case XamlTokenType.XTokWS:
				xamlToRtfError = _xamlContent.IgnorableWhitespace(xamlToken.Text);
				break;
			default:
				xamlToRtfError = XamlToRtfError.Unknown;
				break;
			case XamlTokenType.XTokCData:
			case XamlTokenType.XTokPI:
			case XamlTokenType.XTokComment:
				break;
			}
		}
		if (xamlToRtfError == XamlToRtfError.None && _xamlTagStack.Count != 0)
		{
			xamlToRtfError = XamlToRtfError.Unknown;
		}
		if (xamlToRtfError == XamlToRtfError.None)
		{
			xamlToRtfError = _xamlContent.EndDocument();
		}
		return xamlToRtfError;
	}

	internal void SetCallbacks(IXamlContentHandler xamlContent, IXamlErrorHandler xamlError)
	{
		_xamlContent = xamlContent;
		_xamlError = xamlError;
	}

	private XamlToRtfError ParseXTokStartElement(XamlToken xamlToken, ref string name)
	{
		XamlToRtfError xamlToRtfError = _xamlAttributes.Init(xamlToken.Text);
		if (xamlToRtfError == XamlToRtfError.None)
		{
			xamlToRtfError = _xamlAttributes.GetTag(ref name);
			if (xamlToRtfError == XamlToRtfError.None)
			{
				xamlToRtfError = _xamlContent.StartElement(string.Empty, name, name, _xamlAttributes);
				if (xamlToRtfError == XamlToRtfError.None)
				{
					xamlToRtfError = ((!_xamlAttributes.IsEmpty) ? ((XamlToRtfError)_xamlTagStack.Push(name)) : _xamlContent.EndElement(string.Empty, name, name));
				}
			}
		}
		return xamlToRtfError;
	}

	private XamlToRtfError ParseXTokEndElement(XamlToken xamlToken, ref string name)
	{
		XamlToRtfError xamlToRtfError = _xamlAttributes.Init(xamlToken.Text);
		if (xamlToRtfError == XamlToRtfError.None)
		{
			xamlToRtfError = _xamlAttributes.GetTag(ref name);
			if (xamlToRtfError == XamlToRtfError.None && _xamlTagStack.IsMatchTop(name))
			{
				_xamlTagStack.Pop();
				xamlToRtfError = _xamlContent.EndElement(string.Empty, name, name);
			}
		}
		return xamlToRtfError;
	}
}
