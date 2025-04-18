using System;
using MonoMod.Utils;

namespace MonoMod.Core.Platforms.Systems;

internal sealed class PosixExceptionHelper : INativeExceptionHelper
{
	private readonly IArchitecture arch;

	private readonly IntPtr eh_get_exception_ptr;

	private readonly IntPtr eh_managed_to_native;

	private readonly IntPtr eh_native_to_managed;

	public unsafe IntPtr NativeException
	{
		get
		{
			return *((delegate* unmanaged[Cdecl]<IntPtr*>)(void*)eh_get_exception_ptr)();
		}
		set
		{
			*((delegate* unmanaged[Cdecl]<IntPtr*>)(void*)eh_get_exception_ptr)() = value;
		}
	}

	public unsafe GetExceptionSlot GetExceptionSlot => () => ((delegate* unmanaged[Cdecl]<IntPtr*>)(void*)eh_get_exception_ptr)();

	private PosixExceptionHelper(IArchitecture arch, IntPtr getExPtr, IntPtr m2n, IntPtr n2m)
	{
		this.arch = arch;
		eh_get_exception_ptr = getExPtr;
		eh_managed_to_native = m2n;
		eh_native_to_managed = n2m;
	}

	public static PosixExceptionHelper CreateHelper(IArchitecture arch, string filename)
	{
		IntPtr intPtr = DynDll.OpenLibrary(filename);
		IntPtr export;
		IntPtr export2;
		IntPtr export3;
		try
		{
			export = intPtr.GetExport("eh_get_exception_ptr");
			export2 = intPtr.GetExport("eh_managed_to_native");
			export3 = intPtr.GetExport("eh_native_to_managed");
			Helpers.Assert(export != IntPtr.Zero, null, "eh_get_exception_ptr != IntPtr.Zero");
			Helpers.Assert(export2 != IntPtr.Zero, null, "eh_managed_to_native != IntPtr.Zero");
			Helpers.Assert(export3 != IntPtr.Zero, null, "eh_native_to_managed != IntPtr.Zero");
		}
		catch
		{
			DynDll.CloseLibrary(intPtr);
			throw;
		}
		return new PosixExceptionHelper(arch, export, export2, export3);
	}

	public IntPtr CreateManagedToNativeHelper(IntPtr target, out IDisposable? handle)
	{
		return ((IAllocatedMemory)(handle = arch.CreateSpecialEntryStub(eh_managed_to_native, target))).BaseAddress;
	}

	public IntPtr CreateNativeToManagedHelper(IntPtr target, out IDisposable? handle)
	{
		return ((IAllocatedMemory)(handle = arch.CreateSpecialEntryStub(eh_native_to_managed, target))).BaseAddress;
	}
}
