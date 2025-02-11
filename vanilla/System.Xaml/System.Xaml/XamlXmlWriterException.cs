using System.Runtime.Serialization;

namespace System.Xaml;

/// <summary>The exception that is thrown by certain <see cref="T:System.Xaml.XamlXmlWriter" /> APIs. </summary>
[Serializable]
public class XamlXmlWriterException : XamlException
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlXmlWriterException" /> class with a system-supplied message that describes the error.</summary>
	public XamlXmlWriterException()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlXmlWriterException" /> class with a specified message that describes the error.</summary>
	/// <param name="message">The message that describes the exception. The caller of this constructor is required to ensure that this string has been localized for the current system culture.</param>
	public XamlXmlWriterException(string message)
		: base(message)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlXmlWriterException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="message">The message that describes the exception. The caller of this constructor is required to ensure that this string has been localized for the current system culture. </param>
	/// <param name="innerException">The exception that is the cause of the current exception. If the <paramref name="innerException" /> parameter is not null, the current exception is raised in a catch block that handles the inner exception. </param>
	public XamlXmlWriterException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlXmlWriterException" /> class with serialized data.</summary>
	/// <param name="info">The object that holds the serialized object data. </param>
	/// <param name="context">The contextual information about the source or destination. </param>
	protected XamlXmlWriterException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
