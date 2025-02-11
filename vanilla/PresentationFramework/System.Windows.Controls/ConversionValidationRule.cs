using System.Globalization;

namespace System.Windows.Controls;

internal sealed class ConversionValidationRule : ValidationRule
{
	internal static readonly ConversionValidationRule Instance = new ConversionValidationRule();

	internal ConversionValidationRule()
		: base(ValidationStep.ConvertedProposedValue, validatesOnTargetUpdated: false)
	{
	}

	public override ValidationResult Validate(object value, CultureInfo cultureInfo)
	{
		return ValidationResult.ValidResult;
	}
}
