using System.Xml.Linq;

namespace System.Xml.Schema;

/// <summary>This class contains the LINQ to XML extension methods for XSD validation. </summary>
/// <filterpriority>2</filterpriority>
public static class Extensions
{
	/// <summary>Gets the post-schema-validation infoset (PSVI) of a validated element.</summary>
	/// <returns>A <see cref="T:System.Xml.Schema.IXmlSchemaInfo" /> that contains the post-schema-validation infoset (PSVI) for an <see cref="T:System.Xml.Linq.XElement" />.</returns>
	/// <param name="source">An <see cref="T:System.Xml.Linq.XElement" /> that has been previously validated.</param>
	/// <filterpriority>2</filterpriority>
	public static IXmlSchemaInfo GetSchemaInfo(this XElement source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Annotation<IXmlSchemaInfo>();
	}

	/// <summary>Gets the post-schema-validation infoset (PSVI) of a validated attribute.</summary>
	/// <returns>A <see cref="T:System.Xml.Schema.IXmlSchemaInfo" /> that contains the post-schema-validation infoset for an <see cref="T:System.Xml.Linq.XAttribute" />.</returns>
	/// <param name="source">An <see cref="T:System.Xml.Linq.XAttribute" /> that has been previously validated.</param>
	/// <filterpriority>2</filterpriority>
	public static IXmlSchemaInfo GetSchemaInfo(this XAttribute source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Annotation<IXmlSchemaInfo>();
	}

	/// <summary>This method validates that an <see cref="T:System.Xml.Linq.XDocument" /> conforms to an XSD in an <see cref="T:System.Xml.Schema.XmlSchemaSet" />.</summary>
	/// <param name="source">The <see cref="T:System.Xml.Linq.XDocument" /> to validate.</param>
	/// <param name="schemas">An <see cref="T:System.Xml.Schema.XmlSchemaSet" /> to validate against.</param>
	/// <param name="validationEventHandler">A <see cref="T:System.Xml.Schema.ValidationEventHandler" /> for an event that occurs when the reader encounters validation errors. If null, throws an exception upon validation errors.</param>
	/// <exception cref="T:System.Xml.Schema.XmlSchemaValidationException">Thrown for XML Schema Definition Language (XSD) validation errors.</exception>
	/// <filterpriority>2</filterpriority>
	public static void Validate(this XDocument source, XmlSchemaSet schemas, ValidationEventHandler validationEventHandler)
	{
		source.Validate(schemas, validationEventHandler, addSchemaInfo: false);
	}

	/// <summary>Validates that an <see cref="T:System.Xml.Linq.XDocument" /> conforms to an XSD in an <see cref="T:System.Xml.Schema.XmlSchemaSet" />, optionally populating the XML tree with the post-schema-validation infoset (PSVI).</summary>
	/// <param name="source">The <see cref="T:System.Xml.Linq.XDocument" /> to validate.</param>
	/// <param name="schemas">An <see cref="T:System.Xml.Schema.XmlSchemaSet" /> to validate against.</param>
	/// <param name="validationEventHandler">A <see cref="T:System.Xml.Schema.ValidationEventHandler" /> for an event that occurs when the reader encounters validation errors. If null, throws an exception upon validation errors.</param>
	/// <param name="addSchemaInfo">A <see cref="T:System.Boolean" /> indicating whether to populate the post-schema-validation infoset (PSVI).</param>
	/// <exception cref="T:System.Xml.Schema.XmlSchemaValidationException">Thrown for XML Schema Definition Language (XSD) validation errors.</exception>
	/// <filterpriority>2</filterpriority>
	public static void Validate(this XDocument source, XmlSchemaSet schemas, ValidationEventHandler validationEventHandler, bool addSchemaInfo)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (schemas == null)
		{
			throw new ArgumentNullException("schemas");
		}
		new XNodeValidator(schemas, validationEventHandler).Validate(source, null, addSchemaInfo);
	}

	/// <summary>This method validates that an <see cref="T:System.Xml.Linq.XElement" /> sub-tree conforms to a specified <see cref="T:System.Xml.Schema.XmlSchemaObject" /> and an <see cref="T:System.Xml.Schema.XmlSchemaSet" />.</summary>
	/// <param name="source">The <see cref="T:System.Xml.Linq.XElement" /> to validate.</param>
	/// <param name="partialValidationType">An <see cref="T:System.Xml.Schema.XmlSchemaObject" /> that specifies the sub-tree to validate.</param>
	/// <param name="schemas">An <see cref="T:System.Xml.Schema.XmlSchemaSet" /> to validate against.</param>
	/// <param name="validationEventHandler">A <see cref="T:System.Xml.Schema.ValidationEventHandler" /> for an event that occurs when the reader encounters validation errors. If null, throws an exception upon validation errors.</param>
	/// <exception cref="T:System.Xml.Schema.XmlSchemaValidationException">Thrown for XML Schema Definition Language (XSD) validation errors.</exception>
	/// <filterpriority>2</filterpriority>
	public static void Validate(this XElement source, XmlSchemaObject partialValidationType, XmlSchemaSet schemas, ValidationEventHandler validationEventHandler)
	{
		source.Validate(partialValidationType, schemas, validationEventHandler, addSchemaInfo: false);
	}

	/// <summary>Validates that an <see cref="T:System.Xml.Linq.XElement" /> sub-tree conforms to a specified <see cref="T:System.Xml.Schema.XmlSchemaObject" /> and an <see cref="T:System.Xml.Schema.XmlSchemaSet" />, optionally populating the XML tree with the post-schema-validation infoset (PSVI).</summary>
	/// <param name="source">The <see cref="T:System.Xml.Linq.XElement" /> to validate.</param>
	/// <param name="partialValidationType">An <see cref="T:System.Xml.Schema.XmlSchemaObject" /> that specifies the sub-tree to validate.</param>
	/// <param name="schemas">An <see cref="T:System.Xml.Schema.XmlSchemaSet" /> to validate against.</param>
	/// <param name="validationEventHandler">A <see cref="T:System.Xml.Schema.ValidationEventHandler" /> for an event that occurs when the reader encounters validation errors. If null, throws an exception upon validation errors.</param>
	/// <param name="addSchemaInfo">A <see cref="T:System.Boolean" /> indicating whether to populate the post-schema-validation infoset (PSVI).</param>
	/// <exception cref="T:System.Xml.Schema.XmlSchemaValidationException">Thrown for XML Schema Definition Language (XSD) validation errors.</exception>
	/// <filterpriority>2</filterpriority>
	public static void Validate(this XElement source, XmlSchemaObject partialValidationType, XmlSchemaSet schemas, ValidationEventHandler validationEventHandler, bool addSchemaInfo)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (partialValidationType == null)
		{
			throw new ArgumentNullException("partialValidationType");
		}
		if (schemas == null)
		{
			throw new ArgumentNullException("schemas");
		}
		new XNodeValidator(schemas, validationEventHandler).Validate(source, partialValidationType, addSchemaInfo);
	}

	/// <summary>This method validates that an <see cref="T:System.Xml.Linq.XAttribute" /> conforms to a specified <see cref="T:System.Xml.Schema.XmlSchemaObject" /> and an <see cref="T:System.Xml.Schema.XmlSchemaSet" />.</summary>
	/// <param name="source">The <see cref="T:System.Xml.Linq.XAttribute" /> to validate.</param>
	/// <param name="partialValidationType">An <see cref="T:System.Xml.Schema.XmlSchemaObject" /> that specifies the sub-tree to validate.</param>
	/// <param name="schemas">An <see cref="T:System.Xml.Schema.XmlSchemaSet" /> to validate against.</param>
	/// <param name="validationEventHandler">A <see cref="T:System.Xml.Schema.ValidationEventHandler" /> for an event that occurs when the reader encounters validation errors. If null, throws an exception upon validation errors.</param>
	/// <exception cref="T:System.Xml.Schema.XmlSchemaValidationException">Thrown for XML Schema Definition Language (XSD) validation errors.</exception>
	/// <filterpriority>2</filterpriority>
	public static void Validate(this XAttribute source, XmlSchemaObject partialValidationType, XmlSchemaSet schemas, ValidationEventHandler validationEventHandler)
	{
		source.Validate(partialValidationType, schemas, validationEventHandler, addSchemaInfo: false);
	}

	/// <summary>Validates that an <see cref="T:System.Xml.Linq.XAttribute" /> conforms to a specified <see cref="T:System.Xml.Schema.XmlSchemaObject" /> and an <see cref="T:System.Xml.Schema.XmlSchemaSet" />, optionally populating the XML tree with the post-schema-validation infoset (PSVI).</summary>
	/// <param name="source">The <see cref="T:System.Xml.Linq.XAttribute" /> to validate.</param>
	/// <param name="partialValidationType">An <see cref="T:System.Xml.Schema.XmlSchemaObject" /> that specifies the sub-tree to validate.</param>
	/// <param name="schemas">An <see cref="T:System.Xml.Schema.XmlSchemaSet" /> to validate against.</param>
	/// <param name="validationEventHandler">A <see cref="T:System.Xml.Schema.ValidationEventHandler" /> for an event that occurs when the reader encounters validation errors. If null, throws an exception upon validation errors.</param>
	/// <param name="addSchemaInfo">A <see cref="T:System.Boolean" /> indicating whether to populate the post-schema-validation infoset (PSVI).</param>
	/// <exception cref="T:System.Xml.Schema.XmlSchemaValidationException">Thrown for XML Schema Definition Language (XSD) validation errors.</exception>
	/// <filterpriority>2</filterpriority>
	public static void Validate(this XAttribute source, XmlSchemaObject partialValidationType, XmlSchemaSet schemas, ValidationEventHandler validationEventHandler, bool addSchemaInfo)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (partialValidationType == null)
		{
			throw new ArgumentNullException("partialValidationType");
		}
		if (schemas == null)
		{
			throw new ArgumentNullException("schemas");
		}
		new XNodeValidator(schemas, validationEventHandler).Validate(source, partialValidationType, addSchemaInfo);
	}
}
