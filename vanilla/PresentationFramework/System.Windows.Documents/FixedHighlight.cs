using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using System.Windows.Shapes;

namespace System.Windows.Documents;

internal sealed class FixedHighlight
{
	private readonly UIElement _element;

	private readonly int _gBeginOffset;

	private readonly int _gEndOffset;

	private readonly FixedHighlightType _type;

	private readonly Brush _backgroundBrush;

	private readonly Brush _foregroundBrush;

	internal FixedHighlightType HighlightType => _type;

	internal Glyphs Glyphs => _element as Glyphs;

	internal UIElement Element => _element;

	internal Brush ForegroundBrush => _foregroundBrush;

	internal Brush BackgroundBrush => _backgroundBrush;

	internal FixedHighlight(UIElement element, int beginOffset, int endOffset, FixedHighlightType t, Brush foreground, Brush background)
	{
		_element = element;
		_gBeginOffset = beginOffset;
		_gEndOffset = endOffset;
		_type = t;
		_foregroundBrush = foreground;
		_backgroundBrush = background;
	}

	public override bool Equals(object oCompare)
	{
		if (!(oCompare is FixedHighlight fixedHighlight))
		{
			return false;
		}
		if (fixedHighlight._element == _element && fixedHighlight._gBeginOffset == _gBeginOffset && fixedHighlight._gEndOffset == _gEndOffset)
		{
			return fixedHighlight._type == _type;
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (_element != null)
		{
			return (int)(_element.GetHashCode() + _gBeginOffset + _gEndOffset + _type);
		}
		return 0;
	}

	internal Rect ComputeDesignRect()
	{
		if (!(_element is Glyphs { MeasurementGlyphRun: var measurementGlyphRun } glyphs))
		{
			if (_element is Image { Source: not null } image)
			{
				return new Rect(0.0, 0.0, image.Width, image.Height);
			}
			if (_element is Path path)
			{
				return path.Data.Bounds;
			}
			return Rect.Empty;
		}
		if (measurementGlyphRun == null || _gBeginOffset >= _gEndOffset)
		{
			return Rect.Empty;
		}
		Rect result = measurementGlyphRun.ComputeAlignmentBox();
		result.Offset(glyphs.OriginX, glyphs.OriginY);
		int num = ((measurementGlyphRun.Characters != null) ? measurementGlyphRun.Characters.Count : 0);
		double num2 = measurementGlyphRun.GetDistanceFromCaretCharacterHit(new CharacterHit(_gBeginOffset, 0));
		double num3 = ((_gEndOffset != num) ? measurementGlyphRun.GetDistanceFromCaretCharacterHit(new CharacterHit(_gEndOffset, 0)) : measurementGlyphRun.GetDistanceFromCaretCharacterHit(new CharacterHit(num - 1, 1)));
		if (num3 < num2)
		{
			double num4 = num2;
			num2 = num3;
			num3 = num4;
		}
		double width = num3 - num2;
		if ((measurementGlyphRun.BidiLevel & 1) != 0)
		{
			result.X = glyphs.OriginX - num3;
		}
		else
		{
			result.X = glyphs.OriginX + num2;
		}
		result.Width = width;
		return result;
	}
}
