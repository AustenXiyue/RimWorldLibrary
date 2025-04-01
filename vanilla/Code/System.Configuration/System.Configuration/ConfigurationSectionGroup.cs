using System.Runtime.Versioning;
using Unity;

namespace System.Configuration;

/// <summary>Represents a group of related sections within a configuration file.</summary>
public class ConfigurationSectionGroup
{
	private bool require_declaration;

	private string name;

	private string type_name;

	private ConfigurationSectionCollection sections;

	private ConfigurationSectionGroupCollection groups;

	private Configuration config;

	private SectionGroupInfo group;

	private bool initialized;

	private Configuration Config
	{
		get
		{
			if (config == null)
			{
				throw new InvalidOperationException("ConfigurationSectionGroup cannot be edited until it is added to a Configuration instance as its descendant");
			}
			return config;
		}
	}

	/// <summary>Gets a value that indicates whether this <see cref="T:System.Configuration.ConfigurationSectionGroup" /> object is declared.</summary>
	/// <returns>true if this <see cref="T:System.Configuration.ConfigurationSectionGroup" /> is declared; otherwise, false. The default is false.</returns>
	[System.MonoTODO]
	public bool IsDeclared => false;

	/// <summary>Gets a value that indicates whether this <see cref="T:System.Configuration.ConfigurationSectionGroup" /> object declaration is required. </summary>
	/// <returns>true if this <see cref="T:System.Configuration.ConfigurationSectionGroup" /> declaration is required; otherwise, false.</returns>
	[System.MonoTODO]
	public bool IsDeclarationRequired => require_declaration;

	/// <summary>Gets the name property of this <see cref="T:System.Configuration.ConfigurationSectionGroup" /> object.</summary>
	/// <returns>The name property of this <see cref="T:System.Configuration.ConfigurationSectionGroup" /> object.</returns>
	public string Name => name;

	/// <summary>Gets the section group name associated with this <see cref="T:System.Configuration.ConfigurationSectionGroup" />.</summary>
	/// <returns>The section group name of this <see cref="T:System.Configuration.ConfigurationSectionGroup" /> object.</returns>
	[System.MonoInternalNote("Check if this is correct")]
	public string SectionGroupName => group.XPath;

	/// <summary>Gets a <see cref="T:System.Configuration.ConfigurationSectionGroupCollection" /> object that contains all the <see cref="T:System.Configuration.ConfigurationSectionGroup" /> objects that are children of this <see cref="T:System.Configuration.ConfigurationSectionGroup" /> object.</summary>
	/// <returns>A <see cref="T:System.Configuration.ConfigurationSectionGroupCollection" /> object that contains all the <see cref="T:System.Configuration.ConfigurationSectionGroup" /> objects that are children of this <see cref="T:System.Configuration.ConfigurationSectionGroup" /> object.</returns>
	public ConfigurationSectionGroupCollection SectionGroups
	{
		get
		{
			if (groups == null)
			{
				groups = new ConfigurationSectionGroupCollection(Config, group);
			}
			return groups;
		}
	}

	/// <summary>Gets a <see cref="T:System.Configuration.ConfigurationSectionCollection" /> object that contains all of <see cref="T:System.Configuration.ConfigurationSection" /> objects within this <see cref="T:System.Configuration.ConfigurationSectionGroup" /> object.</summary>
	/// <returns>A <see cref="T:System.Configuration.ConfigurationSectionCollection" /> object that contains all the <see cref="T:System.Configuration.ConfigurationSection" /> objects within this <see cref="T:System.Configuration.ConfigurationSectionGroup" /> object.</returns>
	public ConfigurationSectionCollection Sections
	{
		get
		{
			if (sections == null)
			{
				sections = new ConfigurationSectionCollection(Config, group);
			}
			return sections;
		}
	}

	/// <summary>Gets or sets the type for this <see cref="T:System.Configuration.ConfigurationSectionGroup" /> object.</summary>
	/// <returns>The type of this <see cref="T:System.Configuration.ConfigurationSectionGroup" /> object.</returns>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Configuration.ConfigurationSectionGroup" /> object is the root section group.- or -The <see cref="T:System.Configuration.ConfigurationSectionGroup" /> object has a location.</exception>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">The section or group is already defined at another level.</exception>
	public string Type
	{
		get
		{
			return type_name;
		}
		set
		{
			type_name = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.ConfigurationSectionGroup" /> class. </summary>
	public ConfigurationSectionGroup()
	{
	}

	internal void Initialize(Configuration config, SectionGroupInfo group)
	{
		if (initialized)
		{
			throw new SystemException("INTERNAL ERROR: this configuration section is being initialized twice: " + GetType());
		}
		initialized = true;
		this.config = config;
		this.group = group;
	}

	internal void SetName(string name)
	{
		this.name = name;
	}

	/// <summary>Forces the declaration for this <see cref="T:System.Configuration.ConfigurationSectionGroup" /> object.</summary>
	/// <param name="force">true if the <see cref="T:System.Configuration.ConfigurationSectionGroup" /> object must be written to the file; otherwise, false.</param>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Configuration.ConfigurationSectionGroup" /> object is the root section group.- or -The <see cref="T:System.Configuration.ConfigurationSectionGroup" /> object has a location.</exception>
	[System.MonoTODO]
	public void ForceDeclaration(bool force)
	{
		require_declaration = force;
	}

	/// <summary>Forces the declaration for this <see cref="T:System.Configuration.ConfigurationSectionGroup" /> object.</summary>
	public void ForceDeclaration()
	{
		ForceDeclaration(force: true);
	}

	/// <summary>Indicates whether the current <see cref="T:System.Configuration.ConfigurationSectionGroup" /> instance should be serialized when the configuration object hierarchy is serialized for the specified target version of the .NET Framework.</summary>
	/// <returns>true if the current section group should be serialized; otherwise, false.</returns>
	/// <param name="targetFramework">The target version of the .NET Framework.</param>
	protected internal virtual bool ShouldSerializeSectionGroupInTargetVersion(FrameworkName targetFramework)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
		return default(bool);
	}
}
