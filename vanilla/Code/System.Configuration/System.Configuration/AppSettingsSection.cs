using System.ComponentModel;
using System.IO;
using System.Xml;

namespace System.Configuration;

/// <summary>Provides configuration system support for the appSettings configuration section. This class cannot be inherited.</summary>
public sealed class AppSettingsSection : ConfigurationSection
{
	private static ConfigurationPropertyCollection _properties;

	private static readonly ConfigurationProperty _propFile;

	private static readonly ConfigurationProperty _propSettings;

	/// <summary>Gets or sets a configuration file that provides additional settings or overrides the settings specified in the appSettings element.</summary>
	/// <returns>A configuration file that provides additional settings or overrides the settings specified in the appSettings element.</returns>
	[ConfigurationProperty("file", DefaultValue = "")]
	public string File
	{
		get
		{
			return (string)base[_propFile];
		}
		set
		{
			base[_propFile] = value;
		}
	}

	/// <summary>Gets a collection of key/value pairs that contains application settings.</summary>
	/// <returns>A collection of key/value pairs that contains the application settings from the configuration file.</returns>
	[ConfigurationProperty("", Options = ConfigurationPropertyOptions.IsDefaultCollection)]
	public KeyValueConfigurationCollection Settings => (KeyValueConfigurationCollection)base[_propSettings];

	protected internal override ConfigurationPropertyCollection Properties => _properties;

	static AppSettingsSection()
	{
		_propFile = new ConfigurationProperty("file", typeof(string), "", new StringConverter(), null, ConfigurationPropertyOptions.None);
		_propSettings = new ConfigurationProperty("", typeof(KeyValueConfigurationCollection), null, null, null, ConfigurationPropertyOptions.IsDefaultCollection);
		_properties = new ConfigurationPropertyCollection();
		_properties.Add(_propFile);
		_properties.Add(_propSettings);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.AppSettingsSection" /> class.</summary>
	public AppSettingsSection()
	{
	}

	protected internal override bool IsModified()
	{
		return Settings.IsModified();
	}

	[System.MonoInternalNote("file path?  do we use a System.Configuration api for opening it?  do we keep it open?  do we open it writable?")]
	protected internal override void DeserializeElement(XmlReader reader, bool serializeCollectionKey)
	{
		base.DeserializeElement(reader, serializeCollectionKey);
		if (!(File != ""))
		{
			return;
		}
		try
		{
			string text = File;
			if (!Path.IsPathRooted(text))
			{
				text = Path.Combine(Path.GetDirectoryName(base.Configuration.FilePath), text);
			}
			FileStream fileStream = System.IO.File.OpenRead(text);
			XmlReader reader2 = new ConfigXmlTextReader(fileStream, text);
			base.DeserializeElement(reader2, serializeCollectionKey);
			fileStream.Close();
		}
		catch
		{
		}
	}

	protected internal override void Reset(ConfigurationElement parentSection)
	{
		if (parentSection is AppSettingsSection appSettingsSection)
		{
			Settings.Reset(appSettingsSection.Settings);
		}
	}

	[System.MonoTODO]
	protected internal override string SerializeSection(ConfigurationElement parentElement, string name, ConfigurationSaveMode saveMode)
	{
		if (File == "")
		{
			return base.SerializeSection(parentElement, name, saveMode);
		}
		throw new NotImplementedException();
	}

	protected internal override object GetRuntimeObject()
	{
		KeyValueInternalCollection keyValueInternalCollection = new KeyValueInternalCollection();
		string[] allKeys = Settings.AllKeys;
		foreach (string key in allKeys)
		{
			KeyValueConfigurationElement keyValueConfigurationElement = Settings[key];
			keyValueInternalCollection.Add(keyValueConfigurationElement.Key, keyValueConfigurationElement.Value);
		}
		if (!ConfigurationManager.ConfigurationSystem.SupportsUserConfig)
		{
			keyValueInternalCollection.SetReadOnly();
		}
		return keyValueInternalCollection;
	}
}
