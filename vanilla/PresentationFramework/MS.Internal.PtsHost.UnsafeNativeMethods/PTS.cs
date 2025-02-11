using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Windows;
using MS.Internal.Text;

namespace MS.Internal.PtsHost.UnsafeNativeMethods;

internal static class PTS
{
	[Serializable]
	private class SecondaryException : Exception
	{
		public override string HelpLink
		{
			get
			{
				return base.InnerException.HelpLink;
			}
			set
			{
				base.InnerException.HelpLink = value;
			}
		}

		public override string Message => base.InnerException.Message;

		public override string Source
		{
			get
			{
				return base.InnerException.Source;
			}
			set
			{
				base.InnerException.Source = value;
			}
		}

		public override string StackTrace => base.InnerException.StackTrace;

		internal SecondaryException(Exception e)
			: base(null, e)
		{
		}

		protected SecondaryException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}

	private class PtsException : Exception
	{
		internal PtsException(string s)
			: base(s)
		{
		}
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	internal struct Restrictions
	{
		internal const int tsduRestriction = 1073741823;

		internal const int tsdvRestriction = 1073741823;

		internal const int tscColumnRestriction = 1000;

		internal const int tscSegmentAreaRestriction = 1000;

		internal const int tscHeightAreaRestriction = 1000;

		internal const int tscTableColumnsRestriction = 1000;

		internal const int tscFootnotesRestriction = 1000;

		internal const int tscAttachedObjectsRestriction = 100000;

		internal const int tscLineInParaRestriction = 1000000;

		internal const int tscVerticesRestriction = 10000;

		internal const int tscPolygonsRestriction = 200;

		internal const int tscSeparatorsRestriction = 1000;

		internal const int tscMatrixColumnsRestriction = 1000;

		internal const int tscMatrixRowsRestriction = 10000;

		internal const int tscEquationsRestriction = 10000;

		internal const int tsduFontParameterRestriction = 50000000;

		internal const int tsdvFontParameterRestriction = 50000000;

		internal const int tscBreakingClassesRestriction = 200;

		internal const int tscBreakingUnitsRestriction = 200;

		internal const int tscModWidthClassesRestriction = 200;

		internal const int tscPairActionsRestriction = 200;

		internal const int tscPriorityActionsRestriction = 200;

		internal const int tscExpansionUnitsRestriction = 200;

		internal const int tscCharacterRestriction = 268435455;

		internal const int tscInstalledHandlersRestriction = 200;

		internal const int tscJustPriorityLimRestriction = 20;
	}

	internal struct FSCBK
	{
		internal FSCBKGEN cbkgen;

		internal FSCBKTXT cbktxt;

		internal FSCBKOBJ cbkobj;

		internal FSCBKFIG cbkfig;

		internal FSCBKWRD cbkwrd;
	}

	internal struct FSCBKFIG
	{
		internal GetFigureProperties pfnGetFigureProperties;

		internal GetFigurePolygons pfnGetFigurePolygons;

		internal CalcFigurePosition pfnCalcFigurePosition;
	}

	internal enum FSKREF
	{
		fskrefPage,
		fskrefMargin,
		fskrefParagraph,
		fskrefChar,
		fskrefOutOfMinMargin,
		fskrefOutOfMaxMargin
	}

	internal enum FSKALIGNFIG
	{
		fskalfMin,
		fskalfCenter,
		fskalfMax
	}

	internal struct FSFIGUREPROPS
	{
		internal FSKREF fskrefU;

		internal FSKREF fskrefV;

		internal FSKALIGNFIG fskalfU;

		internal FSKALIGNFIG fskalfV;

		internal FSKWRAP fskwrap;

		internal int fNonTextPlane;

		internal int fAllowOverlap;

		internal int fDelayable;
	}

	internal struct FSCBKGEN
	{
		internal FSkipPage pfnFSkipPage;

		internal GetPageDimensions pfnGetPageDimensions;

		internal GetNextSection pfnGetNextSection;

		internal GetSectionProperties pfnGetSectionProperties;

		internal GetJustificationProperties pfnGetJustificationProperties;

		internal GetMainTextSegment pfnGetMainTextSegment;

		internal GetHeaderSegment pfnGetHeaderSegment;

		internal GetFooterSegment pfnGetFooterSegment;

		internal UpdGetSegmentChange pfnUpdGetSegmentChange;

		internal GetSectionColumnInfo pfnGetSectionColumnInfo;

		internal GetSegmentDefinedColumnSpanAreaInfo pfnGetSegmentDefinedColumnSpanAreaInfo;

		internal GetHeightDefinedColumnSpanAreaInfo pfnGetHeightDefinedColumnSpanAreaInfo;

		internal GetFirstPara pfnGetFirstPara;

		internal GetNextPara pfnGetNextPara;

		internal UpdGetFirstChangeInSegment pfnUpdGetFirstChangeInSegment;

		internal UpdGetParaChange pfnUpdGetParaChange;

		internal GetParaProperties pfnGetParaProperties;

		internal CreateParaclient pfnCreateParaclient;

		internal TransferDisplayInfo pfnTransferDisplayInfo;

		internal DestroyParaclient pfnDestroyParaclient;

		internal FInterruptFormattingAfterPara pfnFInterruptFormattingAfterPara;

		internal GetEndnoteSeparators pfnGetEndnoteSeparators;

		internal GetEndnoteSegment pfnGetEndnoteSegment;

		internal GetNumberEndnoteColumns pfnGetNumberEndnoteColumns;

		internal GetEndnoteColumnInfo pfnGetEndnoteColumnInfo;

		internal GetFootnoteSeparators pfnGetFootnoteSeparators;

		internal FFootnoteBeneathText pfnFFootnoteBeneathText;

		internal GetNumberFootnoteColumns pfnGetNumberFootnoteColumns;

		internal GetFootnoteColumnInfo pfnGetFootnoteColumnInfo;

		internal GetFootnoteSegment pfnGetFootnoteSegment;

		internal GetFootnotePresentationAndRejectionOrder pfnGetFootnotePresentationAndRejectionOrder;

		internal FAllowFootnoteSeparation pfnFAllowFootnoteSeparation;
	}

	internal struct FSPAP
	{
		internal int idobj;

		internal int fKeepWithNext;

		internal int fBreakPageBefore;

		internal int fBreakColumnBefore;
	}

	internal struct FSCBKOBJ
	{
		internal nint pfnNewPtr;

		internal nint pfnDisposePtr;

		internal nint pfnReallocPtr;

		internal DuplicateMcsclient pfnDuplicateMcsclient;

		internal DestroyMcsclient pfnDestroyMcsclient;

		internal FEqualMcsclient pfnFEqualMcsclient;

		internal ConvertMcsclient pfnConvertMcsclient;

		internal GetObjectHandlerInfo pfnGetObjectHandlerInfo;
	}

	internal struct FSCBKTXT
	{
		internal CreateParaBreakingSession pfnCreateParaBreakingSession;

		internal DestroyParaBreakingSession pfnDestroyParaBreakingSession;

		internal GetTextProperties pfnGetTextProperties;

		internal GetNumberFootnotes pfnGetNumberFootnotes;

		internal GetFootnotes pfnGetFootnotes;

		internal FormatDropCap pfnFormatDropCap;

		internal GetDropCapPolygons pfnGetDropCapPolygons;

		internal DestroyDropCap pfnDestroyDropCap;

		internal FormatBottomText pfnFormatBottomText;

		internal FormatLine pfnFormatLine;

		internal FormatLineForced pfnFormatLineForced;

		internal FormatLineVariants pfnFormatLineVariants;

		internal ReconstructLineVariant pfnReconstructLineVariant;

		internal DestroyLine pfnDestroyLine;

		internal DuplicateLineBreakRecord pfnDuplicateLineBreakRecord;

		internal DestroyLineBreakRecord pfnDestroyLineBreakRecord;

		internal SnapGridVertical pfnSnapGridVertical;

		internal GetDvrSuppressibleBottomSpace pfnGetDvrSuppressibleBottomSpace;

		internal GetDvrAdvance pfnGetDvrAdvance;

		internal UpdGetChangeInText pfnUpdGetChangeInText;

		internal UpdGetDropCapChange pfnUpdGetDropCapChange;

		internal FInterruptFormattingText pfnFInterruptFormattingText;

		internal GetTextParaCache pfnGetTextParaCache;

		internal SetTextParaCache pfnSetTextParaCache;

		internal GetOptimalLineDcpCache pfnGetOptimalLineDcpCache;

		internal GetNumberAttachedObjectsBeforeTextLine pfnGetNumberAttachedObjectsBeforeTextLine;

		internal GetAttachedObjectsBeforeTextLine pfnGetAttachedObjectsBeforeTextLine;

		internal GetNumberAttachedObjectsInTextLine pfnGetNumberAttachedObjectsInTextLine;

		internal GetAttachedObjectsInTextLine pfnGetAttachedObjectsInTextLine;

		internal UpdGetAttachedObjectChange pfnUpdGetAttachedObjectChange;

		internal GetDurFigureAnchor pfnGetDurFigureAnchor;
	}

	internal struct FSLINEVARIANT
	{
		internal nint pfsbreakreclineclient;

		internal nint pfslineclient;

		internal int dcpLine;

		internal int fForceBroken;

		internal FSFLRES fslres;

		internal int dvrAscent;

		internal int dvrDescent;

		internal int fReformatNeighborsAsLastLine;

		internal nint ptsLinePenaltyInfo;
	}

	internal struct FSTXTPROPS
	{
		internal uint fswdir;

		internal int dcpStartContent;

		internal int fKeepTogether;

		internal int fDropCap;

		internal int cMinLinesAfterBreak;

		internal int cMinLinesBeforeBreak;

		internal int fVerticalGrid;

		internal int fOptimizeParagraph;

		internal int fAvoidHyphenationAtTrackBottom;

		internal int fAvoidHyphenationOnLastChainElement;

		internal int cMaxConsecutiveHyphens;
	}

	internal enum FSKFMTLINE
	{
		fskfmtlineNormal,
		fskfmtlineOptimal,
		fskfmtlineForced,
		fskfmtlineWord
	}

	internal struct FSFMTLINEIN
	{
		internal FSKFMTLINE fskfmtline;

		internal nint nmp;

		private int iArea;

		private int dcpStartLine;

		private nint pbrLineIn;

		private int urStartLine;

		private int durLine;

		private int urStartTrack;

		private int durTrack;

		private int urPageLeftMargin;

		private int fAllowHyphenation;

		private int fClearOnleft;

		private int fClearOnRight;

		private int fTreatAsFirstInPara;

		private int fTreatAsLastInPara;

		private int fSuppressTopSpace;

		private int dcpLine;

		private int dvrAvailable;

		private int fChain;

		private int vrStartLine;

		private int urStartLr;

		private int durLr;

		private int fHitByPolygon;

		private int fClearLeftLrWord;

		private int fClearRightLrWord;
	}

	internal struct FSCBKWRD
	{
		internal nint pfnGetSectionHorizMargins;

		internal nint pfnFPerformColumnBalancing;

		internal nint pfnCalculateColumnBalancingApproximateHeight;

		internal nint pfnCalculateColumnBalancingStep;

		internal nint pfnGetColumnSectionBreak;

		internal nint pfnFSuppressKeepWithNextAtTopOfPage;

		internal nint pfnFSuppressKeepTogetherAtTopOfPage;

		internal nint pfnFAllowSpaceAfterOverhang;

		internal nint pfnFormatLineWord;

		internal nint pfnGetSuppressedTopSpace;

		internal nint pfnChangeSplatLineHeight;

		internal nint pfnGetDvrAdvanceWord;

		internal nint pfnGetMinDvrAdvance;

		internal nint pfnGetDurTooNarrowForFigure;

		internal nint pfnResolveOverlap;

		internal nint pfnGetOffsetForFlowAroundAndBBox;

		internal nint pfnGetClientGeometryHandle;

		internal nint pfnDuplicateClientGeometryHandle;

		internal nint pfnDestroyClientGeometryHandle;

		internal nint pfnObstacleAddNotification;

		internal nint pfnGetFigureObstaclesForRestart;

		internal nint pfnRepositionFigure;

		internal nint pfnFStopBeforeLr;

		internal nint pfnFStopBeforeLine;

		internal nint pfnFIgnoreCollision;

		internal nint pfnGetNumberOfLinesForColumnBalancing;

		internal nint pfnFCancelPageBreakBefore;

		internal nint pfnChangeVrTopLineForFigure;

		internal nint pfnFApplyWidowOrphanControlInFootnoteResolution;
	}

	internal struct FSCOLUMNINFO
	{
		internal int durBefore;

		internal int durWidth;
	}

	internal enum FSCOMPRESULT
	{
		fscmprNoChange,
		fscmprChangeInside,
		fscmprShifted
	}

	internal struct FSCONTEXTINFO
	{
		internal uint version;

		internal uint fsffi;

		internal int drMinColumnBalancingStep;

		internal int cInstalledObjects;

		internal nint pInstalledObjects;

		internal nint pfsclient;

		internal nint ptsPenaltyModule;

		internal FSCBK fscbk;

		internal AssertFailed pfnAssertFailed;
	}

	internal struct FSRECT
	{
		internal int u;

		internal int v;

		internal int du;

		internal int dv;

		internal FSRECT(int inU, int inV, int inDU, int inDV)
		{
			u = inU;
			v = inV;
			du = inDU;
			dv = inDV;
		}

		internal FSRECT(FSRECT rect)
		{
			u = rect.u;
			v = rect.v;
			du = rect.du;
			dv = rect.dv;
		}

		internal FSRECT(Rect rect)
		{
			if (!rect.IsEmpty)
			{
				u = TextDpi.ToTextDpi(rect.Left);
				v = TextDpi.ToTextDpi(rect.Top);
				du = TextDpi.ToTextDpi(rect.Width);
				dv = TextDpi.ToTextDpi(rect.Height);
			}
			else
			{
				u = (v = (du = (dv = 0)));
			}
		}

		public static bool operator ==(FSRECT rect1, FSRECT rect2)
		{
			if (rect1.u == rect2.u && rect1.v == rect2.v && rect1.du == rect2.du)
			{
				return rect1.dv == rect2.dv;
			}
			return false;
		}

		public static bool operator !=(FSRECT rect1, FSRECT rect2)
		{
			return !(rect1 == rect2);
		}

		public override bool Equals(object o)
		{
			if (o is FSRECT)
			{
				return (FSRECT)o == this;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return u.GetHashCode() ^ v.GetHashCode() ^ du.GetHashCode() ^ dv.GetHashCode();
		}

		internal Rect FromTextDpi()
		{
			return new Rect(TextDpi.FromTextDpi(u), TextDpi.FromTextDpi(v), TextDpi.FromTextDpi(du), TextDpi.FromTextDpi(dv));
		}

		internal bool Contains(FSPOINT point)
		{
			if (point.u >= u && point.u <= u + du && point.v >= v)
			{
				return point.v <= v + dv;
			}
			return false;
		}
	}

	internal struct FSPOINT
	{
		internal int u;

		internal int v;

		internal FSPOINT(int inU, int inV)
		{
			u = inU;
			v = inV;
		}

		public static bool operator ==(FSPOINT point1, FSPOINT point2)
		{
			if (point1.u == point2.u)
			{
				return point1.v == point2.v;
			}
			return false;
		}

		public static bool operator !=(FSPOINT point1, FSPOINT point2)
		{
			return !(point1 == point2);
		}

		public override bool Equals(object o)
		{
			if (o is FSPOINT)
			{
				return (FSPOINT)o == this;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return u.GetHashCode() ^ v.GetHashCode();
		}
	}

	internal struct FSVECTOR
	{
		internal int du;

		internal int dv;

		internal FSVECTOR(int inDU, int inDV)
		{
			du = inDU;
			dv = inDV;
		}

		public static bool operator ==(FSVECTOR vector1, FSVECTOR vector2)
		{
			if (vector1.du == vector2.du)
			{
				return vector1.dv == vector2.dv;
			}
			return false;
		}

		public static bool operator !=(FSVECTOR vector1, FSVECTOR vector2)
		{
			return !(vector1 == vector2);
		}

		public override bool Equals(object o)
		{
			if (o is FSVECTOR)
			{
				return (FSVECTOR)o == this;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return du.GetHashCode() ^ dv.GetHashCode();
		}

		internal Vector FromTextDpi()
		{
			return new Vector(TextDpi.FromTextDpi(du), TextDpi.FromTextDpi(dv));
		}
	}

	internal struct FSBBOX
	{
		internal int fDefined;

		internal FSRECT fsrc;
	}

	internal struct FSFIGOBSTINFO
	{
		internal nint nmpFigure;

		internal FSFLOWAROUND flow;

		internal FSPOLYGONINFO polyginfo;

		internal FSOVERLAP overlap;

		internal FSBBOX fsbbox;

		internal FSPOINT fsptPosPreliminary;

		internal int fNonTextPlane;

		internal int fUTextRelative;

		internal int fVTextRelative;
	}

	internal struct FSFIGOBSTRESTARTINFO
	{
		internal nint nmpFigure;

		internal int fReached;

		internal int fNonTextPlane;
	}

	internal struct FSFLOATERCBK
	{
		internal GetFloaterProperties pfnGetFloaterProperties;

		internal FormatFloaterContentFinite pfnFormatFloaterContentFinite;

		internal FormatFloaterContentBottomless pfnFormatFloaterContentBottomless;

		internal UpdateBottomlessFloaterContent pfnUpdateBottomlessFloaterContent;

		internal GetFloaterPolygons pfnGetFloaterPolygons;

		internal ClearUpdateInfoInFloaterContent pfnClearUpdateInfoInFloaterContent;

		internal CompareFloaterContents pfnCompareFloaterContents;

		internal DestroyFloaterContent pfnDestroyFloaterContent;

		internal DuplicateFloaterContentBreakRecord pfnDuplicateFloaterContentBreakRecord;

		internal DestroyFloaterContentBreakRecord pfnDestroyFloaterContentBreakRecord;

		internal GetFloaterContentColumnBalancingInfo pfnGetFloaterContentColumnBalancingInfo;

		internal GetFloaterContentNumberFootnotes pfnGetFloaterContentNumberFootnotes;

		internal GetFloaterContentFootnoteInfo pfnGetFloaterContentFootnoteInfo;

		internal TransferDisplayInfoInFloaterContent pfnTransferDisplayInfoInFloaterContent;

		internal GetMCSClientAfterFloater pfnGetMCSClientAfterFloater;

		internal GetDvrUsedForFloater pfnGetDvrUsedForFloater;
	}

	internal enum FSKFLOATALIGNMENT
	{
		fskfloatalignMin,
		fskfloatalignCenter,
		fskfloatalignMax
	}

	internal struct FSFLOATERPROPS
	{
		internal FSKCLEAR fskclear;

		internal FSKFLOATALIGNMENT fskfloatalignment;

		internal int fFloat;

		internal FSKWRAP fskwr;

		internal int fDelayNoProgress;

		internal int durDistTextLeft;

		internal int durDistTextRight;

		internal int dvrDistTextTop;

		internal int dvrDistTextBottom;
	}

	internal struct FSFLOATERINIT
	{
		internal FSFLOATERCBK fsfloatercbk;
	}

	internal struct FSFLOATERDETAILS
	{
		internal FSKUPDATE fskupdContent;

		internal nint fsnmFloater;

		internal FSRECT fsrcFloater;

		internal nint pfsFloaterContent;
	}

	internal enum FSFLRES
	{
		fsflrOutOfSpace,
		fsflrOutOfSpaceHyphenated,
		fsflrEndOfParagraph,
		fsflrEndOfParagraphClearLeft,
		fsflrEndOfParagraphClearRight,
		fsflrEndOfParagraphClearBoth,
		fsflrPageBreak,
		fsflrColumnBreak,
		fsflrSoftBreak,
		fsflrSoftBreakClearLeft,
		fsflrSoftBreakClearRight,
		fsflrSoftBreakClearBoth,
		fsflrNoProgressClear
	}

	internal struct FSFLTOBSTINFO
	{
		internal FSFLOWAROUND flow;

		internal FSPOLYGONINFO polyginfo;

		internal int fSuppressAutoClear;
	}

	internal enum FSFMTRKSTOP
	{
		fmtrGoalReached,
		fmtrBrokenOutOfSpace,
		fmtrBrokenPageBreak,
		fmtrBrokenColumnBreak,
		fmtrBrokenPageBreakBeforePara,
		fmtrBrokenColumnBreakBeforePara,
		fmtrBrokenPageBreakBeforeSection,
		fmtrBrokenDelayable,
		fmtrNoProgressOutOfSpace,
		fmtrNoProgressPageBreak,
		fmtrNoProgressPageBreakBeforePara,
		fmtrNoProgressColumnBreakBeforePara,
		fmtrNoProgressPageBreakBeforeSection,
		fmtrNoProgressPageSkipped,
		fmtrNoProgressDelayable,
		fmtrCollision
	}

	internal struct FSFMTR
	{
		internal FSFMTRKSTOP kstop;

		internal int fContainsItemThatStoppedBeforeFootnote;

		internal int fForcedProgress;
	}

	internal enum FSFMTRBL
	{
		fmtrblGoalReached,
		fmtrblCollision,
		fmtrblInterrupted
	}

	internal struct FSFTNINFO
	{
		internal nint nmftn;

		internal int vrAccept;

		internal int vrReject;
	}

	internal struct FSINTERVAL
	{
		internal int ur;

		internal int dur;
	}

	internal struct FSFILLINFO
	{
		internal FSRECT fsrc;

		internal int fLastInPara;
	}

	internal struct FSEMPTYSPACE
	{
		internal int ur;

		internal int dur;

		internal int fPolygonInside;
	}

	internal enum FSHYPHENQUALITY
	{
		fshqExcellent,
		fshqGood,
		fshqFair,
		fshqPoor,
		fshqBad
	}

	internal struct FSIMETHODS
	{
		internal ObjCreateContext pfnCreateContext;

		internal ObjDestroyContext pfnDestroyContext;

		internal ObjFormatParaFinite pfnFormatParaFinite;

		internal ObjFormatParaBottomless pfnFormatParaBottomless;

		internal ObjUpdateBottomlessPara pfnUpdateBottomlessPara;

		internal ObjSynchronizeBottomlessPara pfnSynchronizeBottomlessPara;

		internal ObjComparePara pfnComparePara;

		internal ObjClearUpdateInfoInPara pfnClearUpdateInfoInPara;

		internal ObjDestroyPara pfnDestroyPara;

		internal ObjDuplicateBreakRecord pfnDuplicateBreakRecord;

		internal ObjDestroyBreakRecord pfnDestroyBreakRecord;

		internal ObjGetColumnBalancingInfo pfnGetColumnBalancingInfo;

		internal ObjGetNumberFootnotes pfnGetNumberFootnotes;

		internal ObjGetFootnoteInfo pfnGetFootnoteInfo;

		internal nint pfnGetFootnoteInfoWord;

		internal ObjShiftVertical pfnShiftVertical;

		internal ObjTransferDisplayInfoPara pfnTransferDisplayInfoPara;
	}

	internal enum FSKALIGNPAGE
	{
		fskalpgTop,
		fskalpgCenter,
		fskalpgBottom
	}

	internal enum FSKCHANGE
	{
		fskchNone,
		fskchNew,
		fskchInside
	}

	internal enum FSKCLEAR
	{
		fskclearNone,
		fskclearLeft,
		fskclearRight,
		fskclearBoth
	}

	internal enum FSKWRAP
	{
		fskwrNone,
		fskwrLeft,
		fskwrRight,
		fskwrBoth,
		fskwrLargest
	}

	internal enum FSKSUPPRESSHARDBREAKBEFOREFIRSTPARA
	{
		fsksuppresshardbreakbeforefirstparaNone,
		fsksuppresshardbreakbeforefirstparaColumn,
		fsksuppresshardbreakbeforefirstparaPageAndColumn
	}

	internal struct FSFLOWAROUND
	{
		internal FSRECT fsrcBounding;

		internal FSKWRAP fskwr;

		internal int durTooNarrow;

		internal int durDistTextLeft;

		internal int durDistTextRight;

		internal int dvrDistTextTop;

		internal int dvrDistTextBottom;
	}

	internal struct FSPOLYGONINFO
	{
		internal int cPolygons;

		internal unsafe int* rgcVertices;

		internal int cfspt;

		internal unsafe FSPOINT* rgfspt;

		internal int fWrapThrough;
	}

	internal struct FSOVERLAP
	{
		internal FSRECT fsrc;

		internal int fAllowOverlap;
	}

	internal struct FSFIGUREDETAILS
	{
		internal FSRECT fsrcFlowAround;

		internal FSBBOX fsbbox;

		internal FSPOINT fsptPosPreliminary;

		internal int fDelayed;
	}

	internal struct FSLINEELEMENT
	{
		internal nint pfslineclient;

		internal int dcpFirst;

		internal nint pfsbreakreclineclient;

		internal int dcpLim;

		internal int urStart;

		internal int dur;

		internal int fAllowHyphenation;

		internal int urBBox;

		internal int durBBox;

		internal int urLrWord;

		internal int durLrWord;

		internal int dvrAscent;

		internal int dvrDescent;

		internal int fClearOnLeft;

		internal int fClearOnRight;

		internal int fHitByPolygon;

		internal int fForceBroken;

		internal int fClearLeftLrWord;

		internal int fClearRightLrWord;
	}

	internal struct FSLINEDESCRIPTIONCOMPOSITE
	{
		internal nint pline;

		internal int cElements;

		internal int vrStart;

		internal int dvrAscent;

		internal int dvrDescent;

		internal int fTreatedAsFirst;

		internal int fTreatedAsLast;

		internal int dvrAvailableForcedLine;

		internal int fUsedWordFormatLineInChain;

		internal int fFirstLineInWordLr;
	}

	internal struct FSLINEDESCRIPTIONSINGLE
	{
		internal nint pfslineclient;

		internal nint pfsbreakreclineclient;

		internal int dcpFirst;

		internal int dcpLim;

		internal int urStart;

		internal int dur;

		internal int fAllowHyphenation;

		internal int urBBox;

		internal int durBBox;

		internal int vrStart;

		internal int dvrAscent;

		internal int dvrDescent;

		internal int fClearOnLeft;

		internal int fClearOnRight;

		internal int fTreatedAsFirst;

		internal int fForceBroken;
	}

	internal struct FSATTACHEDOBJECTDESCRIPTION
	{
		internal FSUPDATEINFO fsupdinf;

		internal nint pfspara;

		internal nint pfsparaclient;

		internal nint nmp;

		internal int idobj;

		internal int vrStart;

		internal int dvrUsed;

		internal FSBBOX fsbbox;

		internal int dvrTopSpace;
	}

	internal struct FSDROPCAPDETAILS
	{
		internal FSRECT fsrcDropCap;

		internal int fSuppressDropCapTopSpacing;

		internal nint pdcclient;
	}

	internal enum FSKTEXTLINES
	{
		fsklinesNormal,
		fsklinesOptimal,
		fsklinesForced,
		fsklinesWord
	}

	internal struct FSTEXTDETAILSFULL
	{
		internal uint fswdir;

		internal FSKTEXTLINES fsklines;

		internal int fLinesComposite;

		internal int cLines;

		internal int cAttachedObjects;

		internal int dcpFirst;

		internal int dcpLim;

		internal int fDropCapPresent;

		internal FSUPDATEINFO fsupdinfDropCap;

		internal FSDROPCAPDETAILS dcdetails;

		internal int fSuppressTopLineSpacing;

		internal int fUpdateInfoForLinesPresent;

		internal int cLinesBeforeChange;

		internal int dvrShiftBeforeChange;

		internal int cLinesChanged;

		internal int dcLinesChanged;

		internal int dvrShiftAfterChange;

		internal int ddcpAfterChange;
	}

	internal struct FSTEXTDETAILSCACHED
	{
		internal uint fswdir;

		internal FSKTEXTLINES fsklines;

		internal FSRECT fsrcPara;

		internal int fSuppressTopLineSpacing;

		internal int dcpFirst;

		internal int dcpLim;

		internal int cLines;

		internal int fClearOnLeft;

		internal int fClearOnRight;

		internal int fOptimalLineDcpsCached;
	}

	internal enum FSKTEXTDETAILS
	{
		fsktdCached,
		fsktdFull
	}

	internal struct FSTEXTDETAILS
	{
		[StructLayout(LayoutKind.Explicit)]
		internal struct nested_u
		{
			[FieldOffset(0)]
			internal FSTEXTDETAILSFULL full;

			[FieldOffset(0)]
			internal FSTEXTDETAILSCACHED cached;
		}

		internal FSKTEXTDETAILS fsktd;

		internal nested_u u;
	}

	internal struct FSPARADESCRIPTION
	{
		internal FSUPDATEINFO fsupdinf;

		internal nint pfspara;

		internal nint pfsparaclient;

		internal nint nmp;

		internal int idobj;

		internal int dvrUsed;

		internal FSBBOX fsbbox;

		internal int dvrTopSpace;
	}

	internal struct FSTRACKDETAILS
	{
		internal int cParas;
	}

	internal struct FSTRACKDESCRIPTION
	{
		internal FSUPDATEINFO fsupdinf;

		internal nint nms;

		internal FSRECT fsrc;

		internal FSBBOX fsbbox;

		internal int fTrackRelativeToRect;

		internal nint pfstrack;
	}

	internal struct FSSUBTRACKDETAILS
	{
		internal FSUPDATEINFO fsupdinf;

		internal nint nms;

		internal FSRECT fsrc;

		internal int cParas;
	}

	internal struct FSSUBPAGEDETAILSCOMPLEX
	{
		internal nint nms;

		internal uint fswdir;

		internal FSRECT fsrc;

		internal FSBBOX fsbbox;

		internal int cBasicColumns;

		internal int cSegmentDefinedColumnSpanAreas;

		internal int cHeightDefinedColumnSpanAreas;
	}

	internal struct FSSUBPAGEDETAILSSIMPLE
	{
		internal uint fswdir;

		internal FSTRACKDESCRIPTION trackdescr;
	}

	internal struct FSSUBPAGEDETAILS
	{
		[StructLayout(LayoutKind.Explicit)]
		internal struct nested_u
		{
			[FieldOffset(0)]
			internal FSSUBPAGEDETAILSSIMPLE simple;

			[FieldOffset(0)]
			internal FSSUBPAGEDETAILSCOMPLEX complex;
		}

		internal int fSimple;

		internal nested_u u;
	}

	internal struct FSCOMPOSITECOLUMNDETAILS
	{
		internal FSTRACKDESCRIPTION trackdescrMainText;

		internal FSTRACKDESCRIPTION trackdescrFootnoteSeparator;

		internal FSTRACKDESCRIPTION trackdescrFootnoteContinuationSeparator;

		internal FSTRACKDESCRIPTION trackdescrFootnoteContinuationNotice;

		internal FSTRACKDESCRIPTION trackdescrEndnoteSeparator;

		internal FSTRACKDESCRIPTION trackdescrEndnoteContinuationSeparator;

		internal FSTRACKDESCRIPTION trackdescrEndnoteContinuationNotice;

		internal int cFootnotes;

		internal FSRECT fsrcFootnotes;

		internal FSBBOX fsbboxFootnotes;

		internal FSTRACKDESCRIPTION trackdescrEndnote;
	}

	internal struct FSENDNOTECOLUMNDETAILS
	{
		internal FSTRACKDESCRIPTION trackdescrEndnoteContinuationSeparator;

		internal FSTRACKDESCRIPTION trackdescrEndnoteContinuationNotice;

		internal FSTRACKDESCRIPTION trackdescrEndnote;
	}

	internal struct FSCOMPOSITECOLUMNDESCRIPTION
	{
		internal FSUPDATEINFO fsupdinf;

		internal FSRECT fsrc;

		internal FSBBOX fsbbox;

		internal nint pfscompcol;
	}

	internal struct FSENDNOTECOLUMNDESCRIPTION
	{
		internal FSUPDATEINFO fsupdinf;

		internal FSRECT fsrc;

		internal FSBBOX fsbbox;

		internal nint pfsendnotecol;
	}

	internal struct FSSECTIONDETAILSWITHPAGENOTES
	{
		internal uint fswdir;

		internal int fColumnBalancingApplied;

		internal FSRECT fsrcSectionBody;

		internal FSBBOX fsbboxSectionBody;

		internal int cBasicColumns;

		internal int cSegmentDefinedColumnSpanAreas;

		internal int cHeightDefinedColumnSpanAreas;

		internal FSRECT fsrcEndnote;

		internal FSBBOX fsbboxEndnote;

		internal int cEndnoteColumns;

		internal FSTRACKDESCRIPTION trackdescrEndnoteSeparator;
	}

	internal struct FSSECTIONDETAILSWITHCOLNOTES
	{
		internal uint fswdir;

		internal int fColumnBalancingApplied;

		internal int cCompositeColumns;
	}

	internal struct FSSECTIONDETAILS
	{
		[StructLayout(LayoutKind.Explicit)]
		internal struct nested_u
		{
			[FieldOffset(0)]
			internal FSSECTIONDETAILSWITHPAGENOTES withpagenotes;

			[FieldOffset(0)]
			internal FSSECTIONDETAILSWITHCOLNOTES withcolumnnotes;
		}

		internal int fFootnotesAsPagenotes;

		internal nested_u u;
	}

	internal struct FSSECTIONDESCRIPTION
	{
		internal FSUPDATEINFO fsupdinf;

		internal nint nms;

		internal FSRECT fsrc;

		internal FSBBOX fsbbox;

		internal int fOtherSectionInside;

		internal int dvrUsedTop;

		internal int dvrUsedBottom;

		internal nint pfssection;
	}

	internal struct FSFOOTNOTECOLUMNDETAILS
	{
		internal FSTRACKDESCRIPTION trackdescrFootnoteContinuationSeparator;

		internal FSTRACKDESCRIPTION trackdescrFootnoteContinuationNotice;

		internal int cTracks;
	}

	internal struct FSFOOTNOTECOLUMNDESCRIPTION
	{
		internal FSUPDATEINFO fsupdinf;

		internal FSRECT fsrc;

		internal FSBBOX fsbbox;

		internal nint pfsfootnotecol;
	}

	internal struct FSPAGEDETAILSCOMPLEX
	{
		internal int fTopBottomHeaderFooter;

		internal uint fswdirHeader;

		internal FSTRACKDESCRIPTION trackdescrHeader;

		internal uint fswdirFooter;

		internal FSTRACKDESCRIPTION trackdescrFooter;

		internal int fJustified;

		internal FSKALIGNPAGE fskalpg;

		internal uint fswdirPageProper;

		internal FSUPDATEINFO fsupdinfPageBody;

		internal FSRECT fsrcPageBody;

		internal FSRECT fsrcPageMarginActual;

		internal FSBBOX fsbboxPageBody;

		internal int cSections;

		internal FSRECT fsrcFootnote;

		internal FSBBOX fsbboxFootnote;

		internal int cFootnoteColumns;

		internal FSTRACKDESCRIPTION trackdescrFootnoteSeparator;
	}

	internal struct FSPAGEDETAILSSIMPLE
	{
		internal FSTRACKDESCRIPTION trackdescr;
	}

	internal struct FSPAGEDETAILS
	{
		[StructLayout(LayoutKind.Explicit)]
		internal struct nested_u
		{
			[FieldOffset(0)]
			internal FSPAGEDETAILSSIMPLE simple;

			[FieldOffset(0)]
			internal FSPAGEDETAILSCOMPLEX complex;
		}

		internal FSKUPDATE fskupd;

		internal int fSimple;

		internal nested_u u;
	}

	internal struct FSTABLECBKFETCH
	{
		internal GetFirstHeaderRow pfnGetFirstHeaderRow;

		internal GetNextHeaderRow pfnGetNextHeaderRow;

		internal GetFirstFooterRow pfnGetFirstFooterRow;

		internal GetNextFooterRow pfnGetNextFooterRow;

		internal GetFirstRow pfnGetFirstRow;

		internal GetNextRow pfnGetNextRow;

		internal UpdFChangeInHeaderFooter pfnUpdFChangeInHeaderFooter;

		internal UpdGetFirstChangeInTable pfnUpdGetFirstChangeInTable;

		internal UpdGetRowChange pfnUpdGetRowChange;

		internal UpdGetCellChange pfnUpdGetCellChange;

		internal GetDistributionKind pfnGetDistributionKind;

		internal GetRowProperties pfnGetRowProperties;

		internal GetCells pfnGetCells;

		internal FInterruptFormattingTable pfnFInterruptFormattingTable;

		internal CalcHorizontalBBoxOfRow pfnCalcHorizontalBBoxOfRow;
	}

	internal struct FSTABLECBKCELL
	{
		internal FormatCellFinite pfnFormatCellFinite;

		internal FormatCellBottomless pfnFormatCellBottomless;

		internal UpdateBottomlessCell pfnUpdateBottomlessCell;

		internal CompareCells pfnCompareCells;

		internal ClearUpdateInfoInCell pfnClearUpdateInfoInCell;

		internal SetCellHeight pfnSetCellHeight;

		internal DestroyCell pfnDestroyCell;

		internal DuplicateCellBreakRecord pfnDuplicateCellBreakRecord;

		internal DestroyCellBreakRecord pfnDestroyCellBreakRecord;

		internal GetCellNumberFootnotes pfnGetCellNumberFootnotes;

		internal nint pfnGetCellFootnoteInfo;

		internal nint pfnGetCellFootnoteInfoWord;

		internal GetCellMinColumnBalancingStep pfnGetCellMinColumnBalancingStep;

		internal TransferDisplayInfoCell pfnTransferDisplayInfoCell;
	}

	internal enum FSKTABLEHEIGHTDISTRIBUTION
	{
		fskdistributeUnchanged,
		fskdistributeEqually,
		fskdistributeProportionally
	}

	internal enum FSKROWHEIGHTRESTRICTION
	{
		fskrowheightNatural,
		fskrowheightAtLeast,
		fskrowheightAtMostNoBreak,
		fskrowheightExactNoBreak
	}

	internal enum FSKROWBREAKRESTRICTION
	{
		fskrowbreakAnywhere,
		fskrowbreakNoBreakInside,
		fskrowbreakNoBreakInsideAfter
	}

	internal struct FSTABLEROWPROPS
	{
		internal FSKROWBREAKRESTRICTION fskrowbreak;

		internal FSKROWHEIGHTRESTRICTION fskrowheight;

		internal int dvrRowHeightRestriction;

		internal int fBBoxOverflowsBottom;

		internal int dvrAboveRow;

		internal int dvrBelowRow;

		internal int dvrAboveTopRow;

		internal int dvrBelowBottomRow;

		internal int dvrAboveRowBreak;

		internal int dvrBelowRowBreak;

		internal int cCells;
	}

	internal enum FSTABLEKCELLMERGE
	{
		fskcellmergeNo,
		fskcellmergeFirst,
		fskcellmergeMiddle,
		fskcellmergeLast
	}

	internal enum FSKTABLEOBJALIGNMENT
	{
		fsktableobjAlignLeft,
		fsktableobjAlignRight,
		fsktableobjAlignCenter
	}

	internal struct FSTABLEOBJPROPS
	{
		internal FSKCLEAR fskclear;

		internal FSKTABLEOBJALIGNMENT ktablealignment;

		internal int fFloat;

		internal FSKWRAP fskwr;

		internal int fDelayNoProgress;

		internal int dvrCaptionTop;

		internal int dvrCaptionBottom;

		internal int durCaptionLeft;

		internal int durCaptionRight;

		internal uint fswdirTable;
	}

	internal struct FSTABLEOBJCBK
	{
		internal GetTableProperties pfnGetTableProperties;

		internal AutofitTable pfnAutofitTable;

		internal UpdAutofitTable pfnUpdAutofitTable;

		internal GetMCSClientAfterTable pfnGetMCSClientAfterTable;

		internal nint pfnGetDvrUsedForFloatTable;
	}

	internal struct FSTABLECBKFETCHWORD
	{
		internal nint pfnGetTablePropertiesWord;

		internal nint pfnGetRowPropertiesWord;

		internal nint pfnGetRowWidthWord;

		internal nint pfnGetNumberFiguresForTableRow;

		internal nint pfnGetFiguresForTableRow;

		internal nint pfnFStopBeforeTableRowLr;

		internal nint pfnFIgnoreCollisionForTableRow;

		internal nint pfnChangeRowHeightRestriction;

		internal nint pfnNotifyRowPosition;

		internal nint pfnNotifyRowBorderAbove;

		internal nint pfnNotifyTableBreakRec;
	}

	internal struct FSTABLEOBJINIT
	{
		internal FSTABLEOBJCBK tableobjcbk;

		internal FSTABLECBKFETCH tablecbkfetch;

		internal FSTABLECBKCELL tablecbkcell;

		internal FSTABLECBKFETCHWORD tablecbkfetchword;
	}

	internal struct FSTABLEOBJDETAILS
	{
		internal nint fsnmTable;

		internal FSRECT fsrcTableObj;

		internal int dvrTopCaption;

		internal int dvrBottomCaption;

		internal int durLeftCaption;

		internal int durRightCaption;

		internal uint fswdirTable;

		internal FSKUPDATE fskupdTableProper;

		internal nint pfstableProper;
	}

	internal struct FSTABLEDETAILS
	{
		internal int dvrTable;

		internal int cRows;
	}

	internal struct FSTABLEROWDESCRIPTION
	{
		[StructLayout(LayoutKind.Explicit)]
		internal struct nested_u
		{
			[FieldOffset(0)]
			internal FSRECT fsrcRow;

			[FieldOffset(0)]
			internal int dvrRow;
		}

		internal FSUPDATEINFO fsupdinf;

		internal nint fsnmRow;

		internal nint pfstablerow;

		internal int fRowInSeparateRect;

		internal nested_u u;
	}

	internal enum FSKTABLEROWBOUNDARY
	{
		fsktablerowboundaryOuter,
		fsktablerowboundaryInner,
		fsktablerowboundaryBreak
	}

	internal struct FSTABLEROWDETAILS
	{
		internal FSKTABLEROWBOUNDARY fskboundaryAbove;

		internal int dvrAbove;

		internal FSKTABLEROWBOUNDARY fskboundaryBelow;

		internal int dvrBelow;

		internal int cCells;

		internal int fForcedRow;
	}

	internal struct FSTABLESRVCONTEXT
	{
		internal nint pfscontext;

		internal nint pfsclient;

		internal FSCBKOBJ cbkobj;

		internal FSTABLECBKFETCH tablecbkfetch;

		internal FSTABLECBKCELL tablecbkcell;

		internal uint fsffi;
	}

	internal enum FSKUPDATE
	{
		fskupdInherited,
		fskupdNoChange,
		fskupdNew,
		fskupdChangeInside,
		fskupdShifted
	}

	internal struct FSUPDATEINFO
	{
		public FSKUPDATE fskupd;

		public int dvrShifted;
	}

	internal delegate void AssertFailed(string arg1, string arg2, int arg3, uint arg4);

	internal delegate int GetFigureProperties(nint pfsclient, nint pfsparaclientFigure, nint nmpFigure, int fInTextLine, uint fswdir, int fBottomUndefined, out int dur, out int dvr, out FSFIGUREPROPS fsfigprops, out int cPolygons, out int cVertices, out int durDistTextLeft, out int durDistTextRight, out int dvrDistTextTop, out int dvrDistTextBottom);

	internal unsafe delegate int GetFigurePolygons(nint pfsclient, nint pfsparaclientFigure, nint nmpFigure, uint fswdir, int ncVertices, int nfspt, int* rgcVertices, out int ccVertices, FSPOINT* rgfspt, out int cfspt, out int fWrapThrough);

	internal delegate int CalcFigurePosition(nint pfsclient, nint pfsparaclientFigure, nint nmpFigure, uint fswdir, ref FSRECT fsrcPage, ref FSRECT fsrcMargin, ref FSRECT fsrcTrack, ref FSRECT fsrcFigurePreliminary, int fMustPosition, int fInTextLine, out int fPushToNextTrack, out FSRECT fsrcFlow, out FSRECT fsrcOverlap, out FSBBOX fsbbox, out FSRECT fsrcSearch);

	internal delegate int FSkipPage(nint pfsclient, nint nms, out int fSkip);

	internal delegate int GetPageDimensions(nint pfsclient, nint nms, out uint fswdir, out int fHeaderFooterAtTopBottom, out int durPage, out int dvrPage, ref FSRECT fsrcMargin);

	internal delegate int GetNextSection(nint pfsclient, nint nmsCur, out int fSuccess, out nint nmsNext);

	internal delegate int GetSectionProperties(nint pfsclient, nint nms, out int fNewPage, out uint fswdir, out int fApplyColumnBalancing, out int ccol, out int cSegmentDefinedColumnSpanAreas, out int cHeightDefinedColumnSpanAreas);

	internal unsafe delegate int GetJustificationProperties(nint pfsclient, nint* rgnms, int cnms, int fLastSectionNotBroken, out int fJustify, out FSKALIGNPAGE fskal, out int fCancelAtLastColumn);

	internal delegate int GetMainTextSegment(nint pfsclient, nint nmsSection, out nint nmSegment);

	internal delegate int GetHeaderSegment(nint pfsclient, nint nms, nint pfsbrpagePrelim, uint fswdir, out int fHeaderPresent, out int fHardMargin, out int dvrMaxHeight, out int dvrFromEdge, out uint fswdirHeader, out nint nmsHeader);

	internal delegate int GetFooterSegment(nint pfsclient, nint nms, nint pfsbrpagePrelim, uint fswdir, out int fFooterPresent, out int fHardMargin, out int dvrMaxHeight, out int dvrFromEdge, out uint fswdirFooter, out nint nmsFooter);

	internal delegate int UpdGetSegmentChange(nint pfsclient, nint nms, out FSKCHANGE fskch);

	internal unsafe delegate int GetSectionColumnInfo(nint pfsclient, nint nms, uint fswdir, int ncol, FSCOLUMNINFO* fscolinfo, out int ccol);

	internal unsafe delegate int GetSegmentDefinedColumnSpanAreaInfo(nint pfsclient, nint nms, int cAreas, nint* rgnmSeg, int* rgcColumns, out int cAreasActual);

	internal unsafe delegate int GetHeightDefinedColumnSpanAreaInfo(nint pfsclient, nint nms, int cAreas, int* rgdvrAreaHeight, int* rgcColumns, out int cAreasActual);

	internal delegate int GetFirstPara(nint pfsclient, nint nms, out int fSuccessful, out nint nmp);

	internal delegate int GetNextPara(nint pfsclient, nint nms, nint nmpCur, out int fFound, out nint nmpNext);

	internal delegate int UpdGetFirstChangeInSegment(nint pfsclient, nint nms, out int fFound, out int fChangeFirst, out nint nmpBeforeChange);

	internal delegate int UpdGetParaChange(nint pfsclient, nint nmp, out FSKCHANGE fskch, out int fNoFurtherChanges);

	internal delegate int GetParaProperties(nint pfsclient, nint nmp, ref FSPAP fspap);

	internal delegate int CreateParaclient(nint pfsclient, nint nmp, out nint pfsparaclient);

	internal delegate int TransferDisplayInfo(nint pfsclient, nint pfsparaclientOld, nint pfsparaclientNew);

	internal delegate int DestroyParaclient(nint pfsclient, nint pfsparaclient);

	internal delegate int FInterruptFormattingAfterPara(nint pfsclient, nint pfsparaclient, nint nmp, int vr, out int fInterruptFormatting);

	internal delegate int GetEndnoteSeparators(nint pfsclient, nint nmsSection, out nint nmsEndnoteSeparator, out nint nmEndnoteContSeparator, out nint nmsEndnoteContNotice);

	internal delegate int GetEndnoteSegment(nint pfsclient, nint nmsSection, out int fEndnotesPresent, out nint nmsEndnotes);

	internal delegate int GetNumberEndnoteColumns(nint pfsclient, nint nms, out int ccolEndnote);

	internal unsafe delegate int GetEndnoteColumnInfo(nint pfsclient, nint nms, uint fswdir, int ncolEndnote, FSCOLUMNINFO* fscolinfoEndnote, out int ccolEndnote);

	internal delegate int GetFootnoteSeparators(nint pfsclient, nint nmsSection, out nint nmsFtnSeparator, out nint nmsFtnContSeparator, out nint nmsFtnContNotice);

	internal delegate int FFootnoteBeneathText(nint pfsclient, nint nms, out int fFootnoteBeneathText);

	internal delegate int GetNumberFootnoteColumns(nint pfsclient, nint nms, out int ccolFootnote);

	internal unsafe delegate int GetFootnoteColumnInfo(nint pfsclient, nint nms, uint fswdir, int ncolFootnote, FSCOLUMNINFO* fscolinfoFootnote, out int ccolFootnote);

	internal delegate int GetFootnoteSegment(nint pfsclient, nint nmftn, out nint nmsFootnote);

	internal unsafe delegate int GetFootnotePresentationAndRejectionOrder(nint pfsclient, int cFootnotes, nint* rgProposedPresentationOrder, nint* rgProposedRejectionOrder, out int fProposedPresentationOrderAccepted, nint* rgFinalPresentationOrder, out int fProposedRejectionOrderAccepted, nint* rgFinalRejectionOrder);

	internal delegate int FAllowFootnoteSeparation(nint pfsclient, nint nmftn, out int fAllow);

	internal delegate int DuplicateMcsclient(nint pfsclient, nint pmcsclientIn, out nint pmcsclientNew);

	internal delegate int DestroyMcsclient(nint pfsclient, nint pmcsclient);

	internal delegate int FEqualMcsclient(nint pfsclient, nint pmcsclient1, nint pmcsclient2, out int fEqual);

	internal delegate int ConvertMcsclient(nint pfsclient, nint pfsparaclient, nint nmp, uint fswdir, nint pmcsclient, int fSuppressTopSpace, out int dvr);

	internal delegate int GetObjectHandlerInfo(nint pfsclient, int idobj, nint pobjectinfo);

	internal delegate int CreateParaBreakingSession(nint pfsclient, nint pfsparaclient, nint nmp, int iArea, int fsdcpStart, nint pfsbreakreclineclient, uint fswdir, int urStartTrack, int durTrack, int urPageLeftMargin, out nint ppfsparabreakingsession, out int fParagraphJustified);

	internal delegate int DestroyParaBreakingSession(nint pfsclient, nint pfsparabreakingsession);

	internal delegate int GetTextProperties(nint pfsclient, nint nmp, int iArea, ref FSTXTPROPS fstxtprops);

	internal delegate int GetNumberFootnotes(nint pfsclient, nint nmp, int fsdcpStart, int fsdcpLim, out int nFootnote);

	internal unsafe delegate int GetFootnotes(nint pfsclient, nint nmp, int fsdcpStart, int fsdcpLim, int nFootnotes, nint* rgnmftn, int* rgdcp, out int cFootnotes);

	internal delegate int FormatDropCap(nint pfsclient, nint pfsparaclient, nint nmp, int iArea, uint fswdir, int fSuppressTopSpace, out nint pfsdropc, out int fInMargin, out int dur, out int dvr, out int cPolygons, out int cVertices, out int durText);

	internal unsafe delegate int GetDropCapPolygons(nint pfsclient, nint pfsdropc, nint nmp, uint fswdir, int ncVertices, int nfspt, int* rgcVertices, out int ccVertices, FSPOINT* rgfspt, out int cfspt, out int fWrapThrough);

	internal delegate int DestroyDropCap(nint pfsclient, nint pfsdropc);

	internal delegate int FormatBottomText(nint pfsclient, nint pfsparaclient, nint nmp, int iArea, uint fswdir, nint pfslineLast, int dvrLine, out nint pmcsclientOut);

	internal delegate int FormatLine(nint pfsclient, nint pfsparaclient, nint nmp, int iArea, int dcp, nint pbrlineIn, uint fswdir, int urStartLine, int durLine, int urStartTrack, int durTrack, int urPageLeftMargin, int fAllowHyphenation, int fClearOnLeft, int fClearOnRight, int fTreatAsFirstInPara, int fTreatAsLastInPara, int fSuppressTopSpace, out nint pfsline, out int dcpLine, out nint ppbrlineOut, out int fForcedBroken, out FSFLRES fsflres, out int dvrAscent, out int dvrDescent, out int urBBox, out int durBBox, out int dcpDepend, out int fReformatNeighborsAsLastLine);

	internal delegate int FormatLineForced(nint pfsclient, nint pfsparaclient, nint nmp, int iArea, int dcp, nint pbrlineIn, uint fswdir, int urStartLine, int durLine, int urStartTrack, int durTrack, int urPageLeftMargin, int fClearOnLeft, int fClearOnRight, int fTreatAsFirstInPara, int fTreatAsLastInPara, int fSuppressTopSpace, int dvrAvailable, out nint pfsline, out int dcpLine, out nint ppbrlineOut, out FSFLRES fsflres, out int dvrAscent, out int dvrDescent, out int urBBox, out int durBBox, out int dcpDepend);

	internal unsafe delegate int FormatLineVariants(nint pfsclient, nint pfsparabreakingsession, int dcp, nint pbrlineIn, uint fswdir, int urStartLine, int durLine, int fAllowHyphenation, int fClearOnLeft, int fClearOnRight, int fTreatAsFirstInPara, int fTreatAsLastInPara, int fSuppressTopSpace, nint lineVariantRestriction, int nLineVariantsAlloc, FSLINEVARIANT* rgfslinevariant, out int nLineVariantsActual, out int iLineVariantBest);

	internal delegate int ReconstructLineVariant(nint pfsclient, nint pfsparaclient, nint nmp, int iArea, int dcpStart, nint pbrlineIn, int dcpLine, uint fswdir, int urStartLine, int durLine, int urStartTrack, int durTrack, int urPageLeftMargin, int fAllowHyphenation, int fClearOnLeft, int fClearOnRight, int fTreatAsFirstInPara, int fTreatAsLastInPara, int fSuppressTopSpace, out nint pfsline, out nint ppbrlineOut, out int fForcedBroken, out FSFLRES fsflres, out int dvrAscent, out int dvrDescent, out int urBBox, out int durBBox, out int dcpDepend, out int fReformatNeighborsAsLastLine);

	internal delegate int DestroyLine(nint pfsclient, nint pfsline);

	internal delegate int DuplicateLineBreakRecord(nint pfsclient, nint pbrlineIn, out nint pbrlineDup);

	internal delegate int DestroyLineBreakRecord(nint pfsclient, nint pbrlineIn);

	internal delegate int SnapGridVertical(nint pfsclient, uint fswdir, int vrMargin, int vrCurrent, out int vrNew);

	internal delegate int GetDvrSuppressibleBottomSpace(nint pfsclient, nint pfsparaclient, nint pfsline, uint fswdir, out int dvrSuppressible);

	internal delegate int GetDvrAdvance(nint pfsclient, nint pfsparaclient, nint nmp, int dcp, uint fswdir, out int dvr);

	internal delegate int UpdGetChangeInText(nint pfsclient, nint nmp, out int dcpStart, out int ddcpOld, out int ddcpNew);

	internal delegate int UpdGetDropCapChange(nint pfsclient, nint nmp, out int fChanged);

	internal delegate int FInterruptFormattingText(nint pfsclient, nint pfsparaclient, nint nmp, int dcp, int vr, out int fInterruptFormatting);

	internal delegate int GetTextParaCache(nint pfsclient, nint pfsparaclient, nint nmp, int iArea, uint fswdir, int urStartLine, int durLine, int urStartTrack, int durTrack, int urPageLeftMargin, int fClearOnLeft, int fClearOnRight, int fSuppressTopSpace, out int fFound, out int dcpPara, out int urBBox, out int durBBox, out int dvrPara, out FSKCLEAR fskclear, out nint pmcsclientAfterPara, out int cLines, out int fOptimalLines, out int fOptimalLineDcpsCached, out int dvrMinLineHeight);

	internal unsafe delegate int SetTextParaCache(nint pfsclient, nint pfsparaclient, nint nmp, int iArea, uint fswdir, int urStartLine, int durLine, int urStartTrack, int durTrack, int urPageLeftMargin, int fClearOnLeft, int fClearOnRight, int fSuppressTopSpace, int dcpPara, int urBBox, int durBBox, int dvrPara, FSKCLEAR fskclear, nint pmcsclientAfterPara, int cLines, int fOptimalLines, int* rgdcpOptimalLines, int dvrMinLineHeight);

	internal unsafe delegate int GetOptimalLineDcpCache(nint pfsclient, int cLines, int* rgdcp);

	internal delegate int GetNumberAttachedObjectsBeforeTextLine(nint pfsclient, nint nmp, int dcpFirst, out int cAttachedObjects);

	internal unsafe delegate int GetAttachedObjectsBeforeTextLine(nint pfsclient, nint nmp, int dcpFirst, int nAttachedObjects, nint* rgnmpObjects, int* rgidobj, int* rgdcpAnchor, out int cObjects, out int fEndOfParagraph);

	internal delegate int GetNumberAttachedObjectsInTextLine(nint pfsclient, nint pfsline, nint nmp, int dcpFirst, int dcpLim, int fFoundAttachedObjectsBeforeLine, int dcpMaxAnchorAttachedObjectBeforeLine, out int cAttachedObjects);

	internal unsafe delegate int GetAttachedObjectsInTextLine(nint pfsclient, nint pfsline, nint nmp, int dcpFirst, int dcpLim, int fFoundAttachedObjectsBeforeLine, int dcpMaxAnchorAttachedObjectBeforeLine, int nAttachedObjects, nint* rgnmpObjects, int* rgidobj, int* rgdcpAnchor, out int cObjects);

	internal delegate int UpdGetAttachedObjectChange(nint pfsclient, nint nmp, nint nmpAttachedObject, out FSKCHANGE fskchObject);

	internal delegate int GetDurFigureAnchor(nint pfsclient, nint pfsparaclient, nint pfsparaclientFigure, nint pfsline, nint nmpFigure, uint fswdir, nint pfsFmtLineIn, out int dur);

	internal delegate int GetFloaterProperties(nint pfsclient, nint nmFloater, uint fswdirTrack, out FSFLOATERPROPS fsfloaterprops);

	internal delegate int FormatFloaterContentFinite(nint pfsclient, nint pfsparaclient, nint pfsbrkFloaterContentIn, int fBreakRecordFromPreviousPage, nint nmFloater, nint pftnrej, int fEmptyOk, int fSuppressTopSpace, uint fswdirTrack, int fAtMaxWidth, int durAvailable, int dvrAvailable, FSKSUPPRESSHARDBREAKBEFOREFIRSTPARA fsksuppresshardbreakbeforefirstparaIn, out FSFMTR fsfmtr, out nint pfsbrkFloatContentOut, out nint pbrkrecpara, out int durFloaterWidth, out int dvrFloaterHeight, out FSBBOX fsbbox, out int cPolygons, out int cVertices);

	internal delegate int FormatFloaterContentBottomless(nint pfsclient, nint pfsparaclient, nint nmFloater, int fSuppressTopSpace, uint fswdirTrack, int fAtMaxWidth, int durAvailable, int dvrAvailable, out FSFMTRBL fsfmtrbl, out nint pfsbrkFloatContentOut, out int durFloaterWidth, out int dvrFloaterHeight, out FSBBOX fsbbox, out int cPolygons, out int cVertices);

	internal delegate int UpdateBottomlessFloaterContent(nint pfsFloaterContent, nint pfsparaclient, nint nmFloater, int fSuppressTopSpace, uint fswdirTrack, int fAtMaxWidth, int durAvailable, int dvrAvailable, out FSFMTRBL fsfmtrbl, out int durFloaterWidth, out int dvrFloaterHeight, out FSBBOX fsbbox, out int cPolygons, out int cVertices);

	internal unsafe delegate int GetFloaterPolygons(nint pfsparaclient, nint pfsFloaterContent, nint nmFloater, uint fswdirTrack, int ncVertices, int nfspt, int* rgcVertices, out int cVertices, FSPOINT* rgfspt, out int cfspt, out int fWrapThrough);

	internal delegate int ClearUpdateInfoInFloaterContent(nint pfsFloaterContent);

	internal delegate int CompareFloaterContents(nint pfsFloaterContentOld, nint pfsFloaterContentNew, out FSCOMPRESULT fscmpr);

	internal delegate int DestroyFloaterContent(nint pfsFloaterContent);

	internal delegate int DuplicateFloaterContentBreakRecord(nint pfsclient, nint pfsbrkFloaterContent, out nint pfsbrkFloaterContentDup);

	internal delegate int DestroyFloaterContentBreakRecord(nint pfsclient, nint pfsbrkFloaterContent);

	internal delegate int GetFloaterContentColumnBalancingInfo(nint pfsFloaterContent, uint fswdir, out int nlines, out int dvrSumHeight, out int dvrMinHeight);

	internal delegate int GetFloaterContentNumberFootnotes(nint pfsFloaterContent, out int cftn);

	internal delegate int GetFloaterContentFootnoteInfo(nint pfsFloaterContent, uint fswdir, int nftn, int iftnFirst, ref FSFTNINFO fsftninf, out int iftnLim);

	internal delegate int TransferDisplayInfoInFloaterContent(nint pfsFloaterContentOld, nint pfsFloaterContentNew);

	internal delegate int GetMCSClientAfterFloater(nint pfsclient, nint pfsparaclient, nint nmFloater, uint fswdirTrack, nint pmcsclientIn, out nint pmcsclientOut);

	internal delegate int GetDvrUsedForFloater(nint pfsclient, nint pfsparaclient, nint nmFloater, uint fswdirTrack, nint pmcsclientIn, int dvrDisplaced, out int dvrUsed);

	internal delegate int ObjCreateContext(nint pfsclient, nint pfsc, nint pfscbkobj, uint ffi, int idobj, out nint pfssobjc);

	internal delegate int ObjDestroyContext(nint pfssobjc);

	internal delegate int ObjFormatParaFinite(nint pfssobjc, nint pfsparaclient, nint pfsobjbrk, int fBreakRecordFromPreviousPage, nint nmp, int iArea, nint pftnrej, nint pfsgeom, int fEmptyOk, int fSuppressTopSpace, uint fswdir, ref FSRECT fsrcToFill, nint pmcsclientIn, FSKCLEAR fskclearIn, FSKSUPPRESSHARDBREAKBEFOREFIRSTPARA fsksuppresshardbreakbeforefirstparaIn, int fBreakInside, out FSFMTR fsfmtr, out nint pfspara, out nint pbrkrecpara, out int dvrUsed, out FSBBOX fsbbox, out nint pmcsclientOut, out FSKCLEAR fskclearOut, out int dvrTopSpace, out int fBreakInsidePossible);

	internal delegate int ObjFormatParaBottomless(nint pfssobjc, nint pfsparaclient, nint nmp, int iArea, nint pfsgeom, int fSuppressTopSpace, uint fswdir, int urTrack, int durTrack, int vrTrack, nint pmcsclientIn, FSKCLEAR fskclearIn, int fInterruptable, out FSFMTRBL fsfmtrbl, out nint pfspara, out int dvrUsed, out FSBBOX fsbbox, out nint pmcsclientOut, out FSKCLEAR fskclearOut, out int dvrTopSpace, out int fPageBecomesUninterruptable);

	internal delegate int ObjUpdateBottomlessPara(nint pfspara, nint pfsparaclient, nint nmp, int iArea, nint pfsgeom, int fSuppressTopSpace, uint fswdir, int urTrack, int durTrack, int vrTrack, nint pmcsclientIn, FSKCLEAR fskclearIn, int fInterruptable, out FSFMTRBL fsfmtrbl, out int dvrUsed, out FSBBOX fsbbox, out nint pmcsclientOut, out FSKCLEAR fskclearOut, out int dvrTopSpace, out int fPageBecomesUninterruptable);

	internal delegate int ObjSynchronizeBottomlessPara(nint pfspara, nint pfsparaclient, nint pfsgeom, uint fswdir, int dvrShift);

	internal delegate int ObjComparePara(nint pfsparaclientOld, nint pfsparaOld, nint pfsparaclientNew, nint pfsparaNew, uint fswdir, out FSCOMPRESULT fscmpr, out int dvrShifted);

	internal delegate int ObjClearUpdateInfoInPara(nint pfspara);

	internal delegate int ObjDestroyPara(nint pfspara);

	internal delegate int ObjDuplicateBreakRecord(nint pfssobjc, nint pfsbrkrecparaOrig, out nint pfsbrkrecparaDup);

	internal delegate int ObjDestroyBreakRecord(nint pfssobjc, nint pfsobjbrk);

	internal delegate int ObjGetColumnBalancingInfo(nint pfspara, uint fswdir, out int nlines, out int dvrSumHeight, out int dvrMinHeight);

	internal delegate int ObjGetNumberFootnotes(nint pfspara, out int nftn);

	internal unsafe delegate int ObjGetFootnoteInfo(nint pfspara, uint fswdir, int nftn, int iftnFirst, FSFTNINFO* pfsftninf, out int iftnLim);

	internal delegate int ObjShiftVertical(nint pfspara, nint pfsparaclient, nint pfsshift, uint fswdir, out FSBBOX fsbbox);

	internal delegate int ObjTransferDisplayInfoPara(nint pfsparaOld, nint pfsparaNew);

	internal delegate int GetTableProperties(nint pfsclient, nint nmTable, uint fswdirTrack, out FSTABLEOBJPROPS fstableobjprops);

	internal delegate int AutofitTable(nint pfsclient, nint pfsparaclientTable, nint nmTable, uint fswdirTrack, int durAvailableSpace, out int durTableWidth);

	internal delegate int UpdAutofitTable(nint pfsclient, nint pfsparaclientTable, nint nmTable, uint fswdirTrack, int durAvailableSpace, out int durTableWidth, out int fNoChangeInCellWidths);

	internal delegate int GetMCSClientAfterTable(nint pfsclient, nint pfsparaclientTable, nint nmTable, uint fswdirTrack, nint pmcsclientIn, out nint ppmcsclientOut);

	internal delegate int GetFirstHeaderRow(nint pfsclient, nint nmTable, int fRepeatedHeader, out int fFound, out nint pnmFirstHeaderRow);

	internal delegate int GetNextHeaderRow(nint pfsclient, nint nmTable, nint nmHeaderRow, int fRepeatedHeader, out int fFound, out nint pnmNextHeaderRow);

	internal delegate int GetFirstFooterRow(nint pfsclient, nint nmTable, int fRepeatedFooter, out int fFound, out nint pnmFirstFooterRow);

	internal delegate int GetNextFooterRow(nint pfsclient, nint nmTable, nint nmFooterRow, int fRepeatedFooter, out int fFound, out nint pnmNextFooterRow);

	internal delegate int GetFirstRow(nint pfsclient, nint nmTable, out int fFound, out nint pnmFirstRow);

	internal delegate int GetNextRow(nint pfsclient, nint nmTable, nint nmRow, out int fFound, out nint pnmNextRow);

	internal delegate int UpdFChangeInHeaderFooter(nint pfsclient, nint nmTable, out int fHeaderChanged, out int fFooterChanged, out int fRepeatedHeaderChanged, out int fRepeatedFooterChanged);

	internal delegate int UpdGetFirstChangeInTable(nint pfsclient, nint nmTable, out int fFound, out int fChangeFirst, out nint pnmRowBeforeChange);

	internal delegate int UpdGetRowChange(nint pfsclient, nint nmTable, nint nmRow, out FSKCHANGE fskch, out int fNoFurtherChanges);

	internal delegate int UpdGetCellChange(nint pfsclient, nint nmRow, nint nmCell, out int fWidthChanged, out FSKCHANGE fskchCell);

	internal delegate int GetDistributionKind(nint pfsclient, nint nmTable, uint fswdirTable, out FSKTABLEHEIGHTDISTRIBUTION tabledistr);

	internal delegate int GetRowProperties(nint pfsclient, nint nmRow, uint fswdirTable, out FSTABLEROWPROPS rowprops);

	internal unsafe delegate int GetCells(nint pfsclient, nint nmRow, int cCells, nint* rgnmCell, FSTABLEKCELLMERGE* rgkcellmerge);

	internal delegate int FInterruptFormattingTable(nint pfsclient, nint pfsparaclient, nint nmRow, int dvr, out int fInterrupt);

	internal unsafe delegate int CalcHorizontalBBoxOfRow(nint pfsclient, nint nmRow, int cCells, nint* rgnmCell, nint* rgpfscell, out int urBBox, out int durBBox);

	internal delegate int FormatCellFinite(nint pfsclient, nint pfsparaclientTable, nint pfsbrkcell, nint nmCell, nint pfsFtnRejector, int fEmptyOK, uint fswdirTable, int dvrExtraHeight, int dvrAvailable, out FSFMTR pfmtr, out nint ppfscell, out nint pfsbrkcellOut, out int dvrUsed);

	internal delegate int FormatCellBottomless(nint pfsclient, nint pfsparaclientTable, nint nmCell, uint fswdirTable, out FSFMTRBL fmtrbl, out nint ppfscell, out int dvrUsed);

	internal delegate int UpdateBottomlessCell(nint pfscell, nint pfsparaclientTable, nint nmCell, uint fswdirTable, out FSFMTRBL fmtrbl, out int dvrUsed);

	internal delegate int CompareCells(nint pfscellOld, nint pfscellNew, out FSCOMPRESULT pfscmpr);

	internal delegate int ClearUpdateInfoInCell(nint pfscell);

	internal delegate int SetCellHeight(nint pfscell, nint pfsparaclientTable, nint pfsbrkcell, nint nmCell, int fBrokenHere, uint fswdirTable, int dvrActual);

	internal delegate int DestroyCell(nint pfsCell);

	internal delegate int DuplicateCellBreakRecord(nint pfsclient, nint pfsbrkcell, out nint ppfsbrkcellDup);

	internal delegate int DestroyCellBreakRecord(nint pfsclient, nint pfsbrkcell);

	internal delegate int GetCellNumberFootnotes(nint pfscell, out int cFtn);

	internal delegate int GetCellMinColumnBalancingStep(nint pfscell, uint fswdir, out int pdvrMinStep);

	internal delegate int TransferDisplayInfoCell(nint pfscellOld, nint pfscellNew);

	internal const int True = 1;

	internal const int False = 0;

	internal const int dvBottomUndefined = int.MaxValue;

	internal const int MaxFontSize = 160000;

	internal const int MaxPageSize = 3500000;

	internal const int fsffiWordFlowTextFinite = 1;

	internal const int fsffiWordClashFootnotesWithText = 2;

	internal const int fsffiWordNewSectionAboveFootnotes = 4;

	internal const int fsffiWordStopAfterFirstCollision = 8;

	internal const int fsffiUseTextParaCache = 16;

	internal const int fsffiKeepClientLines = 32;

	internal const int fsffiUseTextQuickLoop = 64;

	internal const int fsffiAvalonDisableOptimalInChains = 256;

	internal const int fsffiWordAdjustTrackWidthsForFigureInWebView = 512;

	internal const int fsidobjText = -1;

	internal const int fsidobjFigure = -2;

	internal const int fswdirDefault = 0;

	internal const int fswdirES = 0;

	internal const int fswdirEN = 1;

	internal const int fswdirSE = 2;

	internal const int fswdirSW = 3;

	internal const int fswdirWS = 4;

	internal const int fswdirWN = 5;

	internal const int fswdirNE = 6;

	internal const int fswdirNW = 7;

	internal const int fUDirection = 4;

	internal const int fVDirection = 1;

	internal const int fUVertical = 2;

	internal const int fserrNone = 0;

	internal const int fserrOutOfMemory = -2;

	internal const int fserrNotImplemented = -10000;

	internal const int fserrCallbackException = -100002;

	internal const int tserrNone = 0;

	internal const int tserrInvalidParameter = -1;

	internal const int tserrOutOfMemory = -2;

	internal const int tserrNullOutputParameter = -3;

	internal const int tserrInvalidLsContext = -4;

	internal const int tserrInvalidLine = -5;

	internal const int tserrInvalidDnode = -6;

	internal const int tserrInvalidDeviceResolution = -7;

	internal const int tserrInvalidRun = -8;

	internal const int tserrMismatchLineContext = -9;

	internal const int tserrContextInUse = -10;

	internal const int tserrDuplicateSpecialCharacter = -11;

	internal const int tserrInvalidAutonumRun = -12;

	internal const int tserrFormattingFunctionDisabled = -13;

	internal const int tserrUnfinishedDnode = -14;

	internal const int tserrInvalidDnodeType = -15;

	internal const int tserrInvalidPenDnode = -16;

	internal const int tserrInvalidNonPenDnode = -17;

	internal const int tserrInvalidBaselinePenDnode = -18;

	internal const int tserrInvalidFormatterResult = -19;

	internal const int tserrInvalidObjectIdFetched = -20;

	internal const int tserrInvalidDcpFetched = -21;

	internal const int tserrInvalidCpContentFetched = -22;

	internal const int tserrInvalidBookmarkType = -23;

	internal const int tserrSetDocDisabled = -24;

	internal const int tserrFiniFunctionDisabled = -25;

	internal const int tserrCurrentDnodeIsNotTab = -26;

	internal const int tserrPendingTabIsNotResolved = -27;

	internal const int tserrWrongFiniFunction = -28;

	internal const int tserrInvalidBreakingClass = -29;

	internal const int tserrBreakingTableNotSet = -30;

	internal const int tserrInvalidModWidthClass = -31;

	internal const int tserrModWidthPairsNotSet = -32;

	internal const int tserrWrongTruncationPoint = -33;

	internal const int tserrWrongBreak = -34;

	internal const int tserrDupInvalid = -35;

	internal const int tserrRubyInvalidVersion = -36;

	internal const int tserrTatenakayokoInvalidVersion = -37;

	internal const int tserrWarichuInvalidVersion = -38;

	internal const int tserrWarichuInvalidData = -39;

	internal const int tserrCreateSublineDisabled = -40;

	internal const int tserrCurrentSublineDoesNotExist = -41;

	internal const int tserrCpOutsideSubline = -42;

	internal const int tserrHihInvalidVersion = -43;

	internal const int tserrInsufficientQueryDepth = -44;

	internal const int tserrInvalidBreakRecord = -45;

	internal const int tserrInvalidPap = -46;

	internal const int tserrContradictoryQueryInput = -47;

	internal const int tserrLineIsNotActive = -48;

	internal const int tserrTooLongParagraph = -49;

	internal const int tserrTooManyCharsToGlyph = -50;

	internal const int tserrWrongHyphenationPosition = -51;

	internal const int tserrTooManyPriorities = -52;

	internal const int tserrWrongGivenCp = -53;

	internal const int tserrWrongCpFirstForGetBreaks = -54;

	internal const int tserrWrongJustTypeForGetBreaks = -55;

	internal const int tserrWrongJustTypeForCreateLineGivenCp = -56;

	internal const int tserrTooLongGlyphContext = -57;

	internal const int tserrInvalidCharToGlyphMapping = -58;

	internal const int tserrInvalidMathUsage = -59;

	internal const int tserrInconsistentChp = -60;

	internal const int tserrStoppedInSubline = -61;

	internal const int tserrPenPositionCouldNotBeUsed = -62;

	internal const int tserrDebugFlagsInShip = -63;

	internal const int tserrInvalidOrderTabs = -64;

	internal const int tserrSystemRestrictionsExceeded = -100;

	internal const int tserrInvalidPtsContext = -103;

	internal const int tserrInvalidClientOutput = -104;

	internal const int tserrInvalidObjectOutput = -105;

	internal const int tserrInvalidGeometry = -106;

	internal const int tserrInvalidFootnoteRejector = -107;

	internal const int tserrInvalidFootnoteInfo = -108;

	internal const int tserrOutputArrayTooSmall = -110;

	internal const int tserrWordNotSupportedInBottomless = -111;

	internal const int tserrPageTooLong = -112;

	internal const int tserrInvalidQuery = -113;

	internal const int tserrWrongWritingDirection = -114;

	internal const int tserrPageNotClearedForUpdate = -115;

	internal const int tserrInternalError = -1000;

	internal const int tserrNotImplemented = -10000;

	internal const int tserrClientAbort = -100000;

	internal const int tserrPageSizeMismatch = -100001;

	internal const int tserrCallbackException = -100002;

	internal const int fsfdbgCheckVariantsConsistency = 1;

	internal static void IgnoreError(int fserr)
	{
	}

	internal static void Validate(int fserr)
	{
		if (fserr != 0)
		{
			Error(fserr, null);
		}
	}

	internal static void Validate(int fserr, PtsContext ptsContext)
	{
		if (fserr != 0)
		{
			Error(fserr, ptsContext);
		}
	}

	private static void Error(int fserr, PtsContext ptsContext)
	{
		switch (fserr)
		{
		case -2:
			throw new OutOfMemoryException();
		case -100002:
			if (ptsContext != null)
			{
				SecondaryException ex = new SecondaryException(ptsContext.CallbackException);
				ptsContext.CallbackException = null;
				throw ex;
			}
			throw new Exception(SR.Format(SR.PTSError, fserr));
		case -112:
		case -100:
			throw new PtsException(SR.Format(SR.FormatRestrictionsExceeded, fserr));
		default:
			throw new PtsException(SR.Format(SR.PTSError, fserr));
		}
	}

	internal static void ValidateAndTrace(int fserr, PtsContext ptsContext)
	{
		if (fserr != 0)
		{
			ErrorTrace(fserr, ptsContext);
		}
	}

	private static void ErrorTrace(int fserr, PtsContext ptsContext)
	{
		if (fserr == -2)
		{
			throw new OutOfMemoryException();
		}
		if (ptsContext != null)
		{
			Exception innermostException = GetInnermostException(ptsContext);
			if (innermostException == null || innermostException is SecondaryException || innermostException is PtsException)
			{
				string p = ((innermostException == null) ? string.Empty : innermostException.Message);
				if (TracePageFormatting.IsEnabled)
				{
					TracePageFormatting.Trace(TraceEventType.Start, TracePageFormatting.PageFormattingError, ptsContext, p);
					TracePageFormatting.Trace(TraceEventType.Stop, TracePageFormatting.PageFormattingError, ptsContext, p);
				}
				return;
			}
			SecondaryException ex = new SecondaryException(innermostException);
			ptsContext.CallbackException = null;
			throw ex;
		}
		throw new Exception(SR.Format(SR.PTSError, fserr));
	}

	private static Exception GetInnermostException(PtsContext ptsContext)
	{
		Invariant.Assert(ptsContext != null);
		Exception ex = ptsContext.CallbackException;
		Exception ex2 = ex;
		while (ex2 != null)
		{
			ex2 = ex.InnerException;
			if (ex2 != null)
			{
				ex = ex2;
			}
			if (!(ex is PtsException) && !(ex is SecondaryException))
			{
				break;
			}
		}
		return ex;
	}

	internal static void ValidateHandle(object handle)
	{
		if (handle == null)
		{
			InvalidHandle();
		}
	}

	private static void InvalidHandle()
	{
		throw new Exception(SR.PTSInvalidHandle);
	}

	internal static int FromBoolean(bool condition)
	{
		return condition ? 1 : 0;
	}

	internal static bool ToBoolean(int flag)
	{
		return flag != 0;
	}

	internal static FSKWRAP WrapDirectionToFskwrap(WrapDirection wrapDirection)
	{
		return (FSKWRAP)wrapDirection;
	}

	internal static FSKCLEAR WrapDirectionToFskclear(WrapDirection wrapDirection)
	{
		return wrapDirection switch
		{
			WrapDirection.None => FSKCLEAR.fskclearNone, 
			WrapDirection.Left => FSKCLEAR.fskclearLeft, 
			WrapDirection.Right => FSKCLEAR.fskclearRight, 
			WrapDirection.Both => FSKCLEAR.fskclearBoth, 
			_ => FSKCLEAR.fskclearNone, 
		};
	}

	internal static FlowDirection FswdirToFlowDirection(uint fswdir)
	{
		if (fswdir == 4)
		{
			return FlowDirection.RightToLeft;
		}
		return FlowDirection.LeftToRight;
	}

	internal static uint FlowDirectionToFswdir(FlowDirection fd)
	{
		if (fd == FlowDirection.RightToLeft)
		{
			return 4u;
		}
		return 0u;
	}

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern int GetFloaterHandlerInfo([In] ref FSFLOATERINIT pfsfloaterinit, nint pFloaterObjectInfo);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern int GetTableObjHandlerInfo([In] ref FSTABLEOBJINIT pfstableobjinit, nint pTableObjectInfo);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern int CreateInstalledObjectsInfo([In] ref FSIMETHODS fssubtrackparamethods, ref FSIMETHODS fssubpageparamethods, out nint pInstalledObjects, out int cInstalledObjects);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern int DestroyInstalledObjectsInfo(nint pInstalledObjects);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern int CreateDocContext([In] ref FSCONTEXTINFO fscontextinfo, out nint pfscontext);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern int DestroyDocContext(nint pfscontext);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern int FsCreatePageFinite(nint pfscontext, nint pfsBRPageStart, nint fsnmSectStart, out FSFMTR pfsfmtrOut, out nint ppfsPageOut, out nint ppfsBRPageOut);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern int FsUpdateFinitePage(nint pfscontext, nint pfspage, nint pfsBRPageStart, nint fsnmSectStart, out FSFMTR pfsfmtrOut, out nint ppfsBRPageOut);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern int FsCreatePageBottomless(nint pfscontext, nint fsnmsect, out FSFMTRBL pfsfmtrbl, out nint ppfspage);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern int FsUpdateBottomlessPage(nint pfscontext, nint pfspage, nint fsnmsect, out FSFMTRBL pfsfmtrbl);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern int FsClearUpdateInfoInPage(nint pfscontext, nint pfspage);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern int FsDestroyPage(nint pfscontext, nint pfspage);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern int FsDestroyPageBreakRecord(nint pfscontext, nint pfsbreakrec);

	[DllImport("PresentationNative_cor3.dll")]
	internal unsafe static extern int FsCreateSubpageFinite(nint pfsContext, nint pBRSubPageStart, int fFromPreviousPage, nint nSeg, nint pFtnRej, int fEmptyOk, int fSuppressTopSpace, uint fswdir, int lWidth, int lHeight, ref FSRECT rcMargin, int cColumns, FSCOLUMNINFO* rgColumnInfo, int fApplyColumnBalancing, int cSegmentAreas, nint* rgnSegmentForArea, int* rgSpanForSegmentArea, int cHeightAreas, int* rgHeightForArea, int* rgSpanForHeightArea, int fAllowOverhangBottom, FSKSUPPRESSHARDBREAKBEFOREFIRSTPARA fsksuppresshardbreakbeforefirstparaIn, out FSFMTR fsfmtr, out nint pSubPage, out nint pBRSubPageOut, out int dvrUsed, out FSBBOX fsBBox, out nint pfsMcsClient, out int topSpace);

	[DllImport("PresentationNative_cor3.dll")]
	internal unsafe static extern int FsCreateSubpageBottomless(nint pfsContext, nint nSeg, int fSuppressTopSpace, uint fswdir, int lWidth, int urMargin, int durMargin, int vrMargin, int cColumns, FSCOLUMNINFO* rgColumnInfo, int cSegmentAreas, nint* rgnSegmentForArea, int* rgSpanForSegmentArea, int cHeightAreas, int* rgHeightForArea, int* rgSpanForHeightArea, int fINterrruptible, out FSFMTRBL pfsfmtr, out nint ppSubPage, out int pdvrUsed, out FSBBOX pfsBBox, out nint pfsMcsClient, out int pTopSpace, out int fPageBecomesUninterruptible);

	[DllImport("PresentationNative_cor3.dll")]
	internal unsafe static extern int FsUpdateBottomlessSubpage(nint pfsContext, nint pfsSubpage, nint nmSeg, int fSuppressTopSpace, uint fswdir, int lWidth, int urMargin, int durMargin, int vrMargin, int cColumns, FSCOLUMNINFO* rgColumnInfo, int cSegmentAreas, nint* rgnSegmentForArea, int* rgSpanForSegmentArea, int cHeightAreas, int* rgHeightForArea, int* rgSpanForHeightArea, int fINterrruptible, out FSFMTRBL pfsfmtr, out int pdvrUsed, out FSBBOX pfsBBox, out nint pfsMcsClient, out int pTopSpace, out int fPageBecomesUninterruptible);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern int FsCompareSubpages(nint pfsContext, nint pfsSubpageOld, nint pfsSubpageNew, out FSCOMPRESULT fsCompResult);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern int FsClearUpdateInfoInSubpage(nint pfscontext, nint pSubpage);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern int FsDestroySubpage(nint pfsContext, nint pSubpage);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern int FsDuplicateSubpageBreakRecord(nint pfsContext, nint pBreakRecSubPageIn, out nint ppBreakRecSubPageOut);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern int FsDestroySubpageBreakRecord(nint pfscontext, nint pfsbreakrec);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern int FsGetSubpageColumnBalancingInfo(nint pfsContext, nint pSubpage, out uint fswdir, out int lLineNumber, out int lLineHeights, out int lMinimumLineHeight);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern int FsGetNumberSubpageFootnotes(nint pfsContext, nint pSubpage, out int cFootnotes);

	[DllImport("PresentationNative_cor3.dll")]
	internal unsafe static extern int FsGetSubpageFootnoteInfo(nint pfsContext, nint pSubpage, int cArraySize, int indexStart, out uint fswdir, FSFTNINFO* rgFootnoteInfo, out int indexLim);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern int FsTransferDisplayInfoSubpage(nint pfsContext, nint pSubpageOld, nint pfsSubpageNew);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern int FsFormatSubtrackFinite(nint pfsContext, nint pfsBRSubtackIn, int fFromPreviousPage, nint fsnmSegment, int iArea, nint pfsFtnRej, nint pfsGeom, int fEmptyOk, int fSuppressTopSpace, uint fswdir, [In] ref FSRECT fsRectToFill, nint pfsMcsClientIn, FSKCLEAR fsKClearIn, FSKSUPPRESSHARDBREAKBEFOREFIRSTPARA fsksuppresshardbreakbeforefirstpara, out FSFMTR pfsfmtr, out nint ppfsSubtrack, out nint pfsBRSubtrackOut, out int pdvrUsed, out FSBBOX pfsBBox, out nint ppfsMcsClientOut, out FSKCLEAR pfsKClearOut, out int pTopSpace);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern int FsFormatSubtrackBottomless(nint pfsContext, nint fsnmSegment, int iArea, nint pfsGeom, int fSuppressTopSpace, uint fswdir, int ur, int dur, int vr, nint pfsMcsClientIn, FSKCLEAR fsKClearIn, int fCanBeInterruptedIn, out FSFMTRBL pfsfmtrbl, out nint ppfsSubtrack, out int pdvrUsed, out FSBBOX pfsBBox, out nint ppfsMcsClientOut, out FSKCLEAR pfsKClearOut, out int pTopSpace, out int pfCanBeInterruptedOut);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern int FsUpdateBottomlessSubtrack(nint pfsContext, nint pfsSubtrack, nint fsnmSegment, int iArea, nint pfsGeom, int fSuppressTopSpace, uint fswdir, int ur, int dur, int vr, nint pfsMcsClientIn, FSKCLEAR fsKClearIn, int fCanBeInterruptedIn, out FSFMTRBL pfsfmtrbl, out int pdvrUsed, out FSBBOX pfsBBox, out nint ppfsMcsClientOut, out FSKCLEAR pfsKClearOut, out int pTopSpace, out int pfCanBeInterruptedOut);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern int FsSynchronizeBottomlessSubtrack(nint pfsContext, nint pfsSubtrack, nint pfsGeom, uint fswdir, int vrShift);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern int FsCompareSubtrack(nint pfsContext, nint pfsSubtrackOld, nint pfsSubtrackNew, uint fswdir, out FSCOMPRESULT fsCompResult, out int dvrShifted);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern int FsClearUpdateInfoInSubtrack(nint pfsContext, nint pfsSubtrack);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern int FsDestroySubtrack(nint pfsContext, nint pfsSubtrack);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern int FsDuplicateSubtrackBreakRecord(nint pfsContext, nint pfsBRSubtrackIn, out nint ppfsBRSubtrackOut);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern int FsDestroySubtrackBreakRecord(nint pfscontext, nint pfsbreakrec);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern int FsGetSubtrackColumnBalancingInfo(nint pfscontext, nint pfsSubtrack, uint fswdir, out int lLineNumber, out int lLineHeights, out int lMinimumLineHeight);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern int FsGetNumberSubtrackFootnotes(nint pfscontext, nint pfsSubtrack, out int cFootnotes);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern int FsTransferDisplayInfoSubtrack(nint pfscontext, nint pfsSubtrackOld, nint pfsSubtrackNew);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern int FsQueryFloaterDetails(nint pfsContext, nint pfsfloater, out FSFLOATERDETAILS fsfloaterdetails);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern int FsQueryPageDetails(nint pfsContext, nint pPage, out FSPAGEDETAILS pPageDetails);

	[DllImport("PresentationNative_cor3.dll")]
	internal unsafe static extern int FsQueryPageSectionList(nint pfsContext, nint pPage, int cArraySize, FSSECTIONDESCRIPTION* rgSectionDescription, out int cActualSize);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern int FsQuerySectionDetails(nint pfsContext, nint pSection, out FSSECTIONDETAILS pSectionDetails);

	[DllImport("PresentationNative_cor3.dll")]
	internal unsafe static extern int FsQuerySectionBasicColumnList(nint pfsContext, nint pSection, int cArraySize, FSTRACKDESCRIPTION* rgColumnDescription, out int cActualSize);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern int FsQueryTrackDetails(nint pfsContext, nint pTrack, out FSTRACKDETAILS pTrackDetails);

	[DllImport("PresentationNative_cor3.dll")]
	internal unsafe static extern int FsQueryTrackParaList(nint pfsContext, nint pTrack, int cParas, FSPARADESCRIPTION* rgParaDesc, out int cParaDesc);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern int FsQuerySubpageDetails(nint pfsContext, nint pSubPage, out FSSUBPAGEDETAILS pSubPageDetails);

	[DllImport("PresentationNative_cor3.dll")]
	internal unsafe static extern int FsQuerySubpageBasicColumnList(nint pfsContext, nint pSubPage, int cArraySize, FSTRACKDESCRIPTION* rgColumnDescription, out int cActualSize);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern int FsQuerySubtrackDetails(nint pfsContext, nint pSubTrack, out FSSUBTRACKDETAILS pSubTrackDetails);

	[DllImport("PresentationNative_cor3.dll")]
	internal unsafe static extern int FsQuerySubtrackParaList(nint pfsContext, nint pSubTrack, int cParas, FSPARADESCRIPTION* rgParaDesc, out int cParaDesc);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern int FsQueryTextDetails(nint pfsContext, nint pPara, out FSTEXTDETAILS pTextDetails);

	[DllImport("PresentationNative_cor3.dll")]
	internal unsafe static extern int FsQueryLineListSingle(nint pfsContext, nint pPara, int cLines, FSLINEDESCRIPTIONSINGLE* rgLineDesc, out int cLineDesc);

	[DllImport("PresentationNative_cor3.dll")]
	internal unsafe static extern int FsQueryLineListComposite(nint pfsContext, nint pPara, int cElements, FSLINEDESCRIPTIONCOMPOSITE* rgLineDescription, out int cLineElements);

	[DllImport("PresentationNative_cor3.dll")]
	internal unsafe static extern int FsQueryLineCompositeElementList(nint pfsContext, nint pLine, int cElements, FSLINEELEMENT* rgLineElement, out int cLineElements);

	[DllImport("PresentationNative_cor3.dll")]
	internal unsafe static extern int FsQueryAttachedObjectList(nint pfsContext, nint pPara, int cAttachedObject, FSATTACHEDOBJECTDESCRIPTION* rgAttachedObjects, out int cAttachedObjectDesc);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern int FsQueryFigureObjectDetails(nint pfsContext, nint pPara, out FSFIGUREDETAILS fsFigureDetails);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern int FsQueryTableObjDetails(nint pfscontext, nint pfstableobj, out FSTABLEOBJDETAILS pfstableobjdetails);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern int FsQueryTableObjTableProperDetails(nint pfscontext, nint pfstableProper, out FSTABLEDETAILS pfstabledetailsProper);

	[DllImport("PresentationNative_cor3.dll")]
	internal unsafe static extern int FsQueryTableObjRowList(nint pfscontext, nint pfstableProper, int cRows, FSTABLEROWDESCRIPTION* rgtablerowdescr, out int pcRowsActual);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern int FsQueryTableObjRowDetails(nint pfscontext, nint pfstablerow, out FSTABLEROWDETAILS ptableorowdetails);

	[DllImport("PresentationNative_cor3.dll")]
	internal unsafe static extern int FsQueryTableObjCellList(nint pfscontext, nint pfstablerow, int cCells, FSKUPDATE* rgfskupd, nint* rgpfscell, FSTABLEKCELLMERGE* rgkcellmerge, out int pcCellsActual);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern int FsTransformRectangle(uint fswdirIn, ref FSRECT rectPage, ref FSRECT rectTransform, uint fswdirOut, out FSRECT rectOut);

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern int FsTransformBbox(uint fswdirIn, ref FSRECT rectPage, ref FSBBOX bboxTransform, uint fswdirOut, out FSBBOX bboxOut);
}
