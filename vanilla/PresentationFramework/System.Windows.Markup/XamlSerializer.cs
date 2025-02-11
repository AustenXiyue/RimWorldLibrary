using System.IO;

namespace System.Windows.Markup;

internal class XamlSerializer
{
	internal const string DefNamespacePrefix = "x";

	internal const string DefNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";

	internal const string ArrayTag = "Array";

	internal const string ArrayTagTypeAttribute = "Type";

	internal virtual void ConvertXamlToBaml(XamlReaderHelper tokenReader, ParserContext context, XamlNode xamlNode, BamlRecordWriter bamlWriter)
	{
		throw new InvalidOperationException(SR.InvalidDeSerialize);
	}

	internal virtual void ConvertXamlToObject(XamlReaderHelper tokenReader, ReadWriteStreamManager streamManager, ParserContext context, XamlNode xamlNode, BamlRecordReader reader)
	{
		throw new InvalidOperationException(SR.InvalidDeSerialize);
	}

	internal virtual void ConvertBamlToObject(BamlRecordReader reader, BamlRecord bamlRecord, ParserContext context)
	{
		throw new InvalidOperationException(SR.InvalidDeSerialize);
	}

	public virtual bool ConvertStringToCustomBinary(BinaryWriter writer, string stringValue)
	{
		throw new InvalidOperationException(SR.InvalidCustomSerialize);
	}

	public virtual object ConvertCustomBinaryToObject(BinaryReader reader)
	{
		throw new InvalidOperationException(SR.InvalidCustomSerialize);
	}

	internal virtual object GetDictionaryKey(BamlRecord bamlRecord, ParserContext parserContext)
	{
		return null;
	}
}
