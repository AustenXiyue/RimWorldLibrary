namespace System.ComponentModel;

/// <summary>Specifies whether a member is typically used for binding. This class cannot be inherited.</summary>
[AttributeUsage(AttributeTargets.All)]
public sealed class BindableAttribute : Attribute
{
	/// <summary>Specifies that a property is typically used for binding. This field is read-only.</summary>
	public static readonly BindableAttribute Yes = new BindableAttribute(bindable: true);

	/// <summary>Specifies that a property is not typically used for binding. This field is read-only.</summary>
	public static readonly BindableAttribute No = new BindableAttribute(bindable: false);

	/// <summary>Specifies the default value for the <see cref="T:System.ComponentModel.BindableAttribute" />, which is <see cref="F:System.ComponentModel.BindableAttribute.No" />. This field is read-only.</summary>
	public static readonly BindableAttribute Default = No;

	private bool bindable;

	private bool isDefault;

	private BindingDirection direction;

	/// <summary>Gets a value indicating that a property is typically used for binding.</summary>
	/// <returns>true if the property is typically used for binding; otherwise, false.</returns>
	public bool Bindable => bindable;

	/// <summary>Gets a value indicating the direction or directions of this property's data binding.</summary>
	/// <returns>The direction of this property’s data binding.</returns>
	public BindingDirection Direction => direction;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.BindableAttribute" /> class with a Boolean value.</summary>
	/// <param name="bindable">true to use property for binding; otherwise, false.</param>
	public BindableAttribute(bool bindable)
		: this(bindable, BindingDirection.OneWay)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.BindableAttribute" /> class.</summary>
	/// <param name="bindable">true to use property for binding; otherwise, false.</param>
	/// <param name="direction">One of the <see cref="T:System.ComponentModel.BindingDirection" /> values.</param>
	public BindableAttribute(bool bindable, BindingDirection direction)
	{
		this.bindable = bindable;
		this.direction = direction;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.BindableAttribute" /> class with one of the <see cref="T:System.ComponentModel.BindableSupport" /> values.</summary>
	/// <param name="flags">One of the <see cref="T:System.ComponentModel.BindableSupport" /> values. </param>
	public BindableAttribute(BindableSupport flags)
		: this(flags, BindingDirection.OneWay)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.BindableAttribute" /> class.</summary>
	/// <param name="flags">One of the <see cref="T:System.ComponentModel.BindableSupport" /> values. </param>
	/// <param name="direction">One of the <see cref="T:System.ComponentModel.BindingDirection" /> values.</param>
	public BindableAttribute(BindableSupport flags, BindingDirection direction)
	{
		bindable = flags != BindableSupport.No;
		isDefault = flags == BindableSupport.Default;
		this.direction = direction;
	}

	/// <summary>Determines whether two <see cref="T:System.ComponentModel.BindableAttribute" /> objects are equal.</summary>
	/// <returns>true if the specified <see cref="T:System.ComponentModel.BindableAttribute" /> is equal to the current <see cref="T:System.ComponentModel.BindableAttribute" />; false if it is not equal.</returns>
	/// <param name="obj">The object to compare.</param>
	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (obj != null && obj is BindableAttribute)
		{
			return ((BindableAttribute)obj).Bindable == bindable;
		}
		return false;
	}

	/// <summary>Serves as a hash function for the <see cref="T:System.ComponentModel.BindableAttribute" /> class.</summary>
	/// <returns>A hash code for the current <see cref="T:System.ComponentModel.BindableAttribute" />.</returns>
	public override int GetHashCode()
	{
		return bindable.GetHashCode();
	}

	/// <summary>Determines if this attribute is the default.</summary>
	/// <returns>true if the attribute is the default value for this attribute class; otherwise, false.</returns>
	public override bool IsDefaultAttribute()
	{
		if (!Equals(Default))
		{
			return isDefault;
		}
		return true;
	}
}
