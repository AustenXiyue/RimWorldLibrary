using System.Configuration;

namespace System.Net.Configuration;

/// <summary>Represents the type information for an authentication module. This class cannot be inherited.</summary>
public sealed class AuthenticationModuleElement : ConfigurationElement
{
	private static ConfigurationPropertyCollection properties;

	private static ConfigurationProperty typeProp;

	protected override ConfigurationPropertyCollection Properties => properties;

	/// <summary>Gets or sets the type and assembly information for the current instance.</summary>
	/// <returns>A string that identifies a type that implements an authentication module or null if no value has been specified.</returns>
	[ConfigurationProperty("type", Options = (ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey))]
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

	static AuthenticationModuleElement()
	{
		typeProp = new ConfigurationProperty("type", typeof(string), null, ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);
		properties = new ConfigurationPropertyCollection();
		properties.Add(typeProp);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Configuration.AuthenticationModuleElement" /> class. </summary>
	public AuthenticationModuleElement()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Configuration.AuthenticationModuleElement" /> class with the specified type information.</summary>
	/// <param name="typeName">A string that identifies the type and the assembly that contains it.</param>
	public AuthenticationModuleElement(string typeName)
	{
		Type = typeName;
	}
}
