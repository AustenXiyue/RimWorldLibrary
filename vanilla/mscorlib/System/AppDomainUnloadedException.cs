using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System;

/// <summary>The exception that is thrown when an attempt is made to access an unloaded application domain. </summary>
/// <filterpriority>2</filterpriority>
[Serializable]
[ComVisible(true)]
public class AppDomainUnloadedException : SystemException
{
	/// <summary>Initializes a new instance of the <see cref="T:System.AppDomainUnloadedException" /> class.</summary>
	public AppDomainUnloadedException()
		: base(Environment.GetResourceString("Attempted to access an unloaded AppDomain."))
	{
		SetErrorCode(-2146234348);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.AppDomainUnloadedException" /> class with a specified error message.</summary>
	/// <param name="message">The message that describes the error. </param>
	public AppDomainUnloadedException(string message)
		: base(message)
	{
		SetErrorCode(-2146234348);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.AppDomainUnloadedException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="message">The message that describes the error. </param>
	/// <param name="innerException">The exception that is the cause of the current exception. If the <paramref name="innerException" /> parameter is not a null reference, the current exception is raised in a catch block that handles the inner exception. </param>
	public AppDomainUnloadedException(string message, Exception innerException)
		: base(message, innerException)
	{
		SetErrorCode(-2146234348);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.AppDomainUnloadedException" /> class with serialized data.</summary>
	/// <param name="info">The object that holds the serialized object data. </param>
	/// <param name="context">The contextual information about the source or destination. </param>
	protected AppDomainUnloadedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
