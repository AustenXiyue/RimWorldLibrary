using System;
using System.Diagnostics.CodeAnalysis;

namespace MonoMod.Core.Platforms.Memory;

internal abstract class QueryingMemoryPageAllocatorBase
{
	public abstract uint PageSize { get; }

	public abstract bool TryQueryPage(IntPtr pageAddr, out bool isFree, out IntPtr allocBase, out nint allocSize);

	public abstract bool TryAllocatePage(nint size, bool executable, out IntPtr allocated);

	public abstract bool TryAllocatePage(IntPtr pageAddr, nint size, bool executable, out IntPtr allocated);

	public abstract bool TryFreePage(IntPtr pageAddr, [_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003ENotNullWhen(false)] out string? errorMsg);
}
