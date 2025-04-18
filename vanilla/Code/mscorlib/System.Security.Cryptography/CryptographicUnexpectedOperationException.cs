using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System.Security.Cryptography;

/// <summary>The exception that is thrown when an unexpected operation occurs during a cryptographic operation.</summary>
[Serializable]
[ComVisible(true)]
public class CryptographicUnexpectedOperationException : CryptographicException
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.CryptographicUnexpectedOperationException" /> class with default properties.</summary>
	public CryptographicUnexpectedOperationException()
	{
		SetErrorCode(-2146233295);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.CryptographicUnexpectedOperationException" /> class with a specified error message.</summary>
	/// <param name="message">The error message that explains the reason for the exception. </param>
	public CryptographicUnexpectedOperationException(string message)
		: base(message)
	{
		SetErrorCode(-2146233295);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.CryptographicUnexpectedOperationException" /> class with a specified error message in the specified format.</summary>
	/// <param name="format">The format used to output the error message. </param>
	/// <param name="insert">The error message that explains the reason for the exception. </param>
	public CryptographicUnexpectedOperationException(string format, string insert)
		: base(string.Format(CultureInfo.CurrentCulture, format, insert))
	{
		SetErrorCode(-2146233295);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.CryptographicUnexpectedOperationException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="message">The error message that explains the reason for the exception. </param>
	/// <param name="inner">The exception that is the cause of the current exception. If the <paramref name="inner" /> parameter is not null, the current exception is raised in a catch block that handles the inner exception. </param>
	public CryptographicUnexpectedOperationException(string message, Exception inner)
		: base(message, inner)
	{
		SetErrorCode(-2146233295);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.CryptographicUnexpectedOperationException" /> class with serialized data.</summary>
	/// <param name="info">The object that holds the serialized object data. </param>
	/// <param name="context">The contextual information about the source or destination. </param>
	protected CryptographicUnexpectedOperationException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
