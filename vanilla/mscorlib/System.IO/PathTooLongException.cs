using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System.IO;

/// <summary>The exception that is thrown when a path or file name is longer than the system-defined maximum length.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
[ComVisible(true)]
public class PathTooLongException : IOException
{
	/// <summary>Initializes a new instance of the <see cref="T:System.IO.PathTooLongException" /> class with its HRESULT set to COR_E_PATHTOOLONG.</summary>
	public PathTooLongException()
		: base(Environment.GetResourceString("The specified path, file name, or both are too long. The fully qualified file name must be less than 260 characters, and the directory name must be less than 248 characters."))
	{
		SetErrorCode(-2147024690);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.PathTooLongException" /> class with its message string set to <paramref name="message" /> and its HRESULT set to COR_E_PATHTOOLONG.</summary>
	/// <param name="message">A <see cref="T:System.String" /> that describes the error. The content of <paramref name="message" /> is intended to be understood by humans. The caller of this constructor is required to ensure that this string has been localized for the current system culture. </param>
	public PathTooLongException(string message)
		: base(message)
	{
		SetErrorCode(-2147024690);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.PathTooLongException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="message">A <see cref="T:System.String" /> that describes the error. The content of <paramref name="message" /> is intended to be understood by humans. The caller of this constructor is required to ensure that this string has been localized for the current system culture. </param>
	/// <param name="innerException">The exception that is the cause of the current exception. If the <paramref name="innerException" /> parameter is not null, the current exception is raised in a catch block that handles the inner exception. </param>
	public PathTooLongException(string message, Exception innerException)
		: base(message, innerException)
	{
		SetErrorCode(-2147024690);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.PathTooLongException" /> class with the specified serialization and context information.</summary>
	/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown. </param>
	/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination. </param>
	protected PathTooLongException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
