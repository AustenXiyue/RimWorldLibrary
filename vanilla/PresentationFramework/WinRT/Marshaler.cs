using System;
using System.Collections.Generic;
using System.Reflection;

namespace WinRT;

internal class Marshaler<T>
{
	public static readonly Type AbiType;

	public static readonly Type RefAbiType;

	public static readonly Func<T, object> CreateMarshaler;

	public static readonly Func<object, object> GetAbi;

	public static readonly Action<object, nint> CopyAbi;

	public static readonly Func<object, T> FromAbi;

	public static readonly Func<T, object> FromManaged;

	public static readonly Action<T, nint> CopyManaged;

	public static readonly Action<object> DisposeMarshaler;

	public static readonly Action<object> DisposeAbi;

	public static readonly Func<T[], object> CreateMarshalerArray;

	public static readonly Func<object, (int, nint)> GetAbiArray;

	public static readonly Func<object, T[]> FromAbiArray;

	public static readonly Func<T[], (int, nint)> FromManagedArray;

	public static readonly Action<T[], nint> CopyManagedArray;

	public static readonly Action<object> DisposeMarshalerArray;

	public static readonly Action<object> DisposeAbiArray;

	static Marshaler()
	{
		Type typeFromHandle = typeof(T);
		if (typeFromHandle.IsArray)
		{
			throw new InvalidOperationException("Arrays may not be marshaled generically.");
		}
		if (typeFromHandle == typeof(string))
		{
			AbiType = typeof(nint);
			CreateMarshaler = (T value) => MarshalString.CreateMarshaler((string)(object)value);
			GetAbi = (object box) => MarshalString.GetAbi(box);
			FromAbi = (object value) => (T)(object)MarshalString.FromAbi((nint)value);
			FromManaged = (T value) => MarshalString.FromManaged((string)(object)value);
			DisposeMarshaler = delegate(object box)
			{
				MarshalString.DisposeMarshaler(box);
			};
			DisposeAbi = delegate(object box)
			{
				MarshalString.DisposeAbi(box);
			};
			CreateMarshalerArray = (T[] array) => MarshalString.CreateMarshalerArray((string[])(object)array);
			GetAbiArray = (object box) => ((int, nint))MarshalString.GetAbiArray(box);
			FromAbiArray = (object box) => (T[])(object)MarshalString.FromAbiArray(box);
			FromManagedArray = (T[] array) => ((int, nint))MarshalString.FromManagedArray((string[])(object)array);
			CopyManagedArray = delegate(T[] array, nint data)
			{
				MarshalString.CopyManagedArray((string[])(object)array, data);
			};
			DisposeMarshalerArray = delegate(object box)
			{
				MarshalString.DisposeMarshalerArray(box);
			};
			DisposeAbiArray = delegate(object box)
			{
				MarshalString.DisposeAbiArray(box);
			};
		}
		else if (typeFromHandle.IsGenericType && typeFromHandle.GetGenericTypeDefinition() == typeof(KeyValuePair<, >))
		{
			AbiType = typeof(nint);
			CreateMarshaler = MarshalGeneric<T>.CreateMarshaler;
			GetAbi = MarshalGeneric<T>.GetAbi;
			CopyAbi = MarshalGeneric<T>.CopyAbi;
			FromAbi = MarshalGeneric<T>.FromAbi;
			FromManaged = MarshalGeneric<T>.FromManaged;
			CopyManaged = MarshalGeneric<T>.CopyManaged;
			DisposeMarshaler = MarshalGeneric<T>.DisposeMarshaler;
			DisposeAbi = delegate
			{
			};
		}
		else if (typeFromHandle.IsValueType || typeFromHandle == typeof(Type))
		{
			AbiType = typeFromHandle.FindHelperType();
			if (AbiType != null && AbiType.GetMethod("FromAbi", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) == null)
			{
				AbiType = null;
			}
			if (AbiType == null)
			{
				AbiType = typeFromHandle;
				CreateMarshaler = (T value) => value;
				GetAbi = (object box) => box;
				FromAbi = (object value) => (T)value;
				FromManaged = (T value) => value;
				DisposeMarshaler = delegate
				{
				};
				DisposeAbi = delegate
				{
				};
				CreateMarshalerArray = (T[] array) => MarshalBlittable<T>.CreateMarshalerArray(array);
				GetAbiArray = (object box) => ((int, nint))MarshalBlittable<T>.GetAbiArray(box);
				FromAbiArray = (object box) => MarshalBlittable<T>.FromAbiArray(box);
				FromManagedArray = (T[] array) => ((int, nint))MarshalBlittable<T>.FromManagedArray(array);
				CopyManagedArray = delegate(T[] array, nint data)
				{
					MarshalBlittable<T>.CopyManagedArray(array, data);
				};
				DisposeMarshalerArray = delegate(object box)
				{
					MarshalBlittable<T>.DisposeMarshalerArray(box);
				};
				DisposeAbiArray = delegate(object box)
				{
					MarshalBlittable<T>.DisposeAbiArray(box);
				};
			}
			else
			{
				CreateMarshaler = MarshalGeneric<T>.CreateMarshaler;
				GetAbi = MarshalGeneric<T>.GetAbi;
				CopyAbi = MarshalGeneric<T>.CopyAbi;
				FromAbi = MarshalGeneric<T>.FromAbi;
				FromManaged = MarshalGeneric<T>.FromManaged;
				CopyManaged = MarshalGeneric<T>.CopyManaged;
				DisposeMarshaler = MarshalGeneric<T>.DisposeMarshaler;
				DisposeAbi = delegate
				{
				};
				CreateMarshalerArray = (T[] array) => MarshalNonBlittable<T>.CreateMarshalerArray(array);
				GetAbiArray = (object box) => ((int, nint))MarshalNonBlittable<T>.GetAbiArray(box);
				FromAbiArray = (object box) => MarshalNonBlittable<T>.FromAbiArray(box);
				FromManagedArray = (T[] array) => ((int, nint))MarshalNonBlittable<T>.FromManagedArray(array);
				CopyManagedArray = delegate(T[] array, nint data)
				{
					MarshalNonBlittable<T>.CopyManagedArray(array, data);
				};
				DisposeMarshalerArray = delegate(object box)
				{
					MarshalNonBlittable<T>.DisposeMarshalerArray(box);
				};
				DisposeAbiArray = delegate(object box)
				{
					MarshalNonBlittable<T>.DisposeAbiArray(box);
				};
			}
		}
		else if (typeFromHandle.IsInterface)
		{
			AbiType = typeof(nint);
			CreateMarshaler = (T value) => MarshalInterface<T>.CreateMarshaler(value);
			GetAbi = (object objRef) => MarshalInterface<T>.GetAbi((IObjectReference)objRef);
			FromAbi = (object value) => (T)(object)MarshalInterface<T>.FromAbi((nint)value);
			FromManaged = (T value) => ((IObjectReference)CreateMarshaler(value)).GetRef();
			DisposeMarshaler = delegate(object objRef)
			{
				MarshalInterface<T>.DisposeMarshaler((IObjectReference)objRef);
			};
			DisposeAbi = delegate(object box)
			{
				MarshalInterface<T>.DisposeAbi((nint)box);
			};
		}
		else if (typeof(T) == typeof(object))
		{
			AbiType = typeof(nint);
			CreateMarshaler = (T value) => MarshalInspectable.CreateMarshaler(value);
			GetAbi = (object objRef) => MarshalInspectable.GetAbi((IObjectReference)objRef);
			FromAbi = (object box) => (T)MarshalInspectable.FromAbi((nint)box);
			FromManaged = (T value) => MarshalInspectable.FromManaged(value);
			CopyManaged = delegate(T value, nint dest)
			{
				MarshalInspectable.CopyManaged(value, dest);
			};
			DisposeMarshaler = delegate(object objRef)
			{
				MarshalInspectable.DisposeMarshaler((IObjectReference)objRef);
			};
			DisposeAbi = delegate(object box)
			{
				MarshalInspectable.DisposeAbi((nint)box);
			};
		}
		else
		{
			AbiType = typeof(nint);
			CreateMarshaler = MarshalGeneric<T>.CreateMarshaler;
			GetAbi = MarshalGeneric<T>.GetAbi;
			FromAbi = MarshalGeneric<T>.FromAbi;
			FromManaged = MarshalGeneric<T>.FromManaged;
			CopyManaged = MarshalGeneric<T>.CopyManaged;
			DisposeMarshaler = MarshalGeneric<T>.DisposeMarshaler;
			DisposeAbi = delegate
			{
			};
		}
		RefAbiType = AbiType.MakeByRefType();
	}
}
