using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MS.Internal.Data;

internal class SystemConvertConverter : IValueConverter
{
	private Type _sourceType;

	private Type _targetType;

	private static readonly Type[] SupportedTypes = new Type[13]
	{
		typeof(string),
		typeof(int),
		typeof(long),
		typeof(float),
		typeof(double),
		typeof(decimal),
		typeof(bool),
		typeof(byte),
		typeof(short),
		typeof(uint),
		typeof(ulong),
		typeof(ushort),
		typeof(sbyte)
	};

	private static readonly Type[] CharSupportedTypes = new Type[9]
	{
		typeof(string),
		typeof(int),
		typeof(long),
		typeof(byte),
		typeof(short),
		typeof(uint),
		typeof(ulong),
		typeof(ushort),
		typeof(sbyte)
	};

	public SystemConvertConverter(Type sourceType, Type targetType)
	{
		_sourceType = sourceType;
		_targetType = targetType;
	}

	public object Convert(object o, Type type, object parameter, CultureInfo culture)
	{
		return System.Convert.ChangeType(o, _targetType, culture);
	}

	public object ConvertBack(object o, Type type, object parameter, CultureInfo culture)
	{
		object obj = DefaultValueConverter.TryParse(o, _sourceType, culture);
		if (obj == DependencyProperty.UnsetValue)
		{
			return System.Convert.ChangeType(o, _sourceType, culture);
		}
		return obj;
	}

	public static bool CanConvert(Type sourceType, Type targetType)
	{
		if (sourceType == typeof(DateTime))
		{
			return targetType == typeof(string);
		}
		if (targetType == typeof(DateTime))
		{
			return sourceType == typeof(string);
		}
		if (sourceType == typeof(char))
		{
			return CanConvertChar(targetType);
		}
		if (targetType == typeof(char))
		{
			return CanConvertChar(sourceType);
		}
		for (int i = 0; i < SupportedTypes.Length; i++)
		{
			if (sourceType == SupportedTypes[i])
			{
				for (i++; i < SupportedTypes.Length; i++)
				{
					if (targetType == SupportedTypes[i])
					{
						return true;
					}
				}
			}
			else
			{
				if (!(targetType == SupportedTypes[i]))
				{
					continue;
				}
				for (i++; i < SupportedTypes.Length; i++)
				{
					if (sourceType == SupportedTypes[i])
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private static bool CanConvertChar(Type type)
	{
		for (int i = 0; i < CharSupportedTypes.Length; i++)
		{
			if (type == CharSupportedTypes[i])
			{
				return true;
			}
		}
		return false;
	}
}
