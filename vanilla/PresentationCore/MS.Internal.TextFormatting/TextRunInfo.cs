using System;
using System.Globalization;
using System.Windows.Media.TextFormatting;

namespace MS.Internal.TextFormatting;

internal sealed class TextRunInfo
{
	[Flags]
	private enum RunFlags
	{
		ContextualSubstitution = 1,
		IsSymbol = 2
	}

	private CharacterBufferRange _charBufferRange;

	private int _textRunLength;

	private int _offsetToFirstCp;

	private TextRun _textRun;

	private Plsrun _plsrun;

	private CultureInfo _digitCulture;

	private ushort _charFlags;

	private ushort _runFlags;

	private TextModifierScope _modifierScope;

	private TextRunProperties _properties;

	internal TextRun TextRun => _textRun;

	internal TextRunProperties Properties
	{
		get
		{
			if (_properties == null)
			{
				if (_modifierScope != null)
				{
					_properties = _modifierScope.ModifyProperties(_textRun.Properties);
				}
				else
				{
					_properties = _textRun.Properties;
				}
			}
			return _properties;
		}
	}

	internal CharacterBuffer CharacterBuffer => _charBufferRange.CharacterBuffer;

	internal int OffsetToFirstChar => _charBufferRange.OffsetToFirstChar;

	internal int OffsetToFirstCp => _offsetToFirstCp;

	internal int StringLength
	{
		get
		{
			return _charBufferRange.Length;
		}
		set
		{
			_charBufferRange = new CharacterBufferRange(_charBufferRange.CharacterBufferReference, value);
		}
	}

	internal int Length
	{
		get
		{
			return _textRunLength;
		}
		set
		{
			_textRunLength = value;
		}
	}

	internal ushort CharacterAttributeFlags
	{
		get
		{
			return _charFlags;
		}
		set
		{
			_charFlags = value;
		}
	}

	internal CultureInfo DigitCulture => _digitCulture;

	internal bool ContextualSubstitution => (_runFlags & 1) != 0;

	internal bool IsSymbol => (_runFlags & 2) != 0;

	internal Plsrun Plsrun => _plsrun;

	internal bool IsEndOfLine => _textRun is TextEndOfLine;

	internal TextModifierScope TextModifierScope => _modifierScope;

	internal TextRunInfo(CharacterBufferRange charBufferRange, int textRunLength, int offsetToFirstCp, TextRun textRun, Plsrun lsRunType, ushort charFlags, CultureInfo digitCulture, bool contextualSubstitution, bool symbolTypeface, TextModifierScope modifierScope)
	{
		_charBufferRange = charBufferRange;
		_textRunLength = textRunLength;
		_offsetToFirstCp = offsetToFirstCp;
		_textRun = textRun;
		_plsrun = lsRunType;
		_charFlags = charFlags;
		_digitCulture = digitCulture;
		_runFlags = 0;
		_modifierScope = modifierScope;
		if (contextualSubstitution)
		{
			_runFlags |= 1;
		}
		if (symbolTypeface)
		{
			_runFlags |= 2;
		}
	}

	internal int GetRoughWidth(double realToIdeal)
	{
		TextRunProperties properties = _textRun.Properties;
		if (properties != null)
		{
			return (int)Math.Round(properties.FontRenderingEmSize * 0.75 * (double)_textRunLength * realToIdeal);
		}
		return 0;
	}

	internal static Plsrun GetRunType(TextRun textRun)
	{
		if (textRun is ITextSymbols || textRun is TextShapeableSymbols)
		{
			return Plsrun.Text;
		}
		if (textRun is TextEmbeddedObject)
		{
			return Plsrun.InlineObject;
		}
		if (textRun is TextEndOfParagraph)
		{
			return Plsrun.ParaBreak;
		}
		if (textRun is TextEndOfLine)
		{
			return Plsrun.LineBreak;
		}
		return Plsrun.Hidden;
	}
}
