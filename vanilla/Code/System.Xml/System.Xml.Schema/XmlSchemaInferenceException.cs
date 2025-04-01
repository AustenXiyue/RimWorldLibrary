using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Xml.Schema;

/// <summary>Returns information about errors encountered by the <see cref="T:System.Xml.Schema.XmlSchemaInference" /> class while inferring a schema from an XML document.</summary>
[Serializable]
public class XmlSchemaInferenceException : XmlSchemaException
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaInferenceException" /> class with the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> and <see cref="T:System.Runtime.Serialization.StreamingContext" /> objects specified that contain all the properties of the <see cref="T:System.Xml.Schema.XmlSchemaInferenceException" />.</summary>
	/// <param name="info">A <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object.</param>
	/// <param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> object.</param>
	protected XmlSchemaInferenceException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	/// <summary>Streams all the <see cref="T:System.Xml.Schema.XmlSchemaInferenceException" /> object properties into the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object specified for the <see cref="T:System.Runtime.Serialization.StreamingContext" /> object specified.</summary>
	/// <param name="info">A <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object.</param>
	/// <param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> object.</param>
	[SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter = true)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaInferenceException" /> class.</summary>
	public XmlSchemaInferenceException()
		: base(null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaInferenceException" /> class with the error message specified.</summary>
	/// <param name="message">A description of the error.</param>
	public XmlSchemaInferenceException(string message)
		: base(message, (Exception)null, 0, 0)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaInferenceException" /> class with the error message specified and the original <see cref="T:System.Exception" /> that caused the <see cref="T:System.Xml.Schema.XmlSchemaInferenceException" /> specified.</summary>
	/// <param name="message">A description of the error.</param>
	/// <param name="innerException">An <see cref="T:System.Exception" /> object containing the original exception that caused the <see cref="T:System.Xml.Schema.XmlSchemaInferenceException" />.</param>
	public XmlSchemaInferenceException(string message, Exception innerException)
		: base(message, innerException, 0, 0)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaInferenceException" /> class with the error message specified, the original <see cref="T:System.Exception" /> that caused the <see cref="T:System.Xml.Schema.XmlSchemaInferenceException" /> specified, and the line number and line position of the error in the XML document specified.</summary>
	/// <param name="message">A description of the error.</param>
	/// <param name="innerException">An <see cref="T:System.Exception" /> object containing the original exception that caused the <see cref="T:System.Xml.Schema.XmlSchemaInferenceException" />.</param>
	/// <param name="lineNumber">The line number in the XML document that caused the <see cref="T:System.Xml.Schema.XmlSchemaInferenceException" />.</param>
	/// <param name="linePosition">The line position in the XML document that caused the <see cref="T:System.Xml.Schema.XmlSchemaInferenceException" />.</param>
	public XmlSchemaInferenceException(string message, Exception innerException, int lineNumber, int linePosition)
		: base(message, innerException, lineNumber, linePosition)
	{
	}

	internal XmlSchemaInferenceException(string res, string[] args)
		: base(res, args, null, null, 0, 0, null)
	{
	}

	internal XmlSchemaInferenceException(string res, string arg)
		: base(res, new string[1] { arg }, null, null, 0, 0, null)
	{
	}

	internal XmlSchemaInferenceException(string res, string arg, string sourceUri, int lineNumber, int linePosition)
		: base(res, new string[1] { arg }, null, sourceUri, lineNumber, linePosition, null)
	{
	}

	internal XmlSchemaInferenceException(string res, string sourceUri, int lineNumber, int linePosition)
		: base(res, null, null, sourceUri, lineNumber, linePosition, null)
	{
	}

	internal XmlSchemaInferenceException(string res, string[] args, string sourceUri, int lineNumber, int linePosition)
		: base(res, args, null, sourceUri, lineNumber, linePosition, null)
	{
	}

	internal XmlSchemaInferenceException(string res, int lineNumber, int linePosition)
		: base(res, null, null, null, lineNumber, linePosition, null)
	{
	}
}
