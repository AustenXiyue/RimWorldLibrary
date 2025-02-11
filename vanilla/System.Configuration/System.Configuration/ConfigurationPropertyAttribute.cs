namespace System.Configuration;

/// <summary>Declaratively instructs the .NET Framework to instantiate a configuration property. This class cannot be inherited.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ConfigurationPropertyAttribute : Attribute
{
	private string name;

	private object default_value = ConfigurationProperty.NoDefaultValue;

	private ConfigurationPropertyOptions flags;

	/// <summary>Gets or sets a value indicating whether this is a key property for the decorated element property.</summary>
	/// <returns>true if the property is a key property for an element of the collection; otherwise, false. The default is false.</returns>
	public bool IsKey
	{
		get
		{
			return (flags & ConfigurationPropertyOptions.IsKey) != 0;
		}
		set
		{
			if (value)
			{
				flags |= ConfigurationPropertyOptions.IsKey;
			}
			else
			{
				flags &= ~ConfigurationPropertyOptions.IsKey;
			}
		}
	}

	/// <summary>Gets or sets a value indicating whether this is the default property collection for the decorated configuration property. </summary>
	/// <returns>true if the property represents the default collection of an element; otherwise, false. The default is false.</returns>
	public bool IsDefaultCollection
	{
		get
		{
			return (flags & ConfigurationPropertyOptions.IsDefaultCollection) != 0;
		}
		set
		{
			if (value)
			{
				flags |= ConfigurationPropertyOptions.IsDefaultCollection;
			}
			else
			{
				flags &= ~ConfigurationPropertyOptions.IsDefaultCollection;
			}
		}
	}

	/// <summary>Gets or sets the default value for the decorated property.</summary>
	/// <returns>The object representing the default value of the decorated configuration-element property.</returns>
	public object DefaultValue
	{
		get
		{
			return default_value;
		}
		set
		{
			default_value = value;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Configuration.ConfigurationPropertyOptions" /> for the decorated configuration-element property.</summary>
	/// <returns>One of the <see cref="T:System.Configuration.ConfigurationPropertyOptions" /> enumeration values associated with the property.</returns>
	public ConfigurationPropertyOptions Options
	{
		get
		{
			return flags;
		}
		set
		{
			flags = value;
		}
	}

	/// <summary>Gets the name of the decorated configuration-element property.</summary>
	/// <returns>The name of the decorated configuration-element property.</returns>
	public string Name => name;

	/// <summary>Gets or sets a value indicating whether the decorated element property is required.</summary>
	/// <returns>true if the property is required; otherwise, false. The default is false.</returns>
	public bool IsRequired
	{
		get
		{
			return (flags & ConfigurationPropertyOptions.IsRequired) != 0;
		}
		set
		{
			if (value)
			{
				flags |= ConfigurationPropertyOptions.IsRequired;
			}
			else
			{
				flags &= ~ConfigurationPropertyOptions.IsRequired;
			}
		}
	}

	/// <summary>Initializes a new instance of <see cref="T:System.Configuration.ConfigurationPropertyAttribute" /> class.</summary>
	/// <param name="name">Name of the <see cref="T:System.Configuration.ConfigurationProperty" /> object defined.</param>
	public ConfigurationPropertyAttribute(string name)
	{
		this.name = name;
	}
}
