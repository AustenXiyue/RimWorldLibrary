using System.Globalization;
using System.Windows.Data;

namespace System.Windows.Controls;

[Localizability(LocalizationCategory.NeverLocalize)]
internal sealed class DataGridHeadersVisibilityToVisibilityConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		bool flag = false;
		if (value is DataGridHeadersVisibility && parameter is DataGridHeadersVisibility)
		{
			DataGridHeadersVisibility dataGridHeadersVisibility = (DataGridHeadersVisibility)value;
			DataGridHeadersVisibility dataGridHeadersVisibility2 = (DataGridHeadersVisibility)parameter;
			switch (dataGridHeadersVisibility)
			{
			case DataGridHeadersVisibility.All:
				flag = true;
				break;
			case DataGridHeadersVisibility.Column:
				flag = dataGridHeadersVisibility2 == DataGridHeadersVisibility.Column || dataGridHeadersVisibility2 == DataGridHeadersVisibility.None;
				break;
			case DataGridHeadersVisibility.Row:
				flag = dataGridHeadersVisibility2 == DataGridHeadersVisibility.Row || dataGridHeadersVisibility2 == DataGridHeadersVisibility.None;
				break;
			}
		}
		if (targetType == typeof(Visibility))
		{
			return (!flag) ? Visibility.Collapsed : Visibility.Visible;
		}
		return DependencyProperty.UnsetValue;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
