using System;
using System.Runtime.InteropServices;
using MS.Internal.WindowsRuntime.Windows.Globalization;
using WinRT;

namespace MS.Internal.WindowsRuntime.ABI.Windows.Globalization;

[ObjectReferenceWrapper("_obj")]
[Guid("9B0252AC-0C27-44F8-B792-9793FB66C63E")]
internal class ILanguageFactory : MS.Internal.WindowsRuntime.Windows.Globalization.ILanguageFactory
{
	[Guid("9B0252AC-0C27-44F8-B792-9793FB66C63E")]
	internal struct Vftbl
	{
		public IInspectable.Vftbl IInspectableVftbl;

		public ILanguageFactory_Delegates.CreateLanguage_0 CreateLanguage_0;

		private static readonly Vftbl AbiToProjectionVftable;

		public static readonly nint AbiToProjectionVftablePtr;

		unsafe static Vftbl()
		{
			AbiToProjectionVftable = new Vftbl
			{
				IInspectableVftbl = IInspectable.Vftbl.AbiToProjectionVftable,
				CreateLanguage_0 = Do_Abi_CreateLanguage_0
			};
			nint* ptr = (nint*)ComWrappersSupport.AllocateVtableMemory(typeof(Vftbl), Marshal.SizeOf<IInspectable.Vftbl>() + sizeof(nint));
			Marshal.StructureToPtr(AbiToProjectionVftable, (nint)ptr, fDeleteOld: false);
			AbiToProjectionVftablePtr = (nint)ptr;
		}

		private static int Do_Abi_CreateLanguage_0(nint thisPtr, nint languageTag, out nint result)
		{
			MS.Internal.WindowsRuntime.Windows.Globalization.Language language = null;
			result = 0;
			try
			{
				language = ComWrappersSupport.FindObject<MS.Internal.WindowsRuntime.Windows.Globalization.ILanguageFactory>(thisPtr).CreateLanguage(MarshalString.FromAbi(languageTag));
				result = Language.FromManaged(language);
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

	public static implicit operator ILanguageFactory(IObjectReference obj)
	{
		if (obj == null)
		{
			return null;
		}
		return new ILanguageFactory(obj);
	}

	public ObjectReference<I> AsInterface<I>()
	{
		return _obj.As<I>();
	}

	public A As<A>()
	{
		return _obj.AsType<A>();
	}

	public ILanguageFactory(IObjectReference obj)
		: this(obj.As<Vftbl>())
	{
	}

	public ILanguageFactory(ObjectReference<Vftbl> obj)
	{
		_obj = obj;
	}

	public MS.Internal.WindowsRuntime.Windows.Globalization.Language CreateLanguage(string languageTag)
	{
		MarshalString m = null;
		nint result = 0;
		try
		{
			m = MarshalString.CreateMarshaler(languageTag);
			ExceptionHelpers.ThrowExceptionForHR(_obj.Vftbl.CreateLanguage_0(ThisPtr, MarshalString.GetAbi(m), out result));
			return Language.FromAbi(result);
		}
		finally
		{
			MarshalString.DisposeMarshaler(m);
			Language.DisposeAbi(result);
		}
	}
}
