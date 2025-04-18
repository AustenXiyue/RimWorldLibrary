using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using MonoMod.Utils.Interop;

namespace MonoMod.Utils;

internal static class DynDll
{
	private abstract class BackendImpl
	{
		protected abstract bool TryOpenLibraryCore(string? name, Assembly assembly, out IntPtr handle);

		public abstract bool TryCloseLibrary(IntPtr handle);

		public abstract bool TryGetExport(IntPtr handle, string name, out IntPtr ptr);

		protected abstract void CheckAndThrowError();

		public virtual bool TryOpenLibrary(string? name, Assembly assembly, out IntPtr handle)
		{
			if (name != null)
			{
				foreach (string item in GetLibrarySearchOrder(name))
				{
					if (TryOpenLibraryCore(item, assembly, out handle))
					{
						return true;
					}
				}
				handle = IntPtr.Zero;
				return false;
			}
			return TryOpenLibraryCore(null, assembly, out handle);
		}

		protected virtual IEnumerable<string> GetLibrarySearchOrder(string name)
		{
			yield return name;
		}

		public virtual IntPtr OpenLibrary(string? name, Assembly assembly)
		{
			if (!TryOpenLibrary(name, assembly, out var handle))
			{
				CheckAndThrowError();
			}
			return handle;
		}

		public virtual void CloseLibrary(IntPtr handle)
		{
			if (!TryCloseLibrary(handle))
			{
				CheckAndThrowError();
			}
		}

		public virtual IntPtr GetExport(IntPtr handle, string name)
		{
			if (!TryGetExport(handle, name, out var ptr))
			{
				CheckAndThrowError();
			}
			return ptr;
		}
	}

	private sealed class WindowsBackend : BackendImpl
	{
		protected override void CheckAndThrowError()
		{
			uint lastError = MonoMod.Utils.Interop.Windows.GetLastError();
			if (lastError != 0)
			{
				throw new Win32Exception((int)lastError);
			}
		}

		protected unsafe override bool TryOpenLibraryCore(string? name, Assembly assembly, out IntPtr handle)
		{
			IntPtr intPtr;
			if (name == null)
			{
				intPtr = (handle = (nint)MonoMod.Utils.Interop.Windows.GetModuleHandleW(null));
			}
			else
			{
				fixed (char* lpLibFileName = name)
				{
					intPtr = (handle = (nint)MonoMod.Utils.Interop.Windows.LoadLibraryW((ushort*)lpLibFileName));
				}
			}
			return intPtr != IntPtr.Zero;
		}

		public unsafe override bool TryCloseLibrary(IntPtr handle)
		{
			return MonoMod.Utils.Interop.Windows.FreeLibrary(new MonoMod.Utils.Interop.Windows.HMODULE((void*)handle));
		}

		public unsafe override bool TryGetExport(IntPtr handle, string name, out IntPtr ptr)
		{
			byte[]? array = Unix.MarshalToUtf8(name);
			IntPtr intPtr;
			fixed (byte* lpProcName = array)
			{
				intPtr = (ptr = MonoMod.Utils.Interop.Windows.GetProcAddress(new MonoMod.Utils.Interop.Windows.HMODULE((void*)handle), (sbyte*)lpProcName));
			}
			Unix.FreeMarshalledArray(array);
			return intPtr != IntPtr.Zero;
		}

		protected override IEnumerable<string> GetLibrarySearchOrder(string name)
		{
			yield return name;
			if (!name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) && !name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
			{
				yield return name + ".dll";
			}
		}
	}

	private abstract class LibdlBackend : BackendImpl
	{
		[ThreadStatic]
		private static IntPtr lastDlErrorReturn;

		protected LibdlBackend()
		{
			Unix.DlError();
		}

		[DoesNotReturn]
		private static void ThrowError(IntPtr dlerr)
		{
			throw new Win32Exception(Marshal.PtrToStringAnsi(dlerr));
		}

		protected override void CheckAndThrowError()
		{
			IntPtr intPtr = lastDlErrorReturn;
			IntPtr intPtr2;
			if (intPtr == IntPtr.Zero)
			{
				intPtr2 = Unix.DlError();
			}
			else
			{
				intPtr2 = intPtr;
				lastDlErrorReturn = IntPtr.Zero;
			}
			if (intPtr2 != IntPtr.Zero)
			{
				ThrowError(intPtr2);
			}
		}

		protected override bool TryOpenLibraryCore(string? name, Assembly assembly, out IntPtr handle)
		{
			Unix.DlopenFlags flags = (Unix.DlopenFlags)258;
			return (handle = Unix.DlOpen(name, flags)) != IntPtr.Zero;
		}

		public override bool TryCloseLibrary(IntPtr handle)
		{
			return Unix.DlClose(handle);
		}

		public override bool TryGetExport(IntPtr handle, string name, out IntPtr ptr)
		{
			Unix.DlError();
			ptr = Unix.DlSym(handle, name);
			return (lastDlErrorReturn = Unix.DlError()) == IntPtr.Zero;
		}

		public override IntPtr GetExport(IntPtr handle, string name)
		{
			Unix.DlError();
			IntPtr result = Unix.DlSym(handle, name);
			IntPtr intPtr = Unix.DlError();
			if (intPtr != IntPtr.Zero)
			{
				ThrowError(intPtr);
			}
			return result;
		}
	}

	private sealed class LinuxOSXBackend : LibdlBackend
	{
		private readonly bool isLinux;

		public LinuxOSXBackend(bool isLinux)
		{
			this.isLinux = isLinux;
		}

		protected override IEnumerable<string> GetLibrarySearchOrder(string name)
		{
			bool hasSlash = name.Contains('/', StringComparison.Ordinal);
			string suffix = ".dylib";
			if (isLinux)
			{
				if (name.EndsWith(".so", StringComparison.Ordinal) || name.Contains(".so.", StringComparison.Ordinal))
				{
					yield return name;
					if (!hasSlash)
					{
						yield return "lib" + name;
					}
					yield return name + ".so";
					if (!hasSlash)
					{
						yield return "lib" + name + ".so";
					}
					yield break;
				}
				suffix = ".so";
			}
			yield return name + suffix;
			if (!hasSlash)
			{
				yield return "lib" + name + suffix;
			}
			yield return name;
			if (!hasSlash)
			{
				yield return "lib" + name;
			}
			bool flag = isLinux;
			if (flag)
			{
				bool flag2 = ((name == "c" || name == "libc") ? true : false);
				flag = flag2;
			}
			if (!flag)
			{
				yield break;
			}
			foreach (string item in GetLibrarySearchOrder("c.so.6"))
			{
				yield return item;
			}
			foreach (string item2 in GetLibrarySearchOrder("glibc"))
			{
				yield return item2;
			}
			foreach (string item3 in GetLibrarySearchOrder("glibc.so.6"))
			{
				yield return item3;
			}
		}
	}

	private sealed class UnknownPosixBackend : LibdlBackend
	{
	}

	private static readonly BackendImpl Backend = CreateCrossplatBackend();

	private static BackendImpl CreateCrossplatBackend()
	{
		OSKind oS = PlatformDetection.OS;
		if (oS.Is(OSKind.Windows))
		{
			return new WindowsBackend();
		}
		if (oS.Is(OSKind.Linux) || oS.Is(OSKind.OSX))
		{
			return new LinuxOSXBackend(oS.Is(OSKind.Linux));
		}
		bool isEnabled;
		MMDbgLog.DebugLogWarningStringHandler message = new MMDbgLog.DebugLogWarningStringHandler(55, 1, out isEnabled);
		if (isEnabled)
		{
			message.AppendLiteral("Unknown OS ");
			message.AppendFormatted(oS);
			message.AppendLiteral(" when setting up DynDll; assuming posix-like");
		}
		MMDbgLog.Warning(ref message);
		return new UnknownPosixBackend();
	}

	public static IntPtr OpenLibrary(string? name)
	{
		return Backend.OpenLibrary(name, Assembly.GetCallingAssembly());
	}

	public static bool TryOpenLibrary(string? name, out IntPtr libraryPtr)
	{
		return Backend.TryOpenLibrary(name, Assembly.GetCallingAssembly(), out libraryPtr);
	}

	public static void CloseLibrary(IntPtr lib)
	{
		Backend.CloseLibrary(lib);
	}

	public static bool TryCloseLibrary(IntPtr lib)
	{
		return Backend.TryCloseLibrary(lib);
	}

	public static IntPtr GetExport(this IntPtr libraryPtr, string name)
	{
		return Backend.GetExport(libraryPtr, name);
	}

	public static bool TryGetExport(this IntPtr libraryPtr, string name, out IntPtr functionPtr)
	{
		return Backend.TryGetExport(libraryPtr, name, out functionPtr);
	}
}
