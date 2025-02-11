using System.Runtime.CompilerServices;

namespace System.Windows.Markup;

/// <summary>Implements a XAML markup extension in order to return a null object, which you can use to explicitly set values to null in XAML. </summary>
[TypeForwardedFrom("PresentationFramework, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
[MarkupExtensionReturnType(typeof(object))]
public class NullExtension : MarkupExtension
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.NullExtension" /> class. </summary>
	public NullExtension()
	{
	}

	/// <summary>Provides null to use as a value as the output of this markup extension. </summary>
	/// <returns>A null reference.</returns>
	/// <param name="serviceProvider">An object that can provide services for the markup extension implementation.</param>
	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		return null;
	}
}
