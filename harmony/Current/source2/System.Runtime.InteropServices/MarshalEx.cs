using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Runtime.InteropServices;

internal static class MarshalEx
{
	private static readonly MethodInfo? Marshal_SetLastWin32Error_Meth = typeof(Marshal).GetMethod("SetLastPInvokeError", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) ?? typeof(Marshal).GetMethod("SetLastWin32Error", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

	private static readonly Action<int>? Marshal_SetLastWin32Error = (((object)Marshal_SetLastWin32Error_Meth == null) ? null : ((Action<int>)Delegate.CreateDelegate(typeof(Action<int>), Marshal_SetLastWin32Error_Meth)));

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int GetLastPInvokeError()
	{
		return Marshal.GetLastWin32Error();
	}

	public static void SetLastPInvokeError(int error)
	{
		(Marshal_SetLastWin32Error ?? throw new PlatformNotSupportedException("Cannot set last P/Invoke error (no method Marshal.SetLastWin32Error or Marshal.SetLastPInvokeError)"))(error);
	}
}
