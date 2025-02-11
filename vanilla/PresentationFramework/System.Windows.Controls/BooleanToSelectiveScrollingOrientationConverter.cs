using System.Globalization;
using System.Windows.Data;

namespace System.Windows.Controls;

[Localizability(LocalizationCategory.NeverLocalize)]
internal sealed class BooleanToSelectiveScrollingOrientationConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is bool && parameter is SelectiveScrollingOrientation)
		{
			bool num = (bool)value;
			SelectiveScrollingOrientation selectiveScrollingOrientation = (SelectiveScrollingOrientation)parameter;
			if (num)
			{
				return selectiveScrollingOrientation;
			}
		}
		return SelectiveScrollingOrientation.Both;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
