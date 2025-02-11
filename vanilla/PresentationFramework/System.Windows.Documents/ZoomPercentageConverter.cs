using System.Globalization;
using System.Windows.Data;

namespace System.Windows.Documents;

/// <summary>Implements a type converter for converting <see cref="T:System.Double" /> (used as the value of <see cref="P:System.Windows.Controls.DocumentViewer.Zoom" />) to and from other types.</summary>
public sealed class ZoomPercentageConverter : IValueConverter
{
	internal const string ZoomPercentageConverterStringFormat = "{0:0.##}%";

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.ZoomPercentageConverter" /> class. </summary>
	public ZoomPercentageConverter()
	{
	}

	/// <summary>Converts the <see cref="T:System.Double" /> (used as the value of <see cref="P:System.Windows.Controls.DocumentViewer.Zoom" />) to an object of the specified type. </summary>
	/// <returns>
	///   <see cref="F:System.Windows.DependencyProperty.UnsetValue" /> when the converter cannot produce a value; for example, when <paramref name="value" /> is null or when <paramref name="targetType" /> is not <see cref="T:System.Double" /> or <see cref="T:System.String" />.- or -The new <see cref="T:System.Object" /> of the designated type. As implemented in this class, this must be either a <see cref="T:System.Double" /> or a <see cref="T:System.String" />. If it is a string, it will be formatted appropriately for the <paramref name="culture" />.</returns>
	/// <param name="value">The current value of <see cref="P:System.Windows.Controls.DocumentViewer.Zoom" />.</param>
	/// <param name="targetType">The type to which <paramref name="value" /> is to be converted. This must be <see cref="T:System.Double" /> or <see cref="T:System.String" />. See Remarks.</param>
	/// <param name="parameter">null. See Remarks.</param>
	/// <param name="culture">The language and culture assumed during the conversion.</param>
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (targetType == null)
		{
			return DependencyProperty.UnsetValue;
		}
		if (value != null && value is double num)
		{
			if (targetType == typeof(string) || targetType == typeof(object))
			{
				if (double.IsNaN(num) || double.IsInfinity(num))
				{
					return DependencyProperty.UnsetValue;
				}
				return string.Format(CultureInfo.CurrentCulture, "{0:0.##}%", num);
			}
			if (targetType == typeof(double))
			{
				return num;
			}
		}
		return DependencyProperty.UnsetValue;
	}

	/// <summary>Returns a previously converted value of <see cref="P:System.Windows.Controls.DocumentViewer.Zoom" /> back to a <see cref="T:System.Double" /> that can be assigned to <see cref="P:System.Windows.Controls.DocumentViewer.Zoom" />. </summary>
	/// <returns>
	///   <see cref="F:System.Windows.DependencyProperty.UnsetValue" /> when the converter cannot produce a value; for example, when <paramref name="value" /> is not a valid percentage when <paramref name="targetType" /> is not <see cref="T:System.Double" /> or <see cref="T:System.String" />.- or -A <see cref="T:System.Double" /> representing the zoom percentage of a <see cref="T:System.Windows.Controls.DocumentViewer" />.</returns>
	/// <param name="value">The object that is to be converted back to a <see cref="T:System.Double" />. </param>
	/// <param name="targetType">The type of <paramref name="value" />. This must be <see cref="T:System.Double" /> or <see cref="T:System.String" />. See Remarks.</param>
	/// <param name="parameter">null. See Remarks.</param>
	/// <param name="culture">The language and culture assumed during the conversion.</param>
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (targetType == typeof(double) && value != null)
		{
			double num = 0.0;
			bool flag = false;
			if (value is int)
			{
				num = (int)value;
				flag = true;
			}
			else if (value is double)
			{
				num = (double)value;
				flag = true;
			}
			else if (value is string)
			{
				try
				{
					string value2 = (string)value;
					if (culture != null && !string.IsNullOrEmpty(value2))
					{
						value2 = ((string)value).Trim();
						if (!culture.IsNeutralCulture && value2.Length > 0 && culture.NumberFormat != null)
						{
							switch (culture.NumberFormat.PercentPositivePattern)
							{
							case 0:
							case 1:
								if (value2.Length - 1 == value2.LastIndexOf(culture.NumberFormat.PercentSymbol, StringComparison.CurrentCultureIgnoreCase))
								{
									value2 = value2.Substring(0, value2.Length - 1);
								}
								break;
							case 2:
								if (value2.IndexOf(culture.NumberFormat.PercentSymbol, StringComparison.CurrentCultureIgnoreCase) == 0)
								{
									value2 = value2.Substring(1);
								}
								break;
							}
						}
						num = System.Convert.ToDouble(value2, culture);
						flag = true;
					}
				}
				catch (ArgumentOutOfRangeException)
				{
				}
				catch (ArgumentNullException)
				{
				}
				catch (FormatException)
				{
				}
				catch (OverflowException)
				{
				}
			}
			if (!flag)
			{
				return DependencyProperty.UnsetValue;
			}
			return num;
		}
		return DependencyProperty.UnsetValue;
	}
}
