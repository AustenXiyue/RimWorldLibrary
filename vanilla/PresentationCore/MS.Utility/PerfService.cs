using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using MS.Internal.PresentationCore;
using MS.Win32.PresentationCore;

namespace MS.Utility;

[FriendAccessAllowed]
internal static class PerfService
{
	private static ConditionalWeakTable<object, object> perfElementIds = new ConditionalWeakTable<object, object>();

	internal static long GetPerfElementID2(object element, string extraData)
	{
		return (long)perfElementIds.GetValue(element, delegate(object key)
		{
			long nextPerfElementId = SafeNativeMethods.GetNextPerfElementId();
			if (EventTrace.IsEnabled(EventTrace.Keyword.KeywordGeneral, EventTrace.Level.Verbose))
			{
				Type type = key.GetType();
				Assembly assembly = type.Assembly;
				if (key != assembly)
				{
					EventTrace.EventProvider.TraceEvent(EventTrace.Event.PerfElementIDAssignment, EventTrace.Keyword.KeywordGeneral, EventTrace.Level.Verbose, nextPerfElementId, type.FullName, extraData, GetPerfElementID2(assembly, assembly.FullName));
				}
			}
			return nextPerfElementId;
		});
	}

	internal static long GetPerfElementID(object element)
	{
		return GetPerfElementID2(element, string.Empty);
	}
}
