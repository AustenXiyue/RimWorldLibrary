using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace System.Xml.Schema;

[Serializable]
[TypeForwardedFrom("System.Xml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
public class XmlSchemaInferenceException : XmlSchemaException
{
	[Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.", DiagnosticId = "SYSLIB0051", UrlFormat = "https://aka.ms/dotnet-warnings/{0}")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected XmlSchemaInferenceException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	[Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.", DiagnosticId = "SYSLIB0051", UrlFormat = "https://aka.ms/dotnet-warnings/{0}")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
	}

	public XmlSchemaInferenceException()
		: base(null)
	{
	}

	public XmlSchemaInferenceException(string message)
		: base(message, (Exception?)null, 0, 0)
	{
	}

	public XmlSchemaInferenceException(string message, Exception? innerException)
		: base(message, innerException, 0, 0)
	{
	}

	public XmlSchemaInferenceException(string message, Exception? innerException, int lineNumber, int linePosition)
		: base(message, innerException, lineNumber, linePosition)
	{
	}

	internal XmlSchemaInferenceException(string res, string arg)
		: base(res, new string[1] { arg }, null, null, 0, 0, null)
	{
	}

	internal XmlSchemaInferenceException(string res, int lineNumber, int linePosition)
		: base(res, null, null, null, lineNumber, linePosition, null)
	{
	}
}
