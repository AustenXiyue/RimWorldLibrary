namespace System.Windows.Controls;

/// <summary>Defines the state or role of a <see cref="T:System.Windows.Controls.GridViewColumnHeader" /> control.</summary>
public enum GridViewColumnHeaderRole
{
	/// <summary>The column header displays above its associated column.</summary>
	Normal,
	/// <summary>The column header is the object of a drag-and-drop operation to move a column.</summary>
	Floating,
	/// <summary>The column header is the last header in the row of column headers and is used for padding.</summary>
	Padding
}
