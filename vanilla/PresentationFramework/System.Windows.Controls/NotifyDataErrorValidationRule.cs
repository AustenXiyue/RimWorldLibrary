using System.Globalization;

namespace System.Windows.Controls;

/// <summary>Represents a rule that checks for errors that are raised by a data source that implements <see cref="T:System.ComponentModel.INotifyDataErrorInfo" />.</summary>
public sealed class NotifyDataErrorValidationRule : ValidationRule
{
	internal static readonly NotifyDataErrorValidationRule Instance = new NotifyDataErrorValidationRule();

	/// <summary>Creates a new instance of the <see cref="T:System.Windows.Controls.NotifyDataErrorValidationRule" /> class.</summary>
	public NotifyDataErrorValidationRule()
		: base(ValidationStep.UpdatedValue, validatesOnTargetUpdated: true)
	{
	}

	/// <summary>Performs validation checks on a value.</summary>
	/// <returns>A <see cref="T:System.Windows.Controls.ValidationResult" /> object.</returns>
	/// <param name="value">The value (from the binding target) to check.</param>
	/// <param name="cultureInfo">The culture to use in this rule.</param>
	public override ValidationResult Validate(object value, CultureInfo cultureInfo)
	{
		return ValidationResult.ValidResult;
	}
}
