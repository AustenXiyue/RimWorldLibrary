using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MS.Internal;

internal static class MILSwDoubleBufferedBitmap
{
	[DllImport("wpfgfx_cor3.dll", EntryPoint = "MILSwDoubleBufferedBitmapCreate")]
	internal static extern int Create(uint width, uint height, double dpiX, double dpiY, ref Guid pixelFormatGuid, SafeMILHandle pPalette, out SafeMILHandle ppSwDoubleBufferedBitmap);

	[DllImport("wpfgfx_cor3.dll", EntryPoint = "MILSwDoubleBufferedBitmapGetBackBuffer", PreserveSig = false)]
	internal static extern void GetBackBuffer(SafeMILHandle THIS_PTR, out BitmapSourceSafeMILHandle pBackBuffer, out uint pBackBufferSize);

	[DllImport("wpfgfx_cor3.dll", EntryPoint = "MILSwDoubleBufferedBitmapAddDirtyRect", PreserveSig = false)]
	internal static extern void AddDirtyRect(SafeMILHandle THIS_PTR, ref Int32Rect dirtyRect);

	[DllImport("wpfgfx_cor3.dll", EntryPoint = "MILSwDoubleBufferedBitmapProtectBackBuffer")]
	internal static extern int ProtectBackBuffer(SafeMILHandle THIS_PTR);
}
