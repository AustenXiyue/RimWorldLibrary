using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.Win32.SafeHandles;
using MonoMod.Core.Interop;
using MonoMod.Core.Platforms.Memory;
using MonoMod.Core.Utils;
using MonoMod.Utils;

namespace MonoMod.Core.Platforms.Systems;

internal sealed class MacOSSystem : ISystem, IInitialize<IArchitecture>
{
	private sealed class MacOsQueryingAllocator : QueryingMemoryPageAllocatorBase
	{
		public override uint PageSize { get; }

		public MacOsQueryingAllocator()
		{
			PageSize = (uint)OSX.GetPageSize();
		}

		public unsafe override bool TryAllocatePage(nint size, bool executable, out IntPtr allocated)
		{
			Helpers.Assert(size == PageSize, null, "size == PageSize");
			OSX.vm_prot_t vm_prot_t = (executable ? OSX.vm_prot_t.Execute : OSX.vm_prot_t.None);
			vm_prot_t |= OSX.vm_prot_t.Default;
			ulong num = 0uL;
			OSX.kern_return_t kern_return_t = OSX.mach_vm_map(OSX.mach_task_self(), &num, (ulong)size, 0uL, OSX.vm_flags.Anywhere, 0, 0uL, true, vm_prot_t, vm_prot_t, OSX.vm_inherit_t.Copy);
			if (!kern_return_t)
			{
				bool isEnabled;
				_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogErrorStringHandler message = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogErrorStringHandler(41, 1, out isEnabled);
				if (isEnabled)
				{
					message.AppendLiteral("Error creating allocation anywhere! kr = ");
					message.AppendFormatted(kern_return_t.Value);
				}
				_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Error(ref message);
				allocated = default(IntPtr);
				return false;
			}
			allocated = (IntPtr)(long)num;
			return true;
		}

		public unsafe override bool TryAllocatePage(IntPtr pageAddr, nint size, bool executable, out IntPtr allocated)
		{
			Helpers.Assert(size == PageSize, null, "size == PageSize");
			OSX.vm_prot_t vm_prot_t = (executable ? OSX.vm_prot_t.Execute : OSX.vm_prot_t.None);
			vm_prot_t |= OSX.vm_prot_t.Default;
			ulong num = (ulong)(long)pageAddr;
			OSX.kern_return_t kern_return_t = OSX.mach_vm_map(OSX.mach_task_self(), &num, (ulong)size, 0uL, OSX.vm_flags.Fixed, 0, 0uL, true, vm_prot_t, vm_prot_t, OSX.vm_inherit_t.Copy);
			if (!kern_return_t)
			{
				bool isEnabled;
				_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogSpamStringHandler message = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogSpamStringHandler(38, 2, out isEnabled);
				if (isEnabled)
				{
					message.AppendLiteral("Error creating allocation at 0x");
					message.AppendFormatted(num, "x16");
					message.AppendLiteral(": kr = ");
					message.AppendFormatted(kern_return_t.Value);
				}
				_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Spam(ref message);
				allocated = default(IntPtr);
				return false;
			}
			allocated = (IntPtr)(long)num;
			return true;
		}

		public override bool TryFreePage(IntPtr pageAddr, [_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003ENotNullWhen(false)] out string? errorMsg)
		{
			OSX.kern_return_t kern_return_t = OSX.mach_vm_deallocate(OSX.mach_task_self(), (ulong)(long)pageAddr, PageSize);
			if (!kern_return_t)
			{
				errorMsg = $"Could not deallocate page: kr = {kern_return_t.Value}";
				return false;
			}
			errorMsg = null;
			return true;
		}

		public override bool TryQueryPage(IntPtr pageAddr, out bool isFree, out IntPtr allocBase, out nint allocSize)
		{
			OSX.vm_prot_t prot;
			OSX.vm_prot_t maxProt;
			OSX.kern_return_t localRegionInfo = GetLocalRegionInfo(pageAddr, out allocBase, out allocSize, out prot, out maxProt);
			if ((bool)localRegionInfo)
			{
				if ((nint)allocBase > (nint)pageAddr)
				{
					allocSize = (nint)allocBase - (nint)pageAddr;
					allocBase = pageAddr;
					isFree = true;
					return true;
				}
				isFree = false;
				return true;
			}
			if (localRegionInfo == OSX.kern_return_t.InvalidAddress)
			{
				isFree = true;
				return true;
			}
			isFree = false;
			return false;
		}
	}

	private IArchitecture? arch;

	private PosixExceptionHelper? lazyNativeExceptionHelper;

	public OSKind Target => OSKind.OSX;

	public SystemFeature Features => SystemFeature.RWXPages | SystemFeature.RXPages;

	public Abi? DefaultAbi { get; }

	public IMemoryAllocator MemoryAllocator { get; } = new QueryingPagedMemoryAllocator(new MacOsQueryingAllocator());

	public INativeExceptionHelper? NativeExceptionHelper => lazyNativeExceptionHelper ?? (lazyNativeExceptionHelper = CreateNativeExceptionHelper());

	private static ReadOnlySpan<byte> NEHTempl => "/tmp/mm-exhelper.dylib.XXXXXX"u8;

	public MacOSSystem()
	{
		if (PlatformDetection.Architecture == ArchitectureKind.x86_64)
		{
			DefaultAbi = new Abi(new SpecialArgumentKind[3]
			{
				SpecialArgumentKind.ReturnBuffer,
				SpecialArgumentKind.ThisPointer,
				SpecialArgumentKind.UserArguments
			}, SystemVABI.ClassifyAMD64, ReturnsReturnBuffer: true);
			return;
		}
		throw new NotImplementedException();
	}

	public unsafe IEnumerable<string?> EnumerateLoadedModuleFiles()
	{
		int count = OSX.task_dyld_info.Count;
		OSX.task_dyld_info task_dyld_info = default(OSX.task_dyld_info);
		if (!OSX.task_info(OSX.mach_task_self(), OSX.task_flavor_t.DyldInfo, &task_dyld_info, &count))
		{
			return ArrayEx.Empty<string>();
		}
		ReadOnlySpan<OSX.dyld_image_info> infoArray = task_dyld_info.all_image_infos->InfoArray;
		string[] array = new string[infoArray.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = infoArray[i].imageFilePath.ToString();
		}
		return array;
	}

	public nint GetSizeOfReadableMemory(IntPtr start, nint guess)
	{
		nint num = 0;
		do
		{
			if (!GetLocalRegionInfo(start, out var startAddr, out var outSize, out var prot, out var _))
			{
				return num;
			}
			if (startAddr > (nint)start)
			{
				return num;
			}
			if ((prot & OSX.vm_prot_t.Read) == 0)
			{
				return num;
			}
			num += startAddr + outSize - (nint)start;
			start = startAddr + outSize;
		}
		while (num < guess);
		return num;
	}

	public unsafe void PatchData(PatchTargetKind targetKind, IntPtr patchTarget, ReadOnlySpan<byte> data, Span<byte> backup)
	{
		int length = data.Length;
		bool isEnabled;
		bool flag;
		bool flag2;
		if (TryGetProtForMem(patchTarget, length, out var _, out var prot, out var crossesAllocBoundary, out var notAllocated))
		{
			if (crossesAllocBoundary)
			{
				_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogWarningStringHandler message = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogWarningStringHandler(101, 2, out isEnabled);
				if (isEnabled)
				{
					message.AppendLiteral("Patch requested for memory which spans multiple memory allocations. Failures may result. (0x");
					message.AppendFormatted(patchTarget, "x16");
					message.AppendLiteral(" length ");
					message.AppendFormatted(length);
					message.AppendLiteral(")");
				}
				_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Warning(ref message);
			}
			flag = prot.Has(OSX.vm_prot_t.Write);
			flag2 = prot.Has(OSX.vm_prot_t.Execute);
		}
		else
		{
			if (notAllocated)
			{
				_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogErrorStringHandler message2 = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogErrorStringHandler(68, 2, out isEnabled);
				if (isEnabled)
				{
					message2.AppendLiteral("Requested patch of region which was not fully allocated (0x");
					message2.AppendFormatted(patchTarget, "x16");
					message2.AppendLiteral(" length ");
					message2.AppendFormatted(length);
					message2.AppendLiteral(")");
				}
				_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Error(ref message2);
				throw new InvalidOperationException("Cannot patch unallocated region");
			}
			flag = false;
			flag2 = targetKind == PatchTargetKind.Executable;
		}
		if (!flag)
		{
			Helpers.Assert(!crossesAllocBoundary, null, "!crossesBoundary");
			MakePageWritable(patchTarget);
		}
		Span<byte> destination = new Span<byte>((void*)patchTarget, data.Length);
		destination.TryCopyTo(backup);
		data.CopyTo(destination);
		if (flag2)
		{
			OSX.sys_icache_invalidate((void*)patchTarget, (nuint)data.Length);
		}
	}

	private unsafe static void MakePageWritable(nint addrInPage)
	{
		Helpers.Assert(GetLocalRegionInfo(addrInPage, out IntPtr startAddr, out IntPtr outSize, out OSX.vm_prot_t prot, out OSX.vm_prot_t maxProt), null, "GetLocalRegionInfo(addrInPage, out var allocStart, out var allocSize, out var allocProt, out var allocMaxProt)");
		Helpers.Assert((nint)startAddr <= addrInPage, null, "allocStart <= addrInPage");
		if (prot.Has(OSX.vm_prot_t.Write))
		{
			return;
		}
		int targetTask = OSX.mach_task_self();
		_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogErrorStringHandler message;
		bool isEnabled;
		OSX.kern_return_t kern_return_t;
		if (maxProt.Has(OSX.vm_prot_t.Write))
		{
			kern_return_t = OSX.mach_vm_protect(targetTask, (ulong)(nint)startAddr, (ulong)(nint)outSize, false, prot | OSX.vm_prot_t.Write);
			if ((bool)kern_return_t)
			{
				return;
			}
			message = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogErrorStringHandler(60, 6, out isEnabled);
			if (isEnabled)
			{
				message.AppendLiteral("Could not vm_protect page 0x");
				message.AppendFormatted(startAddr, "x16");
				message.AppendLiteral("+0x");
				message.AppendFormatted(outSize, "x");
				message.AppendLiteral(" ");
				message.AppendLiteral("from ");
				message.AppendFormatted(OSX.P(prot));
				message.AppendLiteral(" to ");
				message.AppendFormatted(OSX.P(prot | OSX.vm_prot_t.Write));
				message.AppendLiteral(" (max prot ");
				message.AppendFormatted(OSX.P(maxProt));
				message.AppendLiteral("): kr = ");
				message.AppendFormatted(kern_return_t.Value);
			}
			_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Error(ref message);
			_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Error("Trying copy/remap instead...");
		}
		if (!prot.Has(OSX.vm_prot_t.Read))
		{
			if (!maxProt.Has(OSX.vm_prot_t.Read))
			{
				message = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogErrorStringHandler(66, 3, out isEnabled);
				if (isEnabled)
				{
					message.AppendLiteral("Requested 0x");
					message.AppendFormatted(startAddr, "x16");
					message.AppendLiteral("+0x");
					message.AppendFormatted(outSize, "x");
					message.AppendLiteral(" (max: ");
					message.AppendFormatted(OSX.P(maxProt));
					message.AppendLiteral(") to be made writable, but its not readable!");
				}
				_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Error(ref message);
				throw new NotSupportedException("Cannot make page writable because its not readable");
			}
			kern_return_t = OSX.mach_vm_protect(targetTask, (ulong)(nint)startAddr, (ulong)(nint)outSize, false, prot | OSX.vm_prot_t.Read);
			if (!kern_return_t)
			{
				message = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogErrorStringHandler(60, 4, out isEnabled);
				if (isEnabled)
				{
					message.AppendLiteral("vm_protect of 0x");
					message.AppendFormatted(startAddr, "x16");
					message.AppendLiteral("+0x");
					message.AppendFormatted(outSize, "x");
					message.AppendLiteral(" (max: ");
					message.AppendFormatted(OSX.P(maxProt));
					message.AppendLiteral(") to become readable failed: kr = ");
					message.AppendFormatted(kern_return_t.Value);
				}
				_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Error(ref message);
				throw new NotSupportedException("Could not make page readable for remap");
			}
		}
		_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogTraceStringHandler message2 = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogTraceStringHandler(41, 5, out isEnabled);
		if (isEnabled)
		{
			message2.AppendLiteral("Performing page remap on 0x");
			message2.AppendFormatted(startAddr, "x16");
			message2.AppendLiteral("+0x");
			message2.AppendFormatted(outSize, "x");
			message2.AppendLiteral(" from ");
			message2.AppendFormatted(OSX.P(prot));
			message2.AppendLiteral("/");
			message2.AppendFormatted(OSX.P(maxProt));
			message2.AppendLiteral(" to ");
			message2.AppendFormatted(OSX.P(prot | OSX.vm_prot_t.Write));
		}
		_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Trace(ref message2);
		OSX.vm_prot_t vm_prot_t = prot | OSX.vm_prot_t.Write;
		OSX.vm_prot_t vm_prot_t2 = maxProt | OSX.vm_prot_t.Write;
		ulong num = default(ulong);
		kern_return_t = OSX.mach_vm_map(targetTask, &num, (ulong)(nint)outSize, 0uL, OSX.vm_flags.Anywhere, 0, 0uL, true, vm_prot_t, vm_prot_t2, OSX.vm_inherit_t.Copy);
		if (!kern_return_t)
		{
			message = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogErrorStringHandler(36, 1, out isEnabled);
			if (isEnabled)
			{
				message.AppendLiteral("Could not allocate new memory! kr = ");
				message.AppendFormatted(kern_return_t.Value);
			}
			_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Error(ref message);
			throw new OutOfMemoryException();
		}
		try
		{
			new Span<byte>((void*)startAddr, (int)(nint)outSize).CopyTo(new Span<byte>((void*)num, (int)(nint)outSize));
			ulong value = (ulong)(nint)outSize;
			int num2 = default(int);
			kern_return_t = OSX.mach_make_memory_entry_64(targetTask, &value, num, vm_prot_t2, &num2, 0);
			if (!kern_return_t)
			{
				message = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogErrorStringHandler(79, 4, out isEnabled);
				if (isEnabled)
				{
					message.AppendLiteral("make_memory_entry(task_self(), size: 0x");
					message.AppendFormatted(value, "x");
					message.AppendLiteral(", addr: ");
					message.AppendFormatted(num, "x16");
					message.AppendLiteral(", prot: ");
					message.AppendFormatted(OSX.P(vm_prot_t2));
					message.AppendLiteral(", &obj, 0) failed: kr = ");
					message.AppendFormatted(kern_return_t.Value);
				}
				_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Error(ref message);
				throw new NotSupportedException("make_memory_entry() failed");
			}
			ulong value2 = (ulong)(nint)startAddr;
			kern_return_t = OSX.mach_vm_map(targetTask, &value2, (ulong)(nint)outSize, 0uL, OSX.vm_flags.Overwrite, num2, 0uL, true, vm_prot_t, vm_prot_t2, OSX.vm_inherit_t.Copy);
			if (!kern_return_t)
			{
				message = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogErrorStringHandler(78, 10, out isEnabled);
				if (isEnabled)
				{
					message.AppendLiteral("vm_map() failed to map over target range: 0x");
					message.AppendFormatted(value2, "x16");
					message.AppendLiteral("+0x");
					message.AppendFormatted(outSize, "x");
					message.AppendLiteral(" (");
					message.AppendFormatted(OSX.P(prot));
					message.AppendLiteral("/");
					message.AppendFormatted(OSX.P(maxProt));
					message.AppendLiteral(")");
					message.AppendLiteral(" <- (obj ");
					message.AppendFormatted(num2);
					message.AppendLiteral(") 0x");
					message.AppendFormatted(num, "x16");
					message.AppendLiteral("+0x");
					message.AppendFormatted(outSize, "x");
					message.AppendLiteral(" (");
					message.AppendFormatted(OSX.P(vm_prot_t));
					message.AppendLiteral("/");
					message.AppendFormatted(OSX.P(vm_prot_t2));
					message.AppendLiteral("), kr = ");
					message.AppendFormatted(kern_return_t.Value);
				}
				_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Error(ref message);
				throw new NotSupportedException("vm_map() failed");
			}
		}
		finally
		{
			kern_return_t = OSX.mach_vm_deallocate(targetTask, num, (ulong)(nint)outSize);
			if (!kern_return_t)
			{
				message = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogErrorStringHandler(53, 3, out isEnabled);
				if (isEnabled)
				{
					message.AppendLiteral("Could not deallocate created memory page 0x");
					message.AppendFormatted(num, "x16");
					message.AppendLiteral("+0x");
					message.AppendFormatted(outSize, "x");
					message.AppendLiteral("! kr = ");
					message.AppendFormatted(kern_return_t.Value);
				}
				_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Error(ref message);
			}
		}
	}

	private static bool TryGetProtForMem(nint addr, int length, out OSX.vm_prot_t maxProt, out OSX.vm_prot_t prot, out bool crossesAllocBoundary, out bool notAllocated)
	{
		maxProt = (OSX.vm_prot_t)(-1);
		prot = (OSX.vm_prot_t)(-1);
		crossesAllocBoundary = false;
		notAllocated = false;
		nint num = addr;
		while (addr < num + length)
		{
			nint startAddr;
			nint outSize;
			OSX.vm_prot_t prot2;
			OSX.vm_prot_t maxProt2;
			OSX.kern_return_t localRegionInfo = GetLocalRegionInfo(addr, out startAddr, out outSize, out prot2, out maxProt2);
			if ((bool)localRegionInfo)
			{
				if (startAddr > addr)
				{
					notAllocated = true;
					return false;
				}
				prot &= prot2;
				maxProt &= maxProt2;
				addr = startAddr + outSize;
				if (addr >= num + length)
				{
					break;
				}
				crossesAllocBoundary = true;
				continue;
			}
			if (localRegionInfo == OSX.kern_return_t.NoSpace)
			{
				notAllocated = true;
				return false;
			}
			return false;
		}
		return true;
	}

	private unsafe static OSX.kern_return_t GetLocalRegionInfo(nint origAddr, out nint startAddr, out nint outSize, out OSX.vm_prot_t prot, out OSX.vm_prot_t maxProt)
	{
		int num = int.MaxValue;
		int count = OSX.vm_region_submap_short_info_64.Count;
		ulong num2 = (ulong)origAddr;
		ulong num3 = default(ulong);
		OSX.vm_region_submap_short_info_64 vm_region_submap_short_info_ = default(OSX.vm_region_submap_short_info_64);
		OSX.kern_return_t kern_return_t = OSX.mach_vm_region_recurse(OSX.mach_task_self(), &num2, &num3, &num, &vm_region_submap_short_info_, &count);
		if (!kern_return_t)
		{
			startAddr = 0;
			outSize = 0;
			prot = OSX.vm_prot_t.None;
			maxProt = OSX.vm_prot_t.None;
			return kern_return_t;
		}
		Helpers.Assert(!vm_region_submap_short_info_.is_submap, null, "!info.is_submap");
		startAddr = (nint)num2;
		outSize = (nint)num3;
		prot = vm_region_submap_short_info_.protection;
		maxProt = vm_region_submap_short_info_.max_protection;
		return kern_return_t;
	}

	void IInitialize<IArchitecture>.Initialize(IArchitecture value)
	{
		arch = value;
	}

	private unsafe PosixExceptionHelper CreateNativeExceptionHelper()
	{
		Helpers.Assert(arch != null, null, "arch is not null");
		if (arch.Target == ArchitectureKind.x86_64)
		{
			string text = "exhelper_macos_x86_64.dylib";
			string name = text;
			byte[] array = ArrayPool<byte>.Shared.Rent(NEHTempl.Length + 1);
			int num;
			string @string;
			try
			{
				array.AsSpan().Clear();
				NEHTempl.CopyTo(array);
				fixed (byte* template = array)
				{
					num = OSX.MkSTemp(template);
				}
				if (num == -1)
				{
					int errno = OSX.Errno;
					Win32Exception ex = new Win32Exception(errno);
					bool isEnabled;
					_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogErrorStringHandler message = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogErrorStringHandler(55, 2, out isEnabled);
					if (isEnabled)
					{
						message.AppendLiteral("Could not create temp file for NativeExceptionHelper: ");
						message.AppendFormatted(errno);
						message.AppendLiteral(" ");
						message.AppendFormatted(ex);
					}
					_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Error(ref message);
					throw ex;
				}
				@string = Encoding.UTF8.GetString(array, 0, NEHTempl.Length);
			}
			finally
			{
				ArrayPool<byte>.Shared.Return(array);
			}
			using (SafeFileHandle handle = new SafeFileHandle((IntPtr)num, ownsHandle: true))
			{
				using FileStream destination = new FileStream(handle, FileAccess.Write);
				using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
				Helpers.Assert(stream != null, null, "embedded is not null");
				stream.CopyTo(destination);
			}
			return PosixExceptionHelper.CreateHelper(arch, @string);
		}
		throw new NotImplementedException("No exception helper for current arch");
	}
}
