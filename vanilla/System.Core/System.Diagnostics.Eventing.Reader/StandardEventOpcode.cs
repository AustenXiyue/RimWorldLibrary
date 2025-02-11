namespace System.Diagnostics.Eventing.Reader;

/// <summary>Defines the standard opcodes that are attached to events by the event provider. For more information about opcodes, see <see cref="T:System.Diagnostics.Eventing.Reader.EventOpcode" />.</summary>
/// <filterpriority>2</filterpriority>
public enum StandardEventOpcode
{
	/// <summary>An event with this opcode is a trace collection start event.</summary>
	DataCollectionStart = 3,
	/// <summary>An event with this opcode is a trace collection stop event.</summary>
	DataCollectionStop = 4,
	/// <summary>An event with this opcode is an extension event.</summary>
	Extension = 5,
	/// <summary>An event with this opcode is an informational event.</summary>
	Info = 0,
	/// <summary>An event with this opcode is published when one activity in an application receives data.</summary>
	Receive = 240,
	/// <summary>An event with this opcode is published after an activity in an application replies to an event.</summary>
	Reply = 6,
	/// <summary>An event with this opcode is published after an activity in an application resumes from a suspended state. The event should follow an event with the Suspend opcode.</summary>
	Resume = 7,
	/// <summary>An event with this opcode is published when one activity in an application transfers data or system resources to another activity. </summary>
	Send = 9,
	/// <summary>An event with this opcode is published when an application starts a new transaction or activity. This can be embedded into another transaction or activity when multiple events with the Start opcode follow each other without an event with a Stop opcode.</summary>
	Start = 1,
	/// <summary>An event with this opcode is published when an activity or a transaction in an application ends. The event corresponds to the last unpaired event with a Start opcode.</summary>
	Stop = 2,
	/// <summary>An event with this opcode is published when an activity in an application is suspended. </summary>
	Suspend = 8
}
