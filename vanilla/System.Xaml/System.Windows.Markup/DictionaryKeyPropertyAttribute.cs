using System.Runtime.CompilerServices;

namespace System.Windows.Markup;

/// <summary>Specifies a property of the associated class that provides the implicit key value. Implicit keys are used for keys rather than explicit x:Key attributes defined in XAML for an item in <see cref="T:System.Collections.IDictionary" /> collections.</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
[TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
public sealed class DictionaryKeyPropertyAttribute : Attribute
{
	/// <summary>Gets the name of the property that provides the implicit key value.</summary>
	/// <returns>The name of the property that provides the implicit key value.</returns>
	public string Name { get; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.DictionaryKeyPropertyAttribute" /> class.</summary>
	/// <param name="name">The name of the property that provides the implicit key value.</param>
	public DictionaryKeyPropertyAttribute(string name)
	{
		Name = name;
	}
}
