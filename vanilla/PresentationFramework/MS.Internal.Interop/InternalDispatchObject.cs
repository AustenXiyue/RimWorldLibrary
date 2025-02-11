using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MS.Internal.Interop;

internal abstract class InternalDispatchObject<IDispInterface> : IReflect
{
	private Dictionary<int, MethodInfo> _dispId2MethodMap;

	Type IReflect.UnderlyingSystemType => typeof(IDispInterface);

	protected InternalDispatchObject()
	{
		MethodInfo[] methods = typeof(IDispInterface).GetMethods();
		_dispId2MethodMap = new Dictionary<int, MethodInfo>(methods.Length);
		MethodInfo[] array = methods;
		foreach (MethodInfo methodInfo in array)
		{
			int value = ((DispIdAttribute[])methodInfo.GetCustomAttributes(typeof(DispIdAttribute), inherit: false))[0].Value;
			_dispId2MethodMap[value] = methodInfo;
		}
	}

	FieldInfo IReflect.GetField(string name, BindingFlags bindingAttr)
	{
		throw new NotImplementedException();
	}

	FieldInfo[] IReflect.GetFields(BindingFlags bindingAttr)
	{
		return null;
	}

	MemberInfo[] IReflect.GetMember(string name, BindingFlags bindingAttr)
	{
		throw new NotImplementedException();
	}

	MemberInfo[] IReflect.GetMembers(BindingFlags bindingAttr)
	{
		throw new NotImplementedException();
	}

	MethodInfo IReflect.GetMethod(string name, BindingFlags bindingAttr)
	{
		throw new NotImplementedException();
	}

	MethodInfo IReflect.GetMethod(string name, BindingFlags bindingAttr, Binder binder, Type[] types, ParameterModifier[] modifiers)
	{
		throw new NotImplementedException();
	}

	MethodInfo[] IReflect.GetMethods(BindingFlags bindingAttr)
	{
		return null;
	}

	PropertyInfo[] IReflect.GetProperties(BindingFlags bindingAttr)
	{
		return null;
	}

	PropertyInfo IReflect.GetProperty(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
	{
		throw new NotImplementedException();
	}

	PropertyInfo IReflect.GetProperty(string name, BindingFlags bindingAttr)
	{
		throw new NotImplementedException();
	}

	object IReflect.InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
	{
		if (name.StartsWith("[DISPID=", StringComparison.OrdinalIgnoreCase))
		{
			int key = int.Parse(name.Substring(8, name.Length - 9), CultureInfo.InvariantCulture);
			if (_dispId2MethodMap.TryGetValue(key, out var value))
			{
				return value.Invoke(this, invokeAttr, binder, args, culture);
			}
		}
		throw new MissingMethodException(GetType().Name, name);
	}
}
