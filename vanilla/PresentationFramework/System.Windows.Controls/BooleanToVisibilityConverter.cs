using System.Globalization;
using System.Windows.Data;

namespace System.Windows.Controls;

/// <summary>Represents the converter that converts Boolean values to and from <see cref="T:System.Windows.Visibility" /> enumeration values. </summary>
[Localizability(LocalizationCategory.NeverLocalize)]
public sealed class BooleanToVisibilityConverter : IValueConverter
{
	/// <summary>Converts a Boolean value to a <see cref="T:System.Windows.Visibility" /> enumeration value.</summary>
	/// <returns>
	///   <see cref="F:System.Windows.Visibility.Visible" /> if <paramref name="value" /> is true; otherwise, <see cref="F:System.Windows.Visibility.Collapsed" />.</returns>
	/// <param name="value">The Boolean value to convert. This value can be a standard Boolean value or a nullable Boolean value.</param>
	/// <param name="targetType">This parameter is not used.</param>
	/// <param name="parameter">This parameter is not used.</param>
	/// <param name="culture">This parameter is not used.</param>
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		bool flag = false;
		if (value is bool)
		{
			flag = (bool)value;
		}
		else if (value is bool? flag2)
		{
			flag = flag2.HasValue && flag2.Value;
		}
		return (!flag) ? Visibility.Collapsed : Visibility.Visible;
	}

	/// <summary>Converts a <see cref="T:System.Windows.Visibility" /> enumeration value to a Boolean value.</summary>
	/// <returns>true if <paramref name="value" /> is <see cref="F:System.Windows.Visibility.Visible" />; otherwise, false.</returns>
	/// <param name="value">A <see cref="T:System.Windows.Visibility" /> enumeration value. </param>
	/// <param name="targetType">This parameter is not used.</param>
	/// <param name="parameter">This parameter is not used.</param>
	/// <param name="culture">This parameter is not used.</param>
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is Visibility)
		{
			return (Visibility)value == Visibility.Visible;
		}
		return false;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.BooleanToVisibilityConverter" /> class.</summary>
	public BooleanToVisibilityConverter()
	{
	}
}
