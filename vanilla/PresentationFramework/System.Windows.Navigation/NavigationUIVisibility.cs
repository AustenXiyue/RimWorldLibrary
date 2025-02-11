namespace System.Windows.Navigation;

/// <summary>Specifies whether a <see cref="T:System.Windows.Controls.Frame" /> displays its navigation chrome. <see cref="T:System.Windows.Navigation.NavigationUIVisibility" /> is used by the <see cref="P:System.Windows.Controls.Frame.NavigationUIVisibility" /> property.</summary>
public enum NavigationUIVisibility
{
	/// <summary>The navigation chrome is visible when a <see cref="T:System.Windows.Controls.Frame" /> uses its own journal (see <see cref="P:System.Windows.Controls.Frame.JournalOwnership" />).</summary>
	Automatic,
	/// <summary>The navigation chrome is visible.</summary>
	Visible,
	/// <summary>The navigation chrome is not visible.</summary>
	Hidden
}
