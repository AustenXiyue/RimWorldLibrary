using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System.Threading;

/// <summary>The exception that is thrown when an attempt is made to open a system mutex or semaphore that does not exist.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
[ComVisible(false)]
public class WaitHandleCannotBeOpenedException : ApplicationException
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.WaitHandleCannotBeOpenedException" /> class with default values.</summary>
	public WaitHandleCannotBeOpenedException()
		: base(Environment.GetResourceString("No handle of the given name exists."))
	{
		SetErrorCode(-2146233044);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.WaitHandleCannotBeOpenedException" /> class with a specified error message.</summary>
	/// <param name="message">The error message that explains the reason for the exception.</param>
	public WaitHandleCannotBeOpenedException(string message)
		: base(message)
	{
		SetErrorCode(-2146233044);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.WaitHandleCannotBeOpenedException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="message">The error message that explains the reason for the exception.</param>
	/// <param name="innerException">The exception that is the cause of the current exception. If the <paramref name="innerException" /> parameter is not null, the current exception is raised in a catch block that handles the inner exception.</param>
	public WaitHandleCannotBeOpenedException(string message, Exception innerException)
		: base(message, innerException)
	{
		SetErrorCode(-2146233044);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.WaitHandleCannotBeOpenedException" /> class with serialized data.</summary>
	/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object that holds the serialized object data about the exception being thrown. </param>
	/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> object that contains contextual information about the source or destination.</param>
	protected WaitHandleCannotBeOpenedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
