using System.Runtime.Serialization;

namespace System.Windows.Data;

/// <summary>The exception that is thrown by the <see cref="M:System.Windows.Data.BindingGroup.GetValue(System.Object,System.String)" /> method when the value is not available.</summary>
[Serializable]
public class ValueUnavailableException : SystemException
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Data.ValueUnavailableException" /> class. </summary>
	public ValueUnavailableException()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Data.ValueUnavailableException" /> class with the specified message. </summary>
	/// <param name="message">The message that describes the error. </param>
	public ValueUnavailableException(string message)
		: base(message)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Data.ValueUnavailableException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="message">The error message that explains the reason for the exception. </param>
	/// <param name="innerException">The exception that is the cause of the current exception. If the <paramref name="innerException" /> parameter is not a null reference (Nothing in Visual Basic), the current exception is raised in a catch block that handles the inner exception. </param>
	public ValueUnavailableException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Data.ValueUnavailableException" /> class with serialized data.</summary>
	/// <param name="info">The object that holds the serialized object data. </param>
	/// <param name="context">The contextual information about the source or destination. </param>
	protected ValueUnavailableException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
