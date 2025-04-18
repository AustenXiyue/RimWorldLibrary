using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace System.Xml.Schema;

/// <summary>Contains a cache of XML Schema definition language (XSD) schemas.</summary>
public class XmlSchemaSet
{
	private XmlNameTable nameTable;

	private SchemaNames schemaNames;

	private SortedList schemas;

	private ValidationEventHandler internalEventHandler;

	private ValidationEventHandler eventHandler;

	private bool isCompiled;

	private Hashtable schemaLocations;

	private Hashtable chameleonSchemas;

	private Hashtable targetNamespaces;

	private bool compileAll;

	private SchemaInfo cachedCompiledInfo;

	private XmlReaderSettings readerSettings;

	private XmlSchema schemaForSchema;

	private XmlSchemaCompilationSettings compilationSettings;

	internal XmlSchemaObjectTable elements;

	internal XmlSchemaObjectTable attributes;

	internal XmlSchemaObjectTable schemaTypes;

	internal XmlSchemaObjectTable substitutionGroups;

	private XmlSchemaObjectTable typeExtensions;

	private object internalSyncObject;

	internal object InternalSyncObject
	{
		get
		{
			if (internalSyncObject == null)
			{
				object value = new object();
				Interlocked.CompareExchange<object>(ref internalSyncObject, value, (object)null);
			}
			return internalSyncObject;
		}
	}

	/// <summary>Gets the default <see cref="T:System.Xml.XmlNameTable" /> used by the <see cref="T:System.Xml.Schema.XmlSchemaSet" /> when loading new XML Schema definition language (XSD) schemas.</summary>
	/// <returns>A table of atomized string objects.</returns>
	public XmlNameTable NameTable => nameTable;

	/// <summary>Gets a value that indicates whether the XML Schema definition language (XSD) schemas in the <see cref="T:System.Xml.Schema.XmlSchemaSet" /> have been compiled.</summary>
	/// <returns>true if the schemas in the <see cref="T:System.Xml.Schema.XmlSchemaSet" /> have been compiled since the last time a schema was added or removed from the <see cref="T:System.Xml.Schema.XmlSchemaSet" />; otherwise, false.</returns>
	public bool IsCompiled => isCompiled;

	/// <summary>Sets the <see cref="T:System.Xml.XmlResolver" /> used to resolve namespaces or locations referenced in include and import elements of a schema.</summary>
	/// <returns>The <see cref="T:System.Xml.XmlResolver" /> used to resolve namespaces or locations referenced in include and import elements of a schema.</returns>
	public XmlResolver XmlResolver
	{
		set
		{
			readerSettings.XmlResolver = value;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Xml.Schema.XmlSchemaCompilationSettings" /> for the <see cref="T:System.Xml.Schema.XmlSchemaSet" />.</summary>
	/// <returns>The <see cref="T:System.Xml.Schema.XmlSchemaCompilationSettings" /> for the <see cref="T:System.Xml.Schema.XmlSchemaSet" />. The default is an <see cref="T:System.Xml.Schema.XmlSchemaCompilationSettings" /> instance with the <see cref="P:System.Xml.Schema.XmlSchemaCompilationSettings.EnableUpaCheck" /> property set to true.</returns>
	public XmlSchemaCompilationSettings CompilationSettings
	{
		get
		{
			return compilationSettings;
		}
		set
		{
			compilationSettings = value;
		}
	}

	/// <summary>Gets the number of logical XML Schema definition language (XSD) schemas in the <see cref="T:System.Xml.Schema.XmlSchemaSet" />.</summary>
	/// <returns>The number of logical schemas in the <see cref="T:System.Xml.Schema.XmlSchemaSet" />.</returns>
	public int Count => schemas.Count;

	/// <summary>Gets all the global elements in all the XML Schema definition language (XSD) schemas in the <see cref="T:System.Xml.Schema.XmlSchemaSet" />.</summary>
	/// <returns>The collection of global elements.</returns>
	public XmlSchemaObjectTable GlobalElements
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

	/// <summary>Gets all the global attributes in all the XML Schema definition language (XSD) schemas in the <see cref="T:System.Xml.Schema.XmlSchemaSet" />.</summary>
	/// <returns>The collection of global attributes.</returns>
	public XmlSchemaObjectTable GlobalAttributes
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

	/// <summary>Gets all of the global simple and complex types in all the XML Schema definition language (XSD) schemas in the <see cref="T:System.Xml.Schema.XmlSchemaSet" />.</summary>
	/// <returns>The collection of global simple and complex types.</returns>
	public XmlSchemaObjectTable GlobalTypes
	{
		get
		{
			if (schemaTypes == null)
			{
				schemaTypes = new XmlSchemaObjectTable();
			}
			return schemaTypes;
		}
	}

	internal XmlSchemaObjectTable SubstitutionGroups
	{
		get
		{
			if (substitutionGroups == null)
			{
				substitutionGroups = new XmlSchemaObjectTable();
			}
			return substitutionGroups;
		}
	}

	internal Hashtable SchemaLocations => schemaLocations;

	internal XmlSchemaObjectTable TypeExtensions
	{
		get
		{
			if (typeExtensions == null)
			{
				typeExtensions = new XmlSchemaObjectTable();
			}
			return typeExtensions;
		}
	}

	internal SchemaInfo CompiledInfo => cachedCompiledInfo;

	internal XmlReaderSettings ReaderSettings => readerSettings;

	internal SortedList SortedSchemas => schemas;

	internal bool CompileAll => compileAll;

	/// <summary>Specifies an event handler for receiving information about XML Schema definition language (XSD) schema validation errors.</summary>
	public event ValidationEventHandler ValidationEventHandler
	{
		add
		{
			eventHandler = (ValidationEventHandler)Delegate.Remove(eventHandler, internalEventHandler);
			eventHandler = (ValidationEventHandler)Delegate.Combine(eventHandler, value);
			if (eventHandler == null)
			{
				eventHandler = internalEventHandler;
			}
		}
		remove
		{
			eventHandler = (ValidationEventHandler)Delegate.Remove(eventHandler, value);
			if (eventHandler == null)
			{
				eventHandler = internalEventHandler;
			}
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaSet" /> class.</summary>
	public XmlSchemaSet()
		: this(new NameTable())
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaSet" /> class with the specified <see cref="T:System.Xml.XmlNameTable" />.</summary>
	/// <param name="nameTable">The <see cref="T:System.Xml.XmlNameTable" /> object to use.</param>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="T:System.Xml.XmlNameTable" /> object passed as a parameter is null.</exception>
	public XmlSchemaSet(XmlNameTable nameTable)
	{
		if (nameTable == null)
		{
			throw new ArgumentNullException("nameTable");
		}
		this.nameTable = nameTable;
		schemas = new SortedList();
		schemaLocations = new Hashtable();
		chameleonSchemas = new Hashtable();
		targetNamespaces = new Hashtable();
		internalEventHandler = InternalValidationCallback;
		eventHandler = internalEventHandler;
		readerSettings = new XmlReaderSettings();
		if (readerSettings.GetXmlResolver() == null)
		{
			readerSettings.XmlResolver = new XmlUrlResolver();
			readerSettings.IsXmlResolverSet = false;
		}
		readerSettings.NameTable = nameTable;
		readerSettings.DtdProcessing = DtdProcessing.Prohibit;
		compilationSettings = new XmlSchemaCompilationSettings();
		cachedCompiledInfo = new SchemaInfo();
		compileAll = true;
	}

	/// <summary>Adds the XML Schema definition language (XSD) schema at the URL specified to the <see cref="T:System.Xml.Schema.XmlSchemaSet" />.</summary>
	/// <returns>An <see cref="T:System.Xml.Schema.XmlSchema" /> object if the schema is valid. If the schema is not valid and a <see cref="T:System.Xml.Schema.ValidationEventHandler" /> is specified, then null is returned and the appropriate validation event is raised. Otherwise, an <see cref="T:System.Xml.Schema.XmlSchemaException" /> is thrown.</returns>
	/// <param name="targetNamespace">The schema targetNamespace property, or null to use the targetNamespace specified in the schema.</param>
	/// <param name="schemaUri">The URL that specifies the schema to load.</param>
	/// <exception cref="T:System.Xml.Schema.XmlSchemaException">The schema is not valid.</exception>
	/// <exception cref="T:System.ArgumentNullException">The URL passed as a parameter is null or <see cref="F:System.String.Empty" />.</exception>
	public XmlSchema Add(string targetNamespace, string schemaUri)
	{
		if (schemaUri == null || schemaUri.Length == 0)
		{
			throw new ArgumentNullException("schemaUri");
		}
		if (targetNamespace != null)
		{
			targetNamespace = XmlComplianceUtil.CDataNormalize(targetNamespace);
		}
		XmlSchema schema = null;
		lock (InternalSyncObject)
		{
			XmlResolver xmlResolver = readerSettings.GetXmlResolver();
			if (xmlResolver == null)
			{
				xmlResolver = new XmlUrlResolver();
			}
			Uri schemaUri2 = xmlResolver.ResolveUri(null, schemaUri);
			if (IsSchemaLoaded(schemaUri2, targetNamespace, out schema))
			{
				return schema;
			}
			XmlReader xmlReader = XmlReader.Create(schemaUri, readerSettings);
			try
			{
				schema = Add(targetNamespace, ParseSchema(targetNamespace, xmlReader));
				while (xmlReader.Read())
				{
				}
				return schema;
			}
			finally
			{
				xmlReader.Close();
			}
		}
	}

	/// <summary>Adds the XML Schema definition language (XSD) schema contained in the <see cref="T:System.Xml.XmlReader" /> to the <see cref="T:System.Xml.Schema.XmlSchemaSet" />.</summary>
	/// <returns>An <see cref="T:System.Xml.Schema.XmlSchema" /> object if the schema is valid. If the schema is not valid and a <see cref="T:System.Xml.Schema.ValidationEventHandler" /> is specified, then null is returned and the appropriate validation event is raised. Otherwise, an <see cref="T:System.Xml.Schema.XmlSchemaException" /> is thrown.</returns>
	/// <param name="targetNamespace">The schema targetNamespace property, or null to use the targetNamespace specified in the schema.</param>
	/// <param name="schemaDocument">The <see cref="T:System.Xml.XmlReader" /> object.</param>
	/// <exception cref="T:System.Xml.Schema.XmlSchemaException">The schema is not valid.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="T:System.Xml.XmlReader" /> object passed as a parameter is null.</exception>
	public XmlSchema Add(string targetNamespace, XmlReader schemaDocument)
	{
		if (schemaDocument == null)
		{
			throw new ArgumentNullException("schemaDocument");
		}
		if (targetNamespace != null)
		{
			targetNamespace = XmlComplianceUtil.CDataNormalize(targetNamespace);
		}
		lock (InternalSyncObject)
		{
			XmlSchema schema = null;
			Uri schemaUri = new Uri(schemaDocument.BaseURI, UriKind.RelativeOrAbsolute);
			if (IsSchemaLoaded(schemaUri, targetNamespace, out schema))
			{
				return schema;
			}
			DtdProcessing dtdProcessing = readerSettings.DtdProcessing;
			SetDtdProcessing(schemaDocument);
			schema = Add(targetNamespace, ParseSchema(targetNamespace, schemaDocument));
			readerSettings.DtdProcessing = dtdProcessing;
			return schema;
		}
	}

	/// <summary>Adds all the XML Schema definition language (XSD) schemas in the given <see cref="T:System.Xml.Schema.XmlSchemaSet" /> to the <see cref="T:System.Xml.Schema.XmlSchemaSet" />.</summary>
	/// <param name="schemas">The <see cref="T:System.Xml.Schema.XmlSchemaSet" /> object.</param>
	/// <exception cref="T:System.Xml.Schema.XmlSchemaException">A schema in the <see cref="T:System.Xml.Schema.XmlSchemaSet" /> is not valid.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="T:System.Xml.Schema.XmlSchemaSet" /> object passed as a parameter is null.</exception>
	public void Add(XmlSchemaSet schemas)
	{
		if (schemas == null)
		{
			throw new ArgumentNullException("schemas");
		}
		if (this == schemas)
		{
			return;
		}
		bool lockTaken = false;
		bool lockTaken2 = false;
		try
		{
			while (true)
			{
				Monitor.TryEnter(InternalSyncObject, ref lockTaken);
				if (lockTaken)
				{
					Monitor.TryEnter(schemas.InternalSyncObject, ref lockTaken2);
					if (lockTaken2)
					{
						break;
					}
					Monitor.Exit(InternalSyncObject);
					lockTaken = false;
					Thread.Yield();
				}
			}
			if (schemas.IsCompiled)
			{
				CopyFromCompiledSet(schemas);
				return;
			}
			bool flag = false;
			string text = null;
			foreach (XmlSchema value in schemas.SortedSchemas.Values)
			{
				text = value.TargetNamespace;
				if (text == null)
				{
					text = string.Empty;
				}
				if (!this.schemas.ContainsKey(value.SchemaId) && FindSchemaByNSAndUrl(value.BaseUri, text, null) == null && Add(value.TargetNamespace, value) == null)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return;
			}
			foreach (XmlSchema value2 in schemas.SortedSchemas.Values)
			{
				this.schemas.Remove(value2.SchemaId);
				schemaLocations.Remove(value2.BaseUri);
			}
		}
		finally
		{
			if (lockTaken)
			{
				Monitor.Exit(InternalSyncObject);
			}
			if (lockTaken2)
			{
				Monitor.Exit(schemas.InternalSyncObject);
			}
		}
	}

	/// <summary>Adds the given <see cref="T:System.Xml.Schema.XmlSchema" /> to the <see cref="T:System.Xml.Schema.XmlSchemaSet" />.</summary>
	/// <returns>An <see cref="T:System.Xml.Schema.XmlSchema" /> object if the schema is valid. If the schema is not valid and a <see cref="T:System.Xml.Schema.ValidationEventHandler" /> is specified, then null is returned and the appropriate validation event is raised. Otherwise, an <see cref="T:System.Xml.Schema.XmlSchemaException" /> is thrown.</returns>
	/// <param name="schema">The <see cref="T:System.Xml.Schema.XmlSchema" /> object to add to the <see cref="T:System.Xml.Schema.XmlSchemaSet" />.</param>
	/// <exception cref="T:System.Xml.Schema.XmlSchemaException">The schema is not valid.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="T:System.Xml.Schema.XmlSchema" /> object passed as a parameter is null.</exception>
	public XmlSchema Add(XmlSchema schema)
	{
		if (schema == null)
		{
			throw new ArgumentNullException("schema");
		}
		lock (InternalSyncObject)
		{
			if (schemas.ContainsKey(schema.SchemaId))
			{
				return schema;
			}
			return Add(schema.TargetNamespace, schema);
		}
	}

	/// <summary>Removes the specified XML Schema definition language (XSD) schema from the <see cref="T:System.Xml.Schema.XmlSchemaSet" />.</summary>
	/// <returns>The <see cref="T:System.Xml.Schema.XmlSchema" /> object removed from the <see cref="T:System.Xml.Schema.XmlSchemaSet" /> or null if the schema was not found in the <see cref="T:System.Xml.Schema.XmlSchemaSet" />.</returns>
	/// <param name="schema">The <see cref="T:System.Xml.Schema.XmlSchema" /> object to remove from the <see cref="T:System.Xml.Schema.XmlSchemaSet" />.</param>
	/// <exception cref="T:System.Xml.Schema.XmlSchemaException">The schema is not a valid schema.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="T:System.Xml.Schema.XmlSchema" /> passed as a parameter is null.</exception>
	public XmlSchema Remove(XmlSchema schema)
	{
		return Remove(schema, forceCompile: true);
	}

	/// <summary>Removes the specified XML Schema definition language (XSD) schema and all the schemas it imports from the <see cref="T:System.Xml.Schema.XmlSchemaSet" />.</summary>
	/// <returns>true if the <see cref="T:System.Xml.Schema.XmlSchema" /> object and all its imports were successfully removed; otherwise, false.</returns>
	/// <param name="schemaToRemove">The <see cref="T:System.Xml.Schema.XmlSchema" /> object to remove from the <see cref="T:System.Xml.Schema.XmlSchemaSet" />.</param>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="T:System.Xml.Schema.XmlSchema" /> passed as a parameter is null.</exception>
	public bool RemoveRecursive(XmlSchema schemaToRemove)
	{
		if (schemaToRemove == null)
		{
			throw new ArgumentNullException("schemaToRemove");
		}
		if (!schemas.ContainsKey(schemaToRemove.SchemaId))
		{
			return false;
		}
		lock (InternalSyncObject)
		{
			if (schemas.ContainsKey(schemaToRemove.SchemaId))
			{
				Hashtable hashtable = new Hashtable();
				hashtable.Add(GetTargetNamespace(schemaToRemove), schemaToRemove);
				for (int i = 0; i < schemaToRemove.ImportedNamespaces.Count; i++)
				{
					string text = (string)schemaToRemove.ImportedNamespaces[i];
					if (hashtable[text] == null)
					{
						hashtable.Add(text, text);
					}
				}
				ArrayList arrayList = new ArrayList();
				XmlSchema xmlSchema;
				for (int j = 0; j < schemas.Count; j++)
				{
					xmlSchema = (XmlSchema)schemas.GetByIndex(j);
					if (xmlSchema != schemaToRemove && !schemaToRemove.ImportedSchemas.Contains(xmlSchema))
					{
						arrayList.Add(xmlSchema);
					}
				}
				xmlSchema = null;
				for (int k = 0; k < arrayList.Count; k++)
				{
					xmlSchema = (XmlSchema)arrayList[k];
					if (xmlSchema.ImportedNamespaces.Count <= 0)
					{
						continue;
					}
					foreach (string key in hashtable.Keys)
					{
						if (xmlSchema.ImportedNamespaces.Contains(key))
						{
							SendValidationEvent(new XmlSchemaException("The schema could not be removed because other schemas in the set have dependencies on this schema or its imports.", string.Empty), XmlSeverityType.Warning);
							return false;
						}
					}
				}
				Remove(schemaToRemove, forceCompile: true);
				for (int l = 0; l < schemaToRemove.ImportedSchemas.Count; l++)
				{
					XmlSchema schema = (XmlSchema)schemaToRemove.ImportedSchemas[l];
					Remove(schema, forceCompile: true);
				}
				return true;
			}
		}
		return false;
	}

	/// <summary>Indicates whether an XML Schema definition language (XSD) schema with the specified target namespace URI is in the <see cref="T:System.Xml.Schema.XmlSchemaSet" />.</summary>
	/// <returns>true if a schema with the specified target namespace URI is in the <see cref="T:System.Xml.Schema.XmlSchemaSet" />; otherwise, false.</returns>
	/// <param name="targetNamespace">The schema targetNamespace property.</param>
	public bool Contains(string targetNamespace)
	{
		if (targetNamespace == null)
		{
			targetNamespace = string.Empty;
		}
		return targetNamespaces[targetNamespace] != null;
	}

	/// <summary>Indicates whether the specified XML Schema definition language (XSD) <see cref="T:System.Xml.Schema.XmlSchema" /> object is in the <see cref="T:System.Xml.Schema.XmlSchemaSet" />.</summary>
	/// <returns>true if the <see cref="T:System.Xml.Schema.XmlSchema" /> object is in the <see cref="T:System.Xml.Schema.XmlSchemaSet" />; otherwise, false.</returns>
	/// <param name="schema">The <see cref="T:System.Xml.Schema.XmlSchema" /> object.</param>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="T:System.Xml.Schema.XmlSchemaSet" /> passed as a parameter is null.</exception>
	public bool Contains(XmlSchema schema)
	{
		if (schema == null)
		{
			throw new ArgumentNullException("schema");
		}
		return schemas.ContainsValue(schema);
	}

	/// <summary>Compiles the XML Schema definition language (XSD) schemas added to the <see cref="T:System.Xml.Schema.XmlSchemaSet" /> into one logical schema.</summary>
	/// <exception cref="T:System.Xml.Schema.XmlSchemaException">An error occurred when validating and compiling the schemas in the <see cref="T:System.Xml.Schema.XmlSchemaSet" />.</exception>
	public void Compile()
	{
		if (isCompiled)
		{
			return;
		}
		if (schemas.Count == 0)
		{
			ClearTables();
			cachedCompiledInfo = new SchemaInfo();
			isCompiled = true;
			compileAll = false;
			return;
		}
		lock (InternalSyncObject)
		{
			if (isCompiled)
			{
				return;
			}
			Compiler compiler = new Compiler(nameTable, eventHandler, schemaForSchema, compilationSettings);
			SchemaInfo schemaInfo = new SchemaInfo();
			int i = 0;
			if (!compileAll)
			{
				compiler.ImportAllCompiledSchemas(this);
			}
			try
			{
				XmlSchema buildInSchema = Preprocessor.GetBuildInSchema();
				for (i = 0; i < schemas.Count; i++)
				{
					XmlSchema xmlSchema = (XmlSchema)schemas.GetByIndex(i);
					Monitor.Enter(xmlSchema);
					if (!xmlSchema.IsPreprocessed)
					{
						SendValidationEvent(new XmlSchemaException("All schemas in the set should be successfully preprocessed prior to compilation.", string.Empty), XmlSeverityType.Error);
						isCompiled = false;
						return;
					}
					if (xmlSchema.IsCompiledBySet)
					{
						if (!compileAll)
						{
							continue;
						}
						if (xmlSchema == buildInSchema)
						{
							compiler.Prepare(xmlSchema, cleanup: false);
							continue;
						}
					}
					compiler.Prepare(xmlSchema, cleanup: true);
				}
				isCompiled = compiler.Execute(this, schemaInfo);
				if (isCompiled)
				{
					if (!compileAll)
					{
						schemaInfo.Add(cachedCompiledInfo, eventHandler);
					}
					compileAll = false;
					cachedCompiledInfo = schemaInfo;
				}
			}
			finally
			{
				if (i == schemas.Count)
				{
					i--;
				}
				for (int num = i; num >= 0; num--)
				{
					XmlSchema xmlSchema2 = (XmlSchema)schemas.GetByIndex(num);
					if (xmlSchema2 == Preprocessor.GetBuildInSchema())
					{
						Monitor.Exit(xmlSchema2);
					}
					else
					{
						xmlSchema2.IsCompiledBySet = isCompiled;
						Monitor.Exit(xmlSchema2);
					}
				}
			}
		}
	}

	/// <summary>Reprocesses an XML Schema definition language (XSD) schema that already exists in the <see cref="T:System.Xml.Schema.XmlSchemaSet" />.</summary>
	/// <returns>An <see cref="T:System.Xml.Schema.XmlSchema" /> object if the schema is a valid schema. If the schema is not valid and a <see cref="T:System.Xml.Schema.ValidationEventHandler" /> is specified, null is returned and the appropriate validation event is raised. Otherwise, an <see cref="T:System.Xml.Schema.XmlSchemaException" /> is thrown.</returns>
	/// <param name="schema">The schema to reprocess.</param>
	/// <exception cref="T:System.Xml.Schema.XmlSchemaException">The schema is not valid.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="T:System.Xml.Schema.XmlSchema" /> object passed as a parameter is null.</exception>
	/// <exception cref="T:System.ArgumentException">The <see cref="T:System.Xml.Schema.XmlSchema" /> object passed as a parameter does not already exist in the <see cref="T:System.Xml.Schema.XmlSchemaSet" />.</exception>
	public XmlSchema Reprocess(XmlSchema schema)
	{
		if (schema == null)
		{
			throw new ArgumentNullException("schema");
		}
		if (!schemas.ContainsKey(schema.SchemaId))
		{
			throw new ArgumentException(Res.GetString("Schema does not exist in the set."), "schema");
		}
		XmlSchema result = schema;
		lock (InternalSyncObject)
		{
			RemoveSchemaFromGlobalTables(schema);
			RemoveSchemaFromCaches(schema);
			if (schema.BaseUri != null)
			{
				schemaLocations.Remove(schema.BaseUri);
			}
			string targetNamespace = GetTargetNamespace(schema);
			if (Schemas(targetNamespace).Count == 0)
			{
				targetNamespaces.Remove(targetNamespace);
			}
			isCompiled = false;
			compileAll = true;
			if (schema.ErrorCount != 0)
			{
				return result;
			}
			if (PreprocessSchema(ref schema, schema.TargetNamespace))
			{
				if (targetNamespaces[targetNamespace] == null)
				{
					targetNamespaces.Add(targetNamespace, targetNamespace);
				}
				if (schemaForSchema == null && targetNamespace == "http://www.w3.org/2001/XMLSchema" && schema.SchemaTypes[DatatypeImplementation.QnAnyType] != null)
				{
					schemaForSchema = schema;
				}
				for (int i = 0; i < schema.ImportedSchemas.Count; i++)
				{
					XmlSchema xmlSchema = (XmlSchema)schema.ImportedSchemas[i];
					if (!schemas.ContainsKey(xmlSchema.SchemaId))
					{
						schemas.Add(xmlSchema.SchemaId, xmlSchema);
					}
					targetNamespace = GetTargetNamespace(xmlSchema);
					if (targetNamespaces[targetNamespace] == null)
					{
						targetNamespaces.Add(targetNamespace, targetNamespace);
					}
					if (schemaForSchema == null && targetNamespace == "http://www.w3.org/2001/XMLSchema" && schema.SchemaTypes[DatatypeImplementation.QnAnyType] != null)
					{
						schemaForSchema = schema;
					}
				}
				return schema;
			}
			return result;
		}
	}

	/// <summary>Copies all the <see cref="T:System.Xml.Schema.XmlSchema" /> objects from the <see cref="T:System.Xml.Schema.XmlSchemaSet" /> to the given array, starting at the given index.</summary>
	/// <param name="schemas">The array to copy the objects to.</param>
	/// <param name="index">The index in the array where copying will begin.</param>
	public void CopyTo(XmlSchema[] schemas, int index)
	{
		if (schemas == null)
		{
			throw new ArgumentNullException("schemas");
		}
		if (index < 0 || index > schemas.Length - 1)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		this.schemas.Values.CopyTo(schemas, index);
	}

	/// <summary>Returns a collection of all the XML Schema definition language (XSD) schemas in the <see cref="T:System.Xml.Schema.XmlSchemaSet" />.</summary>
	/// <returns>An <see cref="T:System.Collections.ICollection" /> object containing all the schemas that have been added to the <see cref="T:System.Xml.Schema.XmlSchemaSet" />. If no schemas have been added to the <see cref="T:System.Xml.Schema.XmlSchemaSet" />, an empty <see cref="T:System.Collections.ICollection" /> object is returned.</returns>
	public ICollection Schemas()
	{
		return schemas.Values;
	}

	/// <summary>Returns a collection of all the XML Schema definition language (XSD) schemas in the <see cref="T:System.Xml.Schema.XmlSchemaSet" /> that belong to the given namespace.</summary>
	/// <returns>An <see cref="T:System.Collections.ICollection" /> object containing all the schemas that have been added to the <see cref="T:System.Xml.Schema.XmlSchemaSet" /> that belong to the given namespace. If no schemas have been added to the <see cref="T:System.Xml.Schema.XmlSchemaSet" />, an empty <see cref="T:System.Collections.ICollection" /> object is returned.</returns>
	/// <param name="targetNamespace">The schema targetNamespace property.</param>
	public ICollection Schemas(string targetNamespace)
	{
		ArrayList arrayList = new ArrayList();
		if (targetNamespace == null)
		{
			targetNamespace = string.Empty;
		}
		for (int i = 0; i < schemas.Count; i++)
		{
			XmlSchema xmlSchema = (XmlSchema)schemas.GetByIndex(i);
			if (GetTargetNamespace(xmlSchema) == targetNamespace)
			{
				arrayList.Add(xmlSchema);
			}
		}
		return arrayList;
	}

	private XmlSchema Add(string targetNamespace, XmlSchema schema)
	{
		if (schema == null || schema.ErrorCount != 0)
		{
			return null;
		}
		if (PreprocessSchema(ref schema, targetNamespace))
		{
			AddSchemaToSet(schema);
			isCompiled = false;
			return schema;
		}
		return null;
	}

	internal void Add(string targetNamespace, XmlReader reader, Hashtable validatedNamespaces)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (targetNamespace == null)
		{
			targetNamespace = string.Empty;
		}
		if (validatedNamespaces[targetNamespace] != null)
		{
			if (FindSchemaByNSAndUrl(new Uri(reader.BaseURI, UriKind.RelativeOrAbsolute), targetNamespace, null) == null)
			{
				throw new XmlSchemaException("An element or attribute information item has already been validated from the '{0}' namespace. It is an error if 'xsi:schemaLocation', 'xsi:noNamespaceSchemaLocation', or an inline schema occurs for that namespace.", targetNamespace);
			}
		}
		else
		{
			if (IsSchemaLoaded(new Uri(reader.BaseURI, UriKind.RelativeOrAbsolute), targetNamespace, out var schema))
			{
				return;
			}
			schema = ParseSchema(targetNamespace, reader);
			DictionaryEntry[] array = new DictionaryEntry[schemaLocations.Count];
			schemaLocations.CopyTo(array, 0);
			Add(targetNamespace, schema);
			if (schema.ImportedSchemas.Count <= 0)
			{
				return;
			}
			for (int i = 0; i < schema.ImportedSchemas.Count; i++)
			{
				XmlSchema xmlSchema = (XmlSchema)schema.ImportedSchemas[i];
				string text = xmlSchema.TargetNamespace;
				if (text == null)
				{
					text = string.Empty;
				}
				if (validatedNamespaces[text] != null && FindSchemaByNSAndUrl(xmlSchema.BaseUri, text, array) == null)
				{
					RemoveRecursive(schema);
					throw new XmlSchemaException("An element or attribute information item has already been validated from the '{0}' namespace. It is an error if 'xsi:schemaLocation', 'xsi:noNamespaceSchemaLocation', or an inline schema occurs for that namespace.", text);
				}
			}
		}
	}

	internal XmlSchema FindSchemaByNSAndUrl(Uri schemaUri, string ns, DictionaryEntry[] locationsTable)
	{
		if (schemaUri == null || schemaUri.OriginalString.Length == 0)
		{
			return null;
		}
		XmlSchema xmlSchema = null;
		if (locationsTable == null)
		{
			xmlSchema = (XmlSchema)schemaLocations[schemaUri];
		}
		else
		{
			for (int i = 0; i < locationsTable.Length; i++)
			{
				if (schemaUri.Equals(locationsTable[i].Key))
				{
					xmlSchema = (XmlSchema)locationsTable[i].Value;
					break;
				}
			}
		}
		if (xmlSchema != null)
		{
			string text = ((xmlSchema.TargetNamespace == null) ? string.Empty : xmlSchema.TargetNamespace);
			if (text == ns)
			{
				return xmlSchema;
			}
			if (text == string.Empty)
			{
				ChameleonKey key = new ChameleonKey(ns, xmlSchema);
				xmlSchema = (XmlSchema)chameleonSchemas[key];
			}
			else
			{
				xmlSchema = null;
			}
		}
		return xmlSchema;
	}

	private void SetDtdProcessing(XmlReader reader)
	{
		if (reader.Settings != null)
		{
			readerSettings.DtdProcessing = reader.Settings.DtdProcessing;
		}
		else if (reader is XmlTextReader xmlTextReader)
		{
			readerSettings.DtdProcessing = xmlTextReader.DtdProcessing;
		}
	}

	private void AddSchemaToSet(XmlSchema schema)
	{
		schemas.Add(schema.SchemaId, schema);
		string targetNamespace = GetTargetNamespace(schema);
		if (targetNamespaces[targetNamespace] == null)
		{
			targetNamespaces.Add(targetNamespace, targetNamespace);
		}
		if (schemaForSchema == null && targetNamespace == "http://www.w3.org/2001/XMLSchema" && schema.SchemaTypes[DatatypeImplementation.QnAnyType] != null)
		{
			schemaForSchema = schema;
		}
		for (int i = 0; i < schema.ImportedSchemas.Count; i++)
		{
			XmlSchema xmlSchema = (XmlSchema)schema.ImportedSchemas[i];
			if (!schemas.ContainsKey(xmlSchema.SchemaId))
			{
				schemas.Add(xmlSchema.SchemaId, xmlSchema);
			}
			targetNamespace = GetTargetNamespace(xmlSchema);
			if (targetNamespaces[targetNamespace] == null)
			{
				targetNamespaces.Add(targetNamespace, targetNamespace);
			}
			if (schemaForSchema == null && targetNamespace == "http://www.w3.org/2001/XMLSchema" && schema.SchemaTypes[DatatypeImplementation.QnAnyType] != null)
			{
				schemaForSchema = schema;
			}
		}
	}

	private void ProcessNewSubstitutionGroups(XmlSchemaObjectTable substitutionGroupsTable, bool resolve)
	{
		foreach (XmlSchemaSubstitutionGroup value in substitutionGroupsTable.Values)
		{
			if (resolve)
			{
				ResolveSubstitutionGroup(value, substitutionGroupsTable);
			}
			XmlQualifiedName examplar = value.Examplar;
			XmlSchemaSubstitutionGroup xmlSchemaSubstitutionGroup2 = (XmlSchemaSubstitutionGroup)substitutionGroups[examplar];
			if (xmlSchemaSubstitutionGroup2 != null)
			{
				for (int i = 0; i < value.Members.Count; i++)
				{
					if (!xmlSchemaSubstitutionGroup2.Members.Contains(value.Members[i]))
					{
						xmlSchemaSubstitutionGroup2.Members.Add(value.Members[i]);
					}
				}
			}
			else
			{
				AddToTable(substitutionGroups, examplar, value);
			}
		}
	}

	private void ResolveSubstitutionGroup(XmlSchemaSubstitutionGroup substitutionGroup, XmlSchemaObjectTable substTable)
	{
		List<XmlSchemaElement> list = null;
		XmlSchemaElement xmlSchemaElement = (XmlSchemaElement)elements[substitutionGroup.Examplar];
		if (substitutionGroup.Members.Contains(xmlSchemaElement))
		{
			return;
		}
		for (int i = 0; i < substitutionGroup.Members.Count; i++)
		{
			XmlSchemaElement xmlSchemaElement2 = (XmlSchemaElement)substitutionGroup.Members[i];
			XmlSchemaSubstitutionGroup xmlSchemaSubstitutionGroup = (XmlSchemaSubstitutionGroup)substTable[xmlSchemaElement2.QualifiedName];
			if (xmlSchemaSubstitutionGroup == null)
			{
				continue;
			}
			ResolveSubstitutionGroup(xmlSchemaSubstitutionGroup, substTable);
			for (int j = 0; j < xmlSchemaSubstitutionGroup.Members.Count; j++)
			{
				XmlSchemaElement xmlSchemaElement3 = (XmlSchemaElement)xmlSchemaSubstitutionGroup.Members[j];
				if (xmlSchemaElement3 != xmlSchemaElement2)
				{
					if (list == null)
					{
						list = new List<XmlSchemaElement>();
					}
					list.Add(xmlSchemaElement3);
				}
			}
		}
		if (list != null)
		{
			for (int k = 0; k < list.Count; k++)
			{
				substitutionGroup.Members.Add(list[k]);
			}
		}
		substitutionGroup.Members.Add(xmlSchemaElement);
	}

	internal XmlSchema Remove(XmlSchema schema, bool forceCompile)
	{
		if (schema == null)
		{
			throw new ArgumentNullException("schema");
		}
		lock (InternalSyncObject)
		{
			if (schemas.ContainsKey(schema.SchemaId))
			{
				if (forceCompile)
				{
					RemoveSchemaFromGlobalTables(schema);
					RemoveSchemaFromCaches(schema);
				}
				schemas.Remove(schema.SchemaId);
				if (schema.BaseUri != null)
				{
					schemaLocations.Remove(schema.BaseUri);
				}
				string targetNamespace = GetTargetNamespace(schema);
				if (Schemas(targetNamespace).Count == 0)
				{
					targetNamespaces.Remove(targetNamespace);
				}
				if (forceCompile)
				{
					isCompiled = false;
					compileAll = true;
				}
				return schema;
			}
		}
		return null;
	}

	private void ClearTables()
	{
		GlobalElements.Clear();
		GlobalAttributes.Clear();
		GlobalTypes.Clear();
		SubstitutionGroups.Clear();
		TypeExtensions.Clear();
	}

	internal bool PreprocessSchema(ref XmlSchema schema, string targetNamespace)
	{
		Preprocessor preprocessor = new Preprocessor(nameTable, GetSchemaNames(nameTable), eventHandler, compilationSettings);
		preprocessor.XmlResolver = readerSettings.GetXmlResolver_CheckConfig();
		preprocessor.ReaderSettings = readerSettings;
		preprocessor.SchemaLocations = schemaLocations;
		preprocessor.ChameleonSchemas = chameleonSchemas;
		bool result = preprocessor.Execute(schema, targetNamespace, loadExternals: true);
		schema = preprocessor.RootSchema;
		return result;
	}

	internal XmlSchema ParseSchema(string targetNamespace, XmlReader reader)
	{
		XmlNameTable nt = reader.NameTable;
		SchemaNames schemaNames = GetSchemaNames(nt);
		Parser parser = new Parser(SchemaType.XSD, nt, schemaNames, eventHandler);
		parser.XmlResolver = readerSettings.GetXmlResolver_CheckConfig();
		try
		{
			parser.Parse(reader, targetNamespace);
		}
		catch (XmlSchemaException e)
		{
			SendValidationEvent(e, XmlSeverityType.Error);
			return null;
		}
		return parser.XmlSchema;
	}

	internal void CopyFromCompiledSet(XmlSchemaSet otherSet)
	{
		SortedList sortedSchemas = otherSet.SortedSchemas;
		bool flag = schemas.Count == 0;
		ArrayList arrayList = new ArrayList();
		SchemaInfo schemaInfo = new SchemaInfo();
		for (int i = 0; i < sortedSchemas.Count; i++)
		{
			XmlSchema xmlSchema = (XmlSchema)sortedSchemas.GetByIndex(i);
			Uri baseUri = xmlSchema.BaseUri;
			if (schemas.ContainsKey(xmlSchema.SchemaId) || (baseUri != null && baseUri.OriginalString.Length != 0 && schemaLocations[baseUri] != null))
			{
				arrayList.Add(xmlSchema);
				continue;
			}
			schemas.Add(xmlSchema.SchemaId, xmlSchema);
			if (baseUri != null && baseUri.OriginalString.Length != 0)
			{
				schemaLocations.Add(baseUri, xmlSchema);
			}
			string targetNamespace = GetTargetNamespace(xmlSchema);
			if (targetNamespaces[targetNamespace] == null)
			{
				targetNamespaces.Add(targetNamespace, targetNamespace);
			}
		}
		VerifyTables();
		foreach (XmlSchemaElement value in otherSet.GlobalElements.Values)
		{
			if (AddToTable(elements, value.QualifiedName, value))
			{
				continue;
			}
			goto IL_026e;
		}
		foreach (XmlSchemaAttribute value2 in otherSet.GlobalAttributes.Values)
		{
			if (AddToTable(attributes, value2.QualifiedName, value2))
			{
				continue;
			}
			goto IL_026e;
		}
		foreach (XmlSchemaType value3 in otherSet.GlobalTypes.Values)
		{
			if (AddToTable(schemaTypes, value3.QualifiedName, value3))
			{
				continue;
			}
			goto IL_026e;
		}
		ProcessNewSubstitutionGroups(otherSet.SubstitutionGroups, resolve: false);
		schemaInfo.Add(cachedCompiledInfo, eventHandler);
		schemaInfo.Add(otherSet.CompiledInfo, eventHandler);
		cachedCompiledInfo = schemaInfo;
		if (flag)
		{
			isCompiled = true;
			compileAll = false;
		}
		return;
		IL_026e:
		foreach (XmlSchema value4 in sortedSchemas.Values)
		{
			if (!arrayList.Contains(value4))
			{
				Remove(value4, forceCompile: false);
			}
		}
		foreach (XmlSchemaElement value5 in otherSet.GlobalElements.Values)
		{
			if (!arrayList.Contains((XmlSchema)value5.Parent))
			{
				elements.Remove(value5.QualifiedName);
			}
		}
		foreach (XmlSchemaAttribute value6 in otherSet.GlobalAttributes.Values)
		{
			if (!arrayList.Contains((XmlSchema)value6.Parent))
			{
				attributes.Remove(value6.QualifiedName);
			}
		}
		foreach (XmlSchemaType value7 in otherSet.GlobalTypes.Values)
		{
			if (!arrayList.Contains((XmlSchema)value7.Parent))
			{
				schemaTypes.Remove(value7.QualifiedName);
			}
		}
	}

	internal XmlResolver GetResolver()
	{
		return readerSettings.GetXmlResolver_CheckConfig();
	}

	internal ValidationEventHandler GetEventHandler()
	{
		return eventHandler;
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

	internal bool IsSchemaLoaded(Uri schemaUri, string targetNamespace, out XmlSchema schema)
	{
		schema = null;
		if (targetNamespace == null)
		{
			targetNamespace = string.Empty;
		}
		if (GetSchemaByUri(schemaUri, out schema))
		{
			if (!schemas.ContainsKey(schema.SchemaId) || (targetNamespace.Length != 0 && !(targetNamespace == schema.TargetNamespace)))
			{
				if (schema.TargetNamespace == null)
				{
					XmlSchema xmlSchema = FindSchemaByNSAndUrl(schemaUri, targetNamespace, null);
					if (xmlSchema != null && schemas.ContainsKey(xmlSchema.SchemaId))
					{
						schema = xmlSchema;
					}
					else
					{
						schema = Add(targetNamespace, schema);
					}
				}
				else if (targetNamespace.Length != 0 && targetNamespace != schema.TargetNamespace)
				{
					SendValidationEvent(new XmlSchemaException("The targetNamespace parameter '{0}' should be the same value as the targetNamespace '{1}' of the schema.", new string[2] { targetNamespace, schema.TargetNamespace }), XmlSeverityType.Error);
					schema = null;
				}
				else
				{
					AddSchemaToSet(schema);
				}
			}
			return true;
		}
		return false;
	}

	internal bool GetSchemaByUri(Uri schemaUri, out XmlSchema schema)
	{
		schema = null;
		if (schemaUri == null || schemaUri.OriginalString.Length == 0)
		{
			return false;
		}
		schema = (XmlSchema)schemaLocations[schemaUri];
		if (schema != null)
		{
			return true;
		}
		return false;
	}

	internal string GetTargetNamespace(XmlSchema schema)
	{
		if (schema.TargetNamespace != null)
		{
			return schema.TargetNamespace;
		}
		return string.Empty;
	}

	private void RemoveSchemaFromCaches(XmlSchema schema)
	{
		List<XmlSchema> list = new List<XmlSchema>();
		schema.GetExternalSchemasList(list, schema);
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].BaseUri != null && list[i].BaseUri.OriginalString.Length != 0)
			{
				schemaLocations.Remove(list[i].BaseUri);
			}
			ICollection keys = chameleonSchemas.Keys;
			ArrayList arrayList = new ArrayList();
			foreach (ChameleonKey item in keys)
			{
				if (item.chameleonLocation.Equals(list[i].BaseUri) && (item.originalSchema == null || item.originalSchema == list[i]))
				{
					arrayList.Add(item);
				}
			}
			for (int j = 0; j < arrayList.Count; j++)
			{
				chameleonSchemas.Remove(arrayList[j]);
			}
		}
	}

	private void RemoveSchemaFromGlobalTables(XmlSchema schema)
	{
		if (schemas.Count == 0)
		{
			return;
		}
		VerifyTables();
		foreach (XmlSchemaElement value in schema.Elements.Values)
		{
			if ((XmlSchemaElement)elements[value.QualifiedName] == value)
			{
				elements.Remove(value.QualifiedName);
			}
		}
		foreach (XmlSchemaAttribute value2 in schema.Attributes.Values)
		{
			if ((XmlSchemaAttribute)attributes[value2.QualifiedName] == value2)
			{
				attributes.Remove(value2.QualifiedName);
			}
		}
		foreach (XmlSchemaType value3 in schema.SchemaTypes.Values)
		{
			if ((XmlSchemaType)schemaTypes[value3.QualifiedName] == value3)
			{
				schemaTypes.Remove(value3.QualifiedName);
			}
		}
	}

	private bool AddToTable(XmlSchemaObjectTable table, XmlQualifiedName qname, XmlSchemaObject item)
	{
		if (qname.Name.Length == 0)
		{
			return true;
		}
		XmlSchemaObject xmlSchemaObject = table[qname];
		if (xmlSchemaObject != null)
		{
			if (xmlSchemaObject == item || xmlSchemaObject.SourceUri == item.SourceUri)
			{
				return true;
			}
			string res = string.Empty;
			if (item is XmlSchemaComplexType)
			{
				res = "The complexType '{0}' has already been declared.";
			}
			else if (item is XmlSchemaSimpleType)
			{
				res = "The simpleType '{0}' has already been declared.";
			}
			else if (item is XmlSchemaElement)
			{
				res = "The global element '{0}' has already been declared.";
			}
			else if (item is XmlSchemaAttribute)
			{
				if (qname.Namespace == "http://www.w3.org/XML/1998/namespace")
				{
					XmlSchemaObject xmlSchemaObject2 = Preprocessor.GetBuildInSchema().Attributes[qname];
					if (xmlSchemaObject == xmlSchemaObject2)
					{
						table.Insert(qname, item);
						return true;
					}
					if (item == xmlSchemaObject2)
					{
						return true;
					}
				}
				res = "The global attribute '{0}' has already been declared.";
			}
			SendValidationEvent(new XmlSchemaException(res, qname.ToString()), XmlSeverityType.Error);
			return false;
		}
		table.Add(qname, item);
		return true;
	}

	private void VerifyTables()
	{
		if (elements == null)
		{
			elements = new XmlSchemaObjectTable();
		}
		if (attributes == null)
		{
			attributes = new XmlSchemaObjectTable();
		}
		if (schemaTypes == null)
		{
			schemaTypes = new XmlSchemaObjectTable();
		}
		if (substitutionGroups == null)
		{
			substitutionGroups = new XmlSchemaObjectTable();
		}
	}

	private void InternalValidationCallback(object sender, ValidationEventArgs e)
	{
		if (e.Severity == XmlSeverityType.Error)
		{
			throw e.Exception;
		}
	}

	private void SendValidationEvent(XmlSchemaException e, XmlSeverityType severity)
	{
		if (eventHandler != null)
		{
			eventHandler(this, new ValidationEventArgs(e, severity));
			return;
		}
		throw e;
	}
}
