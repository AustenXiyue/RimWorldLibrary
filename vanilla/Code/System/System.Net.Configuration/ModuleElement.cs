using System.Configuration;

namespace System.Net.Configuration;

/// <summary>Represents the type information for a custom <see cref="T:System.Net.IWebProxy" /> module. This class cannot be inherited.</summary>
public sealed class ModuleElement : ConfigurationElement
{
	private static ConfigurationPropertyCollection properties;

	private static ConfigurationProperty typeProp;

	protected override ConfigurationPropertyCollection Properties => properties;

	/// <summary>Gets or sets the type and assembly information for the current instance.</summary>
	/// <returns>A string that identifies a type that implements the <see cref="T:System.Net.IWebProxy" /> interface or null if no value has been specified.</returns>
	[ConfigurationProperty("type")]
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

	static ModuleElement()
	{
		typeProp = new ConfigurationProperty("type", typeof(string), null);
		properties = new ConfigurationPropertyCollection();
		properties.Add(typeProp);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Configuration.ModuleElement" /> class. </summary>
	public ModuleElement()
	{
	}
}
