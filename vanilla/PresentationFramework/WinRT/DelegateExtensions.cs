using System;
using System.Runtime.InteropServices;

namespace WinRT;

internal static class DelegateExtensions
{
	public static void DynamicInvokeAbi(this Delegate del, object[] invoke_params)
	{
		Marshal.ThrowExceptionForHR((int)del.DynamicInvoke(invoke_params));
	}

	public static T AsDelegate<T>(this MulticastDelegate del)
	{
		return Marshal.GetDelegateForFunctionPointer<T>(Marshal.GetFunctionPointerForDelegate(del));
	}
}
