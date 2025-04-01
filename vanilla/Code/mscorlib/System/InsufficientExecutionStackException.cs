using System.Runtime.Serialization;

namespace System;

/// <summary>The exception that is thrown when there is insufficient execution stack available to allow most methods to execute.</summary>
[Serializable]
public sealed class InsufficientExecutionStackException : SystemException
{
	/// <summary>Initializes a new instance of the <see cref="T:System.InsufficientExecutionStackException" /> class. </summary>
	public InsufficientExecutionStackException()
		: base(Environment.GetResourceString("Insufficient stack to continue executing the program safely. This can happen from having too many functions on the call stack or function on the stack using too much stack space."))
	{
		SetErrorCode(-2146232968);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.InsufficientExecutionStackException" /> class with a specified error message.</summary>
	/// <param name="message">The error message that explains the reason for the exception. </param>
	public InsufficientExecutionStackException(string message)
		: base(message)
	{
		SetErrorCode(-2146232968);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.InsufficientExecutionStackException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="message">The error message that explains the reason for the exception. </param>
	/// <param name="innerException">The exception that is the cause of the current exception. If the <paramref name="inner" /> parameter is not null, the current exception is raised in a catch block that handles the inner exception. </param>
	public InsufficientExecutionStackException(string message, Exception innerException)
		: base(message, innerException)
	{
		SetErrorCode(-2146232968);
	}

	private InsufficientExecutionStackException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
