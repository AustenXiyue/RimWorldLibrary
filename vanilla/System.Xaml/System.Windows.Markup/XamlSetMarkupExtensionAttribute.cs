namespace System.Windows.Markup;

/// <summary>Indicates that a class can use a markup extension to provide a value, and references a handler to use for markup extension set operations.</summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class XamlSetMarkupExtensionAttribute : Attribute
{
	/// <summary>Gets the name of the handler to use for markup extension set operations.</summary>
	/// <returns>The name of the handler to use for markup extension set operations.</returns>
	public string XamlSetMarkupExtensionHandler { get; private set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.XamlSetMarkupExtensionAttribute" /> class. </summary>
	/// <param name="xamlSetMarkupExtensionHandler">The name of the handler to use for markup extension set operations.</param>
	public XamlSetMarkupExtensionAttribute(string xamlSetMarkupExtensionHandler)
	{
		XamlSetMarkupExtensionHandler = xamlSetMarkupExtensionHandler;
	}
}
