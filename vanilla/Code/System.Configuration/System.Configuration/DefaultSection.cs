using System.Xml;

namespace System.Configuration;

/// <summary>Represents a basic configuration-section handler that exposes the configuration section's XML for both read and write access.</summary>
public sealed class DefaultSection : ConfigurationSection
{
	private static ConfigurationPropertyCollection properties;

	protected internal override ConfigurationPropertyCollection Properties => properties;

	static DefaultSection()
	{
		properties = new ConfigurationPropertyCollection();
	}

	protected internal override void DeserializeSection(XmlReader xmlReader)
	{
		if (base.RawXml == null)
		{
			base.RawXml = xmlReader.ReadOuterXml();
		}
		else
		{
			xmlReader.Skip();
		}
	}

	[System.MonoTODO]
	protected internal override bool IsModified()
	{
		return base.IsModified();
	}

	[System.MonoTODO]
	protected internal override void Reset(ConfigurationElement parentSection)
	{
		base.Reset(parentSection);
	}

	[System.MonoTODO]
	protected internal override void ResetModified()
	{
		base.ResetModified();
	}

	[System.MonoTODO]
	protected internal override string SerializeSection(ConfigurationElement parentSection, string name, ConfigurationSaveMode saveMode)
	{
		return base.SerializeSection(parentSection, name, saveMode);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.DefaultSection" /> class. </summary>
	public DefaultSection()
	{
	}
}
