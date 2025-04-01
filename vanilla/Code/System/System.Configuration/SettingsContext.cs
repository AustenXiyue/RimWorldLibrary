using System.Collections;

namespace System.Configuration;

/// <summary>Provides contextual information that the provider can use when persisting settings.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
public class SettingsContext : Hashtable
{
	[NonSerialized]
	private ApplicationSettingsBase current;

	internal ApplicationSettingsBase CurrentSettings
	{
		get
		{
			return current;
		}
		set
		{
			current = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.SettingsContext" /> class. </summary>
	public SettingsContext()
	{
	}
}
