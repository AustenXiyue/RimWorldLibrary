using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.ComponentModel;

/// <summary>Thrown when a thread on which an operation should execute no longer exists or has no message loop. </summary>
[Serializable]
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
public class InvalidAsynchronousStateException : ArgumentException
{
	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.InvalidAsynchronousStateException" /> class. </summary>
	public InvalidAsynchronousStateException()
		: this(null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.InvalidAsynchronousStateException" /> class with the specified detailed description.</summary>
	/// <param name="message">A detailed description of the error.</param>
	public InvalidAsynchronousStateException(string message)
		: base(message)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.InvalidAsynchronousStateException" /> class with the specified detailed description and the specified exception. </summary>
	/// <param name="message">A detailed description of the error.</param>
	/// <param name="innerException">A reference to the inner exception that is the cause of this exception.</param>
	public InvalidAsynchronousStateException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.InvalidAsynchronousStateException" /> class with the given <see cref="T:System.Runtime.Serialization.SerializationInfo" /> and <see cref="T:System.Runtime.Serialization.StreamingContext" />. </summary>
	/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> to be used for deserialization.</param>
	/// <param name="context">The destination to be used for deserialization.</param>
	protected InvalidAsynchronousStateException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
