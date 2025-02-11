using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Markup;
using MS.Internal;
using MS.Internal.Data;

namespace System.Windows;

internal struct TriggerCondition
{
	internal readonly DependencyProperty Property;

	internal readonly BindingBase Binding;

	internal readonly LogicalOp LogicalOp;

	internal readonly object Value;

	internal readonly string SourceName;

	internal int SourceChildIndex;

	internal BindingValueCache BindingValueCache;

	internal TriggerCondition(DependencyProperty dp, LogicalOp logicalOp, object value, string sourceName)
	{
		Property = dp;
		Binding = null;
		LogicalOp = logicalOp;
		Value = value;
		SourceName = sourceName;
		SourceChildIndex = 0;
		BindingValueCache = new BindingValueCache(null, null);
	}

	internal TriggerCondition(BindingBase binding, LogicalOp logicalOp, object value)
		: this(binding, logicalOp, value, "~Self")
	{
	}

	internal TriggerCondition(BindingBase binding, LogicalOp logicalOp, object value, string sourceName)
	{
		Property = null;
		Binding = binding;
		LogicalOp = logicalOp;
		Value = value;
		SourceName = sourceName;
		SourceChildIndex = 0;
		BindingValueCache = new BindingValueCache(null, null);
	}

	internal bool Match(object state)
	{
		return Match(state, Value);
	}

	private bool Match(object state, object referenceValue)
	{
		if (LogicalOp == LogicalOp.Equals)
		{
			return object.Equals(state, referenceValue);
		}
		return !object.Equals(state, referenceValue);
	}

	internal bool ConvertAndMatch(object state)
	{
		object obj = Value;
		string text = obj as string;
		Type type = state?.GetType();
		if (text != null && type != null && type != typeof(string))
		{
			BindingValueCache bindingValueCache = BindingValueCache;
			Type bindingValueType = bindingValueCache.BindingValueType;
			object obj2 = bindingValueCache.ValueAsBindingValueType;
			if (type != bindingValueType)
			{
				obj2 = obj;
				TypeConverter converter = DefaultValueConverter.GetConverter(type);
				if (converter != null && converter.CanConvertFrom(typeof(string)))
				{
					try
					{
						obj2 = converter.ConvertFromString(null, TypeConverterHelper.InvariantEnglishUS, text);
					}
					catch (Exception ex)
					{
						if (CriticalExceptions.IsCriticalApplicationException(ex))
						{
							throw;
						}
					}
					catch
					{
					}
				}
				bindingValueCache = new BindingValueCache(type, obj2);
				BindingValueCache = bindingValueCache;
			}
			obj = obj2;
		}
		return Match(state, obj);
	}

	internal bool TypeSpecificEquals(TriggerCondition value)
	{
		if (Property == value.Property && Binding == value.Binding && LogicalOp == value.LogicalOp && Value == value.Value && SourceName == value.SourceName)
		{
			return true;
		}
		return false;
	}
}
