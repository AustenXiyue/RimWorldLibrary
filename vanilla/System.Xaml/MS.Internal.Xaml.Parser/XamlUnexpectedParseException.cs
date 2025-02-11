using System;
using System.Runtime.Serialization;
using System.Xaml;

namespace MS.Internal.Xaml.Parser;

[Serializable]
internal class XamlUnexpectedParseException : XamlParseException
{
	public XamlUnexpectedParseException()
	{
	}

	public XamlUnexpectedParseException(string message)
		: base(message)
	{
	}

	public XamlUnexpectedParseException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public XamlUnexpectedParseException(XamlScanner xamlScanner, ScannerNodeType nodetype, string parseRule)
		: base(xamlScanner, System.SR.Format(System.SR.UnexpectedNodeType, nodetype.ToString(), parseRule))
	{
	}

	protected XamlUnexpectedParseException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
