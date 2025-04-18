using System.Reflection;

namespace System.ComponentModel;

/// <summary>Identifies the property tab or tabs to display for the specified class or classes.</summary>
[AttributeUsage(AttributeTargets.All)]
public class PropertyTabAttribute : Attribute
{
	private PropertyTabScope[] tabScopes;

	private Type[] tabClasses;

	private string[] tabClassNames;

	/// <summary>Gets the types of tabs that this attribute uses.</summary>
	/// <returns>An array of types indicating the types of tabs that this attribute uses.</returns>
	/// <exception cref="T:System.TypeLoadException">The types specified by the <see cref="P:System.ComponentModel.PropertyTabAttribute.TabClassNames" /> property could not be found.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public Type[] TabClasses
	{
		get
		{
			if (tabClasses == null && tabClassNames != null)
			{
				tabClasses = new Type[tabClassNames.Length];
				for (int i = 0; i < tabClassNames.Length; i++)
				{
					int num = tabClassNames[i].IndexOf(',');
					string text = null;
					string text2 = null;
					if (num != -1)
					{
						text = tabClassNames[i].Substring(0, num).Trim();
						text2 = tabClassNames[i].Substring(num + 1).Trim();
					}
					else
					{
						text = tabClassNames[i];
					}
					tabClasses[i] = Type.GetType(text, throwOnError: false);
					if (tabClasses[i] == null)
					{
						if (text2 == null)
						{
							throw new TypeLoadException(global::SR.GetString("Couldn't find type {0}", text));
						}
						Assembly assembly = Assembly.Load(text2);
						if (assembly != null)
						{
							tabClasses[i] = assembly.GetType(text, throwOnError: true);
						}
					}
				}
			}
			return tabClasses;
		}
	}

	/// <summary>Gets the names of the tab classes that this attribute uses.</summary>
	/// <returns>The names of the tab classes that this attribute uses.</returns>
	protected string[] TabClassNames
	{
		get
		{
			if (tabClassNames != null)
			{
				return (string[])tabClassNames.Clone();
			}
			return null;
		}
	}

	/// <summary>Gets an array of tab scopes of each tab of this <see cref="T:System.ComponentModel.PropertyTabAttribute" />.</summary>
	/// <returns>An array of <see cref="T:System.ComponentModel.PropertyTabScope" /> objects that indicate the scopes of the tabs.</returns>
	public PropertyTabScope[] TabScopes => tabScopes;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.PropertyTabAttribute" /> class.</summary>
	public PropertyTabAttribute()
	{
		tabScopes = new PropertyTabScope[0];
		tabClassNames = new string[0];
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.PropertyTabAttribute" /> class using the specified type of tab.</summary>
	/// <param name="tabClass">The type of tab to create. </param>
	public PropertyTabAttribute(Type tabClass)
		: this(tabClass, PropertyTabScope.Component)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.PropertyTabAttribute" /> class using the specified tab class name.</summary>
	/// <param name="tabClassName">The assembly qualified name of the type of tab to create. For an example of this format convention, see <see cref="P:System.Type.AssemblyQualifiedName" />. </param>
	public PropertyTabAttribute(string tabClassName)
		: this(tabClassName, PropertyTabScope.Component)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.PropertyTabAttribute" /> class using the specified type of tab and tab scope.</summary>
	/// <param name="tabClass">The type of tab to create. </param>
	/// <param name="tabScope">A <see cref="T:System.ComponentModel.PropertyTabScope" /> that indicates the scope of this tab. If the scope is <see cref="F:System.ComponentModel.PropertyTabScope.Component" />, it is shown only for components with the corresponding <see cref="T:System.ComponentModel.PropertyTabAttribute" />. If it is <see cref="F:System.ComponentModel.PropertyTabScope.Document" />, it is shown for all components on the document. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="tabScope" /> is not <see cref="F:System.ComponentModel.PropertyTabScope.Document" /> or <see cref="F:System.ComponentModel.PropertyTabScope.Component" />.</exception>
	public PropertyTabAttribute(Type tabClass, PropertyTabScope tabScope)
	{
		tabClasses = new Type[1] { tabClass };
		if (tabScope < PropertyTabScope.Document)
		{
			throw new ArgumentException(global::SR.GetString("Scope must be PropertyTabScope.Document or PropertyTabScope.Component"), "tabScope");
		}
		tabScopes = new PropertyTabScope[1] { tabScope };
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.PropertyTabAttribute" /> class using the specified tab class name and tab scope.</summary>
	/// <param name="tabClassName">The assembly qualified name of the type of tab to create. For an example of this format convention, see <see cref="P:System.Type.AssemblyQualifiedName" />. </param>
	/// <param name="tabScope">A <see cref="T:System.ComponentModel.PropertyTabScope" /> that indicates the scope of this tab. If the scope is <see cref="F:System.ComponentModel.PropertyTabScope.Component" />, it is shown only for components with the corresponding <see cref="T:System.ComponentModel.PropertyTabAttribute" />. If it is <see cref="F:System.ComponentModel.PropertyTabScope.Document" />, it is shown for all components on the document. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="tabScope" /> is not <see cref="F:System.ComponentModel.PropertyTabScope.Document" /> or <see cref="F:System.ComponentModel.PropertyTabScope.Component" />.</exception>
	public PropertyTabAttribute(string tabClassName, PropertyTabScope tabScope)
	{
		tabClassNames = new string[1] { tabClassName };
		if (tabScope < PropertyTabScope.Document)
		{
			throw new ArgumentException(global::SR.GetString("Scope must be PropertyTabScope.Document or PropertyTabScope.Component"), "tabScope");
		}
		tabScopes = new PropertyTabScope[1] { tabScope };
	}

	/// <summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
	/// <returns>true if <paramref name="other" /> refers to the same <see cref="T:System.ComponentModel.PropertyTabAttribute" /> instance; otherwise, false.</returns>
	/// <param name="other">An object to compare to this instance, or null.</param>
	/// <exception cref="T:System.TypeLoadException">The types specified by the <see cref="P:System.ComponentModel.PropertyTabAttribute.TabClassNames" /> property of the<paramref name=" other" /> parameter could not be found.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public override bool Equals(object other)
	{
		if (other is PropertyTabAttribute)
		{
			return Equals((PropertyTabAttribute)other);
		}
		return false;
	}

	/// <summary>Returns a value indicating whether this instance is equal to a specified attribute.</summary>
	/// <returns>true if the <see cref="T:System.ComponentModel.PropertyTabAttribute" /> instances are equal; otherwise, false.</returns>
	/// <param name="other">A <see cref="T:System.ComponentModel.PropertyTabAttribute" /> to compare to this instance, or null.</param>
	/// <exception cref="T:System.TypeLoadException">The types specified by the <see cref="P:System.ComponentModel.PropertyTabAttribute.TabClassNames" /> property of the <paramref name="other" /> parameter cannot be found.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public bool Equals(PropertyTabAttribute other)
	{
		if (other == this)
		{
			return true;
		}
		if (other.TabClasses.Length != TabClasses.Length || other.TabScopes.Length != TabScopes.Length)
		{
			return false;
		}
		for (int i = 0; i < TabClasses.Length; i++)
		{
			if (TabClasses[i] != other.TabClasses[i] || TabScopes[i] != other.TabScopes[i])
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>Gets the hash code for this object.</summary>
	/// <returns>The hash code for the object the attribute belongs to.</returns>
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	/// <summary>Initializes the attribute using the specified names of tab classes and array of tab scopes.</summary>
	/// <param name="tabClassNames">An array of fully qualified type names of the types to create for tabs on the Properties window. </param>
	/// <param name="tabScopes">The scope of each tab. If the scope is <see cref="F:System.ComponentModel.PropertyTabScope.Component" />, it is shown only for components with the corresponding <see cref="T:System.ComponentModel.PropertyTabAttribute" />. If it is <see cref="F:System.ComponentModel.PropertyTabScope.Document" />, it is shown for all components on the document. </param>
	/// <exception cref="T:System.ArgumentException">One or more of the values in <paramref name="tabScopes" /> is not <see cref="F:System.ComponentModel.PropertyTabScope.Document" /> or <see cref="F:System.ComponentModel.PropertyTabScope.Component" />.-or-The length of the <paramref name="tabClassNames" /> and <paramref name="tabScopes" /> arrays do not match.-or-<paramref name="tabClassNames" /> or <paramref name="tabScopes" /> is null.</exception>
	protected void InitializeArrays(string[] tabClassNames, PropertyTabScope[] tabScopes)
	{
		InitializeArrays(tabClassNames, null, tabScopes);
	}

	/// <summary>Initializes the attribute using the specified names of tab classes and array of tab scopes.</summary>
	/// <param name="tabClasses">The types of tabs to create. </param>
	/// <param name="tabScopes">The scope of each tab. If the scope is <see cref="F:System.ComponentModel.PropertyTabScope.Component" />, it is shown only for components with the corresponding <see cref="T:System.ComponentModel.PropertyTabAttribute" />. If it is <see cref="F:System.ComponentModel.PropertyTabScope.Document" />, it is shown for all components on the document. </param>
	/// <exception cref="T:System.ArgumentException">One or more of the values in <paramref name="tabScopes" /> is not <see cref="F:System.ComponentModel.PropertyTabScope.Document" /> or <see cref="F:System.ComponentModel.PropertyTabScope.Component" />.-or-The length of the <paramref name="tabClassNames" /> and <paramref name="tabScopes" /> arrays do not match.-or-<paramref name="tabClassNames" /> or <paramref name="tabScopes" /> is null.</exception>
	protected void InitializeArrays(Type[] tabClasses, PropertyTabScope[] tabScopes)
	{
		InitializeArrays(null, tabClasses, tabScopes);
	}

	private void InitializeArrays(string[] tabClassNames, Type[] tabClasses, PropertyTabScope[] tabScopes)
	{
		if (tabClasses != null)
		{
			if (tabScopes != null && tabClasses.Length != tabScopes.Length)
			{
				throw new ArgumentException(global::SR.GetString("tabClasses must have the same number of items as tabScopes"));
			}
			this.tabClasses = (Type[])tabClasses.Clone();
		}
		else if (tabClassNames != null)
		{
			if (tabScopes != null && tabClasses.Length != tabScopes.Length)
			{
				throw new ArgumentException(global::SR.GetString("tabClasses must have the same number of items as tabScopes"));
			}
			this.tabClassNames = (string[])tabClassNames.Clone();
			this.tabClasses = null;
		}
		else if (this.tabClasses == null && this.tabClassNames == null)
		{
			throw new ArgumentException(global::SR.GetString("An array of tab type names or tab types must be specified"));
		}
		if (tabScopes != null)
		{
			for (int i = 0; i < tabScopes.Length; i++)
			{
				if (tabScopes[i] < PropertyTabScope.Document)
				{
					throw new ArgumentException(global::SR.GetString("Scope must be PropertyTabScope.Document or PropertyTabScope.Component"));
				}
			}
			this.tabScopes = (PropertyTabScope[])tabScopes.Clone();
		}
		else
		{
			this.tabScopes = new PropertyTabScope[tabClasses.Length];
			for (int j = 0; j < TabScopes.Length; j++)
			{
				this.tabScopes[j] = PropertyTabScope.Component;
			}
		}
	}
}
