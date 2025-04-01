using System.ComponentModel;
using System.Configuration;

namespace System.Xml.Serialization.Configuration;

/// <summary>Handles configuration settings for XML serialization of <see cref="T:System.DateTime" /> instances.</summary>
public sealed class DateTimeSerializationSection : ConfigurationSection
{
	/// <summary>Determines XML serialization format of <see cref="T:System.DateTime" /> objects.</summary>
	public enum DateTimeSerializationMode
	{
		/// <summary>Same as Roundtrip.</summary>
		Default,
		/// <summary>The serializer examines individual <see cref="T:System.DateTime" />  instances to determine the serialization format: UTC, local, or unspecified.</summary>
		Roundtrip,
		/// <summary>The serializer formats all <see cref="T:System.DateTime" /> objects as local time. This is for version 1.0 and 1.1 compatibility.</summary>
		Local
	}

	private ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

	private readonly ConfigurationProperty mode = new ConfigurationProperty("mode", typeof(DateTimeSerializationMode), DateTimeSerializationMode.Roundtrip, new EnumConverter(typeof(DateTimeSerializationMode)), null, ConfigurationPropertyOptions.None);

	protected override ConfigurationPropertyCollection Properties => properties;

	/// <summary>Gets or sets a value that determines the serialization format.</summary>
	/// <returns>One of the <see cref="T:System.Xml.Serialization.Configuration.DateTimeSerializationSection.DateTimeSerializationMode" /> values.</returns>
	[ConfigurationProperty("mode", DefaultValue = DateTimeSerializationMode.Roundtrip)]
	public DateTimeSerializationMode Mode
	{
		get
		{
			return (DateTimeSerializationMode)base[mode];
		}
		set
		{
			base[mode] = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Serialization.Configuration.DateTimeSerializationSection" /> class.</summary>
	public DateTimeSerializationSection()
	{
		properties.Add(mode);
	}
}
