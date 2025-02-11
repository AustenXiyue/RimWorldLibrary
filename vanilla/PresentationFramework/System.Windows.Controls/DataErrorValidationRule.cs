using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;
using MS.Internal;

namespace System.Windows.Controls;

/// <summary>Represents a rule that checks for errors that are raised by the <see cref="T:System.ComponentModel.IDataErrorInfo" /> implementation of the source object.</summary>
public sealed class DataErrorValidationRule : ValidationRule
{
	internal static readonly DataErrorValidationRule Instance = new DataErrorValidationRule();

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.DataErrorValidationRule" /> class.</summary>
	public DataErrorValidationRule()
		: base(ValidationStep.UpdatedValue, validatesOnTargetUpdated: true)
	{
	}

	/// <summary>Performs validation checks on a value.</summary>
	/// <returns>The result of the validation.</returns>
	/// <param name="value">The value to check.</param>
	/// <param name="cultureInfo">The culture to use in this rule.</param>
	public override ValidationResult Validate(object value, CultureInfo cultureInfo)
	{
		if (value is BindingGroup { Items: var items })
		{
			for (int num = items.Count - 1; num >= 0; num--)
			{
				if (items[num] is IDataErrorInfo dataErrorInfo)
				{
					string error = dataErrorInfo.Error;
					if (!string.IsNullOrEmpty(error))
					{
						return new ValidationResult(isValid: false, error);
					}
				}
			}
		}
		else
		{
			if (!(value is BindingExpression bindingExpression))
			{
				throw new InvalidOperationException(SR.Format(SR.ValidationRule_UnexpectedValue, this, value));
			}
			IDataErrorInfo dataErrorInfo2 = bindingExpression.SourceItem as IDataErrorInfo;
			string text = ((dataErrorInfo2 != null) ? bindingExpression.SourcePropertyName : null);
			if (!string.IsNullOrEmpty(text))
			{
				string text2;
				try
				{
					text2 = dataErrorInfo2[text];
				}
				catch (Exception ex)
				{
					if (CriticalExceptions.IsCriticalApplicationException(ex))
					{
						throw;
					}
					text2 = null;
					if (TraceData.IsEnabled)
					{
						TraceData.TraceAndNotify(TraceEventType.Error, TraceData.DataErrorInfoFailed(text, dataErrorInfo2.GetType().FullName, ex.GetType().FullName, ex.Message), bindingExpression);
					}
				}
				if (!string.IsNullOrEmpty(text2))
				{
					return new ValidationResult(isValid: false, text2);
				}
			}
		}
		return ValidationResult.ValidResult;
	}
}
