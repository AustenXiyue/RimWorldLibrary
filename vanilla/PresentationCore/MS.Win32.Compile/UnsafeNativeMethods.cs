using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using MS.Internal.PresentationCore;

namespace MS.Win32.Compile;

[FriendAccessAllowed]
internal static class UnsafeNativeMethods
{
	[DllImport("shfolder.dll", BestFitMapping = false, CharSet = CharSet.Auto)]
	public static extern int SHGetFolderPath(nint hwndOwner, int nFolder, nint hToken, int dwFlags, StringBuilder lpszPath);

	[DllImport("urlmon.dll", CharSet = CharSet.Unicode)]
	internal static extern int FindMimeFromData(IBindCtx pBC, string wszUrl, nint Buffer, int cbSize, string wzMimeProposed, int dwMimeFlags, out string wzMimeOut, int dwReserved);
}
