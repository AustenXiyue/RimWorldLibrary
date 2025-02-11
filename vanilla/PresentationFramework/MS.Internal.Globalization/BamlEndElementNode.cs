using System.Windows.Markup;

namespace MS.Internal.Globalization;

internal sealed class BamlEndElementNode : BamlTreeNode
{
	internal BamlEndElementNode()
		: base(BamlNodeType.EndElement)
	{
	}

	internal override void Serialize(BamlWriter writer)
	{
		writer.WriteEndElement();
	}

	internal override BamlTreeNode Copy()
	{
		return new BamlEndElementNode();
	}
}
