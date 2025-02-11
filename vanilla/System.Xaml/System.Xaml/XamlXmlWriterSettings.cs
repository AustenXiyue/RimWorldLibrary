namespace System.Xaml;

/// <summary>Provides initialization settings for the <see cref="T:System.Xaml.XamlXmlWriter" /> XAML writer implementation.</summary>
public class XamlXmlWriterSettings : XamlWriterSettings
{
	/// <summary>Gets or sets a value that specifies whether the <see cref="T:System.Xaml.XamlXmlWriter" /> should always assume valid XAML input for purposes of duplicate resolution or other error checking.</summary>
	/// <returns>true if the <see cref="T:System.Xaml.XamlXmlWriter" /> skips certain validation or error checks, such as throwing exceptions on duplicate members. false if the <see cref="T:System.Xaml.XamlXmlWriter" /> throws exceptions when invalid XAML is encountered. The default is false.</returns>
	public bool AssumeValidInput { get; set; }

	/// <summary>Gets or sets a value that specifies whether the <see cref="T:System.Xaml.XamlXmlWriter" /> should close immediately on Dispose or other operations, or whether the XAML writer should instead write the buffer output before closing. Use this setting with caution; closing immediately can result in invalid XAML that cannot be loaded again.</summary>
	/// <returns>true if <see cref="T:System.Xaml.XamlXmlWriter" /> immediately closes on a Dispose or similar operations. false if the remaining buffer output is written before the <see cref="T:System.Xaml.XamlXmlWriter" /> is released. The default is false.</returns>
	public bool CloseOutput { get; set; }

	/// <summary>Returns a copy of this <see cref="T:System.Xaml.XamlXmlWriterSettings" /> instance.</summary>
	/// <returns>The returned copy.</returns>
	public XamlXmlWriterSettings Copy()
	{
		return new XamlXmlWriterSettings
		{
			AssumeValidInput = AssumeValidInput,
			CloseOutput = CloseOutput
		};
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlXmlWriterSettings" /> class.</summary>
	public XamlXmlWriterSettings()
	{
	}
}
