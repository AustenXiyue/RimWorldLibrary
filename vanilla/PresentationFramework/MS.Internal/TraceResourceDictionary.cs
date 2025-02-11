using System;
using System.Diagnostics;

namespace MS.Internal;

internal static class TraceResourceDictionary
{
	private static AvTrace _avTrace = new AvTrace(() => PresentationTraceSources.ResourceDictionarySource, delegate
	{
		PresentationTraceSources._ResourceDictionarySource = null;
	});

	private static AvTraceDetails _AddResource;

	private static AvTraceDetails _RealizeDeferContent;

	private static AvTraceDetails _FoundResourceOnElement;

	private static AvTraceDetails _FoundResourceInStyle;

	private static AvTraceDetails _FoundResourceInTemplate;

	private static AvTraceDetails _FoundResourceInThemeStyle;

	private static AvTraceDetails _FoundResourceInApplication;

	private static AvTraceDetails _FoundResourceInTheme;

	private static AvTraceDetails _ResourceNotFound;

	private static AvTraceDetails _NewResourceDictionary;

	private static AvTraceDetails _FindResource;

	private static AvTraceDetails _SetKey;

	public static AvTraceDetails AddResource
	{
		get
		{
			if (_AddResource == null)
			{
				_AddResource = new AvTraceDetails(1, new string[1] { "Resource has been added to ResourceDictionary" });
			}
			return _AddResource;
		}
	}

	public static AvTraceDetails RealizeDeferContent
	{
		get
		{
			if (_RealizeDeferContent == null)
			{
				_RealizeDeferContent = new AvTraceDetails(2, new string[1] { "Delayed creation of resource" });
			}
			return _RealizeDeferContent;
		}
	}

	public static AvTraceDetails FoundResourceOnElement
	{
		get
		{
			if (_FoundResourceOnElement == null)
			{
				_FoundResourceOnElement = new AvTraceDetails(3, new string[1] { "Found resource item on an element" });
			}
			return _FoundResourceOnElement;
		}
	}

	public static AvTraceDetails FoundResourceInStyle
	{
		get
		{
			if (_FoundResourceInStyle == null)
			{
				_FoundResourceInStyle = new AvTraceDetails(4, new string[1] { "Found resource item in a style" });
			}
			return _FoundResourceInStyle;
		}
	}

	public static AvTraceDetails FoundResourceInTemplate
	{
		get
		{
			if (_FoundResourceInTemplate == null)
			{
				_FoundResourceInTemplate = new AvTraceDetails(5, new string[1] { "Found resource item in a template" });
			}
			return _FoundResourceInTemplate;
		}
	}

	public static AvTraceDetails FoundResourceInThemeStyle
	{
		get
		{
			if (_FoundResourceInThemeStyle == null)
			{
				_FoundResourceInThemeStyle = new AvTraceDetails(6, new string[1] { "Found resource item in a theme style" });
			}
			return _FoundResourceInThemeStyle;
		}
	}

	public static AvTraceDetails FoundResourceInApplication
	{
		get
		{
			if (_FoundResourceInApplication == null)
			{
				_FoundResourceInApplication = new AvTraceDetails(7, new string[1] { "Found resource item in application" });
			}
			return _FoundResourceInApplication;
		}
	}

	public static AvTraceDetails FoundResourceInTheme
	{
		get
		{
			if (_FoundResourceInTheme == null)
			{
				_FoundResourceInTheme = new AvTraceDetails(8, new string[1] { "Found resource item in theme" });
			}
			return _FoundResourceInTheme;
		}
	}

	public static AvTraceDetails ResourceNotFound
	{
		get
		{
			if (_ResourceNotFound == null)
			{
				_ResourceNotFound = new AvTraceDetails(9, new string[2] { "Resource not found", "ResourceKey" });
			}
			return _ResourceNotFound;
		}
	}

	public static AvTraceDetails NewResourceDictionary
	{
		get
		{
			if (_NewResourceDictionary == null)
			{
				_NewResourceDictionary = new AvTraceDetails(10, new string[1] { "New resource dictionary set" });
			}
			return _NewResourceDictionary;
		}
	}

	public static AvTraceDetails FindResource
	{
		get
		{
			if (_FindResource == null)
			{
				_FindResource = new AvTraceDetails(11, new string[1] { "Searching for resource" });
			}
			return _FindResource;
		}
	}

	public static AvTraceDetails SetKey
	{
		get
		{
			if (_SetKey == null)
			{
				_SetKey = new AvTraceDetails(12, new string[1] { "Deferred resource has been added to ResourceDictionary" });
			}
			return _SetKey;
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
