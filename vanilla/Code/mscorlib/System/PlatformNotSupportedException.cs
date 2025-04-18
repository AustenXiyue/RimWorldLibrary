using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System;

/// <summary>The exception that is thrown when a feature does not run on a particular platform.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
[ComVisible(true)]
public class PlatformNotSupportedException : NotSupportedException
{
	/// <summary>Initializes a new instance of the <see cref="T:System.PlatformNotSupportedException" /> class with default properties.</summary>
	public PlatformNotSupportedException()
		: base(Environment.GetResourceString("Operation is not supported on this platform."))
	{
		SetErrorCode(-2146233031);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.PlatformNotSupportedException" /> class with a specified error message.</summary>
	/// <param name="message">The text message that explains the reason for the exception. </param>
	public PlatformNotSupportedException(string message)
		: base(message)
	{
		SetErrorCode(-2146233031);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.PlatformNotSupportedException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="message">The error message that explains the reason for the exception. </param>
	/// <param name="inner">The exception that is the cause of the current exception. If the <paramref name="inner" /> parameter is not null, the current exception is raised in a catch block that handles the inner exception. </param>
	public PlatformNotSupportedException(string message, Exception inner)
		: base(message, inner)
	{
		SetErrorCode(-2146233031);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.PlatformNotSupportedException" /> class with serialized data.</summary>
	/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown. </param>
	/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination. </param>
	protected PlatformNotSupportedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
