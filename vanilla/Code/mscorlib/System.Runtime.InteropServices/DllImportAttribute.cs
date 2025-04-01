using System.Reflection;
using System.Security;

namespace System.Runtime.InteropServices;

/// <summary>Indicates that the attributed method is exposed by an unmanaged dynamic-link library (DLL) as a static entry point.</summary>
[ComVisible(true)]
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class DllImportAttribute : Attribute
{
	internal string _val;

	/// <summary>Indicates the name or ordinal of the DLL entry point to be called.</summary>
	public string EntryPoint;

	/// <summary>Indicates how to marshal string parameters to the method and controls name mangling.</summary>
	public CharSet CharSet;

	/// <summary>Indicates whether the callee calls the SetLastError Win32 API function before returning from the attributed method.</summary>
	public bool SetLastError;

	/// <summary>Controls whether the <see cref="F:System.Runtime.InteropServices.DllImportAttribute.CharSet" /> field causes the common language runtime to search an unmanaged DLL for entry-point names other than the one specified.</summary>
	public bool ExactSpelling;

	/// <summary>Indicates whether unmanaged methods that have HRESULT or retval return values are directly translated or whether HRESULT or retval return values are automatically converted to exceptions.</summary>
	public bool PreserveSig;

	/// <summary>Indicates the calling convention of an entry point.</summary>
	public CallingConvention CallingConvention;

	/// <summary>Enables or disables best-fit mapping behavior when converting Unicode characters to ANSI characters.</summary>
	public bool BestFitMapping;

	/// <summary>Enables or disables the throwing of an exception on an unmappable Unicode character that is converted to an ANSI "?" character.</summary>
	public bool ThrowOnUnmappableChar;

	/// <summary>Gets the name of the DLL file that contains the entry point.</summary>
	/// <returns>The name of the DLL file that contains the entry point.</returns>
	public string Value => _val;

	[SecurityCritical]
	internal static Attribute GetCustomAttribute(RuntimeMethodInfo method)
	{
		if ((method.Attributes & MethodAttributes.PinvokeImpl) == 0)
		{
			return null;
		}
		string dllName = null;
		_ = method.MetadataToken;
		PInvokeAttributes flags = PInvokeAttributes.CharSetNotSpec;
		((MonoMethod)method).GetPInvoke(out flags, out var entryPoint, out dllName);
		CharSet charSet = CharSet.None;
		switch (flags & PInvokeAttributes.CharSetMask)
		{
		case PInvokeAttributes.CharSetNotSpec:
			charSet = CharSet.None;
			break;
		case PInvokeAttributes.CharSetAnsi:
			charSet = CharSet.Ansi;
			break;
		case PInvokeAttributes.CharSetUnicode:
			charSet = CharSet.Unicode;
			break;
		case PInvokeAttributes.CharSetMask:
			charSet = CharSet.Auto;
			break;
		}
		CallingConvention callingConvention = CallingConvention.Cdecl;
		switch (flags & PInvokeAttributes.CallConvMask)
		{
		case PInvokeAttributes.CallConvWinapi:
			callingConvention = CallingConvention.Winapi;
			break;
		case PInvokeAttributes.CallConvCdecl:
			callingConvention = CallingConvention.Cdecl;
			break;
		case PInvokeAttributes.CallConvStdcall:
			callingConvention = CallingConvention.StdCall;
			break;
		case PInvokeAttributes.CallConvThiscall:
			callingConvention = CallingConvention.ThisCall;
			break;
		case PInvokeAttributes.CallConvFastcall:
			callingConvention = CallingConvention.FastCall;
			break;
		}
		bool exactSpelling = (flags & PInvokeAttributes.NoMangle) != 0;
		bool setLastError = (flags & PInvokeAttributes.SupportsLastError) != 0;
		bool bestFitMapping = (flags & PInvokeAttributes.BestFitMask) == PInvokeAttributes.BestFitEnabled;
		bool throwOnUnmappableChar = (flags & PInvokeAttributes.ThrowOnUnmappableCharMask) == PInvokeAttributes.ThrowOnUnmappableCharEnabled;
		bool preserveSig = (method.GetMethodImplementationFlags() & MethodImplAttributes.PreserveSig) != 0;
		return new DllImportAttribute(dllName, entryPoint, charSet, exactSpelling, setLastError, preserveSig, callingConvention, bestFitMapping, throwOnUnmappableChar);
	}

	internal static bool IsDefined(RuntimeMethodInfo method)
	{
		return (method.Attributes & MethodAttributes.PinvokeImpl) != 0;
	}

	internal DllImportAttribute(string dllName, string entryPoint, CharSet charSet, bool exactSpelling, bool setLastError, bool preserveSig, CallingConvention callingConvention, bool bestFitMapping, bool throwOnUnmappableChar)
	{
		_val = dllName;
		EntryPoint = entryPoint;
		CharSet = charSet;
		ExactSpelling = exactSpelling;
		SetLastError = setLastError;
		PreserveSig = preserveSig;
		CallingConvention = callingConvention;
		BestFitMapping = bestFitMapping;
		ThrowOnUnmappableChar = throwOnUnmappableChar;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.InteropServices.DllImportAttribute" /> class with the name of the DLL containing the method to import.</summary>
	/// <param name="dllName">The name of the DLL that contains the unmanaged method. This can include an assembly display name, if the DLL is included in an assembly.</param>
	public DllImportAttribute(string dllName)
	{
		_val = dllName;
	}
}
