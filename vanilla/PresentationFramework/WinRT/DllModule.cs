using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using WinRT.Interop;

namespace WinRT;

internal class DllModule
{
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int DllGetActivationFactory(nint activatableClassId, out nint activationFactory);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int DllCanUnloadNow();

	private readonly string _fileName;

	private readonly nint _moduleHandle;

	private readonly DllGetActivationFactory _GetActivationFactory;

	private readonly DllCanUnloadNow _CanUnloadNow;

	private static readonly string _currentModuleDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

	private static Dictionary<string, DllModule> _cache = new Dictionary<string, DllModule>();

	public static DllModule Load(string fileName)
	{
		lock (_cache)
		{
			if (!_cache.TryGetValue(fileName, out var value))
			{
				value = new DllModule(fileName);
				_cache[fileName] = value;
			}
			return value;
		}
	}

	private DllModule(string fileName)
	{
		_fileName = fileName;
		_moduleHandle = Platform.LoadLibraryExW(Path.Combine(_currentModuleDirectory, fileName), IntPtr.Zero, 8u);
		if (_moduleHandle == IntPtr.Zero)
		{
			try
			{
				_moduleHandle = NativeLibrary.Load(fileName, Assembly.GetExecutingAssembly(), null);
			}
			catch (Exception)
			{
			}
		}
		if (_moduleHandle == IntPtr.Zero)
		{
			Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
		}
		_GetActivationFactory = Platform.GetProcAddress<DllGetActivationFactory>(_moduleHandle);
		nint procAddress = Platform.GetProcAddress(_moduleHandle, "DllCanUnloadNow");
		if (procAddress != IntPtr.Zero)
		{
			_CanUnloadNow = Marshal.GetDelegateForFunctionPointer<DllCanUnloadNow>(procAddress);
		}
	}

	public (ObjectReference<IActivationFactoryVftbl> obj, int hr) GetActivationFactory(string runtimeClassId)
	{
		MarshalString m = MarshalString.CreateMarshaler(runtimeClassId);
		nint activationFactory;
		int num = _GetActivationFactory(MarshalString.GetAbi(m), out activationFactory);
		return (obj: (num == 0) ? ObjectReference<IActivationFactoryVftbl>.Attach(ref activationFactory) : null, hr: num);
	}

	~DllModule()
	{
		lock (_cache)
		{
			_cache.Remove(_fileName);
		}
		if (_moduleHandle != IntPtr.Zero && !Platform.FreeLibrary(_moduleHandle))
		{
			Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
		}
	}
}
