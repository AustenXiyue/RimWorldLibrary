using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using MS.Internal.WindowsBase;

namespace MS.Internal.ComponentModel;

internal sealed class DependencyObjectPropertyDescriptor : PropertyDescriptor
{
	private static Binder _dpBinder = new AttachedPropertyMethodSelector();

	private static object _nullMethodSentinel = new object();

	private static Hashtable _getMethodCache = new Hashtable();

	private static Hashtable _setMethodCache = new Hashtable();

	private static object _attributeSyncLock = new object();

	private PropertyDescriptor _property;

	private DependencyProperty _dp;

	private Type _componentType;

	private PropertyMetadata _metadata;

	private bool _queriedShouldSerializeMethod;

	private bool _queriedResetMethod;

	private Dictionary<DependencyObject, PropertyChangeTracker> _trackers;

	private MethodInfo _shouldSerializeMethod;

	private MethodInfo _resetMethod;

	private static Type[] _dpType;

	private static Type _boolType;

	private static Type _attributeType;

	private static Type _attachedPropertyBrowsableType;

	public override AttributeCollection Attributes
	{
		get
		{
			AttributeCollection attributes = base.Attributes;
			if (attributes == null)
			{
				lock (_attributeSyncLock)
				{
					attributes = base.Attributes;
				}
			}
			return attributes;
		}
	}

	public override Type ComponentType => _componentType;

	public override bool IsReadOnly
	{
		get
		{
			bool flag = _dp.ReadOnly;
			if (!flag)
			{
				flag = Attributes.Contains(ReadOnlyAttribute.Yes);
			}
			return flag;
		}
	}

	public override Type PropertyType => _dp.PropertyType;

	public override bool SupportsChangeEvents => true;

	internal DependencyProperty DependencyProperty => _dp;

	internal bool IsAttached => _property == null;

	internal PropertyMetadata Metadata => _metadata;

	internal static Type AttachedPropertyBrowsableAttributeType
	{
		get
		{
			Type type = _attachedPropertyBrowsableType;
			if (type == null)
			{
				type = (_attachedPropertyBrowsableType = TypeDescriptor.GetReflectionType(typeof(AttachedPropertyBrowsableAttribute)));
			}
			return type;
		}
	}

	private static Type AttributeType
	{
		get
		{
			Type type = _attributeType;
			if (type == null)
			{
				type = (_attributeType = TypeDescriptor.GetReflectionType(typeof(Attribute)));
			}
			return type;
		}
	}

	private static Type BoolType
	{
		get
		{
			Type type = _boolType;
			if (type == null)
			{
				type = (_boolType = TypeDescriptor.GetReflectionType(typeof(bool)));
			}
			return type;
		}
	}

	private static Type[] DpType
	{
		get
		{
			Type[] array = _dpType;
			if (array == null)
			{
				array = (_dpType = new Type[1] { TypeDescriptor.GetReflectionType(typeof(DependencyObject)) });
			}
			return array;
		}
	}

	internal DependencyObjectPropertyDescriptor(PropertyDescriptor property, DependencyProperty dp, Type objectType)
		: base(dp.Name, null)
	{
		_property = property;
		_dp = dp;
		_componentType = property.ComponentType;
		_metadata = _dp.GetMetadata(objectType);
	}

	internal DependencyObjectPropertyDescriptor(DependencyProperty dp, Type ownerType)
		: base(dp.OwnerType.Name + "." + dp.Name, null)
	{
		_dp = dp;
		_componentType = ownerType;
		_metadata = _dp.GetMetadata(ownerType);
	}

	public override bool CanResetValue(object component)
	{
		return ShouldSerializeValue(component);
	}

	public override object GetValue(object component)
	{
		return FromObj(component).GetValue(_dp);
	}

	public override void ResetValue(object component)
	{
		if (!_queriedResetMethod)
		{
			_resetMethod = GetSpecialMethod("Reset");
			_queriedResetMethod = true;
		}
		DependencyObject dependencyObject = FromObj(component);
		if (_resetMethod != null)
		{
			if (_property == null)
			{
				_resetMethod.Invoke(null, new object[1] { dependencyObject });
			}
			else
			{
				_resetMethod.Invoke(dependencyObject, null);
			}
		}
		else
		{
			dependencyObject.ClearValue(_dp);
		}
	}

	public override void SetValue(object component, object value)
	{
		FromObj(component).SetValue(_dp, value);
	}

	public override bool ShouldSerializeValue(object component)
	{
		DependencyObject dependencyObject = FromObj(component);
		bool flag = dependencyObject.ShouldSerializeProperty(_dp);
		if (flag)
		{
			if (!_queriedShouldSerializeMethod)
			{
				MethodInfo specialMethod = GetSpecialMethod("ShouldSerialize");
				if (specialMethod != null && specialMethod.ReturnType == BoolType)
				{
					_shouldSerializeMethod = specialMethod;
				}
				_queriedShouldSerializeMethod = true;
			}
			if (_shouldSerializeMethod != null)
			{
				flag = ((_property != null) ? ((bool)_shouldSerializeMethod.Invoke(dependencyObject, null)) : ((bool)_shouldSerializeMethod.Invoke(null, new object[1] { dependencyObject })));
			}
		}
		return flag;
	}

	public override void AddValueChanged(object component, EventHandler handler)
	{
		DependencyObject dependencyObject = FromObj(component);
		if (_trackers == null)
		{
			_trackers = new Dictionary<DependencyObject, PropertyChangeTracker>();
		}
		if (!_trackers.TryGetValue(dependencyObject, out var value))
		{
			value = new PropertyChangeTracker(dependencyObject, _dp);
			_trackers.Add(dependencyObject, value);
		}
		PropertyChangeTracker propertyChangeTracker = value;
		propertyChangeTracker.Changed = (EventHandler)Delegate.Combine(propertyChangeTracker.Changed, handler);
	}

	public override void RemoveValueChanged(object component, EventHandler handler)
	{
		if (_trackers == null)
		{
			return;
		}
		DependencyObject key = FromObj(component);
		if (_trackers.TryGetValue(key, out var value))
		{
			PropertyChangeTracker propertyChangeTracker = value;
			propertyChangeTracker.Changed = (EventHandler)Delegate.Remove(propertyChangeTracker.Changed, handler);
			if (value.CanClose)
			{
				value.Close();
				_trackers.Remove(key);
			}
		}
	}

	internal static void ClearCache()
	{
		lock (_getMethodCache)
		{
			_getMethodCache.Clear();
		}
		lock (_setMethodCache)
		{
			_setMethodCache.Clear();
		}
		_dpType = null;
		_boolType = null;
		_attributeType = null;
		_attachedPropertyBrowsableType = null;
	}

	internal static MethodInfo GetAttachedPropertyMethod(DependencyProperty dp)
	{
		Type reflectionType = TypeDescriptor.GetReflectionType(dp.OwnerType);
		object? obj = _getMethodCache[dp];
		MethodInfo methodInfo = obj as MethodInfo;
		if (obj == null || (methodInfo != null && (object)methodInfo.DeclaringType != reflectionType))
		{
			BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public;
			string name = "Get" + dp.Name;
			methodInfo = reflectionType.GetMethod(name, bindingAttr, _dpBinder, DpType, null);
			lock (_getMethodCache)
			{
				_getMethodCache[dp] = ((methodInfo == null) ? _nullMethodSentinel : methodInfo);
			}
		}
		return methodInfo;
	}

	protected override AttributeCollection CreateAttributeCollection()
	{
		MergeAttributes();
		return base.CreateAttributeCollection();
	}

	private static DependencyObject FromObj(object value)
	{
		return (DependencyObject)TypeDescriptor.GetAssociation(typeof(DependencyObject), value);
	}

	private AttributeCollection GetAttachedPropertyAttributes()
	{
		MethodInfo attachedPropertyMethod = GetAttachedPropertyMethod(_dp);
		if (attachedPropertyMethod != null)
		{
			Type attributeType = AttributeType;
			Attribute[] array = (Attribute[])attachedPropertyMethod.GetCustomAttributes(attributeType, inherit: true);
			Attribute[] array2 = (Attribute[])TypeDescriptor.GetReflectionType(_dp.PropertyType).GetCustomAttributes(attributeType, inherit: true);
			if (array2 != null && array2.Length != 0)
			{
				Attribute[] array3 = new Attribute[array.Length + array2.Length];
				Array.Copy(array, array3, array.Length);
				Array.Copy(array2, 0, array3, array.Length, array2.Length);
				array = array3;
			}
			Attribute[] array4 = null;
			Attribute[] array5 = array;
			for (int i = 0; i < array5.Length; i++)
			{
				if (!(array5[i] is AttributeProviderAttribute attributeProviderAttribute))
				{
					continue;
				}
				Type type = Type.GetType(attributeProviderAttribute.TypeName);
				if (!(type != null))
				{
					continue;
				}
				Attribute[] array6 = null;
				if (!string.IsNullOrEmpty(attributeProviderAttribute.PropertyName))
				{
					MemberInfo[] member = type.GetMember(attributeProviderAttribute.PropertyName);
					if (member.Length != 0 && member[0] != null)
					{
						array6 = (Attribute[])member[0].GetCustomAttributes(typeof(Attribute), inherit: true);
					}
				}
				else
				{
					array6 = (Attribute[])type.GetCustomAttributes(typeof(Attribute), inherit: true);
				}
				if (array6 != null)
				{
					if (array4 == null)
					{
						array4 = array6;
						continue;
					}
					Attribute[] array7 = new Attribute[array4.Length + array6.Length];
					array4.CopyTo(array7, 0);
					array6.CopyTo(array7, array4.Length);
					array4 = array7;
				}
			}
			if (array4 != null)
			{
				Attribute[] array8 = new Attribute[array4.Length + array.Length];
				array.CopyTo(array8, 0);
				array4.CopyTo(array8, array.Length);
				array = array8;
			}
			return new AttributeCollection(array);
		}
		return AttributeCollection.Empty;
	}

	private static MethodInfo GetAttachedPropertySetMethod(DependencyProperty dp)
	{
		Type reflectionType = TypeDescriptor.GetReflectionType(dp.OwnerType);
		object? obj = _setMethodCache[dp];
		MethodInfo methodInfo = obj as MethodInfo;
		if (obj == null || (methodInfo != null && (object)methodInfo.DeclaringType != reflectionType))
		{
			BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public;
			string name = "Set" + dp.Name;
			Type[] types = new Type[2]
			{
				DpType[0],
				TypeDescriptor.GetReflectionType(dp.PropertyType)
			};
			methodInfo = reflectionType.GetMethod(name, bindingAttr, _dpBinder, types, null);
			lock (_setMethodCache)
			{
				_setMethodCache[dp] = ((methodInfo == null) ? _nullMethodSentinel : methodInfo);
			}
		}
		return methodInfo;
	}

	private MethodInfo GetSpecialMethod(string methodPrefix)
	{
		BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic;
		Type[] types;
		Type reflectionType;
		if (_property == null)
		{
			types = DpType;
			bindingFlags |= BindingFlags.Static;
			reflectionType = TypeDescriptor.GetReflectionType(_dp.OwnerType);
		}
		else
		{
			types = Type.EmptyTypes;
			bindingFlags |= BindingFlags.Instance;
			reflectionType = TypeDescriptor.GetReflectionType(_property.ComponentType);
		}
		string name = methodPrefix + _dp.Name;
		MethodInfo method = reflectionType.GetMethod(name, bindingFlags, _dpBinder, types, null);
		if (method != null && !method.IsPublic)
		{
			throw new InvalidOperationException(SR.Format(SR.SpecialMethodMustBePublic, method.Name));
		}
		return method;
	}

	private void MergeAttributes()
	{
		AttributeCollection attributeCollection = ((_property == null) ? GetAttachedPropertyAttributes() : _property.Attributes);
		List<Attribute> list = new List<Attribute>(attributeCollection.Count + 1);
		bool flag = false;
		foreach (Attribute item2 in attributeCollection)
		{
			Attribute attribute2 = item2;
			if (item2 is DefaultValueAttribute)
			{
				attribute2 = null;
			}
			else if (item2 is ReadOnlyAttribute readOnlyAttribute)
			{
				flag = readOnlyAttribute.IsReadOnly;
				attribute2 = null;
			}
			if (attribute2 != null)
			{
				list.Add(attribute2);
			}
		}
		flag |= _dp.ReadOnly;
		if (_property == null && !flag && GetAttachedPropertySetMethod(_dp) == null)
		{
			flag = true;
		}
		DependencyPropertyAttribute item = new DependencyPropertyAttribute(_dp, _property == null);
		list.Add(item);
		if (_metadata.DefaultValue != DependencyProperty.UnsetValue)
		{
			list.Add(new DefaultValueAttribute(_metadata.DefaultValue));
		}
		if (flag)
		{
			list.Add(new ReadOnlyAttribute(isReadOnly: true));
		}
		Attribute[] array = list.ToArray();
		for (int i = 0; i < array.Length / 2; i++)
		{
			int num = array.Length - i - 1;
			Attribute attribute3 = array[i];
			array[i] = array[num];
			array[num] = attribute3;
		}
		AttributeArray = array;
	}
}
