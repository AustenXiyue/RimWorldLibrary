using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using MonoMod.Utils.Interop;

namespace MonoMod.Utils;

internal static class PlatformDetection
{
	private static int platInitState;

	private static OSKind os;

	private static ArchitectureKind arch;

	private static int runtimeInitState;

	private static RuntimeKind runtime;

	private static Version? runtimeVersion;

	public static OSKind OS
	{
		get
		{
			EnsurePlatformInfoInitialized();
			return os;
		}
	}

	public static ArchitectureKind Architecture
	{
		get
		{
			EnsurePlatformInfoInitialized();
			return arch;
		}
	}

	public static RuntimeKind Runtime
	{
		get
		{
			EnsureRuntimeInitialized();
			return runtime;
		}
	}

	public static Version RuntimeVersion
	{
		get
		{
			EnsureRuntimeInitialized();
			return runtimeVersion;
		}
	}

	private static void EnsurePlatformInfoInitialized()
	{
		if (platInitState == 0)
		{
			(os, arch) = DetectPlatformInfo();
			Thread.MemoryBarrier();
			Interlocked.Exchange(ref platInitState, 1);
		}
	}

	private static (OSKind OS, ArchitectureKind Arch) DetectPlatformInfo()
	{
		OSKind oSKind = OSKind.Unknown;
		ArchitectureKind architectureKind = ArchitectureKind.Unknown;
		PropertyInfo property = typeof(Environment).GetProperty("Platform", BindingFlags.Static | BindingFlags.NonPublic);
		string self = ((!(property != null)) ? Environment.OSVersion.Platform.ToString() : property.GetValue(null, null)?.ToString())?.ToUpperInvariant() ?? "";
		if (self.Contains("WIN", StringComparison.Ordinal))
		{
			oSKind = OSKind.Windows;
		}
		else if (self.Contains("MAC", StringComparison.Ordinal) || self.Contains("OSX", StringComparison.Ordinal))
		{
			oSKind = OSKind.OSX;
		}
		else if (self.Contains("LIN", StringComparison.Ordinal))
		{
			oSKind = OSKind.Linux;
		}
		else if (self.Contains("BSD", StringComparison.Ordinal))
		{
			oSKind = OSKind.BSD;
		}
		else if (self.Contains("UNIX", StringComparison.Ordinal))
		{
			oSKind = OSKind.Posix;
		}
		if (oSKind == OSKind.Windows)
		{
			DetectInfoWindows(ref oSKind, ref architectureKind);
		}
		else if ((oSKind & OSKind.Posix) != 0)
		{
			DetectInfoPosix(ref oSKind, ref architectureKind);
		}
		if (oSKind != 0)
		{
			if (oSKind == OSKind.Linux && Directory.Exists("/data") && File.Exists("/system/build.prop"))
			{
				oSKind = OSKind.Android;
			}
			else if (oSKind == OSKind.Posix && Directory.Exists("/Applications") && Directory.Exists("/System") && Directory.Exists("/User") && !Directory.Exists("/Users"))
			{
				oSKind = OSKind.IOS;
			}
			else if (oSKind == OSKind.Windows && CheckWine())
			{
				oSKind = OSKind.Wine;
			}
		}
		bool isEnabled;
		MMDbgLog.DebugLogInfoStringHandler message = new MMDbgLog.DebugLogInfoStringHandler(16, 2, out isEnabled);
		if (isEnabled)
		{
			message.AppendLiteral("Platform info: ");
			message.AppendFormatted(oSKind);
			message.AppendLiteral(" ");
			message.AppendFormatted(architectureKind);
		}
		MMDbgLog.Info(ref message);
		return (OS: oSKind, Arch: architectureKind);
	}

	private unsafe static int PosixUname(OSKind os, byte* buf)
	{
		if (os != OSKind.OSX)
		{
			return Libc(buf);
		}
		return Osx(buf);
		unsafe static int Libc(byte* buf)
		{
			return Unix.Uname(buf);
		}
		unsafe static int Osx(byte* buf)
		{
			return OSX.Uname(buf);
		}
	}

	private unsafe static string GetCString(ReadOnlySpan<byte> buffer, out int nullByte)
	{
		fixed (byte* ptr = buffer)
		{
			return Marshal.PtrToStringAnsi((IntPtr)ptr, nullByte = buffer.IndexOf<byte>(0));
		}
	}

	private unsafe static void DetectInfoPosix(ref OSKind os, ref ArchitectureKind arch)
	{
		MMDbgLog.DebugLogErrorStringHandler message2;
		bool isEnabled;
		try
		{
			Span<byte> span = new byte[3078];
			fixed (byte* buf = span)
			{
				if (PosixUname(os, buf) < 0)
				{
					string message = new Win32Exception(Marshal.GetLastWin32Error()).Message;
					message2 = new MMDbgLog.DebugLogErrorStringHandler(24, 1, out isEnabled);
					if (isEnabled)
					{
						message2.AppendLiteral("uname() syscall failed! ");
						message2.AppendFormatted(message);
					}
					MMDbgLog.Error(ref message2);
					return;
				}
			}
			int nullByte;
			string text = GetCString(span, out nullByte).ToUpperInvariant();
			span = span.Slice(nullByte);
			MMDbgLog.DebugLogTraceStringHandler message3 = new MMDbgLog.DebugLogTraceStringHandler(22, 1, out isEnabled);
			if (isEnabled)
			{
				message3.AppendLiteral("uname() call returned ");
				message3.AppendFormatted(text);
			}
			MMDbgLog.Trace(ref message3);
			if (text.Contains("LINUX", StringComparison.Ordinal))
			{
				os = OSKind.Linux;
			}
			else if (text.Contains("DARWIN", StringComparison.Ordinal))
			{
				os = OSKind.OSX;
			}
			else if (text.Contains("BSD", StringComparison.Ordinal))
			{
				os = OSKind.BSD;
			}
			string text2;
			if (os == OSKind.Linux)
			{
				Span<Unix.LinuxAuxvEntry> span2 = MemoryMarshal.Cast<byte, Unix.LinuxAuxvEntry>(Helpers.ReadAllBytes("/proc/self/auxv").AsSpan());
				text2 = string.Empty;
				Span<Unix.LinuxAuxvEntry> span3 = span2;
				for (int i = 0; i < span3.Length; i++)
				{
					Unix.LinuxAuxvEntry linuxAuxvEntry = span3[i];
					if (linuxAuxvEntry.Key == 15)
					{
						text2 = Marshal.PtrToStringAnsi(linuxAuxvEntry.Value) ?? string.Empty;
						break;
					}
				}
				if (text2.Length == 0)
				{
					MMDbgLog.DebugLogWarningStringHandler message4 = new MMDbgLog.DebugLogWarningStringHandler(56, 1, out isEnabled);
					if (isEnabled)
					{
						message4.AppendLiteral("Auxv table did not inlcude useful AT_PLATFORM (0x");
						message4.AppendFormatted(15, "x");
						message4.AppendLiteral(") entry");
					}
					MMDbgLog.Warning(ref message4);
					span3 = span2;
					for (int i = 0; i < span3.Length; i++)
					{
						Unix.LinuxAuxvEntry linuxAuxvEntry2 = span3[i];
						message3 = new MMDbgLog.DebugLogTraceStringHandler(3, 2, out isEnabled);
						if (isEnabled)
						{
							message3.AppendFormatted(linuxAuxvEntry2.Key, "x16");
							message3.AppendLiteral(" = ");
							message3.AppendFormatted(linuxAuxvEntry2.Value, "x16");
						}
						MMDbgLog.Trace(ref message3);
					}
				}
				else
				{
					message3 = new MMDbgLog.DebugLogTraceStringHandler(43, 1, out isEnabled);
					if (isEnabled)
					{
						message3.AppendLiteral("Got architecture name ");
						message3.AppendFormatted(text2);
						message3.AppendLiteral(" from /proc/self/auxv");
					}
					MMDbgLog.Trace(ref message3);
				}
			}
			else
			{
				for (int j = 0; j < 4; j++)
				{
					if (j != 0)
					{
						nullByte = span.IndexOf<byte>(0);
						span = span.Slice(nullByte);
					}
					int k;
					for (k = 0; k < span.Length && span[k] == 0; k++)
					{
					}
					span = span.Slice(k);
				}
				text2 = GetCString(span, out var _);
				message3 = new MMDbgLog.DebugLogTraceStringHandler(35, 1, out isEnabled);
				if (isEnabled)
				{
					message3.AppendLiteral("Got architecture name ");
					message3.AppendFormatted(text2);
					message3.AppendLiteral(" from uname()");
				}
				MMDbgLog.Trace(ref message3);
			}
			text2 = text2.ToUpperInvariant();
			if (text2.Contains("X86_64", StringComparison.Ordinal))
			{
				arch = ArchitectureKind.x86_64;
			}
			else if (text2.Contains("AMD64", StringComparison.Ordinal))
			{
				arch = ArchitectureKind.x86_64;
			}
			else if (text2.Contains("X86", StringComparison.Ordinal))
			{
				arch = ArchitectureKind.x86;
			}
			else if (text2.Contains("AARCH64", StringComparison.Ordinal))
			{
				arch = ArchitectureKind.Arm64;
			}
			else if (text2.Contains("ARM64", StringComparison.Ordinal))
			{
				arch = ArchitectureKind.Arm64;
			}
			else if (text2.Contains("ARM", StringComparison.Ordinal))
			{
				arch = ArchitectureKind.Arm;
			}
			message3 = new MMDbgLog.DebugLogTraceStringHandler(37, 2, out isEnabled);
			if (isEnabled)
			{
				message3.AppendLiteral("uname() detected architecture info: ");
				message3.AppendFormatted(os);
				message3.AppendLiteral(" ");
				message3.AppendFormatted(arch);
			}
			MMDbgLog.Trace(ref message3);
		}
		catch (Exception value)
		{
			message2 = new MMDbgLog.DebugLogErrorStringHandler(49, 1, out isEnabled);
			if (isEnabled)
			{
				message2.AppendLiteral("Error trying to detect info on POSIX-like system ");
				message2.AppendFormatted(value);
			}
			MMDbgLog.Error(ref message2);
		}
	}

	private unsafe static void DetectInfoWindows(ref OSKind os, ref ArchitectureKind arch)
	{
		MonoMod.Utils.Interop.Windows.SYSTEM_INFO sYSTEM_INFO = default(MonoMod.Utils.Interop.Windows.SYSTEM_INFO);
		MonoMod.Utils.Interop.Windows.GetSystemInfo(&sYSTEM_INFO);
		ushort wProcessorArchitecture = sYSTEM_INFO.Anonymous.Anonymous.wProcessorArchitecture;
		arch = wProcessorArchitecture switch
		{
			9 => ArchitectureKind.x86_64, 
			6 => throw new PlatformNotSupportedException("You're running .NET on an Itanium device!?!?"), 
			0 => ArchitectureKind.x86, 
			5 => ArchitectureKind.Arm, 
			12 => ArchitectureKind.Arm64, 
			_ => throw new PlatformNotSupportedException($"Unknown Windows processor architecture {wProcessorArchitecture}"), 
		};
	}

	private unsafe static bool CheckWine()
	{
		if (Switches.TryGetSwitchEnabled("RunningOnWine", out var isEnabled))
		{
			return isEnabled;
		}
		string text = Environment.GetEnvironmentVariable("XL_WINEONLINUX")?.ToUpperInvariant();
		if (text == "TRUE")
		{
			return true;
		}
		if (text == "FALSE")
		{
			return false;
		}
		fixed (char* lpModuleName = "ntdll.dll")
		{
			MonoMod.Utils.Interop.Windows.HMODULE moduleHandleW = MonoMod.Utils.Interop.Windows.GetModuleHandleW((ushort*)lpModuleName);
			if (moduleHandleW != MonoMod.Utils.Interop.Windows.HMODULE.NULL && moduleHandleW != MonoMod.Utils.Interop.Windows.HMODULE.INVALID_VALUE)
			{
				fixed (byte* lpProcName = "wineGetVersion"u8)
				{
					if (MonoMod.Utils.Interop.Windows.GetProcAddress(moduleHandleW, (sbyte*)lpProcName) != IntPtr.Zero)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	[MemberNotNull("runtimeVersion")]
	private static void EnsureRuntimeInitialized()
	{
		if (runtimeInitState != 0)
		{
			if ((object)runtimeVersion == null)
			{
				throw new InvalidOperationException("Despite runtimeInitState being set, runtimeVersion was somehow null");
			}
		}
		else
		{
			(runtime, runtimeVersion) = DetermineRuntimeInfo();
			Thread.MemoryBarrier();
			Interlocked.Exchange(ref runtimeInitState, 1);
		}
	}

	private static (RuntimeKind Rt, Version Ver) DetermineRuntimeInfo()
	{
		Version version = null;
		bool flag = Type.GetType("Mono.Runtime") != null || Type.GetType("Mono.RuntimeStructs") != null;
		bool flag2 = typeof(object).Assembly.GetName().Name == "System.Private.CoreLib";
		RuntimeKind runtimeKind = (flag ? RuntimeKind.Mono : ((!flag2 || flag) ? RuntimeKind.Framework : RuntimeKind.CoreCLR));
		bool isEnabled;
		MMDbgLog.DebugLogTraceStringHandler message = new MMDbgLog.DebugLogTraceStringHandler(21, 2, out isEnabled);
		if (isEnabled)
		{
			message.AppendLiteral("IsMono: ");
			message.AppendFormatted(flag);
			message.AppendLiteral(", IsCoreBcl: ");
			message.AppendFormatted(flag2);
		}
		MMDbgLog.Trace(ref message);
		Version version2 = Environment.Version;
		message = new MMDbgLog.DebugLogTraceStringHandler(25, 1, out isEnabled);
		if (isEnabled)
		{
			message.AppendLiteral("Returned system version: ");
			message.AppendFormatted(version2);
		}
		MMDbgLog.Trace(ref message);
		Type type = Type.GetType("System.Runtime.InteropServices.RuntimeInformation");
		if ((object)type == null)
		{
			type = Type.GetType("System.Runtime.InteropServices.RuntimeInformation, System.Runtime.InteropServices.RuntimeInformation");
		}
		string text = (string)type?.GetProperty("FrameworkDescription")?.GetValue(null, null);
		message = new MMDbgLog.DebugLogTraceStringHandler(22, 1, out isEnabled);
		if (isEnabled)
		{
			message.AppendLiteral("FrameworkDescription: ");
			message.AppendFormatted(text ?? "(null)");
		}
		MMDbgLog.Trace(ref message);
		if (text != null)
		{
			int length;
			if (text.StartsWith("Mono ", StringComparison.Ordinal))
			{
				runtimeKind = RuntimeKind.Mono;
				length = "Mono ".Length;
			}
			else if (text.StartsWith(".NET Core ", StringComparison.Ordinal))
			{
				runtimeKind = RuntimeKind.CoreCLR;
				length = ".NET Core ".Length;
			}
			else if (text.StartsWith(".NET Framework ", StringComparison.Ordinal))
			{
				runtimeKind = RuntimeKind.Framework;
				length = ".NET Framework ".Length;
			}
			else if (text.StartsWith(".NET ", StringComparison.Ordinal))
			{
				runtimeKind = RuntimeKind.CoreCLR;
				length = ".NET ".Length;
			}
			else
			{
				runtimeKind = RuntimeKind.Unknown;
				length = text.Length;
			}
			int num = text.IndexOfAny(new char[2] { ' ', '-' }, length);
			if (num < 0)
			{
				num = text.Length;
			}
			string version3 = text.Substring(length, num - length);
			try
			{
				version = new Version(version3);
			}
			catch (Exception value)
			{
				MMDbgLog.DebugLogErrorStringHandler message2 = new MMDbgLog.DebugLogErrorStringHandler(61, 2, out isEnabled);
				if (isEnabled)
				{
					message2.AppendLiteral("Invalid version string pulled from FrameworkDescription ('");
					message2.AppendFormatted(text);
					message2.AppendLiteral("') ");
					message2.AppendFormatted(value);
				}
				MMDbgLog.Error(ref message2);
			}
		}
		if (runtimeKind == RuntimeKind.Framework && (object)version == null)
		{
			version = version2;
		}
		MMDbgLog.DebugLogInfoStringHandler message3 = new MMDbgLog.DebugLogInfoStringHandler(19, 2, out isEnabled);
		if (isEnabled)
		{
			message3.AppendLiteral("Detected runtime: ");
			message3.AppendFormatted(runtimeKind);
			message3.AppendLiteral(" ");
			message3.AppendFormatted(version?.ToString() ?? "(null)");
		}
		MMDbgLog.Info(ref message3);
		return (Rt: runtimeKind, Ver: version ?? new Version(0, 0));
	}
}
