namespace System.Windows.Markup;

internal class XamlDefAttributeKeyTypeNode : XamlAttributeNode
{
	private string _assemblyName;

	private Type _valueType;

	internal string AssemblyName => _assemblyName;

	internal Type ValueType => _valueType;

	internal XamlDefAttributeKeyTypeNode(int lineNumber, int linePosition, int depth, string value, string assemblyName, Type valueType)
		: base(XamlNodeType.DefKeyTypeAttribute, lineNumber, linePosition, depth, value)
	{
		_assemblyName = assemblyName;
		_valueType = valueType;
	}
}
