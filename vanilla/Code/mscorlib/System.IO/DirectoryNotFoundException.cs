using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System.IO;

/// <summary>The exception that is thrown when part of a file or directory cannot be found.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
[ComVisible(true)]
public class DirectoryNotFoundException : IOException
{
	/// <summary>Initializes a new instance of the <see cref="T:System.IO.DirectoryNotFoundException" /> class with its message string set to a system-supplied message and its HRESULT set to COR_E_DIRECTORYNOTFOUND.</summary>
	public DirectoryNotFoundException()
		: base(Environment.GetResourceString("Attempted to access a path that is not on the disk."))
	{
		SetErrorCode(-2147024893);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.DirectoryNotFoundException" /> class with its message string set to <paramref name="message" /> and its HRESULT set to COR_E_DIRECTORYNOTFOUND.</summary>
	/// <param name="message">A <see cref="T:System.String" /> that describes the error. The content of <paramref name="message" /> is intended to be understood by humans. The caller of this constructor is required to ensure that this string has been localized for the current system culture. </param>
	public DirectoryNotFoundException(string message)
		: base(message)
	{
		SetErrorCode(-2147024893);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.DirectoryNotFoundException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="message">The error message that explains the reason for the exception. </param>
	/// <param name="innerException">The exception that is the cause of the current exception. If the <paramref name="innerException" /> parameter is not null, the current exception is raised in a catch block that handles the inner exception. </param>
	public DirectoryNotFoundException(string message, Exception innerException)
		: base(message, innerException)
	{
		SetErrorCode(-2147024893);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.DirectoryNotFoundException" /> class with the specified serialization and context information.</summary>
	/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown. </param>
	/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination. </param>
	protected DirectoryNotFoundException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
