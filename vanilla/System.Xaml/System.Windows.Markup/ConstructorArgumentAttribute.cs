using System.Runtime.CompilerServices;

namespace System.Windows.Markup;

/// <summary>Specifies that an object can be initialized by using a non-default constructor syntax, and that a property of the specified name supplies construction information.  This information is primarily for XAML serialization.</summary>
[TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class ConstructorArgumentAttribute : Attribute
{
	/// <summary>Gets the name parameter of the constructor that will initialize the associated property.</summary>
	/// <returns>The name of the constructor. Assuming CLR backing, this corresponds to the <see cref="P:System.Reflection.ParameterInfo.Name" /> of the relevant constructor parameter.</returns>
	public string ArgumentName { get; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.ConstructorArgumentAttribute" /> class.</summary>
	/// <param name="argumentName">The name of the constructor that will initialize the associated property.</param>
	public ConstructorArgumentAttribute(string argumentName)
	{
		ArgumentName = argumentName;
	}
}
