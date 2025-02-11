using System;
using System.Runtime.InteropServices;
using MS.Internal.WindowsRuntime.Windows.Globalization;
using WinRT;

namespace MS.Internal.WindowsRuntime.ABI.Windows.Globalization;

[ObjectReferenceWrapper("_obj")]
[Guid("30199F6E-914B-4B2A-9D6E-E3B0E27DBE4F")]
internal class ILanguageStatics2 : MS.Internal.WindowsRuntime.Windows.Globalization.ILanguageStatics2
{
	[Guid("30199F6E-914B-4B2A-9D6E-E3B0E27DBE4F")]
	internal struct Vftbl
	{
		public IInspectable.Vftbl IInspectableVftbl;

		public ILanguageStatics2_Delegates.TrySetInputMethodLanguageTag_0 TrySetInputMethodLanguageTag_0;

		private static readonly Vftbl AbiToProjectionVftable;

		public static readonly nint AbiToProjectionVftablePtr;

		unsafe static Vftbl()
		{
			AbiToProjectionVftable = new Vftbl
			{
				IInspectableVftbl = IInspectable.Vftbl.AbiToProjectionVftable,
				TrySetInputMethodLanguageTag_0 = Do_Abi_TrySetInputMethodLanguageTag_0
			};
			nint* ptr = (nint*)ComWrappersSupport.AllocateVtableMemory(typeof(Vftbl), Marshal.SizeOf<IInspectable.Vftbl>() + sizeof(nint));
			Marshal.StructureToPtr(AbiToProjectionVftable, (nint)ptr, fDeleteOld: false);
			AbiToProjectionVftablePtr = (nint)ptr;
		}

		private static int Do_Abi_TrySetInputMethodLanguageTag_0(nint thisPtr, nint languageTag, out byte result)
		{
			bool flag = false;
			result = 0;
			try
			{
				flag = ComWrappersSupport.FindObject<MS.Internal.WindowsRuntime.Windows.Globalization.ILanguageStatics2>(thisPtr).TrySetInputMethodLanguageTag(MarshalString.FromAbi(languageTag));
				result = (flag ? ((byte)1) : ((byte)0));
			}
			catch (Exception ex)
			{
				ExceptionHelpers.SetErrorInfo(ex);
				return ExceptionHelpers.GetHRForException(ex);
			}
			return 0;
		}
	}

	protected readonly ObjectReference<Vftbl> _obj;

	public IObjectReference ObjRef => _obj;

	public nint ThisPtr => _obj.ThisPtr;

	public static ObjectReference<Vftbl> FromAbi(nint thisPtr)
	{
		return ObjectReference<Vftbl>.FromAbi(thisPtr);
	}

	public static implicit operator ILanguageStatics2(IObjectReference obj)
	{
		if (obj == null)
		{
			return null;
		}
		return new ILanguageStatics2(obj);
	}

	public ObjectReference<I> AsInterface<I>()
	{
		return _obj.As<I>();
	}

	public A As<A>()
	{
		return _obj.AsType<A>();
	}

	public ILanguageStatics2(IObjectReference obj)
		: this(obj.As<Vftbl>())
	{
	}

	public ILanguageStatics2(ObjectReference<Vftbl> obj)
	{
		_obj = obj;
	}

	public bool TrySetInputMethodLanguageTag(string languageTag)
	{
		MarshalString m = null;
		byte result = 0;
		try
		{
			m = MarshalString.CreateMarshaler(languageTag);
			ExceptionHelpers.ThrowExceptionForHR(_obj.Vftbl.TrySetInputMethodLanguageTag_0(ThisPtr, MarshalString.GetAbi(m), out result));
			return result != 0;
		}
		finally
		{
			MarshalString.DisposeMarshaler(m);
		}
	}
}
