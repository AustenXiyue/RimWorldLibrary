using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using MS.Internal.ComponentModel;

namespace System.ComponentModel;

/// <summary>Provides an extension of <see cref="T:System.ComponentModel.PropertyDescriptor" /> that accounts for the additional property characteristics of a dependency property.</summary>
public sealed class DependencyPropertyDescriptor : PropertyDescriptor
{
	private PropertyDescriptor _property;

	private Type _componentType;

	private DependencyProperty _dp;

	private bool _isAttached;

	private PropertyMetadata _metadata;

	private static Dictionary<object, DependencyPropertyDescriptor> _cache = new Dictionary<object, DependencyPropertyDescriptor>(ReferenceEqualityComparer.Instance);

	private static Dictionary<object, DependencyPropertyDescriptor> _ignorePropertyTypeCache = new Dictionary<object, DependencyPropertyDescriptor>(ReferenceEqualityComparer.Instance);

	/// <summary>Returns the dependency property identifier.</summary>
	/// <returns>The dependency property identifier.</returns>
	public DependencyProperty DependencyProperty => _dp;

	/// <summary>Gets a value that indicates whether the property is registered as an attached property and is being used through an attached usage.</summary>
	/// <returns>true if the property is an attached property; otherwise, false.</returns>
	public bool IsAttached => _isAttached;

	/// <summary>Gets the metadata associated with the dependency property.</summary>
	/// <returns>The dependency property metadata.</returns>
	public PropertyMetadata Metadata => _metadata;

	/// <summary>Gets the type of the component this property is bound to.</summary>
	/// <returns>A <see cref="T:System.Type" /> that represents the type of component this property is bound to. When <see cref="M:System.ComponentModel.DependencyPropertyDescriptor.GetValue(System.Object)" /> or <see cref="M:System.ComponentModel.DependencyPropertyDescriptor.SetValue(System.Object,System.Object)" /> are invoked, the object specified might be an instance of this type.</returns>
	public override Type ComponentType => _componentType;

	/// <summary>Gets a value indicating whether this property is read-only.</summary>
	/// <returns>true if the property is read-only; otherwise, false.</returns>
	public override bool IsReadOnly => Property.IsReadOnly;

	/// <summary>Gets the represented <see cref="T:System.Type" /> of the dependency property.</summary>
	/// <returns>The <see cref="T:System.Type" /> of the dependency property.</returns>
	public override Type PropertyType => _dp.PropertyType;

	/// <summary>Gets the collection of attributes for this member.</summary>
	/// <returns>The <see cref="T:System.ComponentModel.AttributeCollection" /> collection of attributes.</returns>
	public override AttributeCollection Attributes => Property.Attributes;

	/// <summary>Gets the name of the category that the member belongs to, as specified in the <see cref="T:System.ComponentModel.CategoryAttribute" />.</summary>
	/// <returns>The name of the category to which the member belongs. If there is no <see cref="T:System.ComponentModel.CategoryAttribute" />, the category name is set to the default category, Misc.</returns>
	public override string Category => Property.Category;

	/// <summary>Gets the description of the member, as specified in the <see cref="T:System.ComponentModel.DescriptionAttribute" />.</summary>
	/// <returns>The description of the member. If there is no <see cref="T:System.ComponentModel.DescriptionAttribute" />, the property value is set to the default, which is an empty string ("").</returns>
	public override string Description => Property.Description;

	/// <summary>Gets whether this member should be set only at design time, as specified in the <see cref="T:System.ComponentModel.DesignOnlyAttribute" />.</summary>
	/// <returns>true if this member should be set only at design time; false if the member can be set during run time. If there is no <see cref="T:System.ComponentModel.DesignOnlyAttribute" />, the return value is the default, which is false.</returns>
	public override bool DesignTimeOnly => Property.DesignTimeOnly;

	/// <summary>Gets the name that can be displayed in a window, such as a Properties window.</summary>
	/// <returns>The name to display for the property.</returns>
	public override string DisplayName => Property.DisplayName;

	/// <summary>Gets the type converter for this property.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.TypeConverter" /> that is used to convert the <see cref="T:System.Type" /> of this property.</returns>
	public override TypeConverter Converter
	{
		get
		{
			TypeConverter converter = Property.Converter;
			if (converter.GetType().IsPublic)
			{
				return converter;
			}
			return null;
		}
	}

	/// <summary>Gets a value that indicates the value of the <see cref="T:System.ComponentModel.BrowsableAttribute" /> on the property.</summary>
	/// <returns>true if the <see cref="T:System.ComponentModel.BrowsableAttribute" /> was specified on the property; otherwise, false.</returns>
	public override bool IsBrowsable => Property.IsBrowsable;

	/// <summary>Gets a value indicating whether this property should be localized, as specified in the <see cref="T:System.ComponentModel.LocalizableAttribute" />.</summary>
	/// <returns>true if the member is marked with the <see cref="T:System.ComponentModel.LocalizableAttribute" /> constructor of the value true; otherwise, false.</returns>
	public override bool IsLocalizable => Property.IsLocalizable;

	/// <summary>Indicates whether value change notifications for this property may originate from outside the property descriptor, such as from the component itself, or whether notifications will only originate from direct calls made to <see cref="M:System.ComponentModel.DependencyPropertyDescriptor.SetValue(System.Object,System.Object)" />. </summary>
	/// <returns>true if notifications for this property may originate from outside the property descriptor, such as from the component itself. false if notifications will only originate from direct calls made to <see cref="M:System.ComponentModel.DependencyPropertyDescriptor.SetValue(System.Object,System.Object)" />.</returns>
	public override bool SupportsChangeEvents => Property.SupportsChangeEvents;

	/// <summary>Gets or sets a callback that designers use to modify the effective value of a dependency property before the dependency property value is stored in the dependency property engine.</summary>
	/// <returns>A callback that designers use to modify the effective value of a dependency property before the dependency property value is stored in the dependency property engine.</returns>
	public CoerceValueCallback DesignerCoerceValueCallback
	{
		get
		{
			return DependencyProperty.DesignerCoerceValueCallback;
		}
		set
		{
			DependencyProperty.DesignerCoerceValueCallback = value;
		}
	}

	private PropertyDescriptor Property
	{
		get
		{
			if (_property == null)
			{
				_property = TypeDescriptor.GetProperties(_componentType)[Name];
				if (_property == null)
				{
					_property = TypeDescriptor.CreateProperty(_componentType, Name, _dp.PropertyType);
				}
			}
			return _property;
		}
	}

	private DependencyPropertyDescriptor(PropertyDescriptor property, string name, Type componentType, DependencyProperty dp, bool isAttached)
		: base(name, null)
	{
		_property = property;
		_componentType = componentType;
		_dp = dp;
		_isAttached = isAttached;
		_metadata = _dp.GetMetadata(componentType);
	}

	/// <summary>Returns a <see cref="T:System.ComponentModel.DependencyPropertyDescriptor" /> for a provided <see cref="T:System.ComponentModel.PropertyDescriptor" />.</summary>
	/// <returns>If the property described by <paramref name="property" /> is a dependency property, returns a valid <see cref="T:System.ComponentModel.DependencyPropertyDescriptor" />. Otherwise, returns a null<see cref="T:System.ComponentModel.DependencyPropertyDescriptor" />.</returns>
	/// <param name="property">The <see cref="T:System.ComponentModel.PropertyDescriptor" /> to check.</param>
	public static DependencyPropertyDescriptor FromProperty(PropertyDescriptor property)
	{
		if (property == null)
		{
			throw new ArgumentNullException("property");
		}
		bool flag;
		DependencyPropertyDescriptor value;
		lock (_cache)
		{
			flag = _cache.TryGetValue(property, out value);
		}
		if (flag)
		{
			return value;
		}
		DependencyProperty dependencyProperty = null;
		bool isAttached = false;
		if (property is DependencyObjectPropertyDescriptor dependencyObjectPropertyDescriptor)
		{
			dependencyProperty = dependencyObjectPropertyDescriptor.DependencyProperty;
			isAttached = dependencyObjectPropertyDescriptor.IsAttached;
		}
		else if (property.Attributes[typeof(DependencyPropertyAttribute)] is DependencyPropertyAttribute dependencyPropertyAttribute)
		{
			dependencyProperty = dependencyPropertyAttribute.DependencyProperty;
			isAttached = dependencyPropertyAttribute.IsAttached;
		}
		if (dependencyProperty != null)
		{
			value = new DependencyPropertyDescriptor(property, property.Name, property.ComponentType, dependencyProperty, isAttached);
			lock (_cache)
			{
				_cache[property] = value;
			}
		}
		return value;
	}

	internal static DependencyPropertyDescriptor FromProperty(DependencyProperty dependencyProperty, Type ownerType, Type targetType, bool ignorePropertyType)
	{
		if (dependencyProperty == null)
		{
			throw new ArgumentNullException("dependencyProperty");
		}
		if (targetType == null)
		{
			throw new ArgumentNullException("targetType");
		}
		DependencyPropertyDescriptor value = null;
		if (ownerType.GetProperty(dependencyProperty.Name) != null)
		{
			lock (_ignorePropertyTypeCache)
			{
				_ignorePropertyTypeCache.TryGetValue(dependencyProperty, out value);
			}
			if (value == null)
			{
				value = new DependencyPropertyDescriptor(null, dependencyProperty.Name, targetType, dependencyProperty, isAttached: false);
				lock (_ignorePropertyTypeCache)
				{
					_ignorePropertyTypeCache[dependencyProperty] = value;
				}
			}
		}
		else
		{
			if (ownerType.GetMethod("Get" + dependencyProperty.Name) == null && ownerType.GetMethod("Set" + dependencyProperty.Name) == null)
			{
				return null;
			}
			PropertyDescriptor attachedPropertyDescriptor = DependencyObjectProvider.GetAttachedPropertyDescriptor(dependencyProperty, targetType);
			if (attachedPropertyDescriptor != null)
			{
				value = FromProperty(attachedPropertyDescriptor);
			}
		}
		return value;
	}

	/// <summary>Returns a <see cref="T:System.ComponentModel.DependencyPropertyDescriptor" /> for a provided dependency property and target type.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.DependencyPropertyDescriptor" /> for the provided dependency property.</returns>
	/// <param name="dependencyProperty">The identifier for a dependency property.</param>
	/// <param name="targetType">The type of the object where the property is set.</param>
	public static DependencyPropertyDescriptor FromProperty(DependencyProperty dependencyProperty, Type targetType)
	{
		if (dependencyProperty == null)
		{
			throw new ArgumentNullException("dependencyProperty");
		}
		if (targetType == null)
		{
			throw new ArgumentNullException("targetType");
		}
		DependencyPropertyDescriptor value = null;
		DependencyPropertyKind dependencyPropertyKind = DependencyObjectProvider.GetDependencyPropertyKind(dependencyProperty, targetType);
		if (dependencyPropertyKind.IsDirect)
		{
			lock (_cache)
			{
				_cache.TryGetValue(dependencyProperty, out value);
			}
			if (value == null)
			{
				value = new DependencyPropertyDescriptor(null, dependencyProperty.Name, targetType, dependencyProperty, isAttached: false);
				lock (_cache)
				{
					_cache[dependencyProperty] = value;
				}
			}
		}
		else if (!dependencyPropertyKind.IsInternal)
		{
			PropertyDescriptor attachedPropertyDescriptor = DependencyObjectProvider.GetAttachedPropertyDescriptor(dependencyProperty, targetType);
			if (attachedPropertyDescriptor != null)
			{
				value = FromProperty(attachedPropertyDescriptor);
			}
		}
		return value;
	}

	/// <summary>Returns a <see cref="T:System.ComponentModel.DependencyPropertyDescriptor" /> for a provided property name.</summary>
	/// <returns>The requested <see cref="T:System.ComponentModel.DependencyPropertyDescriptor" />.</returns>
	/// <param name="name">The registered name of a dependency property or an attached property.</param>
	/// <param name="ownerType">The <see cref="T:System.Type" /> of the object that owns the property definition.</param>
	/// <param name="targetType">The <see cref="T:System.Type" /> of the object you want to set the property for.</param>
	public static DependencyPropertyDescriptor FromName(string name, Type ownerType, Type targetType)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (ownerType == null)
		{
			throw new ArgumentNullException("ownerType");
		}
		if (targetType == null)
		{
			throw new ArgumentNullException("targetType");
		}
		DependencyProperty dependencyProperty = DependencyProperty.FromName(name, ownerType);
		if (dependencyProperty != null)
		{
			return FromProperty(dependencyProperty, targetType);
		}
		return null;
	}

	/// <summary>Returns a <see cref="T:System.ComponentModel.DependencyPropertyDescriptor" /> for a provided property name.</summary>
	/// <returns>The requested <see cref="T:System.ComponentModel.DependencyPropertyDescriptor" />.</returns>
	/// <param name="name">The registered name of a dependency property or an attached property.</param>
	/// <param name="ownerType">The <see cref="T:System.Type" /> of the object that owns the property definition.</param>
	/// <param name="targetType">The <see cref="T:System.Type" /> of the object you want to set the property for.</param>
	/// <param name="ignorePropertyType">Specifies to ignore the property type.</param>
	public static DependencyPropertyDescriptor FromName(string name, Type ownerType, Type targetType, bool ignorePropertyType)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (ownerType == null)
		{
			throw new ArgumentNullException("ownerType");
		}
		if (targetType == null)
		{
			throw new ArgumentNullException("targetType");
		}
		DependencyProperty dependencyProperty = DependencyProperty.FromName(name, ownerType);
		if (dependencyProperty != null)
		{
			if (ignorePropertyType)
			{
				try
				{
					return FromProperty(dependencyProperty, ownerType, targetType, ignorePropertyType);
				}
				catch (AmbiguousMatchException)
				{
					return FromProperty(dependencyProperty, targetType);
				}
			}
			return FromProperty(dependencyProperty, targetType);
		}
		return null;
	}

	/// <summary>Compares two <see cref="T:System.ComponentModel.DependencyPropertyDescriptor" /> instances for equality.</summary>
	/// <returns>true if the values are equivalent; otherwise, false.</returns>
	/// <param name="obj">The <see cref="T:System.ComponentModel.DependencyPropertyDescriptor" />  to compare with the current instance. </param>
	public override bool Equals(object obj)
	{
		if (obj is DependencyPropertyDescriptor dependencyPropertyDescriptor && dependencyPropertyDescriptor._dp == _dp && dependencyPropertyDescriptor._componentType == _componentType)
		{
			return true;
		}
		return false;
	}

	/// <summary>Returns the hash code for this <see cref="T:System.ComponentModel.DependencyPropertyDescriptor" />.</summary>
	/// <returns>A 32-bit signed integer hash code. </returns>
	public override int GetHashCode()
	{
		return _dp.GetHashCode() ^ _componentType.GetHashCode();
	}

	/// <summary>Converts the value of this instance to its equivalent string representation.</summary>
	/// <returns>Returns the <see cref="P:System.ComponentModel.MemberDescriptor.Name" /> value.</returns>
	public override string ToString()
	{
		return Name;
	}

	/// <summary>Returns whether resetting an object changes its value.</summary>
	/// <returns>true if resetting the component changes its value; otherwise, false.</returns>
	/// <param name="component">The component to test for reset capability. </param>
	public override bool CanResetValue(object component)
	{
		return Property.CanResetValue(component);
	}

	/// <summary>Resturns the current value of the property on a component.</summary>
	/// <returns>The requested value.</returns>
	/// <param name="component">The component instance.</param>
	public override object GetValue(object component)
	{
		return Property.GetValue(component);
	}

	/// <summary>Resets the value for this property of the component to the default value.</summary>
	/// <param name="component">The component with the property value that is to be reset to the default value. </param>
	public override void ResetValue(object component)
	{
		Property.ResetValue(component);
	}

	/// <summary>Sets the value of the component to a different value.</summary>
	/// <param name="component">The component with the property value that is to be set. </param>
	/// <param name="value">The new value.</param>
	public override void SetValue(object component, object value)
	{
		Property.SetValue(component, value);
	}

	/// <summary>Indicates whether the value of this property needs to be persisted by serialization processes.</summary>
	/// <returns>true if the property should be persisted; otherwise, false.</returns>
	/// <param name="component">The component with the property to be examined for persistence.</param>
	public override bool ShouldSerializeValue(object component)
	{
		return Property.ShouldSerializeValue(component);
	}

	/// <summary>Enables other objects to be notified when this property changes. </summary>
	/// <param name="component">The component to add the handler for.</param>
	/// <param name="handler">The delegate to add as a listener.</param>
	public override void AddValueChanged(object component, EventHandler handler)
	{
		Property.AddValueChanged(component, handler);
	}

	/// <summary>Enables other objects to be notified when this property changes.</summary>
	/// <param name="component">The component to add the handler for. </param>
	/// <param name="handler">The delegate to add as a listener. </param>
	public override void RemoveValueChanged(object component, EventHandler handler)
	{
		Property.RemoveValueChanged(component, handler);
	}

	/// <summary>Returns a <see cref="T:System.ComponentModel.PropertyDescriptorCollection" />.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.PropertyDescriptorCollection" /> with the properties that match the specified attributes for the specified component.</returns>
	/// <param name="instance">A component to get the properties for.</param>
	/// <param name="filter">An array of type <see cref="T:System.Attribute" /> to use as a filter. </param>
	public override PropertyDescriptorCollection GetChildProperties(object instance, Attribute[] filter)
	{
		return Property.GetChildProperties(instance, filter);
	}

	/// <summary>Gets an editor of the specified type.</summary>
	/// <returns>An instance of the requested editor type, or null if an editor cannot be found.</returns>
	/// <param name="editorBaseType">The base type of editor, which is used to differentiate between multiple editors that a property supports. </param>
	public override object GetEditor(Type editorBaseType)
	{
		return Property.GetEditor(editorBaseType);
	}

	internal static void ClearCache()
	{
		lock (_cache)
		{
			_cache.Clear();
		}
	}
}
