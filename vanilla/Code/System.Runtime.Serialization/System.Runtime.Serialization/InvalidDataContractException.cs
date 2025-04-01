namespace System.Runtime.Serialization;

/// <summary>The exception that is thrown when the <see cref="T:System.Runtime.Serialization.DataContractSerializer" /> or <see cref="T:System.Runtime.Serialization.NetDataContractSerializer" /> encounters an invalid data contract during serialization and deserialization. </summary>
[Serializable]
public class InvalidDataContractException : Exception
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Serialization.InvalidDataContractException" /> class.  </summary>
	public InvalidDataContractException()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Serialization.InvalidDataContractException" /> class with the specified error message. </summary>
	/// <param name="message">A description of the error. </param>
	public InvalidDataContractException(string message)
		: base(message)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Serialization.InvalidDataContractException" /> class with the specified error message and inner exception. </summary>
	/// <param name="message">A description of the error. </param>
	/// <param name="innerException">The original <see cref="T:System.Exception" />. </param>
	public InvalidDataContractException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Serialization.InvalidDataContractException" /> class with the specified <see cref="T:System.Runtime.Serialization.SerializationInfo" /> and <see cref="T:System.Runtime.Serialization.StreamingContext" />. </summary>
	/// <param name="info">A <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that contains data needed to serialize and deserialize an object. </param>
	/// <param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> that specifies user context during serialization and deserialization.</param>
	protected InvalidDataContractException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
