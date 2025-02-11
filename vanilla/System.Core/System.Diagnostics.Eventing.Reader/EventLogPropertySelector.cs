using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using Unity;

namespace System.Diagnostics.Eventing.Reader;

/// <summary>Contains an array of strings that represent XPath queries for elements in the XML representation of an event, which is based on the Event Schema. The queries in this object are used to extract values from the event.</summary>
/// <filterpriority>2</filterpriority>
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public class EventLogPropertySelector : IDisposable
{
	/// <summary>Initializes a new <see cref="T:System.Diagnostics.Eventing.Reader.EventLogPropertySelector" /> class instance.</summary>
	/// <param name="propertyQueries">XPath queries used to extract values from the XML representation of the event.</param>
	/// <filterpriority>2</filterpriority>
	[SecurityCritical]
	public EventLogPropertySelector(IEnumerable<string> propertyQueries)
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
	[SecuritySafeCritical]
	protected virtual void Dispose(bool disposing)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
