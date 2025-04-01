namespace System.Diagnostics.Tracing;

/// <summary>Specifies additional event schema information for an event.</summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class EventAttribute : Attribute
{
	private EventOpcode m_opcode;

	private bool m_opcodeSet;

	/// <summary>Gets or sets the identifier for the event.</summary>
	/// <returns>The event identifier.</returns>
	public int EventId { get; private set; }

	/// <summary>Gets or sets the level for the event.</summary>
	/// <returns>One of the enumeration values that specifies the level for the event.</returns>
	public EventLevel Level { get; set; }

	/// <summary>Gets or sets the keywords for the event.</summary>
	/// <returns>A bitwise combination of the enumeration values.</returns>
	public EventKeywords Keywords { get; set; }

	/// <summary>Gets or sets the operation code for the event.</summary>
	/// <returns>One of the enumeration values that specifies the operation code.</returns>
	public EventOpcode Opcode
	{
		get
		{
			return m_opcode;
		}
		set
		{
			m_opcode = value;
			m_opcodeSet = true;
		}
	}

	internal bool IsOpcodeSet => m_opcodeSet;

	/// <summary>Gets or sets the task for the event.</summary>
	/// <returns>The task for the event.</returns>
	public EventTask Task { get; set; }

	public EventChannel Channel { get; set; }

	/// <summary>Gets or sets the version of the event.</summary>
	/// <returns>The version of the event.</returns>
	public byte Version { get; set; }

	/// <summary>Gets or sets the message for the event.</summary>
	/// <returns>The message for the event.</returns>
	public string Message { get; set; }

	public EventTags Tags { get; set; }

	public EventActivityOptions ActivityOptions { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.Tracing.EventAttribute" /> class with the specified event identifier.</summary>
	/// <param name="eventId">The event identifier for the event.</param>
	public EventAttribute(int eventId)
	{
		EventId = eventId;
		Level = EventLevel.Informational;
		m_opcodeSet = false;
	}
}
