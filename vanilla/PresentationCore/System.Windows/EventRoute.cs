using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using MS.Internal;
using MS.Internal.PresentationCore;
using MS.Utility;

namespace System.Windows;

/// <summary>Represents the container for the route to be followed by a routed event. </summary>
public sealed class EventRoute
{
	private struct BranchNode
	{
		public object Node;

		public object Source;
	}

	private RoutedEvent _routedEvent;

	private FrugalStructList<RouteItem> _routeItemList;

	private Stack<BranchNode> _branchNodeStack;

	private FrugalStructList<SourceItem> _sourceItemList;

	internal RoutedEvent RoutedEvent
	{
		get
		{
			return _routedEvent;
		}
		set
		{
			_routedEvent = value;
		}
	}

	/// <summary> Initializes an instance of the <see cref="T:System.Windows.EventRoute" /> class. </summary>
	/// <param name="routedEvent">The non-NULL event identifier to be associated with this event route.</param>
	public EventRoute(RoutedEvent routedEvent)
	{
		if (routedEvent == null)
		{
			throw new ArgumentNullException("routedEvent");
		}
		_routedEvent = routedEvent;
		_routeItemList = new FrugalStructList<RouteItem>(16);
		_sourceItemList = new FrugalStructList<SourceItem>(16);
	}

	/// <summary> Adds the specified handler for the specified target to the route. </summary>
	/// <param name="target">Specifies the target object of which the handler is to be added to the route.</param>
	/// <param name="handler">Specifies the handler to be added to the route.</param>
	/// <param name="handledEventsToo">Indicates whether or not the listener detects events that have already been handled.</param>
	public void Add(object target, Delegate handler, bool handledEventsToo)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		if ((object)handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		RouteItem value = new RouteItem(target, new RoutedEventHandlerInfo(handler, handledEventsToo));
		_routeItemList.Add(value);
	}

	internal void InvokeHandlers(object source, RoutedEventArgs args)
	{
		InvokeHandlersImpl(source, args, reRaised: false);
	}

	internal void ReInvokeHandlers(object source, RoutedEventArgs args)
	{
		InvokeHandlersImpl(source, args, reRaised: true);
	}

	private void InvokeHandlersImpl(object source, RoutedEventArgs args, bool reRaised)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (args == null)
		{
			throw new ArgumentNullException("args");
		}
		if (args.Source == null)
		{
			throw new ArgumentException(SR.SourceNotSet);
		}
		if (args.RoutedEvent != _routedEvent)
		{
			throw new ArgumentException(SR.Mismatched_RoutedEvent);
		}
		if (args.RoutedEvent.RoutingStrategy == RoutingStrategy.Bubble || args.RoutedEvent.RoutingStrategy == RoutingStrategy.Direct)
		{
			int endIndex = 0;
			for (int i = 0; i < _routeItemList.Count; i++)
			{
				if (i >= endIndex)
				{
					object bubbleSource = GetBubbleSource(i, out endIndex);
					if (!reRaised)
					{
						if (bubbleSource == null)
						{
							args.Source = source;
						}
						else
						{
							args.Source = bubbleSource;
						}
					}
				}
				if (TraceRoutedEvent.IsEnabled)
				{
					TraceRoutedEvent.Trace(TraceEventType.Start, TraceRoutedEvent.InvokeHandlers, _routeItemList[i].Target, args, args.Handled);
				}
				_routeItemList[i].InvokeHandler(args);
				if (TraceRoutedEvent.IsEnabled)
				{
					TraceRoutedEvent.Trace(TraceEventType.Stop, TraceRoutedEvent.InvokeHandlers, _routeItemList[i].Target, args, args.Handled);
				}
			}
			return;
		}
		int startIndex = _routeItemList.Count;
		int num = _routeItemList.Count - 1;
		while (num >= 0)
		{
			object target = _routeItemList[num].Target;
			int num2 = num;
			while (num2 >= 0 && _routeItemList[num2].Target == target)
			{
				num2--;
			}
			for (int j = num2 + 1; j <= num; j++)
			{
				if (j < startIndex)
				{
					object tunnelSource = GetTunnelSource(j, out startIndex);
					if (tunnelSource == null)
					{
						args.Source = source;
					}
					else
					{
						args.Source = tunnelSource;
					}
				}
				if (TraceRoutedEvent.IsEnabled)
				{
					TraceRoutedEvent.Trace(TraceEventType.Start, TraceRoutedEvent.InvokeHandlers, _routeItemList[j].Target, args, args.Handled);
				}
				_routeItemList[j].InvokeHandler(args);
				if (TraceRoutedEvent.IsEnabled)
				{
					TraceRoutedEvent.Trace(TraceEventType.Stop, TraceRoutedEvent.InvokeHandlers, _routeItemList[j].Target, args, args.Handled);
				}
			}
			num = num2;
		}
	}

	/// <summary>Adds the top-most node to the event route stack at which two logical trees diverge.</summary>
	/// <param name="node">The top-most element on the event route stack at which two logical trees diverge.</param>
	/// <param name="source">The source for the top-most element on the event route stack at which two logical trees diverge.</param>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public void PushBranchNode(object node, object source)
	{
		BranchNode item = default(BranchNode);
		item.Node = node;
		item.Source = source;
		(_branchNodeStack ?? (_branchNodeStack = new Stack<BranchNode>(1))).Push(item);
	}

	/// <summary>Returns the top-most node on the event route stack at which two logical trees diverge.</summary>
	/// <returns>The top-most node on the event route stack at which two logical trees diverge.</returns>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public object PopBranchNode()
	{
		Stack<BranchNode> branchNodeStack = _branchNodeStack;
		if (branchNodeStack != null && branchNodeStack.Count > 0)
		{
			return branchNodeStack.Pop().Node;
		}
		return null;
	}

	/// <summary>Returns the top-most element on the event route stack at which two logical trees diverge.</summary>
	/// <returns>The top-most element on the event route stack at which two logical trees diverge.</returns>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public object PeekBranchNode()
	{
		Stack<BranchNode> branchNodeStack = _branchNodeStack;
		if (branchNodeStack != null && branchNodeStack.Count > 0)
		{
			return branchNodeStack.Peek().Node;
		}
		return null;
	}

	/// <summary>Returns the source for the top-most element on the event route stack at which two logical trees diverge.</summary>
	/// <returns>The source for the top-most element on the event route stack at which two logical trees diverge.</returns>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public object PeekBranchSource()
	{
		Stack<BranchNode> branchNodeStack = _branchNodeStack;
		if (branchNodeStack != null && branchNodeStack.Count > 0)
		{
			return branchNodeStack.Peek().Source;
		}
		return null;
	}

	internal void AddSource(object source)
	{
		int count = _routeItemList.Count;
		_sourceItemList.Add(new SourceItem(count, source));
	}

	private object GetBubbleSource(int index, out int endIndex)
	{
		if (_sourceItemList.Count == 0)
		{
			endIndex = _routeItemList.Count;
			return null;
		}
		if (index < _sourceItemList[0].StartIndex)
		{
			endIndex = _sourceItemList[0].StartIndex;
			return null;
		}
		for (int i = 0; i < _sourceItemList.Count - 1; i++)
		{
			if (index >= _sourceItemList[i].StartIndex && index < _sourceItemList[i + 1].StartIndex)
			{
				endIndex = _sourceItemList[i + 1].StartIndex;
				return _sourceItemList[i].Source;
			}
		}
		endIndex = _routeItemList.Count;
		return _sourceItemList[_sourceItemList.Count - 1].Source;
	}

	private object GetTunnelSource(int index, out int startIndex)
	{
		if (_sourceItemList.Count == 0)
		{
			startIndex = 0;
			return null;
		}
		if (index < _sourceItemList[0].StartIndex)
		{
			startIndex = 0;
			return null;
		}
		for (int i = 0; i < _sourceItemList.Count - 1; i++)
		{
			if (index >= _sourceItemList[i].StartIndex && index < _sourceItemList[i + 1].StartIndex)
			{
				startIndex = _sourceItemList[i].StartIndex;
				return _sourceItemList[i].Source;
			}
		}
		startIndex = _sourceItemList[_sourceItemList.Count - 1].StartIndex;
		return _sourceItemList[_sourceItemList.Count - 1].Source;
	}

	internal void Clear()
	{
		_routedEvent = null;
		_routeItemList.Clear();
		if (_branchNodeStack != null)
		{
			_branchNodeStack.Clear();
		}
		_sourceItemList.Clear();
	}
}
