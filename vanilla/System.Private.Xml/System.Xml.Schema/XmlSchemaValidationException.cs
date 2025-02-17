using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace System.Xml.Schema;

[Serializable]
[TypeForwardedFrom("System.Xml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
public class XmlSchemaValidationException : XmlSchemaException
{
	private object _sourceNodeObject;

	public object? SourceObject => _sourceNodeObject;

	[Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.", DiagnosticId = "SYSLIB0051", UrlFormat = "https://aka.ms/dotnet-warnings/{0}")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected XmlSchemaValidationException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	[Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.", DiagnosticId = "SYSLIB0051", UrlFormat = "https://aka.ms/dotnet-warnings/{0}")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
	}

	public XmlSchemaValidationException()
		: base(null)
	{
	}

	public XmlSchemaValidationException(string? message)
		: base(message, (Exception?)null, 0, 0)
	{
	}

	public XmlSchemaValidationException(string? message, Exception? innerException)
		: base(message, innerException, 0, 0)
	{
	}

	public XmlSchemaValidationException(string? message, Exception? innerException, int lineNumber, int linePosition)
		: base(message, innerException, lineNumber, linePosition)
	{
	}

	internal XmlSchemaValidationException(string res, string arg, string sourceUri, int lineNumber, int linePosition)
		: base(res, new string[1] { arg }, null, sourceUri, lineNumber, linePosition, null)
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
		_sourceNodeObject = sourceNode;
	}

	protected internal void SetSourceObject(object? sourceObject)
	{
		_sourceNodeObject = sourceObject;
	}
}
