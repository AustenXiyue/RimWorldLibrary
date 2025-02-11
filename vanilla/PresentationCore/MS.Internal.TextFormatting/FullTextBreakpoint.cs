using System;
using System.Collections.Generic;
using System.Windows.Media.TextFormatting;
using MS.Internal.PresentationCore;

namespace MS.Internal.TextFormatting;

internal sealed class FullTextBreakpoint : TextBreakpoint
{
	private TextMetrics _metrics;

	private MS.Internal.SecurityCriticalDataForSet<nint> _ploline;

	private MS.Internal.SecurityCriticalDataForSet<nint> _penaltyResource;

	private bool _isDisposed;

	private bool _isLineTruncated;

	public override bool IsTruncated => _isLineTruncated;

	public override int Length => _metrics.Length;

	public override int DependentLength => _metrics.DependentLength;

	public override int NewlineLength => _metrics.NewlineLength;

	public override double Start => _metrics.Start;

	public override double Width => _metrics.Width;

	public override double WidthIncludingTrailingWhitespace => _metrics.WidthIncludingTrailingWhitespace;

	public override double Height => _metrics.Height;

	public override double TextHeight => _metrics.TextHeight;

	public override double Baseline => _metrics.Baseline;

	public override double TextBaseline => _metrics.TextBaseline;

	public override double MarkerBaseline => _metrics.MarkerBaseline;

	public override double MarkerHeight => _metrics.MarkerHeight;

	internal static IList<TextBreakpoint> CreateMultiple(TextParagraphCache paragraphCache, int firstCharIndex, int maxLineWidth, TextLineBreak previousLineBreak, nint penaltyRestriction, out int bestFitIndex)
	{
		Invariant.Assert(paragraphCache != null);
		FullTextState fullText = paragraphCache.FullText;
		Invariant.Assert(fullText != null);
		FormatSettings settings = fullText.TextStore.Settings;
		Invariant.Assert(settings != null);
		settings.UpdateSettingsForCurrentLine(maxLineWidth, previousLineBreak, firstCharIndex == fullText.TextStore.CpFirst);
		Invariant.Assert(settings.Formatter != null);
		TextFormatterContext textFormatterContext = settings.Formatter.AcquireContext(fullText, IntPtr.Zero);
		nint previousLineBreakRecord = IntPtr.Zero;
		if (settings.PreviousLineBreak != null)
		{
			previousLineBreakRecord = settings.PreviousLineBreak.BreakRecord.Value;
		}
		fullText.SetTabs(textFormatterContext);
		LsBreaks lsbreaks = default(LsBreaks);
		LsErr lsErr = textFormatterContext.CreateBreaks(fullText.GetBreakpointInternalCp(firstCharIndex), previousLineBreakRecord, paragraphCache.Ploparabreak.Value, penaltyRestriction, ref lsbreaks, out bestFitIndex);
		Exception callbackException = textFormatterContext.CallbackException;
		textFormatterContext.Release();
		if (lsErr != 0)
		{
			if (callbackException != null)
			{
				throw callbackException;
			}
			TextFormatterContext.ThrowExceptionFromLsError(SR.Format(SR.CreateBreaksFailure, lsErr), lsErr);
		}
		GC.KeepAlive(textFormatterContext);
		TextBreakpoint[] array = new TextBreakpoint[lsbreaks.cBreaks];
		for (int i = 0; i < lsbreaks.cBreaks; i++)
		{
			array[i] = new FullTextBreakpoint(fullText, firstCharIndex, maxLineWidth, ref lsbreaks, i);
		}
		return array;
	}

	private unsafe FullTextBreakpoint(FullTextState fullText, int firstCharIndex, int maxLineWidth, ref LsBreaks lsbreaks, int breakIndex)
		: this()
	{
		LsLineWidths lineWidths = default(LsLineWidths);
		lineWidths.upLimLine = maxLineWidth;
		lineWidths.upStartMainText = fullText.TextStore.Settings.TextIndent;
		lineWidths.upStartMarker = lineWidths.upStartMainText;
		lineWidths.upStartTrailing = lineWidths.upLimLine;
		lineWidths.upMinStartTrailing = lineWidths.upStartTrailing;
		_metrics.Compute(fullText, firstCharIndex, maxLineWidth, null, ref lineWidths, lsbreaks.plslinfoArray + breakIndex);
		_ploline = new MS.Internal.SecurityCriticalDataForSet<nint>(lsbreaks.pplolineArray[breakIndex]);
		_penaltyResource = new MS.Internal.SecurityCriticalDataForSet<nint>(lsbreaks.plinepenaltyArray[breakIndex]);
		if (lsbreaks.plslinfoArray[breakIndex].fForcedBreak != 0)
		{
			_isLineTruncated = true;
		}
	}

	private FullTextBreakpoint()
	{
		_metrics = default(TextMetrics);
	}

	~FullTextBreakpoint()
	{
		Dispose(disposing: false);
	}

	protected override void Dispose(bool disposing)
	{
		if (_ploline.Value != IntPtr.Zero)
		{
			UnsafeNativeMethods.LoDisposeLine(_ploline.Value, !disposing);
			_ploline.Value = IntPtr.Zero;
			_penaltyResource.Value = IntPtr.Zero;
			_isDisposed = true;
			GC.KeepAlive(this);
		}
	}

	public override TextLineBreak GetTextLineBreak()
	{
		if (_isDisposed)
		{
			throw new ObjectDisposedException(SR.TextBreakpointHasBeenDisposed);
		}
		return _metrics.GetTextLineBreak(_ploline.Value);
	}

	internal override MS.Internal.SecurityCriticalDataForSet<nint> GetTextPenaltyResource()
	{
		if (_isDisposed)
		{
			throw new ObjectDisposedException(SR.TextBreakpointHasBeenDisposed);
		}
		LsErr lsErr = UnsafeNativeMethods.LoRelievePenaltyResource(_ploline.Value);
		if (lsErr != 0)
		{
			TextFormatterContext.ThrowExceptionFromLsError(SR.Format(SR.RelievePenaltyResourceFailure, lsErr), lsErr);
		}
		return _penaltyResource;
	}
}
