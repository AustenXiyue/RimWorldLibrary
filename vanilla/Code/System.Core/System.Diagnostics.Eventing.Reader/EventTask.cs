using System.Security.Permissions;
using Unity;

namespace System.Diagnostics.Eventing.Reader;

/// <summary>Contains an event task that is defined in an event provider. The task identifies a portion of an application or a component that publishes an event. A task is a 16-bit value with 16 top values reserved.</summary>
/// <filterpriority>2</filterpriority>
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public sealed class EventTask
{
	/// <summary>Gets the localized name for the event task.</summary>
	/// <returns>Returns a string that contains the localized name for the event task.</returns>
	/// <filterpriority>2</filterpriority>
	public string DisplayName
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Gets the event globally unique identifier (GUID) associated with the task. </summary>
	/// <returns>Returns a GUID value.</returns>
	/// <filterpriority>2</filterpriority>
	public Guid EventGuid
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(Guid);
		}
	}

	/// <summary>Gets the non-localized name of the event task.</summary>
	/// <returns>Returns a string that contains the non-localized name of the event task.</returns>
	/// <filterpriority>2</filterpriority>
	public string Name
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Gets the numeric value associated with the task.</summary>
	/// <returns>Returns an integer value.</returns>
	/// <filterpriority>2</filterpriority>
	public int Value
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(int);
		}
	}

	internal EventTask()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
