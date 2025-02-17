using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System.Resources;

/// <summary>The exception that is thrown if the main assembly does not contain the resources for the neutral culture, and an appropriate satellite assembly is missing.</summary>
[Serializable]
[ComVisible(true)]
public class MissingManifestResourceException : SystemException
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Resources.MissingManifestResourceException" /> class with default properties.</summary>
	public MissingManifestResourceException()
		: base(Environment.GetResourceString("Unable to find manifest resource."))
	{
		SetErrorCode(-2146233038);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Resources.MissingManifestResourceException" /> class with the specified error message.</summary>
	/// <param name="message">The error message that explains the reason for the exception. </param>
	public MissingManifestResourceException(string message)
		: base(message)
	{
		SetErrorCode(-2146233038);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Resources.MissingManifestResourceException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="message">The error message that explains the reason for the exception. </param>
	/// <param name="inner">The exception that is the cause of the current exception. If the <paramref name="inner" /> parameter is not null, the current exception is raised in a catch block that handles the inner exception. </param>
	public MissingManifestResourceException(string message, Exception inner)
		: base(message, inner)
	{
		SetErrorCode(-2146233038);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Resources.MissingManifestResourceException" /> class from serialized data.</summary>
	/// <param name="info">The object that holds the serialized object data. </param>
	/// <param name="context">The contextual information about the source or destination of the exception. </param>
	protected MissingManifestResourceException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
