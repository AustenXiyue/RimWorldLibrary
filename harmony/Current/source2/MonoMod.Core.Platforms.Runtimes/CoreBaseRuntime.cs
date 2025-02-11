using System;
using System.IO;
using System.Linq;
using MonoMod.Core.Utils;
using MonoMod.Utils;

namespace MonoMod.Core.Platforms.Runtimes;

internal abstract class CoreBaseRuntime : FxCoreBaseRuntime, IInitialize
{
	private IntPtr? lazyJitObject;

	private INativeExceptionHelper? lazyNativeExceptionHelper;

	public override RuntimeKind Target => RuntimeKind.CoreCLR;

	protected ISystem System { get; }

	protected IntPtr JitObject
	{
		get
		{
			IntPtr valueOrDefault = lazyJitObject.GetValueOrDefault();
			if (!lazyJitObject.HasValue)
			{
				valueOrDefault = GetJitObject();
				lazyJitObject = valueOrDefault;
				return valueOrDefault;
			}
			return valueOrDefault;
		}
	}

	protected INativeExceptionHelper? NativeExceptionHelper => lazyNativeExceptionHelper ?? (lazyNativeExceptionHelper = System.NativeExceptionHelper);

	public static CoreBaseRuntime CreateForVersion(Version version, ISystem system, IArchitecture arch)
	{
		switch (version.Major)
		{
		case 2:
		case 4:
			return new Core21Runtime(system);
		case 3:
			return version.Minor switch
			{
				0 => new Core30Runtime(system), 
				1 => new Core31Runtime(system), 
				_ => throw new PlatformNotSupportedException($"Unknown .NET Core 3.x minor version {version.Minor}"), 
			};
		case 5:
			return new Core50Runtime(system);
		case 6:
			return new Core60Runtime(system);
		case 7:
			return new Core70Runtime(system, arch);
		case 8:
			return new Core80Runtime(system, arch);
		default:
			throw new PlatformNotSupportedException($"CoreCLR version {version} is not supported");
		}
	}

	protected CoreBaseRuntime(ISystem system)
	{
		System = system;
		if (PlatformDetection.Architecture == ArchitectureKind.x86_64)
		{
			Abi? defaultAbi = system.DefaultAbi;
			if (defaultAbi.HasValue)
			{
				Abi valueOrDefault = defaultAbi.GetValueOrDefault();
				AbiCore = FxCoreBaseRuntime.AbiForCoreFx45X64(valueOrDefault);
			}
		}
	}

	void IInitialize.Initialize()
	{
		InstallJitHook(JitObject);
	}

	private static bool IsMaybeClrJitPath(string path)
	{
		return Path.GetFileNameWithoutExtension(path).EndsWith("clrjit", StringComparison.Ordinal);
	}

	protected virtual string GetClrJitPath()
	{
		string text = null;
		bool isEnabled;
		if (Switches.TryGetSwitchValue("JitPath", out object value))
		{
			string jitPath = value as string;
			if (jitPath != null)
			{
				_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogWarningStringHandler message;
				if (!IsMaybeClrJitPath(jitPath))
				{
					message = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogWarningStringHandler(77, 1, out isEnabled);
					if (isEnabled)
					{
						message.AppendLiteral("Provided value for MonoMod.JitPath switch '");
						message.AppendFormatted(jitPath);
						message.AppendLiteral("' does not look like a ClrJIT path");
					}
					_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Warning(ref message);
				}
				else
				{
					text = System.EnumerateLoadedModuleFiles().FirstOrDefault((string f) => f != null && f == jitPath);
					if (text == null)
					{
						message = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogWarningStringHandler(82, 1, out isEnabled);
						if (isEnabled)
						{
							message.AppendLiteral("Provided path for MonoMod.JitPath switch was not loaded in this process. jitPath: ");
							message.AppendFormatted(jitPath);
						}
						_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Warning(ref message);
					}
				}
			}
		}
		if (text == null)
		{
			text = System.EnumerateLoadedModuleFiles().FirstOrDefault((string f) => f != null && IsMaybeClrJitPath(f));
		}
		if (text == null)
		{
			throw new PlatformNotSupportedException("Could not locate clrjit library");
		}
		_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogTraceStringHandler message2 = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogTraceStringHandler(14, 1, out isEnabled);
		if (isEnabled)
		{
			message2.AppendLiteral("Got jit path: ");
			message2.AppendFormatted(text);
		}
		_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Trace(ref message2);
		return text;
	}

	private unsafe IntPtr GetJitObject()
	{
		if (!DynDll.TryOpenLibrary(GetClrJitPath(), out var libraryPtr))
		{
			throw new PlatformNotSupportedException("Could not open clrjit library");
		}
		try
		{
			return ((delegate* unmanaged[Stdcall]<IntPtr>)(void*)libraryPtr.GetExport("getJit"))();
		}
		catch
		{
			DynDll.CloseLibrary(libraryPtr);
			throw;
		}
	}

	protected abstract void InstallJitHook(IntPtr jit);

	protected IntPtr EHNativeToManaged(IntPtr target, out IDisposable? handle)
	{
		if (NativeExceptionHelper != null)
		{
			return NativeExceptionHelper.CreateNativeToManagedHelper(target, out handle);
		}
		handle = null;
		return target;
	}

	protected IntPtr EHManagedToNative(IntPtr target, out IDisposable? handle)
	{
		if (NativeExceptionHelper != null)
		{
			return NativeExceptionHelper.CreateManagedToNativeHelper(target, out handle);
		}
		handle = null;
		return target;
	}
}
