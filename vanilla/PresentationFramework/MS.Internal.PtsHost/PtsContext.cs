using System;
using System.Collections;
using System.Threading;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using System.Windows.Threading;
using MS.Internal.PtsHost.UnsafeNativeMethods;

namespace MS.Internal.PtsHost;

internal sealed class PtsContext : DispatcherObject, IDisposable
{
	private struct HandleIndex
	{
		internal long Index;

		internal object Obj;

		internal bool IsHandle()
		{
			if (Obj != null)
			{
				return Index == 0;
			}
			return false;
		}
	}

	private HandleIndex[] _unmanagedHandles;

	private ArrayList _pages;

	private ArrayList _pageBreakRecords;

	private Exception _callbackException;

	private PtsHost _ptsHost;

	private bool _isOptimalParagraphEnabled;

	private TextFormatter _textFormatter;

	private int _disposed;

	private bool _disposeCompleted;

	private const int _defaultHandlesCapacity = 16;

	internal bool Disposed => _disposed != 0;

	internal nint Context => _ptsHost.Context;

	internal bool IsOptimalParagraphEnabled => _isOptimalParagraphEnabled;

	internal TextFormatter TextFormatter
	{
		get
		{
			return _textFormatter;
		}
		set
		{
			_textFormatter = value;
		}
	}

	internal Exception CallbackException
	{
		get
		{
			return _callbackException;
		}
		set
		{
			_callbackException = value;
		}
	}

	internal PtsContext(bool isOptimalParagraphEnabled, TextFormattingMode textFormattingMode)
	{
		_pages = new ArrayList(1);
		_pageBreakRecords = new ArrayList(1);
		_unmanagedHandles = new HandleIndex[16];
		_isOptimalParagraphEnabled = isOptimalParagraphEnabled;
		BuildFreeList(1);
		_ptsHost = PtsCache.AcquireContext(this, textFormattingMode);
	}

	public void Dispose()
	{
		if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0)
		{
			return;
		}
		try
		{
			Enter();
			for (int i = 0; i < _pageBreakRecords.Count; i++)
			{
				Invariant.Assert((nint)_pageBreakRecords[i] != IntPtr.Zero, "Invalid break record object");
				PTS.Validate(PTS.FsDestroyPageBreakRecord(_ptsHost.Context, (nint)_pageBreakRecords[i]));
			}
		}
		finally
		{
			Leave();
			_pageBreakRecords = null;
		}
		try
		{
			Enter();
			for (int i = 0; i < _pages.Count; i++)
			{
				Invariant.Assert((nint)_pages[i] != IntPtr.Zero, "Invalid break record object");
				PTS.Validate(PTS.FsDestroyPage(_ptsHost.Context, (nint)_pages[i]));
			}
		}
		finally
		{
			Leave();
			_pages = null;
		}
		if (Invariant.Strict && _unmanagedHandles != null)
		{
			for (int i = 0; i < _unmanagedHandles.Length; i++)
			{
				object obj = _unmanagedHandles[i].Obj;
				if (obj != null)
				{
					Invariant.Assert(obj is BaseParagraph || obj is Section || obj is LineBreakRecord, "One of PTS Client objects is not properly disposed.");
				}
			}
		}
		_ptsHost = null;
		_unmanagedHandles = null;
		_callbackException = null;
		_disposeCompleted = true;
	}

	internal nint CreateHandle(object obj)
	{
		Invariant.Assert(obj != null, "Cannot create handle for non-existing object.");
		Invariant.Assert(!Disposed, "PtsContext is already disposed.");
		if (_unmanagedHandles[0].Index == 0L)
		{
			Resize();
		}
		long index = _unmanagedHandles[0].Index;
		_unmanagedHandles[0].Index = _unmanagedHandles[index].Index;
		_unmanagedHandles[index].Obj = obj;
		_unmanagedHandles[index].Index = 0L;
		return (nint)index;
	}

	internal void ReleaseHandle(nint handle)
	{
		long num = handle;
		Invariant.Assert(!_disposeCompleted, "PtsContext is already disposed.");
		Invariant.Assert(num > 0 && num < _unmanagedHandles.Length, "Invalid object handle.");
		Invariant.Assert(_unmanagedHandles[num].IsHandle(), "Handle has been already released.");
		_unmanagedHandles[num].Obj = null;
		_unmanagedHandles[num].Index = _unmanagedHandles[0].Index;
		_unmanagedHandles[0].Index = num;
	}

	internal bool IsValidHandle(nint handle)
	{
		long num = handle;
		Invariant.Assert(!_disposeCompleted, "PtsContext is already disposed.");
		if (num < 0 || num >= _unmanagedHandles.Length)
		{
			return false;
		}
		return _unmanagedHandles[num].IsHandle();
	}

	internal object HandleToObject(nint handle)
	{
		long num = handle;
		Invariant.Assert(!_disposeCompleted, "PtsContext is already disposed.");
		Invariant.Assert(num > 0 && num < _unmanagedHandles.Length, "Invalid object handle.");
		Invariant.Assert(_unmanagedHandles[num].IsHandle(), "Handle has been already released.");
		return _unmanagedHandles[num].Obj;
	}

	internal void Enter()
	{
		Invariant.Assert(!_disposeCompleted, "PtsContext is already disposed.");
		_ptsHost.EnterContext(this);
	}

	internal void Leave()
	{
		Invariant.Assert(!_disposeCompleted, "PtsContext is already disposed.");
		_ptsHost.LeaveContext(this);
	}

	internal void OnPageCreated(MS.Internal.SecurityCriticalDataForSet<nint> ptsPage)
	{
		Invariant.Assert(ptsPage.Value != IntPtr.Zero, "Invalid page object.");
		Invariant.Assert(!Disposed, "PtsContext is already disposed.");
		Invariant.Assert(!_pages.Contains(ptsPage.Value), "Page already exists.");
		_pages.Add(ptsPage.Value);
	}

	internal void OnPageDisposed(MS.Internal.SecurityCriticalDataForSet<nint> ptsPage, bool disposing, bool enterContext)
	{
		Invariant.Assert(ptsPage.Value != IntPtr.Zero, "Invalid page object.");
		if (disposing)
		{
			OnDestroyPage(ptsPage, enterContext);
		}
		else if (!Disposed && !base.Dispatcher.HasShutdownStarted)
		{
			base.Dispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(OnDestroyPage), ptsPage);
		}
	}

	internal void OnPageBreakRecordCreated(MS.Internal.SecurityCriticalDataForSet<nint> br)
	{
		Invariant.Assert(br.Value != IntPtr.Zero, "Invalid break record object.");
		Invariant.Assert(!Disposed, "PtsContext is already disposed.");
		Invariant.Assert(!_pageBreakRecords.Contains(br.Value), "Break record already exists.");
		_pageBreakRecords.Add(br.Value);
	}

	internal void OnPageBreakRecordDisposed(MS.Internal.SecurityCriticalDataForSet<nint> br, bool disposing)
	{
		Invariant.Assert(br.Value != IntPtr.Zero, "Invalid break record object.");
		if (disposing)
		{
			OnDestroyBreakRecord(br);
		}
		else if (!Disposed && !base.Dispatcher.HasShutdownStarted)
		{
			base.Dispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(OnDestroyBreakRecord), br);
		}
	}

	private void BuildFreeList(int freeIndex)
	{
		_unmanagedHandles[0].Index = freeIndex;
		while (freeIndex < _unmanagedHandles.Length)
		{
			_unmanagedHandles[freeIndex].Index = ++freeIndex;
		}
		_unmanagedHandles[freeIndex - 1].Index = 0L;
	}

	private void Resize()
	{
		int freeIndex = _unmanagedHandles.Length;
		HandleIndex[] array = new HandleIndex[_unmanagedHandles.Length * 2];
		Array.Copy(_unmanagedHandles, array, _unmanagedHandles.Length);
		_unmanagedHandles = array;
		BuildFreeList(freeIndex);
	}

	private object OnDestroyPage(object args)
	{
		MS.Internal.SecurityCriticalDataForSet<nint> ptsPage = (MS.Internal.SecurityCriticalDataForSet<nint>)args;
		OnDestroyPage(ptsPage, enterContext: true);
		return null;
	}

	private void OnDestroyPage(MS.Internal.SecurityCriticalDataForSet<nint> ptsPage, bool enterContext)
	{
		Invariant.Assert(ptsPage.Value != IntPtr.Zero, "Invalid page object.");
		if (Disposed)
		{
			return;
		}
		Invariant.Assert(_pages != null, "Collection of pages does not exist.");
		Invariant.Assert(_pages.Contains(ptsPage.Value), "Page does not exist.");
		try
		{
			if (enterContext)
			{
				Enter();
			}
			PTS.Validate(PTS.FsDestroyPage(_ptsHost.Context, ptsPage.Value));
		}
		finally
		{
			if (enterContext)
			{
				Leave();
			}
			_pages.Remove(ptsPage.Value);
		}
	}

	private object OnDestroyBreakRecord(object args)
	{
		MS.Internal.SecurityCriticalDataForSet<nint> securityCriticalDataForSet = (MS.Internal.SecurityCriticalDataForSet<nint>)args;
		Invariant.Assert(securityCriticalDataForSet.Value != IntPtr.Zero, "Invalid break record object.");
		if (!Disposed)
		{
			Invariant.Assert(_pageBreakRecords != null, "Collection of break records does not exist.");
			Invariant.Assert(_pageBreakRecords.Contains(securityCriticalDataForSet.Value), "Break record does not exist.");
			try
			{
				Enter();
				PTS.Validate(PTS.FsDestroyPageBreakRecord(_ptsHost.Context, securityCriticalDataForSet.Value));
			}
			finally
			{
				Leave();
				_pageBreakRecords.Remove(securityCriticalDataForSet.Value);
			}
		}
		return null;
	}
}
