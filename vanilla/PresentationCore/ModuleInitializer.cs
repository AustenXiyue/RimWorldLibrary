using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Media;
using MS.Internal;

internal static class ModuleInitializer
{
	[ModuleInitializer]
	public static void Initialize()
	{
		IsProcessDpiAware();
		NativeWPFDLLLoader.LoadDwrite();
	}

	private static void IsProcessDpiAware()
	{
		bool flag = false;
		Assembly entryAssembly = Assembly.GetEntryAssembly();
		if (entryAssembly != null && Attribute.IsDefined(entryAssembly, typeof(DisableDpiAwarenessAttribute)))
		{
			flag = true;
		}
		if (!flag)
		{
			SetProcessDPIAware_Internal();
		}
	}

	[DllImport("user32.dll", EntryPoint = "SetProcessDPIAware")]
	private static extern void SetProcessDPIAware_Internal();
}
