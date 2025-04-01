namespace System.Configuration;

/// <summary>Represents a simplified configuration element used for updating elements in the configuration. This class cannot be inherited.</summary>
/// <filterpriority>2</filterpriority>
public sealed class SettingElement : ConfigurationElement
{
	private static ConfigurationPropertyCollection properties;

	private static ConfigurationProperty name_prop;

	private static ConfigurationProperty serialize_as_prop;

	private static ConfigurationProperty value_prop;

	/// <summary>Gets or sets the name of the <see cref="T:System.Configuration.SettingElement" /> object.</summary>
	/// <returns>The name of the <see cref="T:System.Configuration.SettingElement" /> object.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[ConfigurationProperty("name", DefaultValue = "", Options = (ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey))]
	public string Name
	{
		get
		{
			return (string)base[name_prop];
		}
		set
		{
			base[name_prop] = value;
		}
	}

	/// <summary>Gets or sets the value of a <see cref="T:System.Configuration.SettingElement" /> object by using a <see cref="T:System.Configuration.SettingValueElement" /> object.</summary>
	/// <returns>A <see cref="T:System.Configuration.SettingValueElement" /> object containing the value of the <see cref="T:System.Configuration.SettingElement" /> object.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[ConfigurationProperty("value", DefaultValue = null, Options = ConfigurationPropertyOptions.IsRequired)]
	public SettingValueElement Value
	{
		get
		{
			return (SettingValueElement)base[value_prop];
		}
		set
		{
			base[value_prop] = value;
		}
	}

	/// <summary>Gets or sets the serialization mechanism used to persist the values of the <see cref="T:System.Configuration.SettingElement" /> object.</summary>
	/// <returns>A <see cref="T:System.Configuration.SettingsSerializeAs" /> object.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[ConfigurationProperty("serializeAs", DefaultValue = SettingsSerializeAs.String, Options = ConfigurationPropertyOptions.IsRequired)]
	public SettingsSerializeAs SerializeAs
	{
		get
		{
			if (base[serialize_as_prop] == null)
			{
				return SettingsSerializeAs.String;
			}
			return (SettingsSerializeAs)base[serialize_as_prop];
		}
		set
		{
			base[serialize_as_prop] = value;
		}
	}

	protected override ConfigurationPropertyCollection Properties => properties;

	static SettingElement()
	{
		name_prop = new ConfigurationProperty("name", typeof(string), string.Empty, ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);
		serialize_as_prop = new ConfigurationProperty("serializeAs", typeof(SettingsSerializeAs), null, ConfigurationPropertyOptions.IsRequired);
		value_prop = new ConfigurationProperty("value", typeof(SettingValueElement), null, ConfigurationPropertyOptions.IsRequired);
		properties = new ConfigurationPropertyCollection();
		properties.Add(name_prop);
		properties.Add(serialize_as_prop);
		properties.Add(value_prop);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.SettingElement" /> class.</summary>
	public SettingElement()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.SettingElement" /> class based on supplied parameters.</summary>
	/// <param name="name">The name of the <see cref="T:System.Configuration.SettingElement" /> object.</param>
	/// <param name="serializeAs">A <see cref="T:System.Configuration.SettingsSerializeAs" /> object. This object is an enumeration used as the serialization scheme to store configuration settings.</param>
	public SettingElement(string name, SettingsSerializeAs serializeAs)
	{
		Name = name;
		SerializeAs = serializeAs;
	}

	/// <summary>Compares the current <see cref="T:System.Configuration.SettingElement" /> instance to the specified object.</summary>
	/// <returns>true if the <see cref="T:System.Configuration.SettingElement" /> instance is equal to the specified object; otherwise, false.</returns>
	/// <param name="settings">The object to compare with.</param>
	/// <filterpriority>2</filterpriority>
	public override bool Equals(object settings)
	{
		if (!(settings is SettingElement settingElement))
		{
			return false;
		}
		if (settingElement.SerializeAs == SerializeAs && settingElement.Value == Value)
		{
			return settingElement.Name == Name;
		}
		return false;
	}

	/// <summary>Gets a unique value representing the <see cref="T:System.Configuration.SettingElement" /> current instance.</summary>
	/// <returns>A unique value representing the <see cref="T:System.Configuration.SettingElement" /> current instance.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public override int GetHashCode()
	{
		int num = (int)(SerializeAs ^ (SettingsSerializeAs)127);
		if (Name != null)
		{
			num += Name.GetHashCode() ^ 0x7F;
		}
		if (Value != null)
		{
			num += Value.GetHashCode();
		}
		return num;
	}
}
