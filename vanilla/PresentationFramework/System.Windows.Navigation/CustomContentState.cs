namespace System.Windows.Navigation;

/// <summary>
///   <see cref="T:System.Windows.Navigation.CustomContentState" /> enables the ability to navigate through different states of a single piece of source content without reloading the source content for each subsequent navigation.</summary>
[Serializable]
public abstract class CustomContentState
{
	/// <summary>The name for the content that is stored in navigation history. The value of <see cref="P:System.Windows.Navigation.CustomContentState.JournalEntryName" /> is displayed from <see cref="T:System.Windows.Navigation.NavigationWindow" />, <see cref="T:System.Windows.Controls.Frame" />, and Windows Internet Explorer 7 navigation UI.</summary>
	/// <returns>The <see cref="T:System.String" /> name of the content that is stored in navigation history.</returns>
	public virtual string JournalEntryName => null;

	/// <summary>Called to reapply state to a piece of content when navigation occurs.</summary>
	/// <param name="navigationService">The <see cref="T:System.Windows.Navigation.NavigationService" /> owned by the navigator responsible for the content to which this <see cref="T:System.Windows.Navigation.CustomContentState" /> is being applied.</param>
	/// <param name="mode">A <see cref="T:System.Windows.Navigation.NavigationMode" /> that specifies how the content to which the <see cref="T:System.Windows.Navigation.CustomContentState" /> is being applied was navigated to.</param>
	public abstract void Replay(NavigationService navigationService, NavigationMode mode);

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Navigation.CustomContentState" /> class.</summary>
	protected CustomContentState()
	{
	}
}
