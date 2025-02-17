using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System;

/// <summary>The exception that is thrown when a non-fatal application error occurs.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
[ComVisible(true)]
public class ApplicationException : Exception
{
	/// <summary>Initializes a new instance of the <see cref="T:System.ApplicationException" /> class.</summary>
	public ApplicationException()
		: base(Environment.GetResourceString("Error in the application."))
	{
		SetErrorCode(-2146232832);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ApplicationException" /> class with a specified error message.</summary>
	/// <param name="message">A message that describes the error. </param>
	public ApplicationException(string message)
		: base(message)
	{
		SetErrorCode(-2146232832);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ApplicationException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="message">The error message that explains the reason for the exception. </param>
	/// <param name="innerException">The exception that is the cause of the current exception. If the <paramref name="innerException" /> parameter is not a null reference, the current exception is raised in a catch block that handles the inner exception. </param>
	public ApplicationException(string message, Exception innerException)
		: base(message, innerException)
	{
		SetErrorCode(-2146232832);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ApplicationException" /> class with serialized data.</summary>
	/// <param name="info">The object that holds the serialized object data. </param>
	/// <param name="context">The contextual information about the source or destination. </param>
	protected ApplicationException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
