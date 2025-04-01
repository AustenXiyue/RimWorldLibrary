using System.Collections;
using System.Threading;
using System.Xml.XmlConfiguration;

namespace System.Xml.Schema;

/// <summary>Contains a cache of XML Schema definition language (XSD) and XML-Data Reduced (XDR) schemas. The <see cref="T:System.Xml.Schema.XmlSchemaCollection" /> class class is obsolete. Use <see cref="T:System.Xml.Schema.XmlSchemaSet" /> instead.</summary>
[Obsolete("Use System.Xml.Schema.XmlSchemaSet for schema compilation and validation. http://go.microsoft.com/fwlink/?linkid=14202")]
public sealed class XmlSchemaCollection : ICollection, IEnumerable
{
	private Hashtable collection;

	private XmlNameTable nameTable;

	private SchemaNames schemaNames;

	private ReaderWriterLock wLock;

	private int timeout = -1;

	private bool isThreadSafe = true;

	private ValidationEventHandler validationEventHandler;

	private XmlResolver xmlResolver;

	/// <summary>Gets the number of namespaces defined in this collection.</summary>
	/// <returns>The number of namespaces defined in this collection.</returns>
	public int Count => collection.Count;

	/// <summary>Gets the default XmlNameTable used by the XmlSchemaCollection when loading new schemas.</summary>
	/// <returns>An XmlNameTable.</returns>
	public XmlNameTable NameTable => nameTable;

	internal XmlResolver XmlResolver
	{
		set
		{
			xmlResolver = value;
		}
	}

	/// <summary>Gets the <see cref="T:System.Xml.Schema.XmlSchema" /> associated with the given namespace URI.</summary>
	/// <returns>The XmlSchema associated with the namespace URI; null if there is no loaded schema associated with the given namespace or if the namespace is associated with an XDR schema.</returns>
	/// <param name="ns">The namespace URI associated with the schema you want to return. This will typically be the targetNamespace of the schema. </param>
	public XmlSchema this[string ns] => ((XmlSchemaCollectionNode)collection[(ns != null) ? ns : string.Empty])?.Schema;

	/// <summary>For a description of this member, see <see cref="P:System.Xml.Schema.XmlSchemaCollection.System.Collections.ICollection.IsSynchronized" />.</summary>
	/// <returns>Returns true if the collection is synchronized, otherwise false.</returns>
	bool ICollection.IsSynchronized => true;

	/// <summary>For a description of this member, see <see cref="P:System.Xml.Schema.XmlSchemaCollection.System.Collections.ICollection.SyncRoot" />.</summary>
	/// <returns>Returns a <see cref="T:System.Collections.ICollection.SyncRoot" /> object that can be used to synchronize access to the collection.</returns>
	object ICollection.SyncRoot => this;

	/// <summary>For a description of this member, see <see cref="P:System.Xml.Schema.XmlSchemaCollection.Count" />.</summary>
	/// <returns>Returns the count of the items in the collection.</returns>
	int ICollection.Count => collection.Count;

	internal ValidationEventHandler EventHandler
	{
		get
		{
			return validationEventHandler;
		}
		set
		{
			validationEventHandler = value;
		}
	}

	/// <summary>Sets an event handler for receiving information about the XDR and XML schema validation errors.</summary>
	public event ValidationEventHandler ValidationEventHandler
	{
		add
		{
			validationEventHandler = (ValidationEventHandler)Delegate.Combine(validationEventHandler, value);
		}
		remove
		{
			validationEventHandler = (ValidationEventHandler)Delegate.Remove(validationEventHandler, value);
		}
	}

	/// <summary>Initializes a new instance of the XmlSchemaCollection class.</summary>
	public XmlSchemaCollection()
		: this(new NameTable())
	{
	}

	/// <summary>Initializes a new instance of the XmlSchemaCollection class with the specified <see cref="T:System.Xml.XmlNameTable" />. The XmlNameTable is used when loading schemas.</summary>
	/// <param name="nametable">The XmlNameTable to use. </param>
	public XmlSchemaCollection(XmlNameTable nametable)
	{
		if (nametable == null)
		{
			throw new ArgumentNullException("nametable");
		}
		nameTable = nametable;
		collection = Hashtable.Synchronized(new Hashtable());
		xmlResolver = XmlReaderSection.CreateDefaultResolver();
		isThreadSafe = true;
		if (isThreadSafe)
		{
			wLock = new ReaderWriterLock();
		}
	}

	/// <summary>Adds the schema located by the given URL into the schema collection.</summary>
	/// <returns>The <see cref="T:System.Xml.Schema.XmlSchema" /> added to the schema collection; null if the schema being added is an XDR schema or if there are compilation errors in the schema. </returns>
	/// <param name="ns">The namespace URI associated with the schema. For XML Schemas, this will typically be the targetNamespace. </param>
	/// <param name="uri">The URL that specifies the schema to load. </param>
	/// <exception cref="T:System.Xml.XmlException">The schema is not a valid schema. </exception>
	public XmlSchema Add(string ns, string uri)
	{
		if (uri == null || uri.Length == 0)
		{
			throw new ArgumentNullException("uri");
		}
		XmlTextReader xmlTextReader = new XmlTextReader(uri, nameTable);
		xmlTextReader.XmlResolver = xmlResolver;
		XmlSchema xmlSchema = null;
		try
		{
			xmlSchema = Add(ns, xmlTextReader, xmlResolver);
			while (xmlTextReader.Read())
			{
			}
			return xmlSchema;
		}
		finally
		{
			xmlTextReader.Close();
		}
	}

	/// <summary>Adds the schema contained in the <see cref="T:System.Xml.XmlReader" /> to the schema collection.</summary>
	/// <returns>The <see cref="T:System.Xml.Schema.XmlSchema" /> added to the schema collection; null if the schema being added is an XDR schema or if there are compilation errors in the schema.</returns>
	/// <param name="ns">The namespace URI associated with the schema. For XML Schemas, this will typically be the targetNamespace. </param>
	/// <param name="reader">
	///   <see cref="T:System.Xml.XmlReader" /> containing the schema to add. </param>
	/// <exception cref="T:System.Xml.XmlException">The schema is not a valid schema. </exception>
	public XmlSchema Add(string ns, XmlReader reader)
	{
		return Add(ns, reader, xmlResolver);
	}

	/// <summary>Adds the schema contained in the <see cref="T:System.Xml.XmlReader" /> to the schema collection. The specified <see cref="T:System.Xml.XmlResolver" /> is used to resolve any external resources.</summary>
	/// <returns>The <see cref="T:System.Xml.Schema.XmlSchema" /> added to the schema collection; null if the schema being added is an XDR schema or if there are compilation errors in the schema.</returns>
	/// <param name="ns">The namespace URI associated with the schema. For XML Schemas, this will typically be the targetNamespace. </param>
	/// <param name="reader">
	///   <see cref="T:System.Xml.XmlReader" /> containing the schema to add. </param>
	/// <param name="resolver">The <see cref="T:System.Xml.XmlResolver" /> used to resolve namespaces referenced in include and import elements or x-schema attribute (XDR schemas). If this is null, external references are not resolved. </param>
	/// <exception cref="T:System.Xml.XmlException">The schema is not a valid schema. </exception>
	public XmlSchema Add(string ns, XmlReader reader, XmlResolver resolver)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		XmlNameTable nt = reader.NameTable;
		SchemaInfo schemaInfo = new SchemaInfo();
		Parser parser = new Parser(SchemaType.None, nt, GetSchemaNames(nt), validationEventHandler);
		parser.XmlResolver = resolver;
		SchemaType schemaType;
		try
		{
			schemaType = parser.Parse(reader, ns);
		}
		catch (XmlSchemaException e)
		{
			SendValidationEvent(e);
			return null;
		}
		if (schemaType == SchemaType.XSD)
		{
			schemaInfo.SchemaType = SchemaType.XSD;
			return Add(ns, schemaInfo, parser.XmlSchema, compile: true, resolver);
		}
		_ = parser.XdrSchema;
		return Add(ns, parser.XdrSchema, null, compile: true, resolver);
	}

	/// <summary>Adds the <see cref="T:System.Xml.Schema.XmlSchema" /> to the collection.</summary>
	/// <returns>The XmlSchema object.</returns>
	/// <param name="schema">The XmlSchema to add to the collection. </param>
	public XmlSchema Add(XmlSchema schema)
	{
		return Add(schema, xmlResolver);
	}

	/// <summary>Adds the <see cref="T:System.Xml.Schema.XmlSchema" /> to the collection. The specified <see cref="T:System.Xml.XmlResolver" /> is used to resolve any external references.</summary>
	/// <returns>The XmlSchema added to the schema collection.</returns>
	/// <param name="schema">The XmlSchema to add to the collection. </param>
	/// <param name="resolver">The <see cref="T:System.Xml.XmlResolver" /> used to resolve namespaces referenced in include and import elements. If this is null, external references are not resolved. </param>
	/// <exception cref="T:System.Xml.XmlException">The schema is not a valid schema. </exception>
	public XmlSchema Add(XmlSchema schema, XmlResolver resolver)
	{
		if (schema == null)
		{
			throw new ArgumentNullException("schema");
		}
		SchemaInfo schemaInfo = new SchemaInfo();
		schemaInfo.SchemaType = SchemaType.XSD;
		return Add(schema.TargetNamespace, schemaInfo, schema, compile: true, resolver);
	}

	/// <summary>Adds all the namespaces defined in the given collection (including their associated schemas) to this collection.</summary>
	/// <param name="schema">The XmlSchemaCollection you want to add to this collection. </param>
	public void Add(XmlSchemaCollection schema)
	{
		if (schema == null)
		{
			throw new ArgumentNullException("schema");
		}
		if (this != schema)
		{
			IDictionaryEnumerator enumerator = schema.collection.GetEnumerator();
			while (enumerator.MoveNext())
			{
				XmlSchemaCollectionNode xmlSchemaCollectionNode = (XmlSchemaCollectionNode)enumerator.Value;
				Add(xmlSchemaCollectionNode.NamespaceURI, xmlSchemaCollectionNode);
			}
		}
	}

	/// <summary>Gets a value indicating whether the targetNamespace of the specified <see cref="T:System.Xml.Schema.XmlSchema" /> is in the collection.</summary>
	/// <returns>true if there is a schema in the collection with the same targetNamespace; otherwise, false.</returns>
	/// <param name="schema">The XmlSchema object. </param>
	public bool Contains(XmlSchema schema)
	{
		if (schema == null)
		{
			throw new ArgumentNullException("schema");
		}
		return this[schema.TargetNamespace] != null;
	}

	/// <summary>Gets a value indicating whether a schema with the specified namespace is in the collection.</summary>
	/// <returns>true if a schema with the specified namespace is in the collection; otherwise, false.</returns>
	/// <param name="ns">The namespace URI associated with the schema. For XML Schemas, this will typically be the target namespace. </param>
	public bool Contains(string ns)
	{
		return collection[(ns != null) ? ns : string.Empty] != null;
	}

	/// <summary>For a description of this member, see <see cref="M:System.Xml.Schema.XmlSchemaCollection.GetEnumerator" />.</summary>
	/// <returns>Returns the <see cref="T:System.Collections.IEnumerator" /> for the collection.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return new XmlSchemaCollectionEnumerator(collection);
	}

	/// <summary>Provides support for the "for each" style iteration over the collection of schemas.</summary>
	/// <returns>An enumerator for iterating over all schemas in the current collection.</returns>
	public XmlSchemaCollectionEnumerator GetEnumerator()
	{
		return new XmlSchemaCollectionEnumerator(collection);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Xml.Schema.XmlSchemaCollection.CopyTo(System.Xml.Schema.XmlSchema[],System.Int32)" />.</summary>
	/// <param name="array">The array to copy the objects to. </param>
	/// <param name="index">The index in <paramref name="array" /> where copying will begin. </param>
	void ICollection.CopyTo(Array array, int index)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		XmlSchemaCollectionEnumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (index == array.Length && array.IsFixedSize)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			array.SetValue(enumerator.Current, index++);
		}
	}

	/// <summary>Copies all the XmlSchema objects from this collection into the given array starting at the given index.</summary>
	/// <param name="array">The array to copy the objects to. </param>
	/// <param name="index">The index in <paramref name="array" /> where copying will begin. </param>
	public void CopyTo(XmlSchema[] array, int index)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		XmlSchemaCollectionEnumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (enumerator.Current != null)
			{
				if (index == array.Length)
				{
					throw new ArgumentOutOfRangeException("index");
				}
				array[index++] = enumerator.Current;
			}
		}
	}

	internal SchemaInfo GetSchemaInfo(string ns)
	{
		return ((XmlSchemaCollectionNode)collection[(ns != null) ? ns : string.Empty])?.SchemaInfo;
	}

	internal SchemaNames GetSchemaNames(XmlNameTable nt)
	{
		if (nameTable != nt)
		{
			return new SchemaNames(nt);
		}
		if (schemaNames == null)
		{
			schemaNames = new SchemaNames(nameTable);
		}
		return schemaNames;
	}

	internal XmlSchema Add(string ns, SchemaInfo schemaInfo, XmlSchema schema, bool compile)
	{
		return Add(ns, schemaInfo, schema, compile, xmlResolver);
	}

	private XmlSchema Add(string ns, SchemaInfo schemaInfo, XmlSchema schema, bool compile, XmlResolver resolver)
	{
		int num = 0;
		if (schema != null)
		{
			if (schema.ErrorCount == 0 && compile)
			{
				if (!schema.CompileSchema(this, resolver, schemaInfo, ns, validationEventHandler, nameTable, CompileContentModel: true))
				{
					num = 1;
				}
				ns = ((schema.TargetNamespace == null) ? string.Empty : schema.TargetNamespace);
			}
			num += schema.ErrorCount;
		}
		else
		{
			num += schemaInfo.ErrorCount;
			ns = NameTable.Add(ns);
		}
		if (num == 0)
		{
			XmlSchemaCollectionNode xmlSchemaCollectionNode = new XmlSchemaCollectionNode();
			xmlSchemaCollectionNode.NamespaceURI = ns;
			xmlSchemaCollectionNode.SchemaInfo = schemaInfo;
			xmlSchemaCollectionNode.Schema = schema;
			Add(ns, xmlSchemaCollectionNode);
			return schema;
		}
		return null;
	}

	private void Add(string ns, XmlSchemaCollectionNode node)
	{
		if (isThreadSafe)
		{
			wLock.AcquireWriterLock(timeout);
		}
		try
		{
			if (collection[ns] != null)
			{
				collection.Remove(ns);
			}
			collection.Add(ns, node);
		}
		finally
		{
			if (isThreadSafe)
			{
				wLock.ReleaseWriterLock();
			}
		}
	}

	private void SendValidationEvent(XmlSchemaException e)
	{
		if (validationEventHandler != null)
		{
			validationEventHandler(this, new ValidationEventArgs(e));
			return;
		}
		throw e;
	}
}
