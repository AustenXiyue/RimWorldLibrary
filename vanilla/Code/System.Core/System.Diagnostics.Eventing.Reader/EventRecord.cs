using System.Collections.Generic;
using System.Security.Permissions;
using System.Security.Principal;
using Unity;

namespace System.Diagnostics.Eventing.Reader;

/// <summary>Defines the properties of an event instance for an event that is received from an <see cref="T:System.Diagnostics.Eventing.Reader.EventLogReader" /> object. The event properties provide information about the event such as the name of the computer where the event was logged and the time the event was created. This class is an abstract class. The <see cref="T:System.Diagnostics.Eventing.Reader.EventLogRecord" /> class implements this class.</summary>
/// <filterpriority>2</filterpriority>
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public abstract class EventRecord : IDisposable
{
	/// <summary>Gets the globally unique identifier (GUID) for the activity in process for which the event is involved. This allows consumers to group related activities.</summary>
	/// <returns>Returns a GUID value.</returns>
	/// <filterpriority>2</filterpriority>
	public abstract Guid? ActivityId { get; }

	/// <summary>Gets a placeholder (bookmark) that corresponds to this event. This can be used as a placeholder in a stream of events.</summary>
	/// <returns>Returns a <see cref="T:System.Diagnostics.Eventing.Reader.EventBookmark" /> object.</returns>
	/// <filterpriority>2</filterpriority>
	public abstract EventBookmark Bookmark { get; }

	/// <summary>Gets the identifier for this event. All events with this identifier value represent the same type of event.</summary>
	/// <returns>Returns an integer value. This value can be null.</returns>
	/// <filterpriority>2</filterpriority>
	public abstract int Id { get; }

	/// <summary>Gets the keyword mask of the event. Get the value of the <see cref="P:System.Diagnostics.Eventing.Reader.EventRecord.KeywordsDisplayNames" /> property to get the name of the keywords used in this mask.</summary>
	/// <returns>Returns a long value. This value can be null.</returns>
	/// <filterpriority>2</filterpriority>
	public abstract long? Keywords { get; }

	/// <summary>Gets the display names of the keywords used in the keyword mask for this event. </summary>
	/// <returns>Returns an enumerable collection of strings that contain the display names of the keywords used in the keyword mask for this event.</returns>
	/// <filterpriority>2</filterpriority>
	public abstract IEnumerable<string> KeywordsDisplayNames { get; }

	/// <summary>Gets the level of the event. The level signifies the severity of the event. For the name of the level, get the value of the <see cref="P:System.Diagnostics.Eventing.Reader.EventRecord.LevelDisplayName" /> property.</summary>
	/// <returns>Returns a byte value. This value can be null.</returns>
	/// <filterpriority>2</filterpriority>
	public abstract byte? Level { get; }

	/// <summary>Gets the display name of the level for this event.</summary>
	/// <returns>Returns a string that contains the display name of the level for this event.</returns>
	/// <filterpriority>2</filterpriority>
	public abstract string LevelDisplayName { get; }

	/// <summary>Gets the name of the event log where this event is logged.</summary>
	/// <returns>Returns a string that contains a name of the event log that contains this event.</returns>
	/// <filterpriority>2</filterpriority>
	public abstract string LogName { get; }

	/// <summary>Gets the name of the computer on which this event was logged.</summary>
	/// <returns>Returns a string that contains the name of the computer on which this event was logged.</returns>
	/// <filterpriority>2</filterpriority>
	public abstract string MachineName { get; }

	/// <summary>Gets the opcode of the event. The opcode defines a numeric value that identifies the activity or a point within an activity that the application was performing when it raised the event. For the name of the opcode, get the value of the <see cref="P:System.Diagnostics.Eventing.Reader.EventRecord.OpcodeDisplayName" /> property.</summary>
	/// <returns>Returns a short value. This value can be null.</returns>
	/// <filterpriority>2</filterpriority>
	public abstract short? Opcode { get; }

	/// <summary>Gets the display name of the opcode for this event.</summary>
	/// <returns>Returns a string that contains the display name of the opcode for this event.</returns>
	/// <filterpriority>2</filterpriority>
	public abstract string OpcodeDisplayName { get; }

	/// <summary>Gets the process identifier for the event provider that logged this event.</summary>
	/// <returns>Returns an integer value. This value can be null.</returns>
	/// <filterpriority>2</filterpriority>
	public abstract int? ProcessId { get; }

	/// <summary>Gets the user-supplied properties of the event.</summary>
	/// <returns>Returns a list of <see cref="T:System.Diagnostics.Eventing.Reader.EventProperty" /> objects.</returns>
	/// <filterpriority>2</filterpriority>
	public abstract IList<EventProperty> Properties { get; }

	/// <summary>Gets the globally unique identifier (GUID) of the event provider that published this event.</summary>
	/// <returns>Returns a GUID value. This value can be null.</returns>
	/// <filterpriority>2</filterpriority>
	public abstract Guid? ProviderId { get; }

	/// <summary>Gets the name of the event provider that published this event.</summary>
	/// <returns>Returns a string that contains the name of the event provider that published this event.</returns>
	/// <filterpriority>2</filterpriority>
	public abstract string ProviderName { get; }

	/// <summary>Gets qualifier numbers that are used for event identification.</summary>
	/// <returns>Returns an integer value. This value can be null.</returns>
	/// <filterpriority>2</filterpriority>
	public abstract int? Qualifiers { get; }

	/// <summary>Gets the event record identifier of the event in the log.</summary>
	/// <returns>Returns a long value. This value can be null.</returns>
	/// <filterpriority>2</filterpriority>
	public abstract long? RecordId { get; }

	/// <summary>Gets a globally unique identifier (GUID) for a related activity in a process for which an event is involved.</summary>
	/// <returns>Returns a GUID value. This value can be null.</returns>
	/// <filterpriority>2</filterpriority>
	public abstract Guid? RelatedActivityId { get; }

	/// <summary>Gets a task identifier for a portion of an application or a component that publishes an event. A task is a 16-bit value with 16 top values reserved. This type allows any value between 0x0000 and 0xffef to be used. To obtain the task name, get the value of the <see cref="P:System.Diagnostics.Eventing.Reader.EventRecord.TaskDisplayName" /> property.</summary>
	/// <returns>Returns an integer value. This value can be null.</returns>
	/// <filterpriority>2</filterpriority>
	public abstract int? Task { get; }

	/// <summary>Gets the display name of the task for the event.</summary>
	/// <returns>Returns a string that contains the display name of the task for the event.</returns>
	/// <filterpriority>2</filterpriority>
	public abstract string TaskDisplayName { get; }

	/// <summary>Gets the thread identifier for the thread that the event provider is running in.</summary>
	/// <returns>Returns an integer value. This value can be null.</returns>
	/// <filterpriority>2</filterpriority>
	public abstract int? ThreadId { get; }

	/// <summary>Gets the time, in <see cref="T:System.DateTime" /> format, that the event was created.</summary>
	/// <returns>Returns a <see cref="T:System.DateTime" /> value. The value can be null.</returns>
	/// <filterpriority>2</filterpriority>
	public abstract DateTime? TimeCreated { get; }

	/// <summary>Gets the security descriptor of the user whose context is used to publish the event.</summary>
	/// <returns>Returns a <see cref="T:System.Security.Principal.SecurityIdentifier" /> value.</returns>
	/// <filterpriority>2</filterpriority>
	public abstract SecurityIdentifier UserId { get; }

	/// <summary>Gets the version number for the event.</summary>
	/// <returns>Returns a byte value. This value can be null.</returns>
	/// <filterpriority>2</filterpriority>
	public abstract byte? Version { get; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.Eventing.Reader.EventRecord" /> class.</summary>
	protected EventRecord()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Releases all the resources used by this object.</summary>
	/// <filterpriority>2</filterpriority>
	public void Dispose()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Releases the unmanaged resources used by this object, and optionally releases the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
	protected virtual void Dispose(bool disposing)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Gets the event message in the current locale.</summary>
	/// <returns>Returns a string that contains the event message in the current locale.</returns>
	/// <filterpriority>2</filterpriority>
	public abstract string FormatDescription();

	/// <summary>Gets the event message, replacing variables in the message with the specified values.</summary>
	/// <returns>Returns a string that contains the event message in the current locale.</returns>
	/// <param name="values">The values used to replace variables in the event message. Variables are represented by %n, where n is a number.</param>
	/// <filterpriority>2</filterpriority>
	public abstract string FormatDescription(IEnumerable<object> values);

	/// <summary>Gets the XML representation of the event. All of the event properties are represented in the event XML. The XML conforms to the event schema.</summary>
	/// <returns>Returns a string that contains the XML representation of the event.</returns>
	/// <filterpriority>2</filterpriority>
	public abstract string ToXml();
}
