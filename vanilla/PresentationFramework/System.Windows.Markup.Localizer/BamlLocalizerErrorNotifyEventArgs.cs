namespace System.Windows.Markup.Localizer;

/// <summary>Provides required event data for the <see cref="E:System.Windows.Markup.Localizer.BamlLocalizer.ErrorNotify" /> event.</summary>
public class BamlLocalizerErrorNotifyEventArgs : EventArgs
{
	private BamlLocalizableResourceKey _key;

	private BamlLocalizerError _error;

	/// <summary>Gets the key associated with the resource that generated the error condition.</summary>
	/// <returns>The key associated with the resource that generated the error condition.</returns>
	public BamlLocalizableResourceKey Key => _key;

	/// <summary>Gets the specific error condition encountered by <see cref="T:System.Windows.Markup.Localizer.BamlLocalizer" />.</summary>
	/// <returns>The error condition encountered by <see cref="T:System.Windows.Markup.Localizer.BamlLocalizer" />, as a value of the enumeration.</returns>
	public BamlLocalizerError Error => _error;

	internal BamlLocalizerErrorNotifyEventArgs(BamlLocalizableResourceKey key, BamlLocalizerError error)
	{
		_key = key;
		_error = error;
	}
}
