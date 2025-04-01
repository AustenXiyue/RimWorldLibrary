namespace System.ComponentModel;

/// <summary>Specifies that a list can be used as a data source. A visual designer should use this attribute to determine whether to display a particular list in a data-binding picker. This class cannot be inherited.</summary>
[AttributeUsage(AttributeTargets.All)]
public sealed class ListBindableAttribute : Attribute
{
	/// <summary>Specifies that the list is bindable. This static field is read-only.</summary>
	public static readonly ListBindableAttribute Yes = new ListBindableAttribute(listBindable: true);

	/// <summary>Specifies that the list is not bindable. This static field is read-only.</summary>
	public static readonly ListBindableAttribute No = new ListBindableAttribute(listBindable: false);

	/// <summary>Represents the default value for <see cref="T:System.ComponentModel.ListBindableAttribute" />.</summary>
	public static readonly ListBindableAttribute Default = Yes;

	private bool listBindable;

	private bool isDefault;

	/// <summary>Gets whether the list is bindable.</summary>
	/// <returns>true if the list is bindable; otherwise, false.</returns>
	public bool ListBindable => listBindable;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.ListBindableAttribute" /> class using a value to indicate whether the list is bindable.</summary>
	/// <param name="listBindable">true if the list is bindable; otherwise, false. </param>
	public ListBindableAttribute(bool listBindable)
	{
		this.listBindable = listBindable;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.ListBindableAttribute" /> class using <see cref="T:System.ComponentModel.BindableSupport" /> to indicate whether the list is bindable.</summary>
	/// <param name="flags">A <see cref="T:System.ComponentModel.BindableSupport" /> that indicates whether the list is bindable. </param>
	public ListBindableAttribute(BindableSupport flags)
	{
		listBindable = flags != BindableSupport.No;
		isDefault = flags == BindableSupport.Default;
	}

	/// <summary>Returns whether the object passed is equal to this <see cref="T:System.ComponentModel.ListBindableAttribute" />.</summary>
	/// <returns>true if the object passed is equal to this <see cref="T:System.ComponentModel.ListBindableAttribute" />; otherwise, false.</returns>
	/// <param name="obj">The object to test equality with. </param>
	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (obj is ListBindableAttribute listBindableAttribute)
		{
			return listBindableAttribute.ListBindable == listBindable;
		}
		return false;
	}

	/// <summary>Returns the hash code for this instance.</summary>
	/// <returns>A hash code for the current <see cref="T:System.ComponentModel.ListBindableAttribute" />.</returns>
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	/// <summary>Returns whether <see cref="P:System.ComponentModel.ListBindableAttribute.ListBindable" /> is set to the default value.</summary>
	/// <returns>true if <see cref="P:System.ComponentModel.ListBindableAttribute.ListBindable" /> is set to the default value; otherwise, false.</returns>
	public override bool IsDefaultAttribute()
	{
		if (!Equals(Default))
		{
			return isDefault;
		}
		return true;
	}
}
