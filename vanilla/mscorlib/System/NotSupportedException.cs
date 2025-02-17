using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System;

/// <summary>The exception that is thrown when an invoked method is not supported, or when there is an attempt to read, seek, or write to a stream that does not support the invoked functionality.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
[ComVisible(true)]
public class NotSupportedException : SystemException
{
	/// <summary>Initializes a new instance of the <see cref="T:System.NotSupportedException" /> class, setting the <see cref="P:System.Exception.Message" /> property of the new instance to a system-supplied message that describes the error. This message takes into account the current system culture.</summary>
	public NotSupportedException()
		: base(Environment.GetResourceString("Specified method is not supported."))
	{
		SetErrorCode(-2146233067);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.NotSupportedException" /> class with a specified error message.</summary>
	/// <param name="message">A <see cref="T:System.String" /> that describes the error. The content of <paramref name="message" /> is intended to be understood by humans. The caller of this constructor is required to ensure that this string has been localized for the current system culture. </param>
	public NotSupportedException(string message)
		: base(message)
	{
		SetErrorCode(-2146233067);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.NotSupportedException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="message">The error message that explains the reason for the exception. </param>
	/// <param name="innerException">The exception that is the cause of the current exception. If the <paramref name="innerException" /> parameter is not a null reference, the current exception is raised in a catch block that handles the inner exception. </param>
	public NotSupportedException(string message, Exception innerException)
		: base(message, innerException)
	{
		SetErrorCode(-2146233067);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.NotSupportedException" /> class with serialized data.</summary>
	/// <param name="info">The object that holds the serialized object data. </param>
	/// <param name="context">The contextual information about the source or destination. </param>
	protected NotSupportedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
