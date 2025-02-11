namespace System.Windows.Markup;

internal class XamlContentPropertyNode : XamlNode
{
	private Type _declaringType;

	private Type _validType;

	private object _propertyMember;

	private string _assemblyName;

	private string _typeFullName;

	private string _propName;

	internal string AssemblyName => _assemblyName;

	internal string TypeFullName => _typeFullName;

	internal string PropName => _propName;

	internal Type PropDeclaringType
	{
		get
		{
			if (_declaringType == null && _propertyMember != null)
			{
				_declaringType = XamlTypeMapper.GetDeclaringType(_propertyMember);
			}
			return _declaringType;
		}
	}

	internal Type PropValidType
	{
		get
		{
			if (_validType == null)
			{
				_validType = XamlTypeMapper.GetPropertyType(_propertyMember);
			}
			return _validType;
		}
	}

	internal XamlContentPropertyNode(int lineNumber, int linePosition, int depth, object propertyMember, string assemblyName, string typeFullName, string propertyName)
		: base(XamlNodeType.ContentProperty, lineNumber, linePosition, depth)
	{
		if (typeFullName == null)
		{
			throw new ArgumentNullException("typeFullName");
		}
		if (propertyName == null)
		{
			throw new ArgumentNullException("propertyName");
		}
		_propertyMember = propertyMember;
		_assemblyName = assemblyName;
		_typeFullName = typeFullName;
		_propName = propertyName;
	}
}
