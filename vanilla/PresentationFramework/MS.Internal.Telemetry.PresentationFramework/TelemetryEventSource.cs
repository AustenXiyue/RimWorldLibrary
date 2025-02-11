using System.Diagnostics.Tracing;

namespace MS.Internal.Telemetry.PresentationFramework;

internal class TelemetryEventSource : EventSource
{
	internal const EventKeywords Reserved44Keyword = (EventKeywords)17592186044416L;

	internal const EventKeywords TelemetryKeyword = (EventKeywords)35184372088832L;

	internal const EventKeywords MeasuresKeyword = (EventKeywords)70368744177664L;

	internal const EventKeywords CriticalDataKeyword = (EventKeywords)140737488355328L;

	internal const EventTags CoreData = (EventTags)524288;

	internal const EventTags InjectXToken = (EventTags)1048576;

	internal const EventTags RealtimeLatency = (EventTags)2097152;

	internal const EventTags NormalLatency = (EventTags)4194304;

	internal const EventTags CriticalPersistence = (EventTags)8388608;

	internal const EventTags NormalPersistence = (EventTags)16777216;

	internal const EventTags DropPii = (EventTags)33554432;

	internal const EventTags HashPii = (EventTags)67108864;

	internal const EventTags MarkPii = (EventTags)134217728;

	internal const EventFieldTags DropPiiField = (EventFieldTags)67108864;

	internal const EventFieldTags HashPiiField = (EventFieldTags)134217728;

	private static readonly string[] telemetryTraits = new string[2] { "ETW_GROUP", "{4f50731a-89cf-4782-b3e0-dce8c90476ba}" };

	internal TelemetryEventSource(string eventSourceName)
		: base(eventSourceName, EventSourceSettings.EtwSelfDescribingEventFormat, telemetryTraits)
	{
	}

	protected TelemetryEventSource()
		: base(EventSourceSettings.EtwSelfDescribingEventFormat, telemetryTraits)
	{
	}

	internal static EventSourceOptions TelemetryOptions()
	{
		EventSourceOptions result = default(EventSourceOptions);
		result.Keywords = (EventKeywords)35184372088832L;
		return result;
	}

	internal static EventSourceOptions MeasuresOptions()
	{
		EventSourceOptions result = default(EventSourceOptions);
		result.Keywords = (EventKeywords)70368744177664L;
		return result;
	}

	internal static EventSourceOptions CriticalDataOptions()
	{
		EventSourceOptions result = default(EventSourceOptions);
		result.Keywords = (EventKeywords)140737488355328L;
		return result;
	}
}
