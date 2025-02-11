using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Threading;

namespace MS.Internal.Documents;

internal class PageCache
{
	private List<PageCacheEntry> _cache;

	private PageDestroyedWatcher _pageDestroyedWatcher;

	private DynamicDocumentPaginator _documentPaginator;

	private bool _originalIsBackgroundPaginationEnabled;

	private bool _dynamicPageSizes;

	private bool _isContentRightToLeft;

	private bool _isPaginationCompleted;

	private bool _isDefaultSizeKnown;

	private Size _defaultPageSize;

	private Size _lastPageSize;

	private readonly Size _initialDefaultPageSize = new Size(816.0, 1056.0);

	private readonly int _defaultCacheSize = 64;

	public DynamicDocumentPaginator Content
	{
		get
		{
			return _documentPaginator;
		}
		set
		{
			if (_documentPaginator == value)
			{
				return;
			}
			_dynamicPageSizes = false;
			_defaultPageSize = _initialDefaultPageSize;
			_isDefaultSizeKnown = false;
			_isPaginationCompleted = false;
			if (_documentPaginator != null)
			{
				_documentPaginator.PagesChanged -= OnPagesChanged;
				_documentPaginator.GetPageCompleted -= OnGetPageCompleted;
				_documentPaginator.PaginationProgress -= OnPaginationProgress;
				_documentPaginator.PaginationCompleted -= OnPaginationCompleted;
				_documentPaginator.IsBackgroundPaginationEnabled = _originalIsBackgroundPaginationEnabled;
			}
			_documentPaginator = value;
			ClearCache();
			if (_documentPaginator != null)
			{
				_documentPaginator.PagesChanged += OnPagesChanged;
				_documentPaginator.GetPageCompleted += OnGetPageCompleted;
				_documentPaginator.PaginationProgress += OnPaginationProgress;
				_documentPaginator.PaginationCompleted += OnPaginationCompleted;
				_documentPaginator.PageSize = _defaultPageSize;
				_originalIsBackgroundPaginationEnabled = _documentPaginator.IsBackgroundPaginationEnabled;
				_documentPaginator.IsBackgroundPaginationEnabled = true;
				if (_documentPaginator.Source is DependencyObject)
				{
					if ((FlowDirection)((DependencyObject)_documentPaginator.Source).GetValue(FrameworkElement.FlowDirectionProperty) == FlowDirection.LeftToRight)
					{
						_isContentRightToLeft = false;
					}
					else
					{
						_isContentRightToLeft = true;
					}
				}
			}
			if (_documentPaginator != null)
			{
				if (_documentPaginator.PageCount > 0)
				{
					OnPaginationProgress(_documentPaginator, new PaginationProgressEventArgs(0, _documentPaginator.PageCount));
				}
				if (_documentPaginator.IsPageCountValid)
				{
					OnPaginationCompleted(_documentPaginator, EventArgs.Empty);
				}
			}
		}
	}

	public int PageCount => _cache.Count;

	public bool DynamicPageSizes => _dynamicPageSizes;

	public bool IsContentRightToLeft => _isContentRightToLeft;

	public bool IsPaginationCompleted => _isPaginationCompleted;

	public event PaginationProgressEventHandler PaginationProgress;

	public event EventHandler PaginationCompleted;

	public event PagesChangedEventHandler PagesChanged;

	public event GetPageCompletedEventHandler GetPageCompleted;

	public event PageCacheChangedEventHandler PageCacheChanged;

	public PageCache()
	{
		_cache = new List<PageCacheEntry>(_defaultCacheSize);
		_pageDestroyedWatcher = new PageDestroyedWatcher();
	}

	public Size GetPageSize(int pageNumber)
	{
		if (pageNumber >= 0 && pageNumber < _cache.Count)
		{
			Size pageSize = _cache[pageNumber].PageSize;
			Invariant.Assert(pageSize != Size.Empty, "PageCache entry's PageSize is Empty.");
			return pageSize;
		}
		return new Size(0.0, 0.0);
	}

	public bool IsPageDirty(int pageNumber)
	{
		if (pageNumber >= 0 && pageNumber < _cache.Count)
		{
			return _cache[pageNumber].Dirty;
		}
		return true;
	}

	private void OnPaginationProgress(object sender, PaginationProgressEventArgs args)
	{
		Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Normal, new DispatcherOperationCallback(PaginationProgressDelegate), args);
	}

	private object PaginationProgressDelegate(object parameter)
	{
		if (!(parameter is PaginationProgressEventArgs paginationProgressEventArgs))
		{
			throw new InvalidOperationException("parameter");
		}
		ValidatePaginationArgs(paginationProgressEventArgs.Start, paginationProgressEventArgs.Count);
		if (_isPaginationCompleted)
		{
			if (paginationProgressEventArgs.Start == 0)
			{
				_isDefaultSizeKnown = false;
				_dynamicPageSizes = false;
			}
			_isPaginationCompleted = false;
		}
		if (paginationProgressEventArgs.Start + paginationProgressEventArgs.Count < 0)
		{
			throw new ArgumentOutOfRangeException("args");
		}
		List<PageCacheChange> list = new List<PageCacheChange>(2);
		if (paginationProgressEventArgs.Count > 0)
		{
			if (paginationProgressEventArgs.Start >= _cache.Count)
			{
				PageCacheChange pageCacheChange = AddRange(paginationProgressEventArgs.Start, paginationProgressEventArgs.Count);
				if (pageCacheChange != null)
				{
					list.Add(pageCacheChange);
				}
			}
			else if (paginationProgressEventArgs.Start + paginationProgressEventArgs.Count < _cache.Count)
			{
				PageCacheChange pageCacheChange = DirtyRange(paginationProgressEventArgs.Start, paginationProgressEventArgs.Count);
				if (pageCacheChange != null)
				{
					list.Add(pageCacheChange);
				}
			}
			else
			{
				PageCacheChange pageCacheChange = DirtyRange(paginationProgressEventArgs.Start, _cache.Count - paginationProgressEventArgs.Start);
				if (pageCacheChange != null)
				{
					list.Add(pageCacheChange);
				}
				pageCacheChange = AddRange(_cache.Count, paginationProgressEventArgs.Count - (_cache.Count - paginationProgressEventArgs.Start) + 1);
				if (pageCacheChange != null)
				{
					list.Add(pageCacheChange);
				}
			}
		}
		int num = ((_documentPaginator != null) ? _documentPaginator.PageCount : 0);
		if (num < _cache.Count)
		{
			PageCacheChange pageCacheChange = new PageCacheChange(num, _cache.Count - num, PageCacheChangeType.Remove);
			list.Add(pageCacheChange);
			_cache.RemoveRange(num, _cache.Count - num);
		}
		FirePageCacheChangedEvent(list);
		if (this.PaginationProgress != null)
		{
			this.PaginationProgress(this, paginationProgressEventArgs);
		}
		return null;
	}

	private void OnPaginationCompleted(object sender, EventArgs args)
	{
		Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Normal, new DispatcherOperationCallback(PaginationCompletedDelegate), args);
	}

	private object PaginationCompletedDelegate(object parameter)
	{
		if (!(parameter is EventArgs e))
		{
			throw new ArgumentOutOfRangeException("parameter");
		}
		_isPaginationCompleted = true;
		if (this.PaginationCompleted != null)
		{
			this.PaginationCompleted(this, e);
		}
		return null;
	}

	private void OnPagesChanged(object sender, PagesChangedEventArgs args)
	{
		Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Normal, new DispatcherOperationCallback(PagesChangedDelegate), args);
	}

	private object PagesChangedDelegate(object parameter)
	{
		if (!(parameter is PagesChangedEventArgs pagesChangedEventArgs))
		{
			throw new ArgumentOutOfRangeException("parameter");
		}
		ValidatePaginationArgs(pagesChangedEventArgs.Start, pagesChangedEventArgs.Count);
		int num = pagesChangedEventArgs.Count;
		if (pagesChangedEventArgs.Start + pagesChangedEventArgs.Count >= _cache.Count || pagesChangedEventArgs.Start + pagesChangedEventArgs.Count < 0)
		{
			num = _cache.Count - pagesChangedEventArgs.Start;
		}
		List<PageCacheChange> list = new List<PageCacheChange>(1);
		if (num > 0)
		{
			PageCacheChange pageCacheChange = DirtyRange(pagesChangedEventArgs.Start, num);
			if (pageCacheChange != null)
			{
				list.Add(pageCacheChange);
			}
			FirePageCacheChangedEvent(list);
		}
		if (this.PagesChanged != null)
		{
			this.PagesChanged(this, pagesChangedEventArgs);
		}
		return null;
	}

	private void OnGetPageCompleted(object sender, GetPageCompletedEventArgs args)
	{
		if (!args.Cancelled && args.Error == null && args.DocumentPage != DocumentPage.Missing)
		{
			_pageDestroyedWatcher.AddPage(args.DocumentPage);
			Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Normal, new DispatcherOperationCallback(GetPageCompletedDelegate), args);
		}
	}

	private object GetPageCompletedDelegate(object parameter)
	{
		if (!(parameter is GetPageCompletedEventArgs getPageCompletedEventArgs))
		{
			throw new ArgumentOutOfRangeException("parameter");
		}
		bool num = _pageDestroyedWatcher.IsDestroyed(getPageCompletedEventArgs.DocumentPage);
		_pageDestroyedWatcher.RemovePage(getPageCompletedEventArgs.DocumentPage);
		if (num)
		{
			return null;
		}
		if (!getPageCompletedEventArgs.Cancelled && getPageCompletedEventArgs.Error == null && getPageCompletedEventArgs.DocumentPage != DocumentPage.Missing)
		{
			if (getPageCompletedEventArgs.DocumentPage.Size == Size.Empty)
			{
				throw new ArgumentOutOfRangeException("args");
			}
			PageCacheEntry newEntry = default(PageCacheEntry);
			newEntry.PageSize = getPageCompletedEventArgs.DocumentPage.Size;
			newEntry.Dirty = false;
			List<PageCacheChange> list = new List<PageCacheChange>(2);
			if (getPageCompletedEventArgs.PageNumber > _cache.Count - 1)
			{
				PageCacheChange pageCacheChange = AddRange(getPageCompletedEventArgs.PageNumber, 1);
				if (pageCacheChange != null)
				{
					list.Add(pageCacheChange);
				}
				pageCacheChange = UpdateEntry(getPageCompletedEventArgs.PageNumber, newEntry);
				if (pageCacheChange != null)
				{
					list.Add(pageCacheChange);
				}
			}
			else
			{
				PageCacheChange pageCacheChange = UpdateEntry(getPageCompletedEventArgs.PageNumber, newEntry);
				if (pageCacheChange != null)
				{
					list.Add(pageCacheChange);
				}
			}
			if (_isDefaultSizeKnown && newEntry.PageSize != _lastPageSize)
			{
				_dynamicPageSizes = true;
			}
			_lastPageSize = newEntry.PageSize;
			if (!_isDefaultSizeKnown)
			{
				_defaultPageSize = newEntry.PageSize;
				_isDefaultSizeKnown = true;
				SetDefaultPageSize(dirtyOnly: true);
			}
			FirePageCacheChangedEvent(list);
		}
		if (this.GetPageCompleted != null)
		{
			this.GetPageCompleted(this, getPageCompletedEventArgs);
		}
		return null;
	}

	private void ValidatePaginationArgs(int start, int count)
	{
		if (start < 0)
		{
			throw new ArgumentOutOfRangeException("start");
		}
		if (count <= 0)
		{
			throw new ArgumentOutOfRangeException("count");
		}
	}

	private void SetDefaultPageSize(bool dirtyOnly)
	{
		List<PageCacheChange> list = new List<PageCacheChange>(PageCount);
		Invariant.Assert(_defaultPageSize != Size.Empty, "Default Page Size is Empty.");
		PageCacheEntry newEntry = default(PageCacheEntry);
		for (int i = 0; i < _cache.Count; i++)
		{
			if (_cache[i].Dirty || !dirtyOnly)
			{
				newEntry.PageSize = _defaultPageSize;
				newEntry.Dirty = true;
				PageCacheChange pageCacheChange = UpdateEntry(i, newEntry);
				if (pageCacheChange != null)
				{
					list.Add(pageCacheChange);
				}
			}
		}
		FirePageCacheChangedEvent(list);
	}

	private void FirePageCacheChangedEvent(List<PageCacheChange> changes)
	{
		if (this.PageCacheChanged != null && changes != null && changes.Count > 0)
		{
			PageCacheChangedEventArgs e = new PageCacheChangedEventArgs(changes);
			this.PageCacheChanged(this, e);
		}
	}

	private PageCacheChange AddRange(int start, int count)
	{
		if (start < 0)
		{
			throw new ArgumentOutOfRangeException("start");
		}
		if (count < 1)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		Invariant.Assert(_defaultPageSize != Size.Empty, "Default Page Size is Empty.");
		if (start >= _cache.Count)
		{
			count += start - _cache.Count;
			start = _cache.Count;
		}
		PageCacheEntry item = default(PageCacheEntry);
		for (int i = start; i < start + count; i++)
		{
			item.PageSize = _defaultPageSize;
			item.Dirty = true;
			_cache.Add(item);
		}
		return new PageCacheChange(start, count, PageCacheChangeType.Add);
	}

	private PageCacheChange UpdateEntry(int index, PageCacheEntry newEntry)
	{
		if (index >= _cache.Count || index < 0)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		Invariant.Assert(newEntry.PageSize != Size.Empty, "Updated entry newEntry has Empty PageSize.");
		if (newEntry.PageSize != _cache[index].PageSize || newEntry.Dirty != _cache[index].Dirty)
		{
			_cache[index] = newEntry;
			return new PageCacheChange(index, 1, PageCacheChangeType.Update);
		}
		return null;
	}

	private PageCacheChange DirtyRange(int start, int count)
	{
		if (start >= _cache.Count)
		{
			throw new ArgumentOutOfRangeException("start");
		}
		if (start + count > _cache.Count || count < 1)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		Invariant.Assert(_defaultPageSize != Size.Empty, "Default Page Size is Empty.");
		PageCacheEntry value = default(PageCacheEntry);
		for (int i = start; i < start + count; i++)
		{
			value.Dirty = true;
			value.PageSize = _defaultPageSize;
			_cache[i] = value;
		}
		return new PageCacheChange(start, count, PageCacheChangeType.Update);
	}

	private void ClearCache()
	{
		if (_cache.Count > 0)
		{
			List<PageCacheChange> list = new List<PageCacheChange>(1);
			PageCacheChange item = new PageCacheChange(0, _cache.Count, PageCacheChangeType.Remove);
			list.Add(item);
			_cache.Clear();
			FirePageCacheChangedEvent(list);
		}
	}
}
