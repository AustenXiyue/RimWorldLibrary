using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using MonoMod.Core.Interop;
using MonoMod.Core.Platforms.Memory;
using MonoMod.Utils;

namespace MonoMod.Core.Platforms.Systems;

internal sealed class WindowsSystem : ISystem
{
	private sealed class PageAllocator : QueryingMemoryPageAllocatorBase
	{
		public override uint PageSize { get; }

		public unsafe PageAllocator()
		{
			MonoMod.Core.Interop.Windows.SYSTEM_INFO sYSTEM_INFO = default(MonoMod.Core.Interop.Windows.SYSTEM_INFO);
			MonoMod.Core.Interop.Windows.GetSystemInfo(&sYSTEM_INFO);
			PageSize = sYSTEM_INFO.dwPageSize;
		}

		public unsafe override bool TryAllocatePage(nint size, bool executable, out IntPtr allocated)
		{
			int flProtect = (executable ? 64 : 4);
			allocated = (IntPtr)MonoMod.Core.Interop.Windows.VirtualAlloc(null, (nuint)size, 12288u, (uint)flProtect);
			return allocated != IntPtr.Zero;
		}

		public unsafe override bool TryAllocatePage(IntPtr pageAddr, nint size, bool executable, out IntPtr allocated)
		{
			int flProtect = (executable ? 64 : 4);
			allocated = (IntPtr)MonoMod.Core.Interop.Windows.VirtualAlloc((void*)pageAddr, (nuint)size, 12288u, (uint)flProtect);
			return allocated != IntPtr.Zero;
		}

		public unsafe override bool TryFreePage(IntPtr pageAddr, [_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003ENotNullWhen(false)] out string? errorMsg)
		{
			if (!MonoMod.Core.Interop.Windows.VirtualFree((void*)pageAddr, 0u, 32768u))
			{
				errorMsg = new Win32Exception((int)MonoMod.Core.Interop.Windows.GetLastError()).Message;
				return false;
			}
			errorMsg = null;
			return true;
		}

		public unsafe override bool TryQueryPage(IntPtr pageAddr, out bool isFree, out IntPtr allocBase, out nint allocSize)
		{
			MonoMod.Core.Interop.Windows.MEMORY_BASIC_INFORMATION mEMORY_BASIC_INFORMATION = default(MonoMod.Core.Interop.Windows.MEMORY_BASIC_INFORMATION);
			if (MonoMod.Core.Interop.Windows.VirtualQuery((void*)pageAddr, &mEMORY_BASIC_INFORMATION, (nuint)sizeof(MonoMod.Core.Interop.Windows.MEMORY_BASIC_INFORMATION)) != 0)
			{
				isFree = mEMORY_BASIC_INFORMATION.State == 65536;
				allocBase = (nint)(isFree ? mEMORY_BASIC_INFORMATION.BaseAddress : mEMORY_BASIC_INFORMATION.AllocationBase);
				allocSize = (nint)pageAddr + (nint)mEMORY_BASIC_INFORMATION.RegionSize - (nint)allocBase;
				return true;
			}
			isFree = false;
			allocBase = IntPtr.Zero;
			allocSize = 0;
			return false;
		}
	}

	public OSKind Target => OSKind.Windows;

	public SystemFeature Features => SystemFeature.RWXPages;

	public INativeExceptionHelper? NativeExceptionHelper => null;

	public Abi? DefaultAbi { get; }

	public IMemoryAllocator MemoryAllocator { get; } = new QueryingPagedMemoryAllocator(new PageAllocator());

	private static TypeClassification ClassifyX64(Type type, bool isReturn)
	{
		int managedSize = type.GetManagedSize();
		if (((uint)(managedSize - 1) <= 1u || managedSize == 4 || managedSize == 8) ? true : false)
		{
			return TypeClassification.InRegister;
		}
		return TypeClassification.ByReference;
	}

	private static TypeClassification ClassifyX86(Type type, bool isReturn)
	{
		if (!isReturn)
		{
			return TypeClassification.OnStack;
		}
		int managedSize = type.GetManagedSize();
		if (((uint)(managedSize - 1) <= 1u || managedSize == 4) ? true : false)
		{
			return TypeClassification.InRegister;
		}
		return TypeClassification.ByReference;
	}

	public WindowsSystem()
	{
		if (PlatformDetection.Architecture == ArchitectureKind.x86_64)
		{
			DefaultAbi = new Abi(new SpecialArgumentKind[3]
			{
				SpecialArgumentKind.ReturnBuffer,
				SpecialArgumentKind.ThisPointer,
				SpecialArgumentKind.UserArguments
			}, ClassifyX64, ReturnsReturnBuffer: true);
		}
		else if (PlatformDetection.Architecture == ArchitectureKind.x86)
		{
			DefaultAbi = new Abi(new SpecialArgumentKind[3]
			{
				SpecialArgumentKind.ThisPointer,
				SpecialArgumentKind.ReturnBuffer,
				SpecialArgumentKind.UserArguments
			}, ClassifyX86, ReturnsReturnBuffer: true);
		}
	}

	public unsafe void PatchData(PatchTargetKind patchKind, IntPtr patchTarget, ReadOnlySpan<byte> data, Span<byte> backup)
	{
		if (patchKind == PatchTargetKind.Executable)
		{
			ProtectRWX(patchTarget, (nuint)data.Length);
		}
		else
		{
			ProtectRW(patchTarget, (nuint)data.Length);
		}
		Span<byte> destination = new Span<byte>((void*)patchTarget, data.Length);
		destination.TryCopyTo(backup);
		data.CopyTo(destination);
		if (patchKind == PatchTargetKind.Executable)
		{
			FlushInstructionCache(patchTarget, (nuint)data.Length);
		}
	}

	private unsafe static void ProtectRW(IntPtr addr, nuint size)
	{
		uint num = default(uint);
		if (!MonoMod.Core.Interop.Windows.VirtualProtect((void*)addr, size, 4u, &num))
		{
			throw LogAllSections(MonoMod.Core.Interop.Windows.GetLastError(), addr, size, "ProtectRW");
		}
	}

	private unsafe static void ProtectRWX(IntPtr addr, nuint size)
	{
		uint num = default(uint);
		if (!MonoMod.Core.Interop.Windows.VirtualProtect((void*)addr, size, 64u, &num))
		{
			throw LogAllSections(MonoMod.Core.Interop.Windows.GetLastError(), addr, size, "ProtectRWX");
		}
	}

	private unsafe static void FlushInstructionCache(IntPtr addr, nuint size)
	{
		if (!MonoMod.Core.Interop.Windows.FlushInstructionCache(MonoMod.Core.Interop.Windows.GetCurrentProcess(), (void*)addr, size))
		{
			throw LogAllSections(MonoMod.Core.Interop.Windows.GetLastError(), addr, size, "FlushInstructionCache");
		}
	}

	public IEnumerable<string?> EnumerateLoadedModuleFiles()
	{
		return from ProcessModule m in Process.GetCurrentProcess().Modules
			select m.FileName;
	}

	public unsafe nint GetSizeOfReadableMemory(nint start, nint guess)
	{
		nint num = 0;
		MonoMod.Core.Interop.Windows.MEMORY_BASIC_INFORMATION mEMORY_BASIC_INFORMATION = default(MonoMod.Core.Interop.Windows.MEMORY_BASIC_INFORMATION);
		do
		{
			bool isEnabled;
			if (MonoMod.Core.Interop.Windows.VirtualQuery((void*)start, &mEMORY_BASIC_INFORMATION, (nuint)sizeof(MonoMod.Core.Interop.Windows.MEMORY_BASIC_INFORMATION)) == 0)
			{
				uint lastError = MonoMod.Core.Interop.Windows.GetLastError();
				_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogWarningStringHandler message = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogWarningStringHandler(22, 2, out isEnabled);
				if (isEnabled)
				{
					message.AppendLiteral("VirtualQuery failed: ");
					message.AppendFormatted(lastError);
					message.AppendLiteral(" ");
					message.AppendFormatted(new Win32Exception((int)lastError).Message);
				}
				_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Warning(ref message);
				return 0;
			}
			_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogSpamStringHandler message2 = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogSpamStringHandler(56, 4, out isEnabled);
			if (isEnabled)
			{
				message2.AppendLiteral("VirtualQuery(0x");
				message2.AppendFormatted(start, "x16");
				message2.AppendLiteral(") == { Protect = ");
				message2.AppendFormatted(mEMORY_BASIC_INFORMATION.Protect, "x");
				message2.AppendLiteral(", BaseAddr = ");
				message2.AppendFormatted((UIntPtr)mEMORY_BASIC_INFORMATION.BaseAddress, "x16");
				message2.AppendLiteral(", Size = ");
				message2.AppendFormatted(mEMORY_BASIC_INFORMATION.RegionSize, "x4");
				message2.AppendLiteral(" }");
			}
			_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Spam(ref message2);
			if ((mEMORY_BASIC_INFORMATION.Protect & 0x66) == 0)
			{
				return num;
			}
			nint num2 = (nint)((byte*)mEMORY_BASIC_INFORMATION.BaseAddress + mEMORY_BASIC_INFORMATION.RegionSize);
			num += num2 - start;
			start = num2;
		}
		while (num < guess);
		return num;
	}

	private unsafe static Exception LogAllSections(uint error, IntPtr src, nuint size, [CallerMemberName] string from = "")
	{
		Exception ex = new Win32Exception((int)error);
		if (!_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.IsWritingLog)
		{
			return ex;
		}
		bool isEnabled;
		_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogErrorStringHandler message = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogErrorStringHandler(47, 3, out isEnabled);
		if (isEnabled)
		{
			message.AppendFormatted(from);
			message.AppendLiteral(" failed for 0x");
			message.AppendFormatted(src, "X16");
			message.AppendLiteral(" + ");
			message.AppendFormatted(size);
			message.AppendLiteral(" - logging all memory sections");
		}
		_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Error(ref message);
		message = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogErrorStringHandler(8, 1, out isEnabled);
		if (isEnabled)
		{
			message.AppendLiteral("reason: ");
			message.AppendFormatted(ex.Message);
		}
		_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Error(ref message);
		try
		{
			IntPtr intPtr = (IntPtr)65536;
			int num = 0;
			MonoMod.Core.Interop.Windows.MEMORY_BASIC_INFORMATION mEMORY_BASIC_INFORMATION = default(MonoMod.Core.Interop.Windows.MEMORY_BASIC_INFORMATION);
			while (MonoMod.Core.Interop.Windows.VirtualQuery((void*)intPtr, &mEMORY_BASIC_INFORMATION, (nuint)sizeof(MonoMod.Core.Interop.Windows.MEMORY_BASIC_INFORMATION)) != 0)
			{
				nuint num2 = (nuint)(nint)src + size;
				void* baseAddress = mEMORY_BASIC_INFORMATION.BaseAddress;
				nuint num3 = (nuint)((byte*)baseAddress + mEMORY_BASIC_INFORMATION.RegionSize);
				bool flag = (nuint)baseAddress <= num2 && (nuint)(nint)src <= num3;
				_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogTraceStringHandler message2 = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogTraceStringHandler(2, 2, out isEnabled);
				if (isEnabled)
				{
					message2.AppendFormatted(flag ? "*" : "-");
					message2.AppendLiteral(" #");
					message2.AppendFormatted(num++);
				}
				_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Trace(ref message2);
				message2 = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogTraceStringHandler(8, 1, out isEnabled);
				if (isEnabled)
				{
					message2.AppendLiteral("addr: 0x");
					message2.AppendFormatted((UIntPtr)mEMORY_BASIC_INFORMATION.BaseAddress, "X16");
				}
				_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Trace(ref message2);
				message2 = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogTraceStringHandler(8, 1, out isEnabled);
				if (isEnabled)
				{
					message2.AppendLiteral("size: 0x");
					message2.AppendFormatted(mEMORY_BASIC_INFORMATION.RegionSize, "X16");
				}
				_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Trace(ref message2);
				message2 = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogTraceStringHandler(9, 1, out isEnabled);
				if (isEnabled)
				{
					message2.AppendLiteral("aaddr: 0x");
					message2.AppendFormatted((UIntPtr)mEMORY_BASIC_INFORMATION.AllocationBase, "X16");
				}
				_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Trace(ref message2);
				message2 = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogTraceStringHandler(7, 1, out isEnabled);
				if (isEnabled)
				{
					message2.AppendLiteral("state: ");
					message2.AppendFormatted(mEMORY_BASIC_INFORMATION.State);
				}
				_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Trace(ref message2);
				message2 = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogTraceStringHandler(6, 1, out isEnabled);
				if (isEnabled)
				{
					message2.AppendLiteral("type: ");
					message2.AppendFormatted(mEMORY_BASIC_INFORMATION.Type);
				}
				_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Trace(ref message2);
				message2 = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogTraceStringHandler(9, 1, out isEnabled);
				if (isEnabled)
				{
					message2.AppendLiteral("protect: ");
					message2.AppendFormatted(mEMORY_BASIC_INFORMATION.Protect);
				}
				_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Trace(ref message2);
				message2 = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogTraceStringHandler(10, 1, out isEnabled);
				if (isEnabled)
				{
					message2.AppendLiteral("aprotect: ");
					message2.AppendFormatted(mEMORY_BASIC_INFORMATION.AllocationProtect);
				}
				_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Trace(ref message2);
				try
				{
					IntPtr intPtr2 = intPtr;
					intPtr = (IntPtr)((long)mEMORY_BASIC_INFORMATION.BaseAddress + (long)mEMORY_BASIC_INFORMATION.RegionSize);
					if ((ulong)(long)intPtr <= (ulong)(long)intPtr2)
					{
						break;
					}
				}
				catch (OverflowException value)
				{
					message = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogErrorStringHandler(9, 1, out isEnabled);
					if (isEnabled)
					{
						message.AppendLiteral("overflow ");
						message.AppendFormatted(value);
					}
					_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Error(ref message);
					break;
				}
			}
		}
		catch
		{
			throw ex;
		}
		return ex;
	}
}
