using System;
using System.Runtime.CompilerServices;

namespace MonoMod.Core.Platforms.Architectures;

internal static class x86Shared
{
	public sealed class Rel32Kind : DetourKindBase
	{
		public static readonly Rel32Kind Instance = new Rel32Kind();

		public override int Size => 5;

		public override int GetBytes(IntPtr from, IntPtr to, Span<byte> buffer, object? data, out IDisposable? allocHandle)
		{
			buffer[0] = 233;
			Unsafe.WriteUnaligned(ref buffer[1], (int)((nint)to - ((nint)from + 5)));
			allocHandle = null;
			return Size;
		}

		public override bool TryGetRetargetInfo(NativeDetourInfo orig, IntPtr to, int maxSize, out NativeDetourInfo retargetInfo)
		{
			nint num = (nint)to - ((nint)orig.From + 5);
			if (Is32Bit(num) || Is32Bit(-num))
			{
				retargetInfo = new NativeDetourInfo(orig.From, to, Instance, null);
				return true;
			}
			retargetInfo = default(NativeDetourInfo);
			return false;
		}

		public override int DoRetarget(NativeDetourInfo origInfo, IntPtr to, Span<byte> buffer, object? data, out IDisposable? allocationHandle, out bool needsRepatch, out bool disposeOldAlloc)
		{
			needsRepatch = true;
			disposeOldAlloc = true;
			return GetBytes(origInfo.From, to, buffer, data, out allocationHandle);
		}
	}

	public static void FixSizeHint(ref int sizeHint)
	{
		if (sizeHint < 0)
		{
			sizeHint = int.MaxValue;
		}
	}

	public unsafe static bool TryRel32Detour(nint from, nint to, int sizeHint, out NativeDetourInfo info)
	{
		nint num = to - (from + 5);
		if (sizeHint >= Rel32Kind.Instance.Size && (Is32Bit(num) || Is32Bit(-num)) && *(byte*)(from + 5) != 95)
		{
			info = new NativeDetourInfo(from, to, Rel32Kind.Instance, null);
			return true;
		}
		info = default(NativeDetourInfo);
		return false;
	}

	public static bool Is32Bit(long to)
	{
		return (to & 0x7FFFFFFF) == to;
	}
}
