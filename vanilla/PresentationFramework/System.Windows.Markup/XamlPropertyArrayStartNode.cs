namespace System.Windows.Markup;

internal class XamlPropertyArrayStartNode : XamlPropertyComplexStartNode
{
	internal XamlPropertyArrayStartNode(int lineNumber, int linePosition, int depth, object propertyMember, string assemblyName, string typeFullName, string propertyName)
		: base(XamlNodeType.PropertyArrayStart, lineNumber, linePosition, depth, propertyMember, assemblyName, typeFullName, propertyName)
	{
	}
}
