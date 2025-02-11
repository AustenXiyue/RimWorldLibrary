using System.Runtime.InteropServices;

namespace MS.Internal;

internal static class MILUpdateSystemParametersInfo
{
	[DllImport("wpfgfx_cor3.dll", EntryPoint = "MILUpdateSystemParametersInfo")]
	internal static extern int Update();
}
