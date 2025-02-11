using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using MS.Internal.PresentationCore;
using MS.Internal.Text.TextInterface;

namespace MS.Internal.TextFormatting;

internal sealed class LineServicesCallbacks
{
	[Serializable]
	private class ExceptionContext
	{
		public const string Key = "ExceptionContext";

		private object _innerContext;

		private string _stackTraceOrMethodName;

		private uint _plsrun;

		[NonSerialized]
		private LSRun _lsrun;

		public ExceptionContext(object innerContext, string stackTraceOrMethodName, Plsrun plsrun, LSRun lsrun)
		{
			_stackTraceOrMethodName = stackTraceOrMethodName;
			_plsrun = (uint)plsrun;
			_lsrun = lsrun;
			_innerContext = innerContext;
		}

		public override string ToString()
		{
			return _stackTraceOrMethodName;
		}
	}

	private FetchRunRedefined _pfnFetchRunRedefined;

	private FetchLineProps _pfnFetchLineProps;

	private FetchPap _pfnFetchPap;

	private GetRunTextMetrics _pfnGetRunTextMetrics;

	private GetRunCharWidths _pfnGetRunCharWidths;

	private GetDurMaxExpandRagged _pfnGetDurMaxExpandRagged;

	private GetAutoNumberInfo _pfnGetAutoNumberInfo;

	private DrawTextRun _pfnDrawTextRun;

	private GetGlyphsRedefined _pfnGetGlyphsRedefined;

	private GetGlyphPositions _pfnGetGlyphPositions;

	private DrawGlyphs _pfnDrawGlyphs;

	private GetObjectHandlerInfo _pfnGetObjectHandlerInfo;

	private GetRunUnderlineInfo _pfnGetRunUnderlineInfo;

	private GetRunStrikethroughInfo _pfnGetRunStrikethroughInfo;

	private Hyphenate _pfnHyphenate;

	private GetNextHyphenOpp _pfnGetNextHyphenOpp;

	private GetPrevHyphenOpp _pfnGetPrevHyphenOpp;

	private DrawUnderline _pfnDrawUnderline;

	private DrawStrikethrough _pfnDrawStrikethrough;

	private FInterruptShaping _pfnFInterruptShaping;

	private GetCharCompressionInfoFullMixed _pfnGetCharCompressionInfoFullMixed;

	private GetCharExpansionInfoFullMixed _pfnGetCharExpansionInfoFullMixed;

	private GetGlyphCompressionInfoFullMixed _pfnGetGlyphCompressionInfoFullMixed;

	private GetGlyphExpansionInfoFullMixed _pfnGetGlyphExpansionInfoFullMixed;

	private EnumText _pfnEnumText;

	private EnumTab _pfnEnumTab;

	private InlineFormat _pfnInlineFormat;

	private InlineDraw _pfnInlineDraw;

	private Exception _exception;

	private object _owner;

	private Rect _boundingBox;

	private ICollection<IndexedGlyphRun> _indexedGlyphRuns;

	internal InlineFormat InlineFormatDelegate
	{
		get
		{
			if (_pfnInlineFormat == null)
			{
				_pfnInlineFormat = InlineFormat;
			}
			return _pfnInlineFormat;
		}
	}

	internal InlineDraw InlineDrawDelegate
	{
		get
		{
			if (_pfnInlineDraw == null)
			{
				_pfnInlineDraw = InlineDraw;
			}
			return _pfnInlineDraw;
		}
	}

	internal Exception Exception
	{
		get
		{
			return _exception;
		}
		set
		{
			_exception = value;
		}
	}

	internal object Owner
	{
		get
		{
			return _owner;
		}
		set
		{
			_owner = value;
		}
	}

	private FullTextState FullText => _owner as FullTextState;

	private DrawingState Draw => _owner as DrawingState;

	internal Rect BoundingBox => _boundingBox;

	internal ICollection<IndexedGlyphRun> IndexedGlyphRuns
	{
		get
		{
			if (_indexedGlyphRuns == null)
			{
				_indexedGlyphRuns = new List<IndexedGlyphRun>(8);
			}
			return _indexedGlyphRuns;
		}
	}

	internal unsafe LsErr FetchRunRedefined(nint pols, int lscpFetch, int fIsStyle, nint pstyle, char* pwchTextBuffer, int cchTextBuffer, ref int fIsBufferUsed, out char* pwchText, ref int cchText, ref int fIsHidden, ref LsChp lschp, ref nint lsplsrun)
	{
		LsErr result = LsErr.None;
		pwchText = null;
		Plsrun plsrun = Plsrun.Undefined;
		LSRun lSRun = null;
		try
		{
			FullTextState fullText = FullText;
			TextStore textStore = fullText.StoreFrom(lscpFetch);
			lSRun = textStore.FetchLSRun(lscpFetch, fullText.TextFormattingMode, fullText.IsSideways, out plsrun, out var lsrunOffset, out cchText);
			fIsBufferUsed = 0;
			pwchText = lSRun.CharacterBuffer.GetCharacterPointer();
			if (pwchText == null)
			{
				if (cchText > cchTextBuffer)
				{
					return LsErr.None;
				}
				Invariant.Assert(pwchTextBuffer != null);
				int num = lSRun.OffsetToFirstChar + lsrunOffset;
				int num2 = 0;
				while (num2 < cchText)
				{
					pwchTextBuffer[num2] = lSRun.CharacterBuffer[num];
					num2++;
					num++;
				}
				fIsBufferUsed = 1;
			}
			else
			{
				pwchText += lSRun.OffsetToFirstChar + lsrunOffset;
			}
			lschp = default(LsChp);
			fIsHidden = 0;
			switch (lSRun.Type)
			{
			case Plsrun.Reverse:
				lschp.idObj = 0;
				break;
			case Plsrun.CloseAnchor:
			case Plsrun.FormatAnchor:
				lschp.idObj = ushort.MaxValue;
				break;
			case Plsrun.InlineObject:
				lschp.idObj = 1;
				SetChpFormat(lSRun.RunProp, ref lschp);
				break;
			case Plsrun.Hidden:
				lschp.idObj = ushort.MaxValue;
				fIsHidden = 1;
				break;
			case Plsrun.Text:
				lschp.idObj = ushort.MaxValue;
				if (lSRun.Shapeable != null && lSRun.Shapeable.IsShapingRequired)
				{
					lschp.flags |= LsChp.Flags.fGlyphBased;
					if (lSRun.Shapeable.NeedsMaxClusterSize)
					{
						lschp.dcpMaxContent = lSRun.Shapeable.MaxClusterSize;
					}
				}
				SetChpFormat(lSRun.RunProp, ref lschp);
				Invariant.Assert(!TextStore.IsNewline(lSRun.CharacterAttributeFlags));
				break;
			default:
				lschp.idObj = ushort.MaxValue;
				textStore.CchEol = lSRun.Length;
				break;
			}
			if ((lSRun.Type == Plsrun.Text || lSRun.Type == Plsrun.InlineObject) && lSRun.RunProp != null && lSRun.RunProp.BaselineAlignment != BaselineAlignment.Baseline)
			{
				FullText.VerticalAdjust = true;
			}
			lsplsrun = (nint)plsrun;
		}
		catch (Exception e)
		{
			SaveException(e, plsrun, lSRun);
			result = LsErr.ClientAbort;
		}
		catch
		{
			SaveNonCLSException("FetchRunRedefined", plsrun, lSRun);
			result = LsErr.ClientAbort;
		}
		return result;
	}

	private void SetChpFormat(TextRunProperties runProp, ref LsChp lschp)
	{
		SetChpFormat(runProp.TextDecorations, ref lschp);
		SetChpFormat(FullText.TextStore.Pap.TextDecorations, ref lschp);
	}

	private void SetChpFormat(TextDecorationCollection textDecorations, ref LsChp lschp)
	{
		if (textDecorations == null)
		{
			return;
		}
		for (int i = 0; i < textDecorations.Count; i++)
		{
			switch (textDecorations[i].Location)
			{
			case TextDecorationLocation.Underline:
				lschp.flags |= LsChp.Flags.fUnderline;
				break;
			case TextDecorationLocation.OverLine:
			case TextDecorationLocation.Strikethrough:
			case TextDecorationLocation.Baseline:
				lschp.flags |= LsChp.Flags.fStrike;
				break;
			}
		}
	}

	internal LsErr FetchPap(nint pols, int lscpFetch, ref LsPap lspap)
	{
		LsErr result = LsErr.None;
		try
		{
			lspap = default(LsPap);
			TextStore textStore = FullText.StoreFrom(lscpFetch);
			lspap.cpFirst = (lspap.cpFirstContent = lscpFetch);
			lspap.lskeop = LsKEOP.lskeopEndPara1;
			lspap.grpf |= LsPap.Flags.fFmiTreatHyphenAsRegular;
			ParaProp pap = textStore.Pap;
			if (FullText.ForceWrap)
			{
				lspap.grpf |= LsPap.Flags.fFmiApplyBreakingRules;
			}
			else if (pap.Wrap)
			{
				lspap.grpf |= LsPap.Flags.fFmiApplyBreakingRules;
				if (!pap.EmergencyWrap)
				{
					lspap.grpf |= LsPap.Flags.fFmiForceBreakAsNext;
				}
				if (pap.Hyphenator != null)
				{
					lspap.grpf |= LsPap.Flags.fFmiAllowHyphenation;
				}
			}
			if (pap.FirstLineInParagraph)
			{
				lspap.cpFirstContent = textStore.CpFirst;
				lspap.cpFirst = lspap.cpFirstContent;
				if (FullText.TextMarkerStore != null)
				{
					lspap.grpf |= LsPap.Flags.fFmiAnm;
				}
			}
			lspap.fJustify = (pap.Justify ? 1 : 0);
			if (pap.Wrap && pap.OptimalBreak)
			{
				lspap.lsbrj = LsBreakJust.lsbrjBreakOptimal;
				lspap.lskj = LsKJust.lskjFullMixed;
			}
			else
			{
				lspap.lsbrj = LsBreakJust.lsbrjBreakJustify;
				if (pap.Justify)
				{
					lspap.lskj = LsKJust.lskjFullInterWord;
				}
			}
			lspap.lstflow = (pap.RightToLeft ? LsTFlow.lstflowWS : LsTFlow.lstflowDefault);
		}
		catch (Exception e)
		{
			SaveException(e, Plsrun.Undefined, null);
			result = LsErr.ClientAbort;
		}
		catch
		{
			SaveNonCLSException("FetchPap", Plsrun.Undefined, null);
			result = LsErr.ClientAbort;
		}
		return result;
	}

	internal LsErr FetchLineProps(nint pols, int lscpFetch, int firstLineInPara, ref LsLineProps lsLineProps)
	{
		LsErr result = LsErr.None;
		try
		{
			TextStore textStore = FullText.TextStore;
			TextStore textMarkerStore = FullText.TextMarkerStore;
			ParaProp pap = textStore.Pap;
			FormatSettings settings = textStore.Settings;
			lsLineProps = default(LsLineProps);
			if (FullText.GetMainTextToMarkerIdealDistance() != 0)
			{
				lsLineProps.durLeft = TextFormatterImp.RealToIdeal(textMarkerStore.Pap.TextMarkerProperties.Offset);
			}
			else
			{
				lsLineProps.durLeft = settings.TextIndent;
			}
			if (pap.Wrap && pap.OptimalBreak && settings.MaxLineWidth < FullText.FormatWidth)
			{
				lsLineProps.durRightBreak = (lsLineProps.durRightJustify = FullText.FormatWidth - settings.MaxLineWidth);
			}
		}
		catch (Exception e)
		{
			SaveException(e, Plsrun.Undefined, null);
			result = LsErr.ClientAbort;
		}
		catch
		{
			SaveNonCLSException("FetchLineProps", Plsrun.Undefined, null);
			result = LsErr.ClientAbort;
		}
		return result;
	}

	internal LsErr GetRunTextMetrics(nint pols, Plsrun plsrun, LsDevice lsDevice, LsTFlow lstFlow, ref LsTxM lstTextMetrics)
	{
		LsErr result = LsErr.None;
		LSRun lSRun = null;
		try
		{
			FullTextState fullText = FullText;
			TextStore textStore = fullText.StoreFrom(plsrun);
			lSRun = textStore.GetRun(plsrun);
			if (lSRun.Height > 0)
			{
				lstTextMetrics.dvAscent = lSRun.BaselineOffset;
				lstTextMetrics.dvMultiLineHeight = lSRun.Height;
			}
			else
			{
				Typeface defaultTypeface = textStore.Pap.DefaultTypeface;
				lstTextMetrics.dvAscent = (int)Math.Round(defaultTypeface.Baseline(textStore.Pap.EmSize, 1.0 / 300.0, textStore.Settings.TextSource.PixelsPerDip, fullText.TextFormattingMode));
				lstTextMetrics.dvMultiLineHeight = (int)Math.Round(defaultTypeface.LineSpacing(textStore.Pap.EmSize, 1.0 / 300.0, textStore.Settings.TextSource.PixelsPerDip, fullText.TextFormattingMode));
			}
			lstTextMetrics.dvDescent = lstTextMetrics.dvMultiLineHeight - lstTextMetrics.dvAscent;
			lstTextMetrics.fMonospaced = 0;
		}
		catch (Exception e)
		{
			SaveException(e, plsrun, lSRun);
			result = LsErr.ClientAbort;
		}
		catch
		{
			SaveNonCLSException("GetRunTextMetrics", plsrun, lSRun);
			result = LsErr.ClientAbort;
		}
		return result;
	}

	internal unsafe LsErr GetRunCharWidths(nint pols, Plsrun plsrun, LsDevice device, char* charString, int stringLength, int maxWidth, LsTFlow textFlow, int* charWidths, ref int totalWidth, ref int stringLengthFitted)
	{
		LsErr result = LsErr.None;
		LSRun lSRun = null;
		try
		{
			if (FullText != null)
			{
				lSRun = FullText.StoreFrom(plsrun).GetRun(plsrun);
				_ = FullText.Formatter;
			}
			else
			{
				lSRun = Draw.CurrentLine.GetRun(plsrun);
				_ = Draw.CurrentLine.Formatter;
			}
			if (lSRun.Type == Plsrun.Text)
			{
				lSRun.Shapeable.GetAdvanceWidthsUnshaped(charString, stringLength, TextFormatterImp.ToIdeal, charWidths);
				totalWidth = 0;
				stringLengthFitted = 0;
				do
				{
					totalWidth += charWidths[stringLengthFitted];
				}
				while (++stringLengthFitted < stringLength && totalWidth <= maxWidth);
				if (totalWidth <= maxWidth && FullText != null)
				{
					int num = lSRun.OffsetToFirstCp + stringLengthFitted;
					if (num > FullText.CpMeasured)
					{
						FullText.CpMeasured = num;
					}
				}
			}
			else
			{
				*charWidths = 0;
				totalWidth = 0;
				stringLengthFitted = stringLength;
			}
		}
		catch (Exception e)
		{
			SaveException(e, plsrun, lSRun);
			result = LsErr.ClientAbort;
		}
		catch
		{
			SaveNonCLSException("GetRunCharWidths", plsrun, lSRun);
			result = LsErr.ClientAbort;
		}
		return result;
	}

	internal LsErr GetDurMaxExpandRagged(nint pols, Plsrun plsrun, LsTFlow lstFlow, ref int maxExpandRagged)
	{
		LsErr result = LsErr.None;
		LSRun lSRun = null;
		try
		{
			lSRun = FullText.StoreFrom(plsrun).GetRun(plsrun);
			maxExpandRagged = lSRun.EmSize;
		}
		catch (Exception e)
		{
			SaveException(e, plsrun, lSRun);
			result = LsErr.ClientAbort;
		}
		catch
		{
			SaveNonCLSException("GetDurMaxExpandRagged", plsrun, lSRun);
			result = LsErr.ClientAbort;
		}
		return result;
	}

	internal LsErr GetAutoNumberInfo(nint pols, ref LsKAlign alignment, ref LsChp lschp, ref nint lsplsrun, ref ushort addedChar, ref LsChp lschpAddedChar, ref nint lsplsrunAddedChar, ref int fWord95Model, ref int offset, ref int width)
	{
		LsErr result = LsErr.None;
		Plsrun plsrun = Plsrun.Undefined;
		LSRun lSRun = null;
		try
		{
			FullTextState fullText = FullText;
			TextStore textMarkerStore = fullText.TextMarkerStore;
			_ = fullText.TextStore;
			int num = -2147483647;
			do
			{
				lSRun = textMarkerStore.FetchLSRun(num, fullText.TextFormattingMode, fullText.IsSideways, out plsrun, out var _, out var lsrunLength);
				num += lsrunLength;
			}
			while (!TextStore.IsContent(plsrun));
			alignment = LsKAlign.lskalRight;
			lschp = default(LsChp);
			lschp.idObj = ushort.MaxValue;
			SetChpFormat(lSRun.RunProp, ref lschp);
			addedChar = (ushort)((FullText.GetMainTextToMarkerIdealDistance() != 0) ? 9 : 0);
			lschpAddedChar = lschp;
			fWord95Model = 0;
			offset = 0;
			width = 0;
			lsplsrun = (nint)plsrun;
			lsplsrunAddedChar = lsplsrun;
		}
		catch (Exception e)
		{
			SaveException(e, plsrun, lSRun);
			result = LsErr.ClientAbort;
		}
		catch
		{
			SaveNonCLSException("GetAutoNumberInfo", plsrun, lSRun);
			result = LsErr.ClientAbort;
		}
		return result;
	}

	internal LsErr GetRunUnderlineInfo(nint pols, Plsrun plsrun, ref LsHeights lsHeights, LsTFlow textFlow, ref LsULInfo ulInfo)
	{
		LsErr result = LsErr.None;
		LSRun lSRun = null;
		try
		{
			lSRun = Draw.CurrentLine.GetRun(plsrun);
			ulInfo = default(LsULInfo);
			double underlinePosition;
			double underlineThickness;
			if (lSRun.Shapeable != null)
			{
				underlinePosition = lSRun.Shapeable.UnderlinePosition;
				underlineThickness = lSRun.Shapeable.UnderlineThickness;
			}
			else
			{
				underlinePosition = lSRun.RunProp.Typeface.UnderlinePosition;
				underlineThickness = lSRun.RunProp.Typeface.UnderlineThickness;
			}
			ulInfo.cNumberOfLines = 1;
			ulInfo.dvpFirstUnderlineOffset = (int)Math.Round((double)lSRun.EmSize * (0.0 - underlinePosition));
			ulInfo.dvpFirstUnderlineSize = (int)Math.Round((double)lSRun.EmSize * underlineThickness);
			if (ulInfo.dvpFirstUnderlineSize <= 0)
			{
				ulInfo.dvpFirstUnderlineSize = 1;
			}
		}
		catch (Exception e)
		{
			SaveException(e, plsrun, lSRun);
			result = LsErr.ClientAbort;
		}
		catch
		{
			SaveNonCLSException("GetAutoNumberInfo", plsrun, lSRun);
			result = LsErr.ClientAbort;
		}
		return result;
	}

	internal LsErr GetRunStrikethroughInfo(nint pols, Plsrun plsrun, ref LsHeights lsHeights, LsTFlow textFlow, ref LsStInfo stInfo)
	{
		LsErr result = LsErr.None;
		LSRun lSRun = null;
		try
		{
			lSRun = Draw.CurrentLine.GetRun(plsrun);
			stInfo = default(LsStInfo);
			GetLSRunStrikethroughMetrics(lSRun, out var strikeThroughPositionInEm, out var strikeThroughThicknessInEm);
			stInfo.cNumberOfLines = 1;
			stInfo.dvpLowerStrikethroughOffset = (int)Math.Round((double)lSRun.EmSize * strikeThroughPositionInEm);
			stInfo.dvpLowerStrikethroughSize = (int)Math.Round((double)lSRun.EmSize * strikeThroughThicknessInEm);
			if (stInfo.dvpLowerStrikethroughSize <= 0)
			{
				stInfo.dvpLowerStrikethroughSize = 1;
			}
		}
		catch (Exception e)
		{
			SaveException(e, plsrun, lSRun);
			result = LsErr.ClientAbort;
		}
		catch
		{
			SaveNonCLSException("GetRunStrikethroughInfo", plsrun, lSRun);
			result = LsErr.ClientAbort;
		}
		return result;
	}

	private void GetLSRunStrikethroughMetrics(LSRun lsrun, out double strikeThroughPositionInEm, out double strikeThroughThicknessInEm)
	{
		if (lsrun.Shapeable != null)
		{
			strikeThroughPositionInEm = lsrun.Shapeable.StrikethroughPosition;
			strikeThroughThicknessInEm = lsrun.Shapeable.StrikethroughThickness;
		}
		else
		{
			strikeThroughPositionInEm = lsrun.RunProp.Typeface.StrikethroughPosition;
			strikeThroughThicknessInEm = lsrun.RunProp.Typeface.StrikethroughThickness;
		}
	}

	internal LsErr Hyphenate(nint pols, int fLastHyphenFound, int lscpLastHyphen, ref LsHyph lastHyph, int lscpWordStart, int lscpExceed, ref int fHyphenFound, ref int lscpHyphen, ref LsHyph lsHyph)
	{
		LsErr result = LsErr.None;
		try
		{
			fHyphenFound = (FullText.FindNextHyphenBreak(lscpWordStart, lscpExceed - lscpWordStart, isCurrentAtWordStart: true, ref lscpHyphen, ref lsHyph) ? 1 : 0);
			Invariant.Assert(fHyphenFound == 0 || (lscpHyphen >= lscpWordStart && lscpHyphen < lscpExceed));
		}
		catch (Exception e)
		{
			SaveException(e, Plsrun.Undefined, null);
			result = LsErr.ClientAbort;
		}
		catch
		{
			SaveNonCLSException("Hyphenate", Plsrun.Undefined, null);
			result = LsErr.ClientAbort;
		}
		return result;
	}

	internal LsErr GetNextHyphenOpp(nint pols, int lscpStartSearch, int lsdcpSearch, ref int fHyphenFound, ref int lscpHyphen, ref LsHyph lsHyph)
	{
		LsErr result = LsErr.None;
		try
		{
			fHyphenFound = (FullText.FindNextHyphenBreak(lscpStartSearch, lsdcpSearch, isCurrentAtWordStart: false, ref lscpHyphen, ref lsHyph) ? 1 : 0);
			Invariant.Assert(fHyphenFound == 0 || (lscpHyphen >= lscpStartSearch && lscpHyphen < lscpStartSearch + lsdcpSearch));
		}
		catch (Exception e)
		{
			SaveException(e, Plsrun.Undefined, null);
			result = LsErr.ClientAbort;
		}
		catch
		{
			SaveNonCLSException("GetNextHyphenOpp", Plsrun.Undefined, null);
			result = LsErr.ClientAbort;
		}
		return result;
	}

	internal LsErr GetPrevHyphenOpp(nint pols, int lscpStartSearch, int lsdcpSearch, ref int fHyphenFound, ref int lscpHyphen, ref LsHyph lsHyph)
	{
		LsErr result = LsErr.None;
		try
		{
			fHyphenFound = (FullText.FindNextHyphenBreak(lscpStartSearch + 1, -lsdcpSearch, isCurrentAtWordStart: false, ref lscpHyphen, ref lsHyph) ? 1 : 0);
			Invariant.Assert(fHyphenFound == 0 || (lscpHyphen > lscpStartSearch - lsdcpSearch && lscpHyphen <= lscpStartSearch));
		}
		catch (Exception e)
		{
			SaveException(e, Plsrun.Undefined, null);
			result = LsErr.ClientAbort;
		}
		catch
		{
			SaveNonCLSException("GetPrevHyphenOpp", Plsrun.Undefined, null);
			result = LsErr.ClientAbort;
		}
		return result;
	}

	internal LsErr DrawStrikethrough(nint pols, Plsrun plsrun, uint stType, ref LSPOINT ptOrigin, int stLength, int stThickness, LsTFlow textFlow, uint displayMode, ref LSRECT clipRect)
	{
		LsErr result = LsErr.None;
		LSRun lSRun = null;
		try
		{
			if (!TextStore.IsContent(plsrun))
			{
				return LsErr.None;
			}
			lSRun = Draw.CurrentLine.GetRun(plsrun);
			GetLSRunStrikethroughMetrics(lSRun, out var strikeThroughPositionInEm, out var strikeThroughThicknessInEm);
			int num = ptOrigin.y + (int)Math.Round((double)lSRun.EmSize * strikeThroughPositionInEm);
			int overlineTop = num - (lSRun.BaselineOffset - (int)Math.Round((double)lSRun.EmSize * strikeThroughThicknessInEm));
			DrawTextDecorations(lSRun, 14u, ptOrigin.x, 0, overlineTop, ptOrigin.y, num, stLength, stThickness, textFlow);
		}
		catch (Exception e)
		{
			SaveException(e, plsrun, lSRun);
			result = LsErr.ClientAbort;
		}
		catch
		{
			SaveNonCLSException("DrawStrikethrough", plsrun, lSRun);
			result = LsErr.ClientAbort;
		}
		return result;
	}

	internal LsErr DrawUnderline(nint pols, Plsrun plsrun, uint ulType, ref LSPOINT ptOrigin, int ulLength, int ulThickness, LsTFlow textFlow, uint displayMode, ref LSRECT clipRect)
	{
		LsErr result = LsErr.None;
		LSRun lsrun = null;
		try
		{
			if (!TextStore.IsContent(plsrun))
			{
				return LsErr.None;
			}
			lsrun = Draw.CurrentLine.GetRun(plsrun);
			DrawTextDecorations(lsrun, 1u, ptOrigin.x, ptOrigin.y, 0, 0, 0, ulLength, ulThickness, textFlow);
		}
		catch (Exception e)
		{
			SaveException(e, plsrun, lsrun);
			result = LsErr.ClientAbort;
		}
		catch
		{
			SaveNonCLSException("DrawUnderline", plsrun, lsrun);
			result = LsErr.ClientAbort;
		}
		return result;
	}

	private void DrawTextDecorations(LSRun lsrun, uint locationMask, int left, int underlineTop, int overlineTop, int strikethroughTop, int baselineTop, int length, int thickness, LsTFlow textFlow)
	{
		TextMetrics.FullTextLine currentLine = Draw.CurrentLine;
		TextDecorationCollection textDecorations = currentLine.TextDecorations;
		if (textDecorations != null)
		{
			DrawTextDecorationCollection(lsrun, locationMask, textDecorations, currentLine.DefaultTextDecorationsBrush, left, underlineTop, overlineTop, strikethroughTop, baselineTop, length, thickness, textFlow);
		}
		textDecorations = lsrun.RunProp.TextDecorations;
		if (textDecorations != null)
		{
			DrawTextDecorationCollection(lsrun, locationMask, textDecorations, lsrun.RunProp.ForegroundBrush, left, underlineTop, overlineTop, strikethroughTop, baselineTop, length, thickness, textFlow);
		}
	}

	private void DrawTextDecorationCollection(LSRun lsrun, uint locationMask, TextDecorationCollection textDecorations, Brush foregroundBrush, int left, int underlineTop, int overlineTop, int strikethroughTop, int baselineTop, int length, int thickness, LsTFlow textFlow)
	{
		Invariant.Assert(textDecorations != null);
		foreach (TextDecoration textDecoration in textDecorations)
		{
			if (((uint)(1 << (int)textDecoration.Location) & locationMask) != 0)
			{
				switch (textDecoration.Location)
				{
				case TextDecorationLocation.Underline:
					_boundingBox.Union(DrawTextDecoration(lsrun, foregroundBrush, new LSPOINT(left, underlineTop), length, thickness, textFlow, textDecoration));
					break;
				case TextDecorationLocation.OverLine:
					_boundingBox.Union(DrawTextDecoration(lsrun, foregroundBrush, new LSPOINT(left, overlineTop), length, thickness, textFlow, textDecoration));
					break;
				case TextDecorationLocation.Strikethrough:
					_boundingBox.Union(DrawTextDecoration(lsrun, foregroundBrush, new LSPOINT(left, strikethroughTop), length, thickness, textFlow, textDecoration));
					break;
				case TextDecorationLocation.Baseline:
					_boundingBox.Union(DrawTextDecoration(lsrun, foregroundBrush, new LSPOINT(left, baselineTop), length, thickness, textFlow, textDecoration));
					break;
				}
			}
		}
	}

	private Rect DrawTextDecoration(LSRun lsrun, Brush foregroundBrush, LSPOINT ptOrigin, int ulLength, int ulThickness, LsTFlow textFlow, TextDecoration textDecoration)
	{
		if (textFlow == LsTFlow.lstflowWS || (uint)(textFlow - 6) <= 1u)
		{
			ptOrigin.x -= ulLength;
		}
		TextMetrics.FullTextLine currentLine = Draw.CurrentLine;
		if (currentLine.RightToLeft)
		{
			ptOrigin.x = -ptOrigin.x;
		}
		int u = currentLine.LSLineUToParagraphU(ptOrigin.x);
		Point point = LSRun.UVToXY(Draw.LineOrigin, Draw.VectorToLineOrigin, u, currentLine.BaselineOffset, currentLine);
		Point point2 = LSRun.UVToXY(Draw.LineOrigin, Draw.VectorToLineOrigin, u, ptOrigin.y + lsrun.BaselineMoveOffset, currentLine);
		double num = 1.0;
		if (textDecoration.Pen != null)
		{
			num = textDecoration.Pen.Thickness;
		}
		switch (textDecoration.PenThicknessUnit)
		{
		case TextDecorationUnit.FontRecommended:
			num = currentLine.Formatter.IdealToReal((double)ulThickness * num, currentLine.PixelsPerDip);
			break;
		case TextDecorationUnit.FontRenderingEmSize:
			num = currentLine.Formatter.IdealToReal(num * (double)lsrun.EmSize, currentLine.PixelsPerDip);
			break;
		}
		num = Math.Abs(num);
		double num2 = 1.0;
		switch (textDecoration.PenOffsetUnit)
		{
		case TextDecorationUnit.FontRecommended:
			num2 = point2.Y - point.Y;
			break;
		case TextDecorationUnit.FontRenderingEmSize:
			num2 = currentLine.Formatter.IdealToReal(lsrun.EmSize, currentLine.PixelsPerDip);
			break;
		case TextDecorationUnit.Pixel:
			num2 = 1.0;
			break;
		}
		double num3 = currentLine.Formatter.IdealToReal(ulLength, currentLine.PixelsPerDip);
		DrawingContext drawingContext = Draw.DrawingContext;
		if (drawingContext != null)
		{
			double num4 = num;
			Point point3 = point2;
			bool flag = !textDecoration.CanFreeze && num2 != 0.0;
			int num5 = 0;
			Draw.SetGuidelineY(point.Y);
			try
			{
				if (flag)
				{
					ScaleTransform transform = new ScaleTransform(1.0, num2, point3.X, point3.Y);
					TranslateTransform transform2 = new TranslateTransform(0.0, textDecoration.PenOffset);
					num4 /= Math.Abs(num2);
					drawingContext.PushTransform(transform);
					num5++;
					drawingContext.PushTransform(transform2);
					num5++;
				}
				else
				{
					point3.Y += num2 * textDecoration.PenOffset;
				}
				drawingContext.PushGuidelineY2(point.Y, point3.Y - num4 * 0.5 - point.Y);
				num5++;
				if (textDecoration.Pen == null)
				{
					drawingContext.DrawRectangle(foregroundBrush, null, new Rect(point3.X, point3.Y - num4 * 0.5, num3, num4));
				}
				else
				{
					Pen pen = textDecoration.Pen.CloneCurrentValue();
					if (textDecoration.Pen == pen)
					{
						pen = textDecoration.Pen.Clone();
					}
					pen.Thickness = num4;
					drawingContext.DrawLine(pen, point3, new Point(point3.X + num3, point3.Y));
				}
			}
			finally
			{
				for (int i = 0; i < num5; i++)
				{
					drawingContext.Pop();
				}
				Draw.UnsetGuidelineY();
			}
		}
		return new Rect(point2.X, point2.Y + num2 * textDecoration.PenOffset - num * 0.5, num3, num);
	}

	internal unsafe LsErr DrawTextRun(nint pols, Plsrun plsrun, ref LSPOINT ptText, char* pwchText, int* piCharAdvances, int cchText, LsTFlow textFlow, uint displayMode, ref LSPOINT ptRun, ref LsHeights lsHeights, int dupRun, ref LSRECT clipRect)
	{
		LsErr result = LsErr.None;
		LSRun lSRun = null;
		try
		{
			TextMetrics.FullTextLine currentLine = Draw.CurrentLine;
			lSRun = currentLine.GetRun(plsrun);
			GlyphRun glyphRun = ComputeUnshapedGlyphRun(lSRun, textFlow, currentLine.Formatter, originProvided: true, ptText, dupRun, cchText, pwchText, piCharAdvances, currentLine.IsJustified);
			if (glyphRun != null)
			{
				DrawingContext drawingContext = Draw.DrawingContext;
				Draw.SetGuidelineY(glyphRun.BaselineOrigin.Y);
				try
				{
					_boundingBox.Union(lSRun.DrawGlyphRun(drawingContext, null, glyphRun));
				}
				finally
				{
					Draw.UnsetGuidelineY();
				}
			}
		}
		catch (Exception e)
		{
			SaveException(e, plsrun, lSRun);
			result = LsErr.ClientAbort;
		}
		catch
		{
			SaveNonCLSException("DrawTextRun", plsrun, lSRun);
			result = LsErr.ClientAbort;
		}
		return result;
	}

	internal LsErr FInterruptShaping(nint pols, LsTFlow textFlow, Plsrun plsrunFirst, Plsrun plsrunSecond, ref int fIsInterruptOk)
	{
		LsErr result = LsErr.None;
		try
		{
			TextStore textStore = FullText.StoreFrom(plsrunFirst);
			if (!TextStore.IsContent(plsrunFirst) || !TextStore.IsContent(plsrunSecond))
			{
				fIsInterruptOk = 1;
				return LsErr.None;
			}
			LSRun run = textStore.GetRun(plsrunFirst);
			LSRun run2 = textStore.GetRun(plsrunSecond);
			fIsInterruptOk = ((run.BidiLevel != run2.BidiLevel || run.Shapeable == null || run2.Shapeable == null || !run.Shapeable.CanShapeTogether(run2.Shapeable)) ? 1 : 0);
		}
		catch (Exception e)
		{
			SaveException(e, Plsrun.Undefined, null);
			result = LsErr.ClientAbort;
		}
		catch
		{
			SaveNonCLSException("FInterruptShaping", Plsrun.Undefined, null);
			result = LsErr.ClientAbort;
		}
		return result;
	}

	internal static CultureInfo GetNumberCulture(TextRunProperties properties, out NumberSubstitutionMethod method)
	{
		NumberSubstitution numberSubstitution = properties.NumberSubstitution;
		if (numberSubstitution == null)
		{
			method = NumberSubstitutionMethod.AsCulture;
			return CultureMapper.GetSpecificCulture(properties.CultureInfo);
		}
		method = numberSubstitution.Substitution;
		return numberSubstitution.CultureSource switch
		{
			NumberCultureSource.Text => CultureMapper.GetSpecificCulture(properties.CultureInfo), 
			NumberCultureSource.User => CultureInfo.CurrentCulture, 
			NumberCultureSource.Override => numberSubstitution.CultureOverride, 
			_ => null, 
		};
	}

	internal unsafe LsErr GetGlyphsRedefined(nint pols, nint* plsplsruns, int* pcchPlsrun, int plsrunCount, char* pwchText, int cchText, LsTFlow textFlow, ushort* puGlyphsBuffer, uint* piGlyphPropsBuffer, int cgiGlyphBuffers, ref int fIsGlyphBuffersUsed, ushort* puClusterMap, ushort* puCharProperties, int* pfCanGlyphAlone, ref int glyphCount)
	{
		Invariant.Assert(puGlyphsBuffer != null && piGlyphPropsBuffer != null);
		LsErr result = LsErr.None;
		LSRun lSRun = null;
		try
		{
			LSRun[] array = RemapLSRuns(plsplsruns, plsrunCount);
			lSRun = array[0];
			bool isRightToLeft = (lSRun.BidiLevel & 1) != 0;
			checked
			{
				uint num = (uint)cchText;
				LSRun.CompileFeatureSet(array, pcchPlsrun, num, out var fontFeatures, out var fontFeatureRanges);
				GlyphTypeface glyphTypeFace = lSRun.Shapeable.GlyphTypeFace;
				FullText.Formatter.TextAnalyzer.GetGlyphs(pwchText, num, glyphTypeFace.FontDWrite, glyphTypeFace.BlankGlyphIndex, isSideways: false, isRightToLeft, lSRun.RunProp.CultureInfo, fontFeatures, fontFeatureRanges, (uint)cgiGlyphBuffers, FullText.TextFormattingMode, lSRun.Shapeable.ItemProps, puClusterMap, puCharProperties, puGlyphsBuffer, piGlyphPropsBuffer, pfCanGlyphAlone, out var actualGlyphCount);
				glyphCount = (int)actualGlyphCount;
				if (glyphCount <= cgiGlyphBuffers)
				{
					fIsGlyphBuffersUsed = 1;
				}
				else
				{
					fIsGlyphBuffersUsed = 0;
				}
			}
		}
		catch (Exception e)
		{
			SaveException(e, (Plsrun)(*plsplsruns), lSRun);
			result = LsErr.ClientAbort;
		}
		catch
		{
			SaveNonCLSException("GetGlyphsRedefined", (Plsrun)(*plsplsruns), lSRun);
			result = LsErr.ClientAbort;
		}
		return result;
	}

	internal unsafe LsErr GetGlyphPositions(nint pols, nint* plsplsruns, int* pcchPlsrun, int plsrunCount, LsDevice device, char* pwchText, ushort* puClusterMap, ushort* puCharProperties, int cchText, ushort* puGlyphs, uint* piGlyphProperties, int glyphCount, LsTFlow textFlow, int* piGlyphAdvances, GlyphOffset* piiGlyphOffsets)
	{
		LsErr result = LsErr.None;
		LSRun lSRun = null;
		try
		{
			LSRun[] array = RemapLSRuns(plsplsruns, plsrunCount);
			lSRun = array[0];
			bool isRightToLeft = (lSRun.BidiLevel & 1) != 0;
			GlyphTypeface glyphTypeFace = lSRun.Shapeable.GlyphTypeFace;
			LSRun.CompileFeatureSet(array, pcchPlsrun, checked((uint)cchText), out var fontFeatures, out var fontFeatureRanges);
			FullText.Formatter.TextAnalyzer.GetGlyphPlacements(pwchText, puClusterMap, puCharProperties, (uint)cchText, puGlyphs, piGlyphProperties, (uint)glyphCount, glyphTypeFace.FontDWrite, lSRun.Shapeable.EmSize, TextFormatterImp.ToIdeal, isSideways: false, isRightToLeft, lSRun.RunProp.CultureInfo, fontFeatures, fontFeatureRanges, FullText.TextFormattingMode, lSRun.Shapeable.ItemProps, (float)FullText.StoreFrom(lSRun.Type).Settings.TextSource.PixelsPerDip, piGlyphAdvances, out var glyphOffsets);
			for (int i = 0; i < glyphCount; i++)
			{
				piiGlyphOffsets[i].du = glyphOffsets[i].du;
				piiGlyphOffsets[i].dv = glyphOffsets[i].dv;
			}
		}
		catch (Exception e)
		{
			SaveException(e, (Plsrun)(*plsplsruns), lSRun);
			result = LsErr.ClientAbort;
		}
		catch
		{
			SaveNonCLSException("GetGlyphPositions", (Plsrun)(*plsplsruns), lSRun);
			result = LsErr.ClientAbort;
		}
		return result;
	}

	private unsafe LSRun[] RemapLSRuns(nint* plsplsruns, int plsrunCount)
	{
		LSRun[] array = new LSRun[plsrunCount];
		TextStore textStore = FullText.StoreFrom((Plsrun)(*plsplsruns));
		for (int i = 0; i < array.Length; i++)
		{
			Plsrun plsrun = (Plsrun)plsplsruns[i];
			array[i] = textStore.GetRun(plsrun);
		}
		return array;
	}

	internal unsafe LsErr DrawGlyphs(nint pols, Plsrun plsrun, char* pwchText, ushort* puClusterMap, ushort* puCharProperties, int charCount, ushort* puGlyphs, int* piJustifiedGlyphAdvances, int* piGlyphAdvances, GlyphOffset* piiGlyphOffsets, uint* piGlyphProperties, LsExpType* plsExpType, int glyphCount, LsTFlow textFlow, uint displayMode, ref LSPOINT ptRun, ref LsHeights lsHeights, int runWidth, ref LSRECT clippingRect)
	{
		LsErr result = LsErr.None;
		LSRun lSRun = null;
		try
		{
			TextMetrics.FullTextLine currentLine = Draw.CurrentLine;
			lSRun = currentLine.GetRun(plsrun);
			GlyphRun glyphRun = ComputeShapedGlyphRun(lSRun, currentLine.Formatter, originProvided: true, ptRun, charCount, pwchText, puClusterMap, glyphCount, puGlyphs, piJustifiedGlyphAdvances, piiGlyphOffsets, currentLine.IsJustified);
			if (glyphRun != null)
			{
				DrawingContext drawingContext = Draw.DrawingContext;
				Draw.SetGuidelineY(glyphRun.BaselineOrigin.Y);
				try
				{
					_boundingBox.Union(lSRun.DrawGlyphRun(drawingContext, null, glyphRun));
				}
				finally
				{
					Draw.UnsetGuidelineY();
				}
			}
		}
		catch (Exception e)
		{
			SaveException(e, plsrun, lSRun);
			result = LsErr.ClientAbort;
		}
		catch
		{
			SaveNonCLSException("DrawGlyphs", plsrun, lSRun);
			result = LsErr.ClientAbort;
		}
		return result;
	}

	internal unsafe LsErr GetCharCompressionInfoFullMixed(nint pols, LsDevice device, LsTFlow textFlow, LsCharRunInfo* plscharrunInfo, LsNeighborInfo* plsneighborInfoLeft, LsNeighborInfo* plsneighborInfoRight, int maxPriorityLevel, int** pplscompressionLeft, int** pplscompressionRight)
	{
		LsErr lsErr = LsErr.None;
		Plsrun plsrun = Plsrun.Undefined;
		LSRun lSRun = null;
		try
		{
			Invariant.Assert(maxPriorityLevel == 3);
			plsrun = plscharrunInfo->plsrun;
			lSRun = FullText.StoreFrom(plsrun).GetRun(plsrun);
			return AdjustChars(plscharrunInfo, expanding: false, (int)((double)lSRun.EmSize * 0.2), pplscompressionLeft, pplscompressionRight);
		}
		catch (Exception e)
		{
			SaveException(e, plsrun, lSRun);
			return LsErr.ClientAbort;
		}
		catch
		{
			SaveNonCLSException("GetCharCompressionInfoFullMixed", plsrun, lSRun);
			return LsErr.ClientAbort;
		}
	}

	internal unsafe LsErr GetCharExpansionInfoFullMixed(nint pols, LsDevice device, LsTFlow textFlow, LsCharRunInfo* plscharrunInfo, LsNeighborInfo* plsneighborInfoLeft, LsNeighborInfo* plsneighborInfoRight, int maxPriorityLevel, int** pplsexpansionLeft, int** pplsexpansionRight)
	{
		LsErr lsErr = LsErr.None;
		Plsrun plsrun = Plsrun.Undefined;
		LSRun lSRun = null;
		try
		{
			Invariant.Assert(maxPriorityLevel == 3);
			plsrun = plscharrunInfo->plsrun;
			lSRun = FullText.StoreFrom(plsrun).GetRun(plsrun);
			return AdjustChars(plscharrunInfo, expanding: true, (int)((double)lSRun.EmSize * 0.5), pplsexpansionLeft, pplsexpansionRight);
		}
		catch (Exception e)
		{
			SaveException(e, plsrun, lSRun);
			return LsErr.ClientAbort;
		}
		catch
		{
			SaveNonCLSException("GetCharExpansionInfoFullMixed", plsrun, lSRun);
			return LsErr.ClientAbort;
		}
	}

	private unsafe LsErr AdjustChars(LsCharRunInfo* plscharrunInfo, bool expanding, int interWordAdjustTo, int** pplsAdjustLeft, int** pplsAdjustRight)
	{
		char* pwch = plscharrunInfo->pwch;
		int cwch = plscharrunInfo->cwch;
		for (int i = 0; i < cwch; i++)
		{
			int num = plscharrunInfo->rgduNominalWidth[i] + plscharrunInfo->rgduChangeLeft[i] + plscharrunInfo->rgduChangeRight[i];
			(*pplsAdjustLeft)[i] = 0;
			pplsAdjustLeft[1][i] = 0;
			pplsAdjustLeft[2][i] = 0;
			(*pplsAdjustRight)[i] = 0;
			pplsAdjustRight[1][i] = 0;
			pplsAdjustRight[2][i] = 0;
			if ((Classification.CharAttributeOf(Classification.GetUnicodeClassUTF16(pwch[i])).Flags & 0x80) != 0)
			{
				if (expanding)
				{
					int num2 = Math.Max(0, interWordAdjustTo - num);
					(*pplsAdjustRight)[i] = num2;
					pplsAdjustRight[1][i] = num2 * 2;
					pplsAdjustRight[2][i] = FullText.FormatWidth;
				}
				else
				{
					(*pplsAdjustRight)[i] = Math.Max(0, num - interWordAdjustTo);
				}
			}
			else if (expanding)
			{
				pplsAdjustRight[2][i] = FullText.FormatWidth;
			}
		}
		return LsErr.None;
	}

	internal unsafe LsErr GetGlyphCompressionInfoFullMixed(nint pols, LsDevice device, LsTFlow textFlow, LsGlyphRunInfo* plsglyphrunInfo, LsNeighborInfo* plsneighborInfoLeft, LsNeighborInfo* plsneighborInfoRight, int maxPriorityLevel, int** pplscompressionLeft, int** pplscompressionRight)
	{
		LsErr lsErr = LsErr.None;
		Plsrun plsrun = Plsrun.Undefined;
		LSRun lSRun = null;
		try
		{
			Invariant.Assert(maxPriorityLevel == 3);
			plsrun = plsglyphrunInfo->plsrun;
			lSRun = FullText.StoreFrom(plsrun).GetRun(plsrun);
			int emSize = lSRun.EmSize;
			return CompressGlyphs(plsglyphrunInfo, (int)((double)emSize * 0.2), pplscompressionLeft, pplscompressionRight);
		}
		catch (Exception e)
		{
			SaveException(e, plsrun, lSRun);
			return LsErr.ClientAbort;
		}
		catch
		{
			SaveNonCLSException("GetGlyphCompressionInfoFullMixed", plsrun, lSRun);
			return LsErr.ClientAbort;
		}
	}

	private unsafe LsErr CompressGlyphs(LsGlyphRunInfo* plsglyphrunInfo, int interWordCompressTo, int** pplsCompressionLeft, int** pplsCompressionRight)
	{
		char* pwch = plsglyphrunInfo->pwch;
		ushort* rggmap = plsglyphrunInfo->rggmap;
		int cwch = plsglyphrunInfo->cwch;
		int cgindex = plsglyphrunInfo->cgindex;
		int num = 0;
		int num2 = rggmap[num];
		int num3 = 0;
		while (num < cwch)
		{
			int i;
			for (i = 1; num + i < cwch && rggmap[num + i] == num2; i++)
			{
			}
			num3 = ((num + i == cwch) ? (cgindex - num2) : (rggmap[num + i] - num2));
			int j;
			for (j = 0; j < i && (Classification.CharAttributeOf(Classification.GetUnicodeClassUTF16(pwch[num + j])).Flags & 0x80) == 0; j++)
			{
			}
			int num4 = 0;
			for (int k = 0; k < num3; k++)
			{
				num4 += plsglyphrunInfo->rgduWidth[num2 + k];
				(*pplsCompressionLeft)[num2 + k] = 0;
				pplsCompressionLeft[1][num2 + k] = 0;
				pplsCompressionLeft[2][num2 + k] = 0;
				(*pplsCompressionRight)[num2 + k] = 0;
				pplsCompressionRight[1][num2 + k] = 0;
				pplsCompressionRight[2][num2 + k] = 0;
				if (k == num3 - 1 && i == 1 && j < i)
				{
					(*pplsCompressionRight)[num2 + k] = Math.Max(0, num4 - interWordCompressTo);
				}
			}
			num += i;
			num2 += num3;
		}
		Invariant.Assert(num2 == cgindex);
		return LsErr.None;
	}

	internal unsafe LsErr GetGlyphExpansionInfoFullMixed(nint pols, LsDevice device, LsTFlow textFlow, LsGlyphRunInfo* plsglyphrunInfo, LsNeighborInfo* plsneighborInfoLeft, LsNeighborInfo* plsneighborInfoRight, int maxPriorityLevel, int** pplsexpansionLeft, int** pplsexpansionRight, LsExpType* plsexptype, int* pduMinInk)
	{
		LsErr lsErr = LsErr.None;
		Plsrun plsrun = Plsrun.Undefined;
		LSRun lSRun = null;
		try
		{
			Invariant.Assert(maxPriorityLevel == 3);
			plsrun = plsglyphrunInfo->plsrun;
			lSRun = FullText.StoreFrom(plsrun).GetRun(plsrun);
			int emSize = lSRun.EmSize;
			return ExpandGlyphs(plsglyphrunInfo, (int)((double)emSize * 0.5), pplsexpansionLeft, pplsexpansionRight, plsexptype, LsExpType.AddWhiteSpace, ((lSRun.BidiLevel & 1) == 0) ? LsExpType.AddWhiteSpace : LsExpType.None);
		}
		catch (Exception e)
		{
			SaveException(e, plsrun, lSRun);
			return LsErr.ClientAbort;
		}
		catch
		{
			SaveNonCLSException("GetGlyphExpansionInfoFullMixed", plsrun, lSRun);
			return LsErr.ClientAbort;
		}
	}

	private unsafe LsErr ExpandGlyphs(LsGlyphRunInfo* plsglyphrunInfo, int interWordExpandTo, int** pplsExpansionLeft, int** pplsExpansionRight, LsExpType* plsexptype, LsExpType interWordExpansionType, LsExpType interLetterExpansionType)
	{
		char* pwch = plsglyphrunInfo->pwch;
		ushort* rggmap = plsglyphrunInfo->rggmap;
		int cwch = plsglyphrunInfo->cwch;
		int cgindex = plsglyphrunInfo->cgindex;
		int num = 0;
		int num2 = rggmap[num];
		int num3 = 0;
		while (num < cwch)
		{
			int i;
			for (i = 1; num + i < cwch && rggmap[num + i] == num2; i++)
			{
			}
			num3 = ((num + i == cwch) ? (cgindex - num2) : (rggmap[num + i] - num2));
			int j;
			for (j = 0; j < i && (Classification.CharAttributeOf(Classification.GetUnicodeClassUTF16(pwch[num + j])).Flags & 0x80) == 0; j++)
			{
			}
			int num4 = 0;
			for (int k = 0; k < num3; k++)
			{
				num4 += plsglyphrunInfo->rgduWidth[num2 + k];
				(*pplsExpansionLeft)[num2 + k] = 0;
				pplsExpansionLeft[1][num2 + k] = 0;
				pplsExpansionLeft[2][num2 + k] = 0;
				(*pplsExpansionRight)[num2 + k] = 0;
				pplsExpansionRight[1][num2 + k] = 0;
				pplsExpansionRight[2][num2 + k] = 0;
				if (k == num3 - 1)
				{
					if (i == 1 && j < i)
					{
						int num5 = Math.Max(0, interWordExpandTo - num4);
						(*pplsExpansionRight)[num2 + k] = num5;
						pplsExpansionRight[1][num2 + k] = num5 * 2;
						pplsExpansionRight[2][num2 + k] = FullText.FormatWidth;
						plsexptype[num2 + k] = interWordExpansionType;
					}
					else
					{
						pplsExpansionRight[2][num2 + k] = FullText.FormatWidth;
						plsexptype[num2 + k] = interLetterExpansionType;
					}
				}
			}
			num += i;
			num2 += num3;
		}
		Invariant.Assert(num2 == cgindex);
		return LsErr.None;
	}

	internal unsafe LsErr GetObjectHandlerInfo(nint pols, uint objectId, void* objectInfo)
	{
		LsErr result = LsErr.None;
		try
		{
			switch (objectId)
			{
			case 0u:
				return UnsafeNativeMethods.LocbkGetObjectHandlerInfo(pols, objectId, objectInfo);
			case 1u:
			{
				InlineInit structure = default(InlineInit);
				structure.pfnFormat = InlineFormatDelegate;
				structure.pfnDraw = InlineDrawDelegate;
				Marshal.StructureToPtr(structure, (nint)objectInfo, fDeleteOld: false);
				break;
			}
			}
		}
		catch (Exception e)
		{
			SaveException(e, Plsrun.Undefined, null);
			result = LsErr.ClientAbort;
		}
		catch
		{
			SaveNonCLSException("GetObjectHandlerInfo", Plsrun.Undefined, null);
			result = LsErr.ClientAbort;
		}
		return result;
	}

	internal LsErr InlineFormat(nint pols, Plsrun plsrun, int lscpInline, int currentPosition, int rightMargin, ref ObjDim pobjDim, out int fFirstRealOnLine, out int fPenPositionUsed, out LsBrkCond breakBefore, out LsBrkCond breakAfter)
	{
		LsErr result = LsErr.None;
		LSRun lSRun = null;
		fFirstRealOnLine = 0;
		fPenPositionUsed = 0;
		breakBefore = LsBrkCond.Please;
		breakAfter = LsBrkCond.Please;
		try
		{
			_ = FullText.Formatter;
			TextStore textStore = FullText.StoreFrom(plsrun);
			lSRun = textStore.GetRun(plsrun);
			TextEmbeddedObject textEmbeddedObject = lSRun.TextRun as TextEmbeddedObject;
			int externalCp = textStore.GetExternalCp(lscpInline);
			fFirstRealOnLine = ((externalCp == textStore.CpFirst) ? 1 : 0);
			TextEmbeddedObjectMetrics textEmbeddedObjectMetrics = textStore.FormatTextObject(textEmbeddedObject, externalCp, currentPosition, rightMargin);
			pobjDim = default(ObjDim);
			pobjDim.dur = TextFormatterImp.RealToIdeal(textEmbeddedObjectMetrics.Width);
			pobjDim.heightsRef.dvMultiLineHeight = TextFormatterImp.RealToIdeal(textEmbeddedObjectMetrics.Height);
			pobjDim.heightsRef.dvAscent = TextFormatterImp.RealToIdeal(textEmbeddedObjectMetrics.Baseline);
			pobjDim.heightsRef.dvDescent = pobjDim.heightsRef.dvMultiLineHeight - pobjDim.heightsRef.dvAscent;
			pobjDim.heightsPres = pobjDim.heightsRef;
			breakBefore = BreakConditionToLsBrkCond(textEmbeddedObject.BreakBefore);
			breakAfter = BreakConditionToLsBrkCond(textEmbeddedObject.BreakAfter);
			fPenPositionUsed = ((!textEmbeddedObject.HasFixedSize) ? 1 : 0);
			lSRun.BaselineOffset = pobjDim.heightsRef.dvAscent;
			lSRun.Height = pobjDim.heightsRef.dvMultiLineHeight;
		}
		catch (Exception e)
		{
			SaveException(e, plsrun, lSRun);
			result = LsErr.ClientAbort;
		}
		catch
		{
			SaveNonCLSException("InlineFormat", plsrun, lSRun);
			result = LsErr.ClientAbort;
		}
		return result;
	}

	private LsBrkCond BreakConditionToLsBrkCond(LineBreakCondition breakCondition)
	{
		return breakCondition switch
		{
			LineBreakCondition.BreakDesired => LsBrkCond.Please, 
			LineBreakCondition.BreakPossible => LsBrkCond.Can, 
			LineBreakCondition.BreakRestrained => LsBrkCond.Never, 
			LineBreakCondition.BreakAlways => LsBrkCond.Must, 
			_ => LsBrkCond.Please, 
		};
	}

	internal LsErr InlineDraw(nint pols, Plsrun plsrun, ref LSPOINT ptRun, LsTFlow textFlow, int runWidth)
	{
		LsErr result = LsErr.None;
		LSRun lSRun = null;
		try
		{
			TextMetrics.FullTextLine currentLine = Draw.CurrentLine;
			lSRun = currentLine.GetRun(plsrun);
			LSPOINT lSPOINT = ptRun;
			int num = (currentLine.RightToLeft ? 1 : 0);
			int num2 = lSRun.BidiLevel & 1;
			if (num != 0)
			{
				lSPOINT.x = -lSPOINT.x;
			}
			TextEmbeddedObject textEmbeddedObject = lSRun.TextRun as TextEmbeddedObject;
			if ((num ^ num2) != 0)
			{
				lSPOINT.x -= runWidth;
			}
			Point origin = new Point(currentLine.Formatter.IdealToReal(currentLine.LSLineUToParagraphU(lSPOINT.x), currentLine.PixelsPerDip) + Draw.VectorToLineOrigin.X, currentLine.Formatter.IdealToReal(lSPOINT.y + lSRun.BaselineMoveOffset, currentLine.PixelsPerDip) + Draw.VectorToLineOrigin.Y);
			Rect rect = textEmbeddedObject.ComputeBoundingBox(num != 0, sideways: false);
			if (!rect.IsEmpty)
			{
				rect.X += origin.X;
				rect.Y += origin.Y;
			}
			_boundingBox.Union(new Rect(LSRun.UVToXY(Draw.LineOrigin, default(Point), rect.Location.X, rect.Location.Y, currentLine), LSRun.UVToXY(Draw.LineOrigin, default(Point), rect.Location.X + rect.Size.Width, rect.Location.Y + rect.Size.Height, currentLine)));
			DrawingContext drawingContext = Draw.DrawingContext;
			if (drawingContext != null)
			{
				Draw.SetGuidelineY(origin.Y);
				try
				{
					if (Draw.AntiInversion == null)
					{
						textEmbeddedObject.Draw(drawingContext, LSRun.UVToXY(Draw.LineOrigin, default(Point), origin.X, origin.Y, currentLine), num != 0, sideways: false);
					}
					else
					{
						drawingContext.PushTransform(Draw.AntiInversion);
						try
						{
							textEmbeddedObject.Draw(drawingContext, origin, num != 0, sideways: false);
						}
						finally
						{
							drawingContext.Pop();
						}
					}
				}
				finally
				{
					Draw.UnsetGuidelineY();
				}
			}
		}
		catch (Exception e)
		{
			SaveException(e, plsrun, lSRun);
			result = LsErr.ClientAbort;
		}
		catch
		{
			SaveNonCLSException("InlineDraw", plsrun, lSRun);
			result = LsErr.ClientAbort;
		}
		return result;
	}

	internal unsafe LsErr EnumText(nint pols, Plsrun plsrun, int cpFirst, int dcp, char* pwchText, int cchText, LsTFlow lstFlow, int fReverseOrder, int fGeometryProvided, ref LSPOINT pptStart, ref LsHeights pheights, int dupRun, int glyphBaseRun, int* piCharAdvances, ushort* puClusterMap, ushort* characterProperties, ushort* puGlyphs, int* piJustifiedGlyphAdvances, GlyphOffset* piiGlyphOffsets, uint* piGlyphProperties, int glyphCount)
	{
		if (cpFirst < 0)
		{
			return LsErr.None;
		}
		LsErr result = LsErr.None;
		LSRun lsrun = null;
		try
		{
			TextMetrics.FullTextLine currentLine = Draw.CurrentLine;
			lsrun = currentLine.GetRun(plsrun);
			GlyphRun glyphRun = null;
			if (glyphBaseRun != 0)
			{
				if (glyphCount > 0)
				{
					glyphRun = ComputeShapedGlyphRun(lsrun, currentLine.Formatter, originProvided: false, pptStart, cchText, pwchText, puClusterMap, glyphCount, puGlyphs, piJustifiedGlyphAdvances, piiGlyphOffsets, currentLine.IsJustified);
				}
			}
			else if (cchText > 0)
			{
				dupRun = 0;
				for (int i = 0; i < cchText; i++)
				{
					dupRun += piCharAdvances[i];
				}
				glyphRun = ComputeUnshapedGlyphRun(lsrun, lstFlow, currentLine.Formatter, originProvided: false, pptStart, dupRun, cchText, pwchText, piCharAdvances, currentLine.IsJustified);
			}
			if (glyphRun != null)
			{
				IndexedGlyphRuns.Add(new IndexedGlyphRun(currentLine.GetExternalCp(cpFirst), dcp, glyphRun));
			}
		}
		catch (Exception e)
		{
			SaveException(e, plsrun, lsrun);
			result = LsErr.ClientAbort;
		}
		catch
		{
			SaveNonCLSException("EnumText", plsrun, lsrun);
			result = LsErr.ClientAbort;
		}
		return result;
	}

	internal unsafe LsErr EnumTab(nint pols, Plsrun plsrun, int cpFirst, char* pwchText, char tabLeader, LsTFlow lstFlow, int fReverseOrder, int fGeometryProvided, ref LSPOINT pptStart, ref LsHeights heights, int dupRun)
	{
		if (cpFirst < 0)
		{
			return LsErr.None;
		}
		LsErr result = LsErr.None;
		LSRun lSRun = null;
		try
		{
			TextMetrics.FullTextLine currentLine = Draw.CurrentLine;
			lSRun = currentLine.GetRun(plsrun);
			GlyphRun glyphRun = null;
			if (lSRun.Type == Plsrun.Text)
			{
				int dupRun2 = 0;
				lSRun.Shapeable.GetAdvanceWidthsUnshaped(&tabLeader, 1, TextFormatterImp.ToIdeal, &dupRun2);
				glyphRun = ComputeUnshapedGlyphRun(lSRun, lstFlow, currentLine.Formatter, originProvided: false, pptStart, dupRun2, 1, &tabLeader, &dupRun2, currentLine.IsJustified);
			}
			if (glyphRun != null)
			{
				IndexedGlyphRuns.Add(new IndexedGlyphRun(currentLine.GetExternalCp(cpFirst), 1, glyphRun));
			}
		}
		catch (Exception e)
		{
			SaveException(e, plsrun, lSRun);
			result = LsErr.ClientAbort;
		}
		catch
		{
			SaveNonCLSException("EnumTab", plsrun, lSRun);
			result = LsErr.ClientAbort;
		}
		return result;
	}

	private bool IsSpace(char ch)
	{
		if (ch == '\t' || ch == ' ')
		{
			return true;
		}
		return false;
	}

	private static int RealToIdeal(double i)
	{
		return TextFormatterImp.RealToIdeal(i);
	}

	private static double RoundDipForDisplayModeJustifiedText(double value, double pixelsPerDip)
	{
		return TextFormatterImp.RoundDipForDisplayModeJustifiedText(value, pixelsPerDip);
	}

	private static double IdealToRealWithNoRounding(double i)
	{
		return TextFormatterImp.IdealToRealWithNoRounding(i);
	}

	private unsafe void AdjustMetricsForDisplayModeJustifiedText(char* pwchText, int* piGlyphAdvances, int glyphCount, bool isRightToLeft, int idealBaselineOriginX, int idealBaselineOriginY, double pixelsPerDip, out Point baselineOrigin, out IList<double> adjustedAdvanceWidths)
	{
		adjustedAdvanceWidths = new double[glyphCount];
		baselineOrigin = new Point(RoundDipForDisplayModeJustifiedText(IdealToRealWithNoRounding(idealBaselineOriginX), pixelsPerDip), RoundDipForDisplayModeJustifiedText(IdealToRealWithNoRounding(idealBaselineOriginY), pixelsPerDip));
		int num = RealToIdeal(baselineOrigin.X);
		int num2 = idealBaselineOriginX - num;
		if (isRightToLeft)
		{
			num2 *= -1;
		}
		if (glyphCount <= 0)
		{
			return;
		}
		double num3 = 0.0;
		double num4 = 0.0;
		int num5 = num2;
		double num6 = 0.0;
		int num7 = -1;
		double num8 = 0.0;
		for (int i = 0; i < glyphCount; i++)
		{
			if (IsSpace(pwchText[i]))
			{
				num7 = i;
			}
			num5 += piGlyphAdvances[i];
			num4 = IdealToRealWithNoRounding(num5);
			num8 = RoundDipForDisplayModeJustifiedText(IdealToRealWithNoRounding(piGlyphAdvances[i]), pixelsPerDip);
			num3 += num8;
			num6 += RoundDipForDisplayModeJustifiedText(num3 - RoundDipForDisplayModeJustifiedText(num4, pixelsPerDip), pixelsPerDip);
			adjustedAdvanceWidths[i] = num8;
			if (num7 >= 0)
			{
				adjustedAdvanceWidths[num7] -= num6;
				num3 -= num6;
				num6 = 0.0;
			}
		}
		if (num7 < 0)
		{
			num3 = 0.0;
			num4 = 0.0;
			num5 = num2;
			num8 = 0.0;
			num6 = 0.0;
			for (int j = 0; j < glyphCount; j++)
			{
				num5 += piGlyphAdvances[j];
				num4 = IdealToRealWithNoRounding(num5);
				num8 = RoundDipForDisplayModeJustifiedText(IdealToRealWithNoRounding(piGlyphAdvances[j]), pixelsPerDip);
				num3 += num8;
				num6 = RoundDipForDisplayModeJustifiedText(num3 - RoundDipForDisplayModeJustifiedText(num4, pixelsPerDip), pixelsPerDip);
				adjustedAdvanceWidths[j] = num8 - num6;
				num3 -= num6;
			}
		}
	}

	private unsafe GlyphRun ComputeShapedGlyphRun(LSRun lsrun, TextFormatterImp textFormatterImp, bool originProvided, LSPOINT lsrunOrigin, int charCount, char* pwchText, ushort* puClusterMap, int glyphCount, ushort* puGlyphs, int* piJustifiedGlyphAdvances, GlyphOffset* piiGlyphOffsets, bool justify)
	{
		TextMetrics.FullTextLine currentLine = Draw.CurrentLine;
		Point baselineOrigin = default(Point);
		int nominalX = 0;
		int nominalY = 0;
		if (originProvided)
		{
			if (currentLine.RightToLeft)
			{
				lsrunOrigin.x = -lsrunOrigin.x;
			}
			if (textFormatterImp.TextFormattingMode == TextFormattingMode.Display && justify)
			{
				LSRun.UVToNominalXY(Draw.LineOrigin, Draw.VectorToLineOrigin, currentLine.LSLineUToParagraphU(lsrunOrigin.x), lsrunOrigin.y + lsrun.BaselineMoveOffset, currentLine, out nominalX, out nominalY);
			}
			else
			{
				baselineOrigin = LSRun.UVToXY(Draw.LineOrigin, Draw.VectorToLineOrigin, currentLine.LSLineUToParagraphU(lsrunOrigin.x), lsrunOrigin.y + lsrun.BaselineMoveOffset, currentLine);
			}
		}
		char[] array = new char[charCount];
		ushort[] array2 = new ushort[charCount];
		for (int i = 0; i < charCount; i++)
		{
			array[i] = pwchText[i];
			array2[i] = puClusterMap[i];
		}
		ushort[] array3 = new ushort[glyphCount];
		bool flag = (lsrun.BidiLevel & 1) != 0;
		IList<double> adjustedAdvanceWidths;
		IList<Point> list;
		if (textFormatterImp.TextFormattingMode == TextFormattingMode.Ideal)
		{
			adjustedAdvanceWidths = new ThousandthOfEmRealDoubles(textFormatterImp.IdealToReal(lsrun.EmSize, currentLine.PixelsPerDip), glyphCount);
			list = new ThousandthOfEmRealPoints(textFormatterImp.IdealToReal(lsrun.EmSize, currentLine.PixelsPerDip), glyphCount);
			for (int j = 0; j < glyphCount; j++)
			{
				array3[j] = puGlyphs[j];
				adjustedAdvanceWidths[j] = textFormatterImp.IdealToReal(piJustifiedGlyphAdvances[j], currentLine.PixelsPerDip);
				list[j] = new Point(textFormatterImp.IdealToReal(piiGlyphOffsets[j].du, currentLine.PixelsPerDip), textFormatterImp.IdealToReal(piiGlyphOffsets[j].dv, currentLine.PixelsPerDip));
			}
		}
		else
		{
			if (justify)
			{
				AdjustMetricsForDisplayModeJustifiedText(pwchText, piJustifiedGlyphAdvances, glyphCount, flag, nominalX, nominalY, currentLine.PixelsPerDip, out baselineOrigin, out adjustedAdvanceWidths);
			}
			else
			{
				adjustedAdvanceWidths = new List<double>(glyphCount);
				for (int k = 0; k < glyphCount; k++)
				{
					adjustedAdvanceWidths.Add(textFormatterImp.IdealToReal(piJustifiedGlyphAdvances[k], currentLine.PixelsPerDip));
				}
			}
			list = new List<Point>(glyphCount);
			for (int l = 0; l < glyphCount; l++)
			{
				array3[l] = puGlyphs[l];
				list.Add(new Point(textFormatterImp.IdealToReal(piiGlyphOffsets[l].du, currentLine.PixelsPerDip), textFormatterImp.IdealToReal(piiGlyphOffsets[l].dv, currentLine.PixelsPerDip)));
			}
		}
		return lsrun.Shapeable.ComputeShapedGlyphRun(baselineOrigin, array, array2, array3, adjustedAdvanceWidths, list, flag, sideways: false);
	}

	private unsafe GlyphRun ComputeUnshapedGlyphRun(LSRun lsrun, LsTFlow textFlow, TextFormatterImp textFormatterImp, bool originProvided, LSPOINT lsrunOrigin, int dupRun, int cchText, char* pwchText, int* piCharAdvances, bool justify)
	{
		GlyphRun result = null;
		if (lsrun.Type == Plsrun.Text)
		{
			Point baselineOrigin = default(Point);
			int nominalX = 0;
			int nominalY = 0;
			if (originProvided)
			{
				TextMetrics.FullTextLine currentLine = Draw.CurrentLine;
				if (textFlow == LsTFlow.lstflowWS)
				{
					lsrunOrigin.x -= dupRun;
				}
				if (currentLine.RightToLeft)
				{
					lsrunOrigin.x = -lsrunOrigin.x;
				}
				if (textFormatterImp.TextFormattingMode == TextFormattingMode.Display && justify)
				{
					LSRun.UVToNominalXY(Draw.LineOrigin, Draw.VectorToLineOrigin, currentLine.LSLineUToParagraphU(lsrunOrigin.x), lsrunOrigin.y + lsrun.BaselineMoveOffset, currentLine, out nominalX, out nominalY);
				}
				else
				{
					baselineOrigin = LSRun.UVToXY(Draw.LineOrigin, Draw.VectorToLineOrigin, currentLine.LSLineUToParagraphU(lsrunOrigin.x), lsrunOrigin.y + lsrun.BaselineMoveOffset, currentLine);
				}
			}
			char[] array = new char[cchText];
			bool isRightToLeft = (lsrun.BidiLevel & 1) != 0;
			IList<double> adjustedAdvanceWidths;
			if (textFormatterImp.TextFormattingMode == TextFormattingMode.Ideal)
			{
				adjustedAdvanceWidths = new ThousandthOfEmRealDoubles(textFormatterImp.IdealToReal(lsrun.EmSize, Draw.CurrentLine.PixelsPerDip), cchText);
				for (int i = 0; i < cchText; i++)
				{
					array[i] = pwchText[i];
					adjustedAdvanceWidths[i] = textFormatterImp.IdealToReal(piCharAdvances[i], Draw.CurrentLine.PixelsPerDip);
				}
			}
			else
			{
				if (justify)
				{
					AdjustMetricsForDisplayModeJustifiedText(pwchText, piCharAdvances, cchText, isRightToLeft, nominalX, nominalY, Draw.CurrentLine.PixelsPerDip, out baselineOrigin, out adjustedAdvanceWidths);
				}
				else
				{
					adjustedAdvanceWidths = new List<double>(cchText);
					for (int j = 0; j < cchText; j++)
					{
						adjustedAdvanceWidths.Add(textFormatterImp.IdealToReal(piCharAdvances[j], Draw.CurrentLine.PixelsPerDip));
					}
				}
				for (int k = 0; k < cchText; k++)
				{
					array[k] = pwchText[k];
				}
			}
			result = lsrun.Shapeable.ComputeUnshapedGlyphRun(baselineOrigin, array, adjustedAdvanceWidths);
		}
		return result;
	}

	internal unsafe LineServicesCallbacks()
	{
		_pfnFetchRunRedefined = FetchRunRedefined;
		_pfnFetchLineProps = FetchLineProps;
		_pfnFetchPap = FetchPap;
		_pfnGetRunTextMetrics = GetRunTextMetrics;
		_pfnGetRunCharWidths = GetRunCharWidths;
		_pfnGetDurMaxExpandRagged = GetDurMaxExpandRagged;
		_pfnDrawTextRun = DrawTextRun;
		_pfnGetGlyphsRedefined = GetGlyphsRedefined;
		_pfnGetGlyphPositions = GetGlyphPositions;
		_pfnGetAutoNumberInfo = GetAutoNumberInfo;
		_pfnDrawGlyphs = DrawGlyphs;
		_pfnGetObjectHandlerInfo = GetObjectHandlerInfo;
		_pfnGetRunUnderlineInfo = GetRunUnderlineInfo;
		_pfnGetRunStrikethroughInfo = GetRunStrikethroughInfo;
		_pfnHyphenate = Hyphenate;
		_pfnGetNextHyphenOpp = GetNextHyphenOpp;
		_pfnGetPrevHyphenOpp = GetPrevHyphenOpp;
		_pfnDrawUnderline = DrawUnderline;
		_pfnDrawStrikethrough = DrawStrikethrough;
		_pfnFInterruptShaping = FInterruptShaping;
		_pfnGetCharCompressionInfoFullMixed = GetCharCompressionInfoFullMixed;
		_pfnGetCharExpansionInfoFullMixed = GetCharExpansionInfoFullMixed;
		_pfnGetGlyphCompressionInfoFullMixed = GetGlyphCompressionInfoFullMixed;
		_pfnGetGlyphExpansionInfoFullMixed = GetGlyphExpansionInfoFullMixed;
		_pfnEnumText = EnumText;
		_pfnEnumTab = EnumTab;
	}

	internal void PopulateContextInfo(ref LsContextInfo contextInfo, ref LscbkRedefined lscbkRedef)
	{
		lscbkRedef.pfnFetchRunRedefined = _pfnFetchRunRedefined;
		lscbkRedef.pfnGetGlyphsRedefined = _pfnGetGlyphsRedefined;
		lscbkRedef.pfnFetchLineProps = _pfnFetchLineProps;
		contextInfo.pfnFetchLineProps = _pfnFetchLineProps;
		contextInfo.pfnFetchPap = _pfnFetchPap;
		contextInfo.pfnGetRunTextMetrics = _pfnGetRunTextMetrics;
		contextInfo.pfnGetRunCharWidths = _pfnGetRunCharWidths;
		contextInfo.pfnGetDurMaxExpandRagged = _pfnGetDurMaxExpandRagged;
		contextInfo.pfnDrawTextRun = _pfnDrawTextRun;
		contextInfo.pfnGetGlyphPositions = _pfnGetGlyphPositions;
		contextInfo.pfnGetAutoNumberInfo = _pfnGetAutoNumberInfo;
		contextInfo.pfnDrawGlyphs = _pfnDrawGlyphs;
		contextInfo.pfnGetObjectHandlerInfo = _pfnGetObjectHandlerInfo;
		contextInfo.pfnGetRunUnderlineInfo = _pfnGetRunUnderlineInfo;
		contextInfo.pfnGetRunStrikethroughInfo = _pfnGetRunStrikethroughInfo;
		contextInfo.pfnHyphenate = _pfnHyphenate;
		contextInfo.pfnGetNextHyphenOpp = _pfnGetNextHyphenOpp;
		contextInfo.pfnGetPrevHyphenOpp = _pfnGetPrevHyphenOpp;
		contextInfo.pfnDrawUnderline = _pfnDrawUnderline;
		contextInfo.pfnDrawStrikethrough = _pfnDrawStrikethrough;
		contextInfo.pfnFInterruptShaping = _pfnFInterruptShaping;
		contextInfo.pfnGetCharCompressionInfoFullMixed = _pfnGetCharCompressionInfoFullMixed;
		contextInfo.pfnGetCharExpansionInfoFullMixed = _pfnGetCharExpansionInfoFullMixed;
		contextInfo.pfnGetGlyphCompressionInfoFullMixed = _pfnGetGlyphCompressionInfoFullMixed;
		contextInfo.pfnGetGlyphExpansionInfoFullMixed = _pfnGetGlyphExpansionInfoFullMixed;
		contextInfo.pfnEnumText = _pfnEnumText;
		contextInfo.pfnEnumTab = _pfnEnumTab;
	}

	private void SaveException(Exception e, Plsrun plsrun, LSRun lsrun)
	{
		e.Data["ExceptionContext"] = new ExceptionContext(e.Data["ExceptionContext"], e.StackTrace, plsrun, lsrun);
		_exception = e;
	}

	private void SaveNonCLSException(string methodName, Plsrun plsrun, LSRun lsrun)
	{
		Exception ex = new Exception(SR.NonCLSException);
		ex.Data["ExceptionContext"] = new ExceptionContext(null, methodName, plsrun, lsrun);
		_exception = ex;
	}

	internal void EmptyBoundingBox()
	{
		_boundingBox = Rect.Empty;
	}

	internal void ClearIndexedGlyphRuns()
	{
		_indexedGlyphRuns = null;
	}
}
