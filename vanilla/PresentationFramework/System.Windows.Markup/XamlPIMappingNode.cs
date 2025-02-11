using System.Diagnostics;

namespace System.Windows.Markup;

[DebuggerDisplay("PIMap:{_xmlns}={_clrns};{_assy}")]
internal class XamlPIMappingNode : XamlNode
{
	private string _xmlns;

	private string _clrns;

	private string _assy;

	internal string XmlNamespace => _xmlns;

	internal string ClrNamespace => _clrns;

	internal string AssemblyName => _assy;

	internal XamlPIMappingNode(int lineNumber, int linePosition, int depth, string xmlNamespace, string clrNamespace, string assemblyName)
		: base(XamlNodeType.PIMapping, lineNumber, linePosition, depth)
	{
		_xmlns = xmlNamespace;
		_clrns = clrNamespace;
		_assy = assemblyName;
	}
}
