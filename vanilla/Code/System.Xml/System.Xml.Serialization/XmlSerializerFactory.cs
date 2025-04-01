using System.Security.Permissions;
using System.Security.Policy;

namespace System.Xml.Serialization;

/// <summary>Creates typed versions of the <see cref="T:System.Xml.Serialization.XmlSerializer" /> for more efficient serialization.</summary>
public class XmlSerializerFactory
{
	private static TempAssemblyCache cache = new TempAssemblyCache();

	/// <summary>Returns a derivation of the <see cref="T:System.Xml.Serialization.XmlSerializer" /> class that can serialize objects of the specified type into XML document instances, and vice versa. Each object to be serialized can itself contain instances of classes, which this overload can override with other classes. This overload also specifies the default namespace for all the XML elements, and the class to use as the XML root element.</summary>
	/// <returns>A derivation of the <see cref="T:System.Xml.Serialization.XmlSerializer" />.</returns>
	/// <param name="type">The <see cref="T:System.Type" /> to serialize.</param>
	/// <param name="overrides">An <see cref="T:System.Xml.Serialization.XmlAttributeOverrides" /> that contains fields that override the default serialization behavior.</param>
	/// <param name="extraTypes">A <see cref="T:System.Type" /> array of additional object types to serialize.</param>
	/// <param name="root">An <see cref="T:System.Xml.Serialization.XmlRootAttribute" /> that represents the XML root element.</param>
	/// <param name="defaultNamespace">The default namespace of all XML elements in the XML document. </param>
	public XmlSerializer CreateSerializer(Type type, XmlAttributeOverrides overrides, Type[] extraTypes, XmlRootAttribute root, string defaultNamespace)
	{
		return CreateSerializer(type, overrides, extraTypes, root, defaultNamespace, null);
	}

	/// <summary>Returns a derivation of the <see cref="T:System.Xml.Serialization.XmlSerializer" /> class that can serialize objects of the specified type into XML documents, and vice versa. Specifies the object that represents the XML root element.</summary>
	/// <returns>A derivation of the <see cref="T:System.Xml.Serialization.XmlSerializer" />.</returns>
	/// <param name="type">The <see cref="T:System.Type" /> to serialize.</param>
	/// <param name="root">An <see cref="T:System.Xml.Serialization.XmlRootAttribute" /> that represents the XML root element.</param>
	public XmlSerializer CreateSerializer(Type type, XmlRootAttribute root)
	{
		return CreateSerializer(type, null, new Type[0], root, null, null);
	}

	/// <summary>Returns a derivation of the <see cref="T:System.Xml.Serialization.XmlSerializerFactory" /> class that is used to serialize the specified type. If a property or field returns an array, the <paramref name="extraTypes" /> parameter specifies objects that can be inserted into the array.</summary>
	/// <returns>A derivation of the <see cref="T:System.Xml.Serialization.XmlSerializer" />.</returns>
	/// <param name="type">The <see cref="T:System.Type" /> to serialize.</param>
	/// <param name="extraTypes">A <see cref="T:System.Type" /> array of additional object types to serialize.</param>
	public XmlSerializer CreateSerializer(Type type, Type[] extraTypes)
	{
		return CreateSerializer(type, null, extraTypes, null, null, null);
	}

	/// <summary>Returns a derivation of the <see cref="T:System.Xml.Serialization.XmlSerializer" /> class that can serialize objects of the specified type into XML documents, and vice versa. Each object to be serialized can itself contain instances of classes, which this overload can override with other classes.</summary>
	/// <returns>A derivation of the <see cref="T:System.Xml.Serialization.XmlSerializer" />.</returns>
	/// <param name="type">The <see cref="T:System.Type" /> to serialize.</param>
	/// <param name="overrides">An <see cref="T:System.Xml.Serialization.XmlAttributeOverrides" /> that contains fields that override the default serialization behavior.</param>
	public XmlSerializer CreateSerializer(Type type, XmlAttributeOverrides overrides)
	{
		return CreateSerializer(type, overrides, new Type[0], null, null, null);
	}

	/// <summary>Returns a derivation of the <see cref="T:System.Xml.Serialization.XmlSerializerFactory" /> class using an object that maps one type to another.</summary>
	/// <returns>A derivation of the <see cref="T:System.Xml.Serialization.XmlSerializer" /> class that is specifically created to serialize the mapped type.</returns>
	/// <param name="xmlTypeMapping">An <see cref="T:System.Xml.Serialization.XmlTypeMapping" /> that maps one type to another.</param>
	public XmlSerializer CreateSerializer(XmlTypeMapping xmlTypeMapping)
	{
		return (XmlSerializer)XmlSerializer.GenerateTempAssembly(xmlTypeMapping).Contract.TypedSerializers[xmlTypeMapping.Key];
	}

	/// <summary>Returns a derivation of the <see cref="T:System.Xml.Serialization.XmlSerializerFactory" /> class that is used to serialize the specified type.</summary>
	/// <returns>A derivation of the <see cref="T:System.Xml.Serialization.XmlSerializer" /> class that is specifically created to serialize the specified type.</returns>
	/// <param name="type">The <see cref="T:System.Type" /> to serialize.</param>
	public XmlSerializer CreateSerializer(Type type)
	{
		return CreateSerializer(type, (string)null);
	}

	/// <summary>Returns a derivation of the <see cref="T:System.Xml.Serialization.XmlSerializerFactory" /> class that is used to serialize the specified type and namespace.</summary>
	/// <returns>A derivation of the <see cref="T:System.Xml.Serialization.XmlSerializer" /> class that is specifically created to serialize the specified type.</returns>
	/// <param name="type">The <see cref="T:System.Type" /> to serialize.</param>
	/// <param name="defaultNamespace">The default namespace to use for all the XML elements. </param>
	public XmlSerializer CreateSerializer(Type type, string defaultNamespace)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		TempAssembly tempAssembly = cache[defaultNamespace, type];
		XmlTypeMapping xmlTypeMapping = null;
		if (tempAssembly == null)
		{
			lock (cache)
			{
				tempAssembly = cache[defaultNamespace, type];
				if (tempAssembly == null)
				{
					if (TempAssembly.LoadGeneratedAssembly(type, defaultNamespace, out var contract) == null)
					{
						xmlTypeMapping = new XmlReflectionImporter(defaultNamespace).ImportTypeMapping(type, null, defaultNamespace);
						tempAssembly = XmlSerializer.GenerateTempAssembly(xmlTypeMapping, type, defaultNamespace);
					}
					else
					{
						tempAssembly = new TempAssembly(contract);
					}
					cache.Add(defaultNamespace, type, tempAssembly);
				}
			}
		}
		if (xmlTypeMapping == null)
		{
			xmlTypeMapping = XmlReflectionImporter.GetTopLevelMapping(type, defaultNamespace);
		}
		return tempAssembly.Contract.GetSerializer(type);
	}

	/// <summary>Returns a derivation of the <see cref="T:System.Xml.Serialization.XmlSerializer" /> class that can serialize objects of the specified type into XML document instances, and vice versa. Each object to be serialized can itself contain instances of classes, which this overload can override with other classes. This overload also specifies the default namespace for all the XML elements, and the class to use as the XML root element.</summary>
	/// <returns>A derivation of the <see cref="T:System.Xml.Serialization.XmlSerializer" />.</returns>
	/// <param name="type">The <see cref="T:System.Type" /> of the object that this <see cref="T:System.Xml.Serialization.XmlSerializer" /> can serialize.</param>
	/// <param name="overrides">An <see cref="T:System.Xml.Serialization.XmlAttributeOverrides" /> that extends or overrides the behavior of the class specified in the type parameter.</param>
	/// <param name="extraTypes">A <see cref="T:System.Type" /> array of additional object types to serialize.</param>
	/// <param name="root">An <see cref="T:System.Xml.Serialization.XmlRootAttribute" /> that defines the XML root element properties.</param>
	/// <param name="defaultNamespace">The default namespace of all XML elements in the XML document.</param>
	/// <param name="location">The path that specifies the location of the types.</param>
	public XmlSerializer CreateSerializer(Type type, XmlAttributeOverrides overrides, Type[] extraTypes, XmlRootAttribute root, string defaultNamespace, string location)
	{
		return CreateSerializer(type, overrides, extraTypes, root, defaultNamespace, location, null);
	}

	/// <summary>Returns a derivation of the <see cref="T:System.Xml.Serialization.XmlSerializer" /> class that can serialize objects of the specified type into XML document instances, and vice versa. Each object to be serialized can itself contain instances of classes, which this overload can override with other classes. This overload also specifies the default namespace for all the XML elements, and the class to use as the XML root element.</summary>
	/// <returns>A derivation of the <see cref="T:System.Xml.Serialization.XmlSerializer" />.</returns>
	/// <param name="type">The <see cref="T:System.Type" /> of the object that this <see cref="T:System.Xml.Serialization.XmlSerializer" /> can serialize.</param>
	/// <param name="overrides">An <see cref="T:System.Xml.Serialization.XmlAttributeOverrides" /> that extends or overrides the behavior of the class specified in the type parameter.</param>
	/// <param name="extraTypes">A <see cref="T:System.Type" /> array of additional object types to serialize.</param>
	/// <param name="root">An <see cref="T:System.Xml.Serialization.XmlRootAttribute" /> that defines the XML root element properties.</param>
	/// <param name="defaultNamespace">The default namespace of all XML elements in the XML document.</param>
	/// <param name="location">The path that specifies the location of the types.</param>
	/// <param name="evidence">An instance of the <see cref="T:System.Security.Policy.Evidence" /> class that contains credentials needed to access types.</param>
	[Obsolete("This method is obsolete and will be removed in a future release of the .NET Framework. Please use an overload of CreateSerializer which does not take an Evidence parameter. See http://go2.microsoft.com/fwlink/?LinkId=131738 for more information.")]
	public XmlSerializer CreateSerializer(Type type, XmlAttributeOverrides overrides, Type[] extraTypes, XmlRootAttribute root, string defaultNamespace, string location, Evidence evidence)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (location != null || evidence != null)
		{
			DemandForUserLocationOrEvidence();
		}
		XmlReflectionImporter xmlReflectionImporter = new XmlReflectionImporter(overrides, defaultNamespace);
		for (int i = 0; i < extraTypes.Length; i++)
		{
			xmlReflectionImporter.IncludeType(extraTypes[i]);
		}
		XmlTypeMapping xmlTypeMapping = xmlReflectionImporter.ImportTypeMapping(type, root, defaultNamespace);
		return (XmlSerializer)XmlSerializer.GenerateTempAssembly(xmlTypeMapping, type, defaultNamespace, location, evidence).Contract.TypedSerializers[xmlTypeMapping.Key];
	}

	[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	private void DemandForUserLocationOrEvidence()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Serialization.XmlSerializerFactory" /> class. </summary>
	public XmlSerializerFactory()
	{
	}
}
