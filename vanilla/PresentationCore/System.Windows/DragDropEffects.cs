namespace System.Windows;

/// <summary>Specifies the effects of a drag-and-drop operation.</summary>
[Flags]
public enum DragDropEffects
{
	/// <summary>The drop target does not accept the data.</summary>
	None = 0,
	/// <summary>The data is copied to the drop target.</summary>
	Copy = 1,
	/// <summary>The data from the drag source is moved to the drop target.</summary>
	Move = 2,
	/// <summary>The data from the drag source is linked to the drop target.</summary>
	Link = 4,
	/// <summary>Scrolling is about to start or is currently occurring in the drop target.</summary>
	Scroll = int.MinValue,
	/// <summary>The data is copied, removed from the drag source, and scrolled in the drop target.</summary>
	All = -2147483645
}
