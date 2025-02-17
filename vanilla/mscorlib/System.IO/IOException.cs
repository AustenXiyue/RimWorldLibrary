using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System.IO;

/// <summary>The exception that is thrown when an I/O error occurs.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[ComVisible(true)]
public class IOException : SystemException
{
	[NonSerialized]
	private string _maybeFullPath;

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.IOException" /> class with its message string set to the empty string (""), its HRESULT set to COR_E_IO, and its inner exception set to a null reference.</summary>
	public IOException()
		: base(Environment.GetResourceString("I/O error occurred."))
	{
		SetErrorCode(-2146232800);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.IOException" /> class with its message string set to <paramref name="message" />, its HRESULT set to COR_E_IO, and its inner exception set to null.</summary>
	/// <param name="message">A <see cref="T:System.String" /> that describes the error. The content of <paramref name="message" /> is intended to be understood by humans. The caller of this constructor is required to ensure that this string has been localized for the current system culture. </param>
	public IOException(string message)
		: base(message)
	{
		SetErrorCode(-2146232800);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.IOException" /> class with its message string set to <paramref name="message" /> and its HRESULT user-defined.</summary>
	/// <param name="message">A <see cref="T:System.String" /> that describes the error. The content of <paramref name="message" /> is intended to be understood by humans. The caller of this constructor is required to ensure that this string has been localized for the current system culture. </param>
	/// <param name="hresult">An integer identifying the error that has occurred. </param>
	public IOException(string message, int hresult)
		: base(message)
	{
		SetErrorCode(hresult);
	}

	internal IOException(string message, int hresult, string maybeFullPath)
		: base(message)
	{
		SetErrorCode(hresult);
		_maybeFullPath = maybeFullPath;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.IOException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="message">The error message that explains the reason for the exception. </param>
	/// <param name="innerException">The exception that is the cause of the current exception. If the <paramref name="innerException" /> parameter is not null, the current exception is raised in a catch block that handles the inner exception. </param>
	public IOException(string message, Exception innerException)
		: base(message, innerException)
	{
		SetErrorCode(-2146232800);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.IOException" /> class with the specified serialization and context information.</summary>
	/// <param name="info">The data for serializing or deserializing the object. </param>
	/// <param name="context">The source and destination for the object. </param>
	protected IOException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
