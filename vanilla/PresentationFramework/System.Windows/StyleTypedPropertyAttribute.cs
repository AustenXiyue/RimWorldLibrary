namespace System.Windows;

/// <summary>Represents an attribute that is applied to the class definition and determines the <see cref="P:System.Windows.Style.TargetType" />s of the properties that are of type <see cref="T:System.Windows.Style" />.</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class StyleTypedPropertyAttribute : Attribute
{
	private string _property;

	private Type _styleTargetType;

	/// <summary>Gets or sets the name of the property that is of type <see cref="T:System.Windows.Style" />.</summary>
	/// <returns>The name of the property that is of type <see cref="T:System.Windows.Style" />.</returns>
	public string Property
	{
		get
		{
			return _property;
		}
		set
		{
			_property = value;
		}
	}

	/// <summary>Gets or sets the <see cref="P:System.Windows.Style.TargetType" /> of the <see cref="P:System.Windows.StyleTypedPropertyAttribute.Property" /> this attribute is specifying.</summary>
	/// <returns>The <see cref="P:System.Windows.Style.TargetType" /> of the <see cref="P:System.Windows.StyleTypedPropertyAttribute.Property" /> this attribute is specifying.</returns>
	public Type StyleTargetType
	{
		get
		{
			return _styleTargetType;
		}
		set
		{
			_styleTargetType = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.StyleTypedPropertyAttribute" /> class.</summary>
	public StyleTypedPropertyAttribute()
	{
	}
}
