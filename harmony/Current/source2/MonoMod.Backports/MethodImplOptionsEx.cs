using System.Runtime.CompilerServices;

namespace MonoMod.Backports;

internal static class MethodImplOptionsEx
{
	public const MethodImplOptions Unmanaged = MethodImplOptions.Unmanaged;

	public const MethodImplOptions NoInlining = MethodImplOptions.NoInlining;

	public const MethodImplOptions ForwardRef = MethodImplOptions.ForwardRef;

	public const MethodImplOptions Synchronized = MethodImplOptions.Synchronized;

	public const MethodImplOptions NoOptimization = MethodImplOptions.NoOptimization;

	public const MethodImplOptions PreserveSig = MethodImplOptions.PreserveSig;

	public const MethodImplOptions AggressiveInlining = MethodImplOptions.AggressiveInlining;

	public const MethodImplOptions AggressiveOptimization = (MethodImplOptions)512;

	public const MethodImplOptions InternalCall = MethodImplOptions.InternalCall;
}
