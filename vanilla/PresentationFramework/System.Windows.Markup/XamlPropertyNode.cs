namespace System.Windows.Markup;

internal class XamlPropertyNode : XamlPropertyBaseNode
{
	private string _value;

	private BamlAttributeUsage _attributeUsage;

	private bool _complexAsSimple;

	private bool _isDefinitionName;

	private Type _valueDeclaringType;

	private string _valuePropertyName;

	private Type _valuePropertyType;

	private object _valuePropertyMember;

	private bool _hasValueId;

	private short _valueId;

	private string _memberName;

	private Type _defaultTargetType;

	internal string Value => _value;

	internal Type ValueDeclaringType
	{
		get
		{
			if (_valueDeclaringType == null)
			{
				return base.PropDeclaringType;
			}
			return _valueDeclaringType;
		}
		set
		{
			_valueDeclaringType = value;
		}
	}

	internal string ValuePropertyName
	{
		get
		{
			if (_valuePropertyName == null)
			{
				return base.PropName;
			}
			return _valuePropertyName;
		}
		set
		{
			_valuePropertyName = value;
		}
	}

	internal Type ValuePropertyType
	{
		get
		{
			if (_valuePropertyType == null)
			{
				return base.PropValidType;
			}
			return _valuePropertyType;
		}
		set
		{
			_valuePropertyType = value;
		}
	}

	internal object ValuePropertyMember
	{
		get
		{
			if (_valuePropertyMember == null)
			{
				return base.PropertyMember;
			}
			return _valuePropertyMember;
		}
		set
		{
			_valuePropertyMember = value;
		}
	}

	internal bool HasValueId => _hasValueId;

	internal short ValueId
	{
		get
		{
			return _valueId;
		}
		set
		{
			_valueId = value;
			_hasValueId = true;
		}
	}

	internal string MemberName
	{
		get
		{
			return _memberName;
		}
		set
		{
			_memberName = value;
		}
	}

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

	internal BamlAttributeUsage AttributeUsage => _attributeUsage;

	internal bool ComplexAsSimple => _complexAsSimple;

	internal XamlPropertyNode(int lineNumber, int linePosition, int depth, object propertyMember, string assemblyName, string typeFullName, string propertyName, string value, BamlAttributeUsage attributeUsage, bool complexAsSimple)
		: base(XamlNodeType.Property, lineNumber, linePosition, depth, propertyMember, assemblyName, typeFullName, propertyName)
	{
		_value = value;
		_attributeUsage = attributeUsage;
		_complexAsSimple = complexAsSimple;
	}

	internal XamlPropertyNode(int lineNumber, int linePosition, int depth, object propertyMember, string assemblyName, string typeFullName, string propertyName, string value, BamlAttributeUsage attributeUsage, bool complexAsSimple, bool isDefinitionName)
		: this(lineNumber, linePosition, depth, propertyMember, assemblyName, typeFullName, propertyName, value, attributeUsage, complexAsSimple)
	{
		_isDefinitionName = isDefinitionName;
	}

	internal void SetValue(string value)
	{
		_value = value;
	}
}
