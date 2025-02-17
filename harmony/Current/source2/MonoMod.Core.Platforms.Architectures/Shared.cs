using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using MonoMod.Utils;

namespace MonoMod.Core.Platforms.Architectures;

internal static class Shared
{
	public unsafe static ReadOnlyMemory<IAllocatedMemory> CreateVtableStubs(ISystem system, IntPtr vtableBase, int vtableSize, ReadOnlySpan<byte> stubData, int indexOffs, bool premulOffset)
	{
		int maxSize = system.MemoryAllocator.MaxSize;
		int num = stubData.Length * vtableSize;
		int num2 = num / maxSize;
		int num3 = maxSize / stubData.Length;
		int num4 = num3 * stubData.Length;
		int num5 = num % num4;
		IAllocatedMemory[] array = new IAllocatedMemory[num2 + ((num5 != 0) ? 1 : 0)];
		byte[] array2 = ArrayPool<byte>.Shared.Rent(num4);
		Span<byte> backup = array2.AsSpan();
		Span<byte> span = backup.Slice(0, num4);
		for (int j = 0; j < num3; j++)
		{
			stubData.CopyTo(span.Slice(j * stubData.Length));
		}
		ref IntPtr vtblBase2 = ref Unsafe.AsRef<IntPtr>((void*)vtableBase);
		AllocationRequest allocationRequest = new AllocationRequest(num4);
		allocationRequest.Alignment = IntPtr.Size;
		allocationRequest.Executable = true;
		AllocationRequest allocationRequest2 = allocationRequest;
		for (int k = 0; k < num2; k++)
		{
			Helpers.Assert(system.MemoryAllocator.TryAllocate(allocationRequest2, out IAllocatedMemory allocated), null, "system.MemoryAllocator.TryAllocate(allocReq, out var alloc)");
			array[k] = allocated;
			FillBufferIndicies(stubData.Length, indexOffs, num3, k, span, premulOffset);
			FillVtbl(stubData.Length, num3 * k, ref vtblBase2, num3, allocated.BaseAddress);
			IntPtr baseAddress = allocated.BaseAddress;
			ReadOnlySpan<byte> data = span;
			backup = default(Span<byte>);
			system.PatchData(PatchTargetKind.Executable, baseAddress, data, backup);
		}
		if (num5 > 0)
		{
			allocationRequest2 = allocationRequest2 with
			{
				Size = num5
			};
			Helpers.Assert(system.MemoryAllocator.TryAllocate(allocationRequest2, out IAllocatedMemory allocated2), null, "system.MemoryAllocator.TryAllocate(allocReq, out var alloc)");
			array[array.Length - 1] = allocated2;
			FillBufferIndicies(stubData.Length, indexOffs, num3, num2, span, premulOffset);
			FillVtbl(stubData.Length, num3 * num2, ref vtblBase2, num5 / stubData.Length, allocated2.BaseAddress);
			IntPtr baseAddress2 = allocated2.BaseAddress;
			ReadOnlySpan<byte> data2 = span.Slice(0, num5);
			backup = default(Span<byte>);
			system.PatchData(PatchTargetKind.Executable, baseAddress2, data2, backup);
		}
		ArrayPool<byte>.Shared.Return(array2);
		return array;
		static void FillBufferIndicies(int stubSize, int indexOffs, int numPerAlloc, int i, Span<byte> mainAllocBuf, bool premul)
		{
			for (int l = 0; l < numPerAlloc; l++)
			{
				ref byte destination = ref mainAllocBuf[l * stubSize + indexOffs];
				uint num6 = (uint)(numPerAlloc * i + l);
				if (premul)
				{
					num6 *= (uint)IntPtr.Size;
				}
				Unsafe.WriteUnaligned(ref destination, num6);
			}
		}
		static void FillVtbl(int stubSize, int baseIndex, ref IntPtr vtblBase, int numEntries, nint baseAddr)
		{
			for (int m = 0; m < numEntries; m++)
			{
				Unsafe.Add(ref vtblBase, baseIndex + m) = baseAddr + stubSize * m;
			}
		}
	}
}
