using System;
using System.Linq.Expressions;
using System.Reflection;

namespace WinRT;

internal class MarshalGeneric<T>
{
	protected static readonly Type HelperType;

	protected static readonly Type AbiType;

	protected static readonly Type MarshalerType;

	public static readonly Func<T, object> CreateMarshaler;

	public static readonly Func<object, object> GetAbi;

	public static readonly Action<object, nint> CopyAbi;

	public static readonly Func<object, T> FromAbi;

	public static readonly Func<T, object> FromManaged;

	public static readonly Action<T, nint> CopyManaged;

	public static readonly Action<object> DisposeMarshaler;

	static MarshalGeneric()
	{
		HelperType = typeof(T).GetHelperType();
		AbiType = typeof(T).GetAbiType();
		MarshalerType = typeof(T).GetMarshalerType();
		CreateMarshaler = BindCreateMarshaler();
		GetAbi = BindGetAbi();
		FromAbi = BindFromAbi();
		CopyAbi = BindCopyAbi();
		FromManaged = BindFromManaged();
		CopyManaged = BindCopyManaged();
		DisposeMarshaler = BindDisposeMarshaler();
	}

	private static Func<T, object> BindCreateMarshaler()
	{
		ParameterExpression[] array = new ParameterExpression[1] { Expression.Parameter(typeof(T), "arg") };
		MethodInfo? method = HelperType.GetMethod("CreateMarshaler", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		Expression[] arguments = array;
		return Expression.Lambda<Func<T, object>>(Expression.Convert(Expression.Call(method, arguments), typeof(object)), array).Compile();
	}

	private static Func<object, object> BindGetAbi()
	{
		ParameterExpression[] array = new ParameterExpression[1] { Expression.Parameter(typeof(object), "arg") };
		MethodInfo? method = HelperType.GetMethod("GetAbi", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		Expression[] arguments = new UnaryExpression[1] { Expression.Convert(array[0], MarshalerType) };
		return Expression.Lambda<Func<object, object>>(Expression.Convert(Expression.Call(method, arguments), typeof(object)), array).Compile();
	}

	private static Action<object, nint> BindCopyAbi()
	{
		MethodInfo method = HelperType.GetMethod("CopyAbi", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		if (method == null)
		{
			return null;
		}
		ParameterExpression[] array = new ParameterExpression[2]
		{
			Expression.Parameter(typeof(object), "arg"),
			Expression.Parameter(typeof(nint), "dest")
		};
		return Expression.Lambda<Action<object, nint>>(Expression.Call(method, new Expression[2]
		{
			Expression.Convert(array[0], MarshalerType),
			array[1]
		}), array).Compile();
	}

	private static Func<object, T> BindFromAbi()
	{
		ParameterExpression[] array = new ParameterExpression[1] { Expression.Parameter(typeof(object), "arg") };
		MethodInfo? method = HelperType.GetMethod("FromAbi", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		Expression[] arguments = new UnaryExpression[1] { Expression.Convert(array[0], AbiType) };
		return Expression.Lambda<Func<object, T>>(Expression.Call(method, arguments), array).Compile();
	}

	private static Func<T, object> BindFromManaged()
	{
		ParameterExpression[] array = new ParameterExpression[1] { Expression.Parameter(typeof(T), "arg") };
		MethodInfo? method = HelperType.GetMethod("FromManaged", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		Expression[] arguments = array;
		return Expression.Lambda<Func<T, object>>(Expression.Convert(Expression.Call(method, arguments), typeof(object)), array).Compile();
	}

	private static Action<T, nint> BindCopyManaged()
	{
		MethodInfo method = HelperType.GetMethod("CopyManaged", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		if (method == null)
		{
			return null;
		}
		ParameterExpression[] array = new ParameterExpression[2]
		{
			Expression.Parameter(typeof(T), "arg"),
			Expression.Parameter(typeof(nint), "dest")
		};
		Expression[] arguments = array;
		return Expression.Lambda<Action<T, nint>>(Expression.Call(method, arguments), array).Compile();
	}

	private static Action<object> BindDisposeMarshaler()
	{
		ParameterExpression[] array = new ParameterExpression[1] { Expression.Parameter(typeof(object), "arg") };
		MethodInfo? method = HelperType.GetMethod("DisposeMarshaler", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		Expression[] arguments = new UnaryExpression[1] { Expression.Convert(array[0], MarshalerType) };
		return Expression.Lambda<Action<object>>(Expression.Call(method, arguments), array).Compile();
	}
}
