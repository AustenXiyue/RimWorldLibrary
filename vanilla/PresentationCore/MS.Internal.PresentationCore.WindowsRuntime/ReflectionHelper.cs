using System;
using System.Reflection;

namespace MS.Internal.PresentationCore.WindowsRuntime;

internal static class ReflectionHelper
{
	public static TResult ReflectionStaticCall<TResult>(this Type type, string methodName)
	{
		MethodInfo? method = type.GetMethod(methodName, Type.EmptyTypes);
		if (method == null)
		{
			throw new MissingMethodException(methodName);
		}
		return (TResult)method.Invoke(null, null);
	}

	public static TResult ReflectionStaticCall<TResult, TArg>(this Type type, string methodName, TArg arg)
	{
		MethodInfo? method = type.GetMethod(methodName, new Type[1] { typeof(TArg) });
		if (method == null)
		{
			throw new MissingMethodException(methodName);
		}
		return (TResult)method.Invoke(null, new object[1] { arg });
	}

	public static TResult ReflectionCall<TResult>(this object obj, string methodName)
	{
		return (TResult)obj.ReflectionCall(methodName);
	}

	public static object ReflectionCall(this object obj, string methodName)
	{
		MethodInfo? method = obj.GetType().GetMethod(methodName, Type.EmptyTypes);
		if (method == null)
		{
			throw new MissingMethodException(methodName);
		}
		return method.Invoke(obj, null);
	}

	public static object ReflectionCall<TArg1>(this object obj, string methodName, TArg1 arg1)
	{
		MethodInfo? method = obj.GetType().GetMethod(methodName, new Type[1] { typeof(TArg1) });
		if (method == null)
		{
			throw new MissingMethodException(methodName);
		}
		return method.Invoke(obj, new object[1] { arg1 });
	}

	public static TResult ReflectionCall<TResult, TArg1>(this object obj, string methodName, TArg1 arg1)
	{
		return (TResult)obj.ReflectionCall(methodName, arg1);
	}

	public static object ReflectionCall<TArg1, TArg2>(this object obj, string methodName, TArg1 arg1, TArg2 arg2)
	{
		MethodInfo? method = obj.GetType().GetMethod(methodName, new Type[2]
		{
			typeof(TArg1),
			typeof(TArg2)
		});
		if (method == null)
		{
			throw new MissingMethodException(methodName);
		}
		return method.Invoke(obj, new object[2] { arg1, arg2 });
	}

	public static TResult ReflectionCall<TResult, TArg1, TArg2>(this object obj, string methodName, TArg1 arg1, TArg2 arg2)
	{
		return (TResult)obj.ReflectionCall(methodName, arg1, arg2);
	}

	public static TResult ReflectionGetField<TResult>(this object obj, string fieldName)
	{
		FieldInfo? field = obj.GetType().GetField(fieldName);
		if (field == null)
		{
			throw new MissingFieldException(fieldName);
		}
		return (TResult)field.GetValue(obj);
	}

	public static object ReflectionNew(this Type type)
	{
		ConstructorInfo? constructor = type.GetConstructor(Type.EmptyTypes);
		if (constructor == null)
		{
			throw new MissingMethodException(type.FullName + "." + type.Name + "()");
		}
		return constructor.Invoke(null);
	}

	public static object ReflectionNew<TArg1>(this Type type, TArg1 arg1)
	{
		ConstructorInfo? constructor = type.GetConstructor(new Type[1] { typeof(TArg1) });
		if (constructor == null)
		{
			throw new MissingMethodException($"{type.FullName}.{type.Name}({typeof(TArg1).Name})");
		}
		return constructor.Invoke(new object[1] { arg1 });
	}

	public static object ReflectionNew<TArg1, TArg2>(this Type type, TArg1 arg1, TArg2 arg2)
	{
		ConstructorInfo constructor = type.GetConstructor(new Type[2]
		{
			typeof(TArg1),
			typeof(TArg2)
		});
		if (constructor == null)
		{
			throw new MissingMethodException($"{type.FullName}.{type.Name}({typeof(TArg1).Name},{typeof(TArg2).Name})");
		}
		return constructor.Invoke(new object[2] { arg1, arg2 });
	}

	public static TResult ReflectionGetProperty<TResult>(this object obj, string propertyName)
	{
		PropertyInfo? property = obj.GetType().GetProperty(propertyName);
		if (property == null)
		{
			throw new MissingMemberException(propertyName);
		}
		return (TResult)property.GetValue(obj);
	}

	public static object ReflectionGetProperty(this object obj, string propertyName)
	{
		return obj.ReflectionGetProperty<object>(propertyName);
	}

	public static TResult ReflectionStaticGetProperty<TResult>(this Type type, string propertyName)
	{
		PropertyInfo? property = type.GetProperty(propertyName, BindingFlags.Static);
		if (property == null)
		{
			throw new MissingMemberException(propertyName);
		}
		return (TResult)property.GetValue(null);
	}
}
