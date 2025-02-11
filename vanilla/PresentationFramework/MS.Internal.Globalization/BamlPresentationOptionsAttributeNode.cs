using System.Windows.Markup;

namespace MS.Internal.Globalization;

internal sealed class BamlPresentationOptionsAttributeNode : BamlTreeNode
{
	private string _name;

	private string _value;

	internal BamlPresentationOptionsAttributeNode(string name, string value)
		: base(BamlNodeType.PresentationOptionsAttribute)
	{
		_name = name;
		_value = value;
	}

	internal override void Serialize(BamlWriter writer)
	{
		writer.WritePresentationOptionsAttribute(_name, _value);
	}

	internal override BamlTreeNode Copy()
	{
		return new BamlPresentationOptionsAttributeNode(_name, _value);
	}
}
