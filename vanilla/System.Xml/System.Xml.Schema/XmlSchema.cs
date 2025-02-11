using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Xml.Serialization;
using System.Xml.XmlConfiguration;

namespace System.Xml.Schema;

/// <summary>An in-memory representation of an XML Schema as specified in the World Wide Web Consortium (W3C) XML Schema Part 1: Structures and XML Schema Part 2: Datatypes specifications.</summary>
[XmlRoot("schema", Namespace = "http://www.w3.org/2001/XMLSchema")]
public class XmlSchema : XmlSchemaObject
{
	/// <summary>The XML schema namespace. This field is constant.</summary>
	public const string Namespace = "http://www.w3.org/2001/XMLSchema";

	/// <summary>The XML schema instance namespace. This field is constant. </summary>
	public const string InstanceNamespace = "http://www.w3.org/2001/XMLSchema-instance";

	private XmlSchemaForm attributeFormDefault;

	private XmlSchemaForm elementFormDefault;

	private XmlSchemaDerivationMethod blockDefault = XmlSchemaDerivationMethod.None;

	private XmlSchemaDerivationMethod finalDefault = XmlSchemaDerivationMethod.None;

	private string targetNs;

	private string version;

	private XmlSchemaObjectCollection includes = new XmlSchemaObjectCollection();

	private XmlSchemaObjectCollection items = new XmlSchemaObjectCollection();

	private string id;

	private XmlAttribute[] moreAttributes;

	private bool isCompiled;

	private bool isCompiledBySet;

	private bool isPreprocessed;

	private bool isRedefined;

	private int errorCount;

	private XmlSchemaObjectTable attributes;

	private XmlSchemaObjectTable attributeGroups = new XmlSchemaObjectTable();

	private XmlSchemaObjectTable elements = new XmlSchemaObjectTable();

	private XmlSchemaObjectTable types = new XmlSchemaObjectTable();

	private XmlSchemaObjectTable groups = new XmlSchemaObjectTable();

	private XmlSchemaObjectTable notations = new XmlSchemaObjectTable();

	private XmlSchemaObjectTable identityConstraints = new XmlSchemaObjectTable();

	private static int globalIdCounter = -1;

	private ArrayList importedSchemas;

	private ArrayList importedNamespaces;

	private int schemaId = -1;

	private Uri baseUri;

	private bool isChameleon;

	private Hashtable ids = new Hashtable();

	private XmlDocument document;

	private XmlNameTable nameTable;

	/// <summary>Gets or sets the form for attributes declared in the target namespace of the schema.</summary>
	/// <returns>The <see cref="T:System.Xml.Schema.XmlSchemaForm" /> value that indicates if attributes from the target namespace are required to be qualified with the namespace prefix. The default is <see cref="F:System.Xml.Schema.XmlSchemaForm.None" />.</returns>
	[DefaultValue(XmlSchemaForm.None)]
	[XmlAttribute("attributeFormDefault")]
	public XmlSchemaForm AttributeFormDefault
	{
		get
		{
			return attributeFormDefault;
		}
		set
		{
			attributeFormDefault = value;
		}
	}

	/// <summary>Gets or sets the blockDefault attribute which sets the default value of the block attribute on element and complex types in the targetNamespace of the schema.</summary>
	/// <returns>An <see cref="T:System.Xml.Schema.XmlSchemaDerivationMethod" /> value representing the different methods for preventing derivation. The default value is XmlSchemaDerivationMethod.None.</returns>
	[DefaultValue(XmlSchemaDerivationMethod.None)]
	[XmlAttribute("blockDefault")]
	public XmlSchemaDerivationMethod BlockDefault
	{
		get
		{
			return blockDefault;
		}
		set
		{
			blockDefault = value;
		}
	}

	/// <summary>Gets or sets the finalDefault attribute which sets the default value of the final attribute on elements and complex types in the target namespace of the schema.</summary>
	/// <returns>An <see cref="T:System.Xml.Schema.XmlSchemaDerivationMethod" /> value representing the different methods for preventing derivation. The default value is XmlSchemaDerivationMethod.None.</returns>
	[DefaultValue(XmlSchemaDerivationMethod.None)]
	[XmlAttribute("finalDefault")]
	public XmlSchemaDerivationMethod FinalDefault
	{
		get
		{
			return finalDefault;
		}
		set
		{
			finalDefault = value;
		}
	}

	/// <summary>Gets or sets the form for elements declared in the target namespace of the schema.</summary>
	/// <returns>The <see cref="T:System.Xml.Schema.XmlSchemaForm" /> value that indicates if elements from the target namespace are required to be qualified with the namespace prefix. The default is <see cref="F:System.Xml.Schema.XmlSchemaForm.None" />.</returns>
	[DefaultValue(XmlSchemaForm.None)]
	[XmlAttribute("elementFormDefault")]
	public XmlSchemaForm ElementFormDefault
	{
		get
		{
			return elementFormDefault;
		}
		set
		{
			elementFormDefault = value;
		}
	}

	/// <summary>Gets or sets the Uniform Resource Identifier (URI) of the schema target namespace.</summary>
	/// <returns>The schema target namespace.</returns>
	[XmlAttribute("targetNamespace", DataType = "anyURI")]
	public string TargetNamespace
	{
		get
		{
			return targetNs;
		}
		set
		{
			targetNs = value;
		}
	}

	/// <summary>Gets or sets the version of the schema.</summary>
	/// <returns>The version of the schema. The default value is String.Empty.</returns>
	[XmlAttribute("version", DataType = "token")]
	public string Version
	{
		get
		{
			return version;
		}
		set
		{
			version = value;
		}
	}

	/// <summary>Gets the collection of included and imported schemas.</summary>
	/// <returns>An <see cref="T:System.Xml.Schema.XmlSchemaObjectCollection" /> of the included and imported schemas.</returns>
	[XmlElement("include", typeof(XmlSchemaInclude))]
	[XmlElement("import", typeof(XmlSchemaImport))]
	[XmlElement("redefine", typeof(XmlSchemaRedefine))]
	public XmlSchemaObjectCollection Includes => includes;

	/// <summary>Gets the collection of schema elements in the schema and is used to add new element types at the schema element level.</summary>
	/// <returns>An <see cref="T:System.Xml.Schema.XmlSchemaObjectCollection" /> of schema elements in the schema.</returns>
	[XmlElement("notation", typeof(XmlSchemaNotation))]
	[XmlElement("group", typeof(XmlSchemaGroup))]
	[XmlElement("element", typeof(XmlSchemaElement))]
	[XmlElement("attribute", typeof(XmlSchemaAttribute))]
	[XmlElement("attributeGroup", typeof(XmlSchemaAttributeGroup))]
	[XmlElement("complexType", typeof(XmlSchemaComplexType))]
	[XmlElement("simpleType", typeof(XmlSchemaSimpleType))]
	[XmlElement("annotation", typeof(XmlSchemaAnnotation))]
	public XmlSchemaObjectCollection Items => items;

	/// <summary>Indicates if the schema has been compiled.</summary>
	/// <returns>true if schema has been compiled, otherwise, false. The default value is false.</returns>
	[XmlIgnore]
	public bool IsCompiled
	{
		get
		{
			if (!isCompiled)
			{
				return isCompiledBySet;
			}
			return true;
		}
	}

	[XmlIgnore]
	internal bool IsCompiledBySet
	{
		get
		{
			return isCompiledBySet;
		}
		set
		{
			isCompiledBySet = value;
		}
	}

	[XmlIgnore]
	internal bool IsPreprocessed
	{
		get
		{
			return isPreprocessed;
		}
		set
		{
			isPreprocessed = value;
		}
	}

	[XmlIgnore]
	internal bool IsRedefined
	{
		get
		{
			return isRedefined;
		}
		set
		{
			isRedefined = value;
		}
	}

	/// <summary>Gets the post-schema-compilation value for all the attributes in the schema.</summary>
	/// <returns>An <see cref="T:System.Xml.Schema.XmlSchemaObjectTable" /> collection of all the attributes in the schema.</returns>
	[XmlIgnore]
	public XmlSchemaObjectTable Attributes
	{
		get
		{
			if (attributes == null)
			{
				attributes = new XmlSchemaObjectTable();
			}
			return attributes;
		}
	}

	/// <summary>Gets the post-schema-compilation value of all the global attribute groups in the schema.</summary>
	/// <returns>An <see cref="T:System.Xml.Schema.XmlSchemaObjectTable" /> collection of all the global attribute groups in the schema.</returns>
	[XmlIgnore]
	public XmlSchemaObjectTable AttributeGroups
	{
		get
		{
			if (attributeGroups == null)
			{
				attributeGroups = new XmlSchemaObjectTable();
			}
			return attributeGroups;
		}
	}

	/// <summary>Gets the post-schema-compilation value of all schema types in the schema.</summary>
	/// <returns>An <see cref="T:System.Xml.Schema.XmlSchemaObjectCollection" /> of all schema types in the schema.</returns>
	[XmlIgnore]
	public XmlSchemaObjectTable SchemaTypes
	{
		get
		{
			if (types == null)
			{
				types = new XmlSchemaObjectTable();
			}
			return types;
		}
	}

	/// <summary>Gets the post-schema-compilation value for all the elements in the schema.</summary>
	/// <returns>An <see cref="T:System.Xml.Schema.XmlSchemaObjectTable" /> collection of all the elements in the schema.</returns>
	[XmlIgnore]
	public XmlSchemaObjectTable Elements
	{
		get
		{
			if (elements == null)
			{
				elements = new XmlSchemaObjectTable();
			}
			return elements;
		}
	}

	/// <summary>Gets or sets the string ID.</summary>
	/// <returns>The ID of the string. The default value is String.Empty.</returns>
	[XmlAttribute("id", DataType = "ID")]
	public string Id
	{
		get
		{
			return id;
		}
		set
		{
			id = value;
		}
	}

	/// <summary>Gets and sets the qualified attributes which do not belong to the schema target namespace.</summary>
	/// <returns>An array of qualified <see cref="T:System.Xml.XmlAttribute" /> objects that do not belong to the schema target namespace.</returns>
	[XmlAnyAttribute]
	public XmlAttribute[] UnhandledAttributes
	{
		get
		{
			return moreAttributes;
		}
		set
		{
			moreAttributes = value;
		}
	}

	/// <summary>Gets the post-schema-compilation value of all the groups in the schema.</summary>
	/// <returns>An <see cref="T:System.Xml.Schema.XmlSchemaObjectTable" /> collection of all the groups in the schema.</returns>
	[XmlIgnore]
	public XmlSchemaObjectTable Groups => groups;

	/// <summary>Gets the post-schema-compilation value for all notations in the schema.</summary>
	/// <returns>An <see cref="T:System.Xml.Schema.XmlSchemaObjectTable" /> collection of all notations in the schema.</returns>
	[XmlIgnore]
	public XmlSchemaObjectTable Notations => notations;

	[XmlIgnore]
	internal XmlSchemaObjectTable IdentityConstraints => identityConstraints;

	[XmlIgnore]
	internal Uri BaseUri
	{
		get
		{
			return baseUri;
		}
		set
		{
			baseUri = value;
		}
	}

	[XmlIgnore]
	internal int SchemaId
	{
		get
		{
			if (schemaId == -1)
			{
				schemaId = Interlocked.Increment(ref globalIdCounter);
			}
			return schemaId;
		}
	}

	[XmlIgnore]
	internal bool IsChameleon
	{
		get
		{
			return isChameleon;
		}
		set
		{
			isChameleon = value;
		}
	}

	[XmlIgnore]
	internal Hashtable Ids => ids;

	[XmlIgnore]
	internal XmlDocument Document
	{
		get
		{
			if (document == null)
			{
				document = new XmlDocument();
			}
			return document;
		}
	}

	[XmlIgnore]
	internal int ErrorCount
	{
		get
		{
			return errorCount;
		}
		set
		{
			errorCount = value;
		}
	}

	[XmlIgnore]
	internal override string IdAttribute
	{
		get
		{
			return Id;
		}
		set
		{
			Id = value;
		}
	}

	internal XmlNameTable NameTable
	{
		get
		{
			if (nameTable == null)
			{
				nameTable = new NameTable();
			}
			return nameTable;
		}
	}

	internal ArrayList ImportedSchemas
	{
		get
		{
			if (importedSchemas == null)
			{
				importedSchemas = new ArrayList();
			}
			return importedSchemas;
		}
	}

	internal ArrayList ImportedNamespaces
	{
		get
		{
			if (importedNamespaces == null)
			{
				importedNamespaces = new ArrayList();
			}
			return importedNamespaces;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchema" /> class.</summary>
	public XmlSchema()
	{
	}

	/// <summary>Reads an XML Schema from the supplied <see cref="T:System.IO.TextReader" />.</summary>
	/// <returns>The <see cref="T:System.Xml.Schema.XmlSchema" /> object representing the XML Schema.</returns>
	/// <param name="reader">The TextReader containing the XML Schema to read. </param>
	/// <param name="validationEventHandler">The validation event handler that receives information about the XML Schema syntax errors. </param>
	/// <exception cref="T:System.Xml.Schema.XmlSchemaException">An <see cref="T:System.Xml.Schema.XmlSchemaException" /> is raised if no <see cref="T:System.Xml.Schema.ValidationEventHandler" /> is specified.</exception>
	public static XmlSchema Read(TextReader reader, ValidationEventHandler validationEventHandler)
	{
		return Read(new XmlTextReader(reader), validationEventHandler);
	}

	/// <summary>Reads an XML Schema  from the supplied stream.</summary>
	/// <returns>The <see cref="T:System.Xml.Schema.XmlSchema" /> object representing the XML Schema.</returns>
	/// <param name="stream">The supplied data stream. </param>
	/// <param name="validationEventHandler">The validation event handler that receives information about XML Schema syntax errors. </param>
	/// <exception cref="T:System.Xml.Schema.XmlSchemaException">An <see cref="T:System.Xml.Schema.XmlSchemaException" /> is raised if no <see cref="T:System.Xml.Schema.ValidationEventHandler" /> is specified.</exception>
	public static XmlSchema Read(Stream stream, ValidationEventHandler validationEventHandler)
	{
		return Read(new XmlTextReader(stream), validationEventHandler);
	}

	/// <summary>Reads an XML Schema from the supplied <see cref="T:System.Xml.XmlReader" />.</summary>
	/// <returns>The <see cref="T:System.Xml.Schema.XmlSchema" /> object representing the XML Schema.</returns>
	/// <param name="reader">The XmlReader containing the XML Schema to read. </param>
	/// <param name="validationEventHandler">The validation event handler that receives information about the XML Schema syntax errors. </param>
	/// <exception cref="T:System.Xml.Schema.XmlSchemaException">An <see cref="T:System.Xml.Schema.XmlSchemaException" /> is raised if no <see cref="T:System.Xml.Schema.ValidationEventHandler" /> is specified.</exception>
	public static XmlSchema Read(XmlReader reader, ValidationEventHandler validationEventHandler)
	{
		XmlNameTable xmlNameTable = reader.NameTable;
		Parser parser = new Parser(SchemaType.XSD, xmlNameTable, new SchemaNames(xmlNameTable), validationEventHandler);
		try
		{
			parser.Parse(reader, null);
		}
		catch (XmlSchemaException ex)
		{
			if (validationEventHandler != null)
			{
				validationEventHandler(null, new ValidationEventArgs(ex));
				return null;
			}
			throw ex;
		}
		return parser.XmlSchema;
	}

	/// <summary>Writes the XML Schema to the supplied data stream.</summary>
	/// <param name="stream">The supplied data stream. </param>
	public void Write(Stream stream)
	{
		Write(stream, null);
	}

	/// <summary>Writes the XML Schema to the supplied <see cref="T:System.IO.Stream" /> using the <see cref="T:System.Xml.XmlNamespaceManager" /> specified.</summary>
	/// <param name="stream">The supplied data stream. </param>
	/// <param name="namespaceManager">The <see cref="T:System.Xml.XmlNamespaceManager" />.</param>
	public void Write(Stream stream, XmlNamespaceManager namespaceManager)
	{
		XmlTextWriter xmlTextWriter = new XmlTextWriter(stream, null);
		xmlTextWriter.Formatting = Formatting.Indented;
		Write(xmlTextWriter, namespaceManager);
	}

	/// <summary>Writes the XML Schema to the supplied <see cref="T:System.IO.TextWriter" />.</summary>
	/// <param name="writer">The <see cref="T:System.IO.TextWriter" /> to write to.</param>
	public void Write(TextWriter writer)
	{
		Write(writer, null);
	}

	/// <summary>Writes the XML Schema to the supplied <see cref="T:System.IO.TextWriter" />.</summary>
	/// <param name="writer">The <see cref="T:System.IO.TextWriter" /> to write to.</param>
	/// <param name="namespaceManager">The <see cref="T:System.Xml.XmlNamespaceManager" />. </param>
	public void Write(TextWriter writer, XmlNamespaceManager namespaceManager)
	{
		XmlTextWriter xmlTextWriter = new XmlTextWriter(writer);
		xmlTextWriter.Formatting = Formatting.Indented;
		Write(xmlTextWriter, namespaceManager);
	}

	/// <summary>Writes the XML Schema to the supplied <see cref="T:System.Xml.XmlWriter" />.</summary>
	/// <param name="writer">The <see cref="T:System.Xml.XmlWriter" /> to write to. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="writer" /> parameter is null.</exception>
	public void Write(XmlWriter writer)
	{
		Write(writer, null);
	}

	/// <summary>Writes the XML Schema to the supplied <see cref="T:System.Xml.XmlWriter" />.</summary>
	/// <param name="writer">The <see cref="T:System.Xml.XmlWriter" /> to write to.</param>
	/// <param name="namespaceManager">The <see cref="T:System.Xml.XmlNamespaceManager" />. </param>
	public void Write(XmlWriter writer, XmlNamespaceManager namespaceManager)
	{
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(XmlSchema));
		XmlSerializerNamespaces xmlSerializerNamespaces;
		if (namespaceManager != null)
		{
			xmlSerializerNamespaces = new XmlSerializerNamespaces();
			bool flag = false;
			if (base.Namespaces != null)
			{
				flag = base.Namespaces.Namespaces["xs"] != null || base.Namespaces.Namespaces.ContainsValue("http://www.w3.org/2001/XMLSchema");
			}
			if (!flag && namespaceManager.LookupPrefix("http://www.w3.org/2001/XMLSchema") == null && namespaceManager.LookupNamespace("xs") == null)
			{
				xmlSerializerNamespaces.Add("xs", "http://www.w3.org/2001/XMLSchema");
			}
			foreach (string item in namespaceManager)
			{
				if (item != "xml" && item != "xmlns")
				{
					xmlSerializerNamespaces.Add(item, namespaceManager.LookupNamespace(item));
				}
			}
		}
		else if (base.Namespaces != null && base.Namespaces.Count > 0)
		{
			Hashtable hashtable = base.Namespaces.Namespaces;
			if (hashtable["xs"] == null && !hashtable.ContainsValue("http://www.w3.org/2001/XMLSchema"))
			{
				hashtable.Add("xs", "http://www.w3.org/2001/XMLSchema");
			}
			xmlSerializerNamespaces = base.Namespaces;
		}
		else
		{
			xmlSerializerNamespaces = new XmlSerializerNamespaces();
			xmlSerializerNamespaces.Add("xs", "http://www.w3.org/2001/XMLSchema");
			if (targetNs != null && targetNs.Length != 0)
			{
				xmlSerializerNamespaces.Add("tns", targetNs);
			}
		}
		xmlSerializer.Serialize(writer, this, xmlSerializerNamespaces);
	}

	/// <summary>Compiles the XML Schema Object Model (SOM) into schema information for validation. Used to check the syntactic and semantic structure of the programmatically built SOM. Semantic validation checking is performed during compilation.</summary>
	/// <param name="validationEventHandler">The validation event handler that receives information about XML Schema validation errors. </param>
	[Obsolete("Use System.Xml.Schema.XmlSchemaSet for schema compilation and validation. http://go.microsoft.com/fwlink/?linkid=14202")]
	public void Compile(ValidationEventHandler validationEventHandler)
	{
		SchemaInfo schemaInfo = new SchemaInfo();
		schemaInfo.SchemaType = SchemaType.XSD;
		CompileSchema(null, XmlReaderSection.CreateDefaultResolver(), schemaInfo, null, validationEventHandler, NameTable, CompileContentModel: false);
	}

	/// <summary>Compiles the XML Schema Object Model (SOM) into schema information for validation. Used to check the syntactic and semantic structure of the programmatically built SOM. Semantic validation checking is performed during compilation.</summary>
	/// <param name="validationEventHandler">The validation event handler that receives information about the XML Schema validation errors. </param>
	/// <param name="resolver">The XmlResolver used to resolve namespaces referenced in include and import elements. </param>
	[Obsolete("Use System.Xml.Schema.XmlSchemaSet for schema compilation and validation. http://go.microsoft.com/fwlink/?linkid=14202")]
	public void Compile(ValidationEventHandler validationEventHandler, XmlResolver resolver)
	{
		SchemaInfo schemaInfo = new SchemaInfo();
		schemaInfo.SchemaType = SchemaType.XSD;
		CompileSchema(null, resolver, schemaInfo, null, validationEventHandler, NameTable, CompileContentModel: false);
	}

	internal bool CompileSchema(XmlSchemaCollection xsc, XmlResolver resolver, SchemaInfo schemaInfo, string ns, ValidationEventHandler validationEventHandler, XmlNameTable nameTable, bool CompileContentModel)
	{
		lock (this)
		{
			if (!new SchemaCollectionPreprocessor(nameTable, null, validationEventHandler)
			{
				XmlResolver = resolver
			}.Execute(this, ns, loadExternals: true, xsc))
			{
				return false;
			}
			SchemaCollectionCompiler schemaCollectionCompiler = new SchemaCollectionCompiler(nameTable, validationEventHandler);
			isCompiled = schemaCollectionCompiler.Execute(this, schemaInfo, CompileContentModel);
			SetIsCompiled(isCompiled);
			return isCompiled;
		}
	}

	internal void CompileSchemaInSet(XmlNameTable nameTable, ValidationEventHandler eventHandler, XmlSchemaCompilationSettings compilationSettings)
	{
		Compiler compiler = new Compiler(nameTable, eventHandler, null, compilationSettings);
		compiler.Prepare(this, cleanup: true);
		isCompiledBySet = compiler.Compile();
	}

	internal new XmlSchema Clone()
	{
		XmlSchema obj = new XmlSchema
		{
			attributeFormDefault = attributeFormDefault,
			elementFormDefault = elementFormDefault,
			blockDefault = blockDefault,
			finalDefault = finalDefault,
			targetNs = targetNs,
			version = version,
			includes = includes,
			Namespaces = base.Namespaces,
			items = items,
			BaseUri = BaseUri
		};
		SchemaCollectionCompiler.Cleanup(obj);
		return obj;
	}

	internal XmlSchema DeepClone()
	{
		XmlSchema xmlSchema = new XmlSchema();
		xmlSchema.attributeFormDefault = attributeFormDefault;
		xmlSchema.elementFormDefault = elementFormDefault;
		xmlSchema.blockDefault = blockDefault;
		xmlSchema.finalDefault = finalDefault;
		xmlSchema.targetNs = targetNs;
		xmlSchema.version = version;
		xmlSchema.isPreprocessed = isPreprocessed;
		for (int i = 0; i < items.Count; i++)
		{
			XmlSchemaObject item = ((items[i] is XmlSchemaComplexType xmlSchemaComplexType) ? xmlSchemaComplexType.Clone(this) : ((items[i] is XmlSchemaElement xmlSchemaElement) ? xmlSchemaElement.Clone(this) : ((!(items[i] is XmlSchemaGroup xmlSchemaGroup)) ? items[i].Clone() : xmlSchemaGroup.Clone(this))));
			xmlSchema.Items.Add(item);
		}
		for (int j = 0; j < includes.Count; j++)
		{
			XmlSchemaExternal item2 = (XmlSchemaExternal)includes[j].Clone();
			xmlSchema.Includes.Add(item2);
		}
		xmlSchema.Namespaces = base.Namespaces;
		xmlSchema.BaseUri = BaseUri;
		return xmlSchema;
	}

	internal void SetIsCompiled(bool isCompiled)
	{
		this.isCompiled = isCompiled;
	}

	internal override void SetUnhandledAttributes(XmlAttribute[] moreAttributes)
	{
		this.moreAttributes = moreAttributes;
	}

	internal override void AddAnnotation(XmlSchemaAnnotation annotation)
	{
		items.Add(annotation);
	}

	internal void GetExternalSchemasList(IList extList, XmlSchema schema)
	{
		if (extList.Contains(schema))
		{
			return;
		}
		extList.Add(schema);
		for (int i = 0; i < schema.Includes.Count; i++)
		{
			XmlSchemaExternal xmlSchemaExternal = (XmlSchemaExternal)schema.Includes[i];
			if (xmlSchemaExternal.Schema != null)
			{
				GetExternalSchemasList(extList, xmlSchemaExternal.Schema);
			}
		}
	}
}
