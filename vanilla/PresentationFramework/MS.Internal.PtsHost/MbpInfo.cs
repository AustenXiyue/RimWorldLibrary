using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using MS.Internal.Text;

namespace MS.Internal.PtsHost;

internal sealed class MbpInfo
{
	private Thickness _margin;

	private Thickness _border;

	private Thickness _padding;

	private Brush _borderBrush;

	private static MbpInfo _empty;

	internal int MBPLeft => TextDpi.ToTextDpi(_margin.Left) + TextDpi.ToTextDpi(_border.Left) + TextDpi.ToTextDpi(_padding.Left);

	internal int MBPRight => TextDpi.ToTextDpi(_margin.Right) + TextDpi.ToTextDpi(_border.Right) + TextDpi.ToTextDpi(_padding.Right);

	internal int MBPTop => TextDpi.ToTextDpi(_margin.Top) + TextDpi.ToTextDpi(_border.Top) + TextDpi.ToTextDpi(_padding.Top);

	internal int MBPBottom => TextDpi.ToTextDpi(_margin.Bottom) + TextDpi.ToTextDpi(_border.Bottom) + TextDpi.ToTextDpi(_padding.Bottom);

	internal int BPLeft => TextDpi.ToTextDpi(_border.Left) + TextDpi.ToTextDpi(_padding.Left);

	internal int BPRight => TextDpi.ToTextDpi(_border.Right) + TextDpi.ToTextDpi(_padding.Right);

	internal int BPTop => TextDpi.ToTextDpi(_border.Top) + TextDpi.ToTextDpi(_padding.Top);

	internal int BPBottom => TextDpi.ToTextDpi(_border.Bottom) + TextDpi.ToTextDpi(_padding.Bottom);

	internal int BorderLeft => TextDpi.ToTextDpi(_border.Left);

	internal int BorderRight => TextDpi.ToTextDpi(_border.Right);

	internal int BorderTop => TextDpi.ToTextDpi(_border.Top);

	internal int BorderBottom => TextDpi.ToTextDpi(_border.Bottom);

	internal int MarginLeft => TextDpi.ToTextDpi(_margin.Left);

	internal int MarginRight => TextDpi.ToTextDpi(_margin.Right);

	internal int MarginTop => TextDpi.ToTextDpi(_margin.Top);

	internal int MarginBottom => TextDpi.ToTextDpi(_margin.Bottom);

	internal Thickness Margin
	{
		get
		{
			return _margin;
		}
		set
		{
			_margin = value;
		}
	}

	internal Thickness Border
	{
		get
		{
			return _border;
		}
		set
		{
			_border = value;
		}
	}

	internal Thickness Padding
	{
		get
		{
			return _padding;
		}
		set
		{
			_padding = value;
		}
	}

	internal Brush BorderBrush => _borderBrush;

	private bool IsPaddingAuto
	{
		get
		{
			if (!double.IsNaN(_padding.Left) && !double.IsNaN(_padding.Right) && !double.IsNaN(_padding.Top))
			{
				return double.IsNaN(_padding.Bottom);
			}
			return true;
		}
	}

	private bool IsMarginAuto
	{
		get
		{
			if (!double.IsNaN(_margin.Left) && !double.IsNaN(_margin.Right) && !double.IsNaN(_margin.Top))
			{
				return double.IsNaN(_margin.Bottom);
			}
			return true;
		}
	}

	internal static MbpInfo FromElement(DependencyObject o, double pixelsPerDip)
	{
		if (o is Block || o is AnchoredBlock || o is TableCell || o is ListItem)
		{
			MbpInfo mbpInfo = new MbpInfo((TextElement)o);
			double lineHeightValue = DynamicPropertyReader.GetLineHeightValue(o);
			if (mbpInfo.IsMarginAuto)
			{
				ResolveAutoMargin(mbpInfo, o, lineHeightValue);
			}
			if (mbpInfo.IsPaddingAuto)
			{
				ResolveAutoPadding(mbpInfo, o, lineHeightValue, pixelsPerDip);
			}
			return mbpInfo;
		}
		return _empty;
	}

	internal void MirrorMargin()
	{
		ReverseFlowDirection(ref _margin);
	}

	internal void MirrorBP()
	{
		ReverseFlowDirection(ref _border);
		ReverseFlowDirection(ref _padding);
	}

	private static void ReverseFlowDirection(ref Thickness thickness)
	{
		double left = thickness.Left;
		thickness.Left = thickness.Right;
		thickness.Right = left;
	}

	static MbpInfo()
	{
		_empty = new MbpInfo();
	}

	private MbpInfo()
	{
		_margin = default(Thickness);
		_border = default(Thickness);
		_padding = default(Thickness);
		_borderBrush = new SolidColorBrush();
	}

	private MbpInfo(TextElement block)
	{
		_margin = (Thickness)block.GetValue(Block.MarginProperty);
		_border = (Thickness)block.GetValue(Block.BorderThicknessProperty);
		_padding = (Thickness)block.GetValue(Block.PaddingProperty);
		_borderBrush = (Brush)block.GetValue(Block.BorderBrushProperty);
	}

	private static void ResolveAutoMargin(MbpInfo mbp, DependencyObject o, double lineHeight)
	{
		Thickness thickness;
		if (!(o is Paragraph))
		{
			thickness = ((!(o is Table) && !(o is List)) ? ((!(o is Figure) && !(o is Floater)) ? new Thickness(0.0) : new Thickness(0.5 * lineHeight)) : new Thickness(0.0, lineHeight, 0.0, lineHeight));
		}
		else
		{
			DependencyObject parent = ((Paragraph)o).Parent;
			thickness = ((!(parent is ListItem) && !(parent is TableCell) && !(parent is AnchoredBlock)) ? new Thickness(0.0, lineHeight, 0.0, lineHeight) : new Thickness(0.0));
		}
		mbp.Margin = new Thickness(double.IsNaN(mbp.Margin.Left) ? thickness.Left : mbp.Margin.Left, double.IsNaN(mbp.Margin.Top) ? thickness.Top : mbp.Margin.Top, double.IsNaN(mbp.Margin.Right) ? thickness.Right : mbp.Margin.Right, double.IsNaN(mbp.Margin.Bottom) ? thickness.Bottom : mbp.Margin.Bottom);
	}

	private static void ResolveAutoPadding(MbpInfo mbp, DependencyObject o, double lineHeight, double pixelsPerDip)
	{
		Thickness thickness = ((o is Figure || o is Floater) ? new Thickness(0.5 * lineHeight) : ((!(o is List)) ? new Thickness(0.0) : ListMarkerSourceInfo.CalculatePadding((List)o, lineHeight, pixelsPerDip)));
		mbp.Padding = new Thickness(double.IsNaN(mbp.Padding.Left) ? thickness.Left : mbp.Padding.Left, double.IsNaN(mbp.Padding.Top) ? thickness.Top : mbp.Padding.Top, double.IsNaN(mbp.Padding.Right) ? thickness.Right : mbp.Padding.Right, double.IsNaN(mbp.Padding.Bottom) ? thickness.Bottom : mbp.Padding.Bottom);
	}
}
