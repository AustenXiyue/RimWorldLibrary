using System;
using System.Runtime.CompilerServices;

namespace MonoMod;

internal static class ILHelpers
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static T TailCallDelegatePtr<T>(IntPtr source)
	{
		//IL_0001: Ignored invalid 'tail' prefix
		return ((delegate*<T>)source)();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T TailCallFunc<T>(Func<T> func)
	{
		return func();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ref T? UnboxAnyUnsafe<T>(ref object? box)
	{
		if (default(T) == null)
		{
			return ref Unsafe.As<object?, T?>(ref box);
		}
		return ref (T)box;
	}
}
