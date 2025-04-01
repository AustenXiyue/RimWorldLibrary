using System.Runtime.InteropServices;
using System.Security.Permissions;
using Unity;

namespace System.Diagnostics.Eventing;

/// <summary>Contains the metadata that defines an event.</summary>
/// <filterpriority>2</filterpriority>
[StructLayout(LayoutKind.Explicit, Size = 16)]
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public struct EventDescriptor
{
	/// <summary>Retrieves the channel value from the event descriptor.</summary>
	/// <returns>The channel that defines a potential target for the event.</returns>
	/// <filterpriority>2</filterpriority>
	public byte Channel
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(byte);
		}
	}

	/// <summary>Retrieves the event identifier value from the event descriptor.</summary>
	/// <returns>The event identifier.</returns>
	/// <filterpriority>2</filterpriority>
	public int EventId
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(int);
		}
	}

	/// <summary>Retrieves the keyword value from the event descriptor.</summary>
	/// <returns>The keyword, which is a bit mask, that specifies the event category.</returns>
	/// <filterpriority>2</filterpriority>
	public long Keywords
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(long);
		}
	}

	/// <summary>Retrieves the level value from the event descriptor.</summary>
	/// <returns>The level of detail included in the event.</returns>
	/// <filterpriority>2</filterpriority>
	public byte Level
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(byte);
		}
	}

	/// <summary>Retrieves the operation code value from the event descriptor.</summary>
	/// <returns>The operation being performed at the time the event is written.</returns>
	/// <filterpriority>2</filterpriority>
	public byte Opcode
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(byte);
		}
	}

	/// <summary>Retrieves the task value from the event descriptor.</summary>
	/// <returns>The task that identifies the logical component of the application that is writing the event.</returns>
	/// <filterpriority>2</filterpriority>
	public int Task
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(int);
		}
	}

	/// <summary>Retrieves the version value from the event descriptor.</summary>
	/// <returns>The version of the event. </returns>
	/// <filterpriority>2</filterpriority>
	public byte Version
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(byte);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.Eventing.EventDescriptor" /> class.</summary>
	/// <param name="id">The event identifier.</param>
	/// <param name="version">Version of the event. The version indicates a revision to the event definition. You can use this member and the Id member to identify a unique event.</param>
	/// <param name="channel">Defines a potential target for the event.</param>
	/// <param name="level">Specifies the level of detail included in the event.</param>
	/// <param name="opcode">Operation being performed at the time the event is written.</param>
	/// <param name="task">Identifies a logical component of the application that is writing the event.</param>
	/// <param name="keywords">Bit mask that specifies the event category. The keyword can contain one or more provider-defined keywords, standard keywords, or both.</param>
	/// <filterpriority>2</filterpriority>
	public EventDescriptor(int id, byte version, byte channel, byte level, byte opcode, int task, long keywords)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
