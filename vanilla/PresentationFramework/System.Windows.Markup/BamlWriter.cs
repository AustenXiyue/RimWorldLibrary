using System.Collections;
using System.IO;
using System.Reflection;

namespace System.Windows.Markup;

internal class BamlWriter : IParserHelper
{
	private class WriteStackNode
	{
		private bool _endAttributesReached;

		private BamlRecordType _recordType;

		private Type _elementType;

		public BamlRecordType RecordType => _recordType;

		public bool EndAttributesReached
		{
			get
			{
				return _endAttributesReached;
			}
			set
			{
				_endAttributesReached = value;
			}
		}

		public Type ElementType => _elementType;

		public WriteStackNode(BamlRecordType recordType)
		{
			_recordType = recordType;
			_endAttributesReached = false;
		}

		public WriteStackNode(BamlRecordType recordType, Type elementType)
			: this(recordType)
		{
			_elementType = elementType;
		}
	}

	private BamlRecordWriter _bamlRecordWriter;

	private bool _startDocumentWritten;

	private int _depth;

	private bool _closed;

	private DependencyProperty _dpProperty;

	private ParserStack _nodeTypeStack;

	private Hashtable _assemblies;

	private XamlTypeMapper _xamlTypeMapper;

	private ParserContext _parserContext;

	private MarkupExtensionParser _extensionParser;

	private ArrayList _markupExtensionNodes;

	public BamlWriter(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (!stream.CanWrite)
		{
			throw new ArgumentException(SR.BamlWriterBadStream);
		}
		_parserContext = new ParserContext();
		if (_parserContext.XamlTypeMapper == null)
		{
			_parserContext.XamlTypeMapper = new BamlWriterXamlTypeMapper(XmlParserDefaults.GetDefaultAssemblyNames(), XmlParserDefaults.GetDefaultNamespaceMaps());
		}
		_xamlTypeMapper = _parserContext.XamlTypeMapper;
		_bamlRecordWriter = new BamlRecordWriter(stream, _parserContext, deferLoadingSupport: true);
		_startDocumentWritten = false;
		_depth = 0;
		_closed = false;
		_nodeTypeStack = new ParserStack();
		_assemblies = new Hashtable(7);
		_extensionParser = new MarkupExtensionParser(this, _parserContext);
		_markupExtensionNodes = new ArrayList();
	}

	public void Close()
	{
		_bamlRecordWriter.BamlStream.Close();
		_closed = true;
	}

	string IParserHelper.LookupNamespace(string prefix)
	{
		return _parserContext.XmlnsDictionary[prefix];
	}

	bool IParserHelper.GetElementType(bool extensionFirst, string localName, string namespaceURI, ref string assemblyName, ref string typeFullName, ref Type baseType, ref Type serializerType)
	{
		bool result = false;
		assemblyName = string.Empty;
		typeFullName = string.Empty;
		serializerType = null;
		baseType = null;
		if (namespaceURI == null || localName == null)
		{
			return false;
		}
		TypeAndSerializer typeAndSerializer = _xamlTypeMapper.GetTypeAndSerializer(namespaceURI, localName, null);
		if (typeAndSerializer == null)
		{
			typeAndSerializer = _xamlTypeMapper.GetTypeAndSerializer(namespaceURI, localName + "Extension", null);
		}
		if (typeAndSerializer != null && typeAndSerializer.ObjectType != null)
		{
			serializerType = typeAndSerializer.SerializerType;
			baseType = typeAndSerializer.ObjectType;
			typeFullName = baseType.FullName;
			assemblyName = baseType.Assembly.FullName;
			result = true;
		}
		return result;
	}

	bool IParserHelper.CanResolveLocalAssemblies()
	{
		return false;
	}

	public void WriteStartDocument()
	{
		if (_closed)
		{
			throw new InvalidOperationException(SR.BamlWriterClosed);
		}
		if (_startDocumentWritten)
		{
			throw new InvalidOperationException(SR.BamlWriterStartDoc);
		}
		XamlDocumentStartNode xamlDocumentNode = new XamlDocumentStartNode(0, 0, _depth);
		_bamlRecordWriter.WriteDocumentStart(xamlDocumentNode);
		_startDocumentWritten = true;
		Push(BamlRecordType.DocumentStart);
	}

	public void WriteEndDocument()
	{
		VerifyEndTagState(BamlRecordType.DocumentStart, BamlRecordType.DocumentEnd);
		XamlDocumentEndNode xamlDocumentEndNode = new XamlDocumentEndNode(0, 0, _depth);
		_bamlRecordWriter.WriteDocumentEnd(xamlDocumentEndNode);
	}

	public void WriteConnectionId(int connectionId)
	{
		VerifyWriteState();
		_bamlRecordWriter.WriteConnectionId(connectionId);
	}

	public void WriteStartElement(string assemblyName, string typeFullName, bool isInjected, bool useTypeConverter)
	{
		VerifyWriteState();
		_dpProperty = null;
		_parserContext.PushScope();
		ProcessMarkupExtensionNodes();
		Type type = GetType(assemblyName, typeFullName);
		Type xamlSerializerForType = _xamlTypeMapper.GetXamlSerializerForType(type);
		Push(BamlRecordType.ElementStart, type);
		XamlElementStartNode xamlElementStartNode = new XamlElementStartNode(0, 0, _depth++, assemblyName, typeFullName, type, xamlSerializerForType);
		xamlElementStartNode.IsInjected = isInjected;
		xamlElementStartNode.CreateUsingTypeConverter = useTypeConverter;
		_bamlRecordWriter.WriteElementStart(xamlElementStartNode);
	}

	public void WriteEndElement()
	{
		VerifyEndTagState(BamlRecordType.ElementStart, BamlRecordType.ElementEnd);
		ProcessMarkupExtensionNodes();
		XamlElementEndNode xamlElementEndNode = new XamlElementEndNode(0, 0, --_depth);
		_bamlRecordWriter.WriteElementEnd(xamlElementEndNode);
		_parserContext.PopScope();
	}

	public void WriteStartConstructor()
	{
		VerifyWriteState();
		Push(BamlRecordType.ConstructorParametersStart);
		XamlConstructorParametersStartNode xamlConstructorParametersStartNode = new XamlConstructorParametersStartNode(0, 0, --_depth);
		_bamlRecordWriter.WriteConstructorParametersStart(xamlConstructorParametersStartNode);
	}

	public void WriteEndConstructor()
	{
		VerifyEndTagState(BamlRecordType.ConstructorParametersStart, BamlRecordType.ConstructorParametersEnd);
		XamlConstructorParametersEndNode xamlConstructorParametersEndNode = new XamlConstructorParametersEndNode(0, 0, --_depth);
		_bamlRecordWriter.WriteConstructorParametersEnd(xamlConstructorParametersEndNode);
	}

	public void WriteProperty(string assemblyName, string ownerTypeFullName, string propName, string value, BamlAttributeUsage propUsage)
	{
		VerifyWriteState();
		BamlRecordType bamlRecordType = PeekRecordType();
		if (bamlRecordType != BamlRecordType.ElementStart)
		{
			throw new InvalidOperationException(SR.Format(SR.BamlWriterNoInElement, "WriteProperty", bamlRecordType.ToString()));
		}
		GetDpOrPi(assemblyName, ownerTypeFullName, propName, out var dpOrPi, out var ownerType);
		AttributeData attributeData = _extensionParser.IsMarkupExtensionAttribute(ownerType, propName, ref value, 0, 0, 0, dpOrPi);
		if (attributeData == null)
		{
			XamlPropertyNode xamlPropertyNode = new XamlPropertyNode(0, 0, _depth, dpOrPi, assemblyName, ownerTypeFullName, propName, value, propUsage, complexAsSimple: false);
			if (XamlTypeMapper.GetPropertyType(dpOrPi) == typeof(DependencyProperty))
			{
				Type ownerType2 = null;
				_dpProperty = XamlTypeMapper.ParsePropertyName(_parserContext, value, ref ownerType2);
				if (_bamlRecordWriter != null && _dpProperty != null)
				{
					short typeId;
					short attributeOrTypeId = _parserContext.MapTable.GetAttributeOrTypeId(_bamlRecordWriter.BinaryWriter, ownerType2, _dpProperty.Name, out typeId);
					if (attributeOrTypeId < 0)
					{
						xamlPropertyNode.ValueId = attributeOrTypeId;
						xamlPropertyNode.MemberName = null;
					}
					else
					{
						xamlPropertyNode.ValueId = typeId;
						xamlPropertyNode.MemberName = _dpProperty.Name;
					}
				}
			}
			else if (_dpProperty != null)
			{
				xamlPropertyNode.ValuePropertyType = _dpProperty.PropertyType;
				xamlPropertyNode.ValuePropertyMember = _dpProperty;
				xamlPropertyNode.ValuePropertyName = _dpProperty.Name;
				xamlPropertyNode.ValueDeclaringType = _dpProperty.OwnerType;
				_ = _dpProperty.OwnerType.Assembly.FullName;
				_dpProperty = null;
			}
			_bamlRecordWriter.WriteProperty(xamlPropertyNode);
		}
		else if (attributeData.IsSimple)
		{
			if (attributeData.IsTypeExtension)
			{
				Type typeFromBaseString = _xamlTypeMapper.GetTypeFromBaseString(attributeData.Args, _parserContext, throwOnError: true);
				XamlPropertyWithTypeNode xamlPropertyWithType = new XamlPropertyWithTypeNode(0, 0, _depth, dpOrPi, assemblyName, ownerTypeFullName, propName, typeFromBaseString.FullName, typeFromBaseString.Assembly.FullName, typeFromBaseString, string.Empty, string.Empty);
				_bamlRecordWriter.WritePropertyWithType(xamlPropertyWithType);
			}
			else
			{
				XamlPropertyWithExtensionNode xamlPropertyNode2 = new XamlPropertyWithExtensionNode(0, 0, _depth, dpOrPi, assemblyName, ownerTypeFullName, propName, attributeData.Args, attributeData.ExtensionTypeId, attributeData.IsValueNestedExtension, attributeData.IsValueTypeExtension);
				_bamlRecordWriter.WritePropertyWithExtension(xamlPropertyNode2);
			}
		}
		else
		{
			_extensionParser.CompileAttribute(_markupExtensionNodes, attributeData);
		}
	}

	public void WriteContentProperty(string assemblyName, string ownerTypeFullName, string propName)
	{
		GetDpOrPi(assemblyName, ownerTypeFullName, propName, out var dpOrPi, out var _);
		XamlContentPropertyNode xamlContentPropertyNode = new XamlContentPropertyNode(0, 0, _depth, dpOrPi, assemblyName, ownerTypeFullName, propName);
		_bamlRecordWriter.WriteContentProperty(xamlContentPropertyNode);
	}

	public void WriteXmlnsProperty(string localName, string xmlNamespace)
	{
		VerifyWriteState();
		BamlRecordType bamlRecordType = PeekRecordType();
		if (bamlRecordType != BamlRecordType.ElementStart && bamlRecordType != BamlRecordType.PropertyComplexStart && bamlRecordType != BamlRecordType.PropertyArrayStart && bamlRecordType != BamlRecordType.PropertyIListStart && bamlRecordType != BamlRecordType.PropertyIDictionaryStart)
		{
			throw new InvalidOperationException(SR.Format(SR.BamlWriterBadXmlns, "WriteXmlnsProperty", bamlRecordType.ToString()));
		}
		XamlXmlnsPropertyNode xamlXmlnsPropertyNode = new XamlXmlnsPropertyNode(0, 0, _depth, localName, xmlNamespace);
		_bamlRecordWriter.WriteNamespacePrefix(xamlXmlnsPropertyNode);
		_parserContext.XmlnsDictionary[localName] = xmlNamespace;
	}

	public void WriteDefAttribute(string name, string value)
	{
		VerifyWriteState();
		BamlRecordType bamlRecordType = PeekRecordType();
		if (bamlRecordType != BamlRecordType.ElementStart && name != "Uid")
		{
			throw new InvalidOperationException(SR.Format(SR.BamlWriterNoInElement, "WriteDefAttribute", bamlRecordType.ToString()));
		}
		if (name == "Key")
		{
			DefAttributeData defAttributeData = _extensionParser.IsMarkupExtensionDefAttribute(PeekElementType(), ref value, 0, 0, 0);
			if (defAttributeData != null)
			{
				if (name != "Key")
				{
					defAttributeData.IsSimple = false;
				}
				if (defAttributeData.IsSimple)
				{
					int num = defAttributeData.Args.IndexOf(':');
					string prefix = string.Empty;
					string localName = defAttributeData.Args;
					if (num > 0)
					{
						prefix = defAttributeData.Args.Substring(0, num);
						localName = defAttributeData.Args.Substring(num + 1);
					}
					string namespaceURI = _parserContext.XmlnsDictionary[prefix];
					string assemblyName = string.Empty;
					string typeFullName = string.Empty;
					Type baseType = null;
					Type serializerType = null;
					if (((IParserHelper)this).GetElementType(extensionFirst: false, localName, namespaceURI, ref assemblyName, ref typeFullName, ref baseType, ref serializerType))
					{
						XamlDefAttributeKeyTypeNode xamlDefNode = new XamlDefAttributeKeyTypeNode(0, 0, _depth, typeFullName, baseType.Assembly.FullName, baseType);
						_bamlRecordWriter.WriteDefAttributeKeyType(xamlDefNode);
					}
					else
					{
						defAttributeData.IsSimple = false;
						defAttributeData.Args += "}";
					}
				}
				if (!defAttributeData.IsSimple)
				{
					_extensionParser.CompileDictionaryKey(_markupExtensionNodes, defAttributeData);
				}
				return;
			}
		}
		XamlDefAttributeNode xamlDefNode2 = new XamlDefAttributeNode(0, 0, _depth, name, value);
		_bamlRecordWriter.WriteDefAttribute(xamlDefNode2);
	}

	public void WritePresentationOptionsAttribute(string name, string value)
	{
		VerifyWriteState();
		XamlPresentationOptionsAttributeNode xamlPresentationOptionsNode = new XamlPresentationOptionsAttributeNode(0, 0, _depth, name, value);
		_bamlRecordWriter.WritePresentationOptionsAttribute(xamlPresentationOptionsNode);
	}

	public void WriteStartComplexProperty(string assemblyName, string ownerTypeFullName, string propName)
	{
		VerifyWriteState();
		_parserContext.PushScope();
		ProcessMarkupExtensionNodes();
		Type propertyType = null;
		bool propertyCanWrite = true;
		GetDpOrPi(assemblyName, ownerTypeFullName, propName, out var dpOrPi, out var ownerType);
		if (dpOrPi == null)
		{
			MethodInfo mi = GetMi(assemblyName, ownerTypeFullName, propName, out ownerType);
			if (mi != null)
			{
				XamlTypeMapper.GetPropertyType(mi, out propertyType, out propertyCanWrite);
			}
		}
		else
		{
			propertyType = XamlTypeMapper.GetPropertyType(dpOrPi);
			PropertyInfo propertyInfo = dpOrPi as PropertyInfo;
			if (propertyInfo != null)
			{
				propertyCanWrite = propertyInfo.CanWrite;
			}
			else if (dpOrPi is DependencyProperty dependencyProperty)
			{
				propertyCanWrite = !dependencyProperty.ReadOnly;
			}
		}
		if (propertyType == null)
		{
			Push(BamlRecordType.PropertyComplexStart);
			XamlPropertyComplexStartNode xamlComplexPropertyNode = new XamlPropertyComplexStartNode(0, 0, _depth++, null, assemblyName, ownerTypeFullName, propName);
			_bamlRecordWriter.WritePropertyComplexStart(xamlComplexPropertyNode);
			return;
		}
		BamlRecordType propertyStartRecordType = BamlRecordManager.GetPropertyStartRecordType(propertyType, propertyCanWrite);
		Push(propertyStartRecordType);
		switch (propertyStartRecordType)
		{
		case BamlRecordType.PropertyArrayStart:
		{
			XamlPropertyArrayStartNode xamlPropertyArrayStartNode = new XamlPropertyArrayStartNode(0, 0, _depth++, dpOrPi, assemblyName, ownerTypeFullName, propName);
			_bamlRecordWriter.WritePropertyArrayStart(xamlPropertyArrayStartNode);
			break;
		}
		case BamlRecordType.PropertyIDictionaryStart:
		{
			XamlPropertyIDictionaryStartNode xamlPropertyIDictionaryStartNode = new XamlPropertyIDictionaryStartNode(0, 0, _depth++, dpOrPi, assemblyName, ownerTypeFullName, propName);
			_bamlRecordWriter.WritePropertyIDictionaryStart(xamlPropertyIDictionaryStartNode);
			break;
		}
		case BamlRecordType.PropertyIListStart:
		{
			XamlPropertyIListStartNode xamlPropertyIListStart = new XamlPropertyIListStartNode(0, 0, _depth++, dpOrPi, assemblyName, ownerTypeFullName, propName);
			_bamlRecordWriter.WritePropertyIListStart(xamlPropertyIListStart);
			break;
		}
		default:
		{
			XamlPropertyComplexStartNode xamlComplexPropertyNode2 = new XamlPropertyComplexStartNode(0, 0, _depth++, dpOrPi, assemblyName, ownerTypeFullName, propName);
			_bamlRecordWriter.WritePropertyComplexStart(xamlComplexPropertyNode2);
			break;
		}
		}
	}

	public void WriteEndComplexProperty()
	{
		VerifyWriteState();
		BamlRecordType bamlRecordType = Pop();
		switch (bamlRecordType)
		{
		case BamlRecordType.PropertyArrayStart:
		{
			XamlPropertyArrayEndNode xamlPropertyArrayEndNode = new XamlPropertyArrayEndNode(0, 0, --_depth);
			_bamlRecordWriter.WritePropertyArrayEnd(xamlPropertyArrayEndNode);
			break;
		}
		case BamlRecordType.PropertyIListStart:
		{
			XamlPropertyIListEndNode xamlPropertyIListEndNode = new XamlPropertyIListEndNode(0, 0, --_depth);
			_bamlRecordWriter.WritePropertyIListEnd(xamlPropertyIListEndNode);
			break;
		}
		case BamlRecordType.PropertyIDictionaryStart:
		{
			XamlPropertyIDictionaryEndNode xamlPropertyIDictionaryEndNode = new XamlPropertyIDictionaryEndNode(0, 0, --_depth);
			_bamlRecordWriter.WritePropertyIDictionaryEnd(xamlPropertyIDictionaryEndNode);
			break;
		}
		case BamlRecordType.PropertyComplexStart:
		{
			XamlPropertyComplexEndNode xamlPropertyComplexEnd = new XamlPropertyComplexEndNode(0, 0, --_depth);
			_bamlRecordWriter.WritePropertyComplexEnd(xamlPropertyComplexEnd);
			break;
		}
		default:
			throw new InvalidOperationException(SR.Format(SR.BamlWriterBadScope, bamlRecordType.ToString(), BamlRecordType.PropertyComplexEnd.ToString()));
		}
		_parserContext.PopScope();
	}

	public void WriteLiteralContent(string contents)
	{
		VerifyWriteState();
		ProcessMarkupExtensionNodes();
		XamlLiteralContentNode xamlLiteralContentNode = new XamlLiteralContentNode(0, 0, _depth, contents);
		_bamlRecordWriter.WriteLiteralContent(xamlLiteralContentNode);
	}

	public void WritePIMapping(string xmlNamespace, string clrNamespace, string assemblyName)
	{
		VerifyWriteState();
		ProcessMarkupExtensionNodes();
		XamlPIMappingNode xamlPIMappingNode = new XamlPIMappingNode(0, 0, _depth, xmlNamespace, clrNamespace, assemblyName);
		if (!_xamlTypeMapper.PITable.Contains(xmlNamespace))
		{
			ClrNamespaceAssemblyPair clrNamespaceAssemblyPair = new ClrNamespaceAssemblyPair(clrNamespace, assemblyName);
			_xamlTypeMapper.PITable.Add(xmlNamespace, clrNamespaceAssemblyPair);
		}
		_bamlRecordWriter.WritePIMapping(xamlPIMappingNode);
	}

	public void WriteText(string textContent, string typeConverterAssemblyName, string typeConverterName)
	{
		VerifyWriteState();
		ProcessMarkupExtensionNodes();
		Type converterType = null;
		if (!string.IsNullOrEmpty(typeConverterName))
		{
			converterType = GetType(typeConverterAssemblyName, typeConverterName);
		}
		XamlTextNode xamlTextNode = new XamlTextNode(0, 0, _depth, textContent, converterType);
		_bamlRecordWriter.WriteText(xamlTextNode);
	}

	public void WriteRoutedEvent(string assemblyName, string ownerTypeFullName, string eventIdName, string handlerName)
	{
		throw new NotSupportedException(SR.Format(SR.ParserBamlEvent, eventIdName));
	}

	public void WriteEvent(string eventName, string handlerName)
	{
		throw new NotSupportedException(SR.Format(SR.ParserBamlEvent, eventName));
	}

	private void ProcessMarkupExtensionNodes()
	{
		for (int i = 0; i < _markupExtensionNodes.Count; i++)
		{
			XamlNode xamlNode = _markupExtensionNodes[i] as XamlNode;
			switch (xamlNode.TokenType)
			{
			case XamlNodeType.ElementStart:
				_bamlRecordWriter.WriteElementStart((XamlElementStartNode)xamlNode);
				break;
			case XamlNodeType.ElementEnd:
				_bamlRecordWriter.WriteElementEnd((XamlElementEndNode)xamlNode);
				break;
			case XamlNodeType.KeyElementStart:
				_bamlRecordWriter.WriteKeyElementStart((XamlKeyElementStartNode)xamlNode);
				break;
			case XamlNodeType.KeyElementEnd:
				_bamlRecordWriter.WriteKeyElementEnd((XamlKeyElementEndNode)xamlNode);
				break;
			case XamlNodeType.Property:
				_bamlRecordWriter.WriteProperty((XamlPropertyNode)xamlNode);
				break;
			case XamlNodeType.PropertyWithExtension:
				_bamlRecordWriter.WritePropertyWithExtension((XamlPropertyWithExtensionNode)xamlNode);
				break;
			case XamlNodeType.PropertyWithType:
				_bamlRecordWriter.WritePropertyWithType((XamlPropertyWithTypeNode)xamlNode);
				break;
			case XamlNodeType.PropertyComplexStart:
				_bamlRecordWriter.WritePropertyComplexStart((XamlPropertyComplexStartNode)xamlNode);
				break;
			case XamlNodeType.PropertyComplexEnd:
				_bamlRecordWriter.WritePropertyComplexEnd((XamlPropertyComplexEndNode)xamlNode);
				break;
			case XamlNodeType.Text:
				_bamlRecordWriter.WriteText((XamlTextNode)xamlNode);
				break;
			case XamlNodeType.EndAttributes:
				_bamlRecordWriter.WriteEndAttributes((XamlEndAttributesNode)xamlNode);
				break;
			case XamlNodeType.ConstructorParametersStart:
				_bamlRecordWriter.WriteConstructorParametersStart((XamlConstructorParametersStartNode)xamlNode);
				break;
			case XamlNodeType.ConstructorParametersEnd:
				_bamlRecordWriter.WriteConstructorParametersEnd((XamlConstructorParametersEndNode)xamlNode);
				break;
			default:
				throw new InvalidOperationException(SR.BamlWriterUnknownMarkupExtension);
			}
		}
		_markupExtensionNodes.Clear();
	}

	private void VerifyWriteState()
	{
		if (_closed)
		{
			throw new InvalidOperationException(SR.BamlWriterClosed);
		}
		if (!_startDocumentWritten)
		{
			throw new InvalidOperationException(SR.BamlWriterStartDoc);
		}
	}

	private void VerifyEndTagState(BamlRecordType expectedStartTag, BamlRecordType endTagBeingWritten)
	{
		VerifyWriteState();
		BamlRecordType bamlRecordType = Pop();
		if (bamlRecordType != expectedStartTag)
		{
			throw new InvalidOperationException(SR.Format(SR.BamlWriterBadScope, bamlRecordType.ToString(), endTagBeingWritten.ToString()));
		}
	}

	private Assembly GetAssembly(string assemblyName)
	{
		Assembly assembly = _assemblies[assemblyName] as Assembly;
		if (assembly == null)
		{
			assembly = ReflectionHelper.LoadAssembly(assemblyName, null);
			if (assembly == null)
			{
				throw new ArgumentException(SR.Format(SR.BamlWriterBadAssembly, assemblyName));
			}
			_assemblies[assemblyName] = assembly;
		}
		return assembly;
	}

	private Type GetType(string assemblyName, string typeFullName)
	{
		return GetAssembly(assemblyName).GetType(typeFullName);
	}

	private object GetDpOrPi(Type ownerType, string propName)
	{
		object obj = null;
		if (ownerType != null)
		{
			obj = DependencyProperty.FromName(propName, ownerType);
			if (obj == null)
			{
				PropertyInfo propertyInfo = null;
				MemberInfo[] member = ownerType.GetMember(propName, MemberTypes.Property, BindingFlags.Instance | BindingFlags.Public);
				for (int i = 0; i < member.Length; i++)
				{
					PropertyInfo propertyInfo2 = (PropertyInfo)member[i];
					if (propertyInfo2.GetIndexParameters().Length == 0 && (propertyInfo == null || propertyInfo.DeclaringType.IsAssignableFrom(propertyInfo2.DeclaringType)))
					{
						propertyInfo = propertyInfo2;
					}
				}
				obj = propertyInfo;
			}
		}
		return obj;
	}

	private void GetDpOrPi(string assemblyName, string ownerTypeFullName, string propName, out object dpOrPi, out Type ownerType)
	{
		if (assemblyName == string.Empty || ownerTypeFullName == string.Empty)
		{
			dpOrPi = null;
			ownerType = null;
		}
		else
		{
			ownerType = GetType(assemblyName, ownerTypeFullName);
			dpOrPi = GetDpOrPi(ownerType, propName);
		}
	}

	private MethodInfo GetMi(Type ownerType, string propName)
	{
		MethodInfo methodInfo = null;
		methodInfo = ownerType.GetMethod("Set" + propName, BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
		if (methodInfo != null && methodInfo.GetParameters().Length != 2)
		{
			methodInfo = null;
		}
		if (methodInfo == null)
		{
			methodInfo = ownerType.GetMethod("Get" + propName, BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
			if (methodInfo != null && methodInfo.GetParameters().Length != 1)
			{
				methodInfo = null;
			}
		}
		return methodInfo;
	}

	private MethodInfo GetMi(string assemblyName, string ownerTypeFullName, string propName, out Type ownerType)
	{
		MethodInfo methodInfo = null;
		if (assemblyName == string.Empty || ownerTypeFullName == string.Empty)
		{
			methodInfo = null;
			ownerType = null;
		}
		else
		{
			ownerType = GetType(assemblyName, ownerTypeFullName);
			methodInfo = GetMi(ownerType, propName);
		}
		return methodInfo;
	}

	private void Push(BamlRecordType recordType)
	{
		CheckEndAttributes();
		_nodeTypeStack.Push(new WriteStackNode(recordType));
	}

	private void Push(BamlRecordType recordType, Type elementType)
	{
		CheckEndAttributes();
		_nodeTypeStack.Push(new WriteStackNode(recordType, elementType));
	}

	private BamlRecordType Pop()
	{
		return (_nodeTypeStack.Pop() as WriteStackNode).RecordType;
	}

	private BamlRecordType PeekRecordType()
	{
		return (_nodeTypeStack.Peek() as WriteStackNode).RecordType;
	}

	private Type PeekElementType()
	{
		return (_nodeTypeStack.Peek() as WriteStackNode).ElementType;
	}

	private void CheckEndAttributes()
	{
		if (_nodeTypeStack.Count > 0)
		{
			WriteStackNode writeStackNode = _nodeTypeStack.Peek() as WriteStackNode;
			if (!writeStackNode.EndAttributesReached && writeStackNode.RecordType == BamlRecordType.ElementStart)
			{
				XamlEndAttributesNode xamlEndAttributesNode = new XamlEndAttributesNode(0, 0, _depth, compact: false);
				_bamlRecordWriter.WriteEndAttributes(xamlEndAttributesNode);
			}
			writeStackNode.EndAttributesReached = true;
		}
	}
}
