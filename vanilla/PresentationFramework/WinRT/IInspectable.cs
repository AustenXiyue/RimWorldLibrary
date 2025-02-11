using System;
using System.Runtime.InteropServices;
using WinRT.Interop;

namespace WinRT;

[ObjectReferenceWrapper("_obj")]
[Guid("AF86E2E0-B12D-4c6a-9C5A-D7AA65101E90")]
internal class IInspectable
{
	[Guid("AF86E2E0-B12D-4c6a-9C5A-D7AA65101E90")]
	internal struct Vftbl
	{
		internal delegate int _GetIids(nint pThis, out uint iidCount, out Guid[] iids);

		internal delegate int _GetRuntimeClassName(nint pThis, out nint className);

		internal delegate int _GetTrustLevel(nint pThis, out TrustLevel trustLevel);

		public IUnknownVftbl IUnknownVftbl;

		public _GetIids GetIids;

		public _GetRuntimeClassName GetRuntimeClassName;

		public _GetTrustLevel GetTrustLevel;

		public static readonly Vftbl AbiToProjectionVftable;

		public static readonly nint AbiToProjectionVftablePtr;

		static Vftbl()
		{
			AbiToProjectionVftable = new Vftbl
			{
				IUnknownVftbl = IUnknownVftbl.AbiToProjectionVftbl,
				GetIids = Do_Abi_GetIids,
				GetRuntimeClassName = Do_Abi_GetRuntimeClassName,
				GetTrustLevel = Do_Abi_GetTrustLevel
			};
			AbiToProjectionVftablePtr = Marshal.AllocHGlobal(Marshal.SizeOf<Vftbl>());
			Marshal.StructureToPtr(AbiToProjectionVftable, AbiToProjectionVftablePtr, fDeleteOld: false);
		}

		private static int Do_Abi_GetIids(nint pThis, out uint iidCount, out Guid[] iids)
		{
			iidCount = 0u;
			iids = null;
			try
			{
				iids = ComWrappersSupport.GetInspectableInfo(pThis).IIDs;
				iidCount = (uint)iids.Length;
			}
			catch (Exception ex)
			{
				return ex.HResult;
			}
			return 0;
		}

		private static int Do_Abi_GetRuntimeClassName(nint pThis, out nint className)
		{
			className = 0;
			try
			{
				string runtimeClassName = ComWrappersSupport.GetInspectableInfo(pThis).RuntimeClassName;
				className = MarshalString.FromManaged(runtimeClassName);
			}
			catch (Exception ex)
			{
				return ex.HResult;
			}
			return 0;
		}

		private static int Do_Abi_GetTrustLevel(nint pThis, out TrustLevel trustLevel)
		{
			trustLevel = TrustLevel.BaseTrust;
			return 0;
		}
	}

	private readonly ObjectReference<Vftbl> _obj;

	public nint ThisPtr => _obj.ThisPtr;

	public IObjectReference ObjRef => _obj;

	public static IInspectable FromAbi(nint thisPtr)
	{
		return new IInspectable(ObjectReference<Vftbl>.FromAbi(thisPtr));
	}

	public static implicit operator IInspectable(IObjectReference obj)
	{
		return obj.As<Vftbl>();
	}

	public static implicit operator IInspectable(ObjectReference<Vftbl> obj)
	{
		return new IInspectable(obj);
	}

	public ObjectReference<I> As<I>()
	{
		return _obj.As<I>();
	}

	public IInspectable(IObjectReference obj)
		: this(obj.As<Vftbl>())
	{
	}

	public IInspectable(ObjectReference<Vftbl> obj)
	{
		_obj = obj;
	}

	public unsafe string GetRuntimeClassName(bool noThrow = false)
	{
		nint className = 0;
		try
		{
			int num = _obj.Vftbl.GetRuntimeClassName(ThisPtr, out className);
			if (num != 0)
			{
				if (noThrow)
				{
					return null;
				}
				Marshal.ThrowExceptionForHR(num);
			}
			uint length = default(uint);
			return new string(Platform.WindowsGetStringRawBuffer(className, &length), 0, (int)length);
		}
		finally
		{
			Platform.WindowsDeleteString(className);
		}
	}
}
