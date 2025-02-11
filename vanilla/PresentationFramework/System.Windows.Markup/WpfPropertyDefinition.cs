using System.Reflection;

namespace System.Windows.Markup;

internal struct WpfPropertyDefinition
{
	private BamlRecordReader _reader;

	private short _attributeId;

	private BamlAttributeInfoRecord _attributeInfo;

	private DependencyProperty _dependencyProperty;

	public DependencyProperty DependencyProperty => _dependencyProperty;

	public BamlAttributeUsage AttributeUsage
	{
		get
		{
			if (_attributeInfo != null)
			{
				return _attributeInfo.AttributeUsage;
			}
			if (_reader.MapTable != null)
			{
				_reader.MapTable.GetAttributeInfoFromId(_attributeId, out var _, out var _, out var attributeUsage);
				return attributeUsage;
			}
			return BamlAttributeUsage.Default;
		}
	}

	public BamlAttributeInfoRecord AttributeInfo
	{
		get
		{
			if (_attributeInfo == null && _reader.MapTable != null)
			{
				_attributeInfo = _reader.MapTable.GetAttributeInfoFromIdWithOwnerType(_attributeId);
			}
			return _attributeInfo;
		}
	}

	public PropertyInfo PropertyInfo
	{
		get
		{
			if (AttributeInfo == null)
			{
				return null;
			}
			if (_attributeInfo.PropInfo == null)
			{
				Type type = _reader.GetCurrentObjectData().GetType();
				_reader.XamlTypeMapper.UpdateClrPropertyInfo(type, _attributeInfo);
			}
			return _attributeInfo.PropInfo;
		}
	}

	public MethodInfo AttachedPropertyGetter
	{
		get
		{
			if (AttributeInfo == null)
			{
				return null;
			}
			if (_attributeInfo.AttachedPropertyGetter == null)
			{
				_reader.XamlTypeMapper.UpdateAttachedPropertyGetter(_attributeInfo);
			}
			return _attributeInfo.AttachedPropertyGetter;
		}
	}

	public MethodInfo AttachedPropertySetter
	{
		get
		{
			if (AttributeInfo == null)
			{
				return null;
			}
			if (_attributeInfo.AttachedPropertySetter == null)
			{
				_reader.XamlTypeMapper.UpdateAttachedPropertySetter(_attributeInfo);
			}
			return _attributeInfo.AttachedPropertySetter;
		}
	}

	public bool IsInternal
	{
		get
		{
			if (AttributeInfo == null)
			{
				return false;
			}
			return _attributeInfo.IsInternal;
		}
	}

	public Type PropertyType
	{
		get
		{
			if (DependencyProperty != null)
			{
				return DependencyProperty.PropertyType;
			}
			if (PropertyInfo != null)
			{
				return PropertyInfo.PropertyType;
			}
			if (AttachedPropertySetter != null)
			{
				return XamlTypeMapper.GetPropertyType(AttachedPropertySetter);
			}
			return AttachedPropertyGetter.ReturnType;
		}
	}

	public string Name
	{
		get
		{
			if (DependencyProperty != null)
			{
				return DependencyProperty.Name;
			}
			if (PropertyInfo != null)
			{
				return PropertyInfo.Name;
			}
			if (AttachedPropertySetter != null)
			{
				return AttachedPropertySetter.Name.Substring("Set".Length);
			}
			if (AttachedPropertyGetter != null)
			{
				return AttachedPropertyGetter.Name.Substring("Get".Length);
			}
			if (_attributeInfo != null)
			{
				return _attributeInfo.Name;
			}
			return "<unknown>";
		}
	}

	internal object DpOrPiOrMi
	{
		get
		{
			if (DependencyProperty == null)
			{
				if (!(PropertyInfo != null))
				{
					return AttachedPropertySetter;
				}
				return PropertyInfo;
			}
			return DependencyProperty;
		}
	}

	public WpfPropertyDefinition(BamlRecordReader reader, short attributeId, bool targetIsDependencyObject)
	{
		_reader = reader;
		_attributeId = attributeId;
		_dependencyProperty = null;
		_attributeInfo = null;
		if (_reader.MapTable != null && targetIsDependencyObject)
		{
			_dependencyProperty = _reader.MapTable.GetDependencyProperty(_attributeId);
		}
	}
}
