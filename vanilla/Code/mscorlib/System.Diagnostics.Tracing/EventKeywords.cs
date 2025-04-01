namespace System.Diagnostics.Tracing;

/// <summary>Defines the standard keywords that apply to events.</summary>
[Flags]
public enum EventKeywords : long
{
	/// <summary>No filtering on keywords is performed when the event is published.</summary>
	None = 0L,
	All = -1L,
	MicrosoftTelemetry = 0x2000000000000L,
	/// <summary>Attached to all Windows Diagnostics Infrastructure (WDI) context events.</summary>
	WdiContext = 0x2000000000000L,
	/// <summary>Attached to all Windows Diagnostics Infrastructure (WDI) diagnostic events.</summary>
	WdiDiagnostic = 0x4000000000000L,
	/// <summary>Attached to all Service Quality Mechanism (SQM) events.</summary>
	Sqm = 0x8000000000000L,
	/// <summary>Attached to all failed security audit events. Use this keyword only  for events in the security log.</summary>
	AuditFailure = 0x10000000000000L,
	/// <summary>Attached to all successful security audit events. Use this keyword only for events in the security log.</summary>
	AuditSuccess = 0x20000000000000L,
	/// <summary>Attached to transfer events where the related activity ID (correlation ID) is a computed value and is not guaranteed to be unique (that is, it is not a real GUID).</summary>
	CorrelationHint = 0x10000000000000L,
	/// <summary>Attached to events that are raised by using the RaiseEvent function.</summary>
	EventLogClassic = 0x80000000000000L
}
