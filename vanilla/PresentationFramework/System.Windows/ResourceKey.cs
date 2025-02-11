using System.Reflection;
using System.Windows.Markup;

namespace System.Windows;

/// <summary>Provides an abstract base class for various resource keys. </summary>
[MarkupExtensionReturnType(typeof(ResourceKey))]
public abstract class ResourceKey : MarkupExtension
{
	/// <summary>Gets an assembly object that indicates which assembly's dictionary to look in for the value associated with this key. </summary>
	/// <returns>The retrieved assembly, as a reflection class.</returns>
	public abstract Assembly Assembly { get; }

	/// <summary>Returns this <see cref="T:System.Windows.ResourceKey" />. Instances of this class are typically used as a key in a dictionary. </summary>
	/// <returns>Calling this method always returns the instance itself.</returns>
	/// <param name="serviceProvider">A service implementation that provides the desired value.</param>
	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		return this;
	}

	/// <summary> Initializes a new instance of a class derived from <see cref="T:System.Windows.ResourceKey" />. </summary>
	protected ResourceKey()
	{
	}
}
