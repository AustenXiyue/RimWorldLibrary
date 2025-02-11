namespace System.Windows.Navigation;

/// <summary>Specifies whether a <see cref="T:System.Windows.Controls.Frame" /> uses its own journal. <see cref="T:System.Windows.Navigation.JournalOwnership" /> is used by the <see cref="P:System.Windows.Controls.Frame.JournalOwnership" /> property.</summary>
[Serializable]
public enum JournalOwnership
{
	/// <summary>Whether or not this <see cref="T:System.Windows.Controls.Frame" /> will create and use its own journal depends on its parent.</summary>
	Automatic,
	/// <summary>The <see cref="T:System.Windows.Controls.Frame" /> maintains its own journal.</summary>
	OwnsJournal,
	/// <summary>The <see cref="T:System.Windows.Controls.Frame" /> uses the journal of the next available navigation host up the content tree, if available. Otherwise, navigation history is not maintained for the <see cref="T:System.Windows.Controls.Frame" />.</summary>
	UsesParentJournal
}
