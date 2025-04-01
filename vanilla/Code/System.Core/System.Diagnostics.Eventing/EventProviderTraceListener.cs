using System.Security.Permissions;
using Unity;

namespace System.Diagnostics.Eventing;

/// <summary>A listener for <see cref="T:System.Diagnostics.TraceSource" /> that writes events to the ETW subsytem. </summary>
/// <filterpriority>2</filterpriority>
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public class EventProviderTraceListener : TraceListener
{
	/// <summary>Gets and sets the delimiter used to delimit the event data that is written to the ETW subsystem.</summary>
	/// <returns>The delimiter used to delimit the event data. The default delimiter is a comma.</returns>
	/// <filterpriority>2</filterpriority>
	public string Delimiter
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
		set
		{
			Unity.ThrowStub.ThrowNotSupportedException();
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.Eventing.EventProviderTraceListener" /> class using the specified provider identifier.</summary>
	/// <param name="providerId">A unique string <see cref="T:System.Guid" /> that identifies the provider.</param>
	/// <filterpriority>2</filterpriority>
	public EventProviderTraceListener(string providerId)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.Eventing.EventProviderTraceListener" /> class using the specified provider identifier and name of the listener.</summary>
	/// <param name="providerId">A unique string <see cref="T:System.Guid" /> that identifies the provider.</param>
	/// <param name="name">Name of the listener.</param>
	/// <filterpriority>2</filterpriority>
	public EventProviderTraceListener(string providerId, string name)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.Eventing.EventProviderTraceListener" /> class using the specified provider identifier, name of the listener, and delimiter.</summary>
	/// <param name="providerId">A unique string <see cref="T:System.Guid" /> that identifies the provider.</param>
	/// <param name="name">Name of the listener.</param>
	/// <param name="delimiter">Delimiter used to delimit the event data. (For more details, see the <see cref="P:System.Diagnostics.Eventing.EventProviderTraceListener.Delimiter" /> property.)</param>
	/// <filterpriority>2</filterpriority>
	public EventProviderTraceListener(string providerId, string name, string delimiter)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <filterpriority>2</filterpriority>
	public sealed override void Write(string message)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <filterpriority>2</filterpriority>
	public sealed override void WriteLine(string message)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
