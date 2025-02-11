using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MS.Internal.Data;

internal class SourceDefaultValueConverter : DefaultValueConverter, IValueConverter
{
	public SourceDefaultValueConverter(TypeConverter typeConverter, Type sourceType, Type targetType, bool shouldConvertFrom, bool shouldConvertTo, DataBindEngine engine)
		: base(typeConverter, sourceType, targetType, shouldConvertFrom, shouldConvertTo, engine)
	{
	}

	public object Convert(object o, Type type, object parameter, CultureInfo culture)
	{
		return ConvertTo(o, _targetType, parameter as DependencyObject, culture);
	}

	public object ConvertBack(object o, Type type, object parameter, CultureInfo culture)
	{
		return ConvertFrom(o, _sourceType, parameter as DependencyObject, culture);
	}
}
