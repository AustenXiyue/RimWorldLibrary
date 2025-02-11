using System.Collections.Generic;
using System.Windows.Markup;

namespace System.Windows;

internal class DependencyPropertyValueSerializer : ValueSerializer
{
	public override bool CanConvertToString(object value, IValueSerializerContext context)
	{
		return ValueSerializer.GetSerializerFor(typeof(Type), context) != null;
	}

	public override bool CanConvertFromString(string value, IValueSerializerContext context)
	{
		return ValueSerializer.GetSerializerFor(typeof(Type), context) != null;
	}

	public override string ConvertToString(object value, IValueSerializerContext context)
	{
		if (value is DependencyProperty dependencyProperty)
		{
			ValueSerializer serializerFor = ValueSerializer.GetSerializerFor(typeof(Type), context);
			if (serializerFor != null)
			{
				return serializerFor.ConvertToString(dependencyProperty.OwnerType, context) + "." + dependencyProperty.Name;
			}
		}
		throw GetConvertToException(value, typeof(string));
	}

	public override IEnumerable<Type> TypeReferences(object value, IValueSerializerContext context)
	{
		if (value is DependencyProperty dependencyProperty)
		{
			return new Type[1] { dependencyProperty.OwnerType };
		}
		return base.TypeReferences(value, context);
	}

	public override object ConvertFromString(string value, IValueSerializerContext context)
	{
		ValueSerializer serializerFor = ValueSerializer.GetSerializerFor(typeof(Type), context);
		if (serializerFor != null)
		{
			int num = value.IndexOf('.');
			if (num >= 0)
			{
				string text = value.Substring(0, num - 1);
				Type type = serializerFor.ConvertFromString(text, context) as Type;
				if (type != null)
				{
					return DependencyProperty.FromName(text, type);
				}
			}
		}
		throw GetConvertFromException(value);
	}
}
