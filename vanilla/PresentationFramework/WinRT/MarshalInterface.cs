using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;

namespace WinRT;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct MarshalInterface<T>
{
	private static readonly Type HelperType = typeof(T).GetHelperType();

	private static Func<T, IObjectReference> _ToAbi;

	private static Func<nint, T> _FromAbi;

	private static Func<IObjectReference, IObjectReference> _As;

	public static T FromAbi(nint ptr)
	{
		if (ptr == IntPtr.Zero)
		{
			return (T)(object)null;
		}
		object obj = MarshalInspectable.FromAbi(ptr);
		if (obj is T)
		{
			return (T)obj;
		}
		if (_FromAbi == null)
		{
			_FromAbi = BindFromAbi();
		}
		return _FromAbi(ptr);
	}

	public static IObjectReference CreateMarshaler(T value)
	{
		if (value == null)
		{
			return null;
		}
		if (value.GetType() == HelperType)
		{
			if (_ToAbi == null)
			{
				_ToAbi = BindToAbi();
			}
			return _ToAbi(value);
		}
		if (_As == null)
		{
			_As = BindAs();
		}
		IObjectReference arg = MarshalInspectable.CreateMarshaler(value);
		return _As(arg);
	}

	public static nint GetAbi(IObjectReference value)
	{
		if (value != null)
		{
			return MarshalInterfaceHelper<T>.GetAbi(value);
		}
		return IntPtr.Zero;
	}

	public static void DisposeAbi(nint thisPtr)
	{
		MarshalInterfaceHelper<T>.DisposeAbi(thisPtr);
	}

	public static void DisposeMarshaler(IObjectReference value)
	{
		MarshalInterfaceHelper<T>.DisposeMarshaler(value);
	}

	public static nint FromManaged(T value)
	{
		if (value == null)
		{
			return IntPtr.Zero;
		}
		return CreateMarshaler(value).GetRef();
	}

	public unsafe static void CopyManaged(T value, nint dest)
	{
		*(nint*)((IntPtr)dest).ToPointer() = ((value == null) ? IntPtr.Zero : CreateMarshaler(value).GetRef());
	}

	public static MarshalInterfaceHelper<T>.MarshalerArray CreateMarshalerArray(T[] array)
	{
		return MarshalInterfaceHelper<T>.CreateMarshalerArray(array, (T o) => CreateMarshaler(o));
	}

	public static (int length, nint data) GetAbiArray(object box)
	{
		return MarshalInterfaceHelper<T>.GetAbiArray(box);
	}

	public static T[] FromAbiArray(object box)
	{
		return MarshalInterfaceHelper<T>.FromAbiArray(box, FromAbi);
	}

	public static (int length, nint data) FromManagedArray(T[] array)
	{
		return MarshalInterfaceHelper<T>.FromManagedArray(array, (T o) => FromManaged(o));
	}

	public static void CopyManagedArray(T[] array, nint data)
	{
		MarshalInterfaceHelper<T>.CopyManagedArray(array, data, delegate(T o, nint dest)
		{
			CopyManaged(o, dest);
		});
	}

	public static void DisposeMarshalerArray(object box)
	{
		MarshalInterfaceHelper<T>.DisposeMarshalerArray(box);
	}

	public static void DisposeAbiArray(object box)
	{
		MarshalInterfaceHelper<T>.DisposeAbiArray(box);
	}

	private static Func<nint, T> BindFromAbi()
	{
		MethodInfo method = HelperType.GetMethod("FromAbi", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		ConstructorInfo constructor = HelperType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.CreateInstance, null, new Type[1] { method.ReturnType }, null);
		ParameterExpression[] array = new ParameterExpression[1] { Expression.Parameter(typeof(nint), "arg") };
		return Expression.Lambda<Func<nint, T>>(Expression.New(constructor, Expression.Call(method, array[0])), array).Compile();
	}

	private static Func<T, IObjectReference> BindToAbi()
	{
		ParameterExpression[] array = new ParameterExpression[1] { Expression.Parameter(typeof(T), "arg") };
		return Expression.Lambda<Func<T, IObjectReference>>(Expression.MakeMemberAccess(Expression.Convert(array[0], HelperType), HelperType.GetField("_obj", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic)), array).Compile();
	}

	private static Func<IObjectReference, IObjectReference> BindAs()
	{
		Type helperType = typeof(T).GetHelperType();
		ParameterExpression[] array = new ParameterExpression[1] { Expression.Parameter(typeof(IObjectReference), "arg") };
		return Expression.Lambda<Func<IObjectReference, IObjectReference>>(Expression.Call(array[0], typeof(IObjectReference).GetMethod("As", Type.EmptyTypes).MakeGenericMethod(helperType.FindVftblType())), array).Compile();
	}
}
