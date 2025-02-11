using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;

namespace MS.Internal.Data;

internal class ListSourceConverter : IValueConverter
{
	public object Convert(object o, Type type, object parameter, CultureInfo culture)
	{
		IList result = null;
		if (o is IListSource listSource)
		{
			result = listSource.GetList();
		}
		return result;
	}

	public object ConvertBack(object o, Type type, object parameter, CultureInfo culture)
	{
		return null;
	}
}
