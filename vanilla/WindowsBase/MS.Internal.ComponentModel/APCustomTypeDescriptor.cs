using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace MS.Internal.ComponentModel;

internal struct APCustomTypeDescriptor : ICustomTypeDescriptor
{
	private ICustomTypeDescriptor _parent;

	private DependencyObject _instance;

	private static object _syncLock = new object();

	private static int _dpCacheCount = 0;

	private static DependencyProperty[] _dpCacheArray;

	internal APCustomTypeDescriptor(ICustomTypeDescriptor parent, object instance)
	{
		_parent = parent;
		_instance = FromObj(instance);
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
		if (propertyFilterOptions == PropertyFilterOptions.None)
		{
			return PropertyDescriptorCollection.Empty;
		}
		DependencyProperty[] registeredProperties = GetRegisteredProperties();
		Type type = _instance.GetType();
		List<PropertyDescriptor> list;
		if (propertyFilterOptions == PropertyFilterOptions.SetValues)
		{
			LocalValueEnumerator localValueEnumerator = _instance.GetLocalValueEnumerator();
			list = new List<PropertyDescriptor>(localValueEnumerator.Count);
			while (localValueEnumerator.MoveNext())
			{
				DependencyProperty property = localValueEnumerator.Current.Property;
				DependencyPropertyKind dependencyPropertyKind = DependencyObjectProvider.GetDependencyPropertyKind(property, type);
				if (!dependencyPropertyKind.IsDirect && !dependencyPropertyKind.IsInternal)
				{
					DependencyObjectPropertyDescriptor attachedPropertyDescriptor = DependencyObjectProvider.GetAttachedPropertyDescriptor(property, type);
					list.Add(attachedPropertyDescriptor);
				}
			}
		}
		else
		{
			list = new List<PropertyDescriptor>(registeredProperties.Length);
			DependencyProperty[] array = registeredProperties;
			foreach (DependencyProperty dp in array)
			{
				bool flag = false;
				DependencyPropertyKind dependencyPropertyKind2 = DependencyObjectProvider.GetDependencyPropertyKind(dp, type);
				if (dependencyPropertyKind2.IsAttached)
				{
					PropertyFilterOptions propertyFilterOptions2 = PropertyFilterOptions.SetValues | PropertyFilterOptions.UnsetValues;
					PropertyFilterOptions propertyFilterOptions3 = PropertyFilterOptions.Invalid | PropertyFilterOptions.Valid;
					if ((propertyFilterOptions & propertyFilterOptions2) == propertyFilterOptions2 || (propertyFilterOptions & propertyFilterOptions3) == propertyFilterOptions3)
					{
						flag = true;
					}
					if (!flag && (propertyFilterOptions & propertyFilterOptions3) != 0)
					{
						flag = CanAttachProperty(dp, _instance) ^ ((propertyFilterOptions & propertyFilterOptions3) == PropertyFilterOptions.Invalid);
					}
					if (!flag && (propertyFilterOptions & propertyFilterOptions2) != 0)
					{
						flag = _instance.ContainsValue(dp) ^ ((propertyFilterOptions & propertyFilterOptions2) == PropertyFilterOptions.UnsetValues);
					}
				}
				else if ((propertyFilterOptions & PropertyFilterOptions.SetValues) != 0 && _instance.ContainsValue(dp) && !dependencyPropertyKind2.IsDirect && !dependencyPropertyKind2.IsInternal)
				{
					flag = true;
				}
				if (flag)
				{
					DependencyObjectPropertyDescriptor attachedPropertyDescriptor2 = DependencyObjectProvider.GetAttachedPropertyDescriptor(dp, type);
					list.Add(attachedPropertyDescriptor2);
				}
			}
		}
		return new PropertyDescriptorCollection(list.ToArray(), readOnly: true);
	}

	public AttributeCollection GetAttributes()
	{
		return _parent.GetAttributes();
	}

	public string GetClassName()
	{
		return _parent.GetClassName();
	}

	public string GetComponentName()
	{
		return _parent.GetComponentName();
	}

	public TypeConverter GetConverter()
	{
		TypeConverter converter = _parent.GetConverter();
		if (converter.GetType().IsPublic)
		{
			return converter;
		}
		return null;
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

	private bool CanAttachProperty(DependencyProperty dp, DependencyObject instance)
	{
		return DependencyObjectProvider.GetAttachInfo(dp).CanAttach(instance);
	}

	private static DependencyObject FromObj(object value)
	{
		return (DependencyObject)TypeDescriptor.GetAssociation(typeof(DependencyObject), value);
	}

	private DependencyProperty[] GetRegisteredProperties()
	{
		lock (_syncLock)
		{
			int dpCacheCount = _dpCacheCount;
			int registeredPropertyCount = DependencyProperty.RegisteredPropertyCount;
			if (_dpCacheArray == null || dpCacheCount != registeredPropertyCount)
			{
				List<DependencyProperty> list = new List<DependencyProperty>(registeredPropertyCount);
				lock (DependencyProperty.Synchronized)
				{
					foreach (DependencyProperty registeredProperty in DependencyProperty.RegisteredProperties)
					{
						list.Add(registeredProperty);
					}
					_dpCacheCount = DependencyProperty.RegisteredPropertyCount;
					_dpCacheArray = list.ToArray();
				}
			}
			return _dpCacheArray;
		}
	}
}
