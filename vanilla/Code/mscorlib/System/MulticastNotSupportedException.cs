using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System;

/// <summary>The exception that is thrown when there is an attempt to combine two delegates based on the <see cref="T:System.Delegate" /> type instead of the <see cref="T:System.MulticastDelegate" /> type. This class cannot be inherited. </summary>
/// <filterpriority>2</filterpriority>
[Serializable]
[ComVisible(true)]
public sealed class MulticastNotSupportedException : SystemException
{
	/// <summary>Initializes a new instance of the <see cref="T:System.MulticastNotSupportedException" /> class.</summary>
	public MulticastNotSupportedException()
		: base(Environment.GetResourceString("Attempted to add multiple callbacks to a delegate that does not support multicast."))
	{
		SetErrorCode(-2146233068);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.MulticastNotSupportedException" /> class with a specified error message.</summary>
	/// <param name="message">The message that describes the error. </param>
	public MulticastNotSupportedException(string message)
		: base(message)
	{
		SetErrorCode(-2146233068);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.MulticastNotSupportedException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="message">The error message that explains the reason for the exception. </param>
	/// <param name="inner">The exception that is the cause of the current exception. If the <paramref name="inner" /> parameter is not a null reference (Nothing in Visual Basic), the current exception is raised in a catch block that handles the inner exception. </param>
	public MulticastNotSupportedException(string message, Exception inner)
		: base(message, inner)
	{
		SetErrorCode(-2146233068);
	}

	internal MulticastNotSupportedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
