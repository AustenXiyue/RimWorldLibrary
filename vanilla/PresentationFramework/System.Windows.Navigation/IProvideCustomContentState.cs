namespace System.Windows.Navigation;

/// <summary>Implemented by a class that needs to add custom state to the navigation history entry for content before the content is navigated away from.</summary>
public interface IProvideCustomContentState
{
	/// <summary>Returns an instance of a custom state class that is to be associated with content in navigation history.</summary>
	/// <returns>An instance of a custom <see cref="T:System.Windows.Navigation.CustomContentState" /> class that is to be associated with content in navigation history.</returns>
	CustomContentState GetContentState();
}
