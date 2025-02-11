using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MS.Internal.Interop;

[StructLayout(LayoutKind.Explicit)]
internal struct HRESULT
{
	[FieldOffset(0)]
	private readonly uint _value;

	public static readonly HRESULT S_OK = new HRESULT(0u);

	public static readonly HRESULT S_FALSE = new HRESULT(1u);

	public static readonly HRESULT E_NOTIMPL = new HRESULT(2147500033u);

	public static readonly HRESULT E_NOINTERFACE = new HRESULT(2147500034u);

	public static readonly HRESULT E_POINTER = new HRESULT(2147500035u);

	public static readonly HRESULT E_ABORT = new HRESULT(2147500036u);

	public static readonly HRESULT E_FAIL = new HRESULT(2147500037u);

	public static readonly HRESULT E_UNEXPECTED = new HRESULT(2147549183u);

	public static readonly HRESULT DISP_E_MEMBERNOTFOUND = new HRESULT(2147614723u);

	public static readonly HRESULT DISP_E_TYPEMISMATCH = new HRESULT(2147614725u);

	public static readonly HRESULT DISP_E_UNKNOWNNAME = new HRESULT(2147614726u);

	public static readonly HRESULT DISP_E_EXCEPTION = new HRESULT(2147614729u);

	public static readonly HRESULT DISP_E_OVERFLOW = new HRESULT(2147614730u);

	public static readonly HRESULT DISP_E_BADINDEX = new HRESULT(2147614731u);

	public static readonly HRESULT DISP_E_BADPARAMCOUNT = new HRESULT(2147614734u);

	public static readonly HRESULT DISP_E_PARAMNOTOPTIONAL = new HRESULT(2147614735u);

	public static readonly HRESULT SCRIPT_E_REPORTED = new HRESULT(2147614977u);

	public static readonly HRESULT STG_E_INVALIDFUNCTION = new HRESULT(2147680257u);

	public static readonly HRESULT DESTS_E_NO_MATCHING_ASSOC_HANDLER = new HRESULT(2147749635u);

	public static readonly HRESULT E_ACCESSDENIED = new HRESULT(2147942405u);

	public static readonly HRESULT E_OUTOFMEMORY = new HRESULT(2147942414u);

	public static readonly HRESULT E_INVALIDARG = new HRESULT(2147942487u);

	public static readonly HRESULT COR_E_OBJECTDISPOSED = new HRESULT(2148734498u);

	public static readonly HRESULT WC_E_GREATERTHAN = new HRESULT(3222072867u);

	public static readonly HRESULT WC_E_SYNTAX = new HRESULT(3222072877u);

	public Facility Facility => GetFacility((int)_value);

	public int Code => GetCode((int)_value);

	public bool Succeeded => (int)_value >= 0;

	public bool Failed => (int)_value < 0;

	public HRESULT(uint i)
	{
		_value = i;
	}

	public static HRESULT Make(bool severe, Facility facility, int code)
	{
		return new HRESULT((uint)((severe ? int.MinValue : 0) | ((int)facility << 16) | code));
	}

	public static Facility GetFacility(int errorCode)
	{
		return (Facility)((errorCode >> 16) & 0x1FFF);
	}

	public static int GetCode(int error)
	{
		return error & 0xFFFF;
	}

	public override string ToString()
	{
		FieldInfo[] fields = typeof(HRESULT).GetFields(BindingFlags.Static | BindingFlags.Public);
		foreach (FieldInfo fieldInfo in fields)
		{
			if (fieldInfo.FieldType == typeof(HRESULT) && (HRESULT)fieldInfo.GetValue(null) == this)
			{
				return fieldInfo.Name;
			}
		}
		if (Facility == Facility.Win32)
		{
			fields = typeof(Win32Error).GetFields(BindingFlags.Static | BindingFlags.Public);
			foreach (FieldInfo fieldInfo2 in fields)
			{
				if (fieldInfo2.FieldType == typeof(Win32Error) && (HRESULT)(Win32Error)fieldInfo2.GetValue(null) == this)
				{
					return "HRESULT_FROM_WIN32(" + fieldInfo2.Name + ")";
				}
			}
		}
		return string.Format(CultureInfo.InvariantCulture, "0x{0:X8}", _value);
	}

	public override bool Equals(object obj)
	{
		try
		{
			return ((HRESULT)obj)._value == _value;
		}
		catch (InvalidCastException)
		{
			return false;
		}
	}

	public override int GetHashCode()
	{
		return _value.GetHashCode();
	}

	public static bool operator ==(HRESULT hrLeft, HRESULT hrRight)
	{
		return hrLeft._value == hrRight._value;
	}

	public static bool operator !=(HRESULT hrLeft, HRESULT hrRight)
	{
		return !(hrLeft == hrRight);
	}

	public void ThrowIfFailed()
	{
		ThrowIfFailed(null);
	}

	public void ThrowIfFailed(string message)
	{
		Exception exception = GetException(message);
		if (exception != null)
		{
			throw exception;
		}
	}

	public Exception GetException()
	{
		return GetException(null);
	}

	public Exception GetException(string message)
	{
		if (!Failed)
		{
			return null;
		}
		Exception ex = Marshal.GetExceptionForHR((int)_value, new IntPtr(-1));
		if (ex.GetType() == typeof(COMException))
		{
			ex = ((Facility != Facility.Win32) ? ((ExternalException)new COMException(message ?? ex.Message, (int)_value)) : ((ExternalException)((!string.IsNullOrEmpty(message)) ? new Win32Exception(Code, message) : new Win32Exception(Code))));
		}
		else if (!string.IsNullOrEmpty(message))
		{
			ConstructorInfo constructor = ex.GetType().GetConstructor(new Type[1] { typeof(string) });
			if (null != constructor)
			{
				ex = constructor.Invoke(new object[1] { message }) as Exception;
			}
		}
		return ex;
	}
}
