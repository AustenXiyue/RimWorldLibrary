namespace System.Windows.Controls;

/// <summary>Specifies whether all the pages or only a limited range will be processed by an operation, usually printing.</summary>
public enum PageRangeSelection
{
	/// <summary>All pages.</summary>
	AllPages,
	/// <summary>A user-specified range of pages.</summary>
	UserPages,
	/// <summary>The current page.</summary>
	CurrentPage,
	/// <summary>The selected pages.</summary>
	SelectedPages
}
