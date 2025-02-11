using System.Globalization;
using System.Windows.Data;

namespace System.Windows.Controls;

/// <summary>Provides a way to create a custom rule in order to check the validity of user input. </summary>
public abstract class ValidationRule
{
	private ValidationStep _validationStep;

	private bool _validatesOnTargetUpdated;

	/// <summary>Gets or sets when the validation rule runs.</summary>
	/// <returns>One of the enumeration values.  The default is <see cref="F:System.Windows.Controls.ValidationStep.RawProposedValue" />.</returns>
	public ValidationStep ValidationStep
	{
		get
		{
			return _validationStep;
		}
		set
		{
			_validationStep = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether the validation rule runs when the target of the <see cref="T:System.Windows.Data.Binding" /> is updated. </summary>
	/// <returns>true if the validation rule runs when the target of the <see cref="T:System.Windows.Data.Binding" /> is updated; otherwise, false.</returns>
	public bool ValidatesOnTargetUpdated
	{
		get
		{
			return _validatesOnTargetUpdated;
		}
		set
		{
			_validatesOnTargetUpdated = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.ValidationRule" /> class.</summary>
	protected ValidationRule()
		: this(ValidationStep.RawProposedValue, validatesOnTargetUpdated: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.ValidationRule" /> class with the specified validation step and a value that indicates whether the validation rule runs when the target is updated. </summary>
	/// <param name="validationStep">One of the enumeration values that specifies when the validation rule runs.</param>
	/// <param name="validatesOnTargetUpdated">true to have the validation rule run when the target of the <see cref="T:System.Windows.Data.Binding" /> is updated; otherwise, false.</param>
	protected ValidationRule(ValidationStep validationStep, bool validatesOnTargetUpdated)
	{
		_validationStep = validationStep;
		_validatesOnTargetUpdated = validatesOnTargetUpdated;
	}

	/// <summary>When overridden in a derived class, performs validation checks on a value.</summary>
	/// <returns>A <see cref="T:System.Windows.Controls.ValidationResult" /> object.</returns>
	/// <param name="value">The value from the binding target to check.</param>
	/// <param name="cultureInfo">The culture to use in this rule.</param>
	public abstract ValidationResult Validate(object value, CultureInfo cultureInfo);

	/// <summary>Performs validation checks on a value.</summary>
	/// <returns>A <see cref="T:System.Windows.Controls.ValidationResult" /> object.</returns>
	/// <param name="value">The value from the binding target to check.</param>
	/// <param name="cultureInfo">The culture to use in this rule.</param>
	/// <param name="owner">The binding expression that uses the validation rule.</param>
	public virtual ValidationResult Validate(object value, CultureInfo cultureInfo, BindingExpressionBase owner)
	{
		ValidationStep validationStep = _validationStep;
		if ((uint)(validationStep - 2) <= 1u)
		{
			value = owner;
		}
		return Validate(value, cultureInfo);
	}

	/// <summary>Performs validation checks on a value.</summary>
	/// <returns>A <see cref="T:System.Windows.Controls.ValidationResult" /> object.</returns>
	/// <param name="value">The value from the binding target to check.</param>
	/// <param name="cultureInfo">The culture to use in this rule.</param>
	/// <param name="owner">The binding group that uses the validation rule.</param>
	public virtual ValidationResult Validate(object value, CultureInfo cultureInfo, BindingGroup owner)
	{
		return Validate(owner, cultureInfo);
	}
}
