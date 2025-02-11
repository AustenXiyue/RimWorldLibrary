namespace System.Windows.Markup;

/// <summary>Provides a mechanism whereby types can declare that they can receive an expression (or another class) from a markup extension, where the output is a different property type than the target property. Do not use for .NET Framework 4 implementations; see Remarks.</summary>
[Obsolete("IReceiveMarkupExtension has been deprecated. This interface is no longer in use.")]
public interface IReceiveMarkupExtension
{
	/// <summary>Provides the handling for markup extensions that provide property values. Do not use for .NET Framework 4 implementations; see Remarks in <see cref="T:System.Windows.Markup.IReceiveMarkupExtension" />.</summary>
	/// <param name="property">The name of the target property.</param>
	/// <param name="markupExtension">The markup extension instance of the incoming data.</param>
	/// <param name="serviceProvider">Can provide additional services that should be performed when processing the markup extension data for a property value.</param>
	void ReceiveMarkupExtension(string property, MarkupExtension markupExtension, IServiceProvider serviceProvider);
}
