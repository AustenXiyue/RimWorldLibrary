namespace System.Windows.Markup;

internal class XamlPropertyWithExtensionNode : XamlPropertyBaseNode
{
	private short _extensionTypeId;

	private string _value;

	private bool _isValueNestedExtension;

	private bool _isValueTypeExtension;

	private Type _defaultTargetType;

	internal short ExtensionTypeId => _extensionTypeId;

	internal string Value => _value;

	internal bool IsValueNestedExtension => _isValueNestedExtension;

	internal bool IsValueTypeExtension => _isValueTypeExtension;

	internal Type DefaultTargetType
	{
		get
		{
			return _defaultTargetType;
		}
		set
		{
			_defaultTargetType = value;
		}
	}

	internal XamlPropertyWithExtensionNode(int lineNumber, int linePosition, int depth, object propertyMember, string assemblyName, string typeFullName, string propertyName, string value, short extensionTypeId, bool isValueNestedExtension, bool isValueTypeExtension)
		: base(XamlNodeType.PropertyWithExtension, lineNumber, linePosition, depth, propertyMember, assemblyName, typeFullName, propertyName)
	{
		_value = value;
		_extensionTypeId = extensionTypeId;
		_isValueNestedExtension = isValueNestedExtension;
		_isValueTypeExtension = isValueTypeExtension;
		_defaultTargetType = null;
	}
}
