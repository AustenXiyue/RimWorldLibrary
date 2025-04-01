namespace System.Diagnostics.Tracing;

/// <summary>Identifies the level of an event.</summary>
public enum EventLevel
{
	/// <summary>No level filtering is done on the event.</summary>
	LogAlways,
	/// <summary>This level corresponds to a critical error, which is a serious error that has caused a major failure.</summary>
	Critical,
	/// <summary>This level adds standard errors that signify a problem.</summary>
	Error,
	/// <summary>This level adds warning events (for example, events that are published because a disk is nearing full capacity).</summary>
	Warning,
	/// <summary>This level adds informational events or messages that are not errors. These events can help trace the progress or state of an application.</summary>
	Informational,
	/// <summary>This level adds lengthy events or messages. It causes all events to be logged.</summary>
	Verbose
}
