using System;
using System.Diagnostics;

namespace MS.Internal;

internal static class TraceShell
{
	private static AvTrace _avTrace;

	private static AvTraceDetails _NotOnWindows7;

	private static AvTraceDetails _ExplorerTaskbarTimeout;

	private static AvTraceDetails _ExplorerTaskbarRetrying;

	private static AvTraceDetails _ExplorerTaskbarNotRunning;

	private static AvTraceDetails _NativeTaskbarError;

	private static AvTraceDetails _RejectingJumpItemsBecauseCatastrophicFailure;

	private static AvTraceDetails _RejectingJumpListCategoryBecauseNoRegisteredHandler;

	public static AvTraceDetails NotOnWindows7
	{
		get
		{
			if (_NotOnWindows7 == null)
			{
				_NotOnWindows7 = new AvTraceDetails(1, new string[1] { "Shell integration features are not being applied because the host OS does not support the feature." });
			}
			return _NotOnWindows7;
		}
	}

	public static AvTraceDetails ExplorerTaskbarTimeout
	{
		get
		{
			if (_ExplorerTaskbarTimeout == null)
			{
				_ExplorerTaskbarTimeout = new AvTraceDetails(2, new string[1] { "Communication with Explorer timed out while trying to update the taskbar item for the window." });
			}
			return _ExplorerTaskbarTimeout;
		}
	}

	public static AvTraceDetails ExplorerTaskbarRetrying
	{
		get
		{
			if (_ExplorerTaskbarRetrying == null)
			{
				_ExplorerTaskbarRetrying = new AvTraceDetails(3, new string[1] { "Making another attempt to update the taskbar." });
			}
			return _ExplorerTaskbarRetrying;
		}
	}

	public static AvTraceDetails ExplorerTaskbarNotRunning
	{
		get
		{
			if (_ExplorerTaskbarNotRunning == null)
			{
				_ExplorerTaskbarNotRunning = new AvTraceDetails(4, new string[1] { "Halting attempts at Shell integration with the taskbar because it appears that Explorer is not running." });
			}
			return _ExplorerTaskbarNotRunning;
		}
	}

	public static AvTraceDetails RejectingJumpItemsBecauseCatastrophicFailure
	{
		get
		{
			if (_RejectingJumpItemsBecauseCatastrophicFailure == null)
			{
				_RejectingJumpItemsBecauseCatastrophicFailure = new AvTraceDetails(6, new string[1] { "Failed to apply items to the JumpList because the native interfaces failed." });
			}
			return _RejectingJumpItemsBecauseCatastrophicFailure;
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

	public static AvTraceDetails NativeTaskbarError(params object[] args)
	{
		if (_NativeTaskbarError == null)
		{
			_NativeTaskbarError = new AvTraceDetails(5, new string[1] { "The native ITaskbarList3 interface failed a method call with error {0}." });
		}
		return new AvTraceFormat(_NativeTaskbarError, args);
	}

	public static AvTraceDetails RejectingJumpListCategoryBecauseNoRegisteredHandler(params object[] args)
	{
		if (_RejectingJumpListCategoryBecauseNoRegisteredHandler == null)
		{
			_RejectingJumpListCategoryBecauseNoRegisteredHandler = new AvTraceDetails(7, new string[1] { "Rejecting the category {0} from the jump list because this application is not registered for file types contained in the list.  JumpPath items will be removed and the operation will be retried." });
		}
		return new AvTraceFormat(_RejectingJumpListCategoryBecauseNoRegisteredHandler, args);
	}

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

	static TraceShell()
	{
		_avTrace = new AvTrace(() => PresentationTraceSources.ShellSource, delegate
		{
			PresentationTraceSources._ShellSource = null;
		});
		_avTrace.EnabledByDebugger = true;
	}
}
