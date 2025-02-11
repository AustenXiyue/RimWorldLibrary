using System.Runtime.CompilerServices;

namespace System.Windows.Markup;

/// <summary>Indicates the CLR property of a class that provides the x:Uid Directive value.</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
[TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
public sealed class UidPropertyAttribute : Attribute
{
	private string _name;

	/// <summary>Gets the name of the CLR property that represents the x:Uid Directive value.</summary>
	/// <returns>The name of the CLR property that represents x:Uid Directive.</returns>
	public string Name => _name;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.UidPropertyAttribute" /> class.</summary>
	/// <param name="name">The name of the property that provides the x:Uid value.</param>
	public UidPropertyAttribute(string name)
	{
		_name = name;
	}
}
