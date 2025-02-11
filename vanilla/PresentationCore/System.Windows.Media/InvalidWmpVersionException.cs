using System.Runtime.Serialization;

namespace System.Windows.Media;

/// <summary>The exception that is thrown when the installed Microsoft Windows Media Player version is not supported. </summary>
[Serializable]
public class InvalidWmpVersionException : SystemException
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.InvalidWmpVersionException" /> class.</summary>
	public InvalidWmpVersionException()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.InvalidWmpVersionException" /> class with the given error message.</summary>
	/// <param name="message">The error message used to initialize the exception.</param>
	public InvalidWmpVersionException(string message)
		: base(message)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.InvalidWmpVersionException" /> class with serialization information.</summary>
	/// <param name="info">Serialization information about the object.</param>
	/// <param name="context">Context information about the serialized stream.</param>
	protected InvalidWmpVersionException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.InvalidWmpVersionException" /> class with the given error message and a reference to the inner exception that caused this exception.</summary>
	/// <param name="message">The description of the error.</param>
	/// <param name="innerException">The inner exception that caused this exception.</param>
	public InvalidWmpVersionException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
