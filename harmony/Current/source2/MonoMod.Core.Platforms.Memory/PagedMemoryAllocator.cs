using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;

namespace MonoMod.Core.Platforms.Memory;

internal abstract class PagedMemoryAllocator : IMemoryAllocator
{
	private sealed class FreeMem
	{
		public uint BaseOffset;

		public uint Size;

		public FreeMem? NextFree;
	}

	protected sealed class PageAllocation : IAllocatedMemory, IDisposable
	{
		private readonly Page owner;

		private readonly uint offset;

		private bool disposedValue;

		public bool IsExecutable => owner.IsExecutable;

		public IntPtr BaseAddress => (nint)owner.BaseAddr + (nint)offset;

		public int Size { get; }

		public unsafe Span<byte> Memory => new Span<byte>((void*)BaseAddress, Size);

		public PageAllocation(Page page, uint offset, int size)
		{
			owner = page;
			this.offset = offset;
			Size = size;
		}

		private void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				owner.FreeMem(offset, (uint)Size);
				disposedValue = true;
			}
		}

		~PageAllocation()
		{
			Dispose(disposing: false);
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}

	protected sealed class Page
	{
		private readonly PagedMemoryAllocator owner;

		private readonly object sync = new object();

		private FreeMem? freeList;

		public bool IsEmpty
		{
			get
			{
				FreeMem freeMem = freeList;
				if (freeMem != null && freeMem.BaseOffset == 0)
				{
					return freeMem.Size == Size;
				}
				return false;
			}
		}

		public IntPtr BaseAddr { get; }

		public uint Size { get; }

		public bool IsExecutable { get; }

		public Page(PagedMemoryAllocator owner, IntPtr baseAddr, uint size, bool isExecutable)
		{
			this.owner = owner;
			BaseAddr = baseAddr;
			Size = size;
			IsExecutable = isExecutable;
			freeList = new FreeMem
			{
				BaseOffset = 0u,
				Size = size,
				NextFree = null
			};
		}

		public bool TryAllocate(uint size, uint align, [_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMaybeNullWhen(false)] out PageAllocation alloc)
		{
			lock (sync)
			{
				ref FreeMem nextFree = ref freeList;
				uint num = 0u;
				while (nextFree != null)
				{
					uint num2 = nextFree.BaseOffset % align;
					num2 = ((num2 != 0) ? (align - num2) : num2);
					if (nextFree.Size >= num2 + size)
					{
						num = num2;
						break;
					}
					nextFree = ref nextFree.NextFree;
				}
				if (nextFree == null)
				{
					alloc = null;
					return false;
				}
				uint offset = nextFree.BaseOffset + num;
				if (num == 0)
				{
					nextFree.BaseOffset += size;
					nextFree.Size -= size;
				}
				else
				{
					FreeMem freeMem = new FreeMem
					{
						BaseOffset = nextFree.BaseOffset,
						Size = num,
						NextFree = nextFree
					};
					nextFree.BaseOffset += num + size;
					nextFree.Size -= num + size;
					nextFree = freeMem;
				}
				NormalizeFreeList();
				alloc = new PageAllocation(this, offset, (int)size);
				return true;
			}
		}

		private void NormalizeFreeList()
		{
			ref FreeMem nextFree = ref freeList;
			while (nextFree != null)
			{
				if (nextFree.Size == 0)
				{
					nextFree = nextFree.NextFree;
					continue;
				}
				FreeMem nextFree2 = nextFree.NextFree;
				if (nextFree2 != null && nextFree2.BaseOffset == nextFree.BaseOffset + nextFree.Size)
				{
					nextFree.Size += nextFree2.Size;
					nextFree.NextFree = nextFree2.NextFree;
				}
				else
				{
					nextFree = ref nextFree.NextFree;
				}
			}
		}

		internal void FreeMem(uint offset, uint size)
		{
			lock (sync)
			{
				ref FreeMem nextFree = ref freeList;
				while (nextFree != null && nextFree.BaseOffset <= offset)
				{
					nextFree = ref nextFree.NextFree;
				}
				nextFree = new FreeMem
				{
					BaseOffset = offset,
					Size = size,
					NextFree = nextFree
				};
				NormalizeFreeList();
				if (IsEmpty)
				{
					owner.RegisterForCleanup(this);
				}
			}
		}
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private readonly struct PageComparer : IComparer<Page?>
	{
		public int Compare(Page? x, Page? y)
		{
			if (x == y)
			{
				return 0;
			}
			if (x == null)
			{
				return 1;
			}
			if (y == null)
			{
				return -1;
			}
			return ((long)x.BaseAddr).CompareTo((long)y.BaseAddr);
		}
	}

	private readonly struct PageAddrComparable : IComparable<Page>
	{
		private readonly IntPtr addr;

		public PageAddrComparable(IntPtr addr)
		{
			this.addr = addr;
		}

		public int CompareTo(Page? other)
		{
			if (other == null)
			{
				return 1;
			}
			return ((long)addr).CompareTo((long)other.BaseAddr);
		}
	}

	private readonly nint pageBaseMask;

	private readonly nint pageSize;

	private readonly bool pageSizeIsPow2;

	private Page?[] allocationList = new Page[16];

	private int pageCount;

	private readonly ConcurrentBag<Page> pagesToClean = new ConcurrentBag<Page>();

	private int registeredForCleanup;

	private readonly object sync = new object();

	protected nint PageSize => pageSize;

	private ReadOnlySpan<Page> AllocList => allocationList.AsSpan().Slice(0, pageCount);

	public int MaxSize => (int)pageSize;

	protected PagedMemoryAllocator(nint pageSize)
	{
		this.pageSize = pageSize;
		pageSizeIsPow2 = BitOperationsEx.IsPow2(pageSize);
		pageBaseMask = ~(nint)0 << BitOperationsEx.TrailingZeroCount(pageSize);
	}

	public nint RoundDownToPageBoundary(nint addr)
	{
		if (pageSizeIsPow2)
		{
			return addr & pageBaseMask;
		}
		return addr - addr % pageSize;
	}

	protected void InsertAllocatedPage(Page page)
	{
		if (pageCount == allocationList.Length)
		{
			int newSize = (int)BitOperationsEx.RoundUpToPowerOf2((uint)(allocationList.Length + 1));
			Array.Resize(ref allocationList, newSize);
		}
		Span<Page> span = allocationList.AsSpan();
		int num = span.Slice(0, pageCount).BinarySearch(page, default(PageComparer));
		if (num < 0)
		{
			num = ~num;
			if (num + 1 < span.Length)
			{
				span.Slice(num, pageCount - num).CopyTo(span.Slice(num + 1));
			}
			span[num] = page;
			pageCount++;
		}
	}

	private void RemoveAllocatedPage(Page page)
	{
		Span<Page> span = allocationList.AsSpan();
		int num = span.Slice(0, pageCount).BinarySearch(page, default(PageComparer));
		if (num >= 0)
		{
			span.Slice(num + 1).CopyTo(span.Slice(num));
			pageCount--;
		}
	}

	private int GetBoundIndex(IntPtr ptr)
	{
		int num = MemoryExtensions.BinarySearch(AllocList, new PageAddrComparable(ptr));
		if (num < 0)
		{
			return ~num;
		}
		return num;
	}

	protected void RegisterForCleanup(Page page)
	{
		if (!Environment.HasShutdownStarted && !AppDomain.CurrentDomain.IsFinalizingForUnload())
		{
			pagesToClean.Add(page);
			if (Interlocked.CompareExchange(ref registeredForCleanup, 1, 0) == 0)
			{
				System.Gen2GcCallback.Register(DoCleanup);
			}
		}
	}

	private bool DoCleanup()
	{
		if (Environment.HasShutdownStarted || AppDomain.CurrentDomain.IsFinalizingForUnload())
		{
			return false;
		}
		Volatile.Write(ref registeredForCleanup, 0);
		Page result;
		while (pagesToClean.TryTake(out result))
		{
			lock (sync)
			{
				if (!result.IsEmpty)
				{
					continue;
				}
				RemoveAllocatedPage(result);
				goto IL_005e;
			}
			IL_005e:
			if (!TryFreePage(result, out string errorMsg))
			{
				bool isEnabled;
				_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogErrorStringHandler message = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogErrorStringHandler(27, 1, out isEnabled);
				if (isEnabled)
				{
					message.AppendLiteral("Could not deallocate page! ");
					message.AppendFormatted(errorMsg);
				}
				_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Error(ref message);
			}
		}
		return false;
	}

	protected abstract bool TryFreePage(Page page, [_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003ENotNullWhen(false)] out string? errorMsg);

	public bool TryAllocateInRange(PositionedAllocationRequest request, [_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMaybeNullWhen(false)] out IAllocatedMemory allocated)
	{
		if ((nint)request.Target < (nint)request.LowBound || (nint)request.Target > (nint)request.HighBound)
		{
			throw new ArgumentException("Target not between low and high", "request");
		}
		if (request.Base.Size < 0)
		{
			throw new ArgumentException("Size is negative", "request");
		}
		if (request.Base.Alignment <= 0)
		{
			throw new ArgumentException("Alignment is zero or negative", "request");
		}
		if (request.Base.Size > pageSize)
		{
			throw new NotSupportedException("Single allocations cannot be larger than a page");
		}
		nint num = RoundDownToPageBoundary((nint)request.LowBound + pageSize - 1);
		nint num2 = RoundDownToPageBoundary(request.HighBound);
		nint num3 = RoundDownToPageBoundary(request.Target);
		nint target = request.Target;
		lock (sync)
		{
			int boundIndex = GetBoundIndex(num);
			int boundIndex2 = GetBoundIndex(num2);
			if (boundIndex != boundIndex2)
			{
				int boundIndex3 = GetBoundIndex(num3);
				int i = boundIndex3 - 1;
				int j = boundIndex3;
				while ((uint)j <= AllocList.Length && (uint)i < AllocList.Length && (i >= boundIndex || j < boundIndex2))
				{
					for (; (uint)j < AllocList.Length && j < boundIndex2 && (i < boundIndex || target - (nint)AllocList[i].BaseAddr > (nint)AllocList[j].BaseAddr - target); j++)
					{
						if (TryAllocWithPage(AllocList[j], request, out allocated))
						{
							return true;
						}
					}
					for (; (uint)i < AllocList.Length && i >= boundIndex && (j >= boundIndex2 || target - (nint)AllocList[i].BaseAddr < (nint)AllocList[j].BaseAddr - target); i++)
					{
						if (TryAllocWithPage(AllocList[i], request, out allocated))
						{
							return true;
						}
					}
				}
			}
			return TryAllocateNewPage(request, num3, num, num2, out allocated);
		}
	}

	private static bool TryAllocWithPage(Page page, PositionedAllocationRequest request, [_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMaybeNullWhen(false)] out IAllocatedMemory allocated)
	{
		if (page.IsExecutable == request.Base.Executable && (nint)page.BaseAddr >= (nint)request.LowBound && (nint)page.BaseAddr < (nint)request.HighBound && page.TryAllocate((uint)request.Base.Size, (uint)request.Base.Alignment, out PageAllocation alloc))
		{
			if ((nint)alloc.BaseAddress >= (nint)request.LowBound && (nint)alloc.BaseAddress < (nint)request.HighBound)
			{
				allocated = alloc;
				return true;
			}
			alloc.Dispose();
		}
		allocated = null;
		return false;
	}

	public bool TryAllocate(AllocationRequest request, [_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMaybeNullWhen(false)] out IAllocatedMemory allocated)
	{
		if (request.Size < 0)
		{
			throw new ArgumentException("Size is negative", "request");
		}
		if (request.Alignment <= 0)
		{
			throw new ArgumentException("Alignment is zero or negative", "request");
		}
		if (request.Size > pageSize)
		{
			throw new NotSupportedException("Single allocations cannot be larger than a page");
		}
		lock (sync)
		{
			ReadOnlySpan<Page> allocList = AllocList;
			for (int i = 0; i < allocList.Length; i++)
			{
				Page page = allocList[i];
				if (page.IsExecutable == request.Executable && page.TryAllocate((uint)request.Size, (uint)request.Alignment, out PageAllocation alloc))
				{
					allocated = alloc;
					return true;
				}
			}
			return TryAllocateNewPage(request, out allocated);
		}
	}

	protected abstract bool TryAllocateNewPage(AllocationRequest request, [_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMaybeNullWhen(false)] out IAllocatedMemory allocated);

	protected abstract bool TryAllocateNewPage(PositionedAllocationRequest request, nint targetPage, nint lowPageBound, nint highPageBound, [_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMaybeNullWhen(false)] out IAllocatedMemory allocated);
}
