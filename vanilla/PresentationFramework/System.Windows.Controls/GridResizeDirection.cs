namespace System.Windows.Controls;

/// <summary>Specifies whether a <see cref="T:System.Windows.Controls.GridSplitter" /> control redistributes space between rows or between columns.</summary>
public enum GridResizeDirection
{
	/// <summary>Space is redistributed based on the values of the <see cref="P:System.Windows.FrameworkElement.HorizontalAlignment" />, <see cref="P:System.Windows.FrameworkElement.VerticalAlignment" />, <see cref="P:System.Windows.FrameworkElement.ActualWidth" />, and <see cref="P:System.Windows.FrameworkElement.ActualHeight" /> properties of the <see cref="T:System.Windows.Controls.GridSplitter" />. </summary>
	Auto,
	/// <summary>Space is redistributed between columns.</summary>
	Columns,
	/// <summary>Space is redistributed between rows.</summary>
	Rows
}
