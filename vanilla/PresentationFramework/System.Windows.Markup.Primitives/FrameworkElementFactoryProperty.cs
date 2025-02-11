using System.ComponentModel;

namespace System.Windows.Markup.Primitives;

internal class FrameworkElementFactoryProperty : ElementPropertyBase
{
	private PropertyValue _propertyValue;

	private FrameworkElementFactoryMarkupObject _item;

	private bool _descriptorCalculated;

	private PropertyDescriptor _descriptor;

	public override PropertyDescriptor PropertyDescriptor
	{
		get
		{
			if (!_descriptorCalculated)
			{
				_descriptorCalculated = true;
				if (DependencyProperty.FromName(_propertyValue.Property.Name, _item.ObjectType) == _propertyValue.Property)
				{
					_descriptor = DependencyPropertyDescriptor.FromProperty(_propertyValue.Property, _item.ObjectType);
				}
			}
			return _descriptor;
		}
	}

	public override bool IsAttached
	{
		get
		{
			if (PropertyDescriptor is DependencyPropertyDescriptor dependencyPropertyDescriptor)
			{
				return dependencyPropertyDescriptor.IsAttached;
			}
			return false;
		}
	}

	public override AttributeCollection Attributes
	{
		get
		{
			if (_descriptor != null)
			{
				return _descriptor.Attributes;
			}
			return DependencyPropertyDescriptor.FromProperty(_propertyValue.Property, _item.ObjectType).Attributes;
		}
	}

	public override string Name => _propertyValue.Property.Name;

	public override Type PropertyType => _propertyValue.Property.PropertyType;

	public override DependencyProperty DependencyProperty => _propertyValue.Property;

	public override object Value
	{
		get
		{
			switch (_propertyValue.ValueType)
			{
			case PropertyValueType.Set:
			case PropertyValueType.TemplateBinding:
				return _propertyValue.Value;
			case PropertyValueType.Resource:
				return new DynamicResourceExtension(_propertyValue.Value);
			default:
				return null;
			}
		}
	}

	public FrameworkElementFactoryProperty(PropertyValue propertyValue, FrameworkElementFactoryMarkupObject item)
		: base(item.Manager)
	{
		_propertyValue = propertyValue;
		_item = item;
	}

	protected override IValueSerializerContext GetItemContext()
	{
		return _item.Context;
	}

	protected override Type GetObjectType()
	{
		return _item.ObjectType;
	}
}
