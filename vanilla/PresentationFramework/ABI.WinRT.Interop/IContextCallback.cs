using System;
using System.Runtime.InteropServices;
using WinRT;
using WinRT.Interop;

namespace ABI.WinRT.Interop;

[Guid("000001da-0000-0000-C000-000000000046")]
internal class IContextCallback : global::WinRT.Interop.IContextCallback
{
	[Guid("000001da-0000-0000-C000-000000000046")]
	internal struct Vftbl
	{
		public unsafe delegate int _ContextCallback(nint pThis, PFNCONTEXTCALL pfnCallback, ComCallData* pData, ref Guid riid, int iMethod, nint pUnk);

		private IUnknownVftbl IUnknownVftbl;

		public _ContextCallback ContextCallback_4;
	}

	private struct ContextCallData
	{
		public nint delegateHandle;

		public unsafe ComCallData* userData;
	}

	protected readonly ObjectReference<Vftbl> _obj;

	public nint ThisPtr => _obj.ThisPtr;

	public static ObjectReference<Vftbl> FromAbi(nint thisPtr)
	{
		return ObjectReference<Vftbl>.FromAbi(thisPtr);
	}

	public static implicit operator IContextCallback(IObjectReference obj)
	{
		if (obj == null)
		{
			return null;
		}
		return new IContextCallback(obj);
	}

	public static implicit operator IContextCallback(ObjectReference<Vftbl> obj)
	{
		if (obj == null)
		{
			return null;
		}
		return new IContextCallback(obj);
	}

	public ObjectReference<I> AsInterface<I>()
	{
		return _obj.As<I>();
	}

	public A As<A>()
	{
		return _obj.AsType<A>();
	}

	public IContextCallback(IObjectReference obj)
		: this(obj.As<Vftbl>())
	{
	}

	public IContextCallback(ObjectReference<Vftbl> obj)
	{
		_obj = obj;
	}

	public unsafe void ContextCallback(PFNCONTEXTCALL pfnCallback, ComCallData* pParam, Guid riid, int iMethod)
	{
		Marshal.ThrowExceptionForHR(_obj.Vftbl.ContextCallback_4(ThisPtr, pfnCallback, pParam, ref riid, iMethod, IntPtr.Zero));
	}
}
