namespace System.Xaml;

/// <summary>Represents a service that generates a <see cref="T:System.Xaml.XamlObjectWriter" /> that is based on the current internal parser context.</summary>
public interface IXamlObjectWriterFactory
{
	/// <summary>Returns the <see cref="T:System.Xaml.XamlObjectWriterSettings" /> from the original internal parser context.</summary>
	/// <returns>The settings from the original internal parser context.</returns>
	XamlObjectWriterSettings GetParentSettings();

	/// <summary>Returns a <see cref="T:System.Xaml.XamlObjectWriter" /> that is based on active XAML schema context.</summary>
	/// <returns>A <see cref="T:System.Xaml.XamlObjectWriter" /> that has the specified settings.</returns>
	/// <param name="settings">The settings to use for construction and initialization of the <see cref="T:System.Xaml.XamlObjectWriter" />.</param>
	XamlObjectWriter GetXamlObjectWriter(XamlObjectWriterSettings settings);
}
