using System.Collections.Generic;
using System.Reflection;
using System.Windows.Markup;

namespace System.Windows.Input;

internal class CommandValueSerializer : ValueSerializer
{
	public override bool CanConvertToString(object value, IValueSerializerContext context)
	{
		if (context == null || context.GetValueSerializerFor(typeof(Type)) == null)
		{
			return false;
		}
		if (!(value is RoutedCommand routedCommand) || routedCommand.OwnerType == null)
		{
			return false;
		}
		if (CommandConverter.IsKnownType(routedCommand.OwnerType))
		{
			return true;
		}
		string name = routedCommand.Name + "Command";
		Type ownerType = routedCommand.OwnerType;
		_ = ownerType.Name;
		if (ownerType.GetProperty(name, BindingFlags.Static | BindingFlags.Public) != null)
		{
			return true;
		}
		if (ownerType.GetField(name, BindingFlags.Static | BindingFlags.Public) != null)
		{
			return true;
		}
		return false;
	}

	public override bool CanConvertFromString(string value, IValueSerializerContext context)
	{
		return true;
	}

	public override string ConvertToString(object value, IValueSerializerContext context)
	{
		if (value != null)
		{
			if (value is RoutedCommand routedCommand && null != routedCommand.OwnerType)
			{
				if (CommandConverter.IsKnownType(routedCommand.OwnerType))
				{
					return routedCommand.Name;
				}
				if (context == null)
				{
					throw new InvalidOperationException(SR.Format(SR.ValueSerializerContextUnavailable, GetType().Name));
				}
				return (context.GetValueSerializerFor(typeof(Type)) ?? throw new InvalidOperationException(SR.Format(SR.TypeValueSerializerUnavailable, GetType().Name))).ConvertToString(routedCommand.OwnerType, context) + "." + routedCommand.Name + "Command";
			}
			throw GetConvertToException(value, typeof(string));
		}
		return string.Empty;
	}

	public override IEnumerable<Type> TypeReferences(object value, IValueSerializerContext context)
	{
		if (value != null && value is RoutedCommand routedCommand && routedCommand.OwnerType != null && !CommandConverter.IsKnownType(routedCommand.OwnerType))
		{
			return new Type[1] { routedCommand.OwnerType };
		}
		return base.TypeReferences(value, context);
	}

	public override object ConvertFromString(string value, IValueSerializerContext context)
	{
		if (value != null)
		{
			if (value.Length == 0)
			{
				return null;
			}
			Type ownerType = null;
			int num = value.IndexOf('.');
			string localName;
			if (num >= 0)
			{
				string value2 = value.Substring(0, num);
				if (context == null)
				{
					throw new InvalidOperationException(SR.Format(SR.ValueSerializerContextUnavailable, GetType().Name));
				}
				ownerType = (context.GetValueSerializerFor(typeof(Type)) ?? throw new InvalidOperationException(SR.Format(SR.TypeValueSerializerUnavailable, GetType().Name))).ConvertFromString(value2, context) as Type;
				localName = value.Substring(num + 1).Trim();
			}
			else
			{
				localName = value.Trim();
			}
			ICommand command = CommandConverter.ConvertFromHelper(ownerType, localName);
			if (command != null)
			{
				return command;
			}
		}
		return base.ConvertFromString(value, context);
	}
}
