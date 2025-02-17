using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System;

/// <summary>The exception that is thrown when a method call is invalid for the object's current state.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
[ComVisible(true)]
public class InvalidOperationException : SystemException
{
	/// <summary>Initializes a new instance of the <see cref="T:System.InvalidOperationException" /> class.</summary>
	public InvalidOperationException()
		: base(Environment.GetResourceString("Operation is not valid due to the current state of the object."))
	{
		SetErrorCode(-2146233079);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.InvalidOperationException" /> class with a specified error message.</summary>
	/// <param name="message">The message that describes the error. </param>
	public InvalidOperationException(string message)
		: base(message)
	{
		SetErrorCode(-2146233079);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.InvalidOperationException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="message">The error message that explains the reason for the exception. </param>
	/// <param name="innerException">The exception that is the cause of the current exception. If the <paramref name="innerException" /> parameter is not a null reference (Nothing in Visual Basic), the current exception is raised in a catch block that handles the inner exception. </param>
	public InvalidOperationException(string message, Exception innerException)
		: base(message, innerException)
	{
		SetErrorCode(-2146233079);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.InvalidOperationException" /> class with serialized data.</summary>
	/// <param name="info">The object that holds the serialized object data. </param>
	/// <param name="context">The contextual information about the source or destination. </param>
	protected InvalidOperationException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
