using System.Collections;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.ComponentModel;

/// <summary>Provides an abstraction of a property on a class.</summary>
[ComVisible(true)]
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
public abstract class PropertyDescriptor : MemberDescriptor
{
	private TypeConverter converter;

	private Hashtable valueChangedHandlers;

	private object[] editors;

	private Type[] editorTypes;

	private int editorCount;

	/// <summary>When overridden in a derived class, gets the type of the component this property is bound to.</summary>
	/// <returns>A <see cref="T:System.Type" /> that represents the type of component this property is bound to. When the <see cref="M:System.ComponentModel.PropertyDescriptor.GetValue(System.Object)" /> or <see cref="M:System.ComponentModel.PropertyDescriptor.SetValue(System.Object,System.Object)" /> methods are invoked, the object specified might be an instance of this type.</returns>
	public abstract Type ComponentType { get; }

	/// <summary>Gets the type converter for this property.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.TypeConverter" /> that is used to convert the <see cref="T:System.Type" /> of this property.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public virtual TypeConverter Converter
	{
		get
		{
			AttributeCollection attributeCollection = Attributes;
			if (converter == null)
			{
				TypeConverterAttribute typeConverterAttribute = (TypeConverterAttribute)attributeCollection[typeof(TypeConverterAttribute)];
				if (typeConverterAttribute.ConverterTypeName != null && typeConverterAttribute.ConverterTypeName.Length > 0)
				{
					Type typeFromName = GetTypeFromName(typeConverterAttribute.ConverterTypeName);
					if (typeFromName != null && typeof(TypeConverter).IsAssignableFrom(typeFromName))
					{
						converter = (TypeConverter)CreateInstance(typeFromName);
					}
				}
				if (converter == null)
				{
					converter = TypeDescriptor.GetConverter(PropertyType);
				}
			}
			return converter;
		}
	}

	/// <summary>Gets a value indicating whether this property should be localized, as specified in the <see cref="T:System.ComponentModel.LocalizableAttribute" />.</summary>
	/// <returns>true if the member is marked with the <see cref="T:System.ComponentModel.LocalizableAttribute" /> set to true; otherwise, false.</returns>
	public virtual bool IsLocalizable => LocalizableAttribute.Yes.Equals(Attributes[typeof(LocalizableAttribute)]);

	/// <summary>When overridden in a derived class, gets a value indicating whether this property is read-only.</summary>
	/// <returns>true if the property is read-only; otherwise, false.</returns>
	public abstract bool IsReadOnly { get; }

	/// <summary>Gets a value indicating whether this property should be serialized, as specified in the <see cref="T:System.ComponentModel.DesignerSerializationVisibilityAttribute" />.</summary>
	/// <returns>One of the <see cref="T:System.ComponentModel.DesignerSerializationVisibility" /> enumeration values that specifies whether this property should be serialized.</returns>
	public DesignerSerializationVisibility SerializationVisibility => ((DesignerSerializationVisibilityAttribute)Attributes[typeof(DesignerSerializationVisibilityAttribute)]).Visibility;

	/// <summary>When overridden in a derived class, gets the type of the property.</summary>
	/// <returns>A <see cref="T:System.Type" /> that represents the type of the property.</returns>
	public abstract Type PropertyType { get; }

	/// <summary>Gets a value indicating whether value change notifications for this property may originate from outside the property descriptor.</summary>
	/// <returns>true if value change notifications may originate from outside the property descriptor; otherwise, false.</returns>
	public virtual bool SupportsChangeEvents => false;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.PropertyDescriptor" /> class with the specified name and attributes.</summary>
	/// <param name="name">The name of the property. </param>
	/// <param name="attrs">An array of type <see cref="T:System.Attribute" /> that contains the property attributes. </param>
	protected PropertyDescriptor(string name, Attribute[] attrs)
		: base(name, attrs)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.PropertyDescriptor" /> class with the name and attributes in the specified <see cref="T:System.ComponentModel.MemberDescriptor" />.</summary>
	/// <param name="descr">A <see cref="T:System.ComponentModel.MemberDescriptor" /> that contains the name of the property and its attributes. </param>
	protected PropertyDescriptor(MemberDescriptor descr)
		: base(descr)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.PropertyDescriptor" /> class with the name in the specified <see cref="T:System.ComponentModel.MemberDescriptor" /> and the attributes in both the <see cref="T:System.ComponentModel.MemberDescriptor" /> and the <see cref="T:System.Attribute" /> array.</summary>
	/// <param name="descr">A <see cref="T:System.ComponentModel.MemberDescriptor" /> containing the name of the member and its attributes. </param>
	/// <param name="attrs">An <see cref="T:System.Attribute" /> array containing the attributes you want to associate with the property. </param>
	protected PropertyDescriptor(MemberDescriptor descr, Attribute[] attrs)
		: base(descr, attrs)
	{
	}

	/// <summary>Enables other objects to be notified when this property changes.</summary>
	/// <param name="component">The component to add the handler for. </param>
	/// <param name="handler">The delegate to add as a listener. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="component" /> or <paramref name="handler" /> is null.</exception>
	public virtual void AddValueChanged(object component, EventHandler handler)
	{
		if (component == null)
		{
			throw new ArgumentNullException("component");
		}
		if (handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		if (valueChangedHandlers == null)
		{
			valueChangedHandlers = new Hashtable();
		}
		EventHandler a = (EventHandler)valueChangedHandlers[component];
		valueChangedHandlers[component] = Delegate.Combine(a, handler);
	}

	/// <summary>When overridden in a derived class, returns whether resetting an object changes its value.</summary>
	/// <returns>true if resetting the component changes its value; otherwise, false.</returns>
	/// <param name="component">The component to test for reset capability. </param>
	public abstract bool CanResetValue(object component);

	/// <summary>Compares this to another object to see if they are equivalent.</summary>
	/// <returns>true if the values are equivalent; otherwise, false.</returns>
	/// <param name="obj">The object to compare to this <see cref="T:System.ComponentModel.PropertyDescriptor" />. </param>
	public override bool Equals(object obj)
	{
		try
		{
			if (obj == this)
			{
				return true;
			}
			if (obj == null)
			{
				return false;
			}
			if (obj is PropertyDescriptor propertyDescriptor && propertyDescriptor.NameHashCode == NameHashCode && propertyDescriptor.PropertyType == PropertyType && propertyDescriptor.Name.Equals(Name))
			{
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	/// <summary>Creates an instance of the specified type.</summary>
	/// <returns>A new instance of the type.</returns>
	/// <param name="type">A <see cref="T:System.Type" /> that represents the type to create. </param>
	protected object CreateInstance(Type type)
	{
		Type[] array = new Type[1] { typeof(Type) };
		if (type.GetConstructor(array) != null)
		{
			return TypeDescriptor.CreateInstance(null, type, array, new object[1] { PropertyType });
		}
		return TypeDescriptor.CreateInstance(null, type, null, null);
	}

	/// <summary>Adds the attributes of the <see cref="T:System.ComponentModel.PropertyDescriptor" /> to the specified list of attributes in the parent class.</summary>
	/// <param name="attributeList">An <see cref="T:System.Collections.IList" /> that lists the attributes in the parent class. Initially, this is empty.</param>
	protected override void FillAttributes(IList attributeList)
	{
		converter = null;
		editors = null;
		editorTypes = null;
		editorCount = 0;
		base.FillAttributes(attributeList);
	}

	/// <summary>Returns the default <see cref="T:System.ComponentModel.PropertyDescriptorCollection" />.</summary>
	/// <returns>A collection of property descriptor.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public PropertyDescriptorCollection GetChildProperties()
	{
		return GetChildProperties(null, null);
	}

	/// <summary>Returns a <see cref="T:System.ComponentModel.PropertyDescriptorCollection" /> using a specified array of attributes as a filter.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.PropertyDescriptorCollection" /> with the properties that match the specified attributes.</returns>
	/// <param name="filter">An array of type <see cref="T:System.Attribute" /> to use as a filter. </param>
	public PropertyDescriptorCollection GetChildProperties(Attribute[] filter)
	{
		return GetChildProperties(null, filter);
	}

	/// <summary>Returns a <see cref="T:System.ComponentModel.PropertyDescriptorCollection" /> for a given object.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.PropertyDescriptorCollection" /> with the properties for the specified component.</returns>
	/// <param name="instance">A component to get the properties for. </param>
	public PropertyDescriptorCollection GetChildProperties(object instance)
	{
		return GetChildProperties(instance, null);
	}

	/// <summary>Returns a <see cref="T:System.ComponentModel.PropertyDescriptorCollection" /> for a given object using a specified array of attributes as a filter.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.PropertyDescriptorCollection" /> with the properties that match the specified attributes for the specified component.</returns>
	/// <param name="instance">A component to get the properties for. </param>
	/// <param name="filter">An array of type <see cref="T:System.Attribute" /> to use as a filter. </param>
	public virtual PropertyDescriptorCollection GetChildProperties(object instance, Attribute[] filter)
	{
		if (instance == null)
		{
			return TypeDescriptor.GetProperties(PropertyType, filter);
		}
		return TypeDescriptor.GetProperties(instance, filter);
	}

	/// <summary>Gets an editor of the specified type.</summary>
	/// <returns>An instance of the requested editor type, or null if an editor cannot be found.</returns>
	/// <param name="editorBaseType">The base type of editor, which is used to differentiate between multiple editors that a property supports. </param>
	public virtual object GetEditor(Type editorBaseType)
	{
		object obj = null;
		AttributeCollection attributeCollection = Attributes;
		if (editorTypes != null)
		{
			for (int i = 0; i < editorCount; i++)
			{
				if (editorTypes[i] == editorBaseType)
				{
					return editors[i];
				}
			}
		}
		if (obj == null)
		{
			for (int j = 0; j < attributeCollection.Count; j++)
			{
				if (!(attributeCollection[j] is EditorAttribute editorAttribute))
				{
					continue;
				}
				Type typeFromName = GetTypeFromName(editorAttribute.EditorBaseTypeName);
				if (editorBaseType == typeFromName)
				{
					Type typeFromName2 = GetTypeFromName(editorAttribute.EditorTypeName);
					if (typeFromName2 != null)
					{
						obj = CreateInstance(typeFromName2);
						break;
					}
				}
			}
			if (obj == null)
			{
				obj = TypeDescriptor.GetEditor(PropertyType, editorBaseType);
			}
			if (editorTypes == null)
			{
				editorTypes = new Type[5];
				editors = new object[5];
			}
			if (editorCount >= editorTypes.Length)
			{
				Type[] destinationArray = new Type[editorTypes.Length * 2];
				object[] destinationArray2 = new object[editors.Length * 2];
				Array.Copy(editorTypes, destinationArray, editorTypes.Length);
				Array.Copy(editors, destinationArray2, editors.Length);
				editorTypes = destinationArray;
				editors = destinationArray2;
			}
			editorTypes[editorCount] = editorBaseType;
			editors[editorCount++] = obj;
		}
		return obj;
	}

	/// <summary>Returns the hash code for this object.</summary>
	/// <returns>The hash code for this object.</returns>
	public override int GetHashCode()
	{
		return NameHashCode ^ PropertyType.GetHashCode();
	}

	/// <summary>This method returns the object that should be used during invocation of members.</summary>
	/// <returns>The <see cref="T:System.Object" /> that should be used during invocation of members.</returns>
	/// <param name="type">The <see cref="T:System.Type" /> of the invocation target.</param>
	/// <param name="instance">The potential invocation target.</param>
	protected override object GetInvocationTarget(Type type, object instance)
	{
		object obj = base.GetInvocationTarget(type, instance);
		if (obj is ICustomTypeDescriptor customTypeDescriptor)
		{
			obj = customTypeDescriptor.GetPropertyOwner(this);
		}
		return obj;
	}

	/// <summary>Returns a type using its name.</summary>
	/// <returns>A <see cref="T:System.Type" /> that matches the given type name, or null if a match cannot be found.</returns>
	/// <param name="typeName">The assembly-qualified name of the type to retrieve. </param>
	protected Type GetTypeFromName(string typeName)
	{
		if (typeName == null || typeName.Length == 0)
		{
			return null;
		}
		Type type = Type.GetType(typeName);
		Type type2 = null;
		if (ComponentType != null && (type == null || ComponentType.Assembly.FullName.Equals(type.Assembly.FullName)))
		{
			int num = typeName.IndexOf(',');
			if (num != -1)
			{
				typeName = typeName.Substring(0, num);
			}
			type2 = ComponentType.Assembly.GetType(typeName);
		}
		return type2 ?? type;
	}

	/// <summary>When overridden in a derived class, gets the current value of the property on a component.</summary>
	/// <returns>The value of a property for a given component.</returns>
	/// <param name="component">The component with the property for which to retrieve the value. </param>
	public abstract object GetValue(object component);

	/// <summary>Raises the ValueChanged event that you implemented.</summary>
	/// <param name="component">The object that raises the event. </param>
	/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data. </param>
	protected virtual void OnValueChanged(object component, EventArgs e)
	{
		if (component != null && valueChangedHandlers != null)
		{
			((EventHandler)valueChangedHandlers[component])?.Invoke(component, e);
		}
	}

	/// <summary>Enables other objects to be notified when this property changes.</summary>
	/// <param name="component">The component to remove the handler for. </param>
	/// <param name="handler">The delegate to remove as a listener. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="component" /> or <paramref name="handler" /> is null.</exception>
	public virtual void RemoveValueChanged(object component, EventHandler handler)
	{
		if (component == null)
		{
			throw new ArgumentNullException("component");
		}
		if (handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		if (valueChangedHandlers != null)
		{
			EventHandler source = (EventHandler)valueChangedHandlers[component];
			source = (EventHandler)Delegate.Remove(source, handler);
			if (source != null)
			{
				valueChangedHandlers[component] = source;
			}
			else
			{
				valueChangedHandlers.Remove(component);
			}
		}
	}

	/// <summary>Retrieves the current set of ValueChanged event handlers for a specific component</summary>
	/// <returns>A combined multicast event handler, or null if no event handlers are currently assigned to <paramref name="component" />.</returns>
	/// <param name="component">The component for which to retrieve event handlers.</param>
	protected internal EventHandler GetValueChangedHandler(object component)
	{
		if (component != null && valueChangedHandlers != null)
		{
			return (EventHandler)valueChangedHandlers[component];
		}
		return null;
	}

	/// <summary>When overridden in a derived class, resets the value for this property of the component to the default value.</summary>
	/// <param name="component">The component with the property value that is to be reset to the default value. </param>
	public abstract void ResetValue(object component);

	/// <summary>When overridden in a derived class, sets the value of the component to a different value.</summary>
	/// <param name="component">The component with the property value that is to be set. </param>
	/// <param name="value">The new value. </param>
	public abstract void SetValue(object component, object value);

	/// <summary>When overridden in a derived class, determines a value indicating whether the value of this property needs to be persisted.</summary>
	/// <returns>true if the property should be persisted; otherwise, false.</returns>
	/// <param name="component">The component with the property to be examined for persistence. </param>
	public abstract bool ShouldSerializeValue(object component);
}
