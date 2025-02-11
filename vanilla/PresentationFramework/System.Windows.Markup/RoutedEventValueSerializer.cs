using System.Collections.Generic;
using MS.Internal.WindowsBase;

namespace System.Windows.Markup;

internal class RoutedEventValueSerializer : ValueSerializer
{
	private static Dictionary<Type, Type> initializedTypes = new Dictionary<Type, Type>();

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
		if (value is RoutedEvent routedEvent)
		{
			ValueSerializer serializerFor = ValueSerializer.GetSerializerFor(typeof(Type), context);
			if (serializerFor != null)
			{
				return serializerFor.ConvertToString(routedEvent.OwnerType, context) + "." + routedEvent.Name;
			}
		}
		return base.ConvertToString(value, context);
	}

	private static void ForceTypeConstructors(Type currentType)
	{
		while (currentType != null && !initializedTypes.ContainsKey(currentType))
		{
			SecurityHelper.RunClassConstructor(currentType);
			initializedTypes[currentType] = currentType;
			currentType = currentType.BaseType;
		}
	}

	public override object ConvertFromString(string value, IValueSerializerContext context)
	{
		ValueSerializer serializerFor = ValueSerializer.GetSerializerFor(typeof(Type), context);
		if (serializerFor != null)
		{
			int num = value.IndexOf('.');
			if (num > 0)
			{
				Type type = serializerFor.ConvertFromString(value.Substring(0, num), context) as Type;
				string name = value.Substring(num + 1).Trim();
				ForceTypeConstructors(type);
				return EventManager.GetRoutedEventFromName(name, type);
			}
		}
		return base.ConvertFromString(value, context);
	}
}
