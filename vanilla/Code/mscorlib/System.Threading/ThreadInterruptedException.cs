using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System.Threading;

/// <summary>The exception that is thrown when a <see cref="T:System.Threading.Thread" /> is interrupted while it is in a waiting state.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
[ComVisible(true)]
public class ThreadInterruptedException : SystemException
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.ThreadInterruptedException" /> class with default properties.</summary>
	public ThreadInterruptedException()
		: base(Exception.GetMessageFromNativeResources(ExceptionMessageKind.ThreadInterrupted))
	{
		SetErrorCode(-2146233063);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.ThreadInterruptedException" /> class with a specified error message.</summary>
	/// <param name="message">The error message that explains the reason for the exception. </param>
	public ThreadInterruptedException(string message)
		: base(message)
	{
		SetErrorCode(-2146233063);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.ThreadInterruptedException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="message">The error message that explains the reason for the exception. </param>
	/// <param name="innerException">The exception that is the cause of the current exception. If the <paramref name="innerException" /> parameter is not null, the current exception is raised in a catch block that handles the inner exception. </param>
	public ThreadInterruptedException(string message, Exception innerException)
		: base(message, innerException)
	{
		SetErrorCode(-2146233063);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.ThreadInterruptedException" /> class with serialized data.</summary>
	/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown. </param>
	/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination. </param>
	protected ThreadInterruptedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
