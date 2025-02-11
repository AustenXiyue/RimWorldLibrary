using System;
using System.Runtime.InteropServices;
using MS.Internal.WindowsRuntime.Windows.Data.Text;
using WinRT;

namespace MS.Internal.WindowsRuntime.ABI.Windows.Data.Text;

[ObjectReferenceWrapper("_obj")]
[Guid("97909E87-9291-4F91-B6C8-B6E359D7A7FB")]
internal class IUnicodeCharactersStatics : MS.Internal.WindowsRuntime.Windows.Data.Text.IUnicodeCharactersStatics
{
	[Guid("97909E87-9291-4F91-B6C8-B6E359D7A7FB")]
	internal struct Vftbl
	{
		public IInspectable.Vftbl IInspectableVftbl;

		public IUnicodeCharactersStatics_Delegates.GetCodepointFromSurrogatePair_0 GetCodepointFromSurrogatePair_0;

		public IUnicodeCharactersStatics_Delegates.GetSurrogatePairFromCodepoint_1 GetSurrogatePairFromCodepoint_1;

		public IUnicodeCharactersStatics_Delegates.IsHighSurrogate_2 IsHighSurrogate_2;

		public IUnicodeCharactersStatics_Delegates.IsLowSurrogate_3 IsLowSurrogate_3;

		public IUnicodeCharactersStatics_Delegates.IsSupplementary_4 IsSupplementary_4;

		public IUnicodeCharactersStatics_Delegates.IsNoncharacter_5 IsNoncharacter_5;

		public IUnicodeCharactersStatics_Delegates.IsWhitespace_6 IsWhitespace_6;

		public IUnicodeCharactersStatics_Delegates.IsAlphabetic_7 IsAlphabetic_7;

		public IUnicodeCharactersStatics_Delegates.IsCased_8 IsCased_8;

		public IUnicodeCharactersStatics_Delegates.IsUppercase_9 IsUppercase_9;

		public IUnicodeCharactersStatics_Delegates.IsLowercase_10 IsLowercase_10;

		public IUnicodeCharactersStatics_Delegates.IsIdStart_11 IsIdStart_11;

		public IUnicodeCharactersStatics_Delegates.IsIdContinue_12 IsIdContinue_12;

		public IUnicodeCharactersStatics_Delegates.IsGraphemeBase_13 IsGraphemeBase_13;

		public IUnicodeCharactersStatics_Delegates.IsGraphemeExtend_14 IsGraphemeExtend_14;

		public IUnicodeCharactersStatics_Delegates.GetNumericType_15 GetNumericType_15;

		public IUnicodeCharactersStatics_Delegates.GetGeneralCategory_16 GetGeneralCategory_16;

		private static readonly Vftbl AbiToProjectionVftable;

		public static readonly nint AbiToProjectionVftablePtr;

		unsafe static Vftbl()
		{
			AbiToProjectionVftable = new Vftbl
			{
				IInspectableVftbl = IInspectable.Vftbl.AbiToProjectionVftable,
				GetCodepointFromSurrogatePair_0 = Do_Abi_GetCodepointFromSurrogatePair_0,
				GetSurrogatePairFromCodepoint_1 = Do_Abi_GetSurrogatePairFromCodepoint_1,
				IsHighSurrogate_2 = Do_Abi_IsHighSurrogate_2,
				IsLowSurrogate_3 = Do_Abi_IsLowSurrogate_3,
				IsSupplementary_4 = Do_Abi_IsSupplementary_4,
				IsNoncharacter_5 = Do_Abi_IsNoncharacter_5,
				IsWhitespace_6 = Do_Abi_IsWhitespace_6,
				IsAlphabetic_7 = Do_Abi_IsAlphabetic_7,
				IsCased_8 = Do_Abi_IsCased_8,
				IsUppercase_9 = Do_Abi_IsUppercase_9,
				IsLowercase_10 = Do_Abi_IsLowercase_10,
				IsIdStart_11 = Do_Abi_IsIdStart_11,
				IsIdContinue_12 = Do_Abi_IsIdContinue_12,
				IsGraphemeBase_13 = Do_Abi_IsGraphemeBase_13,
				IsGraphemeExtend_14 = Do_Abi_IsGraphemeExtend_14,
				GetNumericType_15 = Do_Abi_GetNumericType_15,
				GetGeneralCategory_16 = Do_Abi_GetGeneralCategory_16
			};
			nint* ptr = (nint*)ComWrappersSupport.AllocateVtableMemory(typeof(Vftbl), Marshal.SizeOf<IInspectable.Vftbl>() + sizeof(nint) * 17);
			Marshal.StructureToPtr(AbiToProjectionVftable, (nint)ptr, fDeleteOld: false);
			AbiToProjectionVftablePtr = (nint)ptr;
		}

		private static int Do_Abi_GetCodepointFromSurrogatePair_0(nint thisPtr, uint highSurrogate, uint lowSurrogate, out uint codepoint)
		{
			uint num = 0u;
			codepoint = 0u;
			try
			{
				num = ComWrappersSupport.FindObject<MS.Internal.WindowsRuntime.Windows.Data.Text.IUnicodeCharactersStatics>(thisPtr).GetCodepointFromSurrogatePair(highSurrogate, lowSurrogate);
				codepoint = num;
			}
			catch (Exception ex)
			{
				ExceptionHelpers.SetErrorInfo(ex);
				return ExceptionHelpers.GetHRForException(ex);
			}
			return 0;
		}

		private static int Do_Abi_GetSurrogatePairFromCodepoint_1(nint thisPtr, uint codepoint, out ushort highSurrogate, out ushort lowSurrogate)
		{
			highSurrogate = 0;
			lowSurrogate = 0;
			char highSurrogate2 = '\0';
			char lowSurrogate2 = '\0';
			try
			{
				ComWrappersSupport.FindObject<MS.Internal.WindowsRuntime.Windows.Data.Text.IUnicodeCharactersStatics>(thisPtr).GetSurrogatePairFromCodepoint(codepoint, out highSurrogate2, out lowSurrogate2);
				highSurrogate = highSurrogate2;
				lowSurrogate = lowSurrogate2;
			}
			catch (Exception ex)
			{
				ExceptionHelpers.SetErrorInfo(ex);
				return ExceptionHelpers.GetHRForException(ex);
			}
			return 0;
		}

		private static int Do_Abi_IsHighSurrogate_2(nint thisPtr, uint codepoint, out byte value)
		{
			bool flag = false;
			value = 0;
			try
			{
				flag = ComWrappersSupport.FindObject<MS.Internal.WindowsRuntime.Windows.Data.Text.IUnicodeCharactersStatics>(thisPtr).IsHighSurrogate(codepoint);
				value = (flag ? ((byte)1) : ((byte)0));
			}
			catch (Exception ex)
			{
				ExceptionHelpers.SetErrorInfo(ex);
				return ExceptionHelpers.GetHRForException(ex);
			}
			return 0;
		}

		private static int Do_Abi_IsLowSurrogate_3(nint thisPtr, uint codepoint, out byte value)
		{
			bool flag = false;
			value = 0;
			try
			{
				flag = ComWrappersSupport.FindObject<MS.Internal.WindowsRuntime.Windows.Data.Text.IUnicodeCharactersStatics>(thisPtr).IsLowSurrogate(codepoint);
				value = (flag ? ((byte)1) : ((byte)0));
			}
			catch (Exception ex)
			{
				ExceptionHelpers.SetErrorInfo(ex);
				return ExceptionHelpers.GetHRForException(ex);
			}
			return 0;
		}

		private static int Do_Abi_IsSupplementary_4(nint thisPtr, uint codepoint, out byte value)
		{
			bool flag = false;
			value = 0;
			try
			{
				flag = ComWrappersSupport.FindObject<MS.Internal.WindowsRuntime.Windows.Data.Text.IUnicodeCharactersStatics>(thisPtr).IsSupplementary(codepoint);
				value = (flag ? ((byte)1) : ((byte)0));
			}
			catch (Exception ex)
			{
				ExceptionHelpers.SetErrorInfo(ex);
				return ExceptionHelpers.GetHRForException(ex);
			}
			return 0;
		}

		private static int Do_Abi_IsNoncharacter_5(nint thisPtr, uint codepoint, out byte value)
		{
			bool flag = false;
			value = 0;
			try
			{
				flag = ComWrappersSupport.FindObject<MS.Internal.WindowsRuntime.Windows.Data.Text.IUnicodeCharactersStatics>(thisPtr).IsNoncharacter(codepoint);
				value = (flag ? ((byte)1) : ((byte)0));
			}
			catch (Exception ex)
			{
				ExceptionHelpers.SetErrorInfo(ex);
				return ExceptionHelpers.GetHRForException(ex);
			}
			return 0;
		}

		private static int Do_Abi_IsWhitespace_6(nint thisPtr, uint codepoint, out byte value)
		{
			bool flag = false;
			value = 0;
			try
			{
				flag = ComWrappersSupport.FindObject<MS.Internal.WindowsRuntime.Windows.Data.Text.IUnicodeCharactersStatics>(thisPtr).IsWhitespace(codepoint);
				value = (flag ? ((byte)1) : ((byte)0));
			}
			catch (Exception ex)
			{
				ExceptionHelpers.SetErrorInfo(ex);
				return ExceptionHelpers.GetHRForException(ex);
			}
			return 0;
		}

		private static int Do_Abi_IsAlphabetic_7(nint thisPtr, uint codepoint, out byte value)
		{
			bool flag = false;
			value = 0;
			try
			{
				flag = ComWrappersSupport.FindObject<MS.Internal.WindowsRuntime.Windows.Data.Text.IUnicodeCharactersStatics>(thisPtr).IsAlphabetic(codepoint);
				value = (flag ? ((byte)1) : ((byte)0));
			}
			catch (Exception ex)
			{
				ExceptionHelpers.SetErrorInfo(ex);
				return ExceptionHelpers.GetHRForException(ex);
			}
			return 0;
		}

		private static int Do_Abi_IsCased_8(nint thisPtr, uint codepoint, out byte value)
		{
			bool flag = false;
			value = 0;
			try
			{
				flag = ComWrappersSupport.FindObject<MS.Internal.WindowsRuntime.Windows.Data.Text.IUnicodeCharactersStatics>(thisPtr).IsCased(codepoint);
				value = (flag ? ((byte)1) : ((byte)0));
			}
			catch (Exception ex)
			{
				ExceptionHelpers.SetErrorInfo(ex);
				return ExceptionHelpers.GetHRForException(ex);
			}
			return 0;
		}

		private static int Do_Abi_IsUppercase_9(nint thisPtr, uint codepoint, out byte value)
		{
			bool flag = false;
			value = 0;
			try
			{
				flag = ComWrappersSupport.FindObject<MS.Internal.WindowsRuntime.Windows.Data.Text.IUnicodeCharactersStatics>(thisPtr).IsUppercase(codepoint);
				value = (flag ? ((byte)1) : ((byte)0));
			}
			catch (Exception ex)
			{
				ExceptionHelpers.SetErrorInfo(ex);
				return ExceptionHelpers.GetHRForException(ex);
			}
			return 0;
		}

		private static int Do_Abi_IsLowercase_10(nint thisPtr, uint codepoint, out byte value)
		{
			bool flag = false;
			value = 0;
			try
			{
				flag = ComWrappersSupport.FindObject<MS.Internal.WindowsRuntime.Windows.Data.Text.IUnicodeCharactersStatics>(thisPtr).IsLowercase(codepoint);
				value = (flag ? ((byte)1) : ((byte)0));
			}
			catch (Exception ex)
			{
				ExceptionHelpers.SetErrorInfo(ex);
				return ExceptionHelpers.GetHRForException(ex);
			}
			return 0;
		}

		private static int Do_Abi_IsIdStart_11(nint thisPtr, uint codepoint, out byte value)
		{
			bool flag = false;
			value = 0;
			try
			{
				flag = ComWrappersSupport.FindObject<MS.Internal.WindowsRuntime.Windows.Data.Text.IUnicodeCharactersStatics>(thisPtr).IsIdStart(codepoint);
				value = (flag ? ((byte)1) : ((byte)0));
			}
			catch (Exception ex)
			{
				ExceptionHelpers.SetErrorInfo(ex);
				return ExceptionHelpers.GetHRForException(ex);
			}
			return 0;
		}

		private static int Do_Abi_IsIdContinue_12(nint thisPtr, uint codepoint, out byte value)
		{
			bool flag = false;
			value = 0;
			try
			{
				flag = ComWrappersSupport.FindObject<MS.Internal.WindowsRuntime.Windows.Data.Text.IUnicodeCharactersStatics>(thisPtr).IsIdContinue(codepoint);
				value = (flag ? ((byte)1) : ((byte)0));
			}
			catch (Exception ex)
			{
				ExceptionHelpers.SetErrorInfo(ex);
				return ExceptionHelpers.GetHRForException(ex);
			}
			return 0;
		}

		private static int Do_Abi_IsGraphemeBase_13(nint thisPtr, uint codepoint, out byte value)
		{
			bool flag = false;
			value = 0;
			try
			{
				flag = ComWrappersSupport.FindObject<MS.Internal.WindowsRuntime.Windows.Data.Text.IUnicodeCharactersStatics>(thisPtr).IsGraphemeBase(codepoint);
				value = (flag ? ((byte)1) : ((byte)0));
			}
			catch (Exception ex)
			{
				ExceptionHelpers.SetErrorInfo(ex);
				return ExceptionHelpers.GetHRForException(ex);
			}
			return 0;
		}

		private static int Do_Abi_IsGraphemeExtend_14(nint thisPtr, uint codepoint, out byte value)
		{
			bool flag = false;
			value = 0;
			try
			{
				flag = ComWrappersSupport.FindObject<MS.Internal.WindowsRuntime.Windows.Data.Text.IUnicodeCharactersStatics>(thisPtr).IsGraphemeExtend(codepoint);
				value = (flag ? ((byte)1) : ((byte)0));
			}
			catch (Exception ex)
			{
				ExceptionHelpers.SetErrorInfo(ex);
				return ExceptionHelpers.GetHRForException(ex);
			}
			return 0;
		}

		private static int Do_Abi_GetNumericType_15(nint thisPtr, uint codepoint, out UnicodeNumericType value)
		{
			UnicodeNumericType unicodeNumericType = UnicodeNumericType.None;
			value = UnicodeNumericType.None;
			try
			{
				unicodeNumericType = ComWrappersSupport.FindObject<MS.Internal.WindowsRuntime.Windows.Data.Text.IUnicodeCharactersStatics>(thisPtr).GetNumericType(codepoint);
				value = unicodeNumericType;
			}
			catch (Exception ex)
			{
				ExceptionHelpers.SetErrorInfo(ex);
				return ExceptionHelpers.GetHRForException(ex);
			}
			return 0;
		}

		private static int Do_Abi_GetGeneralCategory_16(nint thisPtr, uint codepoint, out UnicodeGeneralCategory value)
		{
			UnicodeGeneralCategory unicodeGeneralCategory = UnicodeGeneralCategory.UppercaseLetter;
			value = UnicodeGeneralCategory.UppercaseLetter;
			try
			{
				unicodeGeneralCategory = ComWrappersSupport.FindObject<MS.Internal.WindowsRuntime.Windows.Data.Text.IUnicodeCharactersStatics>(thisPtr).GetGeneralCategory(codepoint);
				value = unicodeGeneralCategory;
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

	public static implicit operator IUnicodeCharactersStatics(IObjectReference obj)
	{
		if (obj == null)
		{
			return null;
		}
		return new IUnicodeCharactersStatics(obj);
	}

	public ObjectReference<I> AsInterface<I>()
	{
		return _obj.As<I>();
	}

	public A As<A>()
	{
		return _obj.AsType<A>();
	}

	public IUnicodeCharactersStatics(IObjectReference obj)
		: this(obj.As<Vftbl>())
	{
	}

	public IUnicodeCharactersStatics(ObjectReference<Vftbl> obj)
	{
		_obj = obj;
	}

	public uint GetCodepointFromSurrogatePair(uint highSurrogate, uint lowSurrogate)
	{
		uint codepoint = 0u;
		ExceptionHelpers.ThrowExceptionForHR(_obj.Vftbl.GetCodepointFromSurrogatePair_0(ThisPtr, highSurrogate, lowSurrogate, out codepoint));
		return codepoint;
	}

	public void GetSurrogatePairFromCodepoint(uint codepoint, out char highSurrogate, out char lowSurrogate)
	{
		ushort highSurrogate2 = 0;
		ushort lowSurrogate2 = 0;
		ExceptionHelpers.ThrowExceptionForHR(_obj.Vftbl.GetSurrogatePairFromCodepoint_1(ThisPtr, codepoint, out highSurrogate2, out lowSurrogate2));
		highSurrogate = (char)highSurrogate2;
		lowSurrogate = (char)lowSurrogate2;
	}

	public bool IsHighSurrogate(uint codepoint)
	{
		byte value = 0;
		ExceptionHelpers.ThrowExceptionForHR(_obj.Vftbl.IsHighSurrogate_2(ThisPtr, codepoint, out value));
		return value != 0;
	}

	public bool IsLowSurrogate(uint codepoint)
	{
		byte value = 0;
		ExceptionHelpers.ThrowExceptionForHR(_obj.Vftbl.IsLowSurrogate_3(ThisPtr, codepoint, out value));
		return value != 0;
	}

	public bool IsSupplementary(uint codepoint)
	{
		byte value = 0;
		ExceptionHelpers.ThrowExceptionForHR(_obj.Vftbl.IsSupplementary_4(ThisPtr, codepoint, out value));
		return value != 0;
	}

	public bool IsNoncharacter(uint codepoint)
	{
		byte value = 0;
		ExceptionHelpers.ThrowExceptionForHR(_obj.Vftbl.IsNoncharacter_5(ThisPtr, codepoint, out value));
		return value != 0;
	}

	public bool IsWhitespace(uint codepoint)
	{
		byte value = 0;
		ExceptionHelpers.ThrowExceptionForHR(_obj.Vftbl.IsWhitespace_6(ThisPtr, codepoint, out value));
		return value != 0;
	}

	public bool IsAlphabetic(uint codepoint)
	{
		byte value = 0;
		ExceptionHelpers.ThrowExceptionForHR(_obj.Vftbl.IsAlphabetic_7(ThisPtr, codepoint, out value));
		return value != 0;
	}

	public bool IsCased(uint codepoint)
	{
		byte value = 0;
		ExceptionHelpers.ThrowExceptionForHR(_obj.Vftbl.IsCased_8(ThisPtr, codepoint, out value));
		return value != 0;
	}

	public bool IsUppercase(uint codepoint)
	{
		byte value = 0;
		ExceptionHelpers.ThrowExceptionForHR(_obj.Vftbl.IsUppercase_9(ThisPtr, codepoint, out value));
		return value != 0;
	}

	public bool IsLowercase(uint codepoint)
	{
		byte value = 0;
		ExceptionHelpers.ThrowExceptionForHR(_obj.Vftbl.IsLowercase_10(ThisPtr, codepoint, out value));
		return value != 0;
	}

	public bool IsIdStart(uint codepoint)
	{
		byte value = 0;
		ExceptionHelpers.ThrowExceptionForHR(_obj.Vftbl.IsIdStart_11(ThisPtr, codepoint, out value));
		return value != 0;
	}

	public bool IsIdContinue(uint codepoint)
	{
		byte value = 0;
		ExceptionHelpers.ThrowExceptionForHR(_obj.Vftbl.IsIdContinue_12(ThisPtr, codepoint, out value));
		return value != 0;
	}

	public bool IsGraphemeBase(uint codepoint)
	{
		byte value = 0;
		ExceptionHelpers.ThrowExceptionForHR(_obj.Vftbl.IsGraphemeBase_13(ThisPtr, codepoint, out value));
		return value != 0;
	}

	public bool IsGraphemeExtend(uint codepoint)
	{
		byte value = 0;
		ExceptionHelpers.ThrowExceptionForHR(_obj.Vftbl.IsGraphemeExtend_14(ThisPtr, codepoint, out value));
		return value != 0;
	}

	public UnicodeNumericType GetNumericType(uint codepoint)
	{
		UnicodeNumericType value = UnicodeNumericType.None;
		ExceptionHelpers.ThrowExceptionForHR(_obj.Vftbl.GetNumericType_15(ThisPtr, codepoint, out value));
		return value;
	}

	public UnicodeGeneralCategory GetGeneralCategory(uint codepoint)
	{
		UnicodeGeneralCategory value = UnicodeGeneralCategory.UppercaseLetter;
		ExceptionHelpers.ThrowExceptionForHR(_obj.Vftbl.GetGeneralCategory_16(ThisPtr, codepoint, out value));
		return value;
	}
}
