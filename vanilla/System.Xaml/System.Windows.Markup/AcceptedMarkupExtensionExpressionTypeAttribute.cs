namespace System.Windows.Markup;

/// <summary>Notates types for legacy reporting of XAML markup extension characteristics.</summary>
[Obsolete("This is not used by the XAML parser. Please look at XamlSetMarkupExtensionAttribute.")]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class AcceptedMarkupExtensionExpressionTypeAttribute : Attribute
{
	/// <summary>Gets or sets the return type that this attribute reports. </summary>
	/// <returns>The return type that this attribute reports. </returns>
	public Type Type { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.AcceptedMarkupExtensionExpressionTypeAttribute" /> class. </summary>
	/// <param name="type">The return type that this attribute reports. </param>
	public AcceptedMarkupExtensionExpressionTypeAttribute(Type type)
	{
		Type = type;
	}
}
