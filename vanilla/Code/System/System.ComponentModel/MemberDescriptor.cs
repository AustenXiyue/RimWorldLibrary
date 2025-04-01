using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Threading;

namespace System.ComponentModel;

/// <summary>Represents a class member, such as a property or event. This is an abstract base class.</summary>
[ComVisible(true)]
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
public abstract class MemberDescriptor
{
	private string name;

	private string displayName;

	private int nameHash;

	private AttributeCollection attributeCollection;

	private Attribute[] attributes;

	private Attribute[] originalAttributes;

	private bool attributesFiltered;

	private bool attributesFilled;

	private int metadataVersion;

	private string category;

	private string description;

	private object lockCookie = new object();

	/// <summary>Gets or sets an array of attributes.</summary>
	/// <returns>An array of type <see cref="T:System.Attribute" /> that contains the attributes of this member. </returns>
	protected virtual Attribute[] AttributeArray
	{
		get
		{
			CheckAttributesValid();
			FilterAttributesIfNeeded();
			return attributes;
		}
		set
		{
			lock (lockCookie)
			{
				attributes = value;
				originalAttributes = value;
				attributesFiltered = false;
				attributeCollection = null;
			}
		}
	}

	/// <summary>Gets the collection of attributes for this member.</summary>
	/// <returns>An <see cref="T:System.ComponentModel.AttributeCollection" /> that provides the attributes for this member, or an empty collection if there are no attributes in the <see cref="P:System.ComponentModel.MemberDescriptor.AttributeArray" />.</returns>
	public virtual AttributeCollection Attributes
	{
		get
		{
			CheckAttributesValid();
			AttributeCollection attributeCollection = this.attributeCollection;
			if (attributeCollection == null)
			{
				lock (lockCookie)
				{
					attributeCollection = (this.attributeCollection = CreateAttributeCollection());
				}
			}
			return attributeCollection;
		}
	}

	/// <summary>Gets the name of the category to which the member belongs, as specified in the <see cref="T:System.ComponentModel.CategoryAttribute" />.</summary>
	/// <returns>The name of the category to which the member belongs. If there is no <see cref="T:System.ComponentModel.CategoryAttribute" />, the category name is set to the default category, Misc.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public virtual string Category
	{
		get
		{
			if (category == null)
			{
				category = ((CategoryAttribute)Attributes[typeof(CategoryAttribute)]).Category;
			}
			return category;
		}
	}

	/// <summary>Gets the description of the member, as specified in the <see cref="T:System.ComponentModel.DescriptionAttribute" />.</summary>
	/// <returns>The description of the member. If there is no <see cref="T:System.ComponentModel.DescriptionAttribute" />, the property value is set to the default, which is an empty string ("").</returns>
	public virtual string Description
	{
		get
		{
			if (description == null)
			{
				description = ((DescriptionAttribute)Attributes[typeof(DescriptionAttribute)]).Description;
			}
			return description;
		}
	}

	/// <summary>Gets a value indicating whether the member is browsable, as specified in the <see cref="T:System.ComponentModel.BrowsableAttribute" />.</summary>
	/// <returns>true if the member is browsable; otherwise, false. If there is no <see cref="T:System.ComponentModel.BrowsableAttribute" />, the property value is set to the default, which is true.</returns>
	public virtual bool IsBrowsable => ((BrowsableAttribute)Attributes[typeof(BrowsableAttribute)]).Browsable;

	/// <summary>Gets the name of the member.</summary>
	/// <returns>The name of the member.</returns>
	public virtual string Name
	{
		get
		{
			if (name == null)
			{
				return "";
			}
			return name;
		}
	}

	/// <summary>Gets the hash code for the name of the member, as specified in <see cref="M:System.String.GetHashCode" />.</summary>
	/// <returns>The hash code for the name of the member.</returns>
	protected virtual int NameHashCode => nameHash;

	/// <summary>Gets whether this member should be set only at design time, as specified in the <see cref="T:System.ComponentModel.DesignOnlyAttribute" />.</summary>
	/// <returns>true if this member should be set only at design time; false if the member can be set during run time.</returns>
	public virtual bool DesignTimeOnly => DesignOnlyAttribute.Yes.Equals(Attributes[typeof(DesignOnlyAttribute)]);

	/// <summary>Gets the name that can be displayed in a window, such as a Properties window.</summary>
	/// <returns>The name to display for the member.</returns>
	public virtual string DisplayName
	{
		get
		{
			if (!(Attributes[typeof(DisplayNameAttribute)] is DisplayNameAttribute displayNameAttribute) || displayNameAttribute.IsDefaultAttribute())
			{
				return displayName;
			}
			return displayNameAttribute.DisplayName;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.MemberDescriptor" /> class with the specified name of the member.</summary>
	/// <param name="name">The name of the member. </param>
	/// <exception cref="T:System.ArgumentException">The name is an empty string ("") or null.</exception>
	protected MemberDescriptor(string name)
		: this(name, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.MemberDescriptor" /> class with the specified name of the member and an array of attributes.</summary>
	/// <param name="name">The name of the member. </param>
	/// <param name="attributes">An array of type <see cref="T:System.Attribute" /> that contains the member attributes. </param>
	/// <exception cref="T:System.ArgumentException">The name is an empty string ("") or null. </exception>
	protected MemberDescriptor(string name, Attribute[] attributes)
	{
		try
		{
			if (name == null || name.Length == 0)
			{
				throw new ArgumentException(global::SR.GetString("Invalid member name."));
			}
			this.name = name;
			displayName = name;
			nameHash = name.GetHashCode();
			if (attributes != null)
			{
				this.attributes = attributes;
				attributesFiltered = false;
			}
			originalAttributes = this.attributes;
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.MemberDescriptor" /> class with the specified <see cref="T:System.ComponentModel.MemberDescriptor" />.</summary>
	/// <param name="descr">A <see cref="T:System.ComponentModel.MemberDescriptor" /> that contains the name of the member and its attributes. </param>
	protected MemberDescriptor(MemberDescriptor descr)
	{
		name = descr.Name;
		displayName = name;
		nameHash = name.GetHashCode();
		attributes = new Attribute[descr.Attributes.Count];
		descr.Attributes.CopyTo(attributes, 0);
		attributesFiltered = true;
		originalAttributes = attributes;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.MemberDescriptor" /> class with the name in the specified <see cref="T:System.ComponentModel.MemberDescriptor" /> and the attributes in both the old <see cref="T:System.ComponentModel.MemberDescriptor" /> and the <see cref="T:System.Attribute" /> array.</summary>
	/// <param name="oldMemberDescriptor">A <see cref="T:System.ComponentModel.MemberDescriptor" /> that has the name of the member and its attributes. </param>
	/// <param name="newAttributes">An array of <see cref="T:System.Attribute" /> objects with the attributes you want to add to the member. </param>
	protected MemberDescriptor(MemberDescriptor oldMemberDescriptor, Attribute[] newAttributes)
	{
		name = oldMemberDescriptor.Name;
		displayName = oldMemberDescriptor.DisplayName;
		nameHash = name.GetHashCode();
		ArrayList arrayList = new ArrayList();
		if (oldMemberDescriptor.Attributes.Count != 0)
		{
			foreach (object attribute in oldMemberDescriptor.Attributes)
			{
				arrayList.Add(attribute);
			}
		}
		if (newAttributes != null)
		{
			foreach (object value in newAttributes)
			{
				arrayList.Add(value);
			}
		}
		attributes = new Attribute[arrayList.Count];
		arrayList.CopyTo(attributes, 0);
		attributesFiltered = false;
		originalAttributes = attributes;
	}

	private void CheckAttributesValid()
	{
		if (attributesFiltered && metadataVersion != TypeDescriptor.MetadataVersion)
		{
			attributesFilled = false;
			attributesFiltered = false;
			attributeCollection = null;
		}
	}

	/// <summary>Creates a collection of attributes using the array of attributes passed to the constructor.</summary>
	/// <returns>A new <see cref="T:System.ComponentModel.AttributeCollection" /> that contains the <see cref="P:System.ComponentModel.MemberDescriptor.AttributeArray" /> attributes.</returns>
	protected virtual AttributeCollection CreateAttributeCollection()
	{
		return new AttributeCollection(AttributeArray);
	}

	/// <summary>Compares this instance to the given object to see if they are equivalent.</summary>
	/// <returns>true if equivalent; otherwise, false.</returns>
	/// <param name="obj">The object to compare to the current instance. </param>
	public override bool Equals(object obj)
	{
		if (this == obj)
		{
			return true;
		}
		if (obj == null)
		{
			return false;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		MemberDescriptor memberDescriptor = (MemberDescriptor)obj;
		FilterAttributesIfNeeded();
		memberDescriptor.FilterAttributesIfNeeded();
		if (memberDescriptor.nameHash != nameHash)
		{
			return false;
		}
		if (memberDescriptor.category == null != (category == null) || (category != null && !memberDescriptor.category.Equals(category)))
		{
			return false;
		}
		if (!System.LocalAppContextSwitches.MemberDescriptorEqualsReturnsFalseIfEquivalent)
		{
			if (memberDescriptor.description == null != (description == null) || (description != null && !memberDescriptor.description.Equals(description)))
			{
				return false;
			}
		}
		else if (memberDescriptor.description == null != (description == null) || (description != null && !memberDescriptor.category.Equals(description)))
		{
			return false;
		}
		if (memberDescriptor.attributes == null != (attributes == null))
		{
			return false;
		}
		bool result = true;
		if (attributes != null)
		{
			if (attributes.Length != memberDescriptor.attributes.Length)
			{
				return false;
			}
			for (int i = 0; i < attributes.Length; i++)
			{
				if (!attributes[i].Equals(memberDescriptor.attributes[i]))
				{
					result = false;
					break;
				}
			}
		}
		return result;
	}

	/// <summary>When overridden in a derived class, adds the attributes of the inheriting class to the specified list of attributes in the parent class.</summary>
	/// <param name="attributeList">An <see cref="T:System.Collections.IList" /> that lists the attributes in the parent class. Initially, this is empty. </param>
	protected virtual void FillAttributes(IList attributeList)
	{
		if (originalAttributes != null)
		{
			Attribute[] array = originalAttributes;
			foreach (Attribute value in array)
			{
				attributeList.Add(value);
			}
		}
	}

	private void FilterAttributesIfNeeded()
	{
		if (attributesFiltered)
		{
			return;
		}
		IList list;
		if (!attributesFilled)
		{
			list = new ArrayList();
			try
			{
				FillAttributes(list);
			}
			catch (ThreadAbortException)
			{
				throw;
			}
			catch (Exception)
			{
			}
		}
		else
		{
			list = new ArrayList(attributes);
		}
		Hashtable hashtable = new Hashtable(list.Count);
		foreach (Attribute item in list)
		{
			hashtable[item.TypeId] = item;
		}
		Attribute[] array = new Attribute[hashtable.Values.Count];
		hashtable.Values.CopyTo(array, 0);
		lock (lockCookie)
		{
			attributes = array;
			attributesFiltered = true;
			attributesFilled = true;
			metadataVersion = TypeDescriptor.MetadataVersion;
		}
	}

	/// <summary>Finds the given method through reflection, searching only for public methods.</summary>
	/// <returns>A <see cref="T:System.Reflection.MethodInfo" /> that represents the method, or null if the method is not found.</returns>
	/// <param name="componentClass">The component that contains the method. </param>
	/// <param name="name">The name of the method to find. </param>
	/// <param name="args">An array of parameters for the method, used to choose between overloaded methods. </param>
	/// <param name="returnType">The type to return for the method. </param>
	protected static MethodInfo FindMethod(Type componentClass, string name, Type[] args, Type returnType)
	{
		return FindMethod(componentClass, name, args, returnType, publicOnly: true);
	}

	/// <summary>Finds the given method through reflection, with an option to search only public methods.</summary>
	/// <returns>A <see cref="T:System.Reflection.MethodInfo" /> that represents the method, or null if the method is not found.</returns>
	/// <param name="componentClass">The component that contains the method. </param>
	/// <param name="name">The name of the method to find. </param>
	/// <param name="args">An array of parameters for the method, used to choose between overloaded methods. </param>
	/// <param name="returnType">The type to return for the method. </param>
	/// <param name="publicOnly">Whether to restrict search to public methods. </param>
	protected static MethodInfo FindMethod(Type componentClass, string name, Type[] args, Type returnType, bool publicOnly)
	{
		MethodInfo methodInfo = null;
		methodInfo = ((!publicOnly) ? componentClass.GetMethod(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, args, null) : componentClass.GetMethod(name, args));
		if (methodInfo != null && !methodInfo.ReturnType.IsEquivalentTo(returnType))
		{
			methodInfo = null;
		}
		return methodInfo;
	}

	/// <summary>Returns the hash code for this instance.</summary>
	/// <returns>A hash code for the current <see cref="T:System.ComponentModel.MemberDescriptor" />.</returns>
	public override int GetHashCode()
	{
		return nameHash;
	}

	/// <summary>Retrieves the object that should be used during invocation of members.</summary>
	/// <returns>The object to be used during member invocations.</returns>
	/// <param name="type">The <see cref="T:System.Type" /> of the invocation target.</param>
	/// <param name="instance">The potential invocation target.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="type" /> or <paramref name="instance" /> is null.</exception>
	protected virtual object GetInvocationTarget(Type type, object instance)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (instance == null)
		{
			throw new ArgumentNullException("instance");
		}
		return TypeDescriptor.GetAssociation(type, instance);
	}

	/// <summary>Gets a component site for the given component.</summary>
	/// <returns>The site of the component, or null if a site does not exist.</returns>
	/// <param name="component">The component for which you want to find a site. </param>
	protected static ISite GetSite(object component)
	{
		if (!(component is IComponent))
		{
			return null;
		}
		return ((IComponent)component).Site;
	}

	/// <summary>Gets the component on which to invoke a method.</summary>
	/// <returns>An instance of the component to invoke. This method returns a visual designer when the property is attached to a visual designer.</returns>
	/// <param name="componentClass">A <see cref="T:System.Type" /> representing the type of component this <see cref="T:System.ComponentModel.MemberDescriptor" /> is bound to. For example, if this <see cref="T:System.ComponentModel.MemberDescriptor" /> describes a property, this parameter should be the class that the property is declared on. </param>
	/// <param name="component">An instance of the object to call. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="componentClass" /> or <paramref name="component" /> is null.</exception>
	[Obsolete("This method has been deprecated. Use GetInvocationTarget instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
	protected static object GetInvokee(Type componentClass, object component)
	{
		if (componentClass == null)
		{
			throw new ArgumentNullException("componentClass");
		}
		if (component == null)
		{
			throw new ArgumentNullException("component");
		}
		return TypeDescriptor.GetAssociation(componentClass, component);
	}
}
