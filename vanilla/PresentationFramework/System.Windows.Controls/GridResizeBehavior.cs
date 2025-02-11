namespace System.Windows.Controls;

/// <summary>Specifies the rows or columns that are resized by a <see cref="T:System.Windows.Controls.GridSplitter" /> control.</summary>
public enum GridResizeBehavior
{
	/// <summary>Space is redistributed based on the value of the <see cref="P:System.Windows.FrameworkElement.HorizontalAlignment" /> and <see cref="P:System.Windows.FrameworkElement.VerticalAlignment" /> properties. </summary>
	BasedOnAlignment,
	/// <summary>For a horizontal <see cref="T:System.Windows.Controls.GridSplitter" />, space is redistributed between the row that is specified for the <see cref="T:System.Windows.Controls.GridSplitter" /> and the next row that is below it. For a vertical <see cref="T:System.Windows.Controls.GridSplitter" />, space is redistributed between the column that is specified for the <see cref="T:System.Windows.Controls.GridSplitter" /> and the next column that is to the right.</summary>
	CurrentAndNext,
	/// <summary>For a horizontal <see cref="T:System.Windows.Controls.GridSplitter" />, space is redistributed between the row that is specified for the <see cref="T:System.Windows.Controls.GridSplitter" /> and the next row that is above it. For a vertical <see cref="T:System.Windows.Controls.GridSplitter" />, space is redistributed between the column that is specified for the <see cref="T:System.Windows.Controls.GridSplitter" /> and the next column that is to the left.</summary>
	PreviousAndCurrent,
	/// <summary>For a horizontal <see cref="T:System.Windows.Controls.GridSplitter" />, space is redistributed between the rows that are above and below the row that is specified for the <see cref="T:System.Windows.Controls.GridSplitter" />. For a vertical <see cref="T:System.Windows.Controls.GridSplitter" />, space is redistributed between the columns that are to the left and right of the column that is specified for the <see cref="T:System.Windows.Controls.GridSplitter" />.</summary>
	PreviousAndNext
}
