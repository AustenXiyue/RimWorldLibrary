using System;
using System.Runtime.InteropServices;
using MS.Internal.WindowsRuntime.Windows.Data.Text;
using WinRT;
using WinRT.Interop;

namespace MS.Internal.WindowsRuntime.ABI.Windows.Data.Text;

[ObjectReferenceWrapper("_obj")]
[Guid("47396C1E-51B9-4207-9146-248E636A1D1D")]
internal class IAlternateWordForm : MS.Internal.WindowsRuntime.Windows.Data.Text.IAlternateWordForm
{
	[Guid("47396C1E-51B9-4207-9146-248E636A1D1D")]
	internal struct Vftbl
	{
		public IInspectable.Vftbl IInspectableVftbl;

		public IAlternateWordForm_Delegates.get_SourceTextSegment_0 get_SourceTextSegment_0;

		public _get_PropertyAsString get_AlternateText_1;

		public IAlternateWordForm_Delegates.get_NormalizationFormat_2 get_NormalizationFormat_2;

		private static readonly Vftbl AbiToProjectionVftable;

		public static readonly nint AbiToProjectionVftablePtr;

		unsafe static Vftbl()
		{
			AbiToProjectionVftable = new Vftbl
			{
				IInspectableVftbl = IInspectable.Vftbl.AbiToProjectionVftable,
				get_SourceTextSegment_0 = Do_Abi_get_SourceTextSegment_0,
				get_AlternateText_1 = Do_Abi_get_AlternateText_1,
				get_NormalizationFormat_2 = Do_Abi_get_NormalizationFormat_2
			};
			nint* ptr = (nint*)ComWrappersSupport.AllocateVtableMemory(typeof(Vftbl), Marshal.SizeOf<IInspectable.Vftbl>() + sizeof(nint) * 3);
			Marshal.StructureToPtr(AbiToProjectionVftable, (nint)ptr, fDeleteOld: false);
			AbiToProjectionVftablePtr = (nint)ptr;
		}

		private static int Do_Abi_get_AlternateText_1(nint thisPtr, out nint value)
		{
			string text = null;
			value = 0;
			try
			{
				text = ComWrappersSupport.FindObject<MS.Internal.WindowsRuntime.Windows.Data.Text.IAlternateWordForm>(thisPtr).AlternateText;
				value = MarshalString.FromManaged(text);
			}
			catch (Exception ex)
			{
				ExceptionHelpers.SetErrorInfo(ex);
				return ExceptionHelpers.GetHRForException(ex);
			}
			return 0;
		}

		private static int Do_Abi_get_NormalizationFormat_2(nint thisPtr, out AlternateNormalizationFormat value)
		{
			AlternateNormalizationFormat alternateNormalizationFormat = AlternateNormalizationFormat.NotNormalized;
			value = AlternateNormalizationFormat.NotNormalized;
			try
			{
				alternateNormalizationFormat = ComWrappersSupport.FindObject<MS.Internal.WindowsRuntime.Windows.Data.Text.IAlternateWordForm>(thisPtr).NormalizationFormat;
				value = alternateNormalizationFormat;
			}
			catch (Exception ex)
			{
				ExceptionHelpers.SetErrorInfo(ex);
				return ExceptionHelpers.GetHRForException(ex);
			}
			return 0;
		}

		private static int Do_Abi_get_SourceTextSegment_0(nint thisPtr, out TextSegment value)
		{
			TextSegment textSegment = default(TextSegment);
			value = default(TextSegment);
			try
			{
				textSegment = ComWrappersSupport.FindObject<MS.Internal.WindowsRuntime.Windows.Data.Text.IAlternateWordForm>(thisPtr).SourceTextSegment;
				value = textSegment;
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

	public string AlternateText
	{
		get
		{
			nint value = 0;
			try
			{
				ExceptionHelpers.ThrowExceptionForHR(_obj.Vftbl.get_AlternateText_1(ThisPtr, out value));
				return MarshalString.FromAbi(value);
			}
			finally
			{
				MarshalString.DisposeAbi(value);
			}
		}
	}

	public AlternateNormalizationFormat NormalizationFormat
	{
		get
		{
			AlternateNormalizationFormat value = AlternateNormalizationFormat.NotNormalized;
			ExceptionHelpers.ThrowExceptionForHR(_obj.Vftbl.get_NormalizationFormat_2(ThisPtr, out value));
			return value;
		}
	}

	public TextSegment SourceTextSegment
	{
		get
		{
			TextSegment value = default(TextSegment);
			ExceptionHelpers.ThrowExceptionForHR(_obj.Vftbl.get_SourceTextSegment_0(ThisPtr, out value));
			return value;
		}
	}

	public static ObjectReference<Vftbl> FromAbi(nint thisPtr)
	{
		return ObjectReference<Vftbl>.FromAbi(thisPtr);
	}

	public static implicit operator IAlternateWordForm(IObjectReference obj)
	{
		if (obj == null)
		{
			return null;
		}
		return new IAlternateWordForm(obj);
	}

	public ObjectReference<I> AsInterface<I>()
	{
		return _obj.As<I>();
	}

	public A As<A>()
	{
		return _obj.AsType<A>();
	}

	public IAlternateWordForm(IObjectReference obj)
		: this(obj.As<Vftbl>())
	{
	}

	public IAlternateWordForm(ObjectReference<Vftbl> obj)
	{
		_obj = obj;
	}
}
