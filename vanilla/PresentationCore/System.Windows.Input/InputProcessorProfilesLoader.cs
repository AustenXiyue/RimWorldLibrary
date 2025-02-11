using MS.Win32;

namespace System.Windows.Input;

internal static class InputProcessorProfilesLoader
{
	internal static MS.Win32.UnsafeNativeMethods.ITfInputProcessorProfiles Load()
	{
		if (MS.Win32.UnsafeNativeMethods.TF_CreateInputProcessorProfiles(out var profiles) == 0)
		{
			return profiles;
		}
		return null;
	}
}
