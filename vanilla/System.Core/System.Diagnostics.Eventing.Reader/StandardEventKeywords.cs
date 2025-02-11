namespace System.Diagnostics.Eventing.Reader;

/// <summary>Defines the standard keywords that are attached to events by the event provider. For more information about keywords, see <see cref="T:System.Diagnostics.Eventing.Reader.EventKeyword" />.</summary>
/// <filterpriority>2</filterpriority>
[Flags]
public enum StandardEventKeywords : long
{
	/// <summary>Attached to all failed security audit events. This keyword should only be used for events in the Security log.</summary>
	AuditFailure = 0x10000000000000L,
	/// <summary>Attached to all successful security audit events. This keyword should only be used for events in the Security log.</summary>
	AuditSuccess = 0x20000000000000L,
	/// <summary>Attached to transfer events where the related Activity ID (Correlation ID) is a computed value and is not guaranteed to be unique (not a real GUID).</summary>
	[Obsolete("Incorrect value: use CorrelationHint2 instead", false)]
	CorrelationHint = 0x10000000000000L,
	/// <summary>Attached to transfer events where the related Activity ID (Correlation ID) is a computed value and is not guaranteed to be unique (not a real GUID).</summary>
	CorrelationHint2 = 0x40000000000000L,
	/// <summary>Attached to events which are raised using the RaiseEvent function.</summary>
	EventLogClassic = 0x80000000000000L,
	/// <summary>This value indicates that no filtering on keyword is performed when the event is published.</summary>
	None = 0L,
	/// <summary>Attached to all response time events. </summary>
	ResponseTime = 0x1000000000000L,
	/// <summary>Attached to all Service Quality Mechanism (SQM) events.</summary>
	Sqm = 0x8000000000000L,
	/// <summary>Attached to all Windows Diagnostic Infrastructure (WDI) context events.</summary>
	WdiContext = 0x2000000000000L,
	/// <summary>Attached to all Windows Diagnostic Infrastructure (WDI) diagnostic events.</summary>
	WdiDiagnostic = 0x4000000000000L
}
