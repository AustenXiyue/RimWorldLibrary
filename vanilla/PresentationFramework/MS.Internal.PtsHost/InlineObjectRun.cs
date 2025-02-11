using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using MS.Internal.Documents;
using MS.Internal.Text;

namespace MS.Internal.PtsHost;

internal sealed class InlineObjectRun : TextEmbeddedObject
{
	private readonly int _cch;

	private readonly TextRunProperties _textProps;

	private readonly TextParagraph _host;

	private InlineUIContainer _inlineUIContainer;

	public override CharacterBufferReference CharacterBufferReference => new CharacterBufferReference(string.Empty, 0);

	public override int Length => _cch;

	public override TextRunProperties Properties => _textProps;

	public override LineBreakCondition BreakBefore => LineBreakCondition.BreakDesired;

	public override LineBreakCondition BreakAfter => LineBreakCondition.BreakDesired;

	public override bool HasFixedSize => true;

	internal UIElementIsland UIElementIsland => _inlineUIContainer.UIElementIsland;

	internal InlineObjectRun(int cch, UIElement element, TextRunProperties textProps, TextParagraph host)
	{
		_cch = cch;
		_textProps = textProps;
		_host = host;
		_inlineUIContainer = (InlineUIContainer)LogicalTreeHelper.GetParent(element);
	}

	public override TextEmbeddedObjectMetrics Format(double remainingParagraphWidth)
	{
		Size size = _host.MeasureChild(this);
		TextDpi.EnsureValidObjSize(ref size);
		double baseline = size.Height;
		double num = (double)UIElementIsland.Root.GetValue(TextBlock.BaselineOffsetProperty);
		if (!double.IsNaN(num))
		{
			baseline = num;
		}
		return new TextEmbeddedObjectMetrics(size.Width, size.Height, baseline);
	}

	public override Rect ComputeBoundingBox(bool rightToLeft, bool sideways)
	{
		Size desiredSize = UIElementIsland.Root.DesiredSize;
		double num = ((!sideways) ? desiredSize.Height : desiredSize.Width);
		double num2 = (double)UIElementIsland.Root.GetValue(TextBlock.BaselineOffsetProperty);
		if (!sideways && !double.IsNaN(num2))
		{
			num = num2;
		}
		return new Rect(0.0, 0.0 - num, sideways ? desiredSize.Height : desiredSize.Width, sideways ? desiredSize.Width : desiredSize.Height);
	}

	public override void Draw(DrawingContext drawingContext, Point origin, bool rightToLeft, bool sideways)
	{
	}
}
