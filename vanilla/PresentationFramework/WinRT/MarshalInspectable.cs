using System;
using System.Reflection;
using WinRT.Interop;

namespace WinRT;

internal static class MarshalInspectable
{
	public static IObjectReference CreateMarshaler(object o, bool unwrapObject = true)
	{
		if (o == null)
		{
			return null;
		}
		if (unwrapObject && ComWrappersSupport.TryUnwrapObject(o, out var objRef))
		{
			return objRef.As<IInspectable.Vftbl>();
		}
		return ComWrappersSupport.CreateCCWForObject(o);
	}

	public static nint GetAbi(IObjectReference objRef)
	{
		if (objRef != null)
		{
			return MarshalInterfaceHelper<object>.GetAbi(objRef);
		}
		return IntPtr.Zero;
	}

	public static object FromAbi(nint ptr)
	{
		if (ptr == IntPtr.Zero)
		{
			return null;
		}
		using ObjectReference<IUnknownVftbl> objectReference = ObjectReference<IUnknownVftbl>.FromAbi(ptr);
		using ObjectReference<IUnknownVftbl> objectReference2 = objectReference.As<IUnknownVftbl>();
		if (objectReference2.IsReferenceToManagedObject)
		{
			return ComWrappersSupport.FindObject<object>(objectReference2.ThisPtr);
		}
		if (Projections.TryGetMarshalerTypeForProjectedRuntimeClass(objectReference, out var type))
		{
			return (type.GetMethod("FromAbi", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) ?? throw new MissingMethodException()).Invoke(null, new object[1] { ptr });
		}
		return ComWrappersSupport.CreateRcwForComObject(ptr);
	}

	public static void DisposeMarshaler(IObjectReference objRef)
	{
		MarshalInterfaceHelper<object>.DisposeMarshaler(objRef);
	}

	public static void DisposeAbi(nint ptr)
	{
		MarshalInterfaceHelper<object>.DisposeAbi(ptr);
	}

	public static nint FromManaged(object o, bool unwrapObject = true)
	{
		return CreateMarshaler(o, unwrapObject)?.GetRef() ?? IntPtr.Zero;
	}

	public unsafe static void CopyManaged(object o, nint dest, bool unwrapObject = true)
	{
		IObjectReference objectReference = CreateMarshaler(o, unwrapObject);
		*(nint*)((IntPtr)dest).ToPointer() = objectReference?.GetRef() ?? IntPtr.Zero;
	}

	public static MarshalInterfaceHelper<object>.MarshalerArray CreateMarshalerArray(object[] array)
	{
		return MarshalInterfaceHelper<object>.CreateMarshalerArray(array, (object o) => CreateMarshaler(o));
	}

	public static (int length, nint data) GetAbiArray(object box)
	{
		return MarshalInterfaceHelper<object>.GetAbiArray(box);
	}

	public static object[] FromAbiArray(object box)
	{
		return MarshalInterfaceHelper<object>.FromAbiArray(box, FromAbi);
	}

	public static (int length, nint data) FromManagedArray(object[] array)
	{
		return MarshalInterfaceHelper<object>.FromManagedArray(array, (object o) => FromManaged(o));
	}

	public static void CopyManagedArray(object[] array, nint data)
	{
		MarshalInterfaceHelper<object>.CopyManagedArray(array, data, delegate(object o, nint dest)
		{
			CopyManaged(o, dest);
		});
	}

	public static void DisposeMarshalerArray(object box)
	{
		MarshalInterfaceHelper<object>.DisposeMarshalerArray(box);
	}

	public static void DisposeAbiArray(object box)
	{
		MarshalInterfaceHelper<object>.DisposeAbiArray(box);
	}
}
