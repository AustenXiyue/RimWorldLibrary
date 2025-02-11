namespace System.Windows.Controls;

/// <summary>Defines constants that specify whether users can copy data from a <see cref="T:System.Windows.Controls.DataGrid" /> control to the Clipboard and whether column header values are included.</summary>
public enum DataGridClipboardCopyMode
{
	/// <summary>Clipboard support is disabled.</summary>
	None,
	/// <summary>Users can copy the text values of selected cells to the Clipboard, and column header values are not included. </summary>
	ExcludeHeader,
	/// <summary>Users can copy the text values of selected cells to the Clipboard, and column header values are included. </summary>
	IncludeHeader
}
