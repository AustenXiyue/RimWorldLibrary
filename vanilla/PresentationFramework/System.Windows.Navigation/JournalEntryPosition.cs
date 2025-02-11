namespace System.Windows.Navigation;

/// <summary>Specifies the position in navigation history of a piece of content with respect to current content. <see cref="T:System.Windows.Navigation.JournalEntryPosition" /> is used by <see cref="T:System.Windows.Navigation.JournalEntryUnifiedViewConverter" />.</summary>
public enum JournalEntryPosition
{
	/// <summary>Content is in back navigation history relative to current content.</summary>
	Back,
	/// <summary>Content is the current content.</summary>
	Current,
	/// <summary>Content is in forward navigation history with respect to current content.</summary>
	Forward
}
