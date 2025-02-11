namespace System.Diagnostics.Eventing.Reader;

/// <summary>Defines the default access permissions for the event log. The Application and System values indicate that the log shares the access control list (ACL) with the appropriate Windows log (the Application or System event logs) and share the Event Tracing for Windows (ETW) session with other logs of the same isolation. All channels with Custom isolation use a private ETW session.</summary>
/// <filterpriority>2</filterpriority>
public enum EventLogIsolation
{
	/// <summary>The log shares the access control list with the Application event log and shares the ETW session with other logs that have Application isolation.</summary>
	Application = 0,
	/// <summary>The event log is a custom event log that uses its own private ETW session.</summary>
	Custom = 2,
	/// <summary>The log shares the access control list with the System event log and shares the ETW session with other logs that have System isolation.</summary>
	System = 1
}
