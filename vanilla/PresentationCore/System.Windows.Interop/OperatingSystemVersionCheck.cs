namespace System.Windows.Interop;

internal static class OperatingSystemVersionCheck
{
	internal static bool IsVersionOrLater(OperatingSystemVersion version)
	{
		PlatformID platformID = PlatformID.Win32NT;
		int num;
		int num2;
		switch (version)
		{
		case OperatingSystemVersion.Windows8:
			num = 6;
			num2 = 2;
			break;
		case OperatingSystemVersion.Windows7:
			num = 6;
			num2 = 1;
			break;
		case OperatingSystemVersion.WindowsVista:
			num = 6;
			num2 = 0;
			break;
		default:
			num = 5;
			num2 = 1;
			break;
		}
		OperatingSystem oSVersion = Environment.OSVersion;
		if (oSVersion.Platform == platformID)
		{
			if (oSVersion.Version.Major != num || oSVersion.Version.Minor < num2)
			{
				return oSVersion.Version.Major > num;
			}
			return true;
		}
		return false;
	}
}
