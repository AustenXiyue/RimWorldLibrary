using System.Configuration;

namespace System.Net.Configuration;

/// <summary>Represents the performance counter element in the System.Net configuration file that determines whether networking performance counters are enabled. This class cannot be inherited.</summary>
public sealed class PerformanceCountersElement : ConfigurationElement
{
	private static ConfigurationProperty enabledProp;

	private static ConfigurationPropertyCollection properties;

	/// <summary>Gets or sets whether performance counters are enabled.</summary>
	/// <returns>true if performance counters are enabled; otherwise, false.</returns>
	[ConfigurationProperty("enabled", DefaultValue = "False")]
	public bool Enabled
	{
		get
		{
			return (bool)base[enabledProp];
		}
		set
		{
			base[enabledProp] = value;
		}
	}

	protected override ConfigurationPropertyCollection Properties => properties;

	static PerformanceCountersElement()
	{
		enabledProp = new ConfigurationProperty("enabled", typeof(bool), false);
		properties = new ConfigurationPropertyCollection();
		properties.Add(enabledProp);
	}

	/// <summary>Instantiates a <see cref="T:System.Net.Configuration.PerformanceCountersElement" /> object.</summary>
	public PerformanceCountersElement()
	{
	}
}
