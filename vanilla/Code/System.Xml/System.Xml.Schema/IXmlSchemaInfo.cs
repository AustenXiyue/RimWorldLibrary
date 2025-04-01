namespace System.Xml.Schema;

/// <summary>Defines the post-schema-validation infoset of a validated XML node.</summary>
public interface IXmlSchemaInfo
{
	/// <summary>Gets the <see cref="T:System.Xml.Schema.XmlSchemaValidity" /> value of this validated XML node.</summary>
	/// <returns>An <see cref="T:System.Xml.Schema.XmlSchemaValidity" /> value of this validated XML node.</returns>
	XmlSchemaValidity Validity { get; }

	/// <summary>Gets a value indicating if this validated XML node was set as the result of a default being applied during XML Schema Definition Language (XSD) schema validation.</summary>
	/// <returns>true if this validated XML node was set as the result of a default being applied during schema validation; otherwise, false.</returns>
	bool IsDefault { get; }

	/// <summary>Gets a value indicating if the value for this validated XML node is nil.</summary>
	/// <returns>true if the value for this validated XML node is nil; otherwise, false.</returns>
	bool IsNil { get; }

	/// <summary>Gets the dynamic schema type for this validated XML node.</summary>
	/// <returns>An <see cref="T:System.Xml.Schema.XmlSchemaSimpleType" /> object that represents the dynamic schema type for this validated XML node.</returns>
	XmlSchemaSimpleType MemberType { get; }

	/// <summary>Gets the static XML Schema Definition Language (XSD) schema type of this validated XML node.</summary>
	/// <returns>An <see cref="T:System.Xml.Schema.XmlSchemaType" /> of this validated XML node.</returns>
	XmlSchemaType SchemaType { get; }

	/// <summary>Gets the compiled <see cref="T:System.Xml.Schema.XmlSchemaElement" /> that corresponds to this validated XML node.</summary>
	/// <returns>An <see cref="T:System.Xml.Schema.XmlSchemaElement" /> that corresponds to this validated XML node.</returns>
	XmlSchemaElement SchemaElement { get; }

	/// <summary>Gets the compiled <see cref="T:System.Xml.Schema.XmlSchemaAttribute" /> that corresponds to this validated XML node.</summary>
	/// <returns>An <see cref="T:System.Xml.Schema.XmlSchemaAttribute" /> that corresponds to this validated XML node.</returns>
	XmlSchemaAttribute SchemaAttribute { get; }
}
