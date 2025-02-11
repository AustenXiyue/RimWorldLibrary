using System.Windows.Markup;

namespace MS.Internal.Globalization;

internal sealed class BamlXmlnsPropertyNode : BamlTreeNode
{
	private string _prefix;

	private string _xmlns;

	internal BamlXmlnsPropertyNode(string prefix, string xmlns)
		: base(BamlNodeType.XmlnsProperty)
	{
		_prefix = prefix;
		_xmlns = xmlns;
	}

	internal override void Serialize(BamlWriter writer)
	{
		writer.WriteXmlnsProperty(_prefix, _xmlns);
	}

	internal override BamlTreeNode Copy()
	{
		return new BamlXmlnsPropertyNode(_prefix, _xmlns);
	}
}
