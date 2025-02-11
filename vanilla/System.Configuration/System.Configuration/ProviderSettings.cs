using System.Collections.Specialized;

namespace System.Configuration;

/// <summary>Represents the configuration elements associated with a provider.</summary>
public sealed class ProviderSettings : ConfigurationElement
{
	private System.Configuration.ConfigNameValueCollection parameters;

	private static ConfigurationProperty nameProp;

	private static ConfigurationProperty typeProp;

	private static ConfigurationPropertyCollection properties;

	/// <summary>Gets or sets the name of the provider configured by this class.</summary>
	/// <returns>The name of the provider.</returns>
	[ConfigurationProperty("name", Options = (ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey))]
	public string Name
	{
		get
		{
			return (string)base[nameProp];
		}
		set
		{
			base[nameProp] = value;
		}
	}

	/// <summary>Gets or sets the type of the provider configured by this class.</summary>
	/// <returns>The fully qualified namespace and class name for the type of provider configured by this <see cref="T:System.Configuration.ProviderSettings" /> instance.</returns>
	[ConfigurationProperty("type", Options = ConfigurationPropertyOptions.IsRequired)]
	public string Type
	{
		get
		{
			return (string)base[typeProp];
		}
		set
		{
			base[typeProp] = value;
		}
	}

	protected internal override ConfigurationPropertyCollection Properties => properties;

	/// <summary>Gets a collection of user-defined parameters for the provider.</summary>
	/// <returns>A <see cref="T:System.Collections.Specialized.NameValueCollection" /> of parameters for the provider.</returns>
	public NameValueCollection Parameters
	{
		get
		{
			if (parameters == null)
			{
				parameters = new System.Configuration.ConfigNameValueCollection();
			}
			return parameters;
		}
	}

	static ProviderSettings()
	{
		nameProp = new ConfigurationProperty("name", typeof(string), null, ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);
		typeProp = new ConfigurationProperty("type", typeof(string), null, ConfigurationPropertyOptions.IsRequired);
		properties = new ConfigurationPropertyCollection();
		properties.Add(nameProp);
		properties.Add(typeProp);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.ProviderSettings" /> class. </summary>
	public ProviderSettings()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.ProviderSettings" /> class. </summary>
	/// <param name="name">The name of the provider to configure settings for.</param>
	/// <param name="type">The type of the provider to configure settings for.</param>
	public ProviderSettings(string name, string type)
	{
		Name = name;
		Type = type;
	}

	protected override bool OnDeserializeUnrecognizedAttribute(string name, string value)
	{
		if (parameters == null)
		{
			parameters = new System.Configuration.ConfigNameValueCollection();
		}
		parameters[name] = value;
		parameters.ResetModified();
		return true;
	}

	protected internal override bool IsModified()
	{
		if (parameters == null || !parameters.IsModified)
		{
			return base.IsModified();
		}
		return true;
	}

	protected internal override void Reset(ConfigurationElement parentElement)
	{
		base.Reset(parentElement);
		if (parentElement is ProviderSettings { parameters: not null } providerSettings)
		{
			parameters = new System.Configuration.ConfigNameValueCollection(providerSettings.parameters);
		}
		else
		{
			parameters = null;
		}
	}

	[System.MonoTODO]
	protected internal override void Unmerge(ConfigurationElement sourceElement, ConfigurationElement parentElement, ConfigurationSaveMode saveMode)
	{
		base.Unmerge(sourceElement, parentElement, saveMode);
	}
}
