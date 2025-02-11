using System.Security.Permissions;
using Unity;

namespace System.Diagnostics.Eventing.Reader;

/// <summary>Contains an event level that is defined in an event provider. The level signifies the severity of the event.</summary>
/// <filterpriority>2</filterpriority>
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public sealed class EventLevel
{
	/// <summary>Gets the localized name for the event level. The name describes what severity level of events this level is used for.</summary>
	/// <returns>Returns a string that contains the localized name for the event level.</returns>
	/// <filterpriority>2</filterpriority>
	public string DisplayName
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Gets the non-localized name of the event level.</summary>
	/// <returns>Returns a string that contains the non-localized name of the event level.</returns>
	/// <filterpriority>2</filterpriority>
	public string Name
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Gets the numeric value of the event level.</summary>
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

	internal EventLevel()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
