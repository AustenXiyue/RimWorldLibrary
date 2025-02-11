namespace System.Windows.Controls;

/// <summary>Defines constants that specify the visibility of row and column headers in a <see cref="T:System.Windows.Controls.DataGrid" />.</summary>
[Flags]
public enum DataGridHeadersVisibility
{
	/// <summary>Both row and column headers are visible.</summary>
	All = 3,
	/// <summary>Only column headers are visible.</summary>
	Column = 1,
	/// <summary>Only row headers are visible.</summary>
	Row = 2,
	/// <summary>No headers are visible.</summary>
	None = 0
}
