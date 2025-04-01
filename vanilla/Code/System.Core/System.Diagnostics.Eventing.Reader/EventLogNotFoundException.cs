using System.Runtime.Serialization;
using System.Security.Permissions;
using Unity;

namespace System.Diagnostics.Eventing.Reader;

/// <summary>Represents the exception that is thrown when a requested event log (usually specified by the name of the event log or the path to the event log file) does not exist.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public class EventLogNotFoundException : EventLogException
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.Eventing.Reader.EventLogNotFoundException" /> class.</summary>
	/// <filterpriority>2</filterpriority>
	public EventLogNotFoundException()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.Eventing.Reader.EventLogNotFoundException" /> class with serialized data.</summary>
	/// <param name="serializationInfo">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object that holds the serialized object data about the exception thrown.</param>
	/// <param name="streamingContext">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> object that contains contextual information about the source or destination.</param>
	protected EventLogNotFoundException(SerializationInfo serializationInfo, StreamingContext streamingContext)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.Eventing.Reader.EventLogNotFoundException" /> class by specifying the error message that describes the current exception.</summary>
	/// <param name="message">The error message that describes the current exception.</param>
	/// <filterpriority>2</filterpriority>
	public EventLogNotFoundException(string message)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.Eventing.Reader.EventLogNotFoundException" /> class with an error message and inner exception.</summary>
	/// <param name="message">The error message that describes the current exception.</param>
	/// <param name="innerException">The Exception instance that caused the current exception.</param>
	/// <filterpriority>2</filterpriority>
	public EventLogNotFoundException(string message, Exception innerException)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
