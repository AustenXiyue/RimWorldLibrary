using System;
using System.Runtime.InteropServices;
using MS.Internal.WindowsRuntime.Windows.Globalization;
using WinRT;
using WinRT.Interop;

namespace MS.Internal.WindowsRuntime.ABI.Windows.Globalization;

[ObjectReferenceWrapper("_obj")]
[Guid("EA79A752-F7C2-4265-B1BD-C4DEC4E4F080")]
internal class ILanguage : MS.Internal.WindowsRuntime.Windows.Globalization.ILanguage
{
	[Guid("EA79A752-F7C2-4265-B1BD-C4DEC4E4F080")]
	internal struct Vftbl
	{
		public IInspectable.Vftbl IInspectableVftbl;

		public _get_PropertyAsString get_LanguageTag_0;

		public _get_PropertyAsString get_DisplayName_1;

		public _get_PropertyAsString get_NativeName_2;

		public _get_PropertyAsString get_Script_3;

		private static readonly Vftbl AbiToProjectionVftable;

		public static readonly nint AbiToProjectionVftablePtr;

		unsafe static Vftbl()
		{
			AbiToProjectionVftable = new Vftbl
			{
				IInspectableVftbl = IInspectable.Vftbl.AbiToProjectionVftable,
				get_LanguageTag_0 = Do_Abi_get_LanguageTag_0,
				get_DisplayName_1 = Do_Abi_get_DisplayName_1,
				get_NativeName_2 = Do_Abi_get_NativeName_2,
				get_Script_3 = Do_Abi_get_Script_3
			};
			nint* ptr = (nint*)ComWrappersSupport.AllocateVtableMemory(typeof(Vftbl), Marshal.SizeOf<IInspectable.Vftbl>() + sizeof(nint) * 4);
			Marshal.StructureToPtr(AbiToProjectionVftable, (nint)ptr, fDeleteOld: false);
			AbiToProjectionVftablePtr = (nint)ptr;
		}

		private static int Do_Abi_get_DisplayName_1(nint thisPtr, out nint value)
		{
			string text = null;
			value = 0;
			try
			{
				text = ComWrappersSupport.FindObject<MS.Internal.WindowsRuntime.Windows.Globalization.ILanguage>(thisPtr).DisplayName;
				value = MarshalString.FromManaged(text);
			}
			catch (Exception ex)
			{
				ExceptionHelpers.SetErrorInfo(ex);
				return ExceptionHelpers.GetHRForException(ex);
			}
			return 0;
		}

		private static int Do_Abi_get_LanguageTag_0(nint thisPtr, out nint value)
		{
			string text = null;
			value = 0;
			try
			{
				text = ComWrappersSupport.FindObject<MS.Internal.WindowsRuntime.Windows.Globalization.ILanguage>(thisPtr).LanguageTag;
				value = MarshalString.FromManaged(text);
			}
			catch (Exception ex)
			{
				ExceptionHelpers.SetErrorInfo(ex);
				return ExceptionHelpers.GetHRForException(ex);
			}
			return 0;
		}

		private static int Do_Abi_get_NativeName_2(nint thisPtr, out nint value)
		{
			string text = null;
			value = 0;
			try
			{
				text = ComWrappersSupport.FindObject<MS.Internal.WindowsRuntime.Windows.Globalization.ILanguage>(thisPtr).NativeName;
				value = MarshalString.FromManaged(text);
			}
			catch (Exception ex)
			{
				ExceptionHelpers.SetErrorInfo(ex);
				return ExceptionHelpers.GetHRForException(ex);
			}
			return 0;
		}

		private static int Do_Abi_get_Script_3(nint thisPtr, out nint value)
		{
			string text = null;
			value = 0;
			try
			{
				text = ComWrappersSupport.FindObject<MS.Internal.WindowsRuntime.Windows.Globalization.ILanguage>(thisPtr).Script;
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

	public string DisplayName
	{
		get
		{
			nint value = 0;
			try
			{
				ExceptionHelpers.ThrowExceptionForHR(_obj.Vftbl.get_DisplayName_1(ThisPtr, out value));
				return MarshalString.FromAbi(value);
			}
			finally
			{
				MarshalString.DisposeAbi(value);
			}
		}
	}

	public string LanguageTag
	{
		get
		{
			nint value = 0;
			try
			{
				ExceptionHelpers.ThrowExceptionForHR(_obj.Vftbl.get_LanguageTag_0(ThisPtr, out value));
				return MarshalString.FromAbi(value);
			}
			finally
			{
				MarshalString.DisposeAbi(value);
			}
		}
	}

	public string NativeName
	{
		get
		{
			nint value = 0;
			try
			{
				ExceptionHelpers.ThrowExceptionForHR(_obj.Vftbl.get_NativeName_2(ThisPtr, out value));
				return MarshalString.FromAbi(value);
			}
			finally
			{
				MarshalString.DisposeAbi(value);
			}
		}
	}

	public string Script
	{
		get
		{
			nint value = 0;
			try
			{
				ExceptionHelpers.ThrowExceptionForHR(_obj.Vftbl.get_Script_3(ThisPtr, out value));
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

	public static implicit operator ILanguage(IObjectReference obj)
	{
		if (obj == null)
		{
			return null;
		}
		return new ILanguage(obj);
	}

	public ObjectReference<I> AsInterface<I>()
	{
		return _obj.As<I>();
	}

	public A As<A>()
	{
		return _obj.AsType<A>();
	}

	public ILanguage(IObjectReference obj)
		: this(obj.As<Vftbl>())
	{
	}

	public ILanguage(ObjectReference<Vftbl> obj)
	{
		_obj = obj;
	}
}
