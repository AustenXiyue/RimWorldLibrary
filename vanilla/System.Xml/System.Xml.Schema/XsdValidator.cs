using System.Collections;
using System.IO;
using System.Text;

namespace System.Xml.Schema;

internal sealed class XsdValidator : BaseValidator
{
	private int startIDConstraint = -1;

	private const int STACK_INCREMENT = 10;

	private HWStack validationStack;

	private Hashtable attPresence;

	private XmlNamespaceManager nsManager;

	private bool bManageNamespaces;

	private Hashtable IDs;

	private IdRefNode idRefListHead;

	private Parser inlineSchemaParser;

	private XmlSchemaContentProcessing processContents;

	private static readonly XmlSchemaDatatype dtCDATA = XmlSchemaDatatype.FromXmlTokenizedType(XmlTokenizedType.CDATA);

	private static readonly XmlSchemaDatatype dtQName = XmlSchemaDatatype.FromXmlTokenizedTypeXsd(XmlTokenizedType.QName);

	private static readonly XmlSchemaDatatype dtStringArray = dtCDATA.DeriveByList(null);

	private string NsXmlNs;

	private string NsXs;

	private string NsXsi;

	private string XsiType;

	private string XsiNil;

	private string XsiSchemaLocation;

	private string XsiNoNamespaceSchemaLocation;

	private string XsdSchema;

	public ValidationState Context
	{
		set
		{
			context = value;
		}
	}

	public static XmlSchemaDatatype DtQName => dtQName;

	private bool IsInlineSchemaStarted => inlineSchemaParser != null;

	private bool HasSchema => schemaInfo.SchemaType != SchemaType.None;

	public override bool PreserveWhitespace
	{
		get
		{
			if (context.ElementDecl == null)
			{
				return false;
			}
			return context.ElementDecl.ContentValidator.PreserveWhitespace;
		}
	}

	private bool HasIdentityConstraints => startIDConstraint != -1;

	internal XsdValidator(BaseValidator validator)
		: base(validator)
	{
		Init();
	}

	internal XsdValidator(XmlValidatingReaderImpl reader, XmlSchemaCollection schemaCollection, IValidationEventHandling eventHandling)
		: base(reader, schemaCollection, eventHandling)
	{
		Init();
	}

	private void Init()
	{
		nsManager = reader.NamespaceManager;
		if (nsManager == null)
		{
			nsManager = new XmlNamespaceManager(base.NameTable);
			bManageNamespaces = true;
		}
		validationStack = new HWStack(10);
		textValue = new StringBuilder();
		attPresence = new Hashtable();
		schemaInfo = new SchemaInfo();
		checkDatatype = false;
		processContents = XmlSchemaContentProcessing.Strict;
		Push(XmlQualifiedName.Empty);
		NsXmlNs = base.NameTable.Add("http://www.w3.org/2000/xmlns/");
		NsXs = base.NameTable.Add("http://www.w3.org/2001/XMLSchema");
		NsXsi = base.NameTable.Add("http://www.w3.org/2001/XMLSchema-instance");
		XsiType = base.NameTable.Add("type");
		XsiNil = base.NameTable.Add("nil");
		XsiSchemaLocation = base.NameTable.Add("schemaLocation");
		XsiNoNamespaceSchemaLocation = base.NameTable.Add("noNamespaceSchemaLocation");
		XsdSchema = base.NameTable.Add("schema");
	}

	public override void Validate()
	{
		if (IsInlineSchemaStarted)
		{
			ProcessInlineSchema();
			return;
		}
		switch (reader.NodeType)
		{
		case XmlNodeType.Element:
			ValidateElement();
			if (reader.IsEmptyElement)
			{
				goto case XmlNodeType.EndElement;
			}
			break;
		case XmlNodeType.Whitespace:
			ValidateWhitespace();
			break;
		case XmlNodeType.Text:
		case XmlNodeType.CDATA:
		case XmlNodeType.SignificantWhitespace:
			ValidateText();
			break;
		case XmlNodeType.EndElement:
			ValidateEndElement();
			break;
		}
	}

	public override void CompleteValidation()
	{
		CheckForwardRefs();
	}

	private void ProcessInlineSchema()
	{
		if (inlineSchemaParser.ParseReaderNode())
		{
			return;
		}
		inlineSchemaParser.FinishParsing();
		XmlSchema xmlSchema = inlineSchemaParser.XmlSchema;
		string text = null;
		if (xmlSchema != null && xmlSchema.ErrorCount == 0)
		{
			try
			{
				SchemaInfo schemaInfo = new SchemaInfo();
				schemaInfo.SchemaType = SchemaType.XSD;
				text = ((xmlSchema.TargetNamespace == null) ? string.Empty : xmlSchema.TargetNamespace);
				if (!base.SchemaInfo.TargetNamespaces.ContainsKey(text) && base.SchemaCollection.Add(text, schemaInfo, xmlSchema, compile: true) != null)
				{
					base.SchemaInfo.Add(schemaInfo, base.EventHandler);
				}
			}
			catch (XmlSchemaException ex)
			{
				SendValidationEvent("Cannot load the schema for the namespace '{0}' - {1}", new string[2]
				{
					base.BaseUri.AbsoluteUri,
					ex.Message
				}, XmlSeverityType.Error);
			}
		}
		inlineSchemaParser = null;
	}

	private void ValidateElement()
	{
		elementName.Init(reader.LocalName, reader.NamespaceURI);
		object particle = ValidateChildElement();
		if (IsXSDRoot(elementName.Name, elementName.Namespace) && reader.Depth > 0)
		{
			inlineSchemaParser = new Parser(SchemaType.XSD, base.NameTable, base.SchemaNames, base.EventHandler);
			inlineSchemaParser.StartParsing(reader, null);
			ProcessInlineSchema();
		}
		else
		{
			ProcessElement(particle);
		}
	}

	private object ValidateChildElement()
	{
		object obj = null;
		int errorCode = 0;
		if (context.NeedValidateChildren)
		{
			if (context.IsNill)
			{
				SendValidationEvent("Element '{0}' must have no character or element children.", elementName.ToString());
				return null;
			}
			obj = context.ElementDecl.ContentValidator.ValidateElement(elementName, context, out errorCode);
			if (obj == null)
			{
				processContents = (context.ProcessContents = XmlSchemaContentProcessing.Skip);
				if (errorCode == -2)
				{
					SendValidationEvent("Element '{0}' cannot appear more than once if content model type is \"all\".", elementName.ToString());
				}
				XmlSchemaValidator.ElementValidationError(elementName, context, base.EventHandler, reader, reader.BaseURI, base.PositionInfo.LineNumber, base.PositionInfo.LinePosition, null);
			}
		}
		return obj;
	}

	private void ProcessElement(object particle)
	{
		SchemaElementDecl schemaElementDecl = FastGetElementDecl(particle);
		Push(elementName);
		if (bManageNamespaces)
		{
			nsManager.PushScope();
		}
		ProcessXsiAttributes(out var xsiType, out var xsiNil);
		if (processContents != XmlSchemaContentProcessing.Skip)
		{
			if (schemaElementDecl == null || !xsiType.IsEmpty || xsiNil != null)
			{
				schemaElementDecl = ThoroughGetElementDecl(schemaElementDecl, xsiType, xsiNil);
			}
			if (schemaElementDecl == null)
			{
				if (HasSchema && processContents == XmlSchemaContentProcessing.Strict)
				{
					SendValidationEvent("The '{0}' element is not declared.", XmlSchemaValidator.QNameString(context.LocalName, context.Namespace));
				}
				else
				{
					SendValidationEvent("Could not find schema information for the element '{0}'.", XmlSchemaValidator.QNameString(context.LocalName, context.Namespace), XmlSeverityType.Warning);
				}
			}
		}
		context.ElementDecl = schemaElementDecl;
		ValidateStartElementIdentityConstraints();
		ValidateStartElement();
		if (context.ElementDecl != null)
		{
			ValidateEndStartElement();
			context.NeedValidateChildren = processContents != XmlSchemaContentProcessing.Skip;
			context.ElementDecl.ContentValidator.InitValidation(context);
		}
	}

	private void ProcessXsiAttributes(out XmlQualifiedName xsiType, out string xsiNil)
	{
		string[] array = null;
		string text = null;
		xsiType = XmlQualifiedName.Empty;
		xsiNil = null;
		if (reader.Depth == 0)
		{
			LoadSchema(string.Empty, null);
			foreach (string value in nsManager.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml).Values)
			{
				LoadSchema(value, null);
			}
		}
		if (reader.MoveToFirstAttribute())
		{
			do
			{
				string namespaceURI = reader.NamespaceURI;
				string localName = reader.LocalName;
				if (Ref.Equal(namespaceURI, NsXmlNs))
				{
					LoadSchema(reader.Value, null);
					if (bManageNamespaces)
					{
						nsManager.AddNamespace((reader.Prefix.Length == 0) ? string.Empty : reader.LocalName, reader.Value);
					}
				}
				else if (Ref.Equal(namespaceURI, NsXsi))
				{
					if (Ref.Equal(localName, XsiSchemaLocation))
					{
						array = (string[])dtStringArray.ParseValue(reader.Value, base.NameTable, nsManager);
					}
					else if (Ref.Equal(localName, XsiNoNamespaceSchemaLocation))
					{
						text = reader.Value;
					}
					else if (Ref.Equal(localName, XsiType))
					{
						xsiType = (XmlQualifiedName)dtQName.ParseValue(reader.Value, base.NameTable, nsManager);
					}
					else if (Ref.Equal(localName, XsiNil))
					{
						xsiNil = reader.Value;
					}
				}
			}
			while (reader.MoveToNextAttribute());
			reader.MoveToElement();
		}
		if (text != null)
		{
			LoadSchema(string.Empty, text);
		}
		if (array != null)
		{
			for (int i = 0; i < array.Length - 1; i += 2)
			{
				LoadSchema(array[i], array[i + 1]);
			}
		}
	}

	private void ValidateEndElement()
	{
		if (bManageNamespaces)
		{
			nsManager.PopScope();
		}
		if (context.ElementDecl != null)
		{
			if (!context.IsNill)
			{
				if (context.NeedValidateChildren && !context.ElementDecl.ContentValidator.CompleteValidation(context))
				{
					XmlSchemaValidator.CompleteValidationError(context, base.EventHandler, reader, reader.BaseURI, base.PositionInfo.LineNumber, base.PositionInfo.LinePosition, null);
				}
				if (checkDatatype && !context.IsNill)
				{
					string text = ((!hasSibling) ? textString : textValue.ToString());
					if (text.Length != 0 || context.ElementDecl.DefaultValueTyped == null)
					{
						CheckValue(text, null);
						checkDatatype = false;
					}
				}
			}
			if (HasIdentityConstraints)
			{
				EndElementIdentityConstraints();
			}
		}
		Pop();
	}

	private SchemaElementDecl FastGetElementDecl(object particle)
	{
		SchemaElementDecl result = null;
		if (particle != null)
		{
			if (particle is XmlSchemaElement xmlSchemaElement)
			{
				result = xmlSchemaElement.ElementDecl;
			}
			else
			{
				XmlSchemaAny xmlSchemaAny = (XmlSchemaAny)particle;
				processContents = xmlSchemaAny.ProcessContentsCorrect;
			}
		}
		return result;
	}

	private SchemaElementDecl ThoroughGetElementDecl(SchemaElementDecl elementDecl, XmlQualifiedName xsiType, string xsiNil)
	{
		if (elementDecl == null)
		{
			elementDecl = schemaInfo.GetElementDecl(elementName);
		}
		if (elementDecl != null)
		{
			if (xsiType.IsEmpty)
			{
				if (elementDecl.IsAbstract)
				{
					SendValidationEvent("The element '{0}' is abstract or its type is abstract.", XmlSchemaValidator.QNameString(context.LocalName, context.Namespace));
					elementDecl = null;
				}
			}
			else if (xsiNil != null && xsiNil.Equals("true"))
			{
				SendValidationEvent("There can be no type value when attribute is 'xsi:nil' and has value 'true'.");
			}
			else
			{
				if (!schemaInfo.ElementDeclsByType.TryGetValue(xsiType, out var value) && xsiType.Namespace == NsXs)
				{
					XmlSchemaSimpleType simpleTypeFromXsdType = DatatypeImplementation.GetSimpleTypeFromXsdType(new XmlQualifiedName(xsiType.Name, NsXs));
					if (simpleTypeFromXsdType != null)
					{
						value = simpleTypeFromXsdType.ElementDecl;
					}
				}
				if (value == null)
				{
					SendValidationEvent("This is an invalid xsi:type '{0}'.", xsiType.ToString());
					elementDecl = null;
				}
				else if (!XmlSchemaType.IsDerivedFrom(value.SchemaType, elementDecl.SchemaType, elementDecl.Block))
				{
					SendValidationEvent("The xsi:type attribute value '{0}' is not valid for the element '{1}', either because it is not a type validly derived from the type in the schema, or because it has xsi:type derivation blocked.", new string[2]
					{
						xsiType.ToString(),
						XmlSchemaValidator.QNameString(context.LocalName, context.Namespace)
					});
					elementDecl = null;
				}
				else
				{
					elementDecl = value;
				}
			}
			if (elementDecl != null && elementDecl.IsNillable)
			{
				if (xsiNil != null)
				{
					context.IsNill = XmlConvert.ToBoolean(xsiNil);
					if (context.IsNill && elementDecl.DefaultValueTyped != null)
					{
						SendValidationEvent("There must be no fixed value when an attribute is 'xsi:nil' and has a value of 'true'.");
					}
				}
			}
			else if (xsiNil != null)
			{
				SendValidationEvent("If the 'nillable' attribute is false in the schema, the 'xsi:nil' attribute must not be present in the instance.");
			}
		}
		return elementDecl;
	}

	private void ValidateStartElement()
	{
		if (context.ElementDecl != null)
		{
			if (context.ElementDecl.IsAbstract)
			{
				SendValidationEvent("The element '{0}' is abstract or its type is abstract.", XmlSchemaValidator.QNameString(context.LocalName, context.Namespace));
			}
			reader.SchemaTypeObject = context.ElementDecl.SchemaType;
			if (reader.IsEmptyElement && !context.IsNill && context.ElementDecl.DefaultValueTyped != null)
			{
				reader.TypedValueObject = UnWrapUnion(context.ElementDecl.DefaultValueTyped);
				context.IsNill = true;
			}
			else
			{
				reader.TypedValueObject = null;
			}
			if (context.ElementDecl.HasRequiredAttribute || HasIdentityConstraints)
			{
				attPresence.Clear();
			}
		}
		if (!reader.MoveToFirstAttribute())
		{
			return;
		}
		do
		{
			if ((object)reader.NamespaceURI == NsXmlNs || (object)reader.NamespaceURI == NsXsi)
			{
				continue;
			}
			try
			{
				reader.SchemaTypeObject = null;
				XmlQualifiedName xmlQualifiedName = new XmlQualifiedName(reader.LocalName, reader.NamespaceURI);
				bool skip = processContents == XmlSchemaContentProcessing.Skip;
				SchemaAttDef attributeXsd = schemaInfo.GetAttributeXsd(context.ElementDecl, xmlQualifiedName, ref skip);
				if (attributeXsd != null)
				{
					if (context.ElementDecl != null && (context.ElementDecl.HasRequiredAttribute || startIDConstraint != -1))
					{
						attPresence.Add(attributeXsd.Name, attributeXsd);
					}
					reader.SchemaTypeObject = attributeXsd.SchemaType;
					if (attributeXsd.Datatype != null)
					{
						CheckValue(reader.Value, attributeXsd);
					}
					if (HasIdentityConstraints)
					{
						AttributeIdentityConstraints(reader.LocalName, reader.NamespaceURI, reader.TypedValueObject, reader.Value, attributeXsd);
					}
				}
				else if (!skip)
				{
					if (context.ElementDecl == null && processContents == XmlSchemaContentProcessing.Strict && xmlQualifiedName.Namespace.Length != 0 && schemaInfo.Contains(xmlQualifiedName.Namespace))
					{
						SendValidationEvent("The '{0}' attribute is not declared.", xmlQualifiedName.ToString());
					}
					else
					{
						SendValidationEvent("Could not find schema information for the attribute '{0}'.", xmlQualifiedName.ToString(), XmlSeverityType.Warning);
					}
				}
			}
			catch (XmlSchemaException ex)
			{
				ex.SetSource(reader.BaseURI, base.PositionInfo.LineNumber, base.PositionInfo.LinePosition);
				SendValidationEvent(ex);
			}
		}
		while (reader.MoveToNextAttribute());
		reader.MoveToElement();
	}

	private void ValidateEndStartElement()
	{
		if (context.ElementDecl.HasDefaultAttribute)
		{
			for (int i = 0; i < context.ElementDecl.DefaultAttDefs.Count; i++)
			{
				SchemaAttDef schemaAttDef = (SchemaAttDef)context.ElementDecl.DefaultAttDefs[i];
				reader.AddDefaultAttribute(schemaAttDef);
				if (HasIdentityConstraints && !attPresence.Contains(schemaAttDef.Name))
				{
					AttributeIdentityConstraints(schemaAttDef.Name.Name, schemaAttDef.Name.Namespace, UnWrapUnion(schemaAttDef.DefaultValueTyped), schemaAttDef.DefaultValueRaw, schemaAttDef);
				}
			}
		}
		if (context.ElementDecl.HasRequiredAttribute)
		{
			try
			{
				context.ElementDecl.CheckAttributes(attPresence, reader.StandAlone);
			}
			catch (XmlSchemaException ex)
			{
				ex.SetSource(reader.BaseURI, base.PositionInfo.LineNumber, base.PositionInfo.LinePosition);
				SendValidationEvent(ex);
			}
		}
		if (context.ElementDecl.Datatype != null)
		{
			checkDatatype = true;
			hasSibling = false;
			textString = string.Empty;
			textValue.Length = 0;
		}
	}

	private void LoadSchemaFromLocation(string uri, string url)
	{
		XmlReader xmlReader = null;
		SchemaInfo schemaInfo = null;
		try
		{
			Uri uri2 = base.XmlResolver.ResolveUri(base.BaseUri, url);
			Stream input = (Stream)base.XmlResolver.GetEntity(uri2, null, null);
			xmlReader = new XmlTextReader(uri2.ToString(), input, base.NameTable);
			Parser parser = new Parser(SchemaType.XSD, base.NameTable, base.SchemaNames, base.EventHandler);
			parser.XmlResolver = base.XmlResolver;
			SchemaType schemaType = parser.Parse(xmlReader, uri);
			schemaInfo = new SchemaInfo();
			schemaInfo.SchemaType = schemaType;
			if (schemaType == SchemaType.XSD)
			{
				if (base.SchemaCollection.EventHandler == null)
				{
					base.SchemaCollection.EventHandler = base.EventHandler;
				}
				base.SchemaCollection.Add(uri, schemaInfo, parser.XmlSchema, compile: true);
			}
			base.SchemaInfo.Add(schemaInfo, base.EventHandler);
			while (xmlReader.Read())
			{
			}
		}
		catch (XmlSchemaException ex)
		{
			schemaInfo = null;
			SendValidationEvent("Cannot load the schema for the namespace '{0}' - {1}", new string[2] { uri, ex.Message }, XmlSeverityType.Error);
		}
		catch (Exception ex2)
		{
			schemaInfo = null;
			SendValidationEvent("Cannot load the schema for the namespace '{0}' - {1}", new string[2] { uri, ex2.Message }, XmlSeverityType.Warning);
		}
		finally
		{
			xmlReader?.Close();
		}
	}

	private void LoadSchema(string uri, string url)
	{
		if (base.XmlResolver == null || (base.SchemaInfo.TargetNamespaces.ContainsKey(uri) && nsManager.LookupPrefix(uri) != null))
		{
			return;
		}
		SchemaInfo schemaInfo = null;
		if (base.SchemaCollection != null)
		{
			schemaInfo = base.SchemaCollection.GetSchemaInfo(uri);
		}
		if (schemaInfo != null)
		{
			if (schemaInfo.SchemaType != SchemaType.XSD)
			{
				throw new XmlException("Unsupported combination of validation types.", string.Empty, base.PositionInfo.LineNumber, base.PositionInfo.LinePosition);
			}
			base.SchemaInfo.Add(schemaInfo, base.EventHandler);
		}
		else if (url != null)
		{
			LoadSchemaFromLocation(uri, url);
		}
	}

	private void ProcessTokenizedType(XmlTokenizedType ttype, string name)
	{
		switch (ttype)
		{
		case XmlTokenizedType.ID:
			if (FindId(name) != null)
			{
				SendValidationEvent("'{0}' is already used as an ID.", name);
			}
			else
			{
				AddID(name, context.LocalName);
			}
			break;
		case XmlTokenizedType.IDREF:
			if (FindId(name) == null)
			{
				idRefListHead = new IdRefNode(idRefListHead, name, base.PositionInfo.LineNumber, base.PositionInfo.LinePosition);
			}
			break;
		case XmlTokenizedType.ENTITY:
			BaseValidator.ProcessEntity(schemaInfo, name, this, base.EventHandler, reader.BaseURI, base.PositionInfo.LineNumber, base.PositionInfo.LinePosition);
			break;
		case XmlTokenizedType.IDREFS:
			break;
		}
	}

	private void CheckValue(string value, SchemaAttDef attdef)
	{
		try
		{
			reader.TypedValueObject = null;
			bool flag = attdef != null;
			XmlSchemaDatatype xmlSchemaDatatype = (flag ? attdef.Datatype : context.ElementDecl.Datatype);
			if (xmlSchemaDatatype == null)
			{
				return;
			}
			object obj = xmlSchemaDatatype.ParseValue(value, base.NameTable, nsManager, createAtomicValue: true);
			XmlTokenizedType tokenizedType = xmlSchemaDatatype.TokenizedType;
			if (tokenizedType == XmlTokenizedType.ENTITY || tokenizedType == XmlTokenizedType.ID || tokenizedType == XmlTokenizedType.IDREF)
			{
				if (xmlSchemaDatatype.Variety == XmlSchemaDatatypeVariety.List)
				{
					string[] array = (string[])obj;
					for (int i = 0; i < array.Length; i++)
					{
						ProcessTokenizedType(xmlSchemaDatatype.TokenizedType, array[i]);
					}
				}
				else
				{
					ProcessTokenizedType(xmlSchemaDatatype.TokenizedType, (string)obj);
				}
			}
			if (!(flag ? ((SchemaDeclBase)attdef) : ((SchemaDeclBase)context.ElementDecl)).CheckValue(obj))
			{
				if (flag)
				{
					SendValidationEvent("The value of the '{0}' attribute does not equal its fixed value.", attdef.Name.ToString());
				}
				else
				{
					SendValidationEvent("The value of the '{0}' element does not equal its fixed value.", XmlSchemaValidator.QNameString(context.LocalName, context.Namespace));
				}
			}
			if (xmlSchemaDatatype.Variety == XmlSchemaDatatypeVariety.Union)
			{
				obj = UnWrapUnion(obj);
			}
			reader.TypedValueObject = obj;
		}
		catch (XmlSchemaException)
		{
			if (attdef != null)
			{
				SendValidationEvent("The '{0}' attribute has an invalid value according to its data type.", attdef.Name.ToString());
			}
			else
			{
				SendValidationEvent("The '{0}' element has an invalid value according to its data type.", XmlSchemaValidator.QNameString(context.LocalName, context.Namespace));
			}
		}
	}

	internal void AddID(string name, object node)
	{
		if (IDs == null)
		{
			IDs = new Hashtable();
		}
		IDs.Add(name, node);
	}

	public override object FindId(string name)
	{
		if (IDs != null)
		{
			return IDs[name];
		}
		return null;
	}

	public bool IsXSDRoot(string localName, string ns)
	{
		if (Ref.Equal(ns, NsXs))
		{
			return Ref.Equal(localName, XsdSchema);
		}
		return false;
	}

	private void Push(XmlQualifiedName elementName)
	{
		context = (ValidationState)validationStack.Push();
		if (context == null)
		{
			context = new ValidationState();
			validationStack.AddToTop(context);
		}
		context.LocalName = elementName.Name;
		context.Namespace = elementName.Namespace;
		context.HasMatched = false;
		context.IsNill = false;
		context.ProcessContents = processContents;
		context.NeedValidateChildren = false;
		context.Constr = null;
	}

	private void Pop()
	{
		if (validationStack.Length > 1)
		{
			validationStack.Pop();
			if (startIDConstraint == validationStack.Length)
			{
				startIDConstraint = -1;
			}
			context = (ValidationState)validationStack.Peek();
			processContents = context.ProcessContents;
		}
	}

	private void CheckForwardRefs()
	{
		IdRefNode idRefNode = idRefListHead;
		while (idRefNode != null)
		{
			if (FindId(idRefNode.Id) == null)
			{
				SendValidationEvent(new XmlSchemaException("Reference to undeclared ID is '{0}'.", idRefNode.Id, reader.BaseURI, idRefNode.LineNo, idRefNode.LinePos));
			}
			IdRefNode next = idRefNode.Next;
			idRefNode.Next = null;
			idRefNode = next;
		}
		idRefListHead = null;
	}

	private void ValidateStartElementIdentityConstraints()
	{
		if (context.ElementDecl != null)
		{
			if (context.ElementDecl.Constraints != null)
			{
				AddIdentityConstraints();
			}
			if (HasIdentityConstraints)
			{
				ElementIdentityConstraints();
			}
		}
	}

	private void AddIdentityConstraints()
	{
		context.Constr = new ConstraintStruct[context.ElementDecl.Constraints.Length];
		int num = 0;
		for (int i = 0; i < context.ElementDecl.Constraints.Length; i++)
		{
			context.Constr[num++] = new ConstraintStruct(context.ElementDecl.Constraints[i]);
		}
		for (int j = 0; j < context.Constr.Length; j++)
		{
			if (context.Constr[j].constraint.Role != CompiledIdentityConstraint.ConstraintRole.Keyref)
			{
				continue;
			}
			bool flag = false;
			for (int num2 = validationStack.Length - 1; num2 >= ((startIDConstraint >= 0) ? startIDConstraint : (validationStack.Length - 1)); num2--)
			{
				if (((ValidationState)validationStack[num2]).Constr != null)
				{
					ConstraintStruct[] constr = ((ValidationState)validationStack[num2]).Constr;
					for (int k = 0; k < constr.Length; k++)
					{
						if (constr[k].constraint.name == context.Constr[j].constraint.refer)
						{
							flag = true;
							if (constr[k].keyrefTable == null)
							{
								constr[k].keyrefTable = new Hashtable();
							}
							context.Constr[j].qualifiedTable = constr[k].keyrefTable;
							break;
						}
					}
					if (flag)
					{
						break;
					}
				}
			}
			if (!flag)
			{
				SendValidationEvent("The Keyref '{0}' cannot find the referred key or unique in scope.", XmlSchemaValidator.QNameString(context.LocalName, context.Namespace));
			}
		}
		if (startIDConstraint == -1)
		{
			startIDConstraint = validationStack.Length - 1;
		}
	}

	private void ElementIdentityConstraints()
	{
		for (int i = startIDConstraint; i < validationStack.Length; i++)
		{
			if (((ValidationState)validationStack[i]).Constr == null)
			{
				continue;
			}
			ConstraintStruct[] constr = ((ValidationState)validationStack[i]).Constr;
			for (int j = 0; j < constr.Length; j++)
			{
				if (constr[j].axisSelector.MoveToStartElement(reader.LocalName, reader.NamespaceURI))
				{
					constr[j].axisSelector.PushKS(base.PositionInfo.LineNumber, base.PositionInfo.LinePosition);
				}
				for (int k = 0; k < constr[j].axisFields.Count; k++)
				{
					LocatedActiveAxis locatedActiveAxis = (LocatedActiveAxis)constr[j].axisFields[k];
					if (locatedActiveAxis.MoveToStartElement(reader.LocalName, reader.NamespaceURI) && context.ElementDecl != null)
					{
						if (context.ElementDecl.Datatype == null)
						{
							SendValidationEvent("The field '{0}' is expecting an element or attribute with simple type or simple content.", reader.LocalName);
						}
						else
						{
							locatedActiveAxis.isMatched = true;
						}
					}
				}
			}
		}
	}

	private void AttributeIdentityConstraints(string name, string ns, object obj, string sobj, SchemaAttDef attdef)
	{
		for (int i = startIDConstraint; i < validationStack.Length; i++)
		{
			if (((ValidationState)validationStack[i]).Constr == null)
			{
				continue;
			}
			ConstraintStruct[] constr = ((ValidationState)validationStack[i]).Constr;
			for (int j = 0; j < constr.Length; j++)
			{
				for (int k = 0; k < constr[j].axisFields.Count; k++)
				{
					LocatedActiveAxis locatedActiveAxis = (LocatedActiveAxis)constr[j].axisFields[k];
					if (locatedActiveAxis.MoveToAttribute(name, ns))
					{
						if (locatedActiveAxis.Ks[locatedActiveAxis.Column] != null)
						{
							SendValidationEvent("The field '{0}' is expecting at the most one value.", name);
						}
						else if (attdef != null && attdef.Datatype != null)
						{
							locatedActiveAxis.Ks[locatedActiveAxis.Column] = new TypedObject(obj, sobj, attdef.Datatype);
						}
					}
				}
			}
		}
	}

	private object UnWrapUnion(object typedValue)
	{
		if (typedValue is XsdSimpleValue xsdSimpleValue)
		{
			typedValue = xsdSimpleValue.TypedValue;
		}
		return typedValue;
	}

	private void EndElementIdentityConstraints()
	{
		for (int num = validationStack.Length - 1; num >= startIDConstraint; num--)
		{
			if (((ValidationState)validationStack[num]).Constr != null)
			{
				ConstraintStruct[] constr = ((ValidationState)validationStack[num]).Constr;
				for (int i = 0; i < constr.Length; i++)
				{
					for (int j = 0; j < constr[i].axisFields.Count; j++)
					{
						LocatedActiveAxis locatedActiveAxis = (LocatedActiveAxis)constr[i].axisFields[j];
						if (locatedActiveAxis.isMatched)
						{
							locatedActiveAxis.isMatched = false;
							if (locatedActiveAxis.Ks[locatedActiveAxis.Column] != null)
							{
								SendValidationEvent("The field '{0}' is expecting at the most one value.", reader.LocalName);
							}
							else
							{
								string text = ((!hasSibling) ? textString : textValue.ToString());
								if (reader.TypedValueObject != null && text.Length != 0)
								{
									locatedActiveAxis.Ks[locatedActiveAxis.Column] = new TypedObject(reader.TypedValueObject, text, context.ElementDecl.Datatype);
								}
							}
						}
						locatedActiveAxis.EndElement(reader.LocalName, reader.NamespaceURI);
					}
					if (!constr[i].axisSelector.EndElement(reader.LocalName, reader.NamespaceURI))
					{
						continue;
					}
					KeySequence keySequence = constr[i].axisSelector.PopKS();
					switch (constr[i].constraint.Role)
					{
					case CompiledIdentityConstraint.ConstraintRole.Key:
						if (!keySequence.IsQualified())
						{
							SendValidationEvent(new XmlSchemaException("The identity constraint '{0}' validation has failed. Either a key is missing or the existing key has an empty node.", constr[i].constraint.name.ToString(), reader.BaseURI, keySequence.PosLine, keySequence.PosCol));
						}
						else if (constr[i].qualifiedTable.Contains(keySequence))
						{
							SendValidationEvent(new XmlSchemaException("There is a duplicate key sequence '{0}' for the '{1}' key or unique identity constraint.", new string[2]
							{
								keySequence.ToString(),
								constr[i].constraint.name.ToString()
							}, reader.BaseURI, keySequence.PosLine, keySequence.PosCol));
						}
						else
						{
							constr[i].qualifiedTable.Add(keySequence, keySequence);
						}
						break;
					case CompiledIdentityConstraint.ConstraintRole.Unique:
						if (keySequence.IsQualified())
						{
							if (constr[i].qualifiedTable.Contains(keySequence))
							{
								SendValidationEvent(new XmlSchemaException("There is a duplicate key sequence '{0}' for the '{1}' key or unique identity constraint.", new string[2]
								{
									keySequence.ToString(),
									constr[i].constraint.name.ToString()
								}, reader.BaseURI, keySequence.PosLine, keySequence.PosCol));
							}
							else
							{
								constr[i].qualifiedTable.Add(keySequence, keySequence);
							}
						}
						break;
					case CompiledIdentityConstraint.ConstraintRole.Keyref:
						if (constr[i].qualifiedTable != null && keySequence.IsQualified() && !constr[i].qualifiedTable.Contains(keySequence))
						{
							constr[i].qualifiedTable.Add(keySequence, keySequence);
						}
						break;
					}
				}
			}
		}
		ConstraintStruct[] constr2 = ((ValidationState)validationStack[validationStack.Length - 1]).Constr;
		if (constr2 == null)
		{
			return;
		}
		for (int k = 0; k < constr2.Length; k++)
		{
			if (constr2[k].constraint.Role == CompiledIdentityConstraint.ConstraintRole.Keyref || constr2[k].keyrefTable == null)
			{
				continue;
			}
			foreach (KeySequence key in constr2[k].keyrefTable.Keys)
			{
				if (!constr2[k].qualifiedTable.Contains(key))
				{
					SendValidationEvent(new XmlSchemaException("The key sequence '{0}' in '{1}' Keyref fails to refer to some key.", new string[2]
					{
						key.ToString(),
						constr2[k].constraint.name.ToString()
					}, reader.BaseURI, key.PosLine, key.PosCol));
				}
			}
		}
	}
}
