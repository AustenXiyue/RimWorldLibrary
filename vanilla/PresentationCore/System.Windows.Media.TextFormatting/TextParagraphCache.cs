using System.Collections.Generic;
using MS.Internal;
using MS.Internal.PresentationCore;
using MS.Internal.TextFormatting;

namespace System.Windows.Media.TextFormatting;

[FriendAccessAllowed]
internal sealed class TextParagraphCache : IDisposable
{
	private FullTextState _fullText;

	private MS.Internal.SecurityCriticalDataForSet<nint> _ploparabreak;

	private int _finiteFormatWidth;

	private bool _penalizedAsJustified;

	internal FullTextState FullText => _fullText;

	internal MS.Internal.SecurityCriticalDataForSet<nint> Ploparabreak => _ploparabreak;

	internal TextParagraphCache(FormatSettings settings, int firstCharIndex, int paragraphWidth)
	{
		Invariant.Assert(settings != null);
		_finiteFormatWidth = settings.GetFiniteFormatWidth(paragraphWidth);
		_fullText = FullTextState.Create(settings, firstCharIndex, _finiteFormatWidth);
		TextFormatterContext textFormatterContext = settings.Formatter.AcquireContext(_fullText, IntPtr.Zero);
		_fullText.SetTabs(textFormatterContext);
		nint ploparabreak = IntPtr.Zero;
		LsErr lsErr = textFormatterContext.CreateParaBreakingSession(firstCharIndex, _finiteFormatWidth, IntPtr.Zero, ref ploparabreak, ref _penalizedAsJustified);
		Exception callbackException = textFormatterContext.CallbackException;
		textFormatterContext.Release();
		if (lsErr != 0)
		{
			GC.SuppressFinalize(this);
			if (callbackException != null)
			{
				throw new InvalidOperationException(SR.Format(SR.CreateParaBreakingSessionFailure, lsErr), callbackException);
			}
			TextFormatterContext.ThrowExceptionFromLsError(SR.Format(SR.CreateParaBreakingSessionFailure, lsErr), lsErr);
		}
		_ploparabreak.Value = ploparabreak;
		GC.KeepAlive(textFormatterContext);
	}

	~TextParagraphCache()
	{
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	internal IList<TextBreakpoint> FormatBreakpoints(int firstCharIndex, TextLineBreak previousLineBreak, nint breakpointRestrictionHandle, double maxLineWidth, out int bestFitIndex)
	{
		return FullTextBreakpoint.CreateMultiple(this, firstCharIndex, VerifyMaxLineWidth(maxLineWidth), previousLineBreak, breakpointRestrictionHandle, out bestFitIndex);
	}

	private void Dispose(bool disposing)
	{
		if (_ploparabreak.Value != IntPtr.Zero)
		{
			UnsafeNativeMethods.LoDisposeParaBreakingSession(_ploparabreak.Value, !disposing);
			_ploparabreak.Value = IntPtr.Zero;
			GC.KeepAlive(this);
		}
	}

	private int VerifyMaxLineWidth(double maxLineWidth)
	{
		if (double.IsNaN(maxLineWidth))
		{
			throw new ArgumentOutOfRangeException("maxLineWidth", SR.ParameterValueCannotBeNaN);
		}
		if (maxLineWidth == 0.0 || double.IsPositiveInfinity(maxLineWidth))
		{
			return 1073741822;
		}
		if (maxLineWidth < 0.0 || maxLineWidth > 3579139.4066666667)
		{
			throw new ArgumentOutOfRangeException("maxLineWidth", SR.Format(SR.ParameterMustBeBetween, 0, 3579139.4066666667));
		}
		return TextFormatterImp.RealToIdeal(maxLineWidth);
	}
}
