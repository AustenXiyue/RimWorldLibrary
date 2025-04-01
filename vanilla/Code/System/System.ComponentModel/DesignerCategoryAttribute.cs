namespace System.ComponentModel;

/// <summary>Specifies that the designer for a class belongs to a certain category.</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class DesignerCategoryAttribute : Attribute
{
	private string category;

	private string typeId;

	/// <summary>Specifies that a component marked with this category use a component designer. This field is read-only.</summary>
	public static readonly DesignerCategoryAttribute Component = new DesignerCategoryAttribute("Component");

	/// <summary>Specifies that a component marked with this category cannot use a visual designer. This static field is read-only.</summary>
	public static readonly DesignerCategoryAttribute Default = new DesignerCategoryAttribute();

	/// <summary>Specifies that a component marked with this category use a form designer. This static field is read-only.</summary>
	public static readonly DesignerCategoryAttribute Form = new DesignerCategoryAttribute("Form");

	/// <summary>Specifies that a component marked with this category use a generic designer. This static field is read-only.</summary>
	public static readonly DesignerCategoryAttribute Generic = new DesignerCategoryAttribute("Designer");

	/// <summary>Gets the name of the category.</summary>
	/// <returns>The name of the category.</returns>
	public string Category => category;

	/// <summary>Gets a unique identifier for this attribute.</summary>
	/// <returns>An <see cref="T:System.Object" /> that is a unique identifier for the attribute.</returns>
	public override object TypeId
	{
		get
		{
			if (typeId == null)
			{
				typeId = GetType().FullName + Category;
			}
			return typeId;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DesignerCategoryAttribute" /> class with an empty string ("").</summary>
	public DesignerCategoryAttribute()
	{
		category = string.Empty;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DesignerCategoryAttribute" /> class with the given category name.</summary>
	/// <param name="category">The name of the category. </param>
	public DesignerCategoryAttribute(string category)
	{
		this.category = category;
	}

	/// <summary>Returns whether the value of the given object is equal to the current <see cref="T:System.ComponentModel.DesignOnlyAttribute" />.</summary>
	/// <returns>true if the value of the given object is equal to that of the current; otherwise, false.</returns>
	/// <param name="obj">The object to test the value equality of. </param>
	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (obj is DesignerCategoryAttribute designerCategoryAttribute)
		{
			return designerCategoryAttribute.category == category;
		}
		return false;
	}

	/// <summary>Returns the hash code for this instance.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	public override int GetHashCode()
	{
		return category.GetHashCode();
	}

	/// <summary>Determines if this attribute is the default.</summary>
	/// <returns>true if the attribute is the default value for this attribute class; otherwise, false.</returns>
	public override bool IsDefaultAttribute()
	{
		return category.Equals(Default.Category);
	}
}
