using System.ComponentModel;
using System.Configuration;

namespace System.Net.Configuration;

/// <summary>Represents a URI prefix and the associated class that handles creating Web requests for the prefix. This class cannot be inherited.</summary>
public sealed class WebRequestModuleElement : ConfigurationElement
{
	private static ConfigurationPropertyCollection properties;

	private static ConfigurationProperty prefixProp;

	private static ConfigurationProperty typeProp;

	/// <summary>Gets or sets the URI prefix for the current Web request module.</summary>
	/// <returns>A string that contains a URI prefix.</returns>
	[ConfigurationProperty("prefix", Options = (ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey))]
	public string Prefix
	{
		get
		{
			return (string)base[prefixProp];
		}
		set
		{
			base[prefixProp] = value;
		}
	}

	/// <summary>Gets or sets a class that creates Web requests.</summary>
	/// <returns>A <see cref="T:System.Type" /> instance that identifies a Web request module.</returns>
	[ConfigurationProperty("type")]
	[TypeConverter(typeof(TypeConverter))]
	public Type Type
	{
		get
		{
			return Type.GetType((string)base[typeProp]);
		}
		set
		{
			base[typeProp] = value.FullName;
		}
	}

	protected override ConfigurationPropertyCollection Properties => properties;

	static WebRequestModuleElement()
	{
		prefixProp = new ConfigurationProperty("prefix", typeof(string), null, ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);
		typeProp = new ConfigurationProperty("type", typeof(string));
		properties = new ConfigurationPropertyCollection();
		properties.Add(prefixProp);
		properties.Add(typeProp);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Configuration.WebRequestModuleElement" /> class. </summary>
	public WebRequestModuleElement()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Configuration.WebRequestModuleElement" /> class using the specified URI prefix and type information. </summary>
	/// <param name="prefix">A string containing a URI prefix.</param>
	/// <param name="type">A string containing the type and assembly information for the class that handles creating requests for resources that use the <paramref name="prefix" /> URI prefix. For more information, see the Remarks section.</param>
	public WebRequestModuleElement(string prefix, string type)
	{
		base[typeProp] = type;
		Prefix = prefix;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Configuration.WebRequestModuleElement" /> class using the specified URI prefix and type identifier.</summary>
	/// <param name="prefix">A string containing a URI prefix.</param>
	/// <param name="type">A <see cref="T:System.Type" /> that identifies the class that handles creating requests for resources that use the <paramref name="prefix" /> URI prefix. </param>
	public WebRequestModuleElement(string prefix, Type type)
		: this(prefix, type.FullName)
	{
	}
}
