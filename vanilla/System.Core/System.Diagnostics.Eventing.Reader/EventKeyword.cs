using System.Security.Permissions;
using Unity;

namespace System.Diagnostics.Eventing.Reader;

/// <summary>Represents a keyword for an event. Keywords are defined in an event provider and are used to group the event with other similar events (based on the usage of the events).</summary>
/// <filterpriority>2</filterpriority>
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public sealed class EventKeyword
{
	/// <summary>Gets the localized name of the keyword.</summary>
	/// <returns>Returns a string that contains a localized name for this keyword.</returns>
	/// <filterpriority>2</filterpriority>
	public string DisplayName
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Gets the non-localized name of the keyword.</summary>
	/// <returns>Returns a string that contains the non-localized name of this keyword.</returns>
	/// <filterpriority>2</filterpriority>
	public string Name
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Gets the numeric value associated with the keyword.</summary>
	/// <returns>Returns a long value.</returns>
	/// <filterpriority>2</filterpriority>
	public long Value
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(long);
		}
	}

	internal EventKeyword()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
