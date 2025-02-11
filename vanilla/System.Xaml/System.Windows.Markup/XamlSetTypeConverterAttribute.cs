namespace System.Windows.Markup;

/// <summary>Indicates that a class can use a type converter to provide a value, and references a handler to use for type converter setting cases.</summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class XamlSetTypeConverterAttribute : Attribute
{
	/// <summary>Gets the <paramref name="xamlSetTypeConverterHandler" /> initialization value (the handler name) specified in the <see cref="T:System.Windows.Markup.XamlSetTypeConverterAttribute" />.</summary>
	/// <returns>The <paramref name="xamlSetTypeConverterHandler" /> value specified in the <see cref="T:System.Windows.Markup.XamlSetTypeConverterAttribute" />.</returns>
	public string XamlSetTypeConverterHandler { get; private set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.XamlSetTypeConverterAttribute" /> class.</summary>
	/// <param name="xamlSetTypeConverterHandler">The name of the handler to use for type converter setting operations.</param>
	public XamlSetTypeConverterAttribute(string xamlSetTypeConverterHandler)
	{
		XamlSetTypeConverterHandler = xamlSetTypeConverterHandler;
	}
}
