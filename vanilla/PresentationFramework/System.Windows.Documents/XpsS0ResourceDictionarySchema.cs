using System.IO;
using MS.Internal;

namespace System.Windows.Documents;

internal sealed class XpsS0ResourceDictionarySchema : XpsS0Schema
{
	public XpsS0ResourceDictionarySchema()
	{
		XpsSchema.RegisterSchema(this, new ContentType[1] { XpsS0Schema._resourceDictionaryContentType });
	}

	public override string[] ExtractUriFromAttr(string attrName, string attrValue)
	{
		if (attrName.Equals("Source", StringComparison.Ordinal))
		{
			throw new FileFormatException(SR.XpsValidatingLoaderUnsupportedMimeType);
		}
		return base.ExtractUriFromAttr(attrName, attrValue);
	}
}
