namespace System.Windows.Markup;

internal class XamlPropertyComplexStartNode : XamlPropertyBaseNode
{
	internal XamlPropertyComplexStartNode(int lineNumber, int linePosition, int depth, object propertyMember, string assemblyName, string typeFullName, string propertyName)
		: base(XamlNodeType.PropertyComplexStart, lineNumber, linePosition, depth, propertyMember, assemblyName, typeFullName, propertyName)
	{
	}

	internal XamlPropertyComplexStartNode(XamlNodeType token, int lineNumber, int linePosition, int depth, object propertyMember, string assemblyName, string typeFullName, string propertyName)
		: base(token, lineNumber, linePosition, depth, propertyMember, assemblyName, typeFullName, propertyName)
	{
	}
}
