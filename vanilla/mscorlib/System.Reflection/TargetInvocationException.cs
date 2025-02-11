using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System.Reflection;

/// <summary>The exception that is thrown by methods invoked through reflection. This class cannot be inherited.</summary>
[Serializable]
[ComVisible(true)]
public sealed class TargetInvocationException : ApplicationException
{
	private TargetInvocationException()
		: base(Environment.GetResourceString("Exception has been thrown by the target of an invocation."))
	{
		SetErrorCode(-2146232828);
	}

	private TargetInvocationException(string message)
		: base(message)
	{
		SetErrorCode(-2146232828);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Reflection.TargetInvocationException" /> class with a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="inner">The exception that is the cause of the current exception. If the <paramref name="inner" /> parameter is not null, the current exception is raised in a catch block that handles the inner exception. </param>
	public TargetInvocationException(Exception inner)
		: base(Environment.GetResourceString("Exception has been thrown by the target of an invocation."), inner)
	{
		SetErrorCode(-2146232828);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Reflection.TargetInvocationException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="message">The error message that explains the reason for the exception. </param>
	/// <param name="inner">The exception that is the cause of the current exception. If the <paramref name="inner" /> parameter is not null, the current exception is raised in a catch block that handles the inner exception. </param>
	public TargetInvocationException(string message, Exception inner)
		: base(message, inner)
	{
		SetErrorCode(-2146232828);
	}

	internal TargetInvocationException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
