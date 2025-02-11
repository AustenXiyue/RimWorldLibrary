using System.Runtime.InteropServices;

namespace MS.Win32.PresentationCore;

internal static class SafeNativeMethods
{
	private static class SafeNativeMethodsPrivate
	{
		[DllImport("wpfgfx_cor3.dll")]
		internal static extern int MilCompositionEngine_InitializePartitionManager(int nPriority);

		[DllImport("wpfgfx_cor3.dll")]
		internal static extern int MilCompositionEngine_DeinitializePartitionManager();

		[DllImport("wpfgfx_cor3.dll")]
		internal static extern long GetNextPerfElementId();
	}

	internal static int MilCompositionEngine_InitializePartitionManager(int nPriority)
	{
		return SafeNativeMethodsPrivate.MilCompositionEngine_InitializePartitionManager(nPriority);
	}

	internal static int MilCompositionEngine_DeinitializePartitionManager()
	{
		return SafeNativeMethodsPrivate.MilCompositionEngine_DeinitializePartitionManager();
	}

	internal static long GetNextPerfElementId()
	{
		return SafeNativeMethodsPrivate.GetNextPerfElementId();
	}
}
