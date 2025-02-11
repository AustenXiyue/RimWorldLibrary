using System;
using System.Diagnostics;

namespace MS.Internal;

internal static class TraceMarkup
{
	private static AvTrace _avTrace = new AvTrace(() => PresentationTraceSources.MarkupSource, delegate
	{
		PresentationTraceSources._MarkupSource = null;
	});

	private static AvTraceDetails _AddValueToAddChild;

	private static AvTraceDetails _AddValueToArray;

	private static AvTraceDetails _AddValueToDictionary;

	private static AvTraceDetails _AddValueToList;

	private static AvTraceDetails _BeginInit;

	private static AvTraceDetails _CreateMarkupExtension;

	private static AvTraceDetails _CreateObject;

	private static AvTraceDetails _EndInit;

	private static AvTraceDetails _Load;

	private static AvTraceDetails _ProcessConstructorParameter;

	private static AvTraceDetails _ProvideValue;

	private static AvTraceDetails _SetCPA;

	private static AvTraceDetails _SetPropertyValue;

	private static AvTraceDetails _ThrowException;

	private static AvTraceDetails _TypeConvert;

	private static AvTraceDetails _TypeConvertFallback;

	public static AvTraceDetails AddValueToAddChild
	{
		get
		{
			if (_AddValueToAddChild == null)
			{
				_AddValueToAddChild = new AvTraceDetails(1, new string[1] { "Add value to IAddChild" });
			}
			return _AddValueToAddChild;
		}
	}

	public static AvTraceDetails AddValueToArray
	{
		get
		{
			if (_AddValueToArray == null)
			{
				_AddValueToArray = new AvTraceDetails(2, new string[1] { "Add value to an array property" });
			}
			return _AddValueToArray;
		}
	}

	public static AvTraceDetails AddValueToDictionary
	{
		get
		{
			if (_AddValueToDictionary == null)
			{
				_AddValueToDictionary = new AvTraceDetails(3, new string[1] { "Add value to a dictionary property" });
			}
			return _AddValueToDictionary;
		}
	}

	public static AvTraceDetails AddValueToList
	{
		get
		{
			if (_AddValueToList == null)
			{
				_AddValueToList = new AvTraceDetails(4, new string[1] { "CanFreezeAdd value to a collection property" });
			}
			return _AddValueToList;
		}
	}

	public static AvTraceDetails BeginInit
	{
		get
		{
			if (_BeginInit == null)
			{
				_BeginInit = new AvTraceDetails(5, new string[1] { "Start initialization of object (ISupportInitialize.BeginInit)" });
			}
			return _BeginInit;
		}
	}

	public static AvTraceDetails CreateMarkupExtension
	{
		get
		{
			if (_CreateMarkupExtension == null)
			{
				_CreateMarkupExtension = new AvTraceDetails(6, new string[1] { "Create MarkupExtension" });
			}
			return _CreateMarkupExtension;
		}
	}

	public static AvTraceDetails CreateObject
	{
		get
		{
			if (_CreateObject == null)
			{
				_CreateObject = new AvTraceDetails(7, new string[1] { "Create object" });
			}
			return _CreateObject;
		}
	}

	public static AvTraceDetails EndInit
	{
		get
		{
			if (_EndInit == null)
			{
				_EndInit = new AvTraceDetails(8, new string[1] { "End initialization of object (ISupportInitialize.EndInit)" });
			}
			return _EndInit;
		}
	}

	public static AvTraceDetails Load
	{
		get
		{
			if (_Load == null)
			{
				_Load = new AvTraceDetails(9, new string[1] { "Load xaml/baml" });
			}
			return _Load;
		}
	}

	public static AvTraceDetails ProcessConstructorParameter
	{
		get
		{
			if (_ProcessConstructorParameter == null)
			{
				_ProcessConstructorParameter = new AvTraceDetails(10, new string[1] { "Convert constructor parameter" });
			}
			return _ProcessConstructorParameter;
		}
	}

	public static AvTraceDetails ProvideValue
	{
		get
		{
			if (_ProvideValue == null)
			{
				_ProvideValue = new AvTraceDetails(11, new string[1] { "Converted a MarkupExtension" });
			}
			return _ProvideValue;
		}
	}

	public static AvTraceDetails SetCPA
	{
		get
		{
			if (_SetCPA == null)
			{
				_SetCPA = new AvTraceDetails(12, new string[1] { "Set property value to the ContentProperty" });
			}
			return _SetCPA;
		}
	}

	public static AvTraceDetails SetPropertyValue
	{
		get
		{
			if (_SetPropertyValue == null)
			{
				_SetPropertyValue = new AvTraceDetails(13, new string[1] { "Set property value" });
			}
			return _SetPropertyValue;
		}
	}

	public static AvTraceDetails ThrowException
	{
		get
		{
			if (_ThrowException == null)
			{
				_ThrowException = new AvTraceDetails(14, new string[1] { "A xaml exception has been thrown" });
			}
			return _ThrowException;
		}
	}

	public static AvTraceDetails TypeConvert
	{
		get
		{
			if (_TypeConvert == null)
			{
				_TypeConvert = new AvTraceDetails(15, new string[1] { "Converted value" });
			}
			return _TypeConvert;
		}
	}

	public static AvTraceDetails TypeConvertFallback
	{
		get
		{
			if (_TypeConvertFallback == null)
			{
				_TypeConvertFallback = new AvTraceDetails(16, new string[1] { "CanFreezeAdd value to a collection property" });
			}
			return _TypeConvertFallback;
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
