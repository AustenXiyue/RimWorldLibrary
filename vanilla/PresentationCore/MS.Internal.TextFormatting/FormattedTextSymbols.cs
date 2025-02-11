using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using MS.Internal.FontCache;
using MS.Internal.Shaping;
using MS.Internal.Text.TextInterface;

namespace MS.Internal.TextFormatting;

internal sealed class FormattedTextSymbols
{
	private sealed class Glyphs
	{
		private TextShapeableSymbols _shapeable;

		private char[] _charArray;

		private ushort[] _clusterMap;

		private ushort[] _glyphIndices;

		private double[] _glyphAdvances;

		private IList<Point> _glyphOffsets;

		private double _width;

		public double Width => _width;

		public Brush ForegroundBrush => _shapeable.Properties.ForegroundBrush;

		public Brush BackgroundBrush => _shapeable.Properties.BackgroundBrush;

		internal Glyphs(TextShapeableSymbols shapeable, char[] charArray, int[] nominalAdvances, double scalingFactor)
			: this(shapeable, charArray, nominalAdvances, null, null, null, scalingFactor)
		{
		}

		internal Glyphs(TextShapeableSymbols shapeable, char[] charArray, int[] glyphAdvances, ushort[] clusterMap, ushort[] glyphIndices, GlyphOffset[] glyphOffsets, double scalingFactor)
		{
			_shapeable = shapeable;
			_charArray = charArray;
			_glyphAdvances = new double[glyphAdvances.Length];
			double num = 1.0 / scalingFactor;
			for (int i = 0; i < glyphAdvances.Length; i++)
			{
				_glyphAdvances[i] = (double)glyphAdvances[i] * num;
				_width += _glyphAdvances[i];
			}
			if (glyphIndices == null)
			{
				return;
			}
			_clusterMap = clusterMap;
			if (glyphOffsets != null)
			{
				_glyphOffsets = new PartialArray<Point>(new Point[glyphOffsets.Length]);
				for (int j = 0; j < glyphOffsets.Length; j++)
				{
					_glyphOffsets[j] = new Point((double)glyphOffsets[j].du * num, (double)glyphOffsets[j].dv * num);
				}
			}
			if (glyphAdvances.Length != glyphIndices.Length)
			{
				_glyphIndices = new ushort[glyphAdvances.Length];
				for (int k = 0; k < glyphAdvances.Length; k++)
				{
					_glyphIndices[k] = glyphIndices[k];
				}
			}
			else
			{
				_glyphIndices = glyphIndices;
			}
		}

		internal GlyphRun CreateGlyphRun(Point currentOrigin, bool rightToLeft)
		{
			if (!_shapeable.IsShapingRequired)
			{
				return _shapeable.ComputeUnshapedGlyphRun(currentOrigin, _charArray, _glyphAdvances);
			}
			return _shapeable.ComputeShapedGlyphRun(currentOrigin, _charArray, _clusterMap, _glyphIndices, _glyphAdvances, _glyphOffsets, rightToLeft, sideways: false);
		}
	}

	private Glyphs[] _glyphs;

	private bool _rightToLeft;

	private TextFormattingMode _textFormattingMode;

	private bool _isSideways;

	public double Width
	{
		get
		{
			double num = 0.0;
			Glyphs[] glyphs = _glyphs;
			foreach (Glyphs glyphs2 in glyphs)
			{
				num += glyphs2.Width;
			}
			return num;
		}
	}

	public unsafe FormattedTextSymbols(GlyphingCache glyphingCache, TextRun textSymbols, bool rightToLeft, double scalingFactor, float pixelsPerDip, TextFormattingMode textFormattingMode, bool isSideways)
	{
		_textFormattingMode = textFormattingMode;
		_isSideways = isSideways;
		IList<TextShapeableSymbols> textShapeableSymbols = (textSymbols as ITextSymbols).GetTextShapeableSymbols(glyphingCache, textSymbols.CharacterBufferReference, textSymbols.Length, rightToLeft, rightToLeft, null, null, _textFormattingMode, _isSideways);
		_rightToLeft = rightToLeft;
		_glyphs = new Glyphs[textShapeableSymbols.Count];
		CharacterBuffer characterBuffer = textSymbols.CharacterBufferReference.CharacterBuffer;
		int offsetToFirstChar = textSymbols.CharacterBufferReference.OffsetToFirstChar;
		int num = 0;
		int num2 = 0;
		while (num < textShapeableSymbols.Count)
		{
			TextShapeableSymbols textShapeableSymbols2 = textShapeableSymbols[num];
			int length = textShapeableSymbols2.Length;
			char[] array = new char[length];
			for (int i = 0; i < length; i++)
			{
				array[i] = characterBuffer[offsetToFirstChar + num2 + i];
			}
			if (textShapeableSymbols2.IsShapingRequired)
			{
				ushort[] clusterMap;
				ushort[] glyphIndices;
				int[] glyphAdvances;
				GlyphOffset[] glyphOffsets;
				fixed (char* textString = &array[0])
				{
					TextAnalyzer textAnalyzer = DWriteFactory.Instance.CreateTextAnalyzer();
					GlyphTypeface glyphTypeFace = textShapeableSymbols2.GlyphTypeFace;
					uint num3 = checked((uint)length);
					LSRun.CompileFeatureSet(textShapeableSymbols2.Properties.TypographyProperties, num3, out var fontFeatures, out var fontFeatureRanges);
					textAnalyzer.GetGlyphsAndTheirPlacements(textString, num3, glyphTypeFace.FontDWrite, glyphTypeFace.BlankGlyphIndex, isSideways: false, rightToLeft, textShapeableSymbols2.Properties.CultureInfo, fontFeatures, fontFeatureRanges, textShapeableSymbols2.Properties.FontRenderingEmSize, scalingFactor, pixelsPerDip, _textFormattingMode, textShapeableSymbols2.ItemProps, out clusterMap, out glyphIndices, out glyphAdvances, out glyphOffsets);
				}
				_glyphs[num] = new Glyphs(textShapeableSymbols2, array, glyphAdvances, clusterMap, glyphIndices, glyphOffsets, scalingFactor);
			}
			else
			{
				int[] array2 = new int[array.Length];
				fixed (char* characterString = &array[0])
				{
					fixed (int* advanceWidthsUnshaped = &array2[0])
					{
						textShapeableSymbols2.GetAdvanceWidthsUnshaped(characterString, length, scalingFactor, advanceWidthsUnshaped);
					}
				}
				_glyphs[num] = new Glyphs(textShapeableSymbols2, array, array2, scalingFactor);
			}
			num++;
			num2 += length;
		}
	}

	public Rect Draw(DrawingContext drawingContext, Point currentOrigin)
	{
		Rect empty = Rect.Empty;
		Glyphs[] glyphs = _glyphs;
		foreach (Glyphs glyphs2 in glyphs)
		{
			GlyphRun glyphRun = glyphs2.CreateGlyphRun(currentOrigin, _rightToLeft);
			Rect rect;
			if (glyphRun != null)
			{
				rect = glyphRun.ComputeInkBoundingBox();
				if (drawingContext != null)
				{
					glyphRun.EmitBackground(drawingContext, glyphs2.BackgroundBrush);
					drawingContext.PushGuidelineY1(currentOrigin.Y);
					try
					{
						drawingContext.DrawGlyphRun(glyphs2.ForegroundBrush, glyphRun);
					}
					finally
					{
						drawingContext.Pop();
					}
				}
			}
			else
			{
				rect = Rect.Empty;
			}
			if (!rect.IsEmpty)
			{
				rect.X += glyphRun.BaselineOrigin.X;
				rect.Y += glyphRun.BaselineOrigin.Y;
			}
			empty.Union(rect);
			if (_rightToLeft)
			{
				currentOrigin.X -= glyphs2.Width;
			}
			else
			{
				currentOrigin.X += glyphs2.Width;
			}
		}
		return empty;
	}
}
