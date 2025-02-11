using System;
using System.Runtime.InteropServices;
using MS.Internal.WindowsRuntime.Windows.Data.Text;
using WinRT;

namespace MS.Internal.WindowsRuntime.ABI.Windows.Data.Text;

[ObjectReferenceWrapper("_obj")]
[Guid("E6977274-FC35-455C-8BFB-6D7F4653CA97")]
internal class IWordsSegmenterFactory : MS.Internal.WindowsRuntime.Windows.Data.Text.IWordsSegmenterFactory
{
	[Guid("E6977274-FC35-455C-8BFB-6D7F4653CA97")]
	internal struct Vftbl
	{
		public IInspectable.Vftbl IInspectableVftbl;

		public IWordsSegmenterFactory_Delegates.CreateWithLanguage_0 CreateWithLanguage_0;

		private static readonly Vftbl AbiToProjectionVftable;

		public static readonly nint AbiToProjectionVftablePtr;

		unsafe static Vftbl()
		{
			AbiToProjectionVftable = new Vftbl
			{
				IInspectableVftbl = IInspectable.Vftbl.AbiToProjectionVftable,
				CreateWithLanguage_0 = Do_Abi_CreateWithLanguage_0
			};
			nint* ptr = (nint*)ComWrappersSupport.AllocateVtableMemory(typeof(Vftbl), Marshal.SizeOf<IInspectable.Vftbl>() + sizeof(nint));
			Marshal.StructureToPtr(AbiToProjectionVftable, (nint)ptr, fDeleteOld: false);
			AbiToProjectionVftablePtr = (nint)ptr;
		}

		private static int Do_Abi_CreateWithLanguage_0(nint thisPtr, nint language, out nint result)
		{
			MS.Internal.WindowsRuntime.Windows.Data.Text.WordsSegmenter wordsSegmenter = null;
			result = 0;
			try
			{
				wordsSegmenter = ComWrappersSupport.FindObject<MS.Internal.WindowsRuntime.Windows.Data.Text.IWordsSegmenterFactory>(thisPtr).CreateWithLanguage(MarshalString.FromAbi(language));
				result = WordsSegmenter.FromManaged(wordsSegmenter);
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

	public static implicit operator IWordsSegmenterFactory(IObjectReference obj)
	{
		if (obj == null)
		{
			return null;
		}
		return new IWordsSegmenterFactory(obj);
	}

	public ObjectReference<I> AsInterface<I>()
	{
		return _obj.As<I>();
	}

	public A As<A>()
	{
		return _obj.AsType<A>();
	}

	public IWordsSegmenterFactory(IObjectReference obj)
		: this(obj.As<Vftbl>())
	{
	}

	public IWordsSegmenterFactory(ObjectReference<Vftbl> obj)
	{
		_obj = obj;
	}

	public MS.Internal.WindowsRuntime.Windows.Data.Text.WordsSegmenter CreateWithLanguage(string language)
	{
		MarshalString m = null;
		nint result = 0;
		try
		{
			m = MarshalString.CreateMarshaler(language);
			ExceptionHelpers.ThrowExceptionForHR(_obj.Vftbl.CreateWithLanguage_0(ThisPtr, MarshalString.GetAbi(m), out result));
			return WordsSegmenter.FromAbi(result);
		}
		finally
		{
			MarshalString.DisposeMarshaler(m);
			WordsSegmenter.DisposeAbi(result);
		}
	}
}
