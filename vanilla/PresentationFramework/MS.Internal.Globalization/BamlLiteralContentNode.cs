using System.Windows.Markup;

namespace MS.Internal.Globalization;

internal sealed class BamlLiteralContentNode : BamlTreeNode
{
	private string _literalContent;

	internal string Content
	{
		get
		{
			return _literalContent;
		}
		set
		{
			_literalContent = value;
		}
	}

	internal BamlLiteralContentNode(string literalContent)
		: base(BamlNodeType.LiteralContent)
	{
		_literalContent = literalContent;
	}

	internal override void Serialize(BamlWriter writer)
	{
		writer.WriteLiteralContent(_literalContent);
	}

	internal override BamlTreeNode Copy()
	{
		return new BamlLiteralContentNode(_literalContent);
	}
}
