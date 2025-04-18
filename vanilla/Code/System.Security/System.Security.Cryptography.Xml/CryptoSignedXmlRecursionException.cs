using System.Runtime.Serialization;
using System.Xml;

namespace System.Security.Cryptography.Xml;

[Serializable]
internal class CryptoSignedXmlRecursionException : XmlException
{
	public CryptoSignedXmlRecursionException()
	{
	}

	public CryptoSignedXmlRecursionException(string message)
		: base(message)
	{
	}

	public CryptoSignedXmlRecursionException(string message, Exception inner)
		: base(message, inner)
	{
	}

	protected CryptoSignedXmlRecursionException(SerializationInfo info, StreamingContext context)
	{
		throw new PlatformNotSupportedException();
	}
}
