using System.Windows.Documents;

namespace System.Windows.Navigation;

/// <summary>Provides data for the <see cref="E:System.Windows.Documents.Hyperlink.RequestNavigate" /> event. </summary>
public class RequestNavigateEventArgs : RoutedEventArgs
{
	private Uri _uri;

	private string _target;

	/// <summary>The uniform resource identifier (URI) for the content that is being navigated to.</summary>
	/// <returns>The URI for the content that is being navigated to.</returns>
	public Uri Uri => _uri;

	/// <summary>The navigator that will host the content that is navigated to.</summary>
	/// <returns>The navigator (<see cref="T:System.Windows.Navigation.NavigationWindow" /> or <see cref="T:System.Windows.Controls.Frame" />) that will host the content that is navigated to.</returns>
	public string Target => _target;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Navigation.RequestNavigateEventArgs" /> class. This constructor is protected.</summary>
	protected RequestNavigateEventArgs()
	{
		base.RoutedEvent = Hyperlink.RequestNavigateEvent;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Navigation.RequestNavigateEventArgs" /> class with a uniform resource identifier (URI) and target name.</summary>
	/// <param name="uri">The target URI.</param>
	/// <param name="target">The target name.</param>
	public RequestNavigateEventArgs(Uri uri, string target)
	{
		_uri = uri;
		_target = target;
		base.RoutedEvent = Hyperlink.RequestNavigateEvent;
	}

	/// <summary>Invokes a specified event handler from a specified sender. </summary>
	/// <param name="genericHandler">The name of the handler.</param>
	/// <param name="genericTarget">The object that is raising the event.</param>
	protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
	{
		if (base.RoutedEvent == null)
		{
			throw new InvalidOperationException(SR.RequestNavigateEventMustHaveRoutedEvent);
		}
		((RequestNavigateEventHandler)genericHandler)(genericTarget, this);
	}
}
