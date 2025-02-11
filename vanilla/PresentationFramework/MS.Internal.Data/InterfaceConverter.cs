using System;
using System.Globalization;
using System.Windows.Data;

namespace MS.Internal.Data;

internal class InterfaceConverter : IValueConverter
{
	private Type _sourceType;

	private Type _targetType;

	internal InterfaceConverter(Type sourceType, Type targetType)
	{
		_sourceType = sourceType;
		_targetType = targetType;
	}

	public object Convert(object o, Type type, object parameter, CultureInfo culture)
	{
		return ConvertTo(o, _targetType);
	}

	public object ConvertBack(object o, Type type, object parameter, CultureInfo culture)
	{
		return ConvertTo(o, _sourceType);
	}

	private object ConvertTo(object o, Type type)
	{
		if (!type.IsInstanceOfType(o))
		{
			return null;
		}
		return o;
	}
}
