using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System;

/// <summary>The exception that is thrown when there is an invalid attempt to access a private or protected field inside a class.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
[ComVisible(true)]
public class FieldAccessException : MemberAccessException
{
	/// <summary>Initializes a new instance of the <see cref="T:System.FieldAccessException" /> class.</summary>
	public FieldAccessException()
		: base(Environment.GetResourceString("Attempted to access a field that is not accessible by the caller."))
	{
		SetErrorCode(-2146233081);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.FieldAccessException" /> class with a specified error message.</summary>
	/// <param name="message">The error message that explains the reason for the exception.</param>
	public FieldAccessException(string message)
		: base(message)
	{
		SetErrorCode(-2146233081);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.FieldAccessException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="message">The error message that explains the reason for the exception.</param>
	/// <param name="inner">The exception that is the cause of the current exception. If the <paramref name="inner" /> parameter is not null, the current exception is raised in a catch block that handles the inner exception.</param>
	public FieldAccessException(string message, Exception inner)
		: base(message, inner)
	{
		SetErrorCode(-2146233081);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.FieldAccessException" /> class with serialized data.</summary>
	/// <param name="info">The object that holds the serialized object data. </param>
	/// <param name="context">The contextual information about the source or destination. </param>
	protected FieldAccessException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
