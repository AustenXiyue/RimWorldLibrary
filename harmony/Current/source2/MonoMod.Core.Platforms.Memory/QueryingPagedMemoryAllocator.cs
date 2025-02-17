using System.Diagnostics.CodeAnalysis;
using MonoMod.Utils;

namespace MonoMod.Core.Platforms.Memory;

internal sealed class QueryingPagedMemoryAllocator : PagedMemoryAllocator
{
	private readonly QueryingMemoryPageAllocatorBase pageAlloc;

	public QueryingPagedMemoryAllocator(QueryingMemoryPageAllocatorBase alloc)
		: base((nint)Helpers.ThrowIfNull(alloc, "alloc").PageSize)
	{
		pageAlloc = alloc;
	}

	protected override bool TryAllocateNewPage(AllocationRequest request, [_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMaybeNullWhen(false)] out IAllocatedMemory allocated)
	{
		if (!pageAlloc.TryAllocatePage(base.PageSize, request.Executable, out var allocated2))
		{
			allocated = null;
			return false;
		}
		Page page = new Page(this, allocated2, (uint)base.PageSize, request.Executable);
		InsertAllocatedPage(page);
		if (!page.TryAllocate((uint)request.Size, (uint)request.Alignment, out PageAllocation alloc))
		{
			RegisterForCleanup(page);
			allocated = null;
			return false;
		}
		allocated = alloc;
		return true;
	}

	protected override bool TryAllocateNewPage(PositionedAllocationRequest request, nint targetPage, nint lowPageBound, nint highPageBound, [_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMaybeNullWhen(false)] out IAllocatedMemory allocated)
	{
		nint target = request.Target;
		nint page = targetPage;
		nint page2 = targetPage + base.PageSize;
		while (page >= lowPageBound || page2 < highPageBound)
		{
			while (page2 < highPageBound && (page < lowPageBound || target - page > page2 - target))
			{
				if (TryAllocNewPage(request, ref page2, goingUp: true, out allocated))
				{
					return true;
				}
			}
			while (page >= lowPageBound && (page2 >= highPageBound || target - page < page2 - target))
			{
				if (TryAllocNewPage(request, ref page, goingUp: false, out allocated))
				{
					return true;
				}
			}
		}
		allocated = null;
		return false;
	}

	private bool TryAllocNewPage(PositionedAllocationRequest request, ref nint page, bool goingUp, [_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMaybeNullWhen(false)] out IAllocatedMemory allocated)
	{
		if (pageAlloc.TryQueryPage(page, out bool isFree, out nint allocBase, out nint allocSize))
		{
			if (isFree && pageAlloc.TryAllocatePage(page, base.PageSize, request.Base.Executable, out var allocated2))
			{
				Page page2 = new Page(this, allocated2, (uint)base.PageSize, request.Base.Executable);
				InsertAllocatedPage(page2);
				if (page2.TryAllocate((uint)request.Base.Size, (uint)request.Base.Alignment, out PageAllocation alloc))
				{
					allocated = alloc;
					return true;
				}
				RegisterForCleanup(page2);
			}
			if (goingUp)
			{
				page = allocBase + allocSize;
			}
			else
			{
				page = allocBase - base.PageSize;
			}
			allocated = null;
			return false;
		}
		if (goingUp)
		{
			page += base.PageSize;
		}
		else
		{
			page -= base.PageSize;
		}
		allocated = null;
		return false;
	}

	protected override bool TryFreePage(Page page, [_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003ENotNullWhen(false)] out string? errorMsg)
	{
		return pageAlloc.TryFreePage(page.BaseAddr, out errorMsg);
	}
}
