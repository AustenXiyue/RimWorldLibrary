namespace System.ComponentModel;

/// <summary>Specifies whether the property this attribute is bound to is read-only or read/write. This class cannot be inherited</summary>
[AttributeUsage(AttributeTargets.All)]
public sealed class ReadOnlyAttribute : Attribute
{
	private bool isReadOnly;

	/// <summary>Specifies that the property this attribute is bound to is read-only and cannot be modified in the server explorer. This static field is read-only.</summary>
	public static readonly ReadOnlyAttribute Yes = new ReadOnlyAttribute(isReadOnly: true);

	/// <summary>Specifies that the property this attribute is bound to is read/write and can be modified. This static field is read-only.</summary>
	public static readonly ReadOnlyAttribute No = new ReadOnlyAttribute(isReadOnly: false);

	/// <summary>Specifies the default value for the <see cref="T:System.ComponentModel.ReadOnlyAttribute" />, which is <see cref="F:System.ComponentModel.ReadOnlyAttribute.No" /> (that is, the property this attribute is bound to is read/write). This static field is read-only.</summary>
	public static readonly ReadOnlyAttribute Default = No;

	/// <summary>Gets a value indicating whether the property this attribute is bound to is read-only.</summary>
	/// <returns>true if the property this attribute is bound to is read-only; false if the property is read/write.</returns>
	public bool IsReadOnly => isReadOnly;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.ReadOnlyAttribute" /> class.</summary>
	/// <param name="isReadOnly">true to show that the property this attribute is bound to is read-only; false to show that the property is read/write. </param>
	public ReadOnlyAttribute(bool isReadOnly)
	{
		this.isReadOnly = isReadOnly;
	}

	/// <summary>Indicates whether this instance and a specified object are equal.</summary>
	/// <returns>true if <paramref name="value" /> is equal to this instance; otherwise, false.</returns>
	/// <param name="value">Another object to compare to. </param>
	public override bool Equals(object value)
	{
		if (this == value)
		{
			return true;
		}
		if (value is ReadOnlyAttribute readOnlyAttribute)
		{
			return readOnlyAttribute.IsReadOnly == IsReadOnly;
		}
		return false;
	}

	/// <summary>Returns the hash code for this instance.</summary>
	/// <returns>A hash code for the current <see cref="T:System.ComponentModel.ReadOnlyAttribute" />.</returns>
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	/// <summary>Determines if this attribute is the default.</summary>
	/// <returns>true if the attribute is the default value for this attribute class; otherwise, false.</returns>
	public override bool IsDefaultAttribute()
	{
		return IsReadOnly == Default.IsReadOnly;
	}
}
