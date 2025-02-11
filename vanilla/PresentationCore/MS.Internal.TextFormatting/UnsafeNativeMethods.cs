using System.Runtime.InteropServices;

namespace MS.Internal.TextFormatting;

internal static class UnsafeNativeMethods
{
	[DllImport("PresentationNative_cor3.dll")]
	internal static extern LsErr LoCreateContext(ref LsContextInfo contextInfo, ref LscbkRedefined lscbkRedef, out nint ploc);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern LsErr LoDestroyContext(nint ploc);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern LsErr LoCreateLine(nint ploc, int cp, int ccpLim, int durColumn, uint dwLineFlags, nint pInputBreakRec, out LsLInfo plslinfo, out nint pploline, out int maxDepth, out LsLineWidths lineWidths);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern LsErr LoDisposeLine(nint ploline, [MarshalAs(UnmanagedType.Bool)] bool finalizing);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern LsErr LoAcquireBreakRecord(nint ploline, out nint pbreakrec);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern LsErr LoDisposeBreakRecord(nint pBreakRec, [MarshalAs(UnmanagedType.Bool)] bool finalizing);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern LsErr LoCloneBreakRecord(nint pBreakRec, out nint pBreakRecClone);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern LsErr LoRelievePenaltyResource(nint ploline);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern LsErr LoSetBreaking(nint ploc, int strategy);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern LsErr LoSetDoc(nint ploc, int isDisplay, int isReferencePresentationEqual, ref LsDevRes deviceInfo);

	[DllImport("PresentationNative_cor3.dll")]
	internal unsafe static extern LsErr LoSetTabs(nint ploc, int durIncrementalTab, int tabCount, LsTbd* pTabs);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern LsErr LoDisplayLine(nint ploline, ref LSPOINT pt, uint displayMode, ref LSRECT clipRect);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern LsErr LoEnumLine(nint ploline, bool reverseOder, bool fGeometryneeded, ref LSPOINT pt);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern LsErr LoQueryLineCpPpoint(nint ploline, int lscpQuery, int depthQueryMax, nint pSubLineInfo, out int actualDepthQuery, out LsTextCell lsTextCell);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern LsErr LoQueryLinePointPcp(nint ploline, ref LSPOINT ptQuery, int depthQueryMax, nint pSubLineInfo, out int actualDepthQuery, out LsTextCell lsTextCell);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern LsErr LoCreateBreaks(nint ploc, int cpFirst, nint previousBreakRecord, nint ploparabreak, nint ptslinevariantRestriction, ref LsBreaks lsbreaks, out int bestFitIndex);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern LsErr LoCreateParaBreakingSession(nint ploc, int cpParagraphFirst, int maxWidth, nint previousParaBreakRecord, ref nint pploparabreak, [MarshalAs(UnmanagedType.Bool)] ref bool fParagraphJustified);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern LsErr LoDisposeParaBreakingSession(nint ploparabreak, [MarshalAs(UnmanagedType.Bool)] bool finalizing);

	[DllImport("PresentationNative_cor3.dll")]
	internal unsafe static extern LsErr LocbkGetObjectHandlerInfo(nint ploc, uint objectId, void* objectInfo);

	internal static void LoGetEscString(ref EscStringInfo escStringInfo)
	{
		LoGetEscStringImpl(ref escStringInfo);
	}

	[DllImport("PresentationNative_cor3.dll", EntryPoint = "LoGetEscString")]
	private static extern void LoGetEscStringImpl(ref EscStringInfo escStringInfo);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern LsErr LoAcquirePenaltyModule(nint ploc, out nint penaltyModuleHandle);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern LsErr LoDisposePenaltyModule(nint penaltyModuleHandle);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern LsErr LoGetPenaltyModuleInternalHandle(nint penaltyModuleHandle, out nint penaltyModuleInternalHandle);

	[DllImport("PresentationNative_cor3.dll")]
	internal unsafe static extern void* CreateTextAnalysisSink();

	[DllImport("PresentationNative_cor3.dll")]
	internal unsafe static extern void* GetScriptAnalysisList(void* textAnalysisSink);

	[DllImport("PresentationNative_cor3.dll")]
	internal unsafe static extern void* GetNumberSubstitutionList(void* textAnalysisSink);

	[DllImport("PresentationNative_cor3.dll")]
	internal unsafe static extern int CreateTextAnalysisSource(char* text, uint length, char* culture, void* factory, bool isRightToLeft, char* numberCulture, bool ignoreUserOverride, uint numberSubstitutionMethod, void** ppTextAnalysisSource);
}
