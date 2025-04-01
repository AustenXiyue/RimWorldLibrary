namespace System.ComponentModel;

/// <summary>Specifies that this property can be combined with properties belonging to other objects in a Properties window.</summary>
[AttributeUsage(AttributeTargets.All)]
public sealed class MergablePropertyAttribute : Attribute
{
	/// <summary>Specifies that a property can be combined with properties belonging to other objects in a Properties window. This static field is read-only.</summary>
	public static readonly MergablePropertyAttribute Yes = new MergablePropertyAttribute(allowMerge: true);

	/// <summary>Specifies that a property cannot be combined with properties belonging to other objects in a Properties window. This static field is read-only.</summary>
	public static readonly MergablePropertyAttribute No = new MergablePropertyAttribute(allowMerge: false);

	/// <summary>Specifies the default value, which is <see cref="F:System.ComponentModel.MergablePropertyAttribute.Yes" />, that is a property can be combined with properties belonging to other objects in a Properties window. This static field is read-only.</summary>
	public static readonly MergablePropertyAttribute Default = Yes;

	private bool allowMerge;

	/// <summary>Gets a value indicating whether this property can be combined with properties belonging to other objects in a Properties window.</summary>
	/// <returns>true if this property can be combined with properties belonging to other objects in a Properties window; otherwise, false.</returns>
	public bool AllowMerge => allowMerge;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.MergablePropertyAttribute" /> class.</summary>
	/// <param name="allowMerge">true if this property can be combined with properties belonging to other objects in a Properties window; otherwise, false. </param>
	public MergablePropertyAttribute(bool allowMerge)
	{
		this.allowMerge = allowMerge;
	}

	/// <summary>Indicates whether this instance and a specified object are equal.</summary>
	/// <returns>true if <paramref name="obj" /> is equal to this instance; otherwise, false.</returns>
	/// <param name="obj">Another object to compare to. </param>
	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (obj is MergablePropertyAttribute mergablePropertyAttribute)
		{
			return mergablePropertyAttribute.AllowMerge == allowMerge;
		}
		return false;
	}

	/// <summary>Returns the hash code for this instance.</summary>
	/// <returns>A hash code for the current <see cref="T:System.ComponentModel.MergablePropertyAttribute" />.</returns>
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	/// <summary>Determines if this attribute is the default.</summary>
	/// <returns>true if the attribute is the default value for this attribute class; otherwise, false.</returns>
	public override bool IsDefaultAttribute()
	{
		return Equals(Default);
	}
}
