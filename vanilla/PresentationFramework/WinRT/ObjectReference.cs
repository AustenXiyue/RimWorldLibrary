using System;
using System.Reflection;
using System.Runtime.InteropServices;
using WinRT.Interop;

namespace WinRT;

internal class ObjectReference<T> : IObjectReference
{
	private readonly IUnknownVftbl _vftblIUnknown;

	public readonly T Vftbl;

	protected override IUnknownVftbl VftblIUnknownUnsafe => _vftblIUnknown;

	public static ObjectReference<T> Attach(ref nint thisPtr)
	{
		if (thisPtr == IntPtr.Zero)
		{
			return null;
		}
		ObjectReference<T> result = new ObjectReference<T>(thisPtr);
		thisPtr = IntPtr.Zero;
		return result;
	}

	private ObjectReference(nint thisPtr, IUnknownVftbl vftblIUnknown, T vftblT)
		: base(thisPtr)
	{
		_vftblIUnknown = vftblIUnknown;
		Vftbl = vftblT;
	}

	private protected ObjectReference(nint thisPtr)
		: this(thisPtr, GetVtables(thisPtr))
	{
	}

	private ObjectReference(nint thisPtr, (IUnknownVftbl vftblIUnknown, T vftblT) vtables)
		: this(thisPtr, vtables.vftblIUnknown, vtables.vftblT)
	{
	}

	public static ObjectReference<T> FromAbi(nint thisPtr, IUnknownVftbl vftblIUnknown, T vftblT)
	{
		if (thisPtr == IntPtr.Zero)
		{
			return null;
		}
		ObjectReference<T> objectReference = new ObjectReference<T>(thisPtr, vftblIUnknown, vftblT);
		objectReference._vftblIUnknown.AddRef(objectReference.ThisPtr);
		return objectReference;
	}

	public static ObjectReference<T> FromAbi(nint thisPtr)
	{
		if (thisPtr == IntPtr.Zero)
		{
			return null;
		}
		var (vftblIUnknown, vftblT) = GetVtables(thisPtr);
		return FromAbi(thisPtr, vftblIUnknown, vftblT);
	}

	private static (IUnknownVftbl vftblIUnknown, T vftblT) GetVtables(nint thisPtr)
	{
		VftblPtr vftblPtr = Marshal.PtrToStructure<VftblPtr>(thisPtr);
		IUnknownVftbl item = Marshal.PtrToStructure<IUnknownVftbl>(vftblPtr.Vftbl);
		T item2 = (T)((!typeof(T).IsGenericType) ? ((object)Marshal.PtrToStructure<T>(vftblPtr.Vftbl)) : ((object)(T)typeof(T).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.CreateInstance, null, new Type[1] { typeof(nint) }, null).Invoke(new object[1] { thisPtr })));
		return (vftblIUnknown: item, vftblT: item2);
	}
}
