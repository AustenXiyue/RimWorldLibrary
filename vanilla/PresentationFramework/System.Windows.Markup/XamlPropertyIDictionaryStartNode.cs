namespace System.Windows.Markup;

internal class XamlPropertyIDictionaryStartNode : XamlPropertyComplexStartNode
{
	internal XamlPropertyIDictionaryStartNode(int lineNumber, int linePosition, int depth, object propertyMember, string assemblyName, string typeFullName, string propertyName)
		: base(XamlNodeType.PropertyIDictionaryStart, lineNumber, linePosition, depth, propertyMember, assemblyName, typeFullName, propertyName)
	{
	}
}
