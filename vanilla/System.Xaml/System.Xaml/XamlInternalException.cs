using System.Runtime.Serialization;

namespace System.Xaml;

/// <summary>The exception that is thrown for internal inconsistencies that occur during XAML reading and XAML writing. </summary>
[Serializable]
public class XamlInternalException : XamlException
{
	private const string MessagePrefix = "Internal XAML system error: ";

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlInternalException" /> class with a system-supplied message that describes the error.</summary>
	public XamlInternalException()
		: base("Internal XAML system error: ")
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlInternalException" /> class with a specified message that describes the error.</summary>
	/// <param name="message">The message that describes the exception. </param>
	public XamlInternalException(string message)
		: base("Internal XAML system error: " + message, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlInternalException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="message">The message that describes the exception. </param>
	/// <param name="innerException">The exception that is the cause of the current exception. If the <paramref name="innerException" /> parameter is not null, the current exception is raised in a catch block that handles the inner exception. </param>
	public XamlInternalException(string message, Exception innerException)
		: base("Internal XAML system error: " + message, innerException)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlInternalException" /> class with serialized data.</summary>
	/// <param name="info">The object that holds the serialized object data. </param>
	/// <param name="context">The contextual information about the source or destination. </param>
	protected XamlInternalException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
