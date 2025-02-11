namespace System.Windows;

/// <summary>Declares a namescope contract for framework elements.</summary>
public interface IFrameworkInputElement : IInputElement
{
	/// <summary>Gets or sets the name of an element. </summary>
	/// <returns>The element name, which is unique in the namescope and can be used as an identifier for certain operations.</returns>
	string Name { get; set; }
}
