using System;
using System.Diagnostics;

namespace MS.Internal;

internal static class TracePageFormatting
{
	private static AvTrace _avTrace = new AvTrace(() => PresentationTraceSources.DocumentsSource, delegate
	{
		PresentationTraceSources._DocumentsSource = null;
	});

	private static AvTraceDetails _FormatPage;

	private static AvTraceDetails _PageFormattingError;

	private static AvTraceDetails _UnableToFreezeFreezableSubProperty;

	private static AvTraceDetails _UnableToFreezeAnimatedProperties;

	public static AvTraceDetails FormatPage
	{
		get
		{
			if (_FormatPage == null)
			{
				_FormatPage = new AvTraceDetails(1, new string[1] { "Formatting page" });
			}
			return _FormatPage;
		}
	}

	public static AvTraceDetails PageFormattingError
	{
		get
		{
			if (_PageFormattingError == null)
			{
				_PageFormattingError = new AvTraceDetails(2, new string[1] { "Error. Page formatting engine could not complete the formatting operation." });
			}
			return _PageFormattingError;
		}
	}

	public static AvTraceDetails UnableToFreezeFreezableSubProperty
	{
		get
		{
			if (_UnableToFreezeFreezableSubProperty == null)
			{
				_UnableToFreezeFreezableSubProperty = new AvTraceDetails(3, new string[1] { "CanFreeze is returning false because a DependencyProperty on the Freezable has a value that is a Freezable that has also returned false from CanFreeze" });
			}
			return _UnableToFreezeFreezableSubProperty;
		}
	}

	public static AvTraceDetails UnableToFreezeAnimatedProperties
	{
		get
		{
			if (_UnableToFreezeAnimatedProperties == null)
			{
				_UnableToFreezeAnimatedProperties = new AvTraceDetails(4, new string[1] { "CanFreeze is returning false because at least one DependencyProperty on the Freezable is animated." });
			}
			return _UnableToFreezeAnimatedProperties;
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
