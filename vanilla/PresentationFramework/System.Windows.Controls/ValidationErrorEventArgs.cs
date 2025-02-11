using MS.Internal;

namespace System.Windows.Controls;

/// <summary>Provides information for the <see cref="E:System.Windows.Controls.Validation.Error" /> attached event.</summary>
public class ValidationErrorEventArgs : RoutedEventArgs
{
	private ValidationError _validationError;

	private ValidationErrorEventAction _action;

	/// <summary>Gets the error that caused this <see cref="E:System.Windows.Controls.Validation.Error" /> event.</summary>
	/// <returns>The <see cref="T:System.Windows.Controls.ValidationError" /> object that caused this <see cref="E:System.Windows.Controls.Validation.Error" /> event.</returns>
	public ValidationError Error => _validationError;

	/// <summary>Gets a value that indicates whether the error is a new error or an existing error that has now been cleared.</summary>
	/// <returns>A <see cref="T:System.Windows.Controls.ValidationErrorEventAction" /> value that indicates whether the error is a new error or an existing error that has now been cleared.</returns>
	public ValidationErrorEventAction Action => _action;

	internal ValidationErrorEventArgs(ValidationError validationError, ValidationErrorEventAction action)
	{
		Invariant.Assert(validationError != null);
		base.RoutedEvent = Validation.ErrorEvent;
		_validationError = validationError;
		_action = action;
	}

	/// <summary>Invokes the specified handler in a type-specific way on the specified object.</summary>
	/// <param name="genericHandler">The generic handler to call in a type-specific way.</param>
	/// <param name="genericTarget">The object to invoke the handler on.</param>
	protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
	{
		((EventHandler<ValidationErrorEventArgs>)genericHandler)(genericTarget, this);
	}
}
