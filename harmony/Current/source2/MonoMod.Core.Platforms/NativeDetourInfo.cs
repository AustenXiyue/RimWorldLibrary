using System;

namespace MonoMod.Core.Platforms;

internal readonly record struct NativeDetourInfo(IntPtr From, IntPtr To, INativeDetourKind InternalKind, IDisposable? InternalData)
{
	public int Size => InternalKind.Size;
}
