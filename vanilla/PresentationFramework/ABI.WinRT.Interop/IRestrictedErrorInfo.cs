using System;
using System.Runtime.InteropServices;
using WinRT;
using WinRT.Interop;

namespace ABI.WinRT.Interop;

[Guid("82BA7092-4C88-427D-A7BC-16DD93FEB67E")]
internal class IRestrictedErrorInfo : global::WinRT.Interop.IRestrictedErrorInfo
{
	[Guid("82BA7092-4C88-427D-A7BC-16DD93FEB67E")]
	internal struct Vftbl
	{
		internal delegate int _GetErrorDetails(nint thisPtr, out nint description, out int error, out nint restrictedDescription, out nint capabilitySid);

		internal delegate int _GetReference(nint thisPtr, out nint reference);

		public IUnknownVftbl unknownVftbl;

		public _GetErrorDetails GetErrorDetails_0;

		public _GetReference GetReference_1;
	}

	protected readonly ObjectReference<Vftbl> _obj;

	public nint ThisPtr => _obj.ThisPtr;

	public static ObjectReference<Vftbl> FromAbi(nint thisPtr)
	{
		return ObjectReference<Vftbl>.FromAbi(thisPtr);
	}

	public static implicit operator IRestrictedErrorInfo(IObjectReference obj)
	{
		if (obj == null)
		{
			return null;
		}
		return new IRestrictedErrorInfo(obj);
	}

	public static implicit operator IRestrictedErrorInfo(ObjectReference<Vftbl> obj)
	{
		if (obj == null)
		{
			return null;
		}
		return new IRestrictedErrorInfo(obj);
	}

	public ObjectReference<I> AsInterface<I>()
	{
		return _obj.As<I>();
	}

	public A As<A>()
	{
		return _obj.AsType<A>();
	}

	public IRestrictedErrorInfo(IObjectReference obj)
		: this(obj.As<Vftbl>())
	{
	}

	public IRestrictedErrorInfo(ObjectReference<Vftbl> obj)
	{
		_obj = obj;
	}

	public void GetErrorDetails(out string description, out int error, out string restrictedDescription, out string capabilitySid)
	{
		nint description2 = IntPtr.Zero;
		nint restrictedDescription2 = IntPtr.Zero;
		nint capabilitySid2 = IntPtr.Zero;
		try
		{
			Marshal.ThrowExceptionForHR(_obj.Vftbl.GetErrorDetails_0(ThisPtr, out description2, out error, out restrictedDescription2, out capabilitySid2));
			description = ((description2 != IntPtr.Zero) ? Marshal.PtrToStringBSTR(description2) : string.Empty);
			restrictedDescription = ((restrictedDescription2 != IntPtr.Zero) ? Marshal.PtrToStringBSTR(restrictedDescription2) : string.Empty);
			capabilitySid = ((capabilitySid2 != IntPtr.Zero) ? Marshal.PtrToStringBSTR(capabilitySid2) : string.Empty);
		}
		finally
		{
			Marshal.FreeBSTR(description2);
			Marshal.FreeBSTR(restrictedDescription2);
			Marshal.FreeBSTR(capabilitySid2);
		}
	}

	public string GetReference()
	{
		nint reference = 0;
		try
		{
			Marshal.ThrowExceptionForHR(_obj.Vftbl.GetReference_1(ThisPtr, out reference));
			return (reference != IntPtr.Zero) ? Marshal.PtrToStringBSTR(reference) : string.Empty;
		}
		finally
		{
			Marshal.FreeBSTR(reference);
		}
	}
}
