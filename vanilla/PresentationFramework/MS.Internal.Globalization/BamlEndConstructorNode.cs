using System.Windows.Markup;

namespace MS.Internal.Globalization;

internal sealed class BamlEndConstructorNode : BamlTreeNode
{
	internal BamlEndConstructorNode()
		: base(BamlNodeType.EndConstructor)
	{
	}

	internal override void Serialize(BamlWriter writer)
	{
		writer.WriteEndConstructor();
	}

	internal override BamlTreeNode Copy()
	{
		return new BamlEndConstructorNode();
	}
}
