namespace System.Windows.Markup;

internal class XamlKeyElementStartNode : XamlElementStartNode
{
	internal XamlKeyElementStartNode(int lineNumber, int linePosition, int depth, string assemblyName, string typeFullName, Type elementType, Type serializerType)
		: base(XamlNodeType.KeyElementStart, lineNumber, linePosition, depth, assemblyName, typeFullName, elementType, serializerType, isEmptyElement: false, needsDictionaryKey: false, isInjected: false)
	{
	}
}
