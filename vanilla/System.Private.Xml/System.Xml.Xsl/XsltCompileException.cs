using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace System.Xml.Xsl;

[Serializable]
[TypeForwardedFrom("System.Xml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
public class XsltCompileException : XsltException
{
	[Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.", DiagnosticId = "SYSLIB0051", UrlFormat = "https://aka.ms/dotnet-warnings/{0}")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected XsltCompileException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	[Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.", DiagnosticId = "SYSLIB0051", UrlFormat = "https://aka.ms/dotnet-warnings/{0}")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
	}

	public XsltCompileException()
	{
	}

	public XsltCompileException(string message)
		: base(message)
	{
	}

	public XsltCompileException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public XsltCompileException(Exception inner, string sourceUri, int lineNumber, int linePosition)
		: base((lineNumber != 0) ? System.SR.Xslt_CompileError : System.SR.Xslt_CompileError2, new string[3]
		{
			sourceUri,
			lineNumber.ToString(CultureInfo.InvariantCulture),
			linePosition.ToString(CultureInfo.InvariantCulture)
		}, sourceUri, lineNumber, linePosition, inner)
	{
	}
}
