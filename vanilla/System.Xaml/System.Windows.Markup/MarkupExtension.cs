using System.Runtime.CompilerServices;

namespace System.Windows.Markup;

/// <summary>Provides a base class for XAML markup extension implementations that can be supported by .NET Framework XAML Services and other XAML readers and XAML writers.</summary>
[TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
public abstract class MarkupExtension
{
	/// <summary>When implemented in a derived class, returns an object that is provided as the value of the target property for this markup extension. </summary>
	/// <returns>The object value to set on the property where the extension is applied. </returns>
	/// <param name="serviceProvider">A service provider helper that can provide services for the markup extension.</param>
	public abstract object ProvideValue(IServiceProvider serviceProvider);

	/// <summary>Initializes a new instance of a class derived from <see cref="T:System.Windows.Markup.MarkupExtension" />. </summary>
	protected MarkupExtension()
	{
	}
}
