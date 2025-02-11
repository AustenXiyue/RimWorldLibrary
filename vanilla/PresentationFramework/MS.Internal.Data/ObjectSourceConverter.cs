using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MS.Internal.Data;

internal class ObjectSourceConverter : DefaultValueConverter, IValueConverter
{
	public ObjectSourceConverter(Type targetType, DataBindEngine engine)
		: base(null, typeof(object), targetType, shouldConvertFrom: true, shouldConvertTo: false, engine)
	{
	}

	public object Convert(object o, Type type, object parameter, CultureInfo culture)
	{
		if ((o != null && _targetType.IsAssignableFrom(o.GetType())) || (o == null && !_targetType.IsValueType))
		{
			return o;
		}
		if (_targetType == typeof(string))
		{
			return string.Format(culture, "{0}", o);
		}
		EnsureConverter(_targetType);
		return ConvertFrom(o, _targetType, parameter as DependencyObject, culture);
	}

	public object ConvertBack(object o, Type type, object parameter, CultureInfo culture)
	{
		return o;
	}
}
