using System;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace MS.Internal.TextFormatting;

internal sealed class FormatSettings
{
	private TextFormatterImp _formatter;

	private TextSource _textSource;

	private TextRunCacheImp _runCache;

	private ParaProp _pap;

	private DigitState _digitState;

	private TextLineBreak _previousLineBreak;

	private int _maxLineWidth;

	private int _textIndent;

	private TextFormattingMode _textFormattingMode;

	private bool _isSideways;

	internal TextFormattingMode TextFormattingMode => _textFormattingMode;

	internal bool IsSideways => _isSideways;

	internal TextFormatterImp Formatter => _formatter;

	internal TextSource TextSource => _textSource;

	internal TextLineBreak PreviousLineBreak => _previousLineBreak;

	internal ParaProp Pap => _pap;

	internal int MaxLineWidth => _maxLineWidth;

	internal DigitState DigitState => _digitState;

	internal int TextIndent => _textIndent;

	internal FormatSettings(TextFormatterImp formatter, TextSource textSource, TextRunCacheImp runCache, ParaProp pap, TextLineBreak previousLineBreak, bool isSingleLineFormatting, TextFormattingMode textFormattingMode, bool isSideways)
	{
		_isSideways = isSideways;
		_textFormattingMode = textFormattingMode;
		_formatter = formatter;
		_textSource = textSource;
		_runCache = runCache;
		_pap = pap;
		_digitState = new DigitState();
		_previousLineBreak = previousLineBreak;
		_maxLineWidth = 1073741822;
		if (isSingleLineFormatting)
		{
			_textIndent = _pap.Indent;
		}
	}

	internal void UpdateSettingsForCurrentLine(int maxLineWidth, TextLineBreak previousLineBreak, bool isFirstLineInPara)
	{
		_previousLineBreak = previousLineBreak;
		_digitState = new DigitState();
		_maxLineWidth = Math.Max(maxLineWidth, 0);
		if (isFirstLineInPara)
		{
			_textIndent = _pap.Indent;
		}
		else
		{
			_textIndent = 0;
		}
	}

	internal int GetFormatWidth(int finiteFormatWidth)
	{
		if (!_pap.Wrap)
		{
			return 1073741822;
		}
		return finiteFormatWidth;
	}

	internal int GetFiniteFormatWidth(int paragraphWidth)
	{
		return Math.Min(Math.Max(((paragraphWidth <= 0) ? 1073741822 : paragraphWidth) - _pap.ParagraphIndent, 0), 1073741822);
	}

	internal unsafe CharacterBufferRange FetchTextRun(int cpFetch, int cpFirst, out TextRun textRun, out int runLength)
	{
		textRun = _runCache.FetchTextRun(this, cpFetch, cpFirst, out var offsetToFirstCp, out runLength);
		CharacterBufferRange result;
		switch (TextRunInfo.GetRunType(textRun))
		{
		case Plsrun.Text:
		{
			CharacterBufferReference characterBufferReference = textRun.CharacterBufferReference;
			result = new CharacterBufferRange(characterBufferReference.CharacterBuffer, characterBufferReference.OffsetToFirstChar + offsetToFirstCp, runLength);
			break;
		}
		case Plsrun.InlineObject:
			result = new CharacterBufferRange((char*)TextStore.PwchObjectReplacement, 1);
			break;
		case Plsrun.LineBreak:
			result = new CharacterBufferRange((char*)TextStore.PwchLineSeparator, 1);
			break;
		case Plsrun.ParaBreak:
			result = new CharacterBufferRange((char*)TextStore.PwchParaSeparator, 1);
			break;
		case Plsrun.Hidden:
			result = new CharacterBufferRange((char*)TextStore.PwchHidden, 1);
			break;
		default:
			return CharacterBufferRange.Empty;
		}
		return result;
	}

	internal TextSpan<CultureSpecificCharacterBufferRange> GetPrecedingText(int cpLimit)
	{
		return _runCache.GetPrecedingText(_textSource, cpLimit);
	}
}
