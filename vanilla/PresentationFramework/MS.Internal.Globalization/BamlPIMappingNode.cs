using System.Windows.Markup;

namespace MS.Internal.Globalization;

internal sealed class BamlPIMappingNode : BamlTreeNode
{
	private string _xmlNamespace;

	private string _clrNamespace;

	private string _assemblyName;

	internal BamlPIMappingNode(string xmlNamespace, string clrNamespace, string assemblyName)
		: base(BamlNodeType.PIMapping)
	{
		_xmlNamespace = xmlNamespace;
		_clrNamespace = clrNamespace;
		_assemblyName = assemblyName;
	}

	internal override void Serialize(BamlWriter writer)
	{
		writer.WritePIMapping(_xmlNamespace, _clrNamespace, _assemblyName);
	}

	internal override BamlTreeNode Copy()
	{
		return new BamlPIMappingNode(_xmlNamespace, _clrNamespace, _assemblyName);
	}
}
