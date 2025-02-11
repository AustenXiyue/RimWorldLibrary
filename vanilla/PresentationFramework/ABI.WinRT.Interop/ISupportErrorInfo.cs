using System;
using System.Runtime.InteropServices;
using WinRT;
using WinRT.Interop;

namespace ABI.WinRT.Interop;

[Guid("DF0B3D60-548F-101B-8E65-08002B2BD119")]
internal class ISupportErrorInfo : global::WinRT.Interop.ISupportErrorInfo
{
	[Guid("DF0B3D60-548F-101B-8E65-08002B2BD119")]
	internal struct Vftbl
	{
		internal delegate int _InterfaceSupportsErrorInfo(nint thisPtr, ref Guid riid);

		public IUnknownVftbl IUnknownVftbl;

		public _InterfaceSupportsErrorInfo InterfaceSupportsErrorInfo_0;

		private static readonly Vftbl AbiToProjectionVftable;

		public static readonly nint AbiToProjectionVftablePtr;

		unsafe static Vftbl()
		{
			AbiToProjectionVftable = new Vftbl
			{
				IUnknownVftbl = IUnknownVftbl.AbiToProjectionVftbl,
				InterfaceSupportsErrorInfo_0 = Do_Abi_InterfaceSupportsErrorInfo_0
			};
			nint* ptr = (nint*)Marshal.AllocCoTaskMem(Marshal.SizeOf<Vftbl>());
			Marshal.StructureToPtr(AbiToProjectionVftable, (nint)ptr, fDeleteOld: false);
			AbiToProjectionVftablePtr = (nint)ptr;
		}

		private static int Do_Abi_InterfaceSupportsErrorInfo_0(nint thisPtr, ref Guid guid)
		{
			try
			{
				return (!ComWrappersSupport.FindObject<global::WinRT.Interop.ISupportErrorInfo>(thisPtr).InterfaceSupportsErrorInfo(guid)) ? 1 : 0;
			}
			catch (Exception ex)
			{
				ExceptionHelpers.SetErrorInfo(ex);
				return ExceptionHelpers.GetHRForException(ex);
			}
		}
	}

	protected readonly ObjectReference<Vftbl> _obj;

	public nint ThisPtr => _obj.ThisPtr;

	public static ObjectReference<Vftbl> FromAbi(nint thisPtr)
	{
		return ObjectReference<Vftbl>.FromAbi(thisPtr);
	}

	public static implicit operator ISupportErrorInfo(IObjectReference obj)
	{
		if (obj == null)
		{
			return null;
		}
		return new ISupportErrorInfo(obj);
	}

	public static implicit operator ISupportErrorInfo(ObjectReference<Vftbl> obj)
	{
		if (obj == null)
		{
			return null;
		}
		return new ISupportErrorInfo(obj);
	}

	public ObjectReference<I> AsInterface<I>()
	{
		return _obj.As<I>();
	}

	public A As<A>()
	{
		return _obj.AsType<A>();
	}

	public ISupportErrorInfo(IObjectReference obj)
		: this(obj.As<Vftbl>())
	{
	}

	public ISupportErrorInfo(ObjectReference<Vftbl> obj)
	{
		_obj = obj;
	}

	public bool InterfaceSupportsErrorInfo(Guid riid)
	{
		return _obj.Vftbl.InterfaceSupportsErrorInfo_0(ThisPtr, ref riid) == 0;
	}
}
