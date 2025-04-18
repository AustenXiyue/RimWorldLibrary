using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System;

/// <summary>The exception that is thrown when the operating system denies access because of an I/O error or a specific type of security error.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
[ComVisible(true)]
public class UnauthorizedAccessException : SystemException
{
	/// <summary>Initializes a new instance of the <see cref="T:System.UnauthorizedAccessException" /> class.</summary>
	public UnauthorizedAccessException()
		: base(Environment.GetResourceString("Attempted to perform an unauthorized operation."))
	{
		SetErrorCode(-2147024891);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.UnauthorizedAccessException" /> class with a specified error message.</summary>
	/// <param name="message">The message that describes the error. </param>
	public UnauthorizedAccessException(string message)
		: base(message)
	{
		SetErrorCode(-2147024891);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.UnauthorizedAccessException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="message">The error message that explains the reason for the exception. </param>
	/// <param name="inner">The exception that is the cause of the current exception. If the <paramref name="inner" /> parameter is not a null reference (Nothing in Visual Basic), the current exception is raised in a catch block that handles the inner exception. </param>
	public UnauthorizedAccessException(string message, Exception inner)
		: base(message, inner)
	{
		SetErrorCode(-2147024891);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.UnauthorizedAccessException" /> class with serialized data.</summary>
	/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown. </param>
	/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination. </param>
	protected UnauthorizedAccessException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
