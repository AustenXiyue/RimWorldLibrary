using System.Windows.Markup;

namespace MS.Internal.Globalization;

internal sealed class BamlContentPropertyNode : BamlTreeNode
{
	private string _assemblyName;

	private string _typeFullName;

	private string _propertyName;

	internal BamlContentPropertyNode(string assemblyName, string typeFullName, string propertyName)
		: base(BamlNodeType.ContentProperty)
	{
		_assemblyName = assemblyName;
		_typeFullName = typeFullName;
		_propertyName = propertyName;
	}

	internal override void Serialize(BamlWriter writer)
	{
		writer.WriteContentProperty(_assemblyName, _typeFullName, _propertyName);
	}

	internal override BamlTreeNode Copy()
	{
		return new BamlContentPropertyNode(_assemblyName, _typeFullName, _propertyName);
	}
}
