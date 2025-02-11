using System.Xaml;

namespace System.Windows.Baml2006;

internal class Baml2006ReaderSettings : XamlReaderSettings
{
	internal bool OwnsStream { get; set; }

	internal bool IsBamlFragment { get; set; }

	public Baml2006ReaderSettings()
	{
	}

	public Baml2006ReaderSettings(Baml2006ReaderSettings settings)
		: base(settings)
	{
		OwnsStream = settings.OwnsStream;
		IsBamlFragment = settings.IsBamlFragment;
	}

	public Baml2006ReaderSettings(XamlReaderSettings settings)
		: base(settings)
	{
	}
}
