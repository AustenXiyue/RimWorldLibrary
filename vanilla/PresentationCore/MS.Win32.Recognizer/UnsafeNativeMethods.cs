using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Ink;

namespace MS.Win32.Recognizer;

internal static class UnsafeNativeMethods
{
	[DllImport("mshwgst.dll")]
	internal static extern int CreateRecognizer([In] ref Guid clsid, out RecognizerSafeHandle hRec);

	[DllImport("mshwgst.dll")]
	internal static extern int DestroyRecognizer([In] nint hRec);

	[DllImport("mshwgst.dll")]
	internal static extern int CreateContext([In] RecognizerSafeHandle hRec, out ContextSafeHandle hRecContext);

	[DllImport("mshwgst.dll")]
	internal static extern int DestroyContext([In] nint hRecContext);

	[DllImport("mshwgst.dll")]
	internal static extern int AddStroke([In] ContextSafeHandle hRecContext, [In] ref PACKET_DESCRIPTION packetDesc, [In] uint cbPackets, [In] nint pByte, [In][MarshalAs(UnmanagedType.LPStruct)] MS.Win32.NativeMethods.XFORM xForm);

	[DllImport("mshwgst.dll")]
	internal static extern int SetEnabledUnicodeRanges([In] ContextSafeHandle hRecContext, [In] uint cRangs, [In] CHARACTER_RANGE[] charRanges);

	[DllImport("mshwgst.dll")]
	internal static extern int EndInkInput([In] ContextSafeHandle hRecContext);

	[DllImport("mshwgst.dll")]
	internal static extern int Process([In] ContextSafeHandle hRecContext, out bool partialProcessing);

	[DllImport("mshwgst.dll")]
	internal static extern int GetAlternateList([In] ContextSafeHandle hRecContext, [In][Out] ref RECO_RANGE recoRange, [In][Out] ref uint cAlts, [In][Out] nint[] recAtls, [In] ALT_BREAKS breaks);

	[DllImport("mshwgst.dll")]
	internal static extern int DestroyAlternate([In] nint hRecAtls);

	[DllImport("mshwgst.dll", CharSet = CharSet.Unicode)]
	internal static extern int GetString([In] nint hRecAtls, out RECO_RANGE recoRange, [In][Out] ref uint size, [In][Out] StringBuilder recoString);

	[DllImport("mshwgst.dll")]
	internal static extern int GetConfidenceLevel([In] nint hRecAtls, out RECO_RANGE recoRange, out RecognitionConfidence confidenceLevel);

	[DllImport("mshwgst.dll")]
	internal static extern int ResetContext([In] ContextSafeHandle hRecContext);

	[DllImport("mshwgst.dll")]
	internal static extern int GetLatticePtr([In] ContextSafeHandle hRecContext, [In] ref nint pRecoLattice);
}
