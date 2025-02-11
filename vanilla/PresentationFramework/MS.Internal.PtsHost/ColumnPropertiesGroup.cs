using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace MS.Internal.PtsHost;

internal sealed class ColumnPropertiesGroup
{
	private double _columnWidth;

	private bool _isColumnWidthFlexible;

	private double _columnGap;

	private Brush _columnRuleBrush;

	private double _columnRuleWidth;

	internal double ColumnWidth => _columnWidth;

	internal bool IsColumnWidthFlexible => _isColumnWidthFlexible;

	internal ColumnSpaceDistribution ColumnSpaceDistribution => ColumnSpaceDistribution.Between;

	internal double ColumnGap
	{
		get
		{
			Invariant.Assert(!double.IsNaN(_columnGap));
			return _columnGap;
		}
	}

	internal Brush ColumnRuleBrush => _columnRuleBrush;

	internal double ColumnRuleWidth => _columnRuleWidth;

	internal bool ColumnWidthAuto => double.IsNaN(_columnWidth);

	internal bool ColumnGapAuto => double.IsNaN(_columnGap);

	internal ColumnPropertiesGroup(DependencyObject o)
	{
		_columnWidth = (double)o.GetValue(FlowDocument.ColumnWidthProperty);
		_columnGap = (double)o.GetValue(FlowDocument.ColumnGapProperty);
		_columnRuleWidth = (double)o.GetValue(FlowDocument.ColumnRuleWidthProperty);
		_columnRuleBrush = (Brush)o.GetValue(FlowDocument.ColumnRuleBrushProperty);
		_isColumnWidthFlexible = (bool)o.GetValue(FlowDocument.IsColumnWidthFlexibleProperty);
	}
}
