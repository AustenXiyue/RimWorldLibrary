using System;
using System.Threading;

namespace MS.Internal.PtsHost;

internal sealed class PageBreakRecord : IDisposable
{
	private MS.Internal.SecurityCriticalDataForSet<nint> _br;

	private readonly int _pageNumber;

	private WeakReference _ptsContext;

	private int _disposed;

	internal nint BreakRecord => _br.Value;

	internal int PageNumber => _pageNumber;

	internal PageBreakRecord(PtsContext ptsContext, MS.Internal.SecurityCriticalDataForSet<nint> br, int pageNumber)
	{
		Invariant.Assert(ptsContext != null, "Invalid PtsContext object.");
		Invariant.Assert(br.Value != IntPtr.Zero, "Invalid break record object.");
		_br = br;
		_pageNumber = pageNumber;
		_ptsContext = new WeakReference(ptsContext);
		ptsContext.OnPageBreakRecordCreated(_br);
	}

	~PageBreakRecord()
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
		PtsContext ptsContext = null;
		if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
		{
			if (_ptsContext.Target is PtsContext { Disposed: false } ptsContext2)
			{
				ptsContext2.OnPageBreakRecordDisposed(_br, disposing);
			}
			_br.Value = IntPtr.Zero;
			_ptsContext = null;
		}
	}
}
