using System.Runtime.CompilerServices;

namespace System.Windows.Markup;

/// <summary>Indicates that the attributed property is dependent on the value of another property.</summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
[TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
public sealed class DependsOnAttribute : Attribute
{
	/// <summary>Gets a unique identifier for this <see cref="T:System.Windows.Markup.DependsOnAttribute" />. </summary>
	/// <returns>The unique identifier.</returns>
	public override object TypeId => this;

	/// <summary>Gets the name of the related property declared in this <see cref="T:System.Windows.Markup.DependsOnAttribute" />.</summary>
	/// <returns>The name of the related property.</returns>
	public string Name { get; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.DependsOnAttribute" /> class.</summary>
	/// <param name="name">The property that the property associated with this <see cref="T:System.Windows.Markup.DependsOnAttribute" /> depends on.</param>
	public DependsOnAttribute(string name)
	{
		Name = name;
	}
}
