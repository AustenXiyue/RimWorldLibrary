using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using MS.Internal.PresentationCore;

namespace MS.Internal.TextFormatting;

internal class SimpleTextLine : TextLine
{
	[Flags]
	private enum StatusFlags
	{
		None = 0,
		BoundingBoxComputed = 1,
		HasOverflowed = 2
	}

	private SimpleRun[] _runs;

	private int _cpFirst;

	private int _cpLength;

	private int _cpLengthEOT;

	private double _widthAtTrailing;

	private double _width;

	private double _paragraphWidth;

	private double _height;

	private double _offset;

	private int _idealOffsetUnRounded;

	private double _baselineOffset;

	private int _trailing;

	private Rect _boundingBox;

	private StatusFlags _statusFlags;

	private FormatSettings _settings;

	public override int Length => _cpLength;

	public override int TrailingWhitespaceLength => _trailing;

	public override int DependentLength => 0;

	public override int NewlineLength => _cpLengthEOT;

	public override double Start => _offset;

	public override double Width => _widthAtTrailing;

	public override double WidthIncludingTrailingWhitespace => _width;

	public override double Height => _height;

	public override double TextHeight => _height;

	public override double Extent
	{
		get
		{
			CheckBoundingBox();
			return _boundingBox.Bottom - _boundingBox.Top;
		}
	}

	public override double Baseline => _baselineOffset;

	public override double TextBaseline => _baselineOffset;

	public override double MarkerBaseline => Baseline;

	public override double MarkerHeight => Height;

	public override double OverhangLeading
	{
		get
		{
			CheckBoundingBox();
			return _boundingBox.Left - Start;
		}
	}

	public override double OverhangTrailing
	{
		get
		{
			CheckBoundingBox();
			return Start + Width - _boundingBox.Right;
		}
	}

	public override double OverhangAfter
	{
		get
		{
			CheckBoundingBox();
			return _boundingBox.Bottom - Height;
		}
	}

	public override bool HasOverflowed => (_statusFlags & StatusFlags.HasOverflowed) != 0;

	public override bool HasCollapsed => false;

	public static TextLine Create(FormatSettings settings, int cpFirst, int paragraphWidth, double pixelsPerDip)
	{
		ParaProp pap = settings.Pap;
		if (pap.RightToLeft || pap.Justify || (pap.FirstLineInParagraph && pap.TextMarkerProperties != null) || settings.TextIndent != 0 || pap.ParagraphIndent != 0 || pap.LineHeight > 0 || pap.AlwaysCollapsible || (pap.TextDecorations != null && pap.TextDecorations.Count != 0))
		{
			return null;
		}
		int num = cpFirst;
		int nonHiddenLength = 0;
		int num2 = ((pap.Wrap && paragraphWidth > 0) ? paragraphWidth : int.MaxValue);
		int num3 = 0;
		SimpleRun simpleRun = null;
		SimpleRun simpleRun2 = SimpleRun.Create(settings, num, cpFirst, num2, paragraphWidth, num3, pixelsPerDip);
		if (simpleRun2 == null)
		{
			return null;
		}
		if (!simpleRun2.EOT && simpleRun2.IdealWidth <= num2)
		{
			num += simpleRun2.Length;
			num2 -= simpleRun2.IdealWidth;
			num3 += simpleRun2.IdealWidth;
			simpleRun = simpleRun2;
			simpleRun2 = SimpleRun.Create(settings, num, cpFirst, num2, paragraphWidth, num3, pixelsPerDip);
			if (simpleRun2 == null)
			{
				return null;
			}
		}
		int trailing = 0;
		ArrayList runs = new ArrayList(2);
		if (simpleRun != null)
		{
			AddRun(runs, simpleRun, ref nonHiddenLength);
		}
		while (true)
		{
			if (!simpleRun2.EOT && simpleRun2.IdealWidth > num2)
			{
				return null;
			}
			AddRun(runs, simpleRun2, ref nonHiddenLength);
			if (nonHiddenLength >= 9600)
			{
				return null;
			}
			simpleRun = simpleRun2;
			num += simpleRun2.Length;
			num2 -= simpleRun2.IdealWidth;
			num3 += simpleRun2.IdealWidth;
			if (simpleRun2.EOT)
			{
				break;
			}
			simpleRun2 = SimpleRun.Create(settings, num, cpFirst, num2, paragraphWidth, num3, pixelsPerDip);
			if (simpleRun2 == null || (simpleRun2.Underline != null && simpleRun != null && simpleRun.Underline != null && !simpleRun.IsUnderlineCompatible(simpleRun2)))
			{
				return null;
			}
		}
		int trailingSpaceWidth = 0;
		CollectTrailingSpaces(runs, settings.Formatter, ref trailing, ref trailingSpaceWidth);
		return new SimpleTextLine(settings, cpFirst, paragraphWidth, runs, ref trailing, ref trailingSpaceWidth, pixelsPerDip);
	}

	public SimpleTextLine(FormatSettings settings, int cpFirst, int paragraphWidth, ArrayList runs, ref int trailing, ref int trailingSpaceWidth, double pixelsPerDip)
		: base(pixelsPerDip)
	{
		int i = 0;
		_settings = settings;
		double num = 0.0;
		double num2 = 0.0;
		double num3 = 0.0;
		ParaProp pap = settings.Pap;
		TextFormatterImp formatter = settings.Formatter;
		int num4 = 0;
		for (; i < runs.Count; i++)
		{
			SimpleRun simpleRun = (SimpleRun)runs[i];
			if (simpleRun.Length > 0)
			{
				if (simpleRun.EOT)
				{
					trailing += simpleRun.Length;
					_cpLengthEOT += simpleRun.Length;
				}
				else
				{
					num3 = Math.Max(num3, simpleRun.Height);
					num = Math.Max(num, simpleRun.Baseline);
					num2 = Math.Max(num2, simpleRun.Height - simpleRun.Baseline);
				}
				_cpLength += simpleRun.Length;
				num4 += simpleRun.IdealWidth;
			}
		}
		_baselineOffset = formatter.IdealToReal(TextFormatterImp.RealToIdeal(num), base.PixelsPerDip);
		if (num + num2 == num3)
		{
			_height = formatter.IdealToReal(TextFormatterImp.RealToIdeal(num3), base.PixelsPerDip);
		}
		else
		{
			_height = formatter.IdealToReal(TextFormatterImp.RealToIdeal(num) + TextFormatterImp.RealToIdeal(num2), base.PixelsPerDip);
		}
		if (_height <= 0.0)
		{
			_height = formatter.IdealToReal((int)Math.Round(pap.DefaultTypeface.LineSpacing(pap.EmSize, 1.0 / 300.0, base.PixelsPerDip, _settings.TextFormattingMode)), base.PixelsPerDip);
			_baselineOffset = formatter.IdealToReal((int)Math.Round(pap.DefaultTypeface.Baseline(pap.EmSize, 1.0 / 300.0, base.PixelsPerDip, _settings.TextFormattingMode)), base.PixelsPerDip);
		}
		_runs = new SimpleRun[i];
		int num5 = i - 1;
		int num6 = trailing;
		while (num5 >= 0)
		{
			SimpleRun simpleRun2 = (SimpleRun)runs[num5];
			if (num6 > 0)
			{
				simpleRun2.TrimTrailingUnderline = true;
				num6 -= simpleRun2.Length;
			}
			_runs[num5] = simpleRun2;
			num5--;
		}
		_cpFirst = cpFirst;
		_trailing = trailing;
		int num7 = num4 - trailingSpaceWidth;
		if (pap.Align != 0)
		{
			switch (pap.Align)
			{
			case TextAlignment.Right:
				_idealOffsetUnRounded = paragraphWidth - num7;
				_offset = formatter.IdealToReal(_idealOffsetUnRounded, base.PixelsPerDip);
				break;
			case TextAlignment.Center:
				_idealOffsetUnRounded = (int)Math.Round((double)(paragraphWidth - num7) * 0.5);
				_offset = formatter.IdealToReal(_idealOffsetUnRounded, base.PixelsPerDip);
				break;
			}
		}
		_width = formatter.IdealToReal(num4, base.PixelsPerDip);
		_widthAtTrailing = formatter.IdealToReal(num7, base.PixelsPerDip);
		_paragraphWidth = formatter.IdealToReal(paragraphWidth, base.PixelsPerDip);
		if (paragraphWidth > 0 && _widthAtTrailing > _paragraphWidth)
		{
			_statusFlags |= StatusFlags.HasOverflowed;
		}
	}

	public override void Dispose()
	{
	}

	private static void CollectTrailingSpaces(ArrayList runs, TextFormatterImp formatter, ref int trailing, ref int trailingSpaceWidth)
	{
		int num = runs?.Count ?? 0;
		bool flag = true;
		while (num > 0 && flag)
		{
			flag = ((SimpleRun)runs[--num]).CollectTrailingSpaces(formatter, ref trailing, ref trailingSpaceWidth);
		}
	}

	private static void AddRun(ArrayList runs, SimpleRun run, ref int nonHiddenLength)
	{
		if (run.Length > 0)
		{
			runs.Add(run);
			if (!run.Ghost)
			{
				nonHiddenLength += run.Length;
			}
		}
	}

	private double DistanceFromCp(int currentIndex)
	{
		Invariant.Assert(currentIndex >= _cpFirst);
		int num = 0;
		int num2 = currentIndex - _cpFirst;
		SimpleRun[] runs = _runs;
		foreach (SimpleRun simpleRun in runs)
		{
			num += simpleRun.DistanceFromDcp(num2);
			if (num2 <= simpleRun.Length)
			{
				break;
			}
			num2 -= simpleRun.Length;
		}
		return _settings.Formatter.IdealToReal(num + _idealOffsetUnRounded, base.PixelsPerDip);
	}

	public override void Draw(DrawingContext drawingContext, Point origin, InvertAxes inversion)
	{
		if (drawingContext == null)
		{
			throw new ArgumentNullException("drawingContext");
		}
		MatrixTransform matrixTransform = TextFormatterImp.CreateAntiInversionTransform(inversion, _paragraphWidth, _height);
		if (matrixTransform == null)
		{
			DrawTextLine(drawingContext, origin);
			return;
		}
		drawingContext.PushTransform(matrixTransform);
		try
		{
			DrawTextLine(drawingContext, origin);
		}
		finally
		{
			drawingContext.Pop();
		}
	}

	public override TextLine Collapse(params TextCollapsingProperties[] collapsingPropertiesList)
	{
		if (!HasOverflowed)
		{
			return this;
		}
		Invariant.Assert(_settings != null);
		TextMetrics.FullTextLine fullTextLine = new TextMetrics.FullTextLine(_settings, _cpFirst, 0, TextFormatterImp.RealToIdeal(_paragraphWidth), LineFlags.None);
		if (fullTextLine.HasOverflowed)
		{
			TextLine textLine = fullTextLine.Collapse(collapsingPropertiesList);
			if (textLine != fullTextLine)
			{
				fullTextLine.Dispose();
			}
			return textLine;
		}
		return fullTextLine;
	}

	private void CheckBoundingBox()
	{
		if ((_statusFlags & StatusFlags.BoundingBoxComputed) == 0)
		{
			DrawTextLine(null, new Point(0.0, 0.0));
		}
	}

	private void DrawTextLine(DrawingContext drawingContext, Point origin)
	{
		if (_runs.Length == 0)
		{
			_boundingBox = Rect.Empty;
			_statusFlags |= StatusFlags.BoundingBoxComputed;
			return;
		}
		int num = _idealOffsetUnRounded;
		double num2 = origin.Y + Baseline;
		drawingContext?.PushGuidelineY1(num2);
		Rect boundingBox = Rect.Empty;
		try
		{
			SimpleRun[] runs = _runs;
			foreach (SimpleRun simpleRun in runs)
			{
				boundingBox.Union(simpleRun.Draw(drawingContext, _settings.Formatter.IdealToReal(num, base.PixelsPerDip) + origin.X, num2, visiCodePath: false));
				num += simpleRun.IdealWidth;
			}
		}
		finally
		{
			drawingContext?.Pop();
		}
		if (boundingBox.IsEmpty)
		{
			boundingBox = new Rect(Start, 0.0, 0.0, 0.0);
		}
		else
		{
			boundingBox.X -= origin.X;
			boundingBox.Y -= origin.Y;
		}
		_boundingBox = boundingBox;
		_statusFlags |= StatusFlags.BoundingBoxComputed;
	}

	public override CharacterHit GetCharacterHitFromDistance(double distance)
	{
		int num = TextFormatterImp.RealToIdeal(distance) - _idealOffsetUnRounded;
		int num2 = _cpFirst;
		if (num < 0)
		{
			return new CharacterHit(_cpFirst, 0);
		}
		SimpleRun simpleRun = null;
		CharacterHit characterHit = default(CharacterHit);
		for (int i = 0; i < _runs.Length; i++)
		{
			simpleRun = _runs[i];
			if (!simpleRun.EOT)
			{
				num2 += characterHit.TrailingLength;
				characterHit = simpleRun.DcpFromDistance(num);
				num2 += characterHit.FirstCharacterIndex;
			}
			if (num <= simpleRun.IdealWidth)
			{
				break;
			}
			num -= simpleRun.IdealWidth;
		}
		return new CharacterHit(num2, characterHit.TrailingLength);
	}

	public override double GetDistanceFromCharacterHit(CharacterHit characterHit)
	{
		TextFormatterImp.VerifyCaretCharacterHit(characterHit, _cpFirst, _cpLength);
		return DistanceFromCp(characterHit.FirstCharacterIndex + ((characterHit.TrailingLength != 0) ? 1 : 0));
	}

	public override CharacterHit GetNextCaretCharacterHit(CharacterHit characterHit)
	{
		TextFormatterImp.VerifyCaretCharacterHit(characterHit, _cpFirst, _cpLength);
		if (characterHit.TrailingLength == 0 && FindNextVisibleCp(characterHit.FirstCharacterIndex, out var cpVisible))
		{
			return new CharacterHit(cpVisible, 1);
		}
		if (FindNextVisibleCp(characterHit.FirstCharacterIndex + 1, out cpVisible))
		{
			return new CharacterHit(cpVisible, 1);
		}
		return characterHit;
	}

	public override CharacterHit GetPreviousCaretCharacterHit(CharacterHit characterHit)
	{
		TextFormatterImp.VerifyCaretCharacterHit(characterHit, _cpFirst, _cpLength);
		int num = characterHit.FirstCharacterIndex;
		bool flag = characterHit.TrailingLength != 0;
		if (num >= _cpFirst + _cpLength)
		{
			num = _cpFirst + _cpLength - 1;
			flag = true;
		}
		if (flag && FindPreviousVisibleCp(num, out var cpVisible))
		{
			return new CharacterHit(cpVisible, 0);
		}
		if (FindPreviousVisibleCp(num - 1, out cpVisible))
		{
			return new CharacterHit(cpVisible, 0);
		}
		return characterHit;
	}

	public override CharacterHit GetBackspaceCaretCharacterHit(CharacterHit characterHit)
	{
		return GetPreviousCaretCharacterHit(characterHit);
	}

	public override IList<TextBounds> GetTextBounds(int firstTextSourceCharacterIndex, int textLength)
	{
		if (textLength == 0)
		{
			throw new ArgumentOutOfRangeException("textLength", SR.ParameterMustBeGreaterThanZero);
		}
		if (textLength < 0)
		{
			firstTextSourceCharacterIndex += textLength;
			textLength = -textLength;
		}
		if (firstTextSourceCharacterIndex < _cpFirst)
		{
			textLength += firstTextSourceCharacterIndex - _cpFirst;
			firstTextSourceCharacterIndex = _cpFirst;
		}
		if (firstTextSourceCharacterIndex + textLength > _cpFirst + _cpLength)
		{
			textLength = _cpFirst + _cpLength - firstTextSourceCharacterIndex;
		}
		double distanceFromCharacterHit = GetDistanceFromCharacterHit(new CharacterHit(firstTextSourceCharacterIndex, 0));
		double distanceFromCharacterHit2 = GetDistanceFromCharacterHit(new CharacterHit(firstTextSourceCharacterIndex + textLength, 0));
		IList<TextRunBounds> list = null;
		int num = firstTextSourceCharacterIndex - _cpFirst;
		int num2 = 0;
		list = new List<TextRunBounds>(2);
		SimpleRun[] runs = _runs;
		foreach (SimpleRun simpleRun in runs)
		{
			if (!simpleRun.EOT && !simpleRun.Ghost && num2 + simpleRun.Length > num)
			{
				if (num2 >= num + textLength)
				{
					break;
				}
				int num3 = Math.Max(num2, num) + _cpFirst;
				int num4 = Math.Min(num2 + simpleRun.Length, num + textLength) + _cpFirst;
				list.Add(new TextRunBounds(new Rect(new Point(DistanceFromCp(num3), _baselineOffset - simpleRun.Baseline), new Point(DistanceFromCp(num4), _baselineOffset - simpleRun.Baseline + simpleRun.Height)), num3, num4, simpleRun.TextRun));
			}
			num2 += simpleRun.Length;
		}
		return new TextBounds[1]
		{
			new TextBounds(new Rect(distanceFromCharacterHit, 0.0, distanceFromCharacterHit2 - distanceFromCharacterHit, _height), FlowDirection.LeftToRight, (list == null || list.Count == 0) ? null : list)
		};
	}

	public override IList<TextSpan<TextRun>> GetTextRunSpans()
	{
		TextSpan<TextRun>[] array = new TextSpan<TextRun>[_runs.Length];
		for (int i = 0; i < _runs.Length; i++)
		{
			array[i] = new TextSpan<TextRun>(_runs[i].Length, _runs[i].TextRun);
		}
		return array;
	}

	public override IEnumerable<IndexedGlyphRun> GetIndexedGlyphRuns()
	{
		List<IndexedGlyphRun> list = new List<IndexedGlyphRun>(_runs.Length);
		Point origin = new Point(0.0, 0.0);
		int num = _cpFirst;
		SimpleRun[] runs = _runs;
		foreach (SimpleRun simpleRun in runs)
		{
			if (simpleRun.Length > 0 && !simpleRun.Ghost)
			{
				IList<double> list2;
				if (_settings.TextFormattingMode == TextFormattingMode.Ideal)
				{
					list2 = new ThousandthOfEmRealDoubles(simpleRun.EmSize, simpleRun.NominalAdvances.Length);
					for (int j = 0; j < list2.Count; j++)
					{
						list2[j] = _settings.Formatter.IdealToReal(simpleRun.NominalAdvances[j], base.PixelsPerDip);
					}
				}
				else
				{
					list2 = new List<double>(simpleRun.NominalAdvances.Length);
					for (int k = 0; k < simpleRun.NominalAdvances.Length; k++)
					{
						list2.Add(_settings.Formatter.IdealToReal(simpleRun.NominalAdvances[k], base.PixelsPerDip));
					}
				}
				GlyphTypeface glyphTypeface = simpleRun.Typeface.TryGetGlyphTypeface();
				Invariant.Assert(glyphTypeface != null);
				GlyphRun glyphRun = glyphTypeface.ComputeUnshapedGlyphRun(origin, new CharacterBufferRange(simpleRun.CharBufferReference, simpleRun.Length), list2, simpleRun.EmSize, (float)base.PixelsPerDip, simpleRun.TextRun.Properties.FontHintingEmSize, simpleRun.Typeface.NullFont, CultureMapper.GetSpecificCulture(simpleRun.TextRun.Properties.CultureInfo), null, _settings.TextFormattingMode);
				if (glyphRun != null)
				{
					list.Add(new IndexedGlyphRun(num, simpleRun.Length, glyphRun));
				}
			}
			num += simpleRun.Length;
		}
		return list;
	}

	public override TextLineBreak GetTextLineBreak()
	{
		return null;
	}

	public override IList<TextCollapsedRange> GetTextCollapsedRanges()
	{
		Invariant.Assert(!HasCollapsed);
		return null;
	}

	private bool FindNextVisibleCp(int cp, out int cpVisible)
	{
		cpVisible = cp;
		if (cp >= _cpFirst + _cpLength)
		{
			return false;
		}
		GetRunIndexAtCp(cp, out var runIndex, out var cpRunStart);
		while (runIndex < _runs.Length)
		{
			if (_runs[runIndex].IsVisible && !_runs[runIndex].EOT)
			{
				cpVisible = Math.Max(cpRunStart, cp);
				return true;
			}
			cpRunStart += _runs[runIndex++].Length;
		}
		return false;
	}

	private bool FindPreviousVisibleCp(int cp, out int cpVisible)
	{
		cpVisible = cp;
		if (cp < _cpFirst)
		{
			return false;
		}
		GetRunIndexAtCp(cp, out var runIndex, out var cpRunStart);
		cpRunStart += _runs[runIndex].Length - 1;
		while (runIndex >= 0)
		{
			if (_runs[runIndex].IsVisible && !_runs[runIndex].EOT)
			{
				cpVisible = Math.Min(cpRunStart, cp);
				return true;
			}
			if (_runs[runIndex].EOT)
			{
				cpVisible = cpRunStart - _runs[runIndex].Length + 1;
				return true;
			}
			cpRunStart -= _runs[runIndex--].Length;
		}
		return false;
	}

	private void GetRunIndexAtCp(int cp, out int runIndex, out int cpRunStart)
	{
		Invariant.Assert(cp >= _cpFirst && cp < _cpFirst + _cpLength);
		cpRunStart = _cpFirst;
		runIndex = 0;
		while (runIndex < _runs.Length && cpRunStart + _runs[runIndex].Length <= cp)
		{
			cpRunStart += _runs[runIndex++].Length;
		}
	}
}
