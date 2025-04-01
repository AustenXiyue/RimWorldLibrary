using System.Runtime.Serialization;
using System.Security.Permissions;
using Unity;

namespace System.Diagnostics.Eventing.Reader;

/// <summary>Represents an exception that is thrown when an error occurred while reading, querying, or subscribing to the events in an event log. </summary>
/// <filterpriority>2</filterpriority>
[Serializable]
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public class EventLogReadingException : EventLogException
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.Eventing.Reader.EventLogReadingException" /> class.</summary>
	/// <filterpriority>2</filterpriority>
	public EventLogReadingException()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.Eventing.Reader.EventLogReadingException" /> class with serialized data.</summary>
	/// <param name="serializationInfo">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object that holds the serialized object data about the exception thrown.</param>
	/// <param name="streamingContext">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> object that contains contextual information about the source or destination.</param>
	protected EventLogReadingException(SerializationInfo serializationInfo, StreamingContext streamingContext)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.Eventing.Reader.EventLogReadingException" /> class by specifying the error message that describes the current exception.</summary>
	/// <param name="message">The error message that describes the current exception.</param>
	/// <filterpriority>2</filterpriority>
	public EventLogReadingException(string message)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.Eventing.Reader.EventLogReadingException" /> class with an error message and inner exception.</summary>
	/// <param name="message">The error message that describes the current exception.</param>
	/// <param name="innerException">The Exception instance that caused the current exception.</param>
	/// <filterpriority>2</filterpriority>
	public EventLogReadingException(string message, Exception innerException)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
