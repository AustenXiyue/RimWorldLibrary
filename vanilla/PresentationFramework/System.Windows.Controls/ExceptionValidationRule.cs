using System.Globalization;

namespace System.Windows.Controls;

/// <summary>Represents a rule that checks for exceptions that are thrown during the update of the binding source property.</summary>
public sealed class ExceptionValidationRule : ValidationRule
{
	internal static readonly ExceptionValidationRule Instance = new ExceptionValidationRule();

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.ExceptionValidationRule" /> class.</summary>
	public ExceptionValidationRule()
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
