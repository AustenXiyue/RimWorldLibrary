using System.ComponentModel;

namespace System.Windows.Input;

/// <summary>Represents a request to move focus to another control. </summary>
[Serializable]
public class TraversalRequest
{
	private bool _wrapped;

	private FocusNavigationDirection _focusNavigationDirection;

	/// <summary> Gets or sets a value that indicates whether focus traversal has reached the end of child elements that can have focus. </summary>
	/// <returns>true if this traversal has reached the end of child elements that can have focus; otherwise, false. The default is false.</returns>
	public bool Wrapped
	{
		get
		{
			return _wrapped;
		}
		set
		{
			_wrapped = value;
		}
	}

	/// <summary>Gets the traversal direction. </summary>
	/// <returns>One of the traversal direction enumeration values.</returns>
	public FocusNavigationDirection FocusNavigationDirection => _focusNavigationDirection;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.TraversalRequest" /> class. </summary>
	/// <param name="focusNavigationDirection">The intended direction of the focus traversal, as a value of the enumeration.</param>
	public TraversalRequest(FocusNavigationDirection focusNavigationDirection)
	{
		if (focusNavigationDirection != 0 && focusNavigationDirection != FocusNavigationDirection.Previous && focusNavigationDirection != FocusNavigationDirection.First && focusNavigationDirection != FocusNavigationDirection.Last && focusNavigationDirection != FocusNavigationDirection.Left && focusNavigationDirection != FocusNavigationDirection.Right && focusNavigationDirection != FocusNavigationDirection.Up && focusNavigationDirection != FocusNavigationDirection.Down)
		{
			throw new InvalidEnumArgumentException("focusNavigationDirection", (int)focusNavigationDirection, typeof(FocusNavigationDirection));
		}
		_focusNavigationDirection = focusNavigationDirection;
	}
}
