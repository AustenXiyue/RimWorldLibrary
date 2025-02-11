namespace System.Windows.Input;

/// <summary>Contains arguments associated with changes to a <see cref="T:System.Windows.Input.TextComposition" />.</summary>
public class TextCompositionEventArgs : InputEventArgs
{
	private TextComposition _composition;

	/// <summary>Gets the <see cref="T:System.Windows.Input.TextComposition" /> associated with a <see cref="T:System.Windows.Input.TextComposition" /> event.</summary>
	/// <returns>A <see cref="T:System.Windows.Input.TextComposition" /> object containing the text composition associated with this event.</returns>
	public TextComposition TextComposition => _composition;

	/// <summary>Gets input text associated with a <see cref="T:System.Windows.Input.TextComposition" /> event.</summary>
	/// <returns>A string containing the input text associated with this event.</returns>
	public string Text => _composition.Text;

	/// <summary>Gets system text associated with a <see cref="T:System.Windows.Input.TextComposition" /> event.</summary>
	/// <returns>A string containing any system text associated with this event.</returns>
	public string SystemText => _composition.SystemText;

	/// <summary>Gets control text associated with a <see cref="T:System.Windows.Input.TextComposition" /> event.</summary>
	/// <returns>A string containing any control text associated with this event.</returns>
	public string ControlText => _composition.ControlText;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.TextCompositionEventArgs" /> class, taking a specified <see cref="T:System.Windows.Input.InputDevice" /> and <see cref="T:System.Windows.Input.TextComposition" /> as initial values for the class.</summary>
	/// <param name="inputDevice">The input device associated with this event.</param>
	/// <param name="composition">A <see cref="T:System.Windows.Input.TextComposition" /> object associated with this event.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when composition is null.</exception>
	public TextCompositionEventArgs(InputDevice inputDevice, TextComposition composition)
		: base(inputDevice, Environment.TickCount)
	{
		if (composition == null)
		{
			throw new ArgumentNullException("composition");
		}
		_composition = composition;
	}

	/// <summary>Invokes event handlers in a type-specific way, which can increase event system efficiency.</summary>
	/// <param name="genericHandler">The generic handler to call in a type-specific way.</param>
	/// <param name="genericTarget">The target to call the handler on.</param>
	protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
	{
		((TextCompositionEventHandler)genericHandler)(genericTarget, this);
	}
}
