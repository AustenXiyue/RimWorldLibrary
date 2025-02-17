using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System.Collections.Generic;

/// <summary>The exception that is thrown when the key specified for accessing an element in a collection does not match any key in the collection.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[ComVisible(true)]
public class KeyNotFoundException : SystemException, ISerializable
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.KeyNotFoundException" /> class using default property values.</summary>
	public KeyNotFoundException()
		: base(Environment.GetResourceString("The given key was not present in the dictionary."))
	{
		SetErrorCode(-2146232969);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.KeyNotFoundException" /> class with the specified error message.</summary>
	/// <param name="message">The message that describes the error.</param>
	public KeyNotFoundException(string message)
		: base(message)
	{
		SetErrorCode(-2146232969);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.KeyNotFoundException" /> class with the specified error message and a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="message">The error message that explains the reason for the exception.</param>
	/// <param name="innerException">The exception that is the cause of the current exception. If the <paramref name="innerException" /> parameter is not null, the current exception is raised in a catch block that handles the inner exception.</param>
	public KeyNotFoundException(string message, Exception innerException)
		: base(message, innerException)
	{
		SetErrorCode(-2146232969);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.KeyNotFoundException" /> class with serialized data.</summary>
	/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
	/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" />  that contains contextual information about the source or destination.</param>
	protected KeyNotFoundException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
