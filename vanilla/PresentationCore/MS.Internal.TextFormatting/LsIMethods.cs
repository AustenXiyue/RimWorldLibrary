namespace MS.Internal.TextFormatting;

internal struct LsIMethods
{
	public nint pfnCreateILSObj;

	public nint pfnDestroyILSObj;

	public nint pfnSetDoc;

	public nint pfnCreateLNObj;

	public nint pfnDestroyLNObj;

	public nint pfnFmt;

	public nint pfnFmtResume;

	public nint pfnGetModWidthPrecedingChar;

	public nint pfnGetModWidthFollowingChar;

	public nint pfnTruncate;

	public nint pfnFindPrevBreakOppInside;

	public nint pfnFindNextBreakOppInside;

	public nint pfnFindBreakOppBeforeCpTruncate;

	public nint pfnFindBreakOppAfterCpTruncate;

	public nint pfnCreateStartOppInside;

	public nint pfnProposeBreakAfter;

	public nint pfnProposeBreakBefore;

	public nint pfnCreateBreakOppAfter;

	public nint pfnCreateStartOppBefore;

	public nint pfnCreateDobjFragment;

	public nint pfnForceBreak;

	public nint pfnCreateBreakRecord;

	public nint pfnSetBreak;

	public nint pfnDestroyStartOpp;

	public nint pfnDestroyBreakOpp;

	public nint pfnDuplicateBreakRecord;

	public nint pfnDestroyBreakRecord;

	public nint pfnGetSpecialEffectsFromDobj;

	public nint pfnGetSpecialEffectsFromDobjFragment;

	public nint pfnGetSubmissionInfoFromDobj;

	public nint pfnGetSublinesFromDobj;

	public nint pfnGetSubmissionInfoFromDobjFragment;

	public nint pfnGetSubsFromDobjFragment;

	public nint pfnFExpandWithPrecedingChar;

	public nint pfnFExpandWithFollowingChar;

	public nint pfnCalcPresentation;

	public nint pfnQueryPointPcp;

	public nint pfnQueryCpPpoint;

	public nint pfnEnum;

	public nint pfnDisplay;

	public nint pfnDestroyDobj;

	public nint pfnDestroyDobjFragment;
}
