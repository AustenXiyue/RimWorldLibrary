using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Baml2006;
using System.Windows.Data;
using System.Windows.Markup;

namespace MS.Internal.Data;

internal class DefaultValueConverter
{
	internal static readonly IValueConverter ValueConverterNotNeeded = new ObjectTargetConverter(typeof(object), null);

	protected Type _sourceType;

	protected Type _targetType;

	private TypeConverter _typeConverter;

	private bool _shouldConvertFrom;

	private bool _shouldConvertTo;

	private DataBindEngine _engine;

	private static Type StringType = typeof(string);

	protected DataBindEngine Engine => _engine;

	protected DefaultValueConverter(TypeConverter typeConverter, Type sourceType, Type targetType, bool shouldConvertFrom, bool shouldConvertTo, DataBindEngine engine)
	{
		_typeConverter = typeConverter;
		_sourceType = sourceType;
		_targetType = targetType;
		_shouldConvertFrom = shouldConvertFrom;
		_shouldConvertTo = shouldConvertTo;
		_engine = engine;
	}

	internal static IValueConverter Create(Type sourceType, Type targetType, bool targetToSource, DataBindEngine engine)
	{
		bool flag = false;
		bool flag2 = false;
		if (sourceType == targetType || (!targetToSource && targetType.IsAssignableFrom(sourceType)))
		{
			return ValueConverterNotNeeded;
		}
		if (targetType == typeof(object))
		{
			return new ObjectTargetConverter(sourceType, engine);
		}
		if (sourceType == typeof(object))
		{
			return new ObjectSourceConverter(targetType, engine);
		}
		if (SystemConvertConverter.CanConvert(sourceType, targetType))
		{
			return new SystemConvertConverter(sourceType, targetType);
		}
		Type underlyingType = Nullable.GetUnderlyingType(sourceType);
		if (underlyingType != null)
		{
			sourceType = underlyingType;
			flag = true;
		}
		underlyingType = Nullable.GetUnderlyingType(targetType);
		if (underlyingType != null)
		{
			targetType = underlyingType;
			flag2 = true;
		}
		if (flag || flag2)
		{
			return Create(sourceType, targetType, targetToSource, engine);
		}
		if (typeof(IListSource).IsAssignableFrom(sourceType) && targetType.IsAssignableFrom(typeof(IList)))
		{
			return new ListSourceConverter();
		}
		if (sourceType.IsInterface || targetType.IsInterface)
		{
			return new InterfaceConverter(sourceType, targetType);
		}
		TypeConverter converter = GetConverter(sourceType);
		bool flag3 = converter?.CanConvertTo(targetType) ?? false;
		bool flag4 = converter?.CanConvertFrom(targetType) ?? false;
		if ((flag3 || targetType.IsAssignableFrom(sourceType)) && (!targetToSource || flag4 || sourceType.IsAssignableFrom(targetType)))
		{
			return new SourceDefaultValueConverter(converter, sourceType, targetType, targetToSource && flag4, flag3, engine);
		}
		converter = GetConverter(targetType);
		flag3 = converter?.CanConvertTo(sourceType) ?? false;
		flag4 = converter?.CanConvertFrom(sourceType) ?? false;
		if ((flag4 || targetType.IsAssignableFrom(sourceType)) && (!targetToSource || flag3 || sourceType.IsAssignableFrom(targetType)))
		{
			return new TargetDefaultValueConverter(converter, sourceType, targetType, flag4, targetToSource && flag3, engine);
		}
		return null;
	}

	internal static TypeConverter GetConverter(Type type)
	{
		TypeConverter typeConverter = null;
		WpfKnownType wpfKnownType = XamlReader.BamlSharedSchemaContext.GetKnownXamlType(type) as WpfKnownType;
		if (wpfKnownType != null && wpfKnownType.TypeConverter != null)
		{
			typeConverter = wpfKnownType.TypeConverter.ConverterInstance;
		}
		if (typeConverter == null)
		{
			typeConverter = TypeDescriptor.GetConverter(type);
		}
		return typeConverter;
	}

	internal static object TryParse(object o, Type targetType, CultureInfo culture)
	{
		object result = DependencyProperty.UnsetValue;
		if (o is string text)
		{
			try
			{
				MethodInfo method;
				if (culture != null && (method = targetType.GetMethod("Parse", BindingFlags.Static | BindingFlags.Public, null, new Type[3]
				{
					StringType,
					typeof(NumberStyles),
					typeof(IFormatProvider)
				}, null)) != null)
				{
					result = method.Invoke(null, new object[3]
					{
						text,
						NumberStyles.Any,
						culture
					});
				}
				else if (culture != null && (method = targetType.GetMethod("Parse", BindingFlags.Static | BindingFlags.Public, null, new Type[2]
				{
					StringType,
					typeof(IFormatProvider)
				}, null)) != null)
				{
					result = method.Invoke(null, new object[2] { text, culture });
				}
				else if ((method = targetType.GetMethod("Parse", BindingFlags.Static | BindingFlags.Public, null, new Type[1] { StringType }, null)) != null)
				{
					result = method.Invoke(null, new object[1] { text });
				}
			}
			catch (TargetInvocationException)
			{
			}
		}
		return result;
	}

	protected object ConvertFrom(object o, Type destinationType, DependencyObject targetElement, CultureInfo culture)
	{
		return ConvertHelper(o, destinationType, targetElement, culture, isForward: false);
	}

	protected object ConvertTo(object o, Type destinationType, DependencyObject targetElement, CultureInfo culture)
	{
		return ConvertHelper(o, destinationType, targetElement, culture, isForward: true);
	}

	protected void EnsureConverter(Type type)
	{
		if (_typeConverter == null)
		{
			_typeConverter = GetConverter(type);
		}
	}

	private object ConvertHelper(object o, Type destinationType, DependencyObject targetElement, CultureInfo culture, bool isForward)
	{
		object obj = DependencyProperty.UnsetValue;
		bool flag = (isForward ? (!_shouldConvertTo) : (!_shouldConvertFrom));
		NotSupportedException ex = null;
		if (!flag)
		{
			obj = TryParse(o, destinationType, culture);
			if (obj == DependencyProperty.UnsetValue)
			{
				ValueConverterContext valueConverterContext = Engine.ValueConverterContext;
				if (valueConverterContext.IsInUse)
				{
					valueConverterContext = new ValueConverterContext();
				}
				try
				{
					valueConverterContext.SetTargetElement(targetElement);
					obj = ((!isForward) ? _typeConverter.ConvertFrom(valueConverterContext, culture, o) : _typeConverter.ConvertTo(valueConverterContext, culture, o, destinationType));
				}
				catch (NotSupportedException ex2)
				{
					flag = true;
					ex = ex2;
				}
				finally
				{
					valueConverterContext.SetTargetElement(null);
				}
			}
		}
		if (flag && ((o != null && destinationType.IsAssignableFrom(o.GetType())) || (o == null && !destinationType.IsValueType)))
		{
			obj = o;
			flag = false;
		}
		if (TraceData.IsEnabled)
		{
			if (culture != null && ex != null)
			{
				TraceData.TraceAndNotify(TraceEventType.Error, TraceData.DefaultValueConverterFailedForCulture(AvTrace.ToStringHelper(o), AvTrace.TypeName(o), destinationType.ToString(), culture), ex);
			}
			else if (flag)
			{
				TraceData.TraceAndNotify(TraceEventType.Error, TraceData.DefaultValueConverterFailed(AvTrace.ToStringHelper(o), AvTrace.TypeName(o), destinationType.ToString()), ex);
			}
		}
		if (flag && ex != null)
		{
			throw ex;
		}
		return obj;
	}
}
