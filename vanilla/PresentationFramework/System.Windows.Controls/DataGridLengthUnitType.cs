namespace System.Windows.Controls;

/// <summary>Defines constants that specify how elements in a <see cref="T:System.Windows.Controls.DataGrid" /> are sized.</summary>
public enum DataGridLengthUnitType
{
	/// <summary>The size is based on the contents of both the cells and the column header.</summary>
	Auto,
	/// <summary>The size is a fixed value expressed in pixels.</summary>
	Pixel,
	/// <summary>The size is based on the contents of the cells.</summary>
	SizeToCells,
	/// <summary>The size is based on the contents of the column header.</summary>
	SizeToHeader,
	/// <summary>The size is a weighted proportion of available space.</summary>
	Star
}
