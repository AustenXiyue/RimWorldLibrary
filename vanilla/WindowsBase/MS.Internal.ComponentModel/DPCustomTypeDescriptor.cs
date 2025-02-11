using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Markup;

namespace MS.Internal.ComponentModel;

internal struct DPCustomTypeDescriptor : ICustomTypeDescriptor
{
	private ICustomTypeDescriptor _parent;

	private Type _objectType;

	private object _instance;

	private static Dictionary<PropertyDescriptor, DependencyObjectPropertyDescriptor> _propertyMap = new Dictionary<PropertyDescriptor, DependencyObjectPropertyDescriptor>(new PropertyDescriptorComparer());

	private static Hashtable _typeProperties = new Hashtable();

	private const PropertyFilterOptions _anySet = PropertyFilterOptions.SetValues | PropertyFilterOptions.UnsetValues;

	private const PropertyFilterOptions _anyValid = PropertyFilterOptions.Valid;

	internal DPCustomTypeDescriptor(ICustomTypeDescriptor parent, Type objectType, object instance)
	{
		_parent = parent;
		_objectType = objectType;
		_instance = instance;
	}

	public string GetComponentName()
	{
		if (_instance != null && GetAttributes()[typeof(RuntimeNamePropertyAttribute)] is RuntimeNamePropertyAttribute { Name: not null } runtimeNamePropertyAttribute)
		{
			PropertyDescriptor propertyDescriptor = GetProperties()[runtimeNamePropertyAttribute.Name];
			if (propertyDescriptor != null)
			{
				return propertyDescriptor.GetValue(_instance) as string;
			}
		}
		return _parent.GetComponentName();
	}

	public PropertyDescriptorCollection GetProperties()
	{
		return GetProperties(null);
	}

	public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
	{
		PropertyFilterOptions propertyFilterOptions = PropertyFilterOptions.SetValues | PropertyFilterOptions.Valid;
		if (attributes != null)
		{
			for (int i = 0; i < attributes.Length; i++)
			{
				if (attributes[i] is PropertyFilterAttribute propertyFilterAttribute)
				{
					propertyFilterOptions = propertyFilterAttribute.Filter;
					break;
				}
			}
		}
		DependencyObject component;
		switch (propertyFilterOptions)
		{
		case PropertyFilterOptions.None:
		case PropertyFilterOptions.Invalid:
			return PropertyDescriptorCollection.Empty;
		case PropertyFilterOptions.SetValues:
			if (_instance == null)
			{
				return PropertyDescriptorCollection.Empty;
			}
			component = (DependencyObject)TypeDescriptor.GetAssociation(_objectType, _instance);
			break;
		default:
			component = null;
			break;
		}
		PropertyDescriptorCollection propertyDescriptorCollection = (PropertyDescriptorCollection)_typeProperties[_objectType];
		if (propertyDescriptorCollection == null)
		{
			propertyDescriptorCollection = CreateProperties();
			lock (_typeProperties)
			{
				_typeProperties[_objectType] = propertyDescriptorCollection;
			}
		}
		if ((propertyFilterOptions & (PropertyFilterOptions.SetValues | PropertyFilterOptions.UnsetValues)) == (PropertyFilterOptions.SetValues | PropertyFilterOptions.UnsetValues) || (propertyFilterOptions & PropertyFilterOptions.Valid) == PropertyFilterOptions.Valid)
		{
			return propertyDescriptorCollection;
		}
		List<PropertyDescriptor> list = null;
		int count = propertyDescriptorCollection.Count;
		for (int j = 0; j < count; j++)
		{
			PropertyDescriptor propertyDescriptor = propertyDescriptorCollection[j];
			if (!(propertyDescriptor.ShouldSerializeValue(component) ^ ((propertyFilterOptions & (PropertyFilterOptions.SetValues | PropertyFilterOptions.UnsetValues)) == PropertyFilterOptions.UnsetValues)))
			{
				if (list == null)
				{
					list = new List<PropertyDescriptor>(count);
					for (int k = 0; k < j; k++)
					{
						list.Add(propertyDescriptorCollection[k]);
					}
				}
			}
			else
			{
				list?.Add(propertyDescriptor);
			}
		}
		if (list != null)
		{
			propertyDescriptorCollection = new PropertyDescriptorCollection(list.ToArray(), readOnly: true);
		}
		return propertyDescriptorCollection;
	}

	public AttributeCollection GetAttributes()
	{
		return _parent.GetAttributes();
	}

	public string GetClassName()
	{
		return _parent.GetClassName();
	}

	public TypeConverter GetConverter()
	{
		TypeConverter converter = _parent.GetConverter();
		if (converter.GetType().IsPublic)
		{
			return converter;
		}
		return new TypeConverter();
	}

	public EventDescriptor GetDefaultEvent()
	{
		return _parent.GetDefaultEvent();
	}

	public PropertyDescriptor GetDefaultProperty()
	{
		return _parent.GetDefaultProperty();
	}

	public object GetEditor(Type editorBaseType)
	{
		return _parent.GetEditor(editorBaseType);
	}

	public EventDescriptorCollection GetEvents()
	{
		return _parent.GetEvents();
	}

	public EventDescriptorCollection GetEvents(Attribute[] attributes)
	{
		return _parent.GetEvents(attributes);
	}

	public object GetPropertyOwner(PropertyDescriptor property)
	{
		return _parent.GetPropertyOwner(property);
	}

	internal static void ClearCache()
	{
		lock (_propertyMap)
		{
			_propertyMap.Clear();
		}
		lock (_typeProperties)
		{
			_typeProperties.Clear();
		}
	}

	private PropertyDescriptorCollection CreateProperties()
	{
		PropertyDescriptorCollection properties = _parent.GetProperties();
		List<PropertyDescriptor> list = new List<PropertyDescriptor>(properties.Count);
		for (int i = 0; i < properties.Count; i++)
		{
			PropertyDescriptor propertyDescriptor = properties[i];
			DependencyProperty dependencyProperty = null;
			bool flag;
			DependencyObjectPropertyDescriptor value;
			lock (_propertyMap)
			{
				flag = _propertyMap.TryGetValue(propertyDescriptor, out value);
			}
			if (flag && value != null)
			{
				dependencyProperty = DependencyProperty.FromName(propertyDescriptor.Name, _objectType);
				if (dependencyProperty != value.DependencyProperty)
				{
					value = null;
				}
				else if (value.Metadata != dependencyProperty.GetMetadata(_objectType))
				{
					value = null;
				}
			}
			if (value == null)
			{
				if (dependencyProperty != null || typeof(DependencyObject).IsAssignableFrom(propertyDescriptor.ComponentType))
				{
					if (dependencyProperty == null)
					{
						dependencyProperty = DependencyProperty.FromName(propertyDescriptor.Name, _objectType);
					}
					if (dependencyProperty != null)
					{
						value = new DependencyObjectPropertyDescriptor(propertyDescriptor, dependencyProperty, _objectType);
					}
				}
				if (!flag)
				{
					lock (_propertyMap)
					{
						_propertyMap[propertyDescriptor] = value;
					}
				}
			}
			if (value != null)
			{
				propertyDescriptor = value;
			}
			list.Add(propertyDescriptor);
		}
		return new PropertyDescriptorCollection(list.ToArray(), readOnly: true);
	}
}
