namespace System.Diagnostics.Eventing.Reader;

/// <summary>Defines the standard event levels that are used in the Event Log service. The level defines the severity of the event. Custom event levels can be defined beyond these standard levels. For more information about levels, see <see cref="T:System.Diagnostics.Eventing.Reader.EventLevel" />.</summary>
/// <filterpriority>2</filterpriority>
public enum StandardEventLevel
{
	/// <summary>This level corresponds to critical errors, which is a serious error that has caused a major failure. </summary>
	Critical = 1,
	/// <summary>This level corresponds to normal errors that signify a problem. </summary>
	Error = 2,
	/// <summary>This level corresponds to informational events or messages that are not errors. These events can help trace the progress or state of an application.</summary>
	Informational = 4,
	/// <summary>This value indicates that not filtering on the level is done during the event publishing.</summary>
	LogAlways = 0,
	/// <summary>This level corresponds to lengthy events or messages. </summary>
	Verbose = 5,
	/// <summary>This level corresponds to warning events. For example, an event that gets published because a disk is nearing full capacity is a warning event.</summary>
	Warning = 3
}
