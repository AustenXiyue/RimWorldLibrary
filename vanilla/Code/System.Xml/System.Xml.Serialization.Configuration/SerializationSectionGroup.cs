using System.Configuration;

namespace System.Xml.Serialization.Configuration;

/// <summary>Handles the XML elements used to configure XML serialization.</summary>
public sealed class SerializationSectionGroup : ConfigurationSectionGroup
{
	/// <summary>Gets the object that represents the section that contains configuration elements for the <see cref="T:System.Xml.Serialization.XmlSchemaImporter" />.</summary>
	/// <returns>The <see cref="T:System.Xml.Serialization.Configuration.SchemaImporterExtensionsSection" /> that represents the schemaImporterExtenstion element in the configuration file.</returns>
	[ConfigurationProperty("schemaImporterExtensions")]
	public SchemaImporterExtensionsSection SchemaImporterExtensions => (SchemaImporterExtensionsSection)base.Sections["schemaImporterExtensions"];

	/// <summary>Gets the object that represents the <see cref="T:System.DateTime" /> serialization configuration element.</summary>
	/// <returns>The <see cref="T:System.Xml.Serialization.Configuration.DateTimeSerializationSection" /> object that represents the configuration element.</returns>
	[ConfigurationProperty("dateTimeSerialization")]
	public DateTimeSerializationSection DateTimeSerialization => (DateTimeSerializationSection)base.Sections["dateTimeSerialization"];

	/// <summary>Gets the object that represents the configuration group for the <see cref="T:System.Xml.Serialization.XmlSerializer" />.</summary>
	/// <returns>The <see cref="T:System.Xml.Serialization.Configuration.XmlSerializerSection" /> that represents the <see cref="T:System.Xml.Serialization.XmlSerializer" />.</returns>
	public XmlSerializerSection XmlSerializer => (XmlSerializerSection)base.Sections["xmlSerializer"];

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Serialization.Configuration.SerializationSectionGroup" /> class.</summary>
	public SerializationSectionGroup()
	{
	}
}
