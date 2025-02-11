using System.Runtime.InteropServices;
using System.Windows.Media;

namespace MS.Internal;

internal static class MILMedia
{
	[DllImport("wpfgfx_cor3.dll", EntryPoint = "MILMediaOpen")]
	internal static extern int Open(SafeMediaHandle THIS_PTR, [In][MarshalAs(UnmanagedType.BStr)] string src);

	[DllImport("wpfgfx_cor3.dll", EntryPoint = "MILMediaStop")]
	internal static extern int Stop(SafeMediaHandle THIS_PTR);

	[DllImport("wpfgfx_cor3.dll", EntryPoint = "MILMediaClose")]
	internal static extern int Close(SafeMediaHandle THIS_PTR);

	[DllImport("wpfgfx_cor3.dll", EntryPoint = "MILMediaGetPosition")]
	internal static extern int GetPosition(SafeMediaHandle THIS_PTR, ref long pllTime);

	[DllImport("wpfgfx_cor3.dll", EntryPoint = "MILMediaSetPosition")]
	internal static extern int SetPosition(SafeMediaHandle THIS_PTR, long llTime);

	[DllImport("wpfgfx_cor3.dll", EntryPoint = "MILMediaSetVolume")]
	internal static extern int SetVolume(SafeMediaHandle THIS_PTR, double dblVolume);

	[DllImport("wpfgfx_cor3.dll", EntryPoint = "MILMediaSetBalance")]
	internal static extern int SetBalance(SafeMediaHandle THIS_PTR, double dblBalance);

	[DllImport("wpfgfx_cor3.dll", EntryPoint = "MILMediaSetIsScrubbingEnabled")]
	internal static extern int SetIsScrubbingEnabled(SafeMediaHandle THIS_PTR, bool isScrubbingEnabled);

	[DllImport("wpfgfx_cor3.dll", EntryPoint = "MILMediaIsBuffering")]
	internal static extern int IsBuffering(SafeMediaHandle THIS_PTR, ref bool pIsBuffering);

	[DllImport("wpfgfx_cor3.dll", EntryPoint = "MILMediaCanPause")]
	internal static extern int CanPause(SafeMediaHandle THIS_PTR, ref bool pCanPause);

	[DllImport("wpfgfx_cor3.dll", EntryPoint = "MILMediaGetDownloadProgress")]
	internal static extern int GetDownloadProgress(SafeMediaHandle THIS_PTR, ref double pProgress);

	[DllImport("wpfgfx_cor3.dll", EntryPoint = "MILMediaGetBufferingProgress")]
	internal static extern int GetBufferingProgress(SafeMediaHandle THIS_PTR, ref double pProgress);

	[DllImport("wpfgfx_cor3.dll", EntryPoint = "MILMediaSetRate")]
	internal static extern int SetRate(SafeMediaHandle THIS_PTR, double dblRate);

	[DllImport("wpfgfx_cor3.dll", EntryPoint = "MILMediaHasVideo")]
	internal static extern int HasVideo(SafeMediaHandle THIS_PTR, ref bool pfHasVideo);

	[DllImport("wpfgfx_cor3.dll", EntryPoint = "MILMediaHasAudio")]
	internal static extern int HasAudio(SafeMediaHandle THIS_PTR, ref bool pfHasAudio);

	[DllImport("wpfgfx_cor3.dll", EntryPoint = "MILMediaGetNaturalHeight")]
	internal static extern int GetNaturalHeight(SafeMediaHandle THIS_PTR, ref uint puiHeight);

	[DllImport("wpfgfx_cor3.dll", EntryPoint = "MILMediaGetNaturalWidth")]
	internal static extern int GetNaturalWidth(SafeMediaHandle THIS_PTR, ref uint puiWidth);

	[DllImport("wpfgfx_cor3.dll", EntryPoint = "MILMediaGetMediaLength")]
	internal static extern int GetMediaLength(SafeMediaHandle THIS_PTR, ref long pllLength);

	[DllImport("wpfgfx_cor3.dll", EntryPoint = "MILMediaNeedUIFrameUpdate")]
	internal static extern int NeedUIFrameUpdate(SafeMediaHandle THIS_PTR);

	[DllImport("wpfgfx_cor3.dll", EntryPoint = "MILMediaShutdown")]
	internal static extern int Shutdown(nint THIS_PTR);

	[DllImport("wpfgfx_cor3.dll", EntryPoint = "MILMediaProcessExitHandler")]
	internal static extern int ProcessExitHandler(SafeMediaHandle THIS_PTR);
}
