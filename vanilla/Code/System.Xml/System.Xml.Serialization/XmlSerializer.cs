using System.CodeDom.Compiler;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Permissions;
using System.Security.Policy;
using System.Threading;

namespace System.Xml.Serialization;

/// <summary>Serializes and deserializes objects into and from XML documents. The <see cref="T:System.Xml.Serialization.XmlSerializer" /> enables you to control how objects are encoded into XML.</summary>
public class XmlSerializer
{
	private class XmlSerializerMappingKey
	{
		public XmlMapping Mapping;

		public XmlSerializerMappingKey(XmlMapping mapping)
		{
			Mapping = mapping;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is XmlSerializerMappingKey xmlSerializerMappingKey))
			{
				return false;
			}
			if (Mapping.Key != xmlSerializerMappingKey.Mapping.Key)
			{
				return false;
			}
			if (Mapping.ElementName != xmlSerializerMappingKey.Mapping.ElementName)
			{
				return false;
			}
			if (Mapping.Namespace != xmlSerializerMappingKey.Mapping.Namespace)
			{
				return false;
			}
			if (Mapping.IsSoap != xmlSerializerMappingKey.Mapping.IsSoap)
			{
				return false;
			}
			return true;
		}

		public override int GetHashCode()
		{
			int num = ((!Mapping.IsSoap) ? 1 : 0);
			if (Mapping.Key != null)
			{
				num ^= Mapping.Key.GetHashCode();
			}
			if (Mapping.ElementName != null)
			{
				num ^= Mapping.ElementName.GetHashCode();
			}
			if (Mapping.Namespace != null)
			{
				num ^= Mapping.Namespace.GetHashCode();
			}
			return num;
		}
	}

	private TempAssembly tempAssembly;

	private bool typedSerializer;

	private Type primitiveType;

	private XmlMapping mapping;

	private XmlDeserializationEvents events;

	private static TempAssemblyCache cache = new TempAssemblyCache();

	private static volatile XmlSerializerNamespaces defaultNamespaces;

	private static Hashtable xmlSerializerTable = new Hashtable();

	private static XmlSerializerNamespaces DefaultNamespaces
	{
		get
		{
			if (defaultNamespaces == null)
			{
				XmlSerializerNamespaces xmlSerializerNamespaces = new XmlSerializerNamespaces();
				xmlSerializerNamespaces.AddInternal("xsi", "http://www.w3.org/2001/XMLSchema-instance");
				xmlSerializerNamespaces.AddInternal("xsd", "http://www.w3.org/2001/XMLSchema");
				if (defaultNamespaces == null)
				{
					defaultNamespaces = xmlSerializerNamespaces;
				}
			}
			return defaultNamespaces;
		}
	}

	/// <summary>Occurs when the <see cref="T:System.Xml.Serialization.XmlSerializer" /> encounters an XML node of unknown type during deserialization.</summary>
	public event XmlNodeEventHandler UnknownNode
	{
		add
		{
			ref XmlDeserializationEvents reference = ref events;
			reference.OnUnknownNode = (XmlNodeEventHandler)Delegate.Combine(reference.OnUnknownNode, value);
		}
		remove
		{
			ref XmlDeserializationEvents reference = ref events;
			reference.OnUnknownNode = (XmlNodeEventHandler)Delegate.Remove(reference.OnUnknownNode, value);
		}
	}

	/// <summary>Occurs when the <see cref="T:System.Xml.Serialization.XmlSerializer" /> encounters an XML attribute of unknown type during deserialization.</summary>
	public event XmlAttributeEventHandler UnknownAttribute
	{
		add
		{
			ref XmlDeserializationEvents reference = ref events;
			reference.OnUnknownAttribute = (XmlAttributeEventHandler)Delegate.Combine(reference.OnUnknownAttribute, value);
		}
		remove
		{
			ref XmlDeserializationEvents reference = ref events;
			reference.OnUnknownAttribute = (XmlAttributeEventHandler)Delegate.Remove(reference.OnUnknownAttribute, value);
		}
	}

	/// <summary>Occurs when the <see cref="T:System.Xml.Serialization.XmlSerializer" /> encounters an XML element of unknown type during deserialization.</summary>
	public event XmlElementEventHandler UnknownElement
	{
		add
		{
			ref XmlDeserializationEvents reference = ref events;
			reference.OnUnknownElement = (XmlElementEventHandler)Delegate.Combine(reference.OnUnknownElement, value);
		}
		remove
		{
			ref XmlDeserializationEvents reference = ref events;
			reference.OnUnknownElement = (XmlElementEventHandler)Delegate.Remove(reference.OnUnknownElement, value);
		}
	}

	/// <summary>Occurs during deserialization of a SOAP-encoded XML stream, when the <see cref="T:System.Xml.Serialization.XmlSerializer" /> encounters a recognized type that is not used or is unreferenced.</summary>
	public event UnreferencedObjectEventHandler UnreferencedObject
	{
		add
		{
			ref XmlDeserializationEvents reference = ref events;
			reference.OnUnreferencedObject = (UnreferencedObjectEventHandler)Delegate.Combine(reference.OnUnreferencedObject, value);
		}
		remove
		{
			ref XmlDeserializationEvents reference = ref events;
			reference.OnUnreferencedObject = (UnreferencedObjectEventHandler)Delegate.Remove(reference.OnUnreferencedObject, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Serialization.XmlSerializer" /> class.</summary>
	protected XmlSerializer()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Serialization.XmlSerializer" /> class that can serialize objects of type <see cref="T:System.Object" /> into XML document instances, and deserialize XML document instances into objects of type <see cref="T:System.Object" />. Each object to be serialized can itself contain instances of classes, which this overload overrides with other classes. This overload also specifies the default namespace for all the XML elements and the class to use as the XML root element.</summary>
	/// <param name="type">The type of the object that this <see cref="T:System.Xml.Serialization.XmlSerializer" /> can serialize. </param>
	/// <param name="overrides">An <see cref="T:System.Xml.Serialization.XmlAttributeOverrides" /> that extends or overrides the behavior of the class specified in the <paramref name="type" /> parameter. </param>
	/// <param name="extraTypes">A <see cref="T:System.Type" /> array of additional object types to serialize. </param>
	/// <param name="root">An <see cref="T:System.Xml.Serialization.XmlRootAttribute" /> that defines the XML root element properties. </param>
	/// <param name="defaultNamespace">The default namespace of all XML elements in the XML document. </param>
	public XmlSerializer(Type type, XmlAttributeOverrides overrides, Type[] extraTypes, XmlRootAttribute root, string defaultNamespace)
		: this(type, overrides, extraTypes, root, defaultNamespace, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Serialization.XmlSerializer" /> class that can serialize objects of the specified type into XML documents, and deserialize an XML document into object of the specified type. It also specifies the class to use as the XML root element.</summary>
	/// <param name="type">The type of the object that this <see cref="T:System.Xml.Serialization.XmlSerializer" /> can serialize. </param>
	/// <param name="root">An <see cref="T:System.Xml.Serialization.XmlRootAttribute" /> that represents the XML root element. </param>
	public XmlSerializer(Type type, XmlRootAttribute root)
		: this(type, null, new Type[0], root, null, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Serialization.XmlSerializer" /> class that can serialize objects of the specified type into XML documents, and deserialize XML documents into object of a specified type. If a property or field returns an array, the <paramref name="extraTypes" /> parameter specifies objects that can be inserted into the array.</summary>
	/// <param name="type">The type of the object that this <see cref="T:System.Xml.Serialization.XmlSerializer" /> can serialize. </param>
	/// <param name="extraTypes">A <see cref="T:System.Type" /> array of additional object types to serialize. </param>
	public XmlSerializer(Type type, Type[] extraTypes)
		: this(type, null, extraTypes, null, null, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Serialization.XmlSerializer" /> class that can serialize objects of the specified type into XML documents, and deserialize XML documents into objects of the specified type. Each object to be serialized can itself contain instances of classes, which this overload can override with other classes.</summary>
	/// <param name="type">The type of the object to serialize. </param>
	/// <param name="overrides">An <see cref="T:System.Xml.Serialization.XmlAttributeOverrides" />. </param>
	public XmlSerializer(Type type, XmlAttributeOverrides overrides)
		: this(type, overrides, new Type[0], null, null, null)
	{
	}

	/// <summary>Initializes an instance of the <see cref="T:System.Xml.Serialization.XmlSerializer" /> class using an object that maps one type to another.</summary>
	/// <param name="xmlTypeMapping">An <see cref="T:System.Xml.Serialization.XmlTypeMapping" /> that maps one type to another. </param>
	public XmlSerializer(XmlTypeMapping xmlTypeMapping)
	{
		tempAssembly = GenerateTempAssembly(xmlTypeMapping);
		mapping = xmlTypeMapping;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Serialization.XmlSerializer" /> class that can serialize objects of the specified type into XML documents, and deserialize XML documents into objects of the specified type.</summary>
	/// <param name="type">The type of the object that this <see cref="T:System.Xml.Serialization.XmlSerializer" /> can serialize. </param>
	public XmlSerializer(Type type)
		: this(type, (string)null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Serialization.XmlSerializer" /> class that can serialize objects of the specified type into XML documents, and deserialize XML documents into objects of the specified type. Specifies the default namespace for all the XML elements.</summary>
	/// <param name="type">The type of the object that this <see cref="T:System.Xml.Serialization.XmlSerializer" /> can serialize. </param>
	/// <param name="defaultNamespace">The default namespace to use for all the XML elements. </param>
	public XmlSerializer(Type type, string defaultNamespace)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		mapping = GetKnownMapping(type, defaultNamespace);
		if (mapping != null)
		{
			primitiveType = type;
			return;
		}
		tempAssembly = cache[defaultNamespace, type];
		if (tempAssembly == null)
		{
			lock (cache)
			{
				tempAssembly = cache[defaultNamespace, type];
				if (tempAssembly == null)
				{
					XmlSerializerImplementation contract;
					Assembly assembly = TempAssembly.LoadGeneratedAssembly(type, defaultNamespace, out contract);
					if (assembly == null)
					{
						XmlReflectionImporter xmlReflectionImporter = new XmlReflectionImporter(defaultNamespace);
						mapping = xmlReflectionImporter.ImportTypeMapping(type, null, defaultNamespace);
						tempAssembly = GenerateTempAssembly(mapping, type, defaultNamespace);
					}
					else
					{
						mapping = XmlReflectionImporter.GetTopLevelMapping(type, defaultNamespace);
						tempAssembly = new TempAssembly(new XmlMapping[1] { mapping }, assembly, contract);
					}
				}
				cache.Add(defaultNamespace, type, tempAssembly);
			}
		}
		if (mapping == null)
		{
			mapping = XmlReflectionImporter.GetTopLevelMapping(type, defaultNamespace);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Serialization.XmlSerializer" /> class that can serialize objects of type <see cref="T:System.Object" /> into XML document instances, and deserialize XML document instances into objects of type <see cref="T:System.Object" />. Each object to be serialized can itself contain instances of classes, which this overload overrides with other classes. This overload also specifies the default namespace for all the XML elements and the class to use as the XML root element.</summary>
	/// <param name="type">The type of the object that this <see cref="T:System.Xml.Serialization.XmlSerializer" /> can serialize.</param>
	/// <param name="overrides">An <see cref="T:System.Xml.Serialization.XmlAttributeOverrides" /> that extends or overrides the behavior of the class specified in the <paramref name="type" /> parameter.</param>
	/// <param name="extraTypes">A <see cref="T:System.Type" /> array of additional object types to serialize.</param>
	/// <param name="root">An <see cref="T:System.Xml.Serialization.XmlRootAttribute" /> that defines the XML root element properties.</param>
	/// <param name="defaultNamespace">The default namespace of all XML elements in the XML document.</param>
	/// <param name="location">The location of the types.</param>
	public XmlSerializer(Type type, XmlAttributeOverrides overrides, Type[] extraTypes, XmlRootAttribute root, string defaultNamespace, string location)
		: this(type, overrides, extraTypes, root, defaultNamespace, location, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Serialization.XmlSerializer" /> class that can serialize objects of the specified type into XML document instances, and deserialize XML document instances into objects of the specified type. This overload allows you to supply other types that can be encountered during a serialization or deserialization operation, as well as a default namespace for all XML elements, the class to use as the XML root element, its location, and credentials required for access.</summary>
	/// <param name="type">The type of the object that this <see cref="T:System.Xml.Serialization.XmlSerializer" /> can serialize.</param>
	/// <param name="overrides">An <see cref="T:System.Xml.Serialization.XmlAttributeOverrides" /> that extends or overrides the behavior of the class specified in the <paramref name="type" /> parameter.</param>
	/// <param name="extraTypes">A <see cref="T:System.Type" /> array of additional object types to serialize.</param>
	/// <param name="root">An <see cref="T:System.Xml.Serialization.XmlRootAttribute" /> that defines the XML root element properties.</param>
	/// <param name="defaultNamespace">The default namespace of all XML elements in the XML document.</param>
	/// <param name="location">The location of the types.</param>
	/// <param name="evidence">An instance of the <see cref="T:System.Security.Policy.Evidence" /> class that contains credentials required to access types.</param>
	[Obsolete("This method is obsolete and will be removed in a future release of the .NET Framework. Please use a XmlSerializer constructor overload which does not take an Evidence parameter. See http://go2.microsoft.com/fwlink/?LinkId=131738 for more information.")]
	public XmlSerializer(Type type, XmlAttributeOverrides overrides, Type[] extraTypes, XmlRootAttribute root, string defaultNamespace, string location, Evidence evidence)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		XmlReflectionImporter xmlReflectionImporter = new XmlReflectionImporter(overrides, defaultNamespace);
		if (extraTypes != null)
		{
			for (int i = 0; i < extraTypes.Length; i++)
			{
				xmlReflectionImporter.IncludeType(extraTypes[i]);
			}
		}
		mapping = xmlReflectionImporter.ImportTypeMapping(type, root, defaultNamespace);
		if (location != null || evidence != null)
		{
			DemandForUserLocationOrEvidence();
		}
		tempAssembly = GenerateTempAssembly(mapping, type, defaultNamespace, location, evidence);
	}

	[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	private void DemandForUserLocationOrEvidence()
	{
	}

	internal static TempAssembly GenerateTempAssembly(XmlMapping xmlMapping)
	{
		return GenerateTempAssembly(xmlMapping, null, null);
	}

	internal static TempAssembly GenerateTempAssembly(XmlMapping xmlMapping, Type type, string defaultNamespace)
	{
		if (xmlMapping == null)
		{
			throw new ArgumentNullException("xmlMapping");
		}
		return new TempAssembly(new XmlMapping[1] { xmlMapping }, new Type[1] { type }, defaultNamespace, null, null);
	}

	internal static TempAssembly GenerateTempAssembly(XmlMapping xmlMapping, Type type, string defaultNamespace, string location, Evidence evidence)
	{
		return new TempAssembly(new XmlMapping[1] { xmlMapping }, new Type[1] { type }, defaultNamespace, location, evidence);
	}

	/// <summary>Serializes the specified <see cref="T:System.Object" /> and writes the XML document to a file using the specified <see cref="T:System.IO.TextWriter" />.</summary>
	/// <param name="textWriter">The <see cref="T:System.IO.TextWriter" /> used to write the XML document. </param>
	/// <param name="o">The <see cref="T:System.Object" /> to serialize. </param>
	public void Serialize(TextWriter textWriter, object o)
	{
		Serialize(textWriter, o, null);
	}

	/// <summary>Serializes the specified <see cref="T:System.Object" /> and writes the XML document to a file using the specified <see cref="T:System.IO.TextWriter" /> and references the specified namespaces.</summary>
	/// <param name="textWriter">The <see cref="T:System.IO.TextWriter" /> used to write the XML document. </param>
	/// <param name="o">The <see cref="T:System.Object" /> to serialize. </param>
	/// <param name="namespaces">The <see cref="T:System.Xml.Serialization.XmlSerializerNamespaces" /> that contains namespaces for the generated XML document. </param>
	/// <exception cref="T:System.InvalidOperationException">An error occurred during serialization. The original exception is available using the <see cref="P:System.Exception.InnerException" /> property. </exception>
	public void Serialize(TextWriter textWriter, object o, XmlSerializerNamespaces namespaces)
	{
		XmlTextWriter xmlTextWriter = new XmlTextWriter(textWriter);
		xmlTextWriter.Formatting = Formatting.Indented;
		xmlTextWriter.Indentation = 2;
		Serialize(xmlTextWriter, o, namespaces);
	}

	/// <summary>Serializes the specified <see cref="T:System.Object" /> and writes the XML document to a file using the specified <see cref="T:System.IO.Stream" />.</summary>
	/// <param name="stream">The <see cref="T:System.IO.Stream" /> used to write the XML document. </param>
	/// <param name="o">The <see cref="T:System.Object" /> to serialize. </param>
	/// <exception cref="T:System.InvalidOperationException">An error occurred during serialization. The original exception is available using the <see cref="P:System.Exception.InnerException" /> property. </exception>
	public void Serialize(Stream stream, object o)
	{
		Serialize(stream, o, null);
	}

	/// <summary>Serializes the specified <see cref="T:System.Object" /> and writes the XML document to a file using the specified <see cref="T:System.IO.Stream" />that references the specified namespaces.</summary>
	/// <param name="stream">The <see cref="T:System.IO.Stream" /> used to write the XML document. </param>
	/// <param name="o">The <see cref="T:System.Object" /> to serialize. </param>
	/// <param name="namespaces">The <see cref="T:System.Xml.Serialization.XmlSerializerNamespaces" /> referenced by the object. </param>
	/// <exception cref="T:System.InvalidOperationException">An error occurred during serialization. The original exception is available using the <see cref="P:System.Exception.InnerException" /> property. </exception>
	public void Serialize(Stream stream, object o, XmlSerializerNamespaces namespaces)
	{
		XmlTextWriter xmlTextWriter = new XmlTextWriter(stream, null);
		xmlTextWriter.Formatting = Formatting.Indented;
		xmlTextWriter.Indentation = 2;
		Serialize(xmlTextWriter, o, namespaces);
	}

	/// <summary>Serializes the specified <see cref="T:System.Object" /> and writes the XML document to a file using the specified <see cref="T:System.Xml.XmlWriter" />.</summary>
	/// <param name="xmlWriter">The <see cref="T:System.xml.XmlWriter" /> used to write the XML document. </param>
	/// <param name="o">The <see cref="T:System.Object" /> to serialize. </param>
	/// <exception cref="T:System.InvalidOperationException">An error occurred during serialization. The original exception is available using the <see cref="P:System.Exception.InnerException" /> property. </exception>
	public void Serialize(XmlWriter xmlWriter, object o)
	{
		Serialize(xmlWriter, o, null);
	}

	/// <summary>Serializes the specified <see cref="T:System.Object" /> and writes the XML document to a file using the specified <see cref="T:System.Xml.XmlWriter" /> and references the specified namespaces.</summary>
	/// <param name="xmlWriter">The <see cref="T:System.xml.XmlWriter" /> used to write the XML document. </param>
	/// <param name="o">The <see cref="T:System.Object" /> to serialize. </param>
	/// <param name="namespaces">The <see cref="T:System.Xml.Serialization.XmlSerializerNamespaces" /> referenced by the object. </param>
	/// <exception cref="T:System.InvalidOperationException">An error occurred during serialization. The original exception is available using the <see cref="P:System.Exception.InnerException" /> property. </exception>
	public void Serialize(XmlWriter xmlWriter, object o, XmlSerializerNamespaces namespaces)
	{
		Serialize(xmlWriter, o, namespaces, null);
	}

	/// <summary>Serializes the specified object and writes the XML document to a file using the specified <see cref="T:System.Xml.XmlWriter" /> and references the specified namespaces and encoding style.</summary>
	/// <param name="xmlWriter">The <see cref="T:System.xml.XmlWriter" /> used to write the XML document. </param>
	/// <param name="o">The object to serialize. </param>
	/// <param name="namespaces">The <see cref="T:System.Xml.Serialization.XmlSerializerNamespaces" /> referenced by the object. </param>
	/// <param name="encodingStyle">The encoding style of the serialized XML. </param>
	/// <exception cref="T:System.InvalidOperationException">An error occurred during serialization. The original exception is available using the <see cref="P:System.Exception.InnerException" /> property. </exception>
	public void Serialize(XmlWriter xmlWriter, object o, XmlSerializerNamespaces namespaces, string encodingStyle)
	{
		Serialize(xmlWriter, o, namespaces, encodingStyle, null);
	}

	/// <summary>Serializes the specified <see cref="T:System.Object" /> and writes the XML document to a file using the specified <see cref="T:System.Xml.XmlWriter" />, XML namespaces, and encoding. </summary>
	/// <param name="xmlWriter">The <see cref="T:System.Xml.XmlWriter" /> used to write the XML document.</param>
	/// <param name="o">The object to serialize.</param>
	/// <param name="namespaces">An instance of the XmlSerializaerNamespaces that contains namespaces and prefixes to use.</param>
	/// <param name="encodingStyle">The encoding used in the document.</param>
	/// <param name="id">For SOAP encoded messages, the base used to generate id attributes. </param>
	public void Serialize(XmlWriter xmlWriter, object o, XmlSerializerNamespaces namespaces, string encodingStyle, string id)
	{
		try
		{
			if (primitiveType != null)
			{
				if (encodingStyle != null && encodingStyle.Length > 0)
				{
					throw new InvalidOperationException(Res.GetString("The encoding style '{0}' is not valid for this call because this XmlSerializer instance does not support encoding. Use the SoapReflectionImporter to initialize an XmlSerializer that supports encoding.", encodingStyle));
				}
				SerializePrimitive(xmlWriter, o, namespaces);
			}
			else if (tempAssembly == null || typedSerializer)
			{
				XmlSerializationWriter xmlSerializationWriter = CreateWriter();
				xmlSerializationWriter.Init(xmlWriter, (namespaces == null || namespaces.Count == 0) ? DefaultNamespaces : namespaces, encodingStyle, id, tempAssembly);
				try
				{
					Serialize(o, xmlSerializationWriter);
				}
				finally
				{
					xmlSerializationWriter.Dispose();
				}
			}
			else
			{
				tempAssembly.InvokeWriter(mapping, xmlWriter, o, (namespaces == null || namespaces.Count == 0) ? DefaultNamespaces : namespaces, encodingStyle, id);
			}
		}
		catch (Exception innerException)
		{
			if (innerException is ThreadAbortException || innerException is StackOverflowException || innerException is OutOfMemoryException)
			{
				throw;
			}
			if (innerException is TargetInvocationException)
			{
				innerException = innerException.InnerException;
			}
			throw new InvalidOperationException(Res.GetString("There was an error generating the XML document."), innerException);
		}
		xmlWriter.Flush();
	}

	/// <summary>Deserializes the XML document contained by the specified <see cref="T:System.IO.Stream" />.</summary>
	/// <returns>The <see cref="T:System.Object" /> being deserialized.</returns>
	/// <param name="stream">The <see cref="T:System.IO.Stream" /> that contains the XML document to deserialize. </param>
	public object Deserialize(Stream stream)
	{
		XmlTextReader xmlTextReader = new XmlTextReader(stream);
		xmlTextReader.WhitespaceHandling = WhitespaceHandling.Significant;
		xmlTextReader.Normalization = true;
		xmlTextReader.XmlResolver = null;
		return Deserialize(xmlTextReader, null);
	}

	/// <summary>Deserializes the XML document contained by the specified <see cref="T:System.IO.TextReader" />.</summary>
	/// <returns>The <see cref="T:System.Object" /> being deserialized.</returns>
	/// <param name="textReader">The <see cref="T:System.IO.TextReader" /> that contains the XML document to deserialize. </param>
	/// <exception cref="T:System.InvalidOperationException">An error occurred during deserialization. The original exception is available using the <see cref="P:System.Exception.InnerException" /> property. </exception>
	public object Deserialize(TextReader textReader)
	{
		XmlTextReader xmlTextReader = new XmlTextReader(textReader);
		xmlTextReader.WhitespaceHandling = WhitespaceHandling.Significant;
		xmlTextReader.Normalization = true;
		xmlTextReader.XmlResolver = null;
		return Deserialize(xmlTextReader, null);
	}

	/// <summary>Deserializes the XML document contained by the specified <see cref="T:System.xml.XmlReader" />.</summary>
	/// <returns>The <see cref="T:System.Object" /> being deserialized.</returns>
	/// <param name="xmlReader">The <see cref="T:System.xml.XmlReader" /> that contains the XML document to deserialize. </param>
	/// <exception cref="T:System.InvalidOperationException">An error occurred during deserialization. The original exception is available using the <see cref="P:System.Exception.InnerException" /> property. </exception>
	public object Deserialize(XmlReader xmlReader)
	{
		return Deserialize(xmlReader, null);
	}

	/// <summary>Deserializes an XML document contained by the specified <see cref="T:System.Xml.XmlReader" /> and allows the overriding of events that occur during deserialization.</summary>
	/// <returns>The <see cref="T:System.Object" /> being deserialized.</returns>
	/// <param name="xmlReader">The <see cref="T:System.Xml.XmlReader" /> that contains the document to deserialize.</param>
	/// <param name="events">An instance of the <see cref="T:System.Xml.Serialization.XmlDeserializationEvents" /> class. </param>
	public object Deserialize(XmlReader xmlReader, XmlDeserializationEvents events)
	{
		return Deserialize(xmlReader, null, events);
	}

	/// <summary>Deserializes the XML document contained by the specified <see cref="T:System.xml.XmlReader" /> and encoding style.</summary>
	/// <returns>The deserialized object.</returns>
	/// <param name="xmlReader">The <see cref="T:System.xml.XmlReader" /> that contains the XML document to deserialize. </param>
	/// <param name="encodingStyle">The encoding style of the serialized XML. </param>
	/// <exception cref="T:System.InvalidOperationException">An error occurred during deserialization. The original exception is available using the <see cref="P:System.Exception.InnerException" /> property. </exception>
	public object Deserialize(XmlReader xmlReader, string encodingStyle)
	{
		return Deserialize(xmlReader, encodingStyle, events);
	}

	/// <summary>Deserializes the object using the data contained by the specified <see cref="T:System.Xml.XmlReader" />.</summary>
	/// <returns>The object being deserialized.</returns>
	/// <param name="xmlReader">An instance of the <see cref="T:System.Xml.XmlReader" /> class used to read the document.</param>
	/// <param name="encodingStyle">The encoding used.</param>
	/// <param name="events">An instance of the <see cref="T:System.Xml.Serialization.XmlDeserializationEvents" /> class. </param>
	public object Deserialize(XmlReader xmlReader, string encodingStyle, XmlDeserializationEvents events)
	{
		events.sender = this;
		try
		{
			if (primitiveType != null)
			{
				if (encodingStyle != null && encodingStyle.Length > 0)
				{
					throw new InvalidOperationException(Res.GetString("The encoding style '{0}' is not valid for this call because this XmlSerializer instance does not support encoding. Use the SoapReflectionImporter to initialize an XmlSerializer that supports encoding.", encodingStyle));
				}
				return DeserializePrimitive(xmlReader, events);
			}
			if (tempAssembly == null || typedSerializer)
			{
				XmlSerializationReader xmlSerializationReader = CreateReader();
				xmlSerializationReader.Init(xmlReader, events, encodingStyle, tempAssembly);
				try
				{
					return Deserialize(xmlSerializationReader);
				}
				finally
				{
					xmlSerializationReader.Dispose();
				}
			}
			return tempAssembly.InvokeReader(mapping, xmlReader, events, encodingStyle);
		}
		catch (Exception innerException)
		{
			if (innerException is ThreadAbortException || innerException is StackOverflowException || innerException is OutOfMemoryException)
			{
				throw;
			}
			if (innerException is TargetInvocationException)
			{
				innerException = innerException.InnerException;
			}
			if (xmlReader is IXmlLineInfo)
			{
				IXmlLineInfo xmlLineInfo = (IXmlLineInfo)xmlReader;
				throw new InvalidOperationException(Res.GetString("There is an error in XML document ({0}, {1}).", xmlLineInfo.LineNumber.ToString(CultureInfo.InvariantCulture), xmlLineInfo.LinePosition.ToString(CultureInfo.InvariantCulture)), innerException);
			}
			throw new InvalidOperationException(Res.GetString("There is an error in the XML document."), innerException);
		}
	}

	/// <summary>Gets a value that indicates whether this <see cref="T:System.Xml.Serialization.XmlSerializer" /> can deserialize a specified XML document.</summary>
	/// <returns>true if this <see cref="T:System.Xml.Serialization.XmlSerializer" /> can deserialize the object that the <see cref="T:System.Xml.XmlReader" /> points to; otherwise, false.</returns>
	/// <param name="xmlReader">An <see cref="T:System.Xml.XmlReader" /> that points to the document to deserialize. </param>
	public virtual bool CanDeserialize(XmlReader xmlReader)
	{
		if (primitiveType != null)
		{
			TypeDesc typeDesc = (TypeDesc)TypeScope.PrimtiveTypes[primitiveType];
			return xmlReader.IsStartElement(typeDesc.DataType.Name, string.Empty);
		}
		if (tempAssembly != null)
		{
			return tempAssembly.CanRead(mapping, xmlReader);
		}
		return false;
	}

	/// <summary>Returns an array of <see cref="T:System.Xml.Serialization.XmlSerializer" /> objects created from an array of <see cref="T:System.Xml.Serialization.XmlTypeMapping" /> objects.</summary>
	/// <returns>An array of <see cref="T:System.Xml.Serialization.XmlSerializer" /> objects.</returns>
	/// <param name="mappings">An array of <see cref="T:System.Xml.Serialization.XmlTypeMapping" /> that maps one type to another. </param>
	[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
	public static XmlSerializer[] FromMappings(XmlMapping[] mappings)
	{
		return FromMappings(mappings, (Type)null);
	}

	/// <summary>Returns an instance of the <see cref="T:System.Xml.Serialization.XmlSerializer" /> class from the specified mappings.</summary>
	/// <returns>An instance of the <see cref="T:System.Xml.Serialization.XmlSerializer" /> class.</returns>
	/// <param name="mappings">An array of <see cref="T:System.Xml.Serialization.XmlMapping" /> objects.</param>
	/// <param name="type">The <see cref="T:System.Type" /> of the deserialized object.</param>
	[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
	public static XmlSerializer[] FromMappings(XmlMapping[] mappings, Type type)
	{
		if (mappings == null || mappings.Length == 0)
		{
			return new XmlSerializer[0];
		}
		XmlSerializerImplementation contract = null;
		Assembly obj = ((type == null) ? null : TempAssembly.LoadGeneratedAssembly(type, null, out contract));
		TempAssembly tempAssembly = null;
		if (obj == null)
		{
			if (XmlMapping.IsShallow(mappings))
			{
				return new XmlSerializer[0];
			}
			if (type == null)
			{
				tempAssembly = new TempAssembly(mappings, new Type[1] { type }, null, null, null);
				XmlSerializer[] array = new XmlSerializer[mappings.Length];
				contract = tempAssembly.Contract;
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = (XmlSerializer)contract.TypedSerializers[mappings[i].Key];
					array[i].SetTempAssembly(tempAssembly, mappings[i]);
				}
				return array;
			}
			return GetSerializersFromCache(mappings, type);
		}
		XmlSerializer[] array2 = new XmlSerializer[mappings.Length];
		for (int j = 0; j < array2.Length; j++)
		{
			array2[j] = (XmlSerializer)contract.TypedSerializers[mappings[j].Key];
		}
		return array2;
	}

	private static XmlSerializer[] GetSerializersFromCache(XmlMapping[] mappings, Type type)
	{
		XmlSerializer[] array = new XmlSerializer[mappings.Length];
		Hashtable hashtable = null;
		lock (xmlSerializerTable)
		{
			hashtable = xmlSerializerTable[type] as Hashtable;
			if (hashtable == null)
			{
				hashtable = new Hashtable();
				xmlSerializerTable[type] = hashtable;
			}
		}
		lock (hashtable)
		{
			Hashtable hashtable2 = new Hashtable();
			for (int i = 0; i < mappings.Length; i++)
			{
				XmlSerializerMappingKey key = new XmlSerializerMappingKey(mappings[i]);
				array[i] = hashtable[key] as XmlSerializer;
				if (array[i] == null)
				{
					hashtable2.Add(key, i);
				}
			}
			if (hashtable2.Count > 0)
			{
				XmlMapping[] array2 = new XmlMapping[hashtable2.Count];
				int num = 0;
				foreach (XmlSerializerMappingKey key2 in hashtable2.Keys)
				{
					array2[num++] = key2.Mapping;
				}
				TempAssembly tempAssembly = new TempAssembly(array2, new Type[1] { type }, null, null, null);
				XmlSerializerImplementation contract = tempAssembly.Contract;
				foreach (XmlSerializerMappingKey key3 in hashtable2.Keys)
				{
					num = (int)hashtable2[key3];
					array[num] = (XmlSerializer)contract.TypedSerializers[key3.Mapping.Key];
					array[num].SetTempAssembly(tempAssembly, key3.Mapping);
					hashtable[key3] = array[num];
				}
			}
		}
		return array;
	}

	/// <summary>Returns an instance of the <see cref="T:System.Xml.Serialization.XmlSerializer" /> class created from mappings of one XML type to another.</summary>
	/// <returns>An instance of the <see cref="T:System.Xml.Serialization.XmlSerializer" /> class.</returns>
	/// <param name="mappings">An array of <see cref="T:System.Xml.Serialization.XmlMapping" /> objects used to map one type to another.</param>
	/// <param name="evidence">An instance of the <see cref="T:System.Security.Policy.Evidence" /> class that contains host and assembly data presented to the common language runtime policy system.</param>
	[Obsolete("This method is obsolete and will be removed in a future release of the .NET Framework. Please use an overload of FromMappings which does not take an Evidence parameter. See http://go2.microsoft.com/fwlink/?LinkId=131738 for more information.")]
	[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
	public static XmlSerializer[] FromMappings(XmlMapping[] mappings, Evidence evidence)
	{
		if (mappings == null || mappings.Length == 0)
		{
			return new XmlSerializer[0];
		}
		if (XmlMapping.IsShallow(mappings))
		{
			return new XmlSerializer[0];
		}
		XmlSerializerImplementation contract = new TempAssembly(mappings, new Type[0], null, null, evidence).Contract;
		XmlSerializer[] array = new XmlSerializer[mappings.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = (XmlSerializer)contract.TypedSerializers[mappings[i].Key];
		}
		return array;
	}

	/// <summary>Returns an assembly that contains custom-made serializers used to serialize or deserialize the specified type or types, using the specified mappings.</summary>
	/// <returns>An <see cref="T:System.Reflection.Assembly" /> object that contains serializers for the supplied types and mappings.</returns>
	/// <param name="types">A collection of types.</param>
	/// <param name="mappings">A collection of <see cref="T:System.Xml.Serialization.XmlMapping" /> objects used to convert one type to another.</param>
	[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
	public static Assembly GenerateSerializer(Type[] types, XmlMapping[] mappings)
	{
		CompilerParameters compilerParameters = new CompilerParameters();
		compilerParameters.TempFiles = new TempFileCollection();
		compilerParameters.GenerateInMemory = false;
		compilerParameters.IncludeDebugInformation = false;
		return GenerateSerializer(types, mappings, compilerParameters);
	}

	/// <summary>Returns an assembly that contains custom-made serializers used to serialize or deserialize the specified type or types, using the specified mappings and compiler settings and options. </summary>
	/// <returns>An <see cref="T:System.Reflection.Assembly" /> that contains special versions of the <see cref="T:System.Xml.Serialization.XmlSerializer" />.</returns>
	/// <param name="types">An array of type <see cref="T:System.Type" /> that contains objects used to serialize and deserialize data.</param>
	/// <param name="mappings">An array of type <see cref="T:System.Xml.Serialization.XmlMapping" /> that maps the XML data to the type data.</param>
	/// <param name="parameters">An instance of the <see cref="T:System.CodeDom.Compiler.CompilerParameters" /> class that represents the parameters used to invoke a compiler.</param>
	[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	public static Assembly GenerateSerializer(Type[] types, XmlMapping[] mappings, CompilerParameters parameters)
	{
		if (types == null || types.Length == 0)
		{
			return null;
		}
		if (mappings == null)
		{
			throw new ArgumentNullException("mappings");
		}
		if (XmlMapping.IsShallow(mappings))
		{
			throw new InvalidOperationException(Res.GetString("This mapping was not crated by reflection importer and cannot be used in this context."));
		}
		Assembly assembly = null;
		foreach (Type type in types)
		{
			if (DynamicAssemblies.IsTypeDynamic(type))
			{
				throw new InvalidOperationException(Res.GetString("Cannot pre-generate serialization assembly for type '{0}'. Pre-generation of serialization assemblies is not supported for dynamic types. Save the assembly and load it from disk to use it with XmlSerialization.", type.FullName));
			}
			if (assembly == null)
			{
				assembly = type.Assembly;
			}
			else if (type.Assembly != assembly)
			{
				throw new ArgumentException(Res.GetString("Cannot pre-generate serializer for multiple assemblies. Type '{0}' does not belong to assembly {1}.", type.FullName, assembly.Location), "types");
			}
		}
		return TempAssembly.GenerateAssembly(mappings, types, null, null, XmlSerializerCompilerParameters.Create(parameters, needTempDirAccess: true), assembly, new Hashtable());
	}

	/// <summary>Returns an array of <see cref="T:System.Xml.Serialization.XmlSerializer" /> objects created from an array of types.</summary>
	/// <returns>An array of <see cref="T:System.Xml.Serialization.XmlSerializer" /> objects.</returns>
	/// <param name="types">An array of <see cref="T:System.Type" /> objects. </param>
	public static XmlSerializer[] FromTypes(Type[] types)
	{
		if (types == null)
		{
			return new XmlSerializer[0];
		}
		XmlReflectionImporter xmlReflectionImporter = new XmlReflectionImporter();
		XmlTypeMapping[] array = new XmlTypeMapping[types.Length];
		for (int i = 0; i < types.Length; i++)
		{
			array[i] = xmlReflectionImporter.ImportTypeMapping(types[i]);
		}
		return FromMappings(array);
	}

	/// <summary>Returns the name of the assembly that contains one or more versions of the <see cref="T:System.Xml.Serialization.XmlSerializer" /> especially created to serialize or deserialize the specified type.</summary>
	/// <returns>The name of the assembly that contains an <see cref="T:System.Xml.Serialization.XmlSerializer" /> for the type.</returns>
	/// <param name="type">The <see cref="T:System.Type" /> you are deserializing.</param>
	[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	public static string GetXmlSerializerAssemblyName(Type type)
	{
		return GetXmlSerializerAssemblyName(type, null);
	}

	/// <summary>Returns the name of the assembly that contains the serializer for the specified type in the specified namespace.</summary>
	/// <returns>The name of the assembly that contains specially built serializers.</returns>
	/// <param name="type">The <see cref="T:System.Type" /> you are interested in.</param>
	/// <param name="defaultNamespace">The namespace of the type.</param>
	[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	public static string GetXmlSerializerAssemblyName(Type type, string defaultNamespace)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		return Compiler.GetTempAssemblyName(type.Assembly.GetName(), defaultNamespace);
	}

	/// <summary>Returns an object used to read the XML document to be serialized.</summary>
	/// <returns>An <see cref="T:System.Xml.Serialization.XmlSerializationReader" /> used to read the XML document.</returns>
	/// <exception cref="T:System.NotImplementedException">Any attempt is made to access the method when the method is not overridden in a descendant class. </exception>
	protected virtual XmlSerializationReader CreateReader()
	{
		throw new NotImplementedException();
	}

	/// <summary>Deserializes the XML document contained by the specified <see cref="T:System.Xml.Serialization.XmlSerializationReader" />.</summary>
	/// <returns>The deserialized object.</returns>
	/// <param name="reader">The <see cref="T:System.Xml.Serialization.XmlSerializationReader" /> that contains the XML document to deserialize. </param>
	/// <exception cref="T:System.NotImplementedException">Any attempt is made to access the method when the method is not overridden in a descendant class. </exception>
	protected virtual object Deserialize(XmlSerializationReader reader)
	{
		throw new NotImplementedException();
	}

	/// <summary>When overridden in a derived class, returns a writer used to serialize the object.</summary>
	/// <returns>An instance that implements the <see cref="T:System.Xml.Serialization.XmlSerializationWriter" /> class.</returns>
	/// <exception cref="T:System.NotImplementedException">Any attempt is made to access the method when the method is not overridden in a descendant class. </exception>
	protected virtual XmlSerializationWriter CreateWriter()
	{
		throw new NotImplementedException();
	}

	/// <summary>Serializes the specified <see cref="T:System.Object" /> and writes the XML document to a file using the specified <see cref="T:System.Xml.Serialization.XmlSerializationWriter" />.</summary>
	/// <param name="o">The <see cref="T:System.Object" /> to serialize. </param>
	/// <param name="writer">The <see cref="T:System.Xml.Serialization.XmlSerializationWriter" /> used to write the XML document. </param>
	/// <exception cref="T:System.NotImplementedException">Any attempt is made to access the method when the method is not overridden in a descendant class. </exception>
	protected virtual void Serialize(object o, XmlSerializationWriter writer)
	{
		throw new NotImplementedException();
	}

	internal void SetTempAssembly(TempAssembly tempAssembly, XmlMapping mapping)
	{
		this.tempAssembly = tempAssembly;
		this.mapping = mapping;
		typedSerializer = true;
	}

	private static XmlTypeMapping GetKnownMapping(Type type, string ns)
	{
		if (ns != null && ns != string.Empty)
		{
			return null;
		}
		TypeDesc typeDesc = (TypeDesc)TypeScope.PrimtiveTypes[type];
		if (typeDesc == null)
		{
			return null;
		}
		ElementAccessor elementAccessor = new ElementAccessor();
		elementAccessor.Name = typeDesc.DataType.Name;
		XmlTypeMapping xmlTypeMapping = new XmlTypeMapping(null, elementAccessor);
		xmlTypeMapping.SetKeyInternal(XmlMapping.GenerateKey(type, null, null));
		return xmlTypeMapping;
	}

	private void SerializePrimitive(XmlWriter xmlWriter, object o, XmlSerializerNamespaces namespaces)
	{
		XmlSerializationPrimitiveWriter xmlSerializationPrimitiveWriter = new XmlSerializationPrimitiveWriter();
		xmlSerializationPrimitiveWriter.Init(xmlWriter, namespaces, null, null, null);
		switch (Type.GetTypeCode(primitiveType))
		{
		case TypeCode.String:
			xmlSerializationPrimitiveWriter.Write_string(o);
			return;
		case TypeCode.Int32:
			xmlSerializationPrimitiveWriter.Write_int(o);
			return;
		case TypeCode.Boolean:
			xmlSerializationPrimitiveWriter.Write_boolean(o);
			return;
		case TypeCode.Int16:
			xmlSerializationPrimitiveWriter.Write_short(o);
			return;
		case TypeCode.Int64:
			xmlSerializationPrimitiveWriter.Write_long(o);
			return;
		case TypeCode.Single:
			xmlSerializationPrimitiveWriter.Write_float(o);
			return;
		case TypeCode.Double:
			xmlSerializationPrimitiveWriter.Write_double(o);
			return;
		case TypeCode.Decimal:
			xmlSerializationPrimitiveWriter.Write_decimal(o);
			return;
		case TypeCode.DateTime:
			xmlSerializationPrimitiveWriter.Write_dateTime(o);
			return;
		case TypeCode.Char:
			xmlSerializationPrimitiveWriter.Write_char(o);
			return;
		case TypeCode.Byte:
			xmlSerializationPrimitiveWriter.Write_unsignedByte(o);
			return;
		case TypeCode.SByte:
			xmlSerializationPrimitiveWriter.Write_byte(o);
			return;
		case TypeCode.UInt16:
			xmlSerializationPrimitiveWriter.Write_unsignedShort(o);
			return;
		case TypeCode.UInt32:
			xmlSerializationPrimitiveWriter.Write_unsignedInt(o);
			return;
		case TypeCode.UInt64:
			xmlSerializationPrimitiveWriter.Write_unsignedLong(o);
			return;
		}
		if (primitiveType == typeof(XmlQualifiedName))
		{
			xmlSerializationPrimitiveWriter.Write_QName(o);
			return;
		}
		if (primitiveType == typeof(byte[]))
		{
			xmlSerializationPrimitiveWriter.Write_base64Binary(o);
			return;
		}
		if (primitiveType == typeof(Guid))
		{
			xmlSerializationPrimitiveWriter.Write_guid(o);
			return;
		}
		if (primitiveType == typeof(TimeSpan))
		{
			xmlSerializationPrimitiveWriter.Write_TimeSpan(o);
			return;
		}
		throw new InvalidOperationException(Res.GetString("The type {0} was not expected. Use the XmlInclude or SoapInclude attribute to specify types that are not known statically.", primitiveType.FullName));
	}

	private object DeserializePrimitive(XmlReader xmlReader, XmlDeserializationEvents events)
	{
		XmlSerializationPrimitiveReader xmlSerializationPrimitiveReader = new XmlSerializationPrimitiveReader();
		xmlSerializationPrimitiveReader.Init(xmlReader, events, null, null);
		switch (Type.GetTypeCode(primitiveType))
		{
		case TypeCode.String:
			return xmlSerializationPrimitiveReader.Read_string();
		case TypeCode.Int32:
			return xmlSerializationPrimitiveReader.Read_int();
		case TypeCode.Boolean:
			return xmlSerializationPrimitiveReader.Read_boolean();
		case TypeCode.Int16:
			return xmlSerializationPrimitiveReader.Read_short();
		case TypeCode.Int64:
			return xmlSerializationPrimitiveReader.Read_long();
		case TypeCode.Single:
			return xmlSerializationPrimitiveReader.Read_float();
		case TypeCode.Double:
			return xmlSerializationPrimitiveReader.Read_double();
		case TypeCode.Decimal:
			return xmlSerializationPrimitiveReader.Read_decimal();
		case TypeCode.DateTime:
			return xmlSerializationPrimitiveReader.Read_dateTime();
		case TypeCode.Char:
			return xmlSerializationPrimitiveReader.Read_char();
		case TypeCode.Byte:
			return xmlSerializationPrimitiveReader.Read_unsignedByte();
		case TypeCode.SByte:
			return xmlSerializationPrimitiveReader.Read_byte();
		case TypeCode.UInt16:
			return xmlSerializationPrimitiveReader.Read_unsignedShort();
		case TypeCode.UInt32:
			return xmlSerializationPrimitiveReader.Read_unsignedInt();
		case TypeCode.UInt64:
			return xmlSerializationPrimitiveReader.Read_unsignedLong();
		default:
			if (primitiveType == typeof(XmlQualifiedName))
			{
				return xmlSerializationPrimitiveReader.Read_QName();
			}
			if (primitiveType == typeof(byte[]))
			{
				return xmlSerializationPrimitiveReader.Read_base64Binary();
			}
			if (primitiveType == typeof(Guid))
			{
				return xmlSerializationPrimitiveReader.Read_guid();
			}
			if (primitiveType == typeof(TimeSpan) && System.LocalAppContextSwitches.EnableTimeSpanSerialization)
			{
				return xmlSerializationPrimitiveReader.Read_TimeSpan();
			}
			throw new InvalidOperationException(Res.GetString("The type {0} was not expected. Use the XmlInclude or SoapInclude attribute to specify types that are not known statically.", primitiveType.FullName));
		}
	}
}
