using System.Windows.Markup;

namespace MS.Internal.Globalization;

internal sealed class BamlEndComplexPropertyNode : BamlTreeNode
{
	internal BamlEndComplexPropertyNode()
		: base(BamlNodeType.EndComplexProperty)
	{
	}

	internal override void Serialize(BamlWriter writer)
	{
		writer.WriteEndComplexProperty();
	}

	internal override BamlTreeNode Copy()
	{
		return new BamlEndComplexPropertyNode();
	}
}
