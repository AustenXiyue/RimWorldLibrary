namespace System.Windows.Markup;

internal class XamlPropertyIListStartNode : XamlPropertyComplexStartNode
{
	internal XamlPropertyIListStartNode(int lineNumber, int linePosition, int depth, object propertyMember, string assemblyName, string typeFullName, string propertyName)
		: base(XamlNodeType.PropertyIListStart, lineNumber, linePosition, depth, propertyMember, assemblyName, typeFullName, propertyName)
	{
	}
}
