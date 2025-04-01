using System.Runtime.Serialization;

namespace System.IO;

/// <summary>The exception that is thrown when a data stream is in an invalid format.</summary>
[Serializable]
public sealed class InvalidDataException : SystemException
{
	private const int Result = -2146233085;

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.InvalidDataException" /> class.</summary>
	public InvalidDataException()
		: base(global::Locale.GetText("Invalid data format."))
	{
		base.HResult = -2146233085;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.InvalidDataException" /> class with a specified error message.</summary>
	/// <param name="message">The error message that explains the reason for the exception.</param>
	public InvalidDataException(string message)
		: base(message)
	{
		base.HResult = -2146233085;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.InvalidDataException" /> class with a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="message">The error message that explains the reason for the exception.</param>
	/// <param name="innerException">The exception that is the cause of the current exception. If the <paramref name="innerException" /> parameter is not null, the current exception is raised in a catch block that handles the inner exception.</param>
	public InvalidDataException(string message, Exception innerException)
		: base(message, innerException)
	{
		base.HResult = -2146233085;
	}

	private InvalidDataException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
