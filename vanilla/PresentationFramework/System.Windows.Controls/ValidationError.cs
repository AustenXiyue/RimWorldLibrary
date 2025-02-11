namespace System.Windows.Controls;

/// <summary>Represents a validation error that is created either by the binding engine when a <see cref="T:System.Windows.Controls.ValidationRule" /> reports a validation error, or through the <see cref="M:System.Windows.Controls.Validation.MarkInvalid(System.Windows.Data.BindingExpressionBase,System.Windows.Controls.ValidationError)" /> method explicitly.</summary>
public class ValidationError
{
	private ValidationRule _ruleInError;

	private object _errorContent;

	private Exception _exception;

	private object _bindingInError;

	/// <summary>Gets or sets the <see cref="T:System.Windows.Controls.ValidationRule" /> object that was the cause of this <see cref="T:System.Windows.Controls.ValidationError" />, if the error is the result of a validation rule.</summary>
	/// <returns>The <see cref="T:System.Windows.Controls.ValidationRule" /> object, if the error is the result of a validation rule.</returns>
	public ValidationRule RuleInError
	{
		get
		{
			return _ruleInError;
		}
		set
		{
			_ruleInError = value;
		}
	}

	/// <summary>Gets or sets an object that provides additional context for this <see cref="T:System.Windows.Controls.ValidationError" />, such as a string describing the error.</summary>
	/// <returns>An object that provides additional context for this <see cref="T:System.Windows.Controls.ValidationError" />.</returns>
	public object ErrorContent
	{
		get
		{
			return _errorContent;
		}
		set
		{
			_errorContent = value;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Exception" /> object that was the cause of this <see cref="T:System.Windows.Controls.ValidationError" />, if the error is the result of an exception.</summary>
	/// <returns>The <see cref="T:System.Exception" /> object, if the error is the result of an exception.</returns>
	public Exception Exception
	{
		get
		{
			return _exception;
		}
		set
		{
			_exception = value;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.Data.BindingExpression" /> or <see cref="T:System.Windows.Data.MultiBindingExpression" /> object of this <see cref="T:System.Windows.Controls.ValidationError" />. The object is either marked invalid explicitly or has a failed validation rule.</summary>
	/// <returns>The <see cref="T:System.Windows.Data.BindingExpression" /> or <see cref="T:System.Windows.Data.MultiBindingExpression" /> object of this <see cref="T:System.Windows.Controls.ValidationError" />.</returns>
	public object BindingInError => _bindingInError;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.ValidationError" /> class with the specified parameters.</summary>
	/// <param name="ruleInError">The rule that detected validation error.</param>
	/// <param name="bindingInError">The <see cref="T:System.Windows.Data.BindingExpression" /> or <see cref="T:System.Windows.Data.MultiBindingExpression" /> object with the validation error.</param>
	/// <param name="errorContent">Information about the error.</param>
	/// <param name="exception">The exception that caused the validation failure. This parameter is optional and can be set to null.</param>
	public ValidationError(ValidationRule ruleInError, object bindingInError, object errorContent, Exception exception)
	{
		if (ruleInError == null)
		{
			throw new ArgumentNullException("ruleInError");
		}
		if (bindingInError == null)
		{
			throw new ArgumentNullException("bindingInError");
		}
		_ruleInError = ruleInError;
		_bindingInError = bindingInError;
		_errorContent = errorContent;
		_exception = exception;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.ValidationError" /> class with the specified parameters.</summary>
	/// <param name="ruleInError">The rule that detected validation error.</param>
	/// <param name="bindingInError">The <see cref="T:System.Windows.Data.BindingExpression" /> or <see cref="T:System.Windows.Data.MultiBindingExpression" /> object with the validation error.</param>
	public ValidationError(ValidationRule ruleInError, object bindingInError)
		: this(ruleInError, bindingInError, null, null)
	{
	}
}
