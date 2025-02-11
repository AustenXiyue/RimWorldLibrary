using System.Collections.Generic;
using System.Threading;
using MS.Internal;
using MS.Internal.PresentationCore;
using MS.Internal.Text.TextInterface;
using MS.Internal.TextFormatting;

namespace System.Windows.Media.TextFormatting;

[FriendAccessAllowed]
internal class TextFormatterContext
{
	private enum State : byte
	{
		Uninitialized,
		Initialized
	}

	private MS.Internal.SecurityCriticalDataForSet<nint> _ploc;

	private LineServicesCallbacks _callbacks;

	private State _state;

	private BreakStrategies _breaking;

	private static Dictionary<char, bool> _specialCharacters;

	private const uint TwipsPerInch = 1440u;

	internal object Owner
	{
		get
		{
			return _callbacks.Owner;
		}
		set
		{
			_callbacks.Owner = value;
		}
	}

	internal Exception CallbackException
	{
		get
		{
			return _callbacks.Exception;
		}
		set
		{
			_callbacks.Exception = value;
		}
	}

	internal Rect BoundingBox => _callbacks.BoundingBox;

	internal ICollection<IndexedGlyphRun> IndexedGlyphRuns => _callbacks.IndexedGlyphRuns;

	internal MS.Internal.SecurityCriticalDataForSet<nint> Ploc => _ploc;

	public TextFormatterContext()
	{
		_ploc = new MS.Internal.SecurityCriticalDataForSet<nint>(IntPtr.Zero);
		Init();
	}

	private void Init()
	{
		if (_ploc.Value == IntPtr.Zero)
		{
			LsErr lsErr = LsErr.None;
			LsContextInfo contextInfo = default(LsContextInfo);
			LscbkRedefined lscbkRedef = default(LscbkRedefined);
			_callbacks = new LineServicesCallbacks();
			_callbacks.PopulateContextInfo(ref contextInfo, ref lscbkRedef);
			contextInfo.version = 4u;
			contextInfo.pols = IntPtr.Zero;
			contextInfo.cEstimatedCharsPerLine = 100;
			contextInfo.fDontReleaseRuns = 1;
			contextInfo.cJustPriorityLim = 3;
			contextInfo.wchNull = '\0';
			contextInfo.wchUndef = '\u0001';
			contextInfo.wchTab = '\t';
			contextInfo.wchPosTab = contextInfo.wchUndef;
			contextInfo.wchEndPara1 = '\u2029';
			contextInfo.wchEndPara2 = contextInfo.wchUndef;
			contextInfo.wchSpace = ' ';
			contextInfo.wchHyphen = TextAnalyzer.CharHyphen;
			contextInfo.wchNonReqHyphen = '\u00ad';
			contextInfo.wchNonBreakHyphen = '‑';
			contextInfo.wchEnDash = '–';
			contextInfo.wchEmDash = '—';
			contextInfo.wchEnSpace = '\u2002';
			contextInfo.wchEmSpace = '\u2003';
			contextInfo.wchNarrowSpace = '\u2009';
			contextInfo.wchJoiner = '\u200d';
			contextInfo.wchNonJoiner = '\u200c';
			contextInfo.wchVisiNull = '⁐';
			contextInfo.wchVisiAltEndPara = '⁑';
			contextInfo.wchVisiEndLineInPara = '⁒';
			contextInfo.wchVisiEndPara = '⁓';
			contextInfo.wchVisiSpace = '\u2054';
			contextInfo.wchVisiNonBreakSpace = '⁕';
			contextInfo.wchVisiNonBreakHyphen = '⁖';
			contextInfo.wchVisiNonReqHyphen = '⁗';
			contextInfo.wchVisiTab = '⁘';
			contextInfo.wchVisiPosTab = contextInfo.wchUndef;
			contextInfo.wchVisiEmSpace = '⁙';
			contextInfo.wchVisiEnSpace = '⁚';
			contextInfo.wchVisiNarrowSpace = '⁛';
			contextInfo.wchVisiOptBreak = '⁜';
			contextInfo.wchVisiNoBreak = '⁝';
			contextInfo.wchVisiFESpace = '⁞';
			contextInfo.wchFESpace = '\u3000';
			contextInfo.wchEscAnmRun = '\u2029';
			contextInfo.wchAltEndPara = contextInfo.wchUndef;
			contextInfo.wchEndLineInPara = '\u2028';
			contextInfo.wchSectionBreak = contextInfo.wchUndef;
			contextInfo.wchNonBreakSpace = '\u00a0';
			contextInfo.wchNoBreak = contextInfo.wchUndef;
			contextInfo.wchColumnBreak = contextInfo.wchUndef;
			contextInfo.wchPageBreak = contextInfo.wchUndef;
			contextInfo.wchOptBreak = contextInfo.wchUndef;
			contextInfo.wchToReplace = contextInfo.wchUndef;
			contextInfo.wchReplace = contextInfo.wchUndef;
			nint ploc = IntPtr.Zero;
			_ = IntPtr.Zero;
			lsErr = UnsafeNativeMethods.LoCreateContext(ref contextInfo, ref lscbkRedef, out ploc);
			if (lsErr != 0)
			{
				ThrowExceptionFromLsError(SR.Format(SR.CreateContextFailure, lsErr), lsErr);
			}
			if (_specialCharacters == null)
			{
				SetSpecialCharacters(ref contextInfo);
			}
			_ploc.Value = ploc;
			GC.KeepAlive(contextInfo);
			LsDevRes deviceInfo = default(LsDevRes);
			deviceInfo.dxpInch = (deviceInfo.dxrInch = 1440u);
			deviceInfo.dypInch = (deviceInfo.dyrInch = 1440u);
			SetDoc(isDisplay: true, isReferencePresentationEqual: true, ref deviceInfo);
			SetBreaking(BreakStrategies.BreakCJK);
		}
	}

	internal TextPenaltyModule GetTextPenaltyModule()
	{
		Invariant.Assert(_ploc.Value != IntPtr.Zero);
		return new TextPenaltyModule(_ploc);
	}

	internal void Release()
	{
		CallbackException = null;
		Owner = null;
	}

	internal void EmptyBoundingBox()
	{
		_callbacks.EmptyBoundingBox();
	}

	internal void ClearIndexedGlyphRuns()
	{
		_callbacks.ClearIndexedGlyphRuns();
	}

	internal void Destroy()
	{
		if (_ploc.Value != IntPtr.Zero)
		{
			UnsafeNativeMethods.LoDestroyContext(_ploc.Value);
			_ploc.Value = IntPtr.Zero;
		}
	}

	internal void SetBreaking(BreakStrategies breaking)
	{
		if (_state == State.Uninitialized || breaking != _breaking)
		{
			Invariant.Assert(_ploc.Value != IntPtr.Zero);
			LsErr lsErr = UnsafeNativeMethods.LoSetBreaking(_ploc.Value, (int)breaking);
			if (lsErr != 0)
			{
				ThrowExceptionFromLsError(SR.Format(SR.SetBreakingFailure, lsErr), lsErr);
			}
			_breaking = breaking;
		}
		_state = State.Initialized;
	}

	internal LsErr CreateLine(int cpFirst, int lineLength, int maxWidth, LineFlags lineFlags, nint previousLineBreakRecord, out nint ploline, out LsLInfo plslineInfo, out int maxDepth, out LsLineWidths lineWidths)
	{
		Invariant.Assert(_ploc.Value != IntPtr.Zero);
		return UnsafeNativeMethods.LoCreateLine(_ploc.Value, cpFirst, lineLength, maxWidth, (uint)lineFlags, previousLineBreakRecord, out plslineInfo, out ploline, out maxDepth, out lineWidths);
	}

	internal LsErr CreateBreaks(int cpFirst, nint previousLineBreakRecord, nint ploparabreak, nint ptslinevariantRestriction, ref LsBreaks lsbreaks, out int bestFitIndex)
	{
		Invariant.Assert(_ploc.Value != IntPtr.Zero);
		return UnsafeNativeMethods.LoCreateBreaks(_ploc.Value, cpFirst, previousLineBreakRecord, ploparabreak, ptslinevariantRestriction, ref lsbreaks, out bestFitIndex);
	}

	internal LsErr CreateParaBreakingSession(int cpFirst, int maxWidth, nint previousLineBreakRecord, ref nint ploparabreak, ref bool penalizedAsJustified)
	{
		Invariant.Assert(_ploc.Value != IntPtr.Zero);
		return UnsafeNativeMethods.LoCreateParaBreakingSession(_ploc.Value, cpFirst, maxWidth, previousLineBreakRecord, ref ploparabreak, ref penalizedAsJustified);
	}

	internal void SetDoc(bool isDisplay, bool isReferencePresentationEqual, ref LsDevRes deviceInfo)
	{
		Invariant.Assert(_ploc.Value != IntPtr.Zero);
		LsErr lsErr = UnsafeNativeMethods.LoSetDoc(_ploc.Value, isDisplay ? 1 : 0, isReferencePresentationEqual ? 1 : 0, ref deviceInfo);
		if (lsErr != 0)
		{
			ThrowExceptionFromLsError(SR.Format(SR.SetDocFailure, lsErr), lsErr);
		}
	}

	internal unsafe void SetTabs(int incrementalTab, LsTbd* tabStops, int tabStopCount)
	{
		Invariant.Assert(_ploc.Value != IntPtr.Zero);
		LsErr lsErr = UnsafeNativeMethods.LoSetTabs(_ploc.Value, incrementalTab, tabStopCount, tabStops);
		if (lsErr != 0)
		{
			ThrowExceptionFromLsError(SR.Format(SR.SetTabsFailure, lsErr), lsErr);
		}
	}

	internal static void ThrowExceptionFromLsError(string message, LsErr lserr)
	{
		if (lserr == LsErr.OutOfMemory)
		{
			throw new OutOfMemoryException(message);
		}
		throw new Exception(message);
	}

	internal static bool IsSpecialCharacter(char c)
	{
		return _specialCharacters.ContainsKey(c);
	}

	private static void SetSpecialCharacters(ref LsContextInfo contextInfo)
	{
		Dictionary<char, bool> dictionary = new Dictionary<char, bool>();
		dictionary[contextInfo.wchHyphen] = true;
		dictionary[contextInfo.wchTab] = true;
		dictionary[contextInfo.wchPosTab] = true;
		dictionary[contextInfo.wchEndPara1] = true;
		dictionary[contextInfo.wchEndPara2] = true;
		dictionary[contextInfo.wchAltEndPara] = true;
		dictionary[contextInfo.wchEndLineInPara] = true;
		dictionary[contextInfo.wchColumnBreak] = true;
		dictionary[contextInfo.wchSectionBreak] = true;
		dictionary[contextInfo.wchPageBreak] = true;
		dictionary[contextInfo.wchNonBreakSpace] = true;
		dictionary[contextInfo.wchNonBreakHyphen] = true;
		dictionary[contextInfo.wchNonReqHyphen] = true;
		dictionary[contextInfo.wchEmDash] = true;
		dictionary[contextInfo.wchEnDash] = true;
		dictionary[contextInfo.wchEmSpace] = true;
		dictionary[contextInfo.wchEnSpace] = true;
		dictionary[contextInfo.wchNarrowSpace] = true;
		dictionary[contextInfo.wchOptBreak] = true;
		dictionary[contextInfo.wchNoBreak] = true;
		dictionary[contextInfo.wchFESpace] = true;
		dictionary[contextInfo.wchJoiner] = true;
		dictionary[contextInfo.wchNonJoiner] = true;
		dictionary[contextInfo.wchToReplace] = true;
		dictionary[contextInfo.wchReplace] = true;
		dictionary[contextInfo.wchVisiNull] = true;
		dictionary[contextInfo.wchVisiAltEndPara] = true;
		dictionary[contextInfo.wchVisiEndLineInPara] = true;
		dictionary[contextInfo.wchVisiEndPara] = true;
		dictionary[contextInfo.wchVisiSpace] = true;
		dictionary[contextInfo.wchVisiNonBreakSpace] = true;
		dictionary[contextInfo.wchVisiNonBreakHyphen] = true;
		dictionary[contextInfo.wchVisiNonReqHyphen] = true;
		dictionary[contextInfo.wchVisiTab] = true;
		dictionary[contextInfo.wchVisiPosTab] = true;
		dictionary[contextInfo.wchVisiEmSpace] = true;
		dictionary[contextInfo.wchVisiEnSpace] = true;
		dictionary[contextInfo.wchVisiNarrowSpace] = true;
		dictionary[contextInfo.wchVisiOptBreak] = true;
		dictionary[contextInfo.wchVisiNoBreak] = true;
		dictionary[contextInfo.wchVisiFESpace] = true;
		dictionary[contextInfo.wchEscAnmRun] = true;
		dictionary[contextInfo.wchPad] = true;
		dictionary.Remove(contextInfo.wchUndef);
		Interlocked.CompareExchange(ref _specialCharacters, dictionary, null);
	}
}
