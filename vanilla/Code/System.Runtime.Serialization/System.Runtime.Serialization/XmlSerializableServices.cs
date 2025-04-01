using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;

namespace System.Runtime.Serialization;

/// <summary>Contains methods for reading and writing XML. </summary>
/// <filterpriority>2</filterpriority>
public static class XmlSerializableServices
{
	internal static readonly string ReadNodesMethodName = "ReadNodes";

	internal static string WriteNodesMethodName = "WriteNodes";

	internal static string AddDefaultSchemaMethodName = "AddDefaultSchema";

	/// <summary>Reads a set of XML nodes from the specified reader and returns the result.</summary>
	/// <returns>An array of type <see cref="T:System.Xml.XmlNode" />. </returns>
	/// <param name="xmlReader">An <see cref="T:System.Xml.XmlReader" /> used for reading.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="xmlReader" /> argument is null.</exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">While reading, a null node was encountered.</exception>
	public static XmlNode[] ReadNodes(XmlReader xmlReader)
	{
		if (xmlReader == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("xmlReader");
		}
		XmlDocument xmlDocument = new XmlDocument();
		List<XmlNode> list = new List<XmlNode>();
		if (xmlReader.MoveToFirstAttribute())
		{
			do
			{
				if (IsValidAttribute(xmlReader))
				{
					XmlNode xmlNode = xmlDocument.ReadNode(xmlReader);
					if (xmlNode == null)
					{
						throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString("Unexpected end of file.")));
					}
					list.Add(xmlNode);
				}
			}
			while (xmlReader.MoveToNextAttribute());
		}
		xmlReader.MoveToElement();
		if (!xmlReader.IsEmptyElement)
		{
			int depth = xmlReader.Depth;
			xmlReader.Read();
			while (xmlReader.Depth > depth && xmlReader.NodeType != XmlNodeType.EndElement)
			{
				XmlNode xmlNode2 = xmlDocument.ReadNode(xmlReader);
				if (xmlNode2 == null)
				{
					throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString("Unexpected end of file.")));
				}
				list.Add(xmlNode2);
			}
		}
		return list.ToArray();
	}

	private static bool IsValidAttribute(XmlReader xmlReader)
	{
		if (xmlReader.NamespaceURI != "http://schemas.microsoft.com/2003/10/Serialization/" && xmlReader.NamespaceURI != "http://www.w3.org/2001/XMLSchema-instance" && xmlReader.Prefix != "xmlns")
		{
			return xmlReader.LocalName != "xmlns";
		}
		return false;
	}

	/// <summary>Writes the supplied nodes using the specified writer.</summary>
	/// <param name="xmlWriter">An <see cref="T:System.Xml.XmlWriter" /> used for writing.</param>
	/// <param name="nodes">An array of type <see cref="T:System.Xml.XmlNode" /> to write.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="xmlWriter" /> argument is null.</exception>
	public static void WriteNodes(XmlWriter xmlWriter, XmlNode[] nodes)
	{
		if (xmlWriter == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("xmlWriter");
		}
		if (nodes == null)
		{
			return;
		}
		for (int i = 0; i < nodes.Length; i++)
		{
			if (nodes[i] != null)
			{
				nodes[i].WriteTo(xmlWriter);
			}
		}
	}

	/// <summary>Generates a default schema type given the specified type name and adds it to the specified schema set. </summary>
	/// <param name="schemas">An <see cref="T:System.Xml.Schema.XmlSchemaSet" /> to add the generated schema type to.</param>
	/// <param name="typeQName">An <see cref="T:System.Xml.XmlQualifiedName" /> that specifies the type name to assign the schema to. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="schemas" /> or <paramref name="typeQName" /> argument is null.</exception>
	public static void AddDefaultSchema(XmlSchemaSet schemas, XmlQualifiedName typeQName)
	{
		if (schemas == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("schemas");
		}
		if (typeQName == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("typeQName");
		}
		SchemaExporter.AddDefaultXmlType(schemas, typeQName.Name, typeQName.Namespace);
	}
}
