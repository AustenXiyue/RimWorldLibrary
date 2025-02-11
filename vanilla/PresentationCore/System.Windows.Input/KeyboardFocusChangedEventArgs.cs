using MS.Internal.PresentationCore;

namespace System.Windows.Input;

/// <summary>Provides data for <see cref="E:System.Windows.UIElement.LostKeyboardFocus" /> and <see cref="E:System.Windows.UIElement.GotKeyboardFocus" /> routed events, as well as related attached and Preview events.</summary>
public class KeyboardFocusChangedEventArgs : KeyboardEventArgs
{
	private IInputElement _oldFocus;

	private IInputElement _newFocus;

	/// <summary>Gets the element that previously had focus. </summary>
	/// <returns>The previously focused element.</returns>
	public IInputElement OldFocus => _oldFocus;

	/// <summary>Gets the element that focus has moved to.</summary>
	/// <returns>The element with focus.</returns>
	public IInputElement NewFocus => _newFocus;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.KeyboardFocusChangedEventArgs" /> class. </summary>
	/// <param name="keyboard">The logical keyboard device associated with this event.</param>
	/// <param name="timestamp">The time when the input occurred.</param>
	/// <param name="oldFocus">The element that previously had focus.</param>
	/// <param name="newFocus">The element that now has focus.</param>
	public KeyboardFocusChangedEventArgs(KeyboardDevice keyboard, int timestamp, IInputElement oldFocus, IInputElement newFocus)
		: base(keyboard, timestamp)
	{
		if (oldFocus != null && !InputElement.IsValid(oldFocus))
		{
			throw new InvalidOperationException(SR.Format(SR.Invalid_IInputElement, oldFocus.GetType()));
		}
		if (newFocus != null && !InputElement.IsValid(newFocus))
		{
			throw new InvalidOperationException(SR.Format(SR.Invalid_IInputElement, newFocus.GetType()));
		}
		_oldFocus = oldFocus;
		_newFocus = newFocus;
	}

	/// <summary>Invokes event handlers in a type-specific way, which can increase event system efficiency.</summary>
	/// <param name="genericHandler">The generic handler to call in a type-specific way.</param>
	/// <param name="genericTarget">The target to call the handler on.</param>
	protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
	{
		((KeyboardFocusChangedEventHandler)genericHandler)(genericTarget, this);
	}
}
