using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System;

/// <summary>The exception that is thrown when the format of an argument does not meet the parameter specifications of the invoked method.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
[ComVisible(true)]
public class FormatException : SystemException
{
	/// <summary>Initializes a new instance of the <see cref="T:System.FormatException" /> class.</summary>
	public FormatException()
		: base(Environment.GetResourceString("One of the identified items was in an invalid format."))
	{
		SetErrorCode(-2146233033);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.FormatException" /> class with a specified error message.</summary>
	/// <param name="message">The message that describes the error. </param>
	public FormatException(string message)
		: base(message)
	{
		SetErrorCode(-2146233033);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.FormatException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="message">The error message that explains the reason for the exception. </param>
	/// <param name="innerException">The exception that is the cause of the current exception. If the <paramref name="innerException" /> parameter is not a null reference (Nothing in Visual Basic), the current exception is raised in a catch block that handles the inner exception. </param>
	public FormatException(string message, Exception innerException)
		: base(message, innerException)
	{
		SetErrorCode(-2146233033);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.FormatException" /> class with serialized data.</summary>
	/// <param name="info">The object that holds the serialized object data. </param>
	/// <param name="context">The contextual information about the source or destination. </param>
	protected FormatException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
