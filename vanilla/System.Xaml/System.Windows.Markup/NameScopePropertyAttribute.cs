using System.Runtime.CompilerServices;

namespace System.Windows.Markup;

/// <summary>Specifies a property of the associated class that provides the XAML namescope value.</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
[TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
public sealed class NameScopePropertyAttribute : Attribute
{
	/// <summary>Gets the name of the property that provides the XAML namescope.</summary>
	/// <returns>A string value that is the name of the property that provides the XAML namescope.</returns>
	public string Name { get; }

	/// <summary>Gets the owner type of the attached property that provides the XAML namescope support.</summary>
	/// <returns>A <see cref="T:System.Type" /> value that is the owner type of the attached property that provides the XAML namescope support, or null. See Remarks.</returns>
	public Type Type { get; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.NameScopePropertyAttribute" /> class with the specified name.</summary>
	/// <param name="name">The name of the property on the attributed type that provides the XAML namescope.</param>
	public NameScopePropertyAttribute(string name)
	{
		Name = name;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.NameScopePropertyAttribute" /> class with the specified name and type.</summary>
	/// <param name="name">The name of the attachable member that provides the XAML name scope.</param>
	/// <param name="type">The owner type of the attachable member that provides the XAML name scope.</param>
	public NameScopePropertyAttribute(string name, Type type)
	{
		Name = name;
		Type = type;
	}
}
