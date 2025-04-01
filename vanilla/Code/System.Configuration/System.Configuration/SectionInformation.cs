using Unity;

namespace System.Configuration;

/// <summary>Contains metadata about an individual section within the configuration hierarchy. This class cannot be inherited.</summary>
public sealed class SectionInformation
{
	private ConfigurationSection parent;

	private ConfigurationAllowDefinition allow_definition = ConfigurationAllowDefinition.Everywhere;

	private ConfigurationAllowExeDefinition allow_exe_definition = ConfigurationAllowExeDefinition.MachineToApplication;

	private bool allow_location;

	private bool allow_override;

	private bool inherit_on_child_apps;

	private bool restart_on_external_changes;

	private bool require_permission;

	private string config_source = string.Empty;

	private bool force_update;

	private string name;

	private string type_name;

	private string raw_xml;

	private ProtectedConfigurationProvider protection_provider;

	internal string ConfigFilePath { get; set; }

	/// <summary>Gets or sets a value that indicates where in the configuration file hierarchy the associated configuration section can be defined. </summary>
	/// <returns>A value that indicates where in the configuration file hierarchy the associated <see cref="T:System.Configuration.ConfigurationSection" /> object can be declared.</returns>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">The selected value conflicts with a value that is already defined.</exception>
	public ConfigurationAllowDefinition AllowDefinition
	{
		get
		{
			return allow_definition;
		}
		set
		{
			allow_definition = value;
		}
	}

	/// <summary>Gets or sets a value that indicates where in the configuration file hierarchy the associated configuration section can be declared.</summary>
	/// <returns>A value that indicates where in the configuration file hierarchy the associated <see cref="T:System.Configuration.ConfigurationSection" /> object can be declared for .exe files.</returns>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">The selected value conflicts with a value that is already defined.</exception>
	public ConfigurationAllowExeDefinition AllowExeDefinition
	{
		get
		{
			return allow_exe_definition;
		}
		set
		{
			allow_exe_definition = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether the configuration section allows the location attribute.</summary>
	/// <returns>true if the location attribute is allowed; otherwise, false. The default is true.</returns>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">The selected value conflicts with a value that is already defined.</exception>
	public bool AllowLocation
	{
		get
		{
			return allow_location;
		}
		set
		{
			allow_location = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether the associated configuration section can be overridden by lower-level configuration files.</summary>
	/// <returns>true if the section can be overridden; otherwise, false. The default is false.</returns>
	public bool AllowOverride
	{
		get
		{
			return allow_override;
		}
		set
		{
			allow_override = value;
		}
	}

	/// <summary>Gets or sets the name of the include file in which the associated configuration section is defined, if such a file exists.</summary>
	/// <returns>The name of the include file in which the associated <see cref="T:System.Configuration.ConfigurationSection" /> is defined, if such a file exists; otherwise, an empty string ("").</returns>
	public string ConfigSource
	{
		get
		{
			return config_source;
		}
		set
		{
			if (value == null)
			{
				value = string.Empty;
			}
			config_source = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether the associated configuration section will be saved even if it has not been modified.</summary>
	/// <returns>true if the associated <see cref="T:System.Configuration.ConfigurationSection" /> object will be saved even if it has not been modified; otherwise, false. The default is false.NoteIf the configuration file is saved (even if there are no modifications), ASP.NET restarts the application.</returns>
	public bool ForceSave
	{
		get
		{
			return force_update;
		}
		set
		{
			force_update = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether the settings that are specified in the associated configuration section are inherited by applications that reside in a subdirectory of the relevant application.</summary>
	/// <returns>true if the settings specified in this <see cref="T:System.Configuration.ConfigurationSection" /> object are inherited by child applications; otherwise, false. The default is true.</returns>
	public bool InheritInChildApplications
	{
		get
		{
			return inherit_on_child_apps;
		}
		set
		{
			inherit_on_child_apps = value;
		}
	}

	/// <summary>Gets a value that indicates whether the configuration section must be declared in the configuration file.</summary>
	/// <returns>true if the associated <see cref="T:System.Configuration.ConfigurationSection" /> object must be declared in the configuration file; otherwise, false.</returns>
	[System.MonoTODO]
	public bool IsDeclarationRequired
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets a value that indicates whether the associated configuration section is declared in the configuration file.</summary>
	/// <returns>true if this <see cref="T:System.Configuration.ConfigurationSection" /> is declared in the configuration file; otherwise, false. The default is true.</returns>
	[System.MonoTODO]
	public bool IsDeclared => false;

	/// <summary>Gets a value that indicates whether the associated configuration section is locked.</summary>
	/// <returns>true if the section is locked; otherwise, false. </returns>
	[System.MonoTODO]
	public bool IsLocked => false;

	/// <summary>Gets a value that indicates whether the associated configuration section is protected.</summary>
	/// <returns>true if this <see cref="T:System.Configuration.ConfigurationSection" /> is protected; otherwise, false. The default is false.</returns>
	public bool IsProtected => protection_provider != null;

	/// <summary>Gets the name of the associated configuration section.</summary>
	/// <returns>The complete name of the configuration section.</returns>
	public string Name => name;

	/// <summary>Gets the protected configuration provider for the associated configuration section.</summary>
	/// <returns>The protected configuration provider for this <see cref="T:System.Configuration.ConfigurationSection" /> object.</returns>
	public ProtectedConfigurationProvider ProtectionProvider => protection_provider;

	/// <summary>Gets a value that indicates whether the associated configuration section requires access permissions.</summary>
	/// <returns>true if the requirePermission attribute is set to true; otherwise, false. The default is true.</returns>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">The selected value conflicts with a value that is already defined.</exception>
	[System.MonoTODO]
	public bool RequirePermission
	{
		get
		{
			return require_permission;
		}
		set
		{
			require_permission = value;
		}
	}

	/// <summary>Gets or sets a value that specifies whether a change in an external configuration include file requires an application restart.</summary>
	/// <returns>true if a change in an external configuration include file requires an application restart; otherwise, false. The default is true.</returns>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">The selected value conflicts with a value that is already defined.</exception>
	[System.MonoTODO]
	public bool RestartOnExternalChanges
	{
		get
		{
			return restart_on_external_changes;
		}
		set
		{
			restart_on_external_changes = value;
		}
	}

	/// <summary>Gets the name of the associated configuration section.</summary>
	/// <returns>The name of the associated <see cref="T:System.Configuration.ConfigurationSection" /> object.</returns>
	[System.MonoTODO]
	public string SectionName => name;

	/// <summary>Gets or sets the section class name.</summary>
	/// <returns>The name of the class that is associated with this <see cref="T:System.Configuration.ConfigurationSection" /> section.</returns>
	/// <exception cref="T:System.ArgumentException">The selected value is null or an empty string ("").</exception>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">The selected value conflicts with a value that is already defined.</exception>
	public string Type
	{
		get
		{
			return type_name;
		}
		set
		{
			if (value == null || value.Length == 0)
			{
				throw new ArgumentException("Value cannot be null or empty.");
			}
			type_name = value;
		}
	}

	public ConfigurationBuilder ConfigurationBuilder
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Configuration.OverrideMode" /> enumeration value that specifies whether the associated configuration section can be overridden by child configuration files.</summary>
	/// <returns>One of the <see cref="T:System.Configuration.OverrideMode" /> enumeration values.</returns>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">An attempt was made to change both the <see cref="P:System.Configuration.SectionInformation.AllowOverride" /> and <see cref="P:System.Configuration.SectionInformation.OverrideMode" /> properties, which is not supported for compatibility reasons. </exception>
	public OverrideMode OverrideMode
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(OverrideMode);
		}
		set
		{
			Unity.ThrowStub.ThrowNotSupportedException();
		}
	}

	/// <summary>Gets or sets a value that specifies the default override behavior of a configuration section by child configuration files.</summary>
	/// <returns>One of the <see cref="T:System.Configuration.OverrideMode" /> enumeration values.</returns>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">The override behavior is specified in a parent configuration section.</exception>
	public OverrideMode OverrideModeDefault
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(OverrideMode);
		}
		set
		{
			Unity.ThrowStub.ThrowNotSupportedException();
		}
	}

	/// <summary>Gets the override behavior of a configuration section that is in turn based on whether child configuration files can lock the configuration section. </summary>
	/// <returns>One of the <see cref="T:System.Configuration.OverrideMode" /> enumeration values.</returns>
	public OverrideMode OverrideModeEffective
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(OverrideMode);
		}
	}

	[System.MonoTODO("default value for require_permission")]
	internal SectionInformation()
	{
		allow_definition = ConfigurationAllowDefinition.Everywhere;
		allow_location = true;
		allow_override = true;
		inherit_on_child_apps = true;
		restart_on_external_changes = true;
	}

	/// <summary>Gets the configuration section that contains the configuration section associated with this object.</summary>
	/// <returns>The configuration section that contains the <see cref="T:System.Configuration.ConfigurationSection" /> that is associated with this <see cref="T:System.Configuration.SectionInformation" /> object.</returns>
	/// <exception cref="T:System.InvalidOperationException">The method is invoked from a parent section.</exception>
	public ConfigurationSection GetParentSection()
	{
		return parent;
	}

	internal void SetParentSection(ConfigurationSection parent)
	{
		this.parent = parent;
	}

	/// <summary>Returns an XML node object that represents the associated configuration-section object.</summary>
	/// <returns>The XML representation for this configuration section.</returns>
	/// <exception cref="T:System.InvalidOperationException">This configuration object is locked and cannot be edited.</exception>
	public string GetRawXml()
	{
		return raw_xml;
	}

	/// <summary>Marks a configuration section for protection. </summary>
	/// <param name="protectionProvider">The name of the protection provider to use.</param>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Configuration.SectionInformation.AllowLocation" /> property is set to false.- or -The target section is already a protected data section.</exception>
	public void ProtectSection(string protectionProvider)
	{
		protection_provider = ProtectedConfiguration.GetProvider(protectionProvider, throwOnError: true);
	}

	/// <summary>Forces the associated configuration section to appear in the configuration file, or removes an existing section from the configuration file.</summary>
	/// <param name="force">true if the associated section should be written in the configuration file; otherwise, false.</param>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">
	///   <paramref name="force" /> is true and the associated section cannot be exported to the child configuration file, or it is undeclared.</exception>
	[System.MonoTODO]
	public void ForceDeclaration(bool force)
	{
	}

	/// <summary>Forces the associated configuration section to appear in the configuration file.</summary>
	public void ForceDeclaration()
	{
		ForceDeclaration(force: true);
	}

	/// <summary>Causes the associated configuration section to inherit all its values from the parent section.</summary>
	/// <exception cref="T:System.InvalidOperationException">This method cannot be called outside editing mode.</exception>
	[System.MonoTODO]
	public void RevertToParent()
	{
		throw new NotImplementedException();
	}

	/// <summary>Removes the protected configuration encryption from the associated configuration section.</summary>
	public void UnprotectSection()
	{
		protection_provider = null;
	}

	/// <summary>Sets the object to an XML representation of the associated configuration section within the configuration file.</summary>
	/// <param name="rawXml">The XML to use.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="rawXml" /> is null.</exception>
	public void SetRawXml(string rawXml)
	{
		raw_xml = rawXml;
	}

	[System.MonoTODO]
	internal void SetName(string name)
	{
		this.name = name;
	}
}
