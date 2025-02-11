using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MS.Internal.WindowsRuntime.ABI.System.Collections.Generic;
using MS.Internal.WindowsRuntime.Windows.Data.Text;
using WinRT;
using WinRT.Interop;

namespace MS.Internal.WindowsRuntime.ABI.Windows.Data.Text;

[ObjectReferenceWrapper("_obj")]
[Guid("D2D4BA6D-987C-4CC0-B6BD-D49A11B38F9A")]
internal class IWordSegment : MS.Internal.WindowsRuntime.Windows.Data.Text.IWordSegment
{
	[Guid("D2D4BA6D-987C-4CC0-B6BD-D49A11B38F9A")]
	internal struct Vftbl
	{
		public IInspectable.Vftbl IInspectableVftbl;

		public _get_PropertyAsString get_Text_0;

		public IWordSegment_Delegates.get_SourceTextSegment_1 get_SourceTextSegment_1;

		public _get_PropertyAsObject get_AlternateForms_2;

		private static readonly Vftbl AbiToProjectionVftable;

		public static readonly nint AbiToProjectionVftablePtr;

		unsafe static Vftbl()
		{
			AbiToProjectionVftable = new Vftbl
			{
				IInspectableVftbl = IInspectable.Vftbl.AbiToProjectionVftable,
				get_Text_0 = Do_Abi_get_Text_0,
				get_SourceTextSegment_1 = Do_Abi_get_SourceTextSegment_1,
				get_AlternateForms_2 = Do_Abi_get_AlternateForms_2
			};
			nint* ptr = (nint*)ComWrappersSupport.AllocateVtableMemory(typeof(Vftbl), Marshal.SizeOf<IInspectable.Vftbl>() + sizeof(nint) * 3);
			Marshal.StructureToPtr(AbiToProjectionVftable, (nint)ptr, fDeleteOld: false);
			AbiToProjectionVftablePtr = (nint)ptr;
		}

		private static int Do_Abi_get_AlternateForms_2(nint thisPtr, out nint value)
		{
			global::System.Collections.Generic.IReadOnlyList<MS.Internal.WindowsRuntime.Windows.Data.Text.AlternateWordForm> readOnlyList = null;
			value = 0;
			try
			{
				readOnlyList = ComWrappersSupport.FindObject<MS.Internal.WindowsRuntime.Windows.Data.Text.IWordSegment>(thisPtr).AlternateForms;
				value = MS.Internal.WindowsRuntime.ABI.System.Collections.Generic.IReadOnlyList<MS.Internal.WindowsRuntime.Windows.Data.Text.AlternateWordForm>.FromManaged(readOnlyList);
			}
			catch (Exception ex)
			{
				ExceptionHelpers.SetErrorInfo(ex);
				return ExceptionHelpers.GetHRForException(ex);
			}
			return 0;
		}

		private static int Do_Abi_get_SourceTextSegment_1(nint thisPtr, out TextSegment value)
		{
			TextSegment textSegment = default(TextSegment);
			value = default(TextSegment);
			try
			{
				textSegment = ComWrappersSupport.FindObject<MS.Internal.WindowsRuntime.Windows.Data.Text.IWordSegment>(thisPtr).SourceTextSegment;
				value = textSegment;
			}
			catch (Exception ex)
			{
				ExceptionHelpers.SetErrorInfo(ex);
				return ExceptionHelpers.GetHRForException(ex);
			}
			return 0;
		}

		private static int Do_Abi_get_Text_0(nint thisPtr, out nint value)
		{
			string text = null;
			value = 0;
			try
			{
				text = ComWrappersSupport.FindObject<MS.Internal.WindowsRuntime.Windows.Data.Text.IWordSegment>(thisPtr).Text;
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

	public global::System.Collections.Generic.IReadOnlyList<MS.Internal.WindowsRuntime.Windows.Data.Text.AlternateWordForm> AlternateForms
	{
		get
		{
			nint value = 0;
			try
			{
				ExceptionHelpers.ThrowExceptionForHR(_obj.Vftbl.get_AlternateForms_2(ThisPtr, out value));
				return MS.Internal.WindowsRuntime.ABI.System.Collections.Generic.IReadOnlyList<MS.Internal.WindowsRuntime.Windows.Data.Text.AlternateWordForm>.FromAbi(value);
			}
			finally
			{
				MS.Internal.WindowsRuntime.ABI.System.Collections.Generic.IReadOnlyList<MS.Internal.WindowsRuntime.Windows.Data.Text.AlternateWordForm>.DisposeAbi(value);
			}
		}
	}

	public TextSegment SourceTextSegment
	{
		get
		{
			TextSegment value = default(TextSegment);
			ExceptionHelpers.ThrowExceptionForHR(_obj.Vftbl.get_SourceTextSegment_1(ThisPtr, out value));
			return value;
		}
	}

	public string Text
	{
		get
		{
			nint value = 0;
			try
			{
				ExceptionHelpers.ThrowExceptionForHR(_obj.Vftbl.get_Text_0(ThisPtr, out value));
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

	public static implicit operator IWordSegment(IObjectReference obj)
	{
		if (obj == null)
		{
			return null;
		}
		return new IWordSegment(obj);
	}

	public ObjectReference<I> AsInterface<I>()
	{
		return _obj.As<I>();
	}

	public A As<A>()
	{
		return _obj.AsType<A>();
	}

	public IWordSegment(IObjectReference obj)
		: this(obj.As<Vftbl>())
	{
	}

	public IWordSegment(ObjectReference<Vftbl> obj)
	{
		_obj = obj;
	}
}
