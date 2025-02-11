using System;
using System.Diagnostics;

namespace MS.Internal;

internal static class TraceAnimation
{
	private static AvTrace _avTrace = new AvTrace(() => PresentationTraceSources.AnimationSource, delegate
	{
		PresentationTraceSources._AnimationSource = null;
	});

	private static AvTraceDetails _StoryboardBegin;

	private static AvTraceDetails _StoryboardPause;

	private static AvTraceDetails _StoryboardRemove;

	private static AvTraceDetails _StoryboardResume;

	private static AvTraceDetails _StoryboardStop;

	private static AvTraceDetails _StoryboardNotApplied;

	private static AvTraceDetails _AnimateStorageValidationFailed;

	private static AvTraceDetails _AnimateStorageValidationNoLongerFailing;

	public static AvTraceDetails StoryboardBegin
	{
		get
		{
			if (_StoryboardBegin == null)
			{
				_StoryboardBegin = new AvTraceDetails(1, new string[1] { "Storyboard has begun" });
			}
			return _StoryboardBegin;
		}
	}

	public static AvTraceDetails StoryboardPause
	{
		get
		{
			if (_StoryboardPause == null)
			{
				_StoryboardPause = new AvTraceDetails(2, new string[1] { "Storyboard has been paused" });
			}
			return _StoryboardPause;
		}
	}

	public static AvTraceDetails StoryboardRemove
	{
		get
		{
			if (_StoryboardRemove == null)
			{
				_StoryboardRemove = new AvTraceDetails(3, new string[1] { "Storyboard has been removed" });
			}
			return _StoryboardRemove;
		}
	}

	public static AvTraceDetails StoryboardResume
	{
		get
		{
			if (_StoryboardResume == null)
			{
				_StoryboardResume = new AvTraceDetails(4, new string[1] { "Storyboard has been resumed" });
			}
			return _StoryboardResume;
		}
	}

	public static AvTraceDetails StoryboardStop
	{
		get
		{
			if (_StoryboardStop == null)
			{
				_StoryboardStop = new AvTraceDetails(5, new string[1] { "Storyboard has been stopped" });
			}
			return _StoryboardStop;
		}
	}

	public static AvTraceDetails StoryboardNotApplied
	{
		get
		{
			if (_StoryboardNotApplied == null)
			{
				_StoryboardNotApplied = new AvTraceDetails(6, new string[1] { "Unable to perform action because the specified Storyboard was never applied to this object for interactive control." });
			}
			return _StoryboardNotApplied;
		}
	}

	public static AvTraceDetails AnimateStorageValidationFailed
	{
		get
		{
			if (_AnimateStorageValidationFailed == null)
			{
				_AnimateStorageValidationFailed = new AvTraceDetails(7, new string[1] { "Animated property failed validation. Animated value not set." });
			}
			return _AnimateStorageValidationFailed;
		}
	}

	public static AvTraceDetails AnimateStorageValidationNoLongerFailing
	{
		get
		{
			if (_AnimateStorageValidationNoLongerFailing == null)
			{
				_AnimateStorageValidationNoLongerFailing = new AvTraceDetails(8, new string[1] { "Animated property no longer failing validation." });
			}
			return _AnimateStorageValidationNoLongerFailing;
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
