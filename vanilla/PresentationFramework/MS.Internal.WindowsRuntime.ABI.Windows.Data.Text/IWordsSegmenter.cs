using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MS.Internal.WindowsRuntime.ABI.System.Collections.Generic;
using MS.Internal.WindowsRuntime.Windows.Data.Text;
using WinRT;
using WinRT.Interop;

namespace MS.Internal.WindowsRuntime.ABI.Windows.Data.Text;

[ObjectReferenceWrapper("_obj")]
[Guid("86B4D4D1-B2FE-4E34-A81D-66640300454F")]
internal class IWordsSegmenter : MS.Internal.WindowsRuntime.Windows.Data.Text.IWordsSegmenter
{
	[Guid("86B4D4D1-B2FE-4E34-A81D-66640300454F")]
	internal struct Vftbl
	{
		public IInspectable.Vftbl IInspectableVftbl;

		public _get_PropertyAsString get_ResolvedLanguage_0;

		public IWordsSegmenter_Delegates.GetTokenAt_1 GetTokenAt_1;

		public IWordsSegmenter_Delegates.GetTokens_2 GetTokens_2;

		public IWordsSegmenter_Delegates.Tokenize_3 Tokenize_3;

		private static readonly Vftbl AbiToProjectionVftable;

		public static readonly nint AbiToProjectionVftablePtr;

		unsafe static Vftbl()
		{
			AbiToProjectionVftable = new Vftbl
			{
				IInspectableVftbl = IInspectable.Vftbl.AbiToProjectionVftable,
				get_ResolvedLanguage_0 = Do_Abi_get_ResolvedLanguage_0,
				GetTokenAt_1 = Do_Abi_GetTokenAt_1,
				GetTokens_2 = Do_Abi_GetTokens_2,
				Tokenize_3 = Do_Abi_Tokenize_3
			};
			nint* ptr = (nint*)ComWrappersSupport.AllocateVtableMemory(typeof(Vftbl), Marshal.SizeOf<IInspectable.Vftbl>() + sizeof(nint) * 4);
			Marshal.StructureToPtr(AbiToProjectionVftable, (nint)ptr, fDeleteOld: false);
			AbiToProjectionVftablePtr = (nint)ptr;
		}

		private static int Do_Abi_GetTokenAt_1(nint thisPtr, nint text, uint startIndex, out nint result)
		{
			MS.Internal.WindowsRuntime.Windows.Data.Text.WordSegment wordSegment = null;
			result = 0;
			try
			{
				wordSegment = ComWrappersSupport.FindObject<MS.Internal.WindowsRuntime.Windows.Data.Text.IWordsSegmenter>(thisPtr).GetTokenAt(MarshalString.FromAbi(text), startIndex);
				result = WordSegment.FromManaged(wordSegment);
			}
			catch (Exception ex)
			{
				ExceptionHelpers.SetErrorInfo(ex);
				return ExceptionHelpers.GetHRForException(ex);
			}
			return 0;
		}

		private static int Do_Abi_GetTokens_2(nint thisPtr, nint text, out nint result)
		{
			global::System.Collections.Generic.IReadOnlyList<MS.Internal.WindowsRuntime.Windows.Data.Text.WordSegment> readOnlyList = null;
			result = 0;
			try
			{
				readOnlyList = ComWrappersSupport.FindObject<MS.Internal.WindowsRuntime.Windows.Data.Text.IWordsSegmenter>(thisPtr).GetTokens(MarshalString.FromAbi(text));
				result = MS.Internal.WindowsRuntime.ABI.System.Collections.Generic.IReadOnlyList<MS.Internal.WindowsRuntime.Windows.Data.Text.WordSegment>.FromManaged(readOnlyList);
			}
			catch (Exception ex)
			{
				ExceptionHelpers.SetErrorInfo(ex);
				return ExceptionHelpers.GetHRForException(ex);
			}
			return 0;
		}

		private static int Do_Abi_Tokenize_3(nint thisPtr, nint text, uint startIndex, nint handler)
		{
			try
			{
				ComWrappersSupport.FindObject<MS.Internal.WindowsRuntime.Windows.Data.Text.IWordsSegmenter>(thisPtr).Tokenize(MarshalString.FromAbi(text), startIndex, WordSegmentsTokenizingHandler.FromAbi(handler));
			}
			catch (Exception ex)
			{
				ExceptionHelpers.SetErrorInfo(ex);
				return ExceptionHelpers.GetHRForException(ex);
			}
			return 0;
		}

		private static int Do_Abi_get_ResolvedLanguage_0(nint thisPtr, out nint value)
		{
			string text = null;
			value = 0;
			try
			{
				text = ComWrappersSupport.FindObject<MS.Internal.WindowsRuntime.Windows.Data.Text.IWordsSegmenter>(thisPtr).ResolvedLanguage;
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

	public string ResolvedLanguage
	{
		get
		{
			nint value = 0;
			try
			{
				ExceptionHelpers.ThrowExceptionForHR(_obj.Vftbl.get_ResolvedLanguage_0(ThisPtr, out value));
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

	public static implicit operator IWordsSegmenter(IObjectReference obj)
	{
		if (obj == null)
		{
			return null;
		}
		return new IWordsSegmenter(obj);
	}

	public ObjectReference<I> AsInterface<I>()
	{
		return _obj.As<I>();
	}

	public A As<A>()
	{
		return _obj.AsType<A>();
	}

	public IWordsSegmenter(IObjectReference obj)
		: this(obj.As<Vftbl>())
	{
	}

	public IWordsSegmenter(ObjectReference<Vftbl> obj)
	{
		_obj = obj;
	}

	public MS.Internal.WindowsRuntime.Windows.Data.Text.WordSegment GetTokenAt(string text, uint startIndex)
	{
		MarshalString m = null;
		nint result = 0;
		try
		{
			m = MarshalString.CreateMarshaler(text);
			ExceptionHelpers.ThrowExceptionForHR(_obj.Vftbl.GetTokenAt_1(ThisPtr, MarshalString.GetAbi(m), startIndex, out result));
			return WordSegment.FromAbi(result);
		}
		finally
		{
			MarshalString.DisposeMarshaler(m);
			WordSegment.DisposeAbi(result);
		}
	}

	public global::System.Collections.Generic.IReadOnlyList<MS.Internal.WindowsRuntime.Windows.Data.Text.WordSegment> GetTokens(string text)
	{
		MarshalString m = null;
		nint result = 0;
		try
		{
			m = MarshalString.CreateMarshaler(text);
			ExceptionHelpers.ThrowExceptionForHR(_obj.Vftbl.GetTokens_2(ThisPtr, MarshalString.GetAbi(m), out result));
			return MS.Internal.WindowsRuntime.ABI.System.Collections.Generic.IReadOnlyList<MS.Internal.WindowsRuntime.Windows.Data.Text.WordSegment>.FromAbi(result);
		}
		finally
		{
			MarshalString.DisposeMarshaler(m);
			MS.Internal.WindowsRuntime.ABI.System.Collections.Generic.IReadOnlyList<MS.Internal.WindowsRuntime.Windows.Data.Text.WordSegment>.DisposeAbi(result);
		}
	}

	public void Tokenize(string text, uint startIndex, MS.Internal.WindowsRuntime.Windows.Data.Text.WordSegmentsTokenizingHandler handler)
	{
		MarshalString m = null;
		IObjectReference value = null;
		try
		{
			m = MarshalString.CreateMarshaler(text);
			value = WordSegmentsTokenizingHandler.CreateMarshaler(handler);
			ExceptionHelpers.ThrowExceptionForHR(_obj.Vftbl.Tokenize_3(ThisPtr, MarshalString.GetAbi(m), startIndex, WordSegmentsTokenizingHandler.GetAbi(value)));
		}
		finally
		{
			MarshalString.DisposeMarshaler(m);
			WordSegmentsTokenizingHandler.DisposeMarshaler(value);
		}
	}
}
