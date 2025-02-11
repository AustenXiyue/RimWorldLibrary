using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MS.Internal.Data;

internal class DynamicValueConverter : IValueConverter
{
	private Type _sourceType;

	private Type _targetType;

	private IValueConverter _converter;

	private bool _targetToSourceNeeded;

	private DataBindEngine _engine;

	internal DynamicValueConverter(bool targetToSourceNeeded)
	{
		_targetToSourceNeeded = targetToSourceNeeded;
	}

	internal DynamicValueConverter(bool targetToSourceNeeded, Type sourceType, Type targetType)
	{
		_targetToSourceNeeded = targetToSourceNeeded;
		EnsureConverter(sourceType, targetType);
	}

	internal object Convert(object value, Type targetType)
	{
		return Convert(value, targetType, null, CultureInfo.InvariantCulture);
	}

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		object result = DependencyProperty.UnsetValue;
		if (value != null)
		{
			Type type = value.GetType();
			EnsureConverter(type, targetType);
			if (_converter != null)
			{
				result = _converter.Convert(value, targetType, parameter, culture);
			}
		}
		else if (!targetType.IsValueType)
		{
			result = null;
		}
		return result;
	}

	public object ConvertBack(object value, Type sourceType, object parameter, CultureInfo culture)
	{
		object result = DependencyProperty.UnsetValue;
		if (value != null)
		{
			Type type = value.GetType();
			EnsureConverter(sourceType, type);
			if (_converter != null)
			{
				result = _converter.ConvertBack(value, sourceType, parameter, culture);
			}
		}
		else if (!sourceType.IsValueType)
		{
			result = null;
		}
		return result;
	}

	private void EnsureConverter(Type sourceType, Type targetType)
	{
		if (!(_sourceType != sourceType) && !(_targetType != targetType))
		{
			return;
		}
		if (sourceType != null && targetType != null)
		{
			if (_engine == null)
			{
				_engine = DataBindEngine.CurrentDataBindEngine;
			}
			Invariant.Assert(_engine != null);
			_converter = _engine.GetDefaultValueConverter(sourceType, targetType, _targetToSourceNeeded);
		}
		else
		{
			_converter = null;
		}
		_sourceType = sourceType;
		_targetType = targetType;
	}
}
