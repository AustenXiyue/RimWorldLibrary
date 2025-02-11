using System.Globalization;
using System.Windows.Data;
using MS.Internal;

namespace System.Windows.Controls;

/// <summary>Represents a data-binding converter to handle the visibility of repeat buttons in scrolling menus.</summary>
public sealed class MenuScrollingVisibilityConverter : IMultiValueConverter
{
	/// <summary>Called when moving a value from a source to a target.</summary>
	/// <returns>Converted value.</returns>
	/// <param name="values">Values produced by the source binding.</param>
	/// <param name="targetType">Type of the target. Type that the source will be converted into.</param>
	/// <param name="parameter">Converter parameter.</param>
	/// <param name="culture">Culture information.</param>
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		if (parameter == null || values == null || values.Length != 4 || !(values[0] is Visibility) || !(values[1] is double) || !(values[2] is double) || !(values[3] is double))
		{
			return DependencyProperty.UnsetValue;
		}
		if (!(parameter is double) && !(parameter is string))
		{
			return DependencyProperty.UnsetValue;
		}
		if ((Visibility)values[0] == Visibility.Visible)
		{
			double value = ((!(parameter is string)) ? ((double)parameter) : double.Parse((string)parameter, NumberFormatInfo.InvariantInfo));
			double num = (double)values[1];
			double num2 = (double)values[2];
			double num3 = (double)values[3];
			if (num2 != num3 && DoubleUtil.AreClose(Math.Min(100.0, Math.Max(0.0, num * 100.0 / (num2 - num3))), value))
			{
				return Visibility.Collapsed;
			}
			return Visibility.Visible;
		}
		return Visibility.Collapsed;
	}

	/// <summary>Not supported.</summary>
	/// <returns>
	///   <see cref="F:System.Windows.Data.Binding.DoNothing" />
	/// </returns>
	/// <param name="value">This parameter is not used.</param>
	/// <param name="targetTypes">This parameter is not used.</param>
	/// <param name="parameter">This parameter is not used.</param>
	/// <param name="culture">This parameter is not used.</param>
	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
	{
		return new object[1] { Binding.DoNothing };
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.MenuScrollingVisibilityConverter" /> class.</summary>
	public MenuScrollingVisibilityConverter()
	{
	}
}
