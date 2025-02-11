using System.Windows.Markup;

namespace MS.Internal.Globalization;

internal sealed class BamlDefAttributeNode : BamlTreeNode
{
	private string _name;

	private string _value;

	internal BamlDefAttributeNode(string name, string value)
		: base(BamlNodeType.DefAttribute)
	{
		_name = name;
		_value = value;
	}

	internal override void Serialize(BamlWriter writer)
	{
		writer.WriteDefAttribute(_name, _value);
	}

	internal override BamlTreeNode Copy()
	{
		return new BamlDefAttributeNode(_name, _value);
	}
}
