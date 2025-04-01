using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml.Schema;

namespace System.Xml.Serialization;

/// <summary>Represents the collection of XML schemas.</summary>
public class XmlSchemas : CollectionBase, IEnumerable<XmlSchema>, IEnumerable
{
	private XmlSchemaSet schemaSet;

	private Hashtable references;

	private SchemaObjectCache cache;

	private bool shareTypes;

	private Hashtable mergedSchemas;

	internal Hashtable delayedSchemas = new Hashtable();

	private bool isCompiled;

	private static volatile XmlSchema xsd;

	private static volatile XmlSchema xml;

	internal const string xmlSchema = "<?xml version='1.0' encoding='UTF-8' ?> \n<xs:schema targetNamespace='http://www.w3.org/XML/1998/namespace' xmlns:xs='http://www.w3.org/2001/XMLSchema' xml:lang='en'>\n <xs:attribute name='lang' type='xs:language'/>\n <xs:attribute name='space'>\n  <xs:simpleType>\n   <xs:restriction base='xs:NCName'>\n    <xs:enumeration value='default'/>\n    <xs:enumeration value='preserve'/>\n   </xs:restriction>\n  </xs:simpleType>\n </xs:attribute>\n <xs:attribute name='base' type='xs:anyURI'/>\n <xs:attribute name='id' type='xs:ID' />\n <xs:attributeGroup name='specialAttrs'>\n  <xs:attribute ref='xml:base'/>\n  <xs:attribute ref='xml:lang'/>\n  <xs:attribute ref='xml:space'/>\n </xs:attributeGroup>\n</xs:schema>";

	/// <summary>Gets or sets the <see cref="T:System.Xml.Schema.XmlSchema" /> object at the specified index. </summary>
	/// <returns>The specified <see cref="T:System.Xml.Schema.XmlSchema" />.</returns>
	/// <param name="index">The index of the item to retrieve.</param>
	public XmlSchema this[int index]
	{
		get
		{
			return (XmlSchema)base.List[index];
		}
		set
		{
			base.List[index] = value;
		}
	}

	/// <summary>Gets a specified <see cref="T:System.Xml.Schema.XmlSchema" /> object that represents the XML schema associated with the specified namespace.</summary>
	/// <returns>The specified <see cref="T:System.Xml.Schema.XmlSchema" /> object.</returns>
	/// <param name="ns">The namespace of the specified object.</param>
	public XmlSchema this[string ns]
	{
		get
		{
			IList list = (IList)SchemaSet.Schemas(ns);
			if (list.Count == 0)
			{
				return null;
			}
			if (list.Count == 1)
			{
				return (XmlSchema)list[0];
			}
			throw new InvalidOperationException(Res.GetString("There are more then one schema with targetNamespace='{0}'.", ns));
		}
	}

	internal SchemaObjectCache Cache
	{
		get
		{
			if (cache == null)
			{
				cache = new SchemaObjectCache();
			}
			return cache;
		}
	}

	internal Hashtable MergedSchemas
	{
		get
		{
			if (mergedSchemas == null)
			{
				mergedSchemas = new Hashtable();
			}
			return mergedSchemas;
		}
	}

	internal Hashtable References
	{
		get
		{
			if (references == null)
			{
				references = new Hashtable();
			}
			return references;
		}
	}

	internal XmlSchemaSet SchemaSet
	{
		get
		{
			if (schemaSet == null)
			{
				schemaSet = new XmlSchemaSet();
				schemaSet.XmlResolver = null;
				schemaSet.ValidationEventHandler += IgnoreCompileErrors;
			}
			return schemaSet;
		}
	}

	/// <summary>Gets a value that indicates whether the schemas have been compiled.</summary>
	/// <returns>true, if the schemas have been compiled; otherwise, false.</returns>
	public bool IsCompiled => isCompiled;

	internal static XmlSchema XsdSchema
	{
		get
		{
			if (xsd == null)
			{
				xsd = CreateFakeXsdSchema("http://www.w3.org/2001/XMLSchema", "schema");
			}
			return xsd;
		}
	}

	internal static XmlSchema XmlSchema
	{
		get
		{
			if (xml == null)
			{
				xml = XmlSchema.Read(new StringReader("<?xml version='1.0' encoding='UTF-8' ?> \n<xs:schema targetNamespace='http://www.w3.org/XML/1998/namespace' xmlns:xs='http://www.w3.org/2001/XMLSchema' xml:lang='en'>\n <xs:attribute name='lang' type='xs:language'/>\n <xs:attribute name='space'>\n  <xs:simpleType>\n   <xs:restriction base='xs:NCName'>\n    <xs:enumeration value='default'/>\n    <xs:enumeration value='preserve'/>\n   </xs:restriction>\n  </xs:simpleType>\n </xs:attribute>\n <xs:attribute name='base' type='xs:anyURI'/>\n <xs:attribute name='id' type='xs:ID' />\n <xs:attributeGroup name='specialAttrs'>\n  <xs:attribute ref='xml:base'/>\n  <xs:attribute ref='xml:lang'/>\n  <xs:attribute ref='xml:space'/>\n </xs:attributeGroup>\n</xs:schema>"), null);
			}
			return xml;
		}
	}

	/// <summary>Gets a collection of schemas that belong to the same namespace.</summary>
	/// <returns>An <see cref="T:System.Collections.IList" /> implementation that contains the schemas.</returns>
	/// <param name="ns">The namespace of the schemas to retrieve.</param>
	public IList GetSchemas(string ns)
	{
		return (IList)SchemaSet.Schemas(ns);
	}

	internal int Add(XmlSchema schema, bool delay)
	{
		if (delay)
		{
			if (delayedSchemas[schema] == null)
			{
				delayedSchemas.Add(schema, schema);
			}
			return -1;
		}
		return Add(schema);
	}

	/// <summary>Adds an object to the end of the collection.</summary>
	/// <returns>The index at which the <see cref="T:System.Xml.Schema.XmlSchema" /> is added.</returns>
	/// <param name="schema">The <see cref="T:System.Xml.Schema.XmlSchema" /> object to be added to the collection of objects. </param>
	public int Add(XmlSchema schema)
	{
		if (base.List.Contains(schema))
		{
			return base.List.IndexOf(schema);
		}
		return base.List.Add(schema);
	}

	/// <summary>Adds an <see cref="T:System.Xml.Schema.XmlSchema" /> object that represents an assembly reference to the collection.</summary>
	/// <returns>The index at which the <see cref="T:System.Xml.Schema.XmlSchema" /> is added.</returns>
	/// <param name="schema">The <see cref="T:System.Xml.Schema.XmlSchema" /> to add.</param>
	/// <param name="baseUri">The <see cref="T:System.Uri" /> of the schema object.</param>
	public int Add(XmlSchema schema, Uri baseUri)
	{
		if (base.List.Contains(schema))
		{
			return base.List.IndexOf(schema);
		}
		if (baseUri != null)
		{
			schema.BaseUri = baseUri;
		}
		return base.List.Add(schema);
	}

	/// <summary>Adds an instance of the <see cref="T:System.Xml.Serialization.XmlSchemas" /> class to the end of the collection.</summary>
	/// <param name="schemas">The <see cref="T:System.Xml.Serialization.XmlSchemas" /> object to be added to the end of the collection. </param>
	public void Add(XmlSchemas schemas)
	{
		foreach (XmlSchema schema in schemas)
		{
			Add(schema);
		}
	}

	/// <summary>Adds an <see cref="T:System.Xml.Schema.XmlSchema" /> object that represents an assembly reference to the collection.</summary>
	/// <param name="schema">The <see cref="T:System.Xml.Schema.XmlSchema" /> to add.</param>
	public void AddReference(XmlSchema schema)
	{
		References[schema] = schema;
	}

	/// <summary>Inserts a schema into the <see cref="T:System.Xml.Serialization.XmlSchemas" /> at the specified index.</summary>
	/// <param name="index">The zero-based index at which <paramref name="schema" /> should be inserted. </param>
	/// <param name="schema">The <see cref="T:System.Xml.Schema.XmlSchema" /> object to be inserted. </param>
	public void Insert(int index, XmlSchema schema)
	{
		base.List.Insert(index, schema);
	}

	/// <summary>Searches for the specified schema and returns the zero-based index of the first occurrence within the entire <see cref="T:System.Xml.Serialization.XmlSchemas" />.</summary>
	/// <returns>The zero-based index of the first occurrence of the value within the entire <see cref="T:System.Xml.Serialization.XmlSchemas" />, if found; otherwise, -1.</returns>
	/// <param name="schema">The <see cref="T:System.Xml.Schema.XmlSchema" /> to locate. </param>
	public int IndexOf(XmlSchema schema)
	{
		return base.List.IndexOf(schema);
	}

	/// <summary>Determines whether the <see cref="T:System.Xml.Serialization.XmlSchemas" /> contains a specific schema.</summary>
	/// <returns>true, if the collection contains the specified item; otherwise, false.</returns>
	/// <param name="schema">The <see cref="T:System.Xml.Schema.XmlSchema" /> object to locate. </param>
	public bool Contains(XmlSchema schema)
	{
		return base.List.Contains(schema);
	}

	/// <summary>Returns a value that indicates whether the collection contains an <see cref="T:System.Xml.Schema.XmlSchema" /> object that belongs to the specified namespace.</summary>
	/// <returns>true if the item is found; otherwise, false.</returns>
	/// <param name="targetNamespace">The namespace of the item to check for.</param>
	public bool Contains(string targetNamespace)
	{
		return SchemaSet.Contains(targetNamespace);
	}

	/// <summary>Removes the first occurrence of a specific schema from the <see cref="T:System.Xml.Serialization.XmlSchemas" />.</summary>
	/// <param name="schema">The <see cref="T:System.Xml.Schema.XmlSchema" /> to remove. </param>
	public void Remove(XmlSchema schema)
	{
		base.List.Remove(schema);
	}

	/// <summary>Copies the entire <see cref="T:System.Xml.Serialization.XmlSchemas" /> to a compatible one-dimensional <see cref="T:System.Array" />, which starts at the specified index of the target array.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the schemas copied from <see cref="T:System.Xml.Serialization.XmlSchemas" />. The <see cref="T:System.Array" /> must have zero-based indexing. </param>
	/// <param name="index">A 32-bit integer that represents the index in the array where copying begins.</param>
	public void CopyTo(XmlSchema[] array, int index)
	{
		base.List.CopyTo(array, index);
	}

	/// <summary>Performs additional custom processes before inserting a new element into the <see cref="T:System.Xml.Serialization.XmlSchemas" /> instance.</summary>
	/// <param name="index">The zero-based index at which to insert <paramref name="value" />. </param>
	/// <param name="value">The new value of the element at <paramref name="index" />. </param>
	protected override void OnInsert(int index, object value)
	{
		AddName((XmlSchema)value);
	}

	/// <summary>Performs additional custom processes when removing an element from the <see cref="T:System.Xml.Serialization.XmlSchemas" /> instance.</summary>
	/// <param name="index">The zero-based index at which <paramref name="value" /> can be found. </param>
	/// <param name="value">The value of the element to remove at <paramref name="index" />. </param>
	protected override void OnRemove(int index, object value)
	{
		RemoveName((XmlSchema)value);
	}

	/// <summary>Performs additional custom processes when clearing the contents of the <see cref="T:System.Xml.Serialization.XmlSchemas" /> instance.</summary>
	protected override void OnClear()
	{
		schemaSet = null;
	}

	/// <summary>Performs additional custom processes before setting a value in the <see cref="T:System.Xml.Serialization.XmlSchemas" /> instance.</summary>
	/// <param name="index">The zero-based index at which <paramref name="oldValue" /> can be found. </param>
	/// <param name="oldValue">The value to replace with <paramref name="newValue" />. </param>
	/// <param name="newValue">The new value of the element at <paramref name="index" />. </param>
	protected override void OnSet(int index, object oldValue, object newValue)
	{
		RemoveName((XmlSchema)oldValue);
		AddName((XmlSchema)newValue);
	}

	private void AddName(XmlSchema schema)
	{
		if (isCompiled)
		{
			throw new InvalidOperationException(Res.GetString("Cannot add schema to compiled schemas collection."));
		}
		if (SchemaSet.Contains(schema))
		{
			SchemaSet.Reprocess(schema);
			return;
		}
		Prepare(schema);
		SchemaSet.Add(schema);
	}

	private void Prepare(XmlSchema schema)
	{
		ArrayList arrayList = new ArrayList();
		string targetNamespace = schema.TargetNamespace;
		foreach (XmlSchemaExternal include in schema.Includes)
		{
			if (include is XmlSchemaImport && targetNamespace == ((XmlSchemaImport)include).Namespace)
			{
				arrayList.Add(include);
			}
		}
		foreach (XmlSchemaObject item in arrayList)
		{
			schema.Includes.Remove(item);
		}
	}

	private void RemoveName(XmlSchema schema)
	{
		SchemaSet.Remove(schema);
	}

	/// <summary>Locates in one of the XML schemas an <see cref="T:System.Xml.Schema.XmlSchemaObject" /> of the specified name and type. </summary>
	/// <returns>An <see cref="T:System.Xml.Schema.XmlSchemaObject" /> instance, such as an <see cref="T:System.Xml.Schema.XmlSchemaElement" /> or <see cref="T:System.Xml.Schema.XmlSchemaAttribute" />.</returns>
	/// <param name="name">An <see cref="T:System.Xml.XmlQualifiedName" /> that specifies a fully qualified name with a namespace used to locate an <see cref="T:System.Xml.Schema.XmlSchema" /> object in the collection.</param>
	/// <param name="type">The <see cref="T:System.Type" /> of the object to find. Possible types include: <see cref="T:System.Xml.Schema.XmlSchemaGroup" />, <see cref="T:System.Xml.Schema.XmlSchemaAttributeGroup" />, <see cref="T:System.Xml.Schema.XmlSchemaElement" />, <see cref="T:System.Xml.Schema.XmlSchemaAttribute" />, and <see cref="T:System.Xml.Schema.XmlSchemaNotation" />.</param>
	public object Find(XmlQualifiedName name, Type type)
	{
		return Find(name, type, checkCache: true);
	}

	internal object Find(XmlQualifiedName name, Type type, bool checkCache)
	{
		if (!IsCompiled)
		{
			foreach (XmlSchema item in base.List)
			{
				Preprocess(item);
			}
		}
		IList list = (IList)SchemaSet.Schemas(name.Namespace);
		if (list == null)
		{
			return null;
		}
		foreach (XmlSchema item2 in list)
		{
			Preprocess(item2);
			XmlSchemaObject xmlSchemaObject = null;
			if (typeof(XmlSchemaType).IsAssignableFrom(type))
			{
				xmlSchemaObject = item2.SchemaTypes[name];
				if (xmlSchemaObject == null || !type.IsAssignableFrom(xmlSchemaObject.GetType()))
				{
					continue;
				}
			}
			else if (type == typeof(XmlSchemaGroup))
			{
				xmlSchemaObject = item2.Groups[name];
			}
			else if (type == typeof(XmlSchemaAttributeGroup))
			{
				xmlSchemaObject = item2.AttributeGroups[name];
			}
			else if (type == typeof(XmlSchemaElement))
			{
				xmlSchemaObject = item2.Elements[name];
			}
			else if (type == typeof(XmlSchemaAttribute))
			{
				xmlSchemaObject = item2.Attributes[name];
			}
			else if (type == typeof(XmlSchemaNotation))
			{
				xmlSchemaObject = item2.Notations[name];
			}
			if (xmlSchemaObject != null && shareTypes && checkCache && !IsReference(xmlSchemaObject))
			{
				xmlSchemaObject = Cache.AddItem(xmlSchemaObject, name, this);
			}
			if (xmlSchemaObject != null)
			{
				return xmlSchemaObject;
			}
		}
		return null;
	}

	IEnumerator<XmlSchema> IEnumerable<XmlSchema>.GetEnumerator()
	{
		return new XmlSchemaEnumerator(this);
	}

	internal static void Preprocess(XmlSchema schema)
	{
		if (!schema.IsPreprocessed)
		{
			try
			{
				System.Xml.NameTable nameTable = new System.Xml.NameTable();
				Preprocessor preprocessor = new Preprocessor(nameTable, new SchemaNames(nameTable), null);
				preprocessor.SchemaLocations = new Hashtable();
				preprocessor.Execute(schema, schema.TargetNamespace, loadExternals: false);
			}
			catch (XmlSchemaException ex)
			{
				throw CreateValidationException(ex, ex.Message);
			}
		}
	}

	/// <summary>Static method that determines whether the specified XML schema contains a custom IsDataSet attribute set to true, or its equivalent. </summary>
	/// <returns>true if the specified schema exists; otherwise, false.</returns>
	/// <param name="schema">The XML schema to check for an IsDataSet attribute with a true value.</param>
	public static bool IsDataSet(XmlSchema schema)
	{
		foreach (XmlSchemaObject item in schema.Items)
		{
			if (!(item is XmlSchemaElement))
			{
				continue;
			}
			XmlSchemaElement xmlSchemaElement = (XmlSchemaElement)item;
			if (xmlSchemaElement.UnhandledAttributes == null)
			{
				continue;
			}
			XmlAttribute[] unhandledAttributes = xmlSchemaElement.UnhandledAttributes;
			foreach (XmlAttribute xmlAttribute in unhandledAttributes)
			{
				if (xmlAttribute.LocalName == "IsDataSet" && xmlAttribute.NamespaceURI == "urn:schemas-microsoft-com:xml-msdata" && (xmlAttribute.Value == "True" || xmlAttribute.Value == "true" || xmlAttribute.Value == "1"))
				{
					return true;
				}
			}
		}
		return false;
	}

	private void Merge(XmlSchema schema)
	{
		if (MergedSchemas[schema] == null)
		{
			IList list = (IList)SchemaSet.Schemas(schema.TargetNamespace);
			if (list != null && list.Count > 0)
			{
				MergedSchemas.Add(schema, schema);
				Merge(list, schema);
			}
			else
			{
				Add(schema);
				MergedSchemas.Add(schema, schema);
			}
		}
	}

	private void AddImport(IList schemas, string ns)
	{
		foreach (XmlSchema schema in schemas)
		{
			bool flag = true;
			foreach (XmlSchemaExternal include in schema.Includes)
			{
				if (include is XmlSchemaImport && ((XmlSchemaImport)include).Namespace == ns)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				XmlSchemaImport xmlSchemaImport = new XmlSchemaImport();
				xmlSchemaImport.Namespace = ns;
				schema.Includes.Add(xmlSchemaImport);
			}
		}
	}

	private void Merge(IList originals, XmlSchema schema)
	{
		foreach (XmlSchema original in originals)
		{
			if (schema == original)
			{
				return;
			}
		}
		foreach (XmlSchemaExternal include in schema.Includes)
		{
			if (include is XmlSchemaImport)
			{
				include.SchemaLocation = null;
				if (include.Schema != null)
				{
					Merge(include.Schema);
				}
				else
				{
					AddImport(originals, ((XmlSchemaImport)include).Namespace);
				}
			}
			else if (include.Schema == null)
			{
				if (include.SchemaLocation != null)
				{
					throw new InvalidOperationException(Res.GetString("Schema attribute schemaLocation='{1}' is not supported on objects of type {0}.  Please set {0}.Schema property.", GetType().Name, include.SchemaLocation));
				}
			}
			else
			{
				include.SchemaLocation = null;
				Merge(originals, include.Schema);
			}
		}
		bool[] array = new bool[schema.Items.Count];
		int num = 0;
		for (int i = 0; i < schema.Items.Count; i++)
		{
			XmlSchemaObject xmlSchemaObject = schema.Items[i];
			XmlSchemaObject xmlSchemaObject2 = Find(xmlSchemaObject, originals);
			if (xmlSchemaObject2 != null)
			{
				if (!Cache.Match(xmlSchemaObject2, xmlSchemaObject, shareTypes))
				{
					throw new InvalidOperationException(MergeFailedMessage(xmlSchemaObject, xmlSchemaObject2, schema.TargetNamespace));
				}
				array[i] = true;
				num++;
			}
		}
		if (num == schema.Items.Count)
		{
			return;
		}
		XmlSchema xmlSchema2 = (XmlSchema)originals[0];
		for (int j = 0; j < schema.Items.Count; j++)
		{
			if (!array[j])
			{
				xmlSchema2.Items.Add(schema.Items[j]);
			}
		}
		xmlSchema2.IsPreprocessed = false;
		Preprocess(xmlSchema2);
	}

	private static string ItemName(XmlSchemaObject o)
	{
		if (o is XmlSchemaNotation)
		{
			return ((XmlSchemaNotation)o).Name;
		}
		if (o is XmlSchemaGroup)
		{
			return ((XmlSchemaGroup)o).Name;
		}
		if (o is XmlSchemaElement)
		{
			return ((XmlSchemaElement)o).Name;
		}
		if (o is XmlSchemaType)
		{
			return ((XmlSchemaType)o).Name;
		}
		if (o is XmlSchemaAttributeGroup)
		{
			return ((XmlSchemaAttributeGroup)o).Name;
		}
		if (o is XmlSchemaAttribute)
		{
			return ((XmlSchemaAttribute)o).Name;
		}
		return null;
	}

	internal static XmlQualifiedName GetParentName(XmlSchemaObject item)
	{
		while (item.Parent != null)
		{
			if (item.Parent is XmlSchemaType)
			{
				XmlSchemaType xmlSchemaType = (XmlSchemaType)item.Parent;
				if (xmlSchemaType.Name != null && xmlSchemaType.Name.Length != 0)
				{
					return xmlSchemaType.QualifiedName;
				}
			}
			item = item.Parent;
		}
		return XmlQualifiedName.Empty;
	}

	private static string GetSchemaItem(XmlSchemaObject o, string ns, string details)
	{
		if (o == null)
		{
			return null;
		}
		while (o.Parent != null && !(o.Parent is XmlSchema))
		{
			o = o.Parent;
		}
		if (ns == null || ns.Length == 0)
		{
			XmlSchemaObject xmlSchemaObject = o;
			while (xmlSchemaObject.Parent != null)
			{
				xmlSchemaObject = xmlSchemaObject.Parent;
			}
			if (xmlSchemaObject is XmlSchema)
			{
				ns = ((XmlSchema)xmlSchemaObject).TargetNamespace;
			}
		}
		string text = null;
		if (o is XmlSchemaNotation)
		{
			return Res.GetString("Schema item '{1}' named '{2}' from namespace '{0}'. {3}", ns, "notation", ((XmlSchemaNotation)o).Name, details);
		}
		if (o is XmlSchemaGroup)
		{
			return Res.GetString("Schema item '{1}' named '{2}' from namespace '{0}'. {3}", ns, "group", ((XmlSchemaGroup)o).Name, details);
		}
		if (o is XmlSchemaElement)
		{
			XmlSchemaElement xmlSchemaElement = (XmlSchemaElement)o;
			if (xmlSchemaElement.Name == null || xmlSchemaElement.Name.Length == 0)
			{
				XmlQualifiedName parentName = GetParentName(o);
				return Res.GetString("Element reference '{0}' declared in schema type '{1}' from namespace '{2}'.", xmlSchemaElement.RefName.ToString(), parentName.Name, parentName.Namespace);
			}
			return Res.GetString("Schema item '{1}' named '{2}' from namespace '{0}'. {3}", ns, "element", xmlSchemaElement.Name, details);
		}
		if (o is XmlSchemaType)
		{
			return Res.GetString("Schema item '{1}' named '{2}' from namespace '{0}'. {3}", ns, (o.GetType() == typeof(XmlSchemaSimpleType)) ? "simpleType" : "complexType", ((XmlSchemaType)o).Name, null);
		}
		if (o is XmlSchemaAttributeGroup)
		{
			return Res.GetString("Schema item '{1}' named '{2}' from namespace '{0}'. {3}", ns, "attributeGroup", ((XmlSchemaAttributeGroup)o).Name, details);
		}
		if (o is XmlSchemaAttribute)
		{
			XmlSchemaAttribute xmlSchemaAttribute = (XmlSchemaAttribute)o;
			if (xmlSchemaAttribute.Name == null || xmlSchemaAttribute.Name.Length == 0)
			{
				XmlQualifiedName parentName2 = GetParentName(o);
				return Res.GetString("Attribute reference '{0}' declared in schema type '{1}' from namespace '{2}'.", xmlSchemaAttribute.RefName.ToString(), parentName2.Name, parentName2.Namespace);
			}
			return Res.GetString("Schema item '{1}' named '{2}' from namespace '{0}'. {3}", ns, "attribute", xmlSchemaAttribute.Name, details);
		}
		if (o is XmlSchemaContent)
		{
			XmlQualifiedName parentName3 = GetParentName(o);
			return Res.GetString("Check content definition of schema type '{0}' from namespace '{1}'. {2}", parentName3.Name, parentName3.Namespace, null);
		}
		if (o is XmlSchemaExternal)
		{
			string text2 = ((o is XmlSchemaImport) ? "import" : ((o is XmlSchemaInclude) ? "include" : ((o is XmlSchemaRedefine) ? "redefine" : o.GetType().Name)));
			return Res.GetString("Schema item '{1}' from namespace '{0}'. {2}", ns, text2, details);
		}
		if (o is XmlSchema)
		{
			return Res.GetString("Schema with targetNamespace='{0}' has invalid syntax. {1}", ns, details);
		}
		return Res.GetString("Schema item '{1}' named '{2}' from namespace '{0}'. {3}", ns, o.GetType().Name, null, details);
	}

	private static string Dump(XmlSchemaObject o)
	{
		XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
		xmlWriterSettings.OmitXmlDeclaration = true;
		xmlWriterSettings.Indent = true;
		XmlSerializer xmlSerializer = new XmlSerializer(o.GetType());
		StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
		XmlWriter xmlWriter = XmlWriter.Create(stringWriter, xmlWriterSettings);
		XmlSerializerNamespaces xmlSerializerNamespaces = new XmlSerializerNamespaces();
		xmlSerializerNamespaces.Add("xs", "http://www.w3.org/2001/XMLSchema");
		xmlSerializer.Serialize(xmlWriter, o, xmlSerializerNamespaces);
		return stringWriter.ToString();
	}

	private static string MergeFailedMessage(XmlSchemaObject src, XmlSchemaObject dest, string ns)
	{
		return string.Concat(Res.GetString("Cannot merge schemas with targetNamespace='{0}'. Several mismatched declarations were found: {1}", ns, GetSchemaItem(src, ns, null)) + "\r\n" + Dump(src), "\r\n", Dump(dest));
	}

	internal XmlSchemaObject Find(XmlSchemaObject o, IList originals)
	{
		string text = ItemName(o);
		if (text == null)
		{
			return null;
		}
		Type type = o.GetType();
		foreach (XmlSchema original in originals)
		{
			foreach (XmlSchemaObject item in original.Items)
			{
				if (item.GetType() == type && text == ItemName(item))
				{
					return item;
				}
			}
		}
		return null;
	}

	/// <summary>Processes the element and attribute names in the XML schemas and, optionally, validates the XML schemas. </summary>
	/// <param name="handler">A <see cref="T:System.Xml.Schema.ValidationEventHandler" /> that specifies the callback method that handles errors and warnings during XML Schema validation, if the strict parameter is set to true.</param>
	/// <param name="fullCompile">true to validate the XML schemas in the collection using the <see cref="M:System.Xml.Serialization.XmlSchemas.Compile(System.Xml.Schema.ValidationEventHandler,System.Boolean)" /> method of the <see cref="T:System.Xml.Serialization.XmlSchemas" /> class; otherwise, false.</param>
	public void Compile(ValidationEventHandler handler, bool fullCompile)
	{
		if (isCompiled)
		{
			return;
		}
		foreach (XmlSchema value in delayedSchemas.Values)
		{
			Merge(value);
		}
		delayedSchemas.Clear();
		if (fullCompile)
		{
			schemaSet = new XmlSchemaSet();
			schemaSet.XmlResolver = null;
			schemaSet.ValidationEventHandler += handler;
			foreach (XmlSchema value2 in References.Values)
			{
				schemaSet.Add(value2);
			}
			int num = schemaSet.Count;
			foreach (XmlSchema item in base.List)
			{
				if (!SchemaSet.Contains(item))
				{
					schemaSet.Add(item);
					num++;
				}
			}
			if (!SchemaSet.Contains("http://www.w3.org/2001/XMLSchema"))
			{
				AddReference(XsdSchema);
				schemaSet.Add(XsdSchema);
				num++;
			}
			if (!SchemaSet.Contains("http://www.w3.org/XML/1998/namespace"))
			{
				AddReference(XmlSchema);
				schemaSet.Add(XmlSchema);
				num++;
			}
			schemaSet.Compile();
			schemaSet.ValidationEventHandler -= handler;
			isCompiled = schemaSet.IsCompiled && num == schemaSet.Count;
			return;
		}
		try
		{
			System.Xml.NameTable nameTable = new System.Xml.NameTable();
			Preprocessor preprocessor = new Preprocessor(nameTable, new SchemaNames(nameTable), null);
			preprocessor.XmlResolver = null;
			preprocessor.SchemaLocations = new Hashtable();
			preprocessor.ChameleonSchemas = new Hashtable();
			foreach (XmlSchema item2 in SchemaSet.Schemas())
			{
				preprocessor.Execute(item2, item2.TargetNamespace, loadExternals: true);
			}
		}
		catch (XmlSchemaException ex)
		{
			throw CreateValidationException(ex, ex.Message);
		}
	}

	internal static Exception CreateValidationException(XmlSchemaException exception, string message)
	{
		XmlSchemaObject xmlSchemaObject = exception.SourceSchemaObject;
		if (exception.LineNumber == 0 && exception.LinePosition == 0)
		{
			throw new InvalidOperationException(GetSchemaItem(xmlSchemaObject, null, message), exception);
		}
		string text = null;
		if (xmlSchemaObject != null)
		{
			while (xmlSchemaObject.Parent != null)
			{
				xmlSchemaObject = xmlSchemaObject.Parent;
			}
			if (xmlSchemaObject is XmlSchema)
			{
				text = ((XmlSchema)xmlSchemaObject).TargetNamespace;
			}
		}
		throw new InvalidOperationException(Res.GetString("Schema with targetNamespace='{0}' has invalid syntax. {1} Line {2}, position {3}.", text, message, exception.LineNumber, exception.LinePosition), exception);
	}

	internal static void IgnoreCompileErrors(object sender, ValidationEventArgs args)
	{
	}

	private static XmlSchema CreateFakeXsdSchema(string ns, string name)
	{
		XmlSchema obj = new XmlSchema
		{
			TargetNamespace = ns
		};
		XmlSchemaElement xmlSchemaElement = new XmlSchemaElement
		{
			Name = name
		};
		XmlSchemaComplexType schemaType = new XmlSchemaComplexType();
		xmlSchemaElement.SchemaType = schemaType;
		obj.Items.Add(xmlSchemaElement);
		return obj;
	}

	internal void SetCache(SchemaObjectCache cache, bool shareTypes)
	{
		this.shareTypes = shareTypes;
		this.cache = cache;
		if (shareTypes)
		{
			cache.GenerateSchemaGraph(this);
		}
	}

	internal bool IsReference(XmlSchemaObject type)
	{
		XmlSchemaObject xmlSchemaObject = type;
		while (xmlSchemaObject.Parent != null)
		{
			xmlSchemaObject = xmlSchemaObject.Parent;
		}
		return References.Contains(xmlSchemaObject);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Serialization.XmlSchemas" /> class. </summary>
	public XmlSchemas()
	{
	}
}
