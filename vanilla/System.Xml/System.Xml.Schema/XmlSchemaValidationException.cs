using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Xml.Schema;

/// <summary>Represents the exception thrown when XML Schema Definition Language (XSD) schema validation errors and warnings are encountered in an XML document being validated. </summary>
[Serializable]
public class XmlSchemaValidationException : XmlSchemaException
{
	private object sourceNodeObject;

	/// <summary>Gets the XML node that caused this <see cref="T:System.Xml.Schema.XmlSchemaValidationException" />.</summary>
	/// <returns>The XML node that caused this <see cref="T:System.Xml.Schema.XmlSchemaValidationException" />.</returns>
	public object SourceObject => sourceNodeObject;

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaValidationException" /> class with the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> and <see cref="T:System.Runtime.Serialization.StreamingContext" /> objects specified.</summary>
	/// <param name="info">A <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object.</param>
	/// <param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> object.</param>
	protected XmlSchemaValidationException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	/// <summary>Constructs a new <see cref="T:System.Xml.Schema.XmlSchemaValidationException" /> object with the given <see cref="T:System.Runtime.Serialization.SerializationInfo" /> and <see cref="T:System.Runtime.Serialization.StreamingContext" /> information that contains all the properties of the <see cref="T:System.Xml.Schema.XmlSchemaValidationException" />.</summary>
	/// <param name="info">
	///   <see cref="T:System.Runtime.Serialization.SerializationInfo" />
	/// </param>
	/// <param name="context">
	///   <see cref="T:System.Runtime.Serialization.StreamingContext" />
	/// </param>
	[SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter = true)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaValidationException" /> class.</summary>
	public XmlSchemaValidationException()
		: base(null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaValidationException" /> class with the exception message specified.</summary>
	/// <param name="message">A string description of the error condition.</param>
	public XmlSchemaValidationException(string message)
		: base(message, (Exception)null, 0, 0)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaValidationException" /> class with the exception message and original <see cref="T:System.Exception" /> object that caused this exception specified.</summary>
	/// <param name="message">A string description of the error condition.</param>
	/// <param name="innerException">The original <see cref="T:System.Exception" /> object that caused this exception.</param>
	public XmlSchemaValidationException(string message, Exception innerException)
		: base(message, innerException, 0, 0)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaValidationException" /> class with the exception message specified, and the original <see cref="T:System.Exception" /> object, line number, and line position of the XML that cause this exception specified.</summary>
	/// <param name="message">A string description of the error condition.</param>
	/// <param name="innerException">The original <see cref="T:System.Exception" /> object that caused this exception.</param>
	/// <param name="lineNumber">The line number of the XML that caused this exception.</param>
	/// <param name="linePosition">The line position of the XML that caused this exception.</param>
	public XmlSchemaValidationException(string message, Exception innerException, int lineNumber, int linePosition)
		: base(message, innerException, lineNumber, linePosition)
	{
	}

	internal XmlSchemaValidationException(string res, string[] args)
		: base(res, args, null, null, 0, 0, null)
	{
	}

	internal XmlSchemaValidationException(string res, string arg)
		: base(res, new string[1] { arg }, null, null, 0, 0, null)
	{
	}

	internal XmlSchemaValidationException(string res, string arg, string sourceUri, int lineNumber, int linePosition)
		: base(res, new string[1] { arg }, null, sourceUri, lineNumber, linePosition, null)
	{
	}

	internal XmlSchemaValidationException(string res, string sourceUri, int lineNumber, int linePosition)
		: base(res, null, null, sourceUri, lineNumber, linePosition, null)
	{
	}

	internal XmlSchemaValidationException(string res, string[] args, string sourceUri, int lineNumber, int linePosition)
		: base(res, args, null, sourceUri, lineNumber, linePosition, null)
	{
	}

	internal XmlSchemaValidationException(string res, string[] args, Exception innerException, string sourceUri, int lineNumber, int linePosition)
		: base(res, args, innerException, sourceUri, lineNumber, linePosition, null)
	{
	}

	internal XmlSchemaValidationException(string res, string[] args, object sourceNode)
		: base(res, args, null, null, 0, 0, null)
	{
		sourceNodeObject = sourceNode;
	}

	internal XmlSchemaValidationException(string res, string[] args, string sourceUri, object sourceNode)
		: base(res, args, null, sourceUri, 0, 0, null)
	{
		sourceNodeObject = sourceNode;
	}

	internal XmlSchemaValidationException(string res, string[] args, string sourceUri, int lineNumber, int linePosition, XmlSchemaObject source, object sourceNode)
		: base(res, args, null, sourceUri, lineNumber, linePosition, source)
	{
		sourceNodeObject = sourceNode;
	}

	/// <summary>Sets the XML node that causes the error.</summary>
	/// <param name="sourceObject">The source object.</param>
	protected internal void SetSourceObject(object sourceObject)
	{
		sourceNodeObject = sourceObject;
	}
}
