namespace System.Xaml;

/// <summary>Specifies processing rules or option settings for a <see cref="T:System.Xaml.XamlObjectReader" />.</summary>
public class XamlObjectReaderSettings : XamlReaderSettings
{
	/// <summary>Gets or sets a value that determines whether writers that use the associated <see cref="T:System.Xaml.XamlObjectReader" /> for context should use designer settings for writing content explicitly.</summary>
	/// <returns>true to specify that writers that use this context should use designer settings for writing any output content in cases where <see cref="P:System.Xaml.XamlMember.IsWritePublic" /> reports false; false if designer settings should be ignored.</returns>
	public bool RequireExplicitContentVisibility { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlObjectReaderSettings" /> class.</summary>
	public XamlObjectReaderSettings()
	{
	}
}
