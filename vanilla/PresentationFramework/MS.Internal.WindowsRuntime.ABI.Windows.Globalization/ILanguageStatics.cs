using System;
using System.Runtime.InteropServices;
using MS.Internal.WindowsRuntime.Windows.Globalization;
using WinRT;
using WinRT.Interop;

namespace MS.Internal.WindowsRuntime.ABI.Windows.Globalization;

[ObjectReferenceWrapper("_obj")]
[Guid("B23CD557-0865-46D4-89B8-D59BE8990F0D")]
internal class ILanguageStatics : MS.Internal.WindowsRuntime.Windows.Globalization.ILanguageStatics
{
	[Guid("B23CD557-0865-46D4-89B8-D59BE8990F0D")]
	internal struct Vftbl
	{
		public IInspectable.Vftbl IInspectableVftbl;

		public ILanguageStatics_Delegates.IsWellFormed_0 IsWellFormed_0;

		public _get_PropertyAsString get_CurrentInputMethodLanguageTag_1;

		private static readonly Vftbl AbiToProjectionVftable;

		public static readonly nint AbiToProjectionVftablePtr;

		unsafe static Vftbl()
		{
			AbiToProjectionVftable = new Vftbl
			{
				IInspectableVftbl = IInspectable.Vftbl.AbiToProjectionVftable,
				IsWellFormed_0 = Do_Abi_IsWellFormed_0,
				get_CurrentInputMethodLanguageTag_1 = Do_Abi_get_CurrentInputMethodLanguageTag_1
			};
			nint* ptr = (nint*)ComWrappersSupport.AllocateVtableMemory(typeof(Vftbl), Marshal.SizeOf<IInspectable.Vftbl>() + sizeof(nint) * 2);
			Marshal.StructureToPtr(AbiToProjectionVftable, (nint)ptr, fDeleteOld: false);
			AbiToProjectionVftablePtr = (nint)ptr;
		}

		private static int Do_Abi_IsWellFormed_0(nint thisPtr, nint languageTag, out byte result)
		{
			bool flag = false;
			result = 0;
			try
			{
				flag = ComWrappersSupport.FindObject<MS.Internal.WindowsRuntime.Windows.Globalization.ILanguageStatics>(thisPtr).IsWellFormed(MarshalString.FromAbi(languageTag));
				result = (flag ? ((byte)1) : ((byte)0));
			}
			catch (Exception ex)
			{
				ExceptionHelpers.SetErrorInfo(ex);
				return ExceptionHelpers.GetHRForException(ex);
			}
			return 0;
		}

		private static int Do_Abi_get_CurrentInputMethodLanguageTag_1(nint thisPtr, out nint value)
		{
			string text = null;
			value = 0;
			try
			{
				text = ComWrappersSupport.FindObject<MS.Internal.WindowsRuntime.Windows.Globalization.ILanguageStatics>(thisPtr).CurrentInputMethodLanguageTag;
				value = MarshalString.FromManaged(text);
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

	public string CurrentInputMethodLanguageTag
	{
		get
		{
			nint value = 0;
			try
			{
				ExceptionHelpers.ThrowExceptionForHR(_obj.Vftbl.get_CurrentInputMethodLanguageTag_1(ThisPtr, out value));
				return MarshalString.FromAbi(value);
			}
			finally
			{
				MarshalString.DisposeAbi(value);
			}
		}
	}

	public static ObjectReference<Vftbl> FromAbi(nint thisPtr)
	{
		return ObjectReference<Vftbl>.FromAbi(thisPtr);
	}

	public static implicit operator ILanguageStatics(IObjectReference obj)
	{
		if (obj == null)
		{
			return null;
		}
		return new ILanguageStatics(obj);
	}

	public ObjectReference<I> AsInterface<I>()
	{
		return _obj.As<I>();
	}

	public A As<A>()
	{
		return _obj.AsType<A>();
	}

	public ILanguageStatics(IObjectReference obj)
		: this(obj.As<Vftbl>())
	{
	}

	public ILanguageStatics(ObjectReference<Vftbl> obj)
	{
		_obj = obj;
	}

	public bool IsWellFormed(string languageTag)
	{
		MarshalString m = null;
		byte result = 0;
		try
		{
			m = MarshalString.CreateMarshaler(languageTag);
			ExceptionHelpers.ThrowExceptionForHR(_obj.Vftbl.IsWellFormed_0(ThisPtr, MarshalString.GetAbi(m), out result));
			return result != 0;
		}
		finally
		{
			MarshalString.DisposeMarshaler(m);
		}
	}
}
