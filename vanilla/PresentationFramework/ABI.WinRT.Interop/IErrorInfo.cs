using System;
using System.Runtime.InteropServices;
using WinRT;
using WinRT.Interop;

namespace ABI.WinRT.Interop;

[Guid("1CF2B120-547D-101B-8E65-08002B2BD119")]
internal class IErrorInfo : global::WinRT.Interop.IErrorInfo
{
	[Guid("1CF2B120-547D-101B-8E65-08002B2BD119")]
	internal struct Vftbl
	{
		internal delegate int _GetGuid(nint thisPtr, out Guid guid);

		internal delegate int _GetBstr(nint thisPtr, out nint bstr);

		public IUnknownVftbl IUnknownVftbl;

		public _GetGuid GetGuid_0;

		public _GetBstr GetSource_1;

		public _GetBstr GetDescription_2;

		public _GetBstr GetHelpFile_3;

		public _GetBstr GetHelpFileContent_4;

		private static readonly Vftbl AbiToProjectionVftable;

		public static readonly nint AbiToProjectionVftablePtr;

		unsafe static Vftbl()
		{
			AbiToProjectionVftable = new Vftbl
			{
				IUnknownVftbl = IUnknownVftbl.AbiToProjectionVftbl,
				GetGuid_0 = Do_Abi_GetGuid_0,
				GetSource_1 = Do_Abi_GetSource_1,
				GetDescription_2 = Do_Abi_GetDescription_2,
				GetHelpFile_3 = Do_Abi_GetHelpFile_3,
				GetHelpFileContent_4 = Do_Abi_GetHelpFileContent_4
			};
			nint* ptr = (nint*)Marshal.AllocCoTaskMem(Marshal.SizeOf<Vftbl>());
			Marshal.StructureToPtr(AbiToProjectionVftable, (nint)ptr, fDeleteOld: false);
			AbiToProjectionVftablePtr = (nint)ptr;
		}

		private static int Do_Abi_GetGuid_0(nint thisPtr, out Guid guid)
		{
			guid = default(Guid);
			try
			{
				guid = ComWrappersSupport.FindObject<global::WinRT.Interop.IErrorInfo>(thisPtr).GetGuid();
			}
			catch (Exception ex)
			{
				ExceptionHelpers.SetErrorInfo(ex);
				return ExceptionHelpers.GetHRForException(ex);
			}
			return 0;
		}

		private static int Do_Abi_GetSource_1(nint thisPtr, out nint source)
		{
			source = IntPtr.Zero;
			try
			{
				string source2 = ComWrappersSupport.FindObject<global::WinRT.Interop.IErrorInfo>(thisPtr).GetSource();
				source = Marshal.StringToBSTR(source2);
			}
			catch (Exception ex)
			{
				Marshal.FreeBSTR(source);
				ExceptionHelpers.SetErrorInfo(ex);
				return ExceptionHelpers.GetHRForException(ex);
			}
			return 0;
		}

		private static int Do_Abi_GetDescription_2(nint thisPtr, out nint description)
		{
			description = IntPtr.Zero;
			try
			{
				string description2 = ComWrappersSupport.FindObject<global::WinRT.Interop.IErrorInfo>(thisPtr).GetDescription();
				description = Marshal.StringToBSTR(description2);
			}
			catch (Exception ex)
			{
				Marshal.FreeBSTR(description);
				ExceptionHelpers.SetErrorInfo(ex);
				return ExceptionHelpers.GetHRForException(ex);
			}
			return 0;
		}

		private static int Do_Abi_GetHelpFile_3(nint thisPtr, out nint helpFile)
		{
			helpFile = IntPtr.Zero;
			try
			{
				string helpFile2 = ComWrappersSupport.FindObject<global::WinRT.Interop.IErrorInfo>(thisPtr).GetHelpFile();
				helpFile = Marshal.StringToBSTR(helpFile2);
			}
			catch (Exception ex)
			{
				Marshal.FreeBSTR(helpFile);
				ExceptionHelpers.SetErrorInfo(ex);
				return ExceptionHelpers.GetHRForException(ex);
			}
			return 0;
		}

		private static int Do_Abi_GetHelpFileContent_4(nint thisPtr, out nint helpFileContent)
		{
			helpFileContent = IntPtr.Zero;
			try
			{
				string helpFileContent2 = ComWrappersSupport.FindObject<global::WinRT.Interop.IErrorInfo>(thisPtr).GetHelpFileContent();
				helpFileContent = Marshal.StringToBSTR(helpFileContent2);
			}
			catch (Exception ex)
			{
				Marshal.FreeBSTR(helpFileContent);
				ExceptionHelpers.SetErrorInfo(ex);
				return ExceptionHelpers.GetHRForException(ex);
			}
			return 0;
		}
	}

	protected readonly ObjectReference<Vftbl> _obj;

	public nint ThisPtr => _obj.ThisPtr;

	public static ObjectReference<Vftbl> FromAbi(nint thisPtr)
	{
		return ObjectReference<Vftbl>.FromAbi(thisPtr);
	}

	public static implicit operator IErrorInfo(IObjectReference obj)
	{
		if (obj == null)
		{
			return null;
		}
		return new IErrorInfo(obj);
	}

	public static implicit operator IErrorInfo(ObjectReference<Vftbl> obj)
	{
		if (obj == null)
		{
			return null;
		}
		return new IErrorInfo(obj);
	}

	public ObjectReference<I> AsInterface<I>()
	{
		return _obj.As<I>();
	}

	public A As<A>()
	{
		return _obj.AsType<A>();
	}

	public IErrorInfo(IObjectReference obj)
		: this(obj.As<Vftbl>())
	{
	}

	public IErrorInfo(ObjectReference<Vftbl> obj)
	{
		_obj = obj;
	}

	public Guid GetGuid()
	{
		Marshal.ThrowExceptionForHR(_obj.Vftbl.GetGuid_0(ThisPtr, out var guid));
		return guid;
	}

	public string GetSource()
	{
		nint bstr = 0;
		try
		{
			Marshal.ThrowExceptionForHR(_obj.Vftbl.GetSource_1(ThisPtr, out bstr));
			return (bstr != IntPtr.Zero) ? Marshal.PtrToStringBSTR(bstr) : string.Empty;
		}
		finally
		{
			Marshal.FreeBSTR(bstr);
		}
	}

	public string GetDescription()
	{
		nint bstr = 0;
		try
		{
			Marshal.ThrowExceptionForHR(_obj.Vftbl.GetDescription_2(ThisPtr, out bstr));
			return (bstr != IntPtr.Zero) ? Marshal.PtrToStringBSTR(bstr) : string.Empty;
		}
		finally
		{
			Marshal.FreeBSTR(bstr);
		}
	}

	public string GetHelpFile()
	{
		nint bstr = 0;
		try
		{
			Marshal.ThrowExceptionForHR(_obj.Vftbl.GetHelpFile_3(ThisPtr, out bstr));
			return (bstr != IntPtr.Zero) ? Marshal.PtrToStringBSTR(bstr) : string.Empty;
		}
		finally
		{
			Marshal.FreeBSTR(bstr);
		}
	}

	public string GetHelpFileContent()
	{
		nint bstr = 0;
		try
		{
			Marshal.ThrowExceptionForHR(_obj.Vftbl.GetHelpFileContent_4(ThisPtr, out bstr));
			return (bstr != IntPtr.Zero) ? Marshal.PtrToStringBSTR(bstr) : string.Empty;
		}
		finally
		{
			Marshal.FreeBSTR(bstr);
		}
	}
}
