using System;
using MonoMod.Core.Utils;
using MonoMod.Utils;

namespace MonoMod.Core.Platforms;

internal interface IArchitecture
{
	ArchitectureKind Target { get; }

	ArchitectureFeature Features { get; }

	BytePatternCollection KnownMethodThunks { get; }

	IAltEntryFactory AltEntryFactory { get; }

	NativeDetourInfo ComputeDetourInfo(IntPtr from, IntPtr target, int maxSizeHint = -1);

	int GetDetourBytes(NativeDetourInfo info, Span<byte> buffer, out IDisposable? allocationHandle);

	NativeDetourInfo ComputeRetargetInfo(NativeDetourInfo detour, IntPtr target, int maxSizeHint = -1);

	int GetRetargetBytes(NativeDetourInfo original, NativeDetourInfo retarget, Span<byte> buffer, out IDisposable? allocationHandle, out bool needsRepatch, out bool disposeOldAlloc);

	ReadOnlyMemory<IAllocatedMemory> CreateNativeVtableProxyStubs(IntPtr vtableBase, int vtableSize);

	IAllocatedMemory CreateSpecialEntryStub(IntPtr target, IntPtr argument);
}
