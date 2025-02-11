using System.Diagnostics.CodeAnalysis;

namespace MonoMod.Core.Platforms;

internal interface IMemoryAllocator
{
	int MaxSize { get; }

	bool TryAllocate(AllocationRequest request, [_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMaybeNullWhen(false)] out IAllocatedMemory allocated);

	bool TryAllocateInRange(PositionedAllocationRequest request, [_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMaybeNullWhen(false)] out IAllocatedMemory allocated);
}
