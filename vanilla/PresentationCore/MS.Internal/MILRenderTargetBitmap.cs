using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MS.Internal;

internal static class MILRenderTargetBitmap
{
	[DllImport("wpfgfx_cor3.dll", EntryPoint = "MILRenderTargetBitmapGetBitmap")]
	internal static extern int GetBitmap(SafeMILHandle THIS_PTR, out BitmapSourceSafeMILHandle ppIBitmap);

	[DllImport("wpfgfx_cor3.dll", EntryPoint = "MILRenderTargetBitmapClear")]
	internal static extern int Clear(SafeMILHandle THIS_PTR);
}
