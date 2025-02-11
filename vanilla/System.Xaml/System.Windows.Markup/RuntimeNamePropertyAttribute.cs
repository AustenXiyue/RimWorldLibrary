using System.Runtime.CompilerServices;

namespace System.Windows.Markup;

/// <summary>Represents a type-level attribute that reports which property of the type maps to the XAML x:Name attribute.</summary>
[AttributeUsage(AttributeTargets.Class)]
[TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
public sealed class RuntimeNamePropertyAttribute : Attribute
{
	private string _name;

	/// <summary>Gets the name of the runtime name property that is specified by this <see cref="T:System.Windows.Markup.RuntimeNamePropertyAttribute" />.</summary>
	/// <returns>The name of the property.</returns>
	public string Name => _name;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.RuntimeNamePropertyAttribute" /> class.</summary>
	/// <param name="name">The name of the property to use as the x:Name equivalent of the class.</param>
	public RuntimeNamePropertyAttribute(string name)
	{
		_name = name;
	}
}
