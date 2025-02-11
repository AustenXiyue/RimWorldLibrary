namespace System.Windows.Controls;

/// <summary>Provides data for the <see cref="E:System.Windows.Controls.DatePicker.DateValidationError" /> event.</summary>
public class DatePickerDateValidationErrorEventArgs : EventArgs
{
	private bool _throwException;

	/// <summary>Gets the initial exception associated with the <see cref="E:System.Windows.Controls.DatePicker.DateValidationError" /> event.</summary>
	/// <returns>The exception associated with the validation failure.</returns>
	public Exception Exception { get; private set; }

	/// <summary>Gets or sets the text that caused the <see cref="E:System.Windows.Controls.DatePicker.DateValidationError" /> event.</summary>
	/// <returns>The text that caused the validation failure.</returns>
	public string Text { get; private set; }

	/// <summary>Gets or sets a value that indicates whether <see cref="P:System.Windows.Controls.DatePickerDateValidationErrorEventArgs.Exception" /> should be thrown.</summary>
	/// <returns>true if the exception should be thrown; otherwise, false.</returns>
	public bool ThrowException
	{
		get
		{
			return _throwException;
		}
		set
		{
			_throwException = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.DatePickerDateValidationErrorEventArgs" /> class. </summary>
	/// <param name="exception">The initial exception from the <see cref="E:System.Windows.Controls.DatePicker.DateValidationError" /> event.</param>
	/// <param name="text">The text that caused the <see cref="E:System.Windows.Controls.DatePicker.DateValidationError" /> event.</param>
	public DatePickerDateValidationErrorEventArgs(Exception exception, string text)
	{
		Text = text;
		Exception = exception;
	}
}
