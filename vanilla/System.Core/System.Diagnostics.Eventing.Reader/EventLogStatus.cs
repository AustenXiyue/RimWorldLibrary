using System.Security.Permissions;
using Unity;

namespace System.Diagnostics.Eventing.Reader;

/// <summary>Contains the status code or error code for a specific event log. This status can be used to determine if the event log is available for an operation.</summary>
/// <filterpriority>2</filterpriority>
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public sealed class EventLogStatus
{
	/// <summary>Gets the name of the event log for which the status code is obtained.</summary>
	/// <returns>Returns a string that contains the name of the event log for which the status code is obtained.</returns>
	/// <filterpriority>2</filterpriority>
	public string LogName
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Gets the status code or error code for the event log. This status or error is the result of a read or subscription operation on the event log.</summary>
	/// <returns>Returns an integer value.</returns>
	/// <filterpriority>2</filterpriority>
	public int StatusCode
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(int);
		}
	}

	internal EventLogStatus()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
