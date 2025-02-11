namespace System.ComponentModel;

/// <summary>Indicates whether the component associated with this attribute has been inherited from a base class. This class cannot be inherited.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event)]
public sealed class InheritanceAttribute : Attribute
{
	private readonly InheritanceLevel inheritanceLevel;

	/// <summary>Specifies that the component is inherited. This field is read-only.</summary>
	public static readonly InheritanceAttribute Inherited = new InheritanceAttribute(InheritanceLevel.Inherited);

	/// <summary>Specifies that the component is inherited and is read-only. This field is read-only.</summary>
	public static readonly InheritanceAttribute InheritedReadOnly = new InheritanceAttribute(InheritanceLevel.InheritedReadOnly);

	/// <summary>Specifies that the component is not inherited. This field is read-only.</summary>
	public static readonly InheritanceAttribute NotInherited = new InheritanceAttribute(InheritanceLevel.NotInherited);

	/// <summary>Specifies that the default value for <see cref="T:System.ComponentModel.InheritanceAttribute" /> is <see cref="F:System.ComponentModel.InheritanceAttribute.NotInherited" />. This field is read-only.</summary>
	public static readonly InheritanceAttribute Default = NotInherited;

	/// <summary>Gets or sets the current inheritance level stored in this attribute.</summary>
	/// <returns>The <see cref="T:System.ComponentModel.InheritanceLevel" /> stored in this attribute.</returns>
	public InheritanceLevel InheritanceLevel => inheritanceLevel;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.InheritanceAttribute" /> class.</summary>
	public InheritanceAttribute()
	{
		inheritanceLevel = Default.inheritanceLevel;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.InheritanceAttribute" /> class with the specified inheritance level.</summary>
	/// <param name="inheritanceLevel">An <see cref="T:System.ComponentModel.InheritanceLevel" /> that indicates the level of inheritance to set this attribute to. </param>
	public InheritanceAttribute(InheritanceLevel inheritanceLevel)
	{
		this.inheritanceLevel = inheritanceLevel;
	}

	/// <summary>Override to test for equality.</summary>
	/// <returns>true if the object is the same; otherwise, false.</returns>
	/// <param name="value">The object to test. </param>
	public override bool Equals(object value)
	{
		if (value == this)
		{
			return true;
		}
		if (!(value is InheritanceAttribute))
		{
			return false;
		}
		return ((InheritanceAttribute)value).InheritanceLevel == inheritanceLevel;
	}

	/// <summary>Returns the hashcode for this object.</summary>
	/// <returns>A hash code for the current <see cref="T:System.ComponentModel.InheritanceAttribute" />.</returns>
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	/// <summary>Gets a value indicating whether the current value of the attribute is the default value for the attribute.</summary>
	/// <returns>true if the current value of the attribute is the default; otherwise, false.</returns>
	public override bool IsDefaultAttribute()
	{
		return Equals(Default);
	}

	/// <summary>Converts this attribute to a string.</summary>
	/// <returns>A string that represents this <see cref="T:System.ComponentModel.InheritanceAttribute" />.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public override string ToString()
	{
		return TypeDescriptor.GetConverter(typeof(InheritanceLevel)).ConvertToString(InheritanceLevel);
	}
}
