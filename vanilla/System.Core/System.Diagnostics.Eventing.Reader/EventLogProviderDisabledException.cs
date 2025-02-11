using System.Runtime.Serialization;
using System.Security.Permissions;
using Unity;

namespace System.Diagnostics.Eventing.Reader;

/// <summary>Represents the exception that is thrown when a specified event provider name references a disabled event provider. A disabled event provider cannot publish events.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public class EventLogProviderDisabledException : EventLogException
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.Eventing.Reader.EventLogProviderDisabledException" /> class.</summary>
	/// <filterpriority>2</filterpriority>
	public EventLogProviderDisabledException()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.Eventing.Reader.EventLogProviderDisabledException" /> class with serialized data.</summary>
	/// <param name="serializationInfo">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object that holds the serialized object data about the exception thrown.</param>
	/// <param name="streamingContext">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> object that contains contextual information about the source or destination.</param>
	protected EventLogProviderDisabledException(SerializationInfo serializationInfo, StreamingContext streamingContext)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.Eventing.Reader.EventLogProviderDisabledException" /> class by specifying the error message that describes the current exception.</summary>
	/// <param name="message">The error message that describes the current exception.</param>
	/// <filterpriority>2</filterpriority>
	public EventLogProviderDisabledException(string message)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.Eventing.Reader.EventLogProviderDisabledException" /> class with an error message and inner exception.</summary>
	/// <param name="message">The error message that describes the current exception.</param>
	/// <param name="innerException">The Exception instance that caused the current exception.</param>
	/// <filterpriority>2</filterpriority>
	public EventLogProviderDisabledException(string message, Exception innerException)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
