using System.Collections;
using System.IO;
using System.Text;

namespace System.Xml.Schema;

internal sealed class XdrValidator : BaseValidator
{
	private const int STACK_INCREMENT = 10;

	private HWStack validationStack;

	private Hashtable attPresence;

	private XmlQualifiedName name = XmlQualifiedName.Empty;

	private XmlNamespaceManager nsManager;

	private bool isProcessContents;

	private Hashtable IDs;

	private IdRefNode idRefListHead;

	private Parser inlineSchemaParser;

	private const string x_schema = "x-schema:";

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

	internal XdrValidator(BaseValidator validator)
		: base(validator)
	{
		Init();
	}

	internal XdrValidator(XmlValidatingReaderImpl reader, XmlSchemaCollection schemaCollection, IValidationEventHandling eventHandling)
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
			isProcessContents = true;
		}
		validationStack = new HWStack(10);
		textValue = new StringBuilder();
		name = XmlQualifiedName.Empty;
		attPresence = new Hashtable();
		Push(XmlQualifiedName.Empty);
		schemaInfo = new SchemaInfo();
		checkDatatype = false;
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

	private void ValidateElement()
	{
		elementName.Init(reader.LocalName, XmlSchemaDatatype.XdrCanonizeUri(reader.NamespaceURI, base.NameTable, base.SchemaNames));
		ValidateChildElement();
		if (base.SchemaNames.IsXDRRoot(elementName.Name, elementName.Namespace) && reader.Depth > 0)
		{
			inlineSchemaParser = new Parser(SchemaType.XDR, base.NameTable, base.SchemaNames, base.EventHandler);
			inlineSchemaParser.StartParsing(reader, null);
			inlineSchemaParser.ParseReaderNode();
		}
		else
		{
			ProcessElement();
		}
	}

	private void ValidateChildElement()
	{
		if (context.NeedValidateChildren)
		{
			int errorCode = 0;
			context.ElementDecl.ContentValidator.ValidateElement(elementName, context, out errorCode);
			if (errorCode < 0)
			{
				XmlSchemaValidator.ElementValidationError(elementName, context, base.EventHandler, reader, reader.BaseURI, base.PositionInfo.LineNumber, base.PositionInfo.LinePosition, null);
			}
		}
	}

	private void ProcessInlineSchema()
	{
		if (inlineSchemaParser.ParseReaderNode())
		{
			return;
		}
		inlineSchemaParser.FinishParsing();
		SchemaInfo xdrSchema = inlineSchemaParser.XdrSchema;
		if (xdrSchema != null && xdrSchema.ErrorCount == 0)
		{
			foreach (string key in xdrSchema.TargetNamespaces.Keys)
			{
				if (!schemaInfo.HasSchema(key))
				{
					schemaInfo.Add(xdrSchema, base.EventHandler);
					base.SchemaCollection.Add(key, xdrSchema, null, compile: false);
					break;
				}
			}
		}
		inlineSchemaParser = null;
	}

	private void ProcessElement()
	{
		Push(elementName);
		if (isProcessContents)
		{
			nsManager.PopScope();
		}
		context.ElementDecl = ThoroughGetElementDecl();
		if (context.ElementDecl != null)
		{
			ValidateStartElement();
			ValidateEndStartElement();
			context.NeedValidateChildren = true;
			context.ElementDecl.ContentValidator.InitValidation(context);
		}
	}

	private void ValidateEndElement()
	{
		if (isProcessContents)
		{
			nsManager.PopScope();
		}
		if (context.ElementDecl != null)
		{
			if (context.NeedValidateChildren && !context.ElementDecl.ContentValidator.CompleteValidation(context))
			{
				XmlSchemaValidator.CompleteValidationError(context, base.EventHandler, reader, reader.BaseURI, base.PositionInfo.LineNumber, base.PositionInfo.LinePosition, null);
			}
			if (checkDatatype)
			{
				string value = ((!hasSibling) ? textString : textValue.ToString());
				CheckValue(value, null);
				checkDatatype = false;
				textValue.Length = 0;
				textString = string.Empty;
			}
		}
		Pop();
	}

	private SchemaElementDecl ThoroughGetElementDecl()
	{
		if (reader.Depth == 0)
		{
			LoadSchema(string.Empty);
		}
		if (reader.MoveToFirstAttribute())
		{
			do
			{
				string namespaceURI = reader.NamespaceURI;
				string localName = reader.LocalName;
				if (Ref.Equal(namespaceURI, base.SchemaNames.NsXmlNs))
				{
					LoadSchema(reader.Value);
					if (isProcessContents)
					{
						nsManager.AddNamespace((reader.Prefix.Length == 0) ? string.Empty : reader.LocalName, reader.Value);
					}
				}
				if (Ref.Equal(namespaceURI, base.SchemaNames.QnDtDt.Namespace) && Ref.Equal(localName, base.SchemaNames.QnDtDt.Name))
				{
					reader.SchemaTypeObject = XmlSchemaDatatype.FromXdrName(reader.Value);
				}
			}
			while (reader.MoveToNextAttribute());
			reader.MoveToElement();
		}
		SchemaElementDecl elementDecl = schemaInfo.GetElementDecl(elementName);
		if (elementDecl == null && schemaInfo.TargetNamespaces.ContainsKey(context.Namespace))
		{
			SendValidationEvent("The '{0}' element is not declared.", XmlSchemaValidator.QNameString(context.LocalName, context.Namespace));
		}
		return elementDecl;
	}

	private void ValidateStartElement()
	{
		if (context.ElementDecl != null)
		{
			if (context.ElementDecl.SchemaType != null)
			{
				reader.SchemaTypeObject = context.ElementDecl.SchemaType;
			}
			else
			{
				reader.SchemaTypeObject = context.ElementDecl.Datatype;
			}
			if (reader.IsEmptyElement && !context.IsNill && context.ElementDecl.DefaultValueTyped != null)
			{
				reader.TypedValueObject = context.ElementDecl.DefaultValueTyped;
				context.IsNill = true;
			}
			if (context.ElementDecl.HasRequiredAttribute)
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
			if ((object)reader.NamespaceURI == base.SchemaNames.NsXmlNs)
			{
				continue;
			}
			try
			{
				reader.SchemaTypeObject = null;
				SchemaAttDef attributeXdr = schemaInfo.GetAttributeXdr(context.ElementDecl, QualifiedName(reader.LocalName, reader.NamespaceURI));
				if (attributeXdr != null)
				{
					if (context.ElementDecl != null && context.ElementDecl.HasRequiredAttribute)
					{
						attPresence.Add(attributeXdr.Name, attributeXdr);
					}
					reader.SchemaTypeObject = ((attributeXdr.SchemaType != null) ? ((object)attributeXdr.SchemaType) : ((object)attributeXdr.Datatype));
					if (attributeXdr.Datatype != null)
					{
						string value = reader.Value;
						CheckValue(value, attributeXdr);
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
				reader.AddDefaultAttribute((SchemaAttDef)context.ElementDecl.DefaultAttDefs[i]);
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

	private void LoadSchemaFromLocation(string uri)
	{
		if (!XdrBuilder.IsXdrSchema(uri))
		{
			return;
		}
		string relativeUri = uri.Substring("x-schema:".Length);
		XmlReader xmlReader = null;
		SchemaInfo schemaInfo = null;
		try
		{
			Uri uri2 = base.XmlResolver.ResolveUri(base.BaseUri, relativeUri);
			Stream input = (Stream)base.XmlResolver.GetEntity(uri2, null, null);
			xmlReader = new XmlTextReader(uri2.ToString(), input, base.NameTable);
			((XmlTextReader)xmlReader).XmlResolver = base.XmlResolver;
			Parser parser = new Parser(SchemaType.XDR, base.NameTable, base.SchemaNames, base.EventHandler);
			parser.XmlResolver = base.XmlResolver;
			parser.Parse(xmlReader, uri);
			while (xmlReader.Read())
			{
			}
			schemaInfo = parser.XdrSchema;
		}
		catch (XmlSchemaException ex)
		{
			SendValidationEvent("Cannot load the schema for the namespace '{0}' - {1}", new string[2] { uri, ex.Message }, XmlSeverityType.Error);
		}
		catch (Exception ex2)
		{
			SendValidationEvent("Cannot load the schema for the namespace '{0}' - {1}", new string[2] { uri, ex2.Message }, XmlSeverityType.Warning);
		}
		finally
		{
			xmlReader?.Close();
		}
		if (schemaInfo != null && schemaInfo.ErrorCount == 0)
		{
			base.schemaInfo.Add(schemaInfo, base.EventHandler);
			base.SchemaCollection.Add(uri, schemaInfo, null, compile: false);
		}
	}

	private void LoadSchema(string uri)
	{
		if (base.schemaInfo.TargetNamespaces.ContainsKey(uri) || base.XmlResolver == null)
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
			if (schemaInfo.SchemaType != SchemaType.XDR)
			{
				throw new XmlException("Unsupported combination of validation types.", string.Empty, base.PositionInfo.LineNumber, base.PositionInfo.LinePosition);
			}
			base.schemaInfo.Add(schemaInfo, base.EventHandler);
		}
		else
		{
			LoadSchemaFromLocation(uri);
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

	public override void CompleteValidation()
	{
		if (HasSchema)
		{
			CheckForwardRefs();
		}
		else
		{
			SendValidationEvent(new XmlSchemaException("No validation occurred.", string.Empty), XmlSeverityType.Warning);
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
			if (xmlSchemaDatatype.TokenizedType != 0)
			{
				value = value.Trim();
			}
			if (value.Length == 0)
			{
				return;
			}
			object obj = xmlSchemaDatatype.ParseValue(value, base.NameTable, nsManager);
			reader.TypedValueObject = obj;
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
			SchemaDeclBase schemaDeclBase = (flag ? ((SchemaDeclBase)attdef) : ((SchemaDeclBase)context.ElementDecl));
			if (schemaDeclBase.MaxLength != uint.MaxValue && value.Length > schemaDeclBase.MaxLength)
			{
				SendValidationEvent("The actual length is greater than the MaxLength value.", value);
			}
			if (schemaDeclBase.MinLength != uint.MaxValue && value.Length < schemaDeclBase.MinLength)
			{
				SendValidationEvent("The actual length is less than the MinLength value.", value);
			}
			if (schemaDeclBase.Values != null && !schemaDeclBase.CheckEnumeration(obj))
			{
				if (xmlSchemaDatatype.TokenizedType == XmlTokenizedType.NOTATION)
				{
					SendValidationEvent("'{0}' is not in the notation list.", obj.ToString());
				}
				else
				{
					SendValidationEvent("'{0}' is not in the enumeration list.", obj.ToString());
				}
			}
			if (!schemaDeclBase.CheckValue(obj))
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

	public static void CheckDefaultValue(string value, SchemaAttDef attdef, SchemaInfo sinfo, XmlNamespaceManager nsManager, XmlNameTable NameTable, object sender, ValidationEventHandler eventhandler, string baseUri, int lineNo, int linePos)
	{
		try
		{
			XmlSchemaDatatype datatype = attdef.Datatype;
			if (datatype == null)
			{
				return;
			}
			if (datatype.TokenizedType != 0)
			{
				value = value.Trim();
			}
			if (value.Length == 0)
			{
				return;
			}
			object obj = datatype.ParseValue(value, NameTable, nsManager);
			switch (datatype.TokenizedType)
			{
			case XmlTokenizedType.ENTITY:
				if (datatype.Variety == XmlSchemaDatatypeVariety.List)
				{
					string[] array = (string[])obj;
					for (int i = 0; i < array.Length; i++)
					{
						BaseValidator.ProcessEntity(sinfo, array[i], sender, eventhandler, baseUri, lineNo, linePos);
					}
				}
				else
				{
					BaseValidator.ProcessEntity(sinfo, (string)obj, sender, eventhandler, baseUri, lineNo, linePos);
				}
				break;
			case XmlTokenizedType.ENUMERATION:
				if (!attdef.CheckEnumeration(obj))
				{
					XmlSchemaException ex = new XmlSchemaException("'{0}' is not in the enumeration list.", obj.ToString(), baseUri, lineNo, linePos);
					if (eventhandler == null)
					{
						throw ex;
					}
					eventhandler(sender, new ValidationEventArgs(ex));
				}
				break;
			}
			attdef.DefaultValueTyped = obj;
		}
		catch
		{
			XmlSchemaException ex2 = new XmlSchemaException("The default value of '{0}' attribute is invalid according to its datatype.", attdef.Name.ToString(), baseUri, lineNo, linePos);
			if (eventhandler != null)
			{
				eventhandler(sender, new ValidationEventArgs(ex2));
				return;
			}
			throw ex2;
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
		context.NeedValidateChildren = false;
	}

	private void Pop()
	{
		if (validationStack.Length > 1)
		{
			validationStack.Pop();
			context = (ValidationState)validationStack.Peek();
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

	private XmlQualifiedName QualifiedName(string name, string ns)
	{
		return new XmlQualifiedName(name, XmlSchemaDatatype.XdrCanonizeUri(ns, base.NameTable, base.SchemaNames));
	}
}
