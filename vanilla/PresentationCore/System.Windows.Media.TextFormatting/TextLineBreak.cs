using MS.Internal;
using MS.Internal.PresentationCore;
using MS.Internal.TextFormatting;

namespace System.Windows.Media.TextFormatting;

/// <summary>Specifies text properties and state at the point where text is broken by the line breaking process.</summary>
public sealed class TextLineBreak : IDisposable
{
	private TextModifierScope _currentScope;

	private MS.Internal.SecurityCriticalDataForSet<nint> _breakRecord;

	internal TextModifierScope TextModifierScope => _currentScope;

	internal MS.Internal.SecurityCriticalDataForSet<nint> BreakRecord => _breakRecord;

	internal TextLineBreak(TextModifierScope currentScope, MS.Internal.SecurityCriticalDataForSet<nint> breakRecord)
	{
		_currentScope = currentScope;
		_breakRecord = breakRecord;
		if (breakRecord.Value == IntPtr.Zero)
		{
			GC.SuppressFinalize(this);
		}
	}

	~TextLineBreak()
	{
		DisposeInternal(finalizing: true);
	}

	/// <summary>Releases the resources used by the <see cref="T:System.Windows.Media.TextFormatting.TextLineBreak" /> class.</summary>
	public void Dispose()
	{
		DisposeInternal(finalizing: false);
		GC.SuppressFinalize(this);
	}

	/// <summary>Clone a new instance of the <see cref="T:System.Windows.Media.TextFormatting.TextLineBreak" /> object.</summary>
	/// <returns>A new instance of <see cref="T:System.Windows.Media.TextFormatting.TextLineBreak" />.</returns>
	public TextLineBreak Clone()
	{
		nint pBreakRecClone = IntPtr.Zero;
		if (_breakRecord.Value != IntPtr.Zero)
		{
			LsErr lsErr = UnsafeNativeMethods.LoCloneBreakRecord(_breakRecord.Value, out pBreakRecClone);
			if (lsErr != 0)
			{
				TextFormatterContext.ThrowExceptionFromLsError(SR.Format(SR.CloneBreakRecordFailure, lsErr), lsErr);
			}
		}
		return new TextLineBreak(_currentScope, new MS.Internal.SecurityCriticalDataForSet<nint>(pBreakRecClone));
	}

	private void DisposeInternal(bool finalizing)
	{
		if (_breakRecord.Value != IntPtr.Zero)
		{
			UnsafeNativeMethods.LoDisposeBreakRecord(_breakRecord.Value, finalizing);
			_breakRecord.Value = IntPtr.Zero;
			GC.KeepAlive(this);
		}
	}
}
