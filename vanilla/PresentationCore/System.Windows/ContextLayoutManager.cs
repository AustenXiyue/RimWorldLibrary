using System.Windows.Automation.Peers;
using System.Windows.Media;
using System.Windows.Threading;
using MS.Internal.FontCache;
using MS.Internal.PresentationCore;
using MS.Internal.Text.TextInterface;
using MS.Utility;

namespace System.Windows;

internal sealed class ContextLayoutManager : DispatcherObject
{
	internal class InternalMeasureQueue : LayoutQueue
	{
		internal override void setRequest(UIElement e, Request r)
		{
			e.MeasureRequest = r;
		}

		internal override Request getRequest(UIElement e)
		{
			return e.MeasureRequest;
		}

		internal override bool canRelyOnParentRecalc(UIElement parent)
		{
			if (!parent.IsMeasureValid)
			{
				return !parent.MeasureInProgress;
			}
			return false;
		}

		internal override void invalidate(UIElement e)
		{
			e.InvalidateMeasureInternal();
		}
	}

	internal class InternalArrangeQueue : LayoutQueue
	{
		internal override void setRequest(UIElement e, Request r)
		{
			e.ArrangeRequest = r;
		}

		internal override Request getRequest(UIElement e)
		{
			return e.ArrangeRequest;
		}

		internal override bool canRelyOnParentRecalc(UIElement parent)
		{
			if (!parent.IsArrangeValid)
			{
				return !parent.ArrangeInProgress;
			}
			return false;
		}

		internal override void invalidate(UIElement e)
		{
			e.InvalidateArrangeInternal();
		}
	}

	internal abstract class LayoutQueue
	{
		internal class Request
		{
			internal UIElement Target;

			internal Request Next;

			internal Request Prev;
		}

		private const int PocketCapacity = 153;

		private const int PocketReserve = 8;

		private Request _head;

		private Request _pocket;

		private int _pocketSize;

		internal bool IsEmpty => _head == null;

		internal abstract Request getRequest(UIElement e);

		internal abstract void setRequest(UIElement e, Request r);

		internal abstract bool canRelyOnParentRecalc(UIElement parent);

		internal abstract void invalidate(UIElement e);

		internal LayoutQueue()
		{
			for (int i = 0; i < 153; i++)
			{
				_pocket = new Request
				{
					Next = _pocket
				};
			}
			_pocketSize = 153;
		}

		private void _addRequest(UIElement e)
		{
			Request request = _getNewRequest(e);
			if (request != null)
			{
				request.Next = _head;
				if (_head != null)
				{
					_head.Prev = request;
				}
				_head = request;
				setRequest(e, request);
			}
		}

		internal void Add(UIElement e)
		{
			if (getRequest(e) != null || e.CheckFlagsAnd(VisualFlags.IsLayoutSuspended))
			{
				return;
			}
			RemoveOrphans(e);
			UIElement uIParentWithinLayoutIsland = e.GetUIParentWithinLayoutIsland();
			if (uIParentWithinLayoutIsland != null && canRelyOnParentRecalc(uIParentWithinLayoutIsland))
			{
				return;
			}
			ContextLayoutManager contextLayoutManager = From(e.Dispatcher);
			if (contextLayoutManager._isDead)
			{
				return;
			}
			if (_pocketSize > 8)
			{
				_addRequest(e);
			}
			else
			{
				while (e != null)
				{
					UIElement uIParentWithinLayoutIsland2 = e.GetUIParentWithinLayoutIsland();
					invalidate(e);
					if (uIParentWithinLayoutIsland2 != null && uIParentWithinLayoutIsland2.Visibility != Visibility.Collapsed)
					{
						Remove(e);
					}
					else if (getRequest(e) == null)
					{
						RemoveOrphans(e);
						_addRequest(e);
					}
					e = uIParentWithinLayoutIsland2;
				}
			}
			contextLayoutManager.NeedsRecalc();
		}

		internal void Remove(UIElement e)
		{
			Request request = getRequest(e);
			if (request != null)
			{
				_removeRequest(request);
				setRequest(e, null);
			}
		}

		internal void RemoveOrphans(UIElement parent)
		{
			Request request = _head;
			while (request != null)
			{
				UIElement target = request.Target;
				Request next = request.Next;
				ulong num = parent.TreeLevel;
				if (target.TreeLevel == num + 1 && target.GetUIParentWithinLayoutIsland() == parent)
				{
					_removeRequest(getRequest(target));
					setRequest(target, null);
				}
				request = next;
			}
		}

		internal UIElement GetTopMost()
		{
			UIElement result = null;
			ulong num = ulong.MaxValue;
			for (Request request = _head; request != null; request = request.Next)
			{
				ulong num2 = request.Target.TreeLevel;
				if (num2 < num)
				{
					num = num2;
					result = request.Target;
				}
			}
			return result;
		}

		private void _removeRequest(Request entry)
		{
			if (entry.Prev == null)
			{
				_head = entry.Next;
			}
			else
			{
				entry.Prev.Next = entry.Next;
			}
			if (entry.Next != null)
			{
				entry.Next.Prev = entry.Prev;
			}
			ReuseRequest(entry);
		}

		private Request _getNewRequest(UIElement e)
		{
			Request request;
			if (_pocket != null)
			{
				request = _pocket;
				_pocket = request.Next;
				_pocketSize--;
				request.Next = (request.Prev = null);
			}
			else
			{
				ContextLayoutManager contextLayoutManager = From(e.Dispatcher);
				try
				{
					request = new Request();
				}
				catch (OutOfMemoryException)
				{
					contextLayoutManager?.setForceLayout(e);
					throw;
				}
			}
			request.Target = e;
			return request;
		}

		private void ReuseRequest(Request r)
		{
			r.Target = null;
			if (_pocketSize < 153)
			{
				r.Next = _pocket;
				_pocket = r;
				_pocketSize++;
			}
		}
	}

	private static DispatcherOperationCallback _updateCallback = UpdateLayoutCallback;

	private LayoutEventList _layoutEvents;

	private LayoutEventList _automationEvents;

	private UIElement _forceLayoutElement;

	private UIElement _lastExceptionElement;

	private InternalMeasureQueue _measureQueue;

	private InternalArrangeQueue _arrangeQueue;

	private SizeChangedInfo _sizeChangedChain;

	private static DispatcherOperationCallback _updateLayoutBackground = UpdateLayoutBackground;

	private EventHandler _shutdownHandler;

	internal static int s_LayoutRecursionLimit = 4096;

	private int _arrangesOnStack;

	private int _measuresOnStack;

	private int _automationSyncUpdateCounter;

	private bool _isDead;

	private bool _isUpdating;

	private bool _isInUpdateLayout;

	private bool _gotException;

	private bool _layoutRequestPosted;

	private bool _inFireLayoutUpdated;

	private bool _inFireSizeChanged;

	private bool _firePostLayoutEvents;

	private bool _inFireAutomationEvents;

	private bool hasDirtiness
	{
		get
		{
			if (MeasureQueue.IsEmpty)
			{
				return !ArrangeQueue.IsEmpty;
			}
			return true;
		}
	}

	internal LayoutQueue MeasureQueue
	{
		get
		{
			if (_measureQueue == null)
			{
				_measureQueue = new InternalMeasureQueue();
			}
			return _measureQueue;
		}
	}

	internal LayoutQueue ArrangeQueue
	{
		get
		{
			if (_arrangeQueue == null)
			{
				_arrangeQueue = new InternalArrangeQueue();
			}
			return _arrangeQueue;
		}
	}

	internal LayoutEventList LayoutEvents
	{
		get
		{
			if (_layoutEvents == null)
			{
				_layoutEvents = new LayoutEventList();
			}
			return _layoutEvents;
		}
	}

	internal LayoutEventList AutomationEvents
	{
		get
		{
			if (_automationEvents == null)
			{
				_automationEvents = new LayoutEventList();
			}
			return _automationEvents;
		}
	}

	internal int AutomationSyncUpdateCounter
	{
		get
		{
			return _automationSyncUpdateCounter;
		}
		set
		{
			_automationSyncUpdateCounter = value;
		}
	}

	internal ContextLayoutManager()
	{
		_shutdownHandler = OnDispatcherShutdown;
		base.Dispatcher.ShutdownFinished += _shutdownHandler;
	}

	private void OnDispatcherShutdown(object sender, EventArgs e)
	{
		if (_shutdownHandler != null)
		{
			base.Dispatcher.ShutdownFinished -= _shutdownHandler;
		}
		_shutdownHandler = null;
		_layoutEvents = null;
		_measureQueue = null;
		_arrangeQueue = null;
		_sizeChangedChain = null;
		_isDead = true;
	}

	internal static ContextLayoutManager From(Dispatcher dispatcher)
	{
		ContextLayoutManager contextLayoutManager = dispatcher.Reserved3 as ContextLayoutManager;
		if (contextLayoutManager == null)
		{
			if (Dispatcher.CurrentDispatcher != dispatcher)
			{
				throw new InvalidOperationException();
			}
			contextLayoutManager = (ContextLayoutManager)(dispatcher.Reserved3 = new ContextLayoutManager());
		}
		return contextLayoutManager;
	}

	private void setForceLayout(UIElement e)
	{
		_forceLayoutElement = e;
	}

	private void markTreeDirty(UIElement e)
	{
		while (e.GetUIParentNo3DTraversal() is UIElement uIElement)
		{
			e = uIElement;
		}
		markTreeDirtyHelper(e);
		MeasureQueue.Add(e);
		ArrangeQueue.Add(e);
	}

	private void markTreeDirtyHelper(Visual v)
	{
		if (v == null)
		{
			return;
		}
		if (v.CheckFlagsAnd(VisualFlags.IsUIElement))
		{
			UIElement obj = (UIElement)v;
			obj.InvalidateMeasureInternal();
			obj.InvalidateArrangeInternal();
		}
		int internalVisualChildrenCount = v.InternalVisualChildrenCount;
		for (int i = 0; i < internalVisualChildrenCount; i++)
		{
			Visual visual = v.InternalGetVisualChild(i);
			if (visual != null)
			{
				markTreeDirtyHelper(visual);
			}
		}
	}

	private void NeedsRecalc()
	{
		if (!_layoutRequestPosted && !_isUpdating)
		{
			MediaContext.From(base.Dispatcher).BeginInvokeOnRender(_updateCallback, this);
			_layoutRequestPosted = true;
		}
	}

	private static object UpdateLayoutBackground(object arg)
	{
		((ContextLayoutManager)arg).NeedsRecalc();
		return null;
	}

	internal void EnterMeasure()
	{
		base.Dispatcher._disableProcessingCount++;
		_lastExceptionElement = null;
		_measuresOnStack++;
		if (_measuresOnStack > s_LayoutRecursionLimit)
		{
			throw new InvalidOperationException(SR.Format(SR.LayoutManager_DeepRecursion, s_LayoutRecursionLimit));
		}
		_firePostLayoutEvents = true;
	}

	internal void ExitMeasure()
	{
		_measuresOnStack--;
		base.Dispatcher._disableProcessingCount--;
	}

	internal void EnterArrange()
	{
		base.Dispatcher._disableProcessingCount++;
		_lastExceptionElement = null;
		_arrangesOnStack++;
		if (_arrangesOnStack > s_LayoutRecursionLimit)
		{
			throw new InvalidOperationException(SR.Format(SR.LayoutManager_DeepRecursion, s_LayoutRecursionLimit));
		}
		_firePostLayoutEvents = true;
	}

	internal void ExitArrange()
	{
		_arrangesOnStack--;
		base.Dispatcher._disableProcessingCount--;
	}

	internal void UpdateLayout()
	{
		VerifyAccess();
		if (_isInUpdateLayout || _measuresOnStack > 0 || _arrangesOnStack > 0 || _isDead)
		{
			return;
		}
		bool flag = false;
		long num = 0L;
		if (!_isUpdating && EventTrace.IsEnabled(EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordLayout, EventTrace.Level.Info))
		{
			flag = true;
			num = PerfService.GetPerfElementID(this);
			EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientLayoutBegin, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordLayout, EventTrace.Level.Info, num, EventTrace.LayoutSource.LayoutManager);
		}
		int num2 = 0;
		bool flag2 = true;
		UIElement uIElement = null;
		try
		{
			invalidateTreeIfRecovering();
			while (hasDirtiness || _firePostLayoutEvents)
			{
				if (++num2 > 153)
				{
					base.Dispatcher.BeginInvoke(DispatcherPriority.Background, _updateLayoutBackground, this);
					uIElement = null;
					flag2 = false;
					if (flag)
					{
						EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientLayoutAbort, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordLayout, EventTrace.Level.Info, 0, num2);
					}
					return;
				}
				_isUpdating = true;
				_isInUpdateLayout = true;
				if (flag)
				{
					EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientMeasureBegin, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordLayout, EventTrace.Level.Info, num);
				}
				using (base.Dispatcher.DisableProcessing())
				{
					int num3 = 0;
					DateTime dateTime = new DateTime(0L);
					while (true)
					{
						if (++num3 > 153)
						{
							num3 = 0;
							if (dateTime.Ticks == 0L)
							{
								dateTime = DateTime.UtcNow;
							}
							else
							{
								TimeSpan timeSpan = DateTime.UtcNow - dateTime;
								if (timeSpan.Milliseconds > 306)
								{
									base.Dispatcher.BeginInvoke(DispatcherPriority.Background, _updateLayoutBackground, this);
									uIElement = null;
									flag2 = false;
									if (flag)
									{
										EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientMeasureAbort, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordLayout, EventTrace.Level.Info, timeSpan.Milliseconds, num3);
									}
									return;
								}
							}
						}
						uIElement = MeasureQueue.GetTopMost();
						if (uIElement == null)
						{
							break;
						}
						uIElement.Measure(uIElement.PreviousConstraint);
					}
					if (flag)
					{
						EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientMeasureEnd, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordLayout, EventTrace.Level.Info, num3);
						EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientArrangeBegin, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordLayout, EventTrace.Level.Info, num);
					}
					num3 = 0;
					dateTime = new DateTime(0L);
					while (MeasureQueue.IsEmpty)
					{
						if (++num3 > 153)
						{
							num3 = 0;
							if (dateTime.Ticks == 0L)
							{
								dateTime = DateTime.UtcNow;
							}
							else
							{
								TimeSpan timeSpan2 = DateTime.UtcNow - dateTime;
								if (timeSpan2.Milliseconds > 306)
								{
									base.Dispatcher.BeginInvoke(DispatcherPriority.Background, _updateLayoutBackground, this);
									uIElement = null;
									flag2 = false;
									if (flag)
									{
										EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientArrangeAbort, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordLayout, EventTrace.Level.Info, timeSpan2.Milliseconds, num3);
									}
									return;
								}
							}
						}
						uIElement = ArrangeQueue.GetTopMost();
						if (uIElement == null)
						{
							break;
						}
						Rect properArrangeRect = getProperArrangeRect(uIElement);
						uIElement.Arrange(properArrangeRect);
					}
					if (flag)
					{
						EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientArrangeEnd, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordLayout, EventTrace.Level.Info, num3);
					}
					if (!MeasureQueue.IsEmpty)
					{
						continue;
					}
					_isInUpdateLayout = false;
					goto IL_0345;
				}
				IL_0345:
				fireSizeChangedEvents();
				if (hasDirtiness)
				{
					continue;
				}
				fireLayoutUpdateEvent();
				if (!hasDirtiness)
				{
					fireAutomationEvents();
					if (!hasDirtiness)
					{
						fireSizeChangedEvents();
					}
				}
			}
			uIElement = null;
			flag2 = false;
		}
		finally
		{
			_isUpdating = false;
			_layoutRequestPosted = false;
			_isInUpdateLayout = false;
			if (flag2)
			{
				if (flag)
				{
					EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientLayoutException, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordLayout, EventTrace.Level.Info, PerfService.GetPerfElementID(uIElement));
				}
				_gotException = true;
				_forceLayoutElement = uIElement;
				base.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, _updateLayoutBackground, this);
			}
		}
		Font.ResetFontFaceCache();
		BufferCache.Reset();
		if (flag)
		{
			EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientLayoutEnd, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordLayout, EventTrace.Level.Info);
		}
	}

	private Rect getProperArrangeRect(UIElement element)
	{
		Rect previousArrangeRect = element.PreviousArrangeRect;
		if (element.GetUIParentNo3DTraversal() == null)
		{
			double x = (previousArrangeRect.Y = 0.0);
			previousArrangeRect.X = x;
			if (double.IsPositiveInfinity(element.PreviousConstraint.Width))
			{
				previousArrangeRect.Width = element.DesiredSize.Width;
			}
			if (double.IsPositiveInfinity(element.PreviousConstraint.Height))
			{
				previousArrangeRect.Height = element.DesiredSize.Height;
			}
		}
		return previousArrangeRect;
	}

	private void invalidateTreeIfRecovering()
	{
		if (_forceLayoutElement != null || _gotException)
		{
			if (_forceLayoutElement != null)
			{
				markTreeDirty(_forceLayoutElement);
			}
			_forceLayoutElement = null;
			_gotException = false;
		}
	}

	private static object UpdateLayoutCallback(object arg)
	{
		if (arg is ContextLayoutManager contextLayoutManager)
		{
			contextLayoutManager.UpdateLayout();
		}
		return null;
	}

	private void fireLayoutUpdateEvent()
	{
		if (_inFireLayoutUpdated)
		{
			return;
		}
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordLayout, EventTrace.Level.Verbose, EventTrace.Event.WClientLayoutFireLayoutUpdatedBegin);
		try
		{
			_inFireLayoutUpdated = true;
			LayoutEventList.ListItem[] array = LayoutEvents.CopyToArray();
			foreach (LayoutEventList.ListItem listItem in array)
			{
				EventHandler eventHandler = null;
				try
				{
					eventHandler = (EventHandler)listItem.Target;
				}
				catch (InvalidOperationException)
				{
					eventHandler = null;
				}
				if (eventHandler != null)
				{
					eventHandler(null, EventArgs.Empty);
					if (hasDirtiness)
					{
						break;
					}
				}
				else
				{
					LayoutEvents.Remove(listItem);
				}
			}
		}
		finally
		{
			_inFireLayoutUpdated = false;
			EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordLayout, EventTrace.Level.Verbose, EventTrace.Event.WClientLayoutFireLayoutUpdatedEnd);
		}
	}

	internal void AddToSizeChangedChain(SizeChangedInfo info)
	{
		info.Next = _sizeChangedChain;
		_sizeChangedChain = info;
	}

	private void fireSizeChangedEvents()
	{
		if (_inFireSizeChanged)
		{
			return;
		}
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordLayout, EventTrace.Level.Verbose, EventTrace.Event.WClientLayoutFireSizeChangedBegin);
		try
		{
			_inFireSizeChanged = true;
			while (_sizeChangedChain != null)
			{
				SizeChangedInfo sizeChangedChain = _sizeChangedChain;
				_sizeChangedChain = sizeChangedChain.Next;
				sizeChangedChain.Element.sizeChangedInfo = null;
				sizeChangedChain.Element.OnRenderSizeChanged(sizeChangedChain);
				if (hasDirtiness)
				{
					break;
				}
			}
		}
		finally
		{
			_inFireSizeChanged = false;
			EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordLayout, EventTrace.Level.Verbose, EventTrace.Event.WClientLayoutFireSizeChangedEnd);
		}
	}

	private void fireAutomationEvents()
	{
		if (_inFireAutomationEvents)
		{
			return;
		}
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordLayout, EventTrace.Level.Verbose, EventTrace.Event.WClientLayoutFireAutomationEventsBegin);
		try
		{
			_inFireAutomationEvents = true;
			_firePostLayoutEvents = false;
			LayoutEventList.ListItem[] array = AutomationEvents.CopyToArray();
			foreach (LayoutEventList.ListItem listItem in array)
			{
				AutomationPeer automationPeer = null;
				try
				{
					automationPeer = (AutomationPeer)listItem.Target;
				}
				catch (InvalidOperationException)
				{
					automationPeer = null;
				}
				if (automationPeer != null)
				{
					automationPeer.FireAutomationEvents();
					if (hasDirtiness)
					{
						break;
					}
				}
				else
				{
					AutomationEvents.Remove(listItem);
				}
			}
		}
		finally
		{
			_inFireAutomationEvents = false;
			EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordLayout, EventTrace.Level.Verbose, EventTrace.Event.WClientLayoutFireAutomationEventsEnd);
		}
	}

	internal AutomationPeer[] GetAutomationRoots()
	{
		LayoutEventList.ListItem[] array = AutomationEvents.CopyToArray();
		AutomationPeer[] array2 = new AutomationPeer[array.Length];
		int num = 0;
		foreach (LayoutEventList.ListItem listItem in array)
		{
			AutomationPeer automationPeer = null;
			try
			{
				automationPeer = (AutomationPeer)listItem.Target;
			}
			catch (InvalidOperationException)
			{
				automationPeer = null;
			}
			if (automationPeer != null)
			{
				array2[num++] = automationPeer;
			}
		}
		return array2;
	}

	internal UIElement GetLastExceptionElement()
	{
		return _lastExceptionElement;
	}

	internal void SetLastExceptionElement(UIElement e)
	{
		_lastExceptionElement = e;
	}
}
