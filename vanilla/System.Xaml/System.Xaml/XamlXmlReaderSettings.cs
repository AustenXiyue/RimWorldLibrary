using System.Collections.Generic;

namespace System.Xaml;

/// <summary>Specifies processing rules or option settings for the <see cref="T:System.Xaml.XamlXmlReader" /> XAML reader implementation.</summary>
public class XamlXmlReaderSettings : XamlReaderSettings
{
	internal Dictionary<string, string> _xmlnsDictionary;

	/// <summary>Gets or sets the language setting value that the reader may promote to writers that write xml:lang attributes in nodes.</summary>
	/// <returns>The string to use for possible xml:lang output that is based on the reader.</returns>
	public string XmlLang { get; set; }

	/// <summary>Gets or sets a value that determines whether the XAML reader instructs any XAML writers to write xml:space attributes in nodes. If that behavior is desired, this information is passed through shared XAML schema context.</summary>
	/// <returns>true if writers that are processing the XAML node stream can write xml:space="preserve" in output; false if xml:space attributes cannot be written in nodes.</returns>
	public bool XmlSpacePreserve { get; set; }

	/// <summary>Gets or sets a value that determines whether the reader should differ from the default <see cref="T:System.Xaml.XamlXmlReader" /> behavior of how markup compatibility content is processed.</summary>
	/// <returns>true if the initiating reader is directly used, which means that XML compatibility markup is processed as part of the main stream and compatibility is not considered. false if the default behavior is used, which processes XML compatibility separately. The default is false.</returns>
	public bool SkipXmlCompatibilityProcessing { get; set; }

	/// <summary>Gets or sets a value that indicates whether the underlying stream or text reader should be closed when the <see cref="T:System.Xaml.XamlXmlReader" /> is closed.</summary>
	/// <returns>true if the underlying stream or reader should be closed when the <see cref="T:System.Xaml.XamlXmlReader" /> is closed; otherwise, false. The default is false.</returns>
	public bool CloseInput { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlXmlReaderSettings" /> class.</summary>
	public XamlXmlReaderSettings()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlXmlReaderSettings" /> class by copying settings from an existing <see cref="T:System.Xaml.XamlXmlReaderSettings" /> object.</summary>
	/// <param name="settings">The existing <see cref="T:System.Xaml.XamlXmlReaderSettings" /> object to copy.</param>
	public XamlXmlReaderSettings(XamlXmlReaderSettings settings)
		: base(settings)
	{
		if (settings != null)
		{
			if (settings._xmlnsDictionary != null)
			{
				_xmlnsDictionary = new Dictionary<string, string>(settings._xmlnsDictionary);
			}
			XmlLang = settings.XmlLang;
			XmlSpacePreserve = settings.XmlSpacePreserve;
			SkipXmlCompatibilityProcessing = settings.SkipXmlCompatibilityProcessing;
			CloseInput = settings.CloseInput;
		}
	}
}
