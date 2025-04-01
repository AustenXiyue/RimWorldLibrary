using System.Collections.Generic;
using System.Security.Permissions;
using Unity;

namespace System.Diagnostics.Eventing.Reader;

/// <summary>Contains the metadata (properties and settings) for an event that is defined in an event provider. </summary>
/// <filterpriority>2</filterpriority>
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public sealed class EventMetadata
{
	/// <summary>Gets the description template associated with the event using the current thread locale for the description language.</summary>
	/// <returns>Returns a string that contains the description template associated with the event.</returns>
	/// <filterpriority>2</filterpriority>
	public string Description
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Gets the identifier of the event that is defined in the event provider.</summary>
	/// <returns>Returns a long value that is the event identifier.</returns>
	/// <filterpriority>2</filterpriority>
	public long Id
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(long);
		}
	}

	/// <summary>Gets the keywords associated with the event that is defined in the even provider.</summary>
	/// <returns>Returns an enumerable collection of <see cref="T:System.Diagnostics.Eventing.Reader.EventKeyword" /> objects.</returns>
	/// <filterpriority>2</filterpriority>
	public IEnumerable<EventKeyword> Keywords
	{
		get
		{
			//IL_0007: Expected O, but got I4
			Unity.ThrowStub.ThrowNotSupportedException();
			return (IEnumerable<EventKeyword>)0;
		}
	}

	/// <summary>Gets the level associated with the event that is defined in the event provider. The level defines the severity of the event.</summary>
	/// <returns>Returns an <see cref="T:System.Diagnostics.Eventing.Reader.EventLevel" /> object.</returns>
	/// <filterpriority>2</filterpriority>
	public EventLevel Level
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Gets a link to the event log that receives this event when the provider publishes this event.</summary>
	/// <returns>Returns a <see cref="T:System.Diagnostics.Eventing.Reader.EventLogLink" /> object.</returns>
	/// <filterpriority>2</filterpriority>
	public EventLogLink LogLink
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Gets the opcode associated with this event that is defined by an event provider. The opcode defines a numeric value that identifies the activity or a point within an activity that the application was performing when it raised the event.</summary>
	/// <returns>Returns a <see cref="T:System.Diagnostics.Eventing.Reader.EventOpcode" /> object.</returns>
	/// <filterpriority>2</filterpriority>
	public EventOpcode Opcode
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Gets the task associated with the event. A task identifies a portion of an application or a component that publishes an event. </summary>
	/// <returns>Returns a <see cref="T:System.Diagnostics.Eventing.Reader.EventTask" /> object.</returns>
	/// <filterpriority>2</filterpriority>
	public EventTask Task
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Gets the template string for the event. Templates are used to describe data that is used by a provider when an event is published. Templates optionally specify XML that provides the structure of an event. The XML allows values that the event publisher provides to be inserted during the rendering of an event.</summary>
	/// <returns>Returns a string that contains the template for the event.</returns>
	/// <filterpriority>2</filterpriority>
	public string Template
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Gets the version of the event that qualifies the event identifier.</summary>
	/// <returns>Returns a byte value.</returns>
	/// <filterpriority>2</filterpriority>
	public byte Version
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(byte);
		}
	}

	internal EventMetadata()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
