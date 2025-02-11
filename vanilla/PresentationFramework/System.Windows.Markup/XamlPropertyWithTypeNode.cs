namespace System.Windows.Markup;

internal class XamlPropertyWithTypeNode : XamlPropertyBaseNode
{
	private string _valueTypeFullname;

	private string _valueTypeAssemblyName;

	private Type _valueElementType;

	private string _valueSerializerTypeFullName;

	private string _valueSerializerTypeAssemblyName;

	internal string ValueTypeFullName => _valueTypeFullname;

	internal string ValueTypeAssemblyName => _valueTypeAssemblyName;

	internal Type ValueElementType => _valueElementType;

	internal string ValueSerializerTypeFullName => _valueSerializerTypeFullName;

	internal string ValueSerializerTypeAssemblyName => _valueSerializerTypeAssemblyName;

	internal XamlPropertyWithTypeNode(int lineNumber, int linePosition, int depth, object propertyMember, string assemblyName, string typeFullName, string propertyName, string valueTypeFullName, string valueAssemblyName, Type valueElementType, string valueSerializerTypeFullName, string valueSerializerTypeAssemblyName)
		: base(XamlNodeType.PropertyWithType, lineNumber, linePosition, depth, propertyMember, assemblyName, typeFullName, propertyName)
	{
		_valueTypeFullname = valueTypeFullName;
		_valueTypeAssemblyName = valueAssemblyName;
		_valueElementType = valueElementType;
		_valueSerializerTypeFullName = valueSerializerTypeFullName;
		_valueSerializerTypeAssemblyName = valueSerializerTypeAssemblyName;
	}
}
