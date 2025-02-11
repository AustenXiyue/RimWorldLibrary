using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace MS.Internal.Text;

internal sealed class InlineObject : TextEmbeddedObject
{
	private readonly int _dcp;

	private readonly int _cch;

	private readonly UIElement _element;

	private readonly TextRunProperties _textProps;

	private readonly TextBlock _host;

	public override CharacterBufferReference CharacterBufferReference => new CharacterBufferReference(string.Empty, 0);

	public override int Length => _cch;

	public override TextRunProperties Properties => _textProps;

	public override LineBreakCondition BreakBefore => LineBreakCondition.BreakDesired;

	public override LineBreakCondition BreakAfter => LineBreakCondition.BreakDesired;

	public override bool HasFixedSize => true;

	internal int Dcp => _dcp;

	internal UIElement Element => _element;

	internal InlineObject(int dcp, int cch, UIElement element, TextRunProperties textProps, TextBlock host)
	{
		_dcp = dcp;
		_cch = cch;
		_element = element;
		_textProps = textProps;
		_host = host;
	}

	public override TextEmbeddedObjectMetrics Format(double remainingParagraphWidth)
	{
		Size size = _host.MeasureChild(this);
		TextDpi.EnsureValidObjSize(ref size);
		double baseline = size.Height;
		double num = (double)Element.GetValue(TextBlock.BaselineOffsetProperty);
		if (!double.IsNaN(num))
		{
			baseline = num;
		}
		return new TextEmbeddedObjectMetrics(size.Width, size.Height, baseline);
	}

	public override Rect ComputeBoundingBox(bool rightToLeft, bool sideways)
	{
		if (_element.IsArrangeValid)
		{
			Size desiredSize = _element.DesiredSize;
			double num = ((!sideways) ? desiredSize.Height : desiredSize.Width);
			double num2 = (double)Element.GetValue(TextBlock.BaselineOffsetProperty);
			if (!sideways && !double.IsNaN(num2))
			{
				num = num2;
			}
			return new Rect(0.0, 0.0 - num, sideways ? desiredSize.Height : desiredSize.Width, sideways ? desiredSize.Width : desiredSize.Height);
		}
		return Rect.Empty;
	}

	public override void Draw(DrawingContext drawingContext, Point origin, bool rightToLeft, bool sideways)
	{
	}
}
