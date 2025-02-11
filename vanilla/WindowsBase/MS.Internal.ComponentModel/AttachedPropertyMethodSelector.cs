using System;
using System.Globalization;
using System.Reflection;

namespace MS.Internal.ComponentModel;

internal class AttachedPropertyMethodSelector : Binder
{
	public override MethodBase SelectMethod(BindingFlags bindingAttr, MethodBase[] match, Type[] types, ParameterModifier[] modifiers)
	{
		if (types == null)
		{
			if (match.Length > 1)
			{
				throw new AmbiguousMatchException();
			}
			return match[0];
		}
		foreach (MethodBase methodBase in match)
		{
			if (ParametersMatch(methodBase.GetParameters(), types))
			{
				return methodBase;
			}
		}
		return null;
	}

	private static bool ParametersMatch(ParameterInfo[] parameters, Type[] types)
	{
		if (parameters.Length != types.Length)
		{
			return false;
		}
		bool flag = true;
		for (int i = 0; i < parameters.Length; i++)
		{
			ParameterInfo obj = parameters[i];
			Type type = types[i];
			if (obj.ParameterType != type)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			return true;
		}
		flag = true;
		for (int j = 0; j < parameters.Length; j++)
		{
			ParameterInfo parameterInfo = parameters[j];
			if (!types[j].IsAssignableFrom(parameterInfo.ParameterType))
			{
				flag = false;
				break;
			}
		}
		return flag;
	}

	public override MethodBase BindToMethod(BindingFlags bindingAttr, MethodBase[] match, ref object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] names, out object state)
	{
		throw new NotImplementedException();
	}

	public override FieldInfo BindToField(BindingFlags bindingAttr, FieldInfo[] match, object value, CultureInfo culture)
	{
		throw new NotImplementedException();
	}

	public override object ChangeType(object value, Type type, CultureInfo culture)
	{
		throw new NotImplementedException();
	}

	public override void ReorderArgumentArray(ref object[] args, object state)
	{
		throw new NotImplementedException();
	}

	public override PropertyInfo SelectProperty(BindingFlags bindingAttr, PropertyInfo[] match, Type returnType, Type[] indexes, ParameterModifier[] modifiers)
	{
		throw new NotImplementedException();
	}
}
