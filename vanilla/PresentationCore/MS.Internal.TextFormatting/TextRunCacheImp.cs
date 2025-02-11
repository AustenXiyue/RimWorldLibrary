using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Media.TextFormatting;
using MS.Internal.PresentationCore;

namespace MS.Internal.TextFormatting;

internal sealed class TextRunCacheImp
{
	private SpanVector _textRunVector;

	private SpanPosition _latestPosition;

	internal TextRunCacheImp()
	{
		_textRunVector = new SpanVector(null);
		_latestPosition = default(SpanPosition);
	}

	public void Change(int textSourceCharacterIndex, int addition, int removal)
	{
		if (textSourceCharacterIndex >= 0)
		{
			int num = 0;
			for (int i = 0; i < _textRunVector.Count; i++)
			{
				num += _textRunVector[i].length;
			}
			if (textSourceCharacterIndex < num)
			{
				SpanRider spanRider = new SpanRider(_textRunVector, _latestPosition, textSourceCharacterIndex);
				_latestPosition = spanRider.SpanPosition;
				_latestPosition = _textRunVector.SetValue(spanRider.CurrentSpanStart, num - spanRider.CurrentSpanStart, _textRunVector.Default, _latestPosition);
			}
		}
	}

	internal TextRun FetchTextRun(FormatSettings settings, int cpFetch, int cpFirst, out int offsetToFirstCp, out int runLength)
	{
		SpanRider spanRider = new SpanRider(_textRunVector, _latestPosition, cpFetch);
		_latestPosition = spanRider.SpanPosition;
		TextRun textRun = (TextRun)spanRider.CurrentElement;
		if (textRun == null)
		{
			textRun = settings.TextSource.GetTextRun(cpFetch);
			if (textRun.Length < 1)
			{
				throw new ArgumentOutOfRangeException("textRun.Length", SR.ParameterMustBeGreaterThanZero);
			}
			Plsrun runType = TextRunInfo.GetRunType(textRun);
			if (runType == Plsrun.Text || runType == Plsrun.InlineObject)
			{
				TextRunProperties obj = textRun.Properties ?? throw new ArgumentException(SR.TextRunPropertiesCannotBeNull);
				if (obj.FontRenderingEmSize <= 0.0)
				{
					throw new ArgumentException(SR.Format(SR.PropertyOfClassMustBeGreaterThanZero, "FontRenderingEmSize", "TextRunProperties"));
				}
				double num = 35791.39406666667;
				if (obj.FontRenderingEmSize > num)
				{
					throw new ArgumentException(SR.Format(SR.PropertyOfClassCannotBeGreaterThan, "FontRenderingEmSize", "TextRunProperties", num));
				}
				if (CultureMapper.GetSpecificCulture(obj.CultureInfo) == null)
				{
					throw new ArgumentException(SR.Format(SR.PropertyOfClassCannotBeNull, "CultureInfo", "TextRunProperties"));
				}
				if (obj.Typeface == null)
				{
					throw new ArgumentException(SR.Format(SR.PropertyOfClassCannotBeNull, "Typeface", "TextRunProperties"));
				}
			}
			spanRider.At(cpFetch + textRun.Length - 1);
			_latestPosition = spanRider.SpanPosition;
			if (spanRider.CurrentElement != _textRunVector.Default)
			{
				_latestPosition = _textRunVector.SetReference(cpFetch, spanRider.CurrentPosition + spanRider.Length - cpFetch, _textRunVector.Default, _latestPosition);
			}
			_latestPosition = _textRunVector.SetReference(cpFetch, textRun.Length, textRun, _latestPosition);
			spanRider.At(_latestPosition, cpFetch);
		}
		if (textRun.Properties != null)
		{
			textRun.Properties.PixelsPerDip = settings.TextSource.PixelsPerDip;
		}
		offsetToFirstCp = spanRider.CurrentPosition - spanRider.CurrentSpanStart;
		runLength = spanRider.Length;
		if (textRun is ITextSymbols)
		{
			int num2 = 100 - cpFetch + cpFirst;
			if (num2 <= 0)
			{
				num2 = (int)Math.Round(25.0);
			}
			if (runLength > num2)
			{
				if (TextRunInfo.GetRunType(textRun) == Plsrun.Text)
				{
					CharacterBufferReference characterBufferReference = textRun.CharacterBufferReference;
					int num3 = Math.Min(runLength, num2 + 100);
					int sizeofChar = 0;
					int num4 = 0;
					bool flag = false;
					for (num4 = num2 - 1; num4 < num3; num4 += sizeofChar)
					{
						int unicodeScalar = Classification.UnicodeScalar(new CharacterBufferRange(characterBufferReference.CharacterBuffer, characterBufferReference.OffsetToFirstChar + offsetToFirstCp + num4, runLength - num4), out sizeofChar);
						if (flag && !Classification.IsCombining(unicodeScalar) && !Classification.IsJoiner(unicodeScalar))
						{
							break;
						}
						flag = !Classification.IsJoiner(unicodeScalar);
					}
					num2 = Math.Min(runLength, num4);
				}
				runLength = num2;
			}
		}
		return textRun;
	}

	internal TextSpan<CultureSpecificCharacterBufferRange> GetPrecedingText(TextSource textSource, int cpLimit)
	{
		if (cpLimit > 0)
		{
			SpanRider spanRider = new SpanRider(_textRunVector, _latestPosition);
			if (spanRider.At(cpLimit - 1))
			{
				CharacterBufferRange characterBufferRange = CharacterBufferRange.Empty;
				CultureInfo culture = null;
				if (spanRider.CurrentElement is TextRun textRun)
				{
					if (TextRunInfo.GetRunType(textRun) == Plsrun.Text && textRun.CharacterBufferReference.CharacterBuffer != null)
					{
						characterBufferRange = new CharacterBufferRange(textRun.CharacterBufferReference, cpLimit - spanRider.CurrentSpanStart);
						culture = CultureMapper.GetSpecificCulture(textRun.Properties.CultureInfo);
					}
					return new TextSpan<CultureSpecificCharacterBufferRange>(cpLimit - spanRider.CurrentSpanStart, new CultureSpecificCharacterBufferRange(culture, characterBufferRange));
				}
			}
		}
		return textSource.GetPrecedingText(cpLimit);
	}

	internal IList<TextSpan<TextRun>> GetTextRunSpans()
	{
		TextSpan<TextRun>[] array = new TextSpan<TextRun>[_textRunVector.Count];
		for (int i = 0; i < array.Length; i++)
		{
			Span span = _textRunVector[i];
			array[i] = new TextSpan<TextRun>(span.length, span.element as TextRun);
		}
		return array;
	}
}
