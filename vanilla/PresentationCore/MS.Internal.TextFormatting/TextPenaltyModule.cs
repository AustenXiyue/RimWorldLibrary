using System;
using System.Windows.Media.TextFormatting;
using MS.Internal.PresentationCore;

namespace MS.Internal.TextFormatting;

[FriendAccessAllowed]
internal sealed class TextPenaltyModule : IDisposable
{
	private MS.Internal.SecurityCriticalDataForSet<nint> _ploPenaltyModule;

	private bool _isDisposed;

	internal TextPenaltyModule(MS.Internal.SecurityCriticalDataForSet<nint> ploc)
	{
		nint penaltyModuleHandle;
		LsErr lsErr = UnsafeNativeMethods.LoAcquirePenaltyModule(ploc.Value, out penaltyModuleHandle);
		if (lsErr != 0)
		{
			TextFormatterContext.ThrowExceptionFromLsError(SR.Format(SR.AcquirePenaltyModuleFailure, lsErr), lsErr);
		}
		_ploPenaltyModule.Value = penaltyModuleHandle;
	}

	~TextPenaltyModule()
	{
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		if (_ploPenaltyModule.Value != IntPtr.Zero)
		{
			UnsafeNativeMethods.LoDisposePenaltyModule(_ploPenaltyModule.Value);
			_ploPenaltyModule.Value = IntPtr.Zero;
			_isDisposed = true;
			GC.KeepAlive(this);
		}
	}

	internal nint DangerousGetHandle()
	{
		if (_isDisposed)
		{
			throw new ObjectDisposedException(SR.TextPenaltyModuleHasBeenDisposed);
		}
		nint penaltyModuleInternalHandle;
		LsErr lsErr = UnsafeNativeMethods.LoGetPenaltyModuleInternalHandle(_ploPenaltyModule.Value, out penaltyModuleInternalHandle);
		if (lsErr != 0)
		{
			TextFormatterContext.ThrowExceptionFromLsError(SR.Format(SR.GetPenaltyModuleHandleFailure, lsErr), lsErr);
		}
		GC.KeepAlive(this);
		return penaltyModuleInternalHandle;
	}
}
