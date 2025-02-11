namespace System.Diagnostics;

/// <summary>Specifies how to handle entries in an event log that has reached its maximum file size.</summary>
/// <filterpriority>2</filterpriority>
public enum OverflowAction
{
	/// <summary>Indicates that existing entries are retained when the event log is full and new entries are discarded.</summary>
	DoNotOverwrite = -1,
	/// <summary>Indicates that each new entry overwrites the oldest entry when the event log is full.</summary>
	OverwriteAsNeeded,
	/// <summary>Indicates that new events overwrite events older than specified by the <see cref="P:System.Diagnostics.EventLog.MinimumRetentionDays" /> property value when the event log is full. New events are discarded if the event log is full and there are no events older than specified by the <see cref="P:System.Diagnostics.EventLog.MinimumRetentionDays" /> property value.</summary>
	OverwriteOlder
}
