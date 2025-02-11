using System.Windows.Markup;

namespace MS.Internal.Globalization;

internal sealed class BamlEndDocumentNode : BamlTreeNode
{
	internal BamlEndDocumentNode()
		: base(BamlNodeType.EndDocument)
	{
	}

	internal override void Serialize(BamlWriter writer)
	{
		writer.WriteEndDocument();
	}

	internal override BamlTreeNode Copy()
	{
		return new BamlEndDocumentNode();
	}
}
