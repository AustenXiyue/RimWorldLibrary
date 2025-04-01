using Unity;

namespace System.Configuration;

/// <summary>Defines the configuration file mapping for an .exe application. This class cannot be inherited.</summary>
public sealed class ExeConfigurationFileMap : ConfigurationFileMap
{
	private string exeConfigFilename;

	private string localUserConfigFilename;

	private string roamingUserConfigFilename;

	/// <summary>Gets or sets the name of the configuration file.</summary>
	/// <returns>The configuration file name.</returns>
	public string ExeConfigFilename
	{
		get
		{
			return exeConfigFilename;
		}
		set
		{
			exeConfigFilename = value;
		}
	}

	/// <summary>Gets or sets the name of the configuration file for the local user.</summary>
	/// <returns>The configuration file name.</returns>
	public string LocalUserConfigFilename
	{
		get
		{
			return localUserConfigFilename;
		}
		set
		{
			localUserConfigFilename = value;
		}
	}

	/// <summary>Gets or sets the name of the configuration file for the roaming user.</summary>
	/// <returns>The configuration file name.</returns>
	public string RoamingUserConfigFilename
	{
		get
		{
			return roamingUserConfigFilename;
		}
		set
		{
			roamingUserConfigFilename = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.ExeConfigurationFileMap" /> class.</summary>
	public ExeConfigurationFileMap()
	{
		exeConfigFilename = "";
		localUserConfigFilename = "";
		roamingUserConfigFilename = "";
	}

	/// <summary>Creates a copy of the existing <see cref="T:System.Configuration.ExeConfigurationFileMap" /> object.</summary>
	/// <returns>An <see cref="T:System.Configuration.ExeConfigurationFileMap" /> object.</returns>
	public override object Clone()
	{
		return new ExeConfigurationFileMap
		{
			exeConfigFilename = exeConfigFilename,
			localUserConfigFilename = localUserConfigFilename,
			roamingUserConfigFilename = roamingUserConfigFilename,
			MachineConfigFilename = base.MachineConfigFilename
		};
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.ExeConfigurationFileMap" /> class by using the specified machine configuration file name.</summary>
	/// <param name="machineConfigFileName">The name of the machine configuration file that includes the complete physical path (for example, c:\Windows\Microsoft.NET\Framework\v2.0.50727\CONFIG\machine.config).</param>
	public ExeConfigurationFileMap(string machineConfigFileName)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
