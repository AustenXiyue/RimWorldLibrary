using System.Windows.Markup;

namespace MS.Internal.Globalization;

internal sealed class BamlTextNode : BamlTreeNode
{
	private string _content;

	private string _typeConverterAssemblyName;

	private string _typeConverterName;

	internal string Content => _content;

	internal BamlTextNode(string text)
		: this(text, null, null)
	{
	}

	internal BamlTextNode(string text, string typeConverterAssemblyName, string typeConverterName)
		: base(BamlNodeType.Text)
	{
		_content = text;
		_typeConverterAssemblyName = typeConverterAssemblyName;
		_typeConverterName = typeConverterName;
	}

	internal override void Serialize(BamlWriter writer)
	{
		writer.WriteText(_content, _typeConverterAssemblyName, _typeConverterName);
	}

	internal override BamlTreeNode Copy()
	{
		return new BamlTextNode(_content, _typeConverterAssemblyName, _typeConverterName);
	}
}
