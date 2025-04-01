namespace System.Diagnostics;

/// <summary>Defines access levels used by <see cref="T:System.Diagnostics.EventLog" /> permission classes.</summary>
/// <filterpriority>2</filterpriority>
[Flags]
public enum EventLogPermissionAccess
{
	/// <summary>The <see cref="T:System.Diagnostics.EventLog" /> has no permissions.</summary>
	None = 0,
	/// <summary>The <see cref="T:System.Diagnostics.EventLog" /> can read existing logs. Note This member is now obsolete, use <see cref="F:System.Diagnostics.EventLogPermissionAccess.Administer" /> instead.</summary>
	[Obsolete]
	Browse = 2,
	/// <summary>The <see cref="T:System.Diagnostics.EventLog" /> can read or write to existing logs, and create event sources and logs. Note This member is now obsolete, use <see cref="F:System.Diagnostics.EventLogPermissionAccess.Write" /> instead.</summary>
	[Obsolete]
	Instrument = 6,
	/// <summary>The <see cref="T:System.Diagnostics.EventLog" /> can read existing logs, delete event sources or logs, respond to entries, clear an event log, listen to events, and access a collection of all event logs. Note This member is now obsolete, use <see cref="F:System.Diagnostics.EventLogPermissionAccess.Administer" /> instead.</summary>
	[Obsolete]
	Audit = 0xA,
	/// <summary>The <see cref="T:System.Diagnostics.EventLog" /> can write to existing logs, and create event sources and logs.</summary>
	Write = 0x10,
	/// <summary>The <see cref="T:System.Diagnostics.EventLog" /> can create an event source, read existing logs, delete event sources or logs, respond to entries, clear an event log, listen to events, and access a collection of all event logs.</summary>
	Administer = 0x30
}
