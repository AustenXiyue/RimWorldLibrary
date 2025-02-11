using System.Windows.Markup;

namespace MS.Internal.Globalization;

internal sealed class BamlStartConstructorNode : BamlTreeNode
{
	internal BamlStartConstructorNode()
		: base(BamlNodeType.StartConstructor)
	{
	}

	internal override void Serialize(BamlWriter writer)
	{
		writer.WriteStartConstructor();
	}

	internal override BamlTreeNode Copy()
	{
		return new BamlStartConstructorNode();
	}
}
