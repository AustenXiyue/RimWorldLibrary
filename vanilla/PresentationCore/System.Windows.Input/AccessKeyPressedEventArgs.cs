namespace System.Windows.Input;

/// <summary>Provides data for the <see cref="T:System.Windows.Input.AccessKeyManager" /> routed event.</summary>
public class AccessKeyPressedEventArgs : RoutedEventArgs
{
	private object _scope;

	private UIElement _target;

	private string _key;

	/// <summary>Gets the scope for the element that raised this event. </summary>
	/// <returns>The element's scope.</returns>
	public object Scope
	{
		get
		{
			return _scope;
		}
		set
		{
			_scope = value;
		}
	}

	/// <summary> Gets or sets the target for the event. </summary>
	/// <returns>The element that raised this event.</returns>
	public UIElement Target
	{
		get
		{
			return _target;
		}
		set
		{
			_target = value;
		}
	}

	/// <summary>Gets a string representation of the access key that was pressed </summary>
	/// <returns>The access key.</returns>
	public string Key => _key;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.AccessKeyEventArgs" /> class.</summary>
	public AccessKeyPressedEventArgs()
	{
		base.RoutedEvent = AccessKeyManager.AccessKeyPressedEvent;
		_key = null;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.AccessKeyPressedEventArgs" /> class with the specified access key.</summary>
	/// <param name="key">The access key.</param>
	public AccessKeyPressedEventArgs(string key)
		: this()
	{
		_key = key;
	}

	/// <summary>Invokes event handlers in a type-specific way, which can increase event system efficiency.</summary>
	/// <param name="genericHandler">The generic handler to call in a type-specific way.</param>
	/// <param name="genericTarget">The target to call the handler on.</param>
	protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
	{
		((AccessKeyPressedEventHandler)genericHandler)(genericTarget, this);
	}
}
