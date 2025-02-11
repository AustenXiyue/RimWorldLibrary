using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MS.Internal.Data;

internal class ObjectTargetConverter : DefaultValueConverter, IValueConverter
{
	public ObjectTargetConverter(Type sourceType, DataBindEngine engine)
		: base(null, sourceType, typeof(object), shouldConvertFrom: true, shouldConvertTo: false, engine)
	{
	}

	public object Convert(object o, Type type, object parameter, CultureInfo culture)
	{
		return o;
	}

	public object ConvertBack(object o, Type type, object parameter, CultureInfo culture)
	{
		if (o == null && !_sourceType.IsValueType)
		{
			return o;
		}
		if (o != null && _sourceType.IsAssignableFrom(o.GetType()))
		{
			return o;
		}
		if (_sourceType == typeof(string))
		{
			return string.Format(culture, "{0}", o);
		}
		EnsureConverter(_sourceType);
		return ConvertFrom(o, _sourceType, parameter as DependencyObject, culture);
	}
}
