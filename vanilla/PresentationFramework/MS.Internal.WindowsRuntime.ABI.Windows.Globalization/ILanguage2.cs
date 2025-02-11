using System;
using System.Runtime.InteropServices;
using MS.Internal.WindowsRuntime.Windows.Globalization;
using WinRT;

namespace MS.Internal.WindowsRuntime.ABI.Windows.Globalization;

[ObjectReferenceWrapper("_obj")]
[Guid("6A47E5B5-D94D-4886-A404-A5A5B9D5B494")]
internal class ILanguage2 : MS.Internal.WindowsRuntime.Windows.Globalization.ILanguage2
{
	[Guid("6A47E5B5-D94D-4886-A404-A5A5B9D5B494")]
	internal struct Vftbl
	{
		public IInspectable.Vftbl IInspectableVftbl;

		public ILanguage2_Delegates.get_LayoutDirection_0 get_LayoutDirection_0;

		private static readonly Vftbl AbiToProjectionVftable;

		public static readonly nint AbiToProjectionVftablePtr;

		unsafe static Vftbl()
		{
			AbiToProjectionVftable = new Vftbl
			{
				IInspectableVftbl = IInspectable.Vftbl.AbiToProjectionVftable,
				get_LayoutDirection_0 = Do_Abi_get_LayoutDirection_0
			};
			nint* ptr = (nint*)ComWrappersSupport.AllocateVtableMemory(typeof(Vftbl), Marshal.SizeOf<IInspectable.Vftbl>() + sizeof(nint));
			Marshal.StructureToPtr(AbiToProjectionVftable, (nint)ptr, fDeleteOld: false);
			AbiToProjectionVftablePtr = (nint)ptr;
		}

		private static int Do_Abi_get_LayoutDirection_0(nint thisPtr, out LanguageLayoutDirection value)
		{
			LanguageLayoutDirection languageLayoutDirection = LanguageLayoutDirection.Ltr;
			value = LanguageLayoutDirection.Ltr;
			try
			{
				languageLayoutDirection = ComWrappersSupport.FindObject<MS.Internal.WindowsRuntime.Windows.Globalization.ILanguage2>(thisPtr).LayoutDirection;
				value = languageLayoutDirection;
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

	public LanguageLayoutDirection LayoutDirection
	{
		get
		{
			LanguageLayoutDirection value = LanguageLayoutDirection.Ltr;
			ExceptionHelpers.ThrowExceptionForHR(_obj.Vftbl.get_LayoutDirection_0(ThisPtr, out value));
			return value;
		}
	}

	public static ObjectReference<Vftbl> FromAbi(nint thisPtr)
	{
		return ObjectReference<Vftbl>.FromAbi(thisPtr);
	}

	public static implicit operator ILanguage2(IObjectReference obj)
	{
		if (obj == null)
		{
			return null;
		}
		return new ILanguage2(obj);
	}

	public ObjectReference<I> AsInterface<I>()
	{
		return _obj.As<I>();
	}

	public A As<A>()
	{
		return _obj.AsType<A>();
	}

	public ILanguage2(IObjectReference obj)
		: this(obj.As<Vftbl>())
	{
	}

	public ILanguage2(ObjectReference<Vftbl> obj)
	{
		_obj = obj;
	}
}
