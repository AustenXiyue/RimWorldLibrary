using System;
using System.Runtime.CompilerServices;
using MonoMod.Core.Platforms.Architectures.AltEntryFactories;
using MonoMod.Core.Utils;
using MonoMod.Utils;

namespace MonoMod.Core.Platforms.Architectures;

internal sealed class x86_64Arch : IArchitecture
{
	private sealed class Abs64Kind : DetourKindBase
	{
		public static readonly Abs64Kind Instance = new Abs64Kind();

		public override int Size => 14;

		public override int GetBytes(IntPtr from, IntPtr to, Span<byte> buffer, object? data, out IDisposable? allocHandle)
		{
			buffer[0] = byte.MaxValue;
			buffer[1] = 37;
			Unsafe.WriteUnaligned(ref buffer[2], 0);
			Unsafe.WriteUnaligned(ref buffer[6], (long)to);
			allocHandle = null;
			return Size;
		}

		public override bool TryGetRetargetInfo(NativeDetourInfo orig, IntPtr to, int maxSize, out NativeDetourInfo retargetInfo)
		{
			retargetInfo = orig with
			{
				To = to
			};
			return true;
		}

		public override int DoRetarget(NativeDetourInfo origInfo, IntPtr to, Span<byte> buffer, object? data, out IDisposable? allocationHandle, out bool needsRepatch, out bool disposeOldAlloc)
		{
			needsRepatch = true;
			disposeOldAlloc = true;
			return GetBytes(origInfo.From, to, buffer, data, out allocationHandle);
		}
	}

	private sealed class Rel32Ind64Kind : DetourKindBase
	{
		public static readonly Rel32Ind64Kind Instance = new Rel32Ind64Kind();

		public override int Size => 6;

		public override int GetBytes(IntPtr from, IntPtr to, Span<byte> buffer, object? data, out IDisposable? allocHandle)
		{
			Helpers.ThrowIfArgumentNull(data, "data");
			IAllocatedMemory allocatedMemory = (IAllocatedMemory)data;
			buffer[0] = byte.MaxValue;
			buffer[1] = 37;
			Unsafe.WriteUnaligned(ref buffer[2], (int)((nint)allocatedMemory.BaseAddress - ((nint)from + 6)));
			Unsafe.WriteUnaligned(ref allocatedMemory.Memory[0], to);
			allocHandle = allocatedMemory;
			return Size;
		}

		public override bool TryGetRetargetInfo(NativeDetourInfo orig, IntPtr to, int maxSize, out NativeDetourInfo retargetInfo)
		{
			retargetInfo = orig with
			{
				To = to
			};
			return true;
		}

		public override int DoRetarget(NativeDetourInfo origInfo, IntPtr to, Span<byte> buffer, object? data, out IDisposable? allocationHandle, out bool needsRepatch, out bool disposeOldAlloc)
		{
			if (origInfo.InternalKind == this)
			{
				needsRepatch = false;
				disposeOldAlloc = false;
				Helpers.ThrowIfArgumentNull(data, "data");
				IAllocatedMemory allocatedMemory = (IAllocatedMemory)data;
				Unsafe.WriteUnaligned(ref allocatedMemory.Memory[0], to);
				allocationHandle = allocatedMemory;
				return 0;
			}
			needsRepatch = true;
			disposeOldAlloc = true;
			return GetBytes(origInfo.From, to, buffer, data, out allocationHandle);
		}
	}

	private BytePatternCollection? lazyKnownMethodThunks;

	private readonly ISystem system;

	private const int VtblProxyStubIdxOffs = 9;

	private const bool VtblProxyStubIdxPremul = true;

	private const int SpecEntryStubArgOffs = 2;

	private const int SpecEntryStubTargetOffs = 12;

	public ArchitectureKind Target => ArchitectureKind.x86_64;

	public ArchitectureFeature Features => ArchitectureFeature.Immediate64 | ArchitectureFeature.CreateAltEntryPoint;

	public unsafe BytePatternCollection KnownMethodThunks => Helpers.GetOrInit(ref lazyKnownMethodThunks, (delegate*<BytePatternCollection>)(&CreateKnownMethodThunks));

	public IAltEntryFactory AltEntryFactory { get; }

	private static ReadOnlySpan<byte> VtblProxyStubWin => new byte[16]
	{
		72, 139, 73, 8, 72, 139, 1, 255, 160, 85,
		85, 85, 85, 204, 204, 204
	};

	private static ReadOnlySpan<byte> VtblProxyStubSysV => new byte[16]
	{
		72, 139, 127, 8, 72, 139, 7, 255, 160, 85,
		85, 85, 85, 204, 204, 204
	};

	private static ReadOnlySpan<byte> SpecEntryStub => new byte[23]
	{
		72, 184, 0, 0, 0, 0, 0, 0, 0, 0,
		73, 186, 0, 0, 0, 0, 0, 0, 0, 0,
		65, 255, 226
	};

	private static BytePatternCollection CreateKnownMethodThunks()
	{
		RuntimeKind runtime = PlatformDetection.Runtime;
		if ((uint)(runtime - 1) <= 1u)
		{
			return new BytePatternCollection(new BytePattern(new AddressMeaning(AddressKind.Abs64), true, 72, 133, 201, 116, 65280, 72, 139, 1, 73, 65280, 65280, 65280, 65280, 65280, 65280, 65280, 65280, 65280, 73, 59, 194, 116, 65280, 72, 184, 65282, 65282, 65282, 65282, 65282, 65282, 65282, 65282), new BytePattern(new AddressMeaning(AddressKind.Rel32, 5), true, 233, 65282, 65282, 65282, 65282, 95), new BytePattern(new AddressMeaning(AddressKind.Abs64), false, 72, 184, 65282, 65282, 65282, 65282, 65282, 65282, 65282, 65282, 255, 224), new BytePattern(new AddressMeaning(AddressKind.Rel32, 19), mustMatchAtStart: false, new byte[19]
			{
				240, 255, 0, 0, 0, 0, 0, 0, 0, 0,
				255, 255, 240, 255, 255, 0, 0, 0, 0
			}, new byte[19]
			{
				64, 184, 0, 0, 0, 0, 0, 0, 0, 0,
				102, 255, 0, 15, 133, 2, 2, 2, 2
			}), new BytePattern(new AddressMeaning(AddressKind.Abs64), mustMatchAtStart: false, new byte[27]
			{
				240, 255, 0, 0, 0, 0, 0, 0, 0, 0,
				255, 255, 240, 255, 0, 255, 255, 0, 0, 0,
				0, 0, 0, 0, 0, 255, 255
			}, new byte[27]
			{
				64, 184, 0, 0, 0, 0, 0, 0, 0, 0,
				102, 255, 0, 116, 0, 72, 184, 2, 2, 2,
				2, 2, 2, 2, 2, 255, 224
			}), new BytePattern(new AddressMeaning(AddressKind.PrecodeFixupThunkRel32, 5), true, 232, 65282, 65282, 65282, 65282, 94), new BytePattern(new AddressMeaning(AddressKind.PrecodeFixupThunkRel32, 5), true, 232, 65282, 65282, 65282, 65282, 204), new BytePattern(new AddressMeaning(AddressKind.Indirect, 6), mustMatchAtStart: true, new byte[19]
			{
				255, 255, 0, 0, 0, 0, 255, 255, 255, 255,
				255, 255, 255, 255, 255, 255, 255, 255, 255
			}, new byte[19]
			{
				255, 37, 2, 2, 2, 2, 76, 139, 21, 251,
				15, 0, 0, 255, 37, 253, 15, 0, 0
			}), new BytePattern(new AddressMeaning(AddressKind.PrecodeFixupThunkRel32 | AddressKind.Indirect, 13), mustMatchAtStart: true, new byte[13]
			{
				255, 255, 255, 255, 255, 255, 255, 255, 255, 0,
				0, 0, 0
			}, new byte[13]
			{
				76, 139, 21, 251, 15, 0, 0, 255, 37, 2,
				2, 2, 2
			}), new BytePattern(new AddressMeaning(AddressKind.Indirect, 18), mustMatchAtStart: true, new byte[24]
			{
				255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
				255, 255, 255, 255, 0, 0, 0, 0, 255, 255,
				255, 255, 255, 255
			}, new byte[24]
			{
				72, 139, 5, 249, 15, 0, 0, 102, 255, 8,
				116, 6, 255, 37, 2, 2, 2, 2, 255, 37,
				248, 15, 0, 0
			}), new BytePattern(new AddressMeaning(AddressKind.Indirect, 6), mustMatchAtStart: true, new byte[19]
			{
				255, 255, 0, 0, 0, 0, 255, 255, 255, 255,
				255, 255, 255, 255, 255, 255, 255, 255, 255
			}, new byte[19]
			{
				255, 37, 2, 2, 2, 2, 76, 139, 21, 251,
				63, 0, 0, 255, 37, 253, 63, 0, 0
			}), new BytePattern(new AddressMeaning(AddressKind.PrecodeFixupThunkRel32 | AddressKind.Indirect, 13), mustMatchAtStart: true, new byte[13]
			{
				255, 255, 255, 255, 255, 255, 255, 255, 255, 0,
				0, 0, 0
			}, new byte[13]
			{
				76, 139, 21, 251, 63, 0, 0, 255, 37, 2,
				2, 2, 2
			}), new BytePattern(new AddressMeaning(AddressKind.Indirect, 18), mustMatchAtStart: true, new byte[24]
			{
				255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
				255, 255, 255, 255, 0, 0, 0, 0, 255, 255,
				255, 255, 255, 255
			}, new byte[24]
			{
				72, 139, 5, 249, 63, 0, 0, 102, 255, 8,
				116, 6, 255, 37, 2, 2, 2, 2, 255, 37,
				248, 63, 0, 0
			}), null);
		}
		return new BytePatternCollection();
	}

	public x86_64Arch(ISystem system)
	{
		this.system = system;
		AltEntryFactory = new IcedAltEntryFactory(system, 64);
	}

	public NativeDetourInfo ComputeDetourInfo(nint from, nint to, int sizeHint)
	{
		x86Shared.FixSizeHint(ref sizeHint);
		if (x86Shared.TryRel32Detour(from, to, sizeHint, out var info))
		{
			return info;
		}
		nint num = from + 6;
		nint num2 = num + int.MinValue;
		if ((nuint)num2 > (nuint)num)
		{
			num2 = 0;
		}
		nint num3 = num + int.MaxValue;
		if ((nuint)num3 < (nuint)num)
		{
			num3 = -1;
		}
		PositionedAllocationRequest request = new PositionedAllocationRequest(num, num2, num3, new AllocationRequest(IntPtr.Size));
		if (sizeHint >= Rel32Ind64Kind.Instance.Size && system.MemoryAllocator.TryAllocateInRange(request, out IAllocatedMemory allocated))
		{
			return new NativeDetourInfo(from, to, Rel32Ind64Kind.Instance, allocated);
		}
		if (sizeHint < Abs64Kind.Instance.Size)
		{
			bool isEnabled;
			_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogWarningStringHandler message = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogWarningStringHandler(79, 1, out isEnabled);
			if (isEnabled)
			{
				message.AppendLiteral("Size too small for all known detour kinds; defaulting to Abs64. provided size: ");
				message.AppendFormatted(sizeHint);
			}
			_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Warning(ref message);
		}
		return new NativeDetourInfo(from, to, Abs64Kind.Instance, null);
	}

	public int GetDetourBytes(NativeDetourInfo info, Span<byte> buffer, out IDisposable? allocHandle)
	{
		return DetourKindBase.GetDetourBytes(info, buffer, out allocHandle);
	}

	public NativeDetourInfo ComputeRetargetInfo(NativeDetourInfo detour, IntPtr to, int maxSizeHint = -1)
	{
		x86Shared.FixSizeHint(ref maxSizeHint);
		if (DetourKindBase.TryFindRetargetInfo(detour, to, maxSizeHint, out var retargetInfo))
		{
			return retargetInfo;
		}
		return ComputeDetourInfo(detour.From, to, maxSizeHint);
	}

	public int GetRetargetBytes(NativeDetourInfo original, NativeDetourInfo retarget, Span<byte> buffer, out IDisposable? allocationHandle, out bool needsRepatch, out bool disposeOldAlloc)
	{
		return DetourKindBase.DoRetarget(original, retarget, buffer, out allocationHandle, out needsRepatch, out disposeOldAlloc);
	}

	public ReadOnlyMemory<IAllocatedMemory> CreateNativeVtableProxyStubs(IntPtr vtableBase, int vtableSize)
	{
		ReadOnlySpan<byte> stubData;
		int indexOffs;
		bool premulOffset;
		if (PlatformDetection.OS.Is(OSKind.Windows))
		{
			stubData = VtblProxyStubWin;
			indexOffs = 9;
			premulOffset = true;
		}
		else
		{
			stubData = VtblProxyStubSysV;
			indexOffs = 9;
			premulOffset = true;
		}
		return Shared.CreateVtableStubs(system, vtableBase, vtableSize, stubData, indexOffs, premulOffset);
	}

	public IAllocatedMemory CreateSpecialEntryStub(IntPtr target, IntPtr argument)
	{
		Span<byte> span = stackalloc byte[SpecEntryStub.Length];
		SpecEntryStub.CopyTo(span);
		Unsafe.WriteUnaligned(ref span[12], target);
		Unsafe.WriteUnaligned(ref span[2], argument);
		Helpers.Assert(system.MemoryAllocator.TryAllocate(new AllocationRequest(span.Length)
		{
			Executable = true,
			Alignment = 1
		}, out IAllocatedMemory allocated), null, "system.MemoryAllocator.TryAllocate(new(stub.Length) { Executable = true, Alignment = 1 }, out var alloc)");
		system.PatchData(PatchTargetKind.Executable, allocated.BaseAddress, span, default(Span<byte>));
		return allocated;
	}
}
