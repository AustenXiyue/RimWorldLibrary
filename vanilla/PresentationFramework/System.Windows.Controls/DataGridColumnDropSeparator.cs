using System.Windows.Controls.Primitives;

namespace System.Windows.Controls;

internal class DataGridColumnDropSeparator : Separator
{
	private DataGridColumnHeader _referenceHeader;

	internal DataGridColumnHeader ReferenceHeader
	{
		get
		{
			return _referenceHeader;
		}
		set
		{
			_referenceHeader = value;
		}
	}

	static DataGridColumnDropSeparator()
	{
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(DataGridColumnDropSeparator), new FrameworkPropertyMetadata(DataGridColumnHeader.ColumnHeaderDropSeparatorStyleKey));
		FrameworkElement.WidthProperty.OverrideMetadata(typeof(DataGridColumnDropSeparator), new FrameworkPropertyMetadata(null, OnCoerceWidth));
		FrameworkElement.HeightProperty.OverrideMetadata(typeof(DataGridColumnDropSeparator), new FrameworkPropertyMetadata(null, OnCoerceHeight));
	}

	private static object OnCoerceWidth(DependencyObject d, object baseValue)
	{
		if (double.IsNaN((double)baseValue))
		{
			return 2.0;
		}
		return baseValue;
	}

	private static object OnCoerceHeight(DependencyObject d, object baseValue)
	{
		double d2 = (double)baseValue;
		DataGridColumnDropSeparator dataGridColumnDropSeparator = (DataGridColumnDropSeparator)d;
		if (dataGridColumnDropSeparator._referenceHeader != null && double.IsNaN(d2))
		{
			return dataGridColumnDropSeparator._referenceHeader.ActualHeight;
		}
		return baseValue;
	}
}
