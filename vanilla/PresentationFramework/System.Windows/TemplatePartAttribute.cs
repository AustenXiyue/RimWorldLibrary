namespace System.Windows;

/// <summary>Represents an attribute that is applied to the class definition to identify the types of the named parts that are used for templating.</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class TemplatePartAttribute : Attribute
{
	private string _name;

	private Type _type;

	/// <summary>Gets or sets the pre-defined name of the part.</summary>
	/// <returns>The pre-defined name of the part.</returns>
	public string Name
	{
		get
		{
			return _name;
		}
		set
		{
			_name = value;
		}
	}

	/// <summary>Gets or sets the type of the named part this attribute is identifying.</summary>
	/// <returns>The type of the named part this attribute is identifying.</returns>
	public Type Type
	{
		get
		{
			return _type;
		}
		set
		{
			_type = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.TemplatePartAttribute" /> class.</summary>
	public TemplatePartAttribute()
	{
	}
}
