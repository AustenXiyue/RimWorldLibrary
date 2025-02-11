using System.Collections.Generic;
using System.Windows.Markup;
using MS.Internal;
using MS.Internal.FontCache;
using MS.Internal.Shaping;
using MS.Internal.Text.TextInterface;
using MS.Internal.TextFormatting;

namespace System.Windows.Media.TextFormatting;

internal sealed class TextShapeableCharacters : TextShapeableSymbols
{
	private CharacterBufferRange _characterBufferRange;

	private TextFormattingMode _textFormattingMode;

	private bool _isSideways;

	private TextRunProperties _properties;

	private double _emSize;

	private ItemProps _textItem;

	private ShapeTypeface _shapeTypeface;

	private bool _nullShape;

	internal const ushort DefaultMaxClusterSize = 8;

	private const ushort IndicMaxClusterSize = 15;

	public sealed override CharacterBufferReference CharacterBufferReference => _characterBufferRange.CharacterBufferReference;

	public sealed override int Length => _characterBufferRange.Length;

	public sealed override TextRunProperties Properties => _properties;

	internal override double EmSize => _emSize;

	internal override ItemProps ItemProps => _textItem;

	internal sealed override bool NeedsMaxClusterSize
	{
		get
		{
			if (!_textItem.IsLatin || _textItem.HasCombiningMark || _textItem.HasExtendedCharacter)
			{
				return true;
			}
			return false;
		}
	}

	internal sealed override bool IsShapingRequired
	{
		get
		{
			if (_shapeTypeface != null && (_shapeTypeface.DeviceFont == null || _textItem.DigitCulture != null))
			{
				return !IsSymbol;
			}
			return false;
		}
	}

	internal sealed override bool NeedsCaretInfo
	{
		get
		{
			if (!_textItem.HasCombiningMark)
			{
				return _textItem.NeedsCaretInfo;
			}
			return true;
		}
	}

	internal sealed override bool HasExtendedCharacter => _textItem.HasExtendedCharacter;

	internal sealed override double Height => _properties.Typeface.LineSpacing(_properties.FontRenderingEmSize, 1.0, _properties.PixelsPerDip, _textFormattingMode);

	internal sealed override double Baseline => _properties.Typeface.Baseline(_properties.FontRenderingEmSize, 1.0, _properties.PixelsPerDip, _textFormattingMode);

	internal sealed override double UnderlinePosition => _properties.Typeface.UnderlinePosition;

	internal sealed override double UnderlineThickness => _properties.Typeface.UnderlineThickness;

	internal sealed override double StrikethroughPosition => _properties.Typeface.StrikethroughPosition;

	internal sealed override double StrikethroughThickness => _properties.Typeface.StrikethroughThickness;

	internal bool IsSymbol
	{
		get
		{
			if (_shapeTypeface != null)
			{
				return _shapeTypeface.GlyphTypeface.Symbol;
			}
			return _properties.Typeface.Symbol;
		}
	}

	internal override GlyphTypeface GlyphTypeFace
	{
		get
		{
			if (_shapeTypeface != null)
			{
				return _shapeTypeface.GlyphTypeface;
			}
			return _properties.Typeface.TryGetGlyphTypeface();
		}
	}

	internal sealed override ushort MaxClusterSize
	{
		get
		{
			if (_textItem.IsIndic)
			{
				return 15;
			}
			return 8;
		}
	}

	internal TextShapeableCharacters(CharacterBufferRange characterRange, TextRunProperties properties, double emSize, ItemProps textItem, ShapeTypeface shapeTypeface, bool nullShape, TextFormattingMode textFormattingMode, bool isSideways)
	{
		_isSideways = isSideways;
		_textFormattingMode = textFormattingMode;
		_characterBufferRange = characterRange;
		_properties = properties;
		_emSize = emSize;
		_textItem = textItem;
		_shapeTypeface = shapeTypeface;
		_nullShape = nullShape;
	}

	internal sealed override GlyphRun ComputeShapedGlyphRun(Point origin, char[] characterString, ushort[] clusterMap, ushort[] glyphIndices, IList<double> glyphAdvances, IList<Point> glyphOffsets, bool rightToLeft, bool sideways)
	{
		Invariant.Assert(_shapeTypeface != null);
		Invariant.Assert(glyphIndices != null);
		Invariant.Assert(_shapeTypeface.DeviceFont == null || _textItem.DigitCulture != null);
		bool[] array = null;
		if (clusterMap != null && (HasExtendedCharacter || NeedsCaretInfo))
		{
			array = new bool[clusterMap.Length + 1];
			array[0] = true;
			array[clusterMap.Length] = true;
			ushort num = clusterMap[0];
			for (int i = 1; i < clusterMap.Length; i++)
			{
				ushort num2 = clusterMap[i];
				if (num2 != num)
				{
					array[i] = true;
					num = num2;
				}
			}
		}
		return GlyphRun.TryCreate(_shapeTypeface.GlyphTypeface, rightToLeft ? 1 : 0, sideways, _emSize, (float)_properties.PixelsPerDip, glyphIndices, origin, glyphAdvances, glyphOffsets, characterString, null, clusterMap, array, XmlLanguage.GetLanguage(CultureMapper.GetSpecificCulture(_properties.CultureInfo).IetfLanguageTag), _textFormattingMode);
	}

	private GlyphTypeface GetGlyphTypeface(out bool nullFont)
	{
		GlyphTypeface glyphTypeface;
		if (_shapeTypeface == null)
		{
			Typeface typeface = _properties.Typeface;
			glyphTypeface = typeface.TryGetGlyphTypeface();
			nullFont = typeface.NullFont;
		}
		else
		{
			glyphTypeface = _shapeTypeface.GlyphTypeface;
			nullFont = _nullShape;
		}
		Invariant.Assert(glyphTypeface != null);
		return glyphTypeface;
	}

	internal sealed override GlyphRun ComputeUnshapedGlyphRun(Point origin, char[] characterString, IList<double> characterAdvances)
	{
		bool nullFont;
		GlyphTypeface glyphTypeface = GetGlyphTypeface(out nullFont);
		Invariant.Assert(glyphTypeface != null);
		return glyphTypeface.ComputeUnshapedGlyphRun(origin, new CharacterBufferRange(characterString, 0, characterString.Length), characterAdvances, _emSize, (float)_properties.PixelsPerDip, _properties.FontHintingEmSize, nullFont, CultureMapper.GetSpecificCulture(_properties.CultureInfo), (_shapeTypeface == null || _shapeTypeface.DeviceFont == null) ? null : _shapeTypeface.DeviceFont.Name, _textFormattingMode);
	}

	internal sealed override void Draw(DrawingContext drawingContext, Brush foregroundBrush, GlyphRun glyphRun)
	{
		if (drawingContext == null)
		{
			throw new ArgumentNullException("drawingContext");
		}
		glyphRun.EmitBackground(drawingContext, _properties.BackgroundBrush);
		drawingContext.DrawGlyphRun((foregroundBrush != null) ? foregroundBrush : _properties.ForegroundBrush, glyphRun);
	}

	internal unsafe sealed override void GetAdvanceWidthsUnshaped(char* characterString, int characterLength, double scalingFactor, int* advanceWidthsUnshaped)
	{
		if (!IsShapingRequired)
		{
			if (_shapeTypeface != null && _shapeTypeface.DeviceFont != null)
			{
				_shapeTypeface.DeviceFont.GetAdvanceWidths(characterString, characterLength, _emSize * scalingFactor, advanceWidthsUnshaped);
				return;
			}
			bool nullFont;
			GlyphTypeface glyphTypeface = GetGlyphTypeface(out nullFont);
			Invariant.Assert(glyphTypeface != null);
			glyphTypeface.GetAdvanceWidthsUnshaped(characterString, characterLength, _emSize, (float)_properties.PixelsPerDip, scalingFactor, advanceWidthsUnshaped, nullFont, _textFormattingMode, _isSideways);
			return;
		}
		GlyphTypeface glyphTypeface2 = _shapeTypeface.GlyphTypeface;
		Invariant.Assert(glyphTypeface2 != null);
		Invariant.Assert(characterLength > 0);
		CharacterBufferRange characters = new CharacterBufferRange(characterString, characterLength);
		GlyphMetrics[] glyphMetrics = BufferCache.GetGlyphMetrics(characterLength);
		glyphTypeface2.GetGlyphMetricsOptimized(characters, _emSize, (float)_properties.PixelsPerDip, _textFormattingMode, _isSideways, glyphMetrics);
		if (_textFormattingMode == TextFormattingMode.Display && TextFormatterContext.IsSpecialCharacter(*characterString))
		{
			double num = _emSize / (double)(int)glyphTypeface2.DesignEmHeight;
			double pixelsPerDip = _properties.PixelsPerDip;
			for (int i = 0; i < characterLength; i++)
			{
				advanceWidthsUnshaped[i] = (int)Math.Round(TextFormatterImp.RoundDipForDisplayMode((double)glyphMetrics[i].AdvanceWidth * num, pixelsPerDip) * scalingFactor);
			}
		}
		else
		{
			double num2 = _emSize * scalingFactor / (double)(int)glyphTypeface2.DesignEmHeight;
			for (int j = 0; j < characterLength; j++)
			{
				advanceWidthsUnshaped[j] = (int)Math.Round((double)glyphMetrics[j].AdvanceWidth * num2);
			}
		}
		BufferCache.ReleaseGlyphMetrics(glyphMetrics);
	}

	internal sealed override bool CanShapeTogether(TextShapeableSymbols shapeable)
	{
		if (!(shapeable is TextShapeableCharacters textShapeableCharacters))
		{
			return false;
		}
		if (_shapeTypeface.Equals(textShapeableCharacters._shapeTypeface) && _textItem.HasExtendedCharacter == textShapeableCharacters._textItem.HasExtendedCharacter && _emSize == textShapeableCharacters._emSize && ((_properties.CultureInfo == null) ? (textShapeableCharacters._properties.CultureInfo == null) : _properties.CultureInfo.Equals(textShapeableCharacters._properties.CultureInfo)) && _nullShape == textShapeableCharacters._nullShape)
		{
			return _textItem.CanShapeTogether(textShapeableCharacters._textItem);
		}
		return false;
	}
}
