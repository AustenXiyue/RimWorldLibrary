using System.Security.Permissions;
using Unity;

namespace System.Diagnostics.Eventing.Reader;

/// <summary>Represents a link between an event provider and an event log that the provider publishes events into. This object cannot be instantiated.</summary>
/// <filterpriority>2</filterpriority>
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public sealed class EventLogLink
{
	/// <summary>Gets the localized name of the event log.</summary>
	/// <returns>Returns a string that contains the localized name of the event log.</returns>
	/// <filterpriority>2</filterpriority>
	public string DisplayName
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Gets a Boolean value that determines whether the event log is imported, rather than defined in the event provider. An imported event log is defined in a different provider.</summary>
	/// <returns>Returns true if the event log is imported by the event provider, and returns false if the event log is not imported by the event provider.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsImported
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(bool);
		}
	}

	/// <summary>Gets the non-localized name of the event log associated with this object.</summary>
	/// <returns>Returns a string that contains the non-localized name of the event log associated with this object.</returns>
	/// <filterpriority>2</filterpriority>
	public string LogName
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	internal EventLogLink()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
