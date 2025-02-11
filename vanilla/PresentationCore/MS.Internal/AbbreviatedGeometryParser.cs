using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using MS.Internal.PresentationCore;

namespace MS.Internal;

internal sealed class AbbreviatedGeometryParser
{
	private const bool AllowSign = true;

	private const bool AllowComma = true;

	private const bool IsFilled = true;

	private const bool IsClosed = true;

	private const bool IsStroked = true;

	private const bool IsSmoothJoin = true;

	private IFormatProvider _formatProvider;

	private string _pathString;

	private int _pathLength;

	private int _curIndex;

	private bool _figureStarted;

	private Point _lastStart;

	private Point _lastPoint;

	private Point _secondLastPoint;

	private char _token;

	private StreamGeometryContext _context;

	private void ThrowBadToken()
	{
		throw new FormatException(SR.Format(SR.Parser_UnexpectedToken, _pathString, _curIndex - 1));
	}

	private bool More()
	{
		return _curIndex < _pathLength;
	}

	private bool SkipWhiteSpace(bool allowComma)
	{
		bool result = false;
		while (More())
		{
			char c = _pathString[_curIndex];
			switch (c)
			{
			case ',':
				if (allowComma)
				{
					result = true;
					allowComma = false;
				}
				else
				{
					ThrowBadToken();
				}
				break;
			default:
				if ((c > ' ' && c <= 'z') || !char.IsWhiteSpace(c))
				{
					return result;
				}
				break;
			case '\t':
			case '\n':
			case '\r':
			case ' ':
				break;
			}
			_curIndex++;
		}
		return result;
	}

	private bool ReadToken()
	{
		SkipWhiteSpace(allowComma: false);
		if (More())
		{
			_token = _pathString[_curIndex++];
			return true;
		}
		return false;
	}

	private bool IsNumber(bool allowComma)
	{
		bool flag = SkipWhiteSpace(allowComma);
		if (More())
		{
			_token = _pathString[_curIndex];
			if (_token == '.' || _token == '-' || _token == '+' || (_token >= '0' && _token <= '9') || _token == 'I' || _token == 'N')
			{
				return true;
			}
		}
		if (flag)
		{
			ThrowBadToken();
		}
		return false;
	}

	private void SkipDigits(bool signAllowed)
	{
		if (signAllowed && More() && (_pathString[_curIndex] == '-' || _pathString[_curIndex] == '+'))
		{
			_curIndex++;
		}
		while (More() && _pathString[_curIndex] >= '0' && _pathString[_curIndex] <= '9')
		{
			_curIndex++;
		}
	}

	private double ReadNumber(bool allowComma)
	{
		if (!IsNumber(allowComma))
		{
			ThrowBadToken();
		}
		bool flag = true;
		int i = _curIndex;
		if (More() && (_pathString[_curIndex] == '-' || _pathString[_curIndex] == '+'))
		{
			_curIndex++;
		}
		if (More() && _pathString[_curIndex] == 'I')
		{
			_curIndex = Math.Min(_curIndex + 8, _pathLength);
			flag = false;
		}
		else if (More() && _pathString[_curIndex] == 'N')
		{
			_curIndex = Math.Min(_curIndex + 3, _pathLength);
			flag = false;
		}
		else
		{
			SkipDigits(signAllowed: false);
			if (More() && _pathString[_curIndex] == '.')
			{
				flag = false;
				_curIndex++;
				SkipDigits(signAllowed: false);
			}
			if (More() && (_pathString[_curIndex] == 'E' || _pathString[_curIndex] == 'e'))
			{
				flag = false;
				_curIndex++;
				SkipDigits(signAllowed: true);
			}
		}
		if (flag && _curIndex <= i + 8)
		{
			int num = 1;
			if (_pathString[i] == '+')
			{
				i++;
			}
			else if (_pathString[i] == '-')
			{
				i++;
				num = -1;
			}
			int num2 = 0;
			for (; i < _curIndex; i++)
			{
				num2 = num2 * 10 + (_pathString[i] - 48);
			}
			return num2 * num;
		}
		try
		{
			return double.Parse(_pathString.AsSpan(i, _curIndex - i), _formatProvider);
		}
		catch (FormatException innerException)
		{
			throw new FormatException(SR.Format(SR.Parser_UnexpectedToken, _pathString, i), innerException);
		}
	}

	private bool ReadBool()
	{
		SkipWhiteSpace(allowComma: true);
		if (More())
		{
			_token = _pathString[_curIndex++];
			if (_token == '0')
			{
				return false;
			}
			if (_token == '1')
			{
				return true;
			}
		}
		ThrowBadToken();
		return false;
	}

	private Point ReadPoint(char cmd, bool allowcomma)
	{
		double num = ReadNumber(allowcomma);
		double num2 = ReadNumber(allowComma: true);
		if (cmd >= 'a')
		{
			num += _lastPoint.X;
			num2 += _lastPoint.Y;
		}
		return new Point(num, num2);
	}

	private Point Reflect()
	{
		return new Point(2.0 * _lastPoint.X - _secondLastPoint.X, 2.0 * _lastPoint.Y - _secondLastPoint.Y);
	}

	private void EnsureFigure()
	{
		if (!_figureStarted)
		{
			_context.BeginFigure(_lastStart, isFilled: true, isClosed: false);
			_figureStarted = true;
		}
	}

	internal void ParseToGeometryContext(StreamGeometryContext context, string pathString, int startIndex)
	{
		_formatProvider = CultureInfo.InvariantCulture;
		_context = context;
		_pathString = pathString;
		_pathLength = pathString.Length;
		_curIndex = startIndex;
		_secondLastPoint = new Point(0.0, 0.0);
		_lastPoint = new Point(0.0, 0.0);
		_lastStart = new Point(0.0, 0.0);
		_figureStarted = false;
		bool flag = true;
		char c = ' ';
		while (ReadToken())
		{
			char token = _token;
			if (flag)
			{
				if (token != 'M' && token != 'm')
				{
					ThrowBadToken();
				}
				flag = false;
			}
			switch (token)
			{
			case 'M':
			case 'm':
				_lastPoint = ReadPoint(token, allowcomma: false);
				context.BeginFigure(_lastPoint, isFilled: true, isClosed: false);
				_figureStarted = true;
				_lastStart = _lastPoint;
				c = 'M';
				while (IsNumber(allowComma: true))
				{
					_lastPoint = ReadPoint(token, allowcomma: false);
					context.LineTo(_lastPoint, isStroked: true, isSmoothJoin: false);
					c = 'L';
				}
				break;
			case 'H':
			case 'L':
			case 'V':
			case 'h':
			case 'l':
			case 'v':
				EnsureFigure();
				do
				{
					switch (token)
					{
					case 'l':
						_lastPoint = ReadPoint(token, allowcomma: false);
						break;
					case 'L':
						_lastPoint = ReadPoint(token, allowcomma: false);
						break;
					case 'h':
						_lastPoint.X += ReadNumber(allowComma: false);
						break;
					case 'H':
						_lastPoint.X = ReadNumber(allowComma: false);
						break;
					case 'v':
						_lastPoint.Y += ReadNumber(allowComma: false);
						break;
					case 'V':
						_lastPoint.Y = ReadNumber(allowComma: false);
						break;
					}
					context.LineTo(_lastPoint, isStroked: true, isSmoothJoin: false);
				}
				while (IsNumber(allowComma: true));
				c = 'L';
				break;
			case 'C':
			case 'S':
			case 'c':
			case 's':
				EnsureFigure();
				do
				{
					Point point;
					if (token == 's' || token == 'S')
					{
						point = ((c != 'C') ? _lastPoint : Reflect());
						_secondLastPoint = ReadPoint(token, allowcomma: false);
					}
					else
					{
						point = ReadPoint(token, allowcomma: false);
						_secondLastPoint = ReadPoint(token, allowcomma: true);
					}
					_lastPoint = ReadPoint(token, allowcomma: true);
					context.BezierTo(point, _secondLastPoint, _lastPoint, isStroked: true, isSmoothJoin: false);
					c = 'C';
				}
				while (IsNumber(allowComma: true));
				break;
			case 'Q':
			case 'T':
			case 'q':
			case 't':
				EnsureFigure();
				do
				{
					if (token == 't' || token == 'T')
					{
						if (c == 'Q')
						{
							_secondLastPoint = Reflect();
						}
						else
						{
							_secondLastPoint = _lastPoint;
						}
						_lastPoint = ReadPoint(token, allowcomma: false);
					}
					else
					{
						_secondLastPoint = ReadPoint(token, allowcomma: false);
						_lastPoint = ReadPoint(token, allowcomma: true);
					}
					context.QuadraticBezierTo(_secondLastPoint, _lastPoint, isStroked: true, isSmoothJoin: false);
					c = 'Q';
				}
				while (IsNumber(allowComma: true));
				break;
			case 'A':
			case 'a':
				EnsureFigure();
				do
				{
					double width = ReadNumber(allowComma: false);
					double height = ReadNumber(allowComma: true);
					double rotationAngle = ReadNumber(allowComma: true);
					bool isLargeArc = ReadBool();
					bool flag2 = ReadBool();
					_lastPoint = ReadPoint(token, allowcomma: true);
					context.ArcTo(_lastPoint, new Size(width, height), rotationAngle, isLargeArc, flag2 ? SweepDirection.Clockwise : SweepDirection.Counterclockwise, isStroked: true, isSmoothJoin: false);
				}
				while (IsNumber(allowComma: true));
				c = 'A';
				break;
			case 'Z':
			case 'z':
				EnsureFigure();
				context.SetClosedState(closed: true);
				_figureStarted = false;
				c = 'Z';
				_lastPoint = _lastStart;
				break;
			default:
				ThrowBadToken();
				break;
			}
		}
	}
}
