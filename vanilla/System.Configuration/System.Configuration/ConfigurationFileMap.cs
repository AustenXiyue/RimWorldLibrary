using System.Runtime.InteropServices;

namespace System.Configuration;

/// <summary>Defines the configuration file mapping for the machine configuration file. </summary>
public class ConfigurationFileMap : ICloneable
{
	private string machineConfigFilename;

	/// <summary>Gets or sets the name of the machine configuration file name.</summary>
	/// <returns>The machine configuration file name.</returns>
	public string MachineConfigFilename
	{
		get
		{
			return machineConfigFilename;
		}
		set
		{
			machineConfigFilename = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.ConfigurationFileMap" /> class. </summary>
	public ConfigurationFileMap()
	{
		machineConfigFilename = RuntimeEnvironment.SystemConfigurationFile;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.ConfigurationFileMap" /> class based on the supplied parameter.</summary>
	/// <param name="machineConfigFilename">The name of the machine configuration file.</param>
	public ConfigurationFileMap(string machineConfigFilename)
	{
		this.machineConfigFilename = machineConfigFilename;
	}

	/// <summary>Creates a copy of the existing <see cref="T:System.Configuration.ConfigurationFileMap" /> object.</summary>
	/// <returns>A <see cref="T:System.Configuration.ConfigurationFileMap" /> object.</returns>
	public virtual object Clone()
	{
		return new ConfigurationFileMap(machineConfigFilename);
	}
}
