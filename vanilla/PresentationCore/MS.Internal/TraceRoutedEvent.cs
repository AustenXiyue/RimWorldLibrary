using System;
using System.Diagnostics;

namespace MS.Internal;

internal static class TraceRoutedEvent
{
	private static AvTrace _avTrace = new AvTrace(() => PresentationTraceSources.RoutedEventSource, delegate
	{
		PresentationTraceSources._RoutedEventSource = null;
	});

	private static AvTraceDetails _RaiseEvent;

	private static AvTraceDetails _ReRaiseEventAs;

	private static AvTraceDetails _HandleEvent;

	private static AvTraceDetails _InvokeHandlers;

	public static AvTraceDetails RaiseEvent
	{
		get
		{
			if (_RaiseEvent == null)
			{
				_RaiseEvent = new AvTraceDetails(1, new string[1] { "Raise RoutedEvent" });
			}
			return _RaiseEvent;
		}
	}

	public static AvTraceDetails ReRaiseEventAs
	{
		get
		{
			if (_ReRaiseEventAs == null)
			{
				_ReRaiseEventAs = new AvTraceDetails(2, new string[1] { "Raise RoutedEvent" });
			}
			return _ReRaiseEventAs;
		}
	}

	public static AvTraceDetails HandleEvent
	{
		get
		{
			if (_HandleEvent == null)
			{
				_HandleEvent = new AvTraceDetails(3, new string[1] { "RoutedEvent has set Handled" });
			}
			return _HandleEvent;
		}
	}

	public static AvTraceDetails InvokeHandlers
	{
		get
		{
			if (_InvokeHandlers == null)
			{
				_InvokeHandlers = new AvTraceDetails(4, new string[1] { "InvokeHandlers" });
			}
			return _InvokeHandlers;
		}
	}

	public static bool IsEnabled
	{
		get
		{
			if (_avTrace != null)
			{
				return _avTrace.IsEnabled;
			}
			return false;
		}
	}

	public static bool IsEnabledOverride => _avTrace.IsEnabledOverride;

	public static void Trace(TraceEventType type, AvTraceDetails traceDetails, params object[] parameters)
	{
		_avTrace.Trace(type, traceDetails.Id, traceDetails.Message, traceDetails.Labels, parameters);
	}

	public static void Trace(TraceEventType type, AvTraceDetails traceDetails)
	{
		_avTrace.Trace(type, traceDetails.Id, traceDetails.Message, traceDetails.Labels, Array.Empty<object>());
	}

	public static void Trace(TraceEventType type, AvTraceDetails traceDetails, object p1)
	{
		_avTrace.Trace(type, traceDetails.Id, traceDetails.Message, traceDetails.Labels, new object[1] { p1 });
	}

	public static void Trace(TraceEventType type, AvTraceDetails traceDetails, object p1, object p2)
	{
		_avTrace.Trace(type, traceDetails.Id, traceDetails.Message, traceDetails.Labels, new object[2] { p1, p2 });
	}

	public static void Trace(TraceEventType type, AvTraceDetails traceDetails, object p1, object p2, object p3)
	{
		_avTrace.Trace(type, traceDetails.Id, traceDetails.Message, traceDetails.Labels, new object[3] { p1, p2, p3 });
	}

	public static void TraceActivityItem(AvTraceDetails traceDetails, params object[] parameters)
	{
		_avTrace.TraceStartStop(traceDetails.Id, traceDetails.Message, traceDetails.Labels, parameters);
	}

	public static void TraceActivityItem(AvTraceDetails traceDetails)
	{
		_avTrace.TraceStartStop(traceDetails.Id, traceDetails.Message, traceDetails.Labels, Array.Empty<object>());
	}

	public static void TraceActivityItem(AvTraceDetails traceDetails, object p1)
	{
		_avTrace.TraceStartStop(traceDetails.Id, traceDetails.Message, traceDetails.Labels, new object[1] { p1 });
	}

	public static void TraceActivityItem(AvTraceDetails traceDetails, object p1, object p2)
	{
		_avTrace.TraceStartStop(traceDetails.Id, traceDetails.Message, traceDetails.Labels, new object[2] { p1, p2 });
	}

	public static void TraceActivityItem(AvTraceDetails traceDetails, object p1, object p2, object p3)
	{
		_avTrace.TraceStartStop(traceDetails.Id, traceDetails.Message, traceDetails.Labels, new object[3] { p1, p2, p3 });
	}

	public static void Refresh()
	{
		_avTrace.Refresh();
	}
}
