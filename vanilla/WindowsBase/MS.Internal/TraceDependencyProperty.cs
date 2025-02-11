using System;
using System.Diagnostics;

namespace MS.Internal;

internal static class TraceDependencyProperty
{
	private static AvTrace _avTrace = new AvTrace(() => PresentationTraceSources.DependencyPropertySource, delegate
	{
		PresentationTraceSources._DependencyPropertySource = null;
	});

	private static AvTraceDetails _ApplyTemplateContent;

	private static AvTraceDetails _Register;

	private static AvTraceDetails _UpdateEffectiveValueStart;

	private static AvTraceDetails _UpdateEffectiveValueStop;

	public static AvTraceDetails ApplyTemplateContent
	{
		get
		{
			if (_ApplyTemplateContent == null)
			{
				_ApplyTemplateContent = new AvTraceDetails(1, new string[1] { "Apply template" });
			}
			return _ApplyTemplateContent;
		}
	}

	public static AvTraceDetails Register
	{
		get
		{
			if (_Register == null)
			{
				_Register = new AvTraceDetails(2, new string[1] { "Registered DependencyProperty" });
			}
			return _Register;
		}
	}

	public static AvTraceDetails UpdateEffectiveValueStart
	{
		get
		{
			if (_UpdateEffectiveValueStart == null)
			{
				_UpdateEffectiveValueStart = new AvTraceDetails(3, new string[1] { "Update effective DP value (Start)" });
			}
			return _UpdateEffectiveValueStart;
		}
	}

	public static AvTraceDetails UpdateEffectiveValueStop
	{
		get
		{
			if (_UpdateEffectiveValueStop == null)
			{
				_UpdateEffectiveValueStop = new AvTraceDetails(4, new string[1] { "Update effective DP value (Stop)" });
			}
			return _UpdateEffectiveValueStop;
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
