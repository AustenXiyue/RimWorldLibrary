using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.Diagnostics;
using System.Runtime.Serialization.Diagnostics;
using System.Security;
using System.Xml;
using System.Xml.Schema;

namespace System.Runtime.Serialization;

internal class SchemaImporter
{
	private DataContractSet dataContractSet;

	private XmlSchemaSet schemaSet;

	private ICollection<XmlQualifiedName> typeNames;

	private ICollection<XmlSchemaElement> elements;

	private XmlQualifiedName[] elementTypeNames;

	private bool importXmlDataType;

	private Dictionary<XmlQualifiedName, SchemaObjectInfo> schemaObjects;

	private List<XmlSchemaRedefine> redefineList;

	private bool needToImportKnownTypesForObject;

	[SecurityCritical]
	private static Hashtable serializationSchemaElements;

	private Dictionary<XmlQualifiedName, SchemaObjectInfo> SchemaObjects
	{
		get
		{
			if (schemaObjects == null)
			{
				schemaObjects = CreateSchemaObjects();
			}
			return schemaObjects;
		}
	}

	private List<XmlSchemaRedefine> RedefineList
	{
		get
		{
			if (redefineList == null)
			{
				redefineList = CreateRedefineList();
			}
			return redefineList;
		}
	}

	internal SchemaImporter(XmlSchemaSet schemas, ICollection<XmlQualifiedName> typeNames, ICollection<XmlSchemaElement> elements, XmlQualifiedName[] elementTypeNames, DataContractSet dataContractSet, bool importXmlDataType)
	{
		this.dataContractSet = dataContractSet;
		schemaSet = schemas;
		this.typeNames = typeNames;
		this.elements = elements;
		this.elementTypeNames = elementTypeNames;
		this.importXmlDataType = importXmlDataType;
	}

	internal void Import()
	{
		if (!schemaSet.Contains("http://schemas.microsoft.com/2003/10/Serialization/"))
		{
			XmlSchema xmlSchema = XmlSchema.Read(new XmlTextReader(new StringReader("<?xml version='1.0' encoding='utf-8'?>\n<xs:schema elementFormDefault='qualified' attributeFormDefault='qualified' xmlns:tns='http://schemas.microsoft.com/2003/10/Serialization/' targetNamespace='http://schemas.microsoft.com/2003/10/Serialization/' xmlns:xs='http://www.w3.org/2001/XMLSchema'>\n  <xs:element name='anyType' nillable='true' type='xs:anyType' />\n  <xs:element name='anyURI' nillable='true' type='xs:anyURI' />\n  <xs:element name='base64Binary' nillable='true' type='xs:base64Binary' />\n  <xs:element name='boolean' nillable='true' type='xs:boolean' />\n  <xs:element name='byte' nillable='true' type='xs:byte' />\n  <xs:element name='dateTime' nillable='true' type='xs:dateTime' />\n  <xs:element name='decimal' nillable='true' type='xs:decimal' />\n  <xs:element name='double' nillable='true' type='xs:double' />\n  <xs:element name='float' nillable='true' type='xs:float' />\n  <xs:element name='int' nillable='true' type='xs:int' />\n  <xs:element name='long' nillable='true' type='xs:long' />\n  <xs:element name='QName' nillable='true' type='xs:QName' />\n  <xs:element name='short' nillable='true' type='xs:short' />\n  <xs:element name='string' nillable='true' type='xs:string' />\n  <xs:element name='unsignedByte' nillable='true' type='xs:unsignedByte' />\n  <xs:element name='unsignedInt' nillable='true' type='xs:unsignedInt' />\n  <xs:element name='unsignedLong' nillable='true' type='xs:unsignedLong' />\n  <xs:element name='unsignedShort' nillable='true' type='xs:unsignedShort' />\n  <xs:element name='char' nillable='true' type='tns:char' />\n  <xs:simpleType name='char'>\n    <xs:restriction base='xs:int'/>\n  </xs:simpleType>  \n  <xs:element name='duration' nillable='true' type='tns:duration' />\n  <xs:simpleType name='duration'>\n    <xs:restriction base='xs:duration'>\n      <xs:pattern value='\\-?P(\\d*D)?(T(\\d*H)?(\\d*M)?(\\d*(\\.\\d*)?S)?)?' />\n      <xs:minInclusive value='-P10675199DT2H48M5.4775808S' />\n      <xs:maxInclusive value='P10675199DT2H48M5.4775807S' />\n    </xs:restriction>\n  </xs:simpleType>\n  <xs:element name='guid' nillable='true' type='tns:guid' />\n  <xs:simpleType name='guid'>\n    <xs:restriction base='xs:string'>\n      <xs:pattern value='[\\da-fA-F]{8}-[\\da-fA-F]{4}-[\\da-fA-F]{4}-[\\da-fA-F]{4}-[\\da-fA-F]{12}' />\n    </xs:restriction>\n  </xs:simpleType>\n  <xs:attribute name='FactoryType' type='xs:QName' />\n  <xs:attribute name='Id' type='xs:ID' />\n  <xs:attribute name='Ref' type='xs:IDREF' />\n</xs:schema>\n"))
			{
				DtdProcessing = DtdProcessing.Prohibit
			}, null);
			if (xmlSchema == null)
			{
				throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString("Could not read serialization schema for '{0}' namespace.", "http://schemas.microsoft.com/2003/10/Serialization/")));
			}
			schemaSet.Add(xmlSchema);
		}
		try
		{
			CompileSchemaSet(schemaSet);
		}
		catch (Exception ex)
		{
			if (Fx.IsFatal(ex))
			{
				throw;
			}
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString("Cannot import invalid schemas."), ex));
		}
		if (typeNames == null)
		{
			foreach (object item in schemaSet.Schemas())
			{
				XmlSchema xmlSchema2 = (XmlSchema)(item ?? throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString("Cannot import from schema list that contains null."))));
				if (!(xmlSchema2.TargetNamespace != "http://schemas.microsoft.com/2003/10/Serialization/") || !(xmlSchema2.TargetNamespace != "http://www.w3.org/2001/XMLSchema"))
				{
					continue;
				}
				foreach (XmlSchemaObject value in xmlSchema2.SchemaTypes.Values)
				{
					ImportType((XmlSchemaType)value);
				}
				foreach (XmlSchemaElement value2 in xmlSchema2.Elements.Values)
				{
					if (value2.SchemaType != null)
					{
						ImportAnonymousGlobalElement(value2, value2.QualifiedName, xmlSchema2.TargetNamespace);
					}
				}
			}
		}
		else
		{
			foreach (XmlQualifiedName typeName in typeNames)
			{
				if (typeName == null)
				{
					throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString("Cannot import data contract with null name.")));
				}
				ImportType(typeName);
			}
			if (elements != null)
			{
				int num = 0;
				foreach (XmlSchemaElement element in elements)
				{
					XmlQualifiedName schemaTypeName = element.SchemaTypeName;
					if (schemaTypeName != null && schemaTypeName.Name.Length > 0)
					{
						elementTypeNames[num++] = ImportType(schemaTypeName).StableName;
						continue;
					}
					XmlSchema schemaWithGlobalElementDeclaration = SchemaHelper.GetSchemaWithGlobalElementDeclaration(element, schemaSet);
					if (schemaWithGlobalElementDeclaration == null)
					{
						elementTypeNames[num++] = ImportAnonymousElement(element, element.QualifiedName).StableName;
					}
					else
					{
						elementTypeNames[num++] = ImportAnonymousGlobalElement(element, element.QualifiedName, schemaWithGlobalElementDeclaration.TargetNamespace).StableName;
					}
				}
			}
		}
		ImportKnownTypesForObject();
	}

	internal static void CompileSchemaSet(XmlSchemaSet schemaSet)
	{
		if (schemaSet.Contains("http://www.w3.org/2001/XMLSchema"))
		{
			schemaSet.Compile();
			return;
		}
		XmlSchema xmlSchema = new XmlSchema();
		xmlSchema.TargetNamespace = "http://www.w3.org/2001/XMLSchema";
		XmlSchemaElement xmlSchemaElement = new XmlSchemaElement();
		xmlSchemaElement.Name = "schema";
		xmlSchemaElement.SchemaType = new XmlSchemaComplexType();
		xmlSchema.Items.Add(xmlSchemaElement);
		schemaSet.Add(xmlSchema);
		schemaSet.Compile();
	}

	private void ImportKnownTypes(XmlQualifiedName typeName)
	{
		if (!SchemaObjects.TryGetValue(typeName, out var value))
		{
			return;
		}
		List<XmlSchemaType> knownTypes = value.knownTypes;
		if (knownTypes == null)
		{
			return;
		}
		foreach (XmlSchemaType item in knownTypes)
		{
			ImportType(item);
		}
	}

	internal static bool IsObjectContract(DataContract dataContract)
	{
		Dictionary<Type, object> dictionary = new Dictionary<Type, object>();
		while (dataContract is CollectionDataContract)
		{
			if (dataContract.OriginalUnderlyingType == null)
			{
				dataContract = ((CollectionDataContract)dataContract).ItemContract;
				continue;
			}
			if (dictionary.ContainsKey(dataContract.OriginalUnderlyingType))
			{
				break;
			}
			dictionary.Add(dataContract.OriginalUnderlyingType, dataContract.OriginalUnderlyingType);
			dataContract = ((CollectionDataContract)dataContract).ItemContract;
		}
		if (dataContract is PrimitiveDataContract)
		{
			return ((PrimitiveDataContract)dataContract).UnderlyingType == Globals.TypeOfObject;
		}
		return false;
	}

	private void ImportKnownTypesForObject()
	{
		if (!needToImportKnownTypesForObject)
		{
			return;
		}
		needToImportKnownTypesForObject = false;
		if (dataContractSet.KnownTypesForObject != null || !SchemaObjects.TryGetValue(SchemaExporter.AnytypeQualifiedName, out var value))
		{
			return;
		}
		List<XmlSchemaType> knownTypes = value.knownTypes;
		if (knownTypes == null)
		{
			return;
		}
		Dictionary<XmlQualifiedName, DataContract> dictionary = new Dictionary<XmlQualifiedName, DataContract>();
		foreach (XmlSchemaType item in knownTypes)
		{
			DataContract dataContract = ImportType(item);
			if (!dictionary.TryGetValue(dataContract.StableName, out var _))
			{
				dictionary.Add(dataContract.StableName, dataContract);
			}
		}
		dataContractSet.KnownTypesForObject = dictionary;
	}

	internal Dictionary<XmlQualifiedName, SchemaObjectInfo> CreateSchemaObjects()
	{
		Dictionary<XmlQualifiedName, SchemaObjectInfo> dictionary = new Dictionary<XmlQualifiedName, SchemaObjectInfo>();
		ICollection collection = schemaSet.Schemas();
		List<XmlSchemaType> list = new List<XmlSchemaType>();
		dictionary.Add(SchemaExporter.AnytypeQualifiedName, new SchemaObjectInfo(null, null, null, list));
		foreach (XmlSchema item in collection)
		{
			if (!(item.TargetNamespace != "http://schemas.microsoft.com/2003/10/Serialization/"))
			{
				continue;
			}
			foreach (XmlSchemaObject value4 in item.SchemaTypes.Values)
			{
				if (!(value4 is XmlSchemaType xmlSchemaType))
				{
					continue;
				}
				list.Add(xmlSchemaType);
				XmlQualifiedName key = new XmlQualifiedName(xmlSchemaType.Name, item.TargetNamespace);
				if (dictionary.TryGetValue(key, out var value))
				{
					value.type = xmlSchemaType;
					value.schema = item;
				}
				else
				{
					dictionary.Add(key, new SchemaObjectInfo(xmlSchemaType, null, item, null));
				}
				XmlQualifiedName baseTypeName = GetBaseTypeName(xmlSchemaType);
				if (!(baseTypeName != null))
				{
					continue;
				}
				if (dictionary.TryGetValue(baseTypeName, out var value2))
				{
					if (value2.knownTypes == null)
					{
						value2.knownTypes = new List<XmlSchemaType>();
					}
				}
				else
				{
					value2 = new SchemaObjectInfo(null, null, null, new List<XmlSchemaType>());
					dictionary.Add(baseTypeName, value2);
				}
				value2.knownTypes.Add(xmlSchemaType);
			}
			foreach (XmlSchemaObject value5 in item.Elements.Values)
			{
				if (value5 is XmlSchemaElement xmlSchemaElement)
				{
					XmlQualifiedName key2 = new XmlQualifiedName(xmlSchemaElement.Name, item.TargetNamespace);
					if (dictionary.TryGetValue(key2, out var value3))
					{
						value3.element = xmlSchemaElement;
						value3.schema = item;
					}
					else
					{
						dictionary.Add(key2, new SchemaObjectInfo(null, xmlSchemaElement, item, null));
					}
				}
			}
		}
		return dictionary;
	}

	private XmlQualifiedName GetBaseTypeName(XmlSchemaType type)
	{
		XmlQualifiedName result = null;
		if (type is XmlSchemaComplexType { ContentModel: not null, ContentModel: XmlSchemaComplexContent { Content: XmlSchemaComplexContentExtension content } })
		{
			result = content.BaseTypeName;
		}
		return result;
	}

	private List<XmlSchemaRedefine> CreateRedefineList()
	{
		List<XmlSchemaRedefine> list = new List<XmlSchemaRedefine>();
		foreach (object item2 in schemaSet.Schemas())
		{
			if (!(item2 is XmlSchema xmlSchema))
			{
				continue;
			}
			foreach (XmlSchemaExternal include in xmlSchema.Includes)
			{
				if (include is XmlSchemaRedefine item)
				{
					list.Add(item);
				}
			}
		}
		return list;
	}

	[SecuritySafeCritical]
	private DataContract ImportAnonymousGlobalElement(XmlSchemaElement element, XmlQualifiedName typeQName, string ns)
	{
		DataContract dataContract = ImportAnonymousElement(element, typeQName);
		if (dataContract is XmlDataContract xmlDataContract)
		{
			xmlDataContract.SetTopLevelElementName(new XmlQualifiedName(element.Name, ns));
			xmlDataContract.IsTopLevelElementNullable = element.IsNillable;
		}
		return dataContract;
	}

	private DataContract ImportAnonymousElement(XmlSchemaElement element, XmlQualifiedName typeQName)
	{
		if (SchemaHelper.GetSchemaType(SchemaObjects, typeQName) != null)
		{
			int num = 1;
			while (true)
			{
				typeQName = new XmlQualifiedName(typeQName.Name + num.ToString(NumberFormatInfo.InvariantInfo), typeQName.Namespace);
				if (SchemaHelper.GetSchemaType(SchemaObjects, typeQName) == null)
				{
					break;
				}
				if (num == int.MaxValue)
				{
					throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString("Cannot compute unique name for '{0}'.", element.Name)));
				}
				num++;
			}
		}
		if (element.SchemaType == null)
		{
			return ImportType(SchemaExporter.AnytypeQualifiedName);
		}
		return ImportType(element.SchemaType, typeQName, isAnonymous: true);
	}

	private DataContract ImportType(XmlQualifiedName typeName)
	{
		DataContract dataContract = DataContract.GetBuiltInDataContract(typeName.Name, typeName.Namespace);
		if (dataContract == null)
		{
			XmlSchemaType schemaType = SchemaHelper.GetSchemaType(SchemaObjects, typeName);
			if (schemaType == null)
			{
				throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString("Specified type '{0}' in '{1}' namespace is not found in the schemas.", typeName.Name, typeName.Namespace)));
			}
			dataContract = ImportType(schemaType);
		}
		if (IsObjectContract(dataContract))
		{
			needToImportKnownTypesForObject = true;
		}
		return dataContract;
	}

	private DataContract ImportType(XmlSchemaType type)
	{
		return ImportType(type, type.QualifiedName, isAnonymous: false);
	}

	private DataContract ImportType(XmlSchemaType type, XmlQualifiedName typeName, bool isAnonymous)
	{
		DataContract dataContract = dataContractSet[typeName];
		if (dataContract != null)
		{
			return dataContract;
		}
		InvalidDataContractException ex2;
		try
		{
			foreach (XmlSchemaRedefine redefine in RedefineList)
			{
				if (redefine.SchemaTypes[typeName] != null)
				{
					ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString("XML Schema 'redefine' is not supported."));
				}
			}
			if (type is XmlSchemaSimpleType)
			{
				XmlSchemaSimpleType xmlSchemaSimpleType = (XmlSchemaSimpleType)type;
				XmlSchemaSimpleTypeContent content = xmlSchemaSimpleType.Content;
				if (content is XmlSchemaSimpleTypeUnion)
				{
					ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString("simpleType union is not supported."));
				}
				else if (content is XmlSchemaSimpleTypeList)
				{
					dataContract = ImportFlagsEnum(typeName, (XmlSchemaSimpleTypeList)content, xmlSchemaSimpleType.Annotation);
				}
				else if (content is XmlSchemaSimpleTypeRestriction)
				{
					XmlSchemaSimpleTypeRestriction restriction = (XmlSchemaSimpleTypeRestriction)content;
					if (CheckIfEnum(restriction))
					{
						dataContract = ImportEnum(typeName, restriction, isFlags: false, xmlSchemaSimpleType.Annotation);
					}
					else
					{
						dataContract = ImportSimpleTypeRestriction(typeName, restriction);
						if (dataContract.IsBuiltInDataContract && !isAnonymous)
						{
							dataContractSet.InternalAdd(typeName, dataContract);
						}
					}
				}
			}
			else if (type is XmlSchemaComplexType)
			{
				XmlSchemaComplexType xmlSchemaComplexType = (XmlSchemaComplexType)type;
				if (xmlSchemaComplexType.ContentModel == null)
				{
					CheckComplexType(typeName, xmlSchemaComplexType);
					dataContract = ImportType(typeName, xmlSchemaComplexType.Particle, xmlSchemaComplexType.Attributes, xmlSchemaComplexType.AnyAttribute, null, xmlSchemaComplexType.Annotation);
				}
				else
				{
					XmlSchemaContentModel contentModel = xmlSchemaComplexType.ContentModel;
					if (contentModel is XmlSchemaSimpleContent)
					{
						ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString("Simple content is not supported."));
					}
					else if (contentModel is XmlSchemaComplexContent)
					{
						XmlSchemaComplexContent xmlSchemaComplexContent = (XmlSchemaComplexContent)contentModel;
						if (xmlSchemaComplexContent.IsMixed)
						{
							ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString("Mixed content is not supported."));
						}
						if (xmlSchemaComplexContent.Content is XmlSchemaComplexContentExtension)
						{
							XmlSchemaComplexContentExtension xmlSchemaComplexContentExtension = (XmlSchemaComplexContentExtension)xmlSchemaComplexContent.Content;
							dataContract = ImportType(typeName, xmlSchemaComplexContentExtension.Particle, xmlSchemaComplexContentExtension.Attributes, xmlSchemaComplexContentExtension.AnyAttribute, xmlSchemaComplexContentExtension.BaseTypeName, xmlSchemaComplexType.Annotation);
						}
						else if (xmlSchemaComplexContent.Content is XmlSchemaComplexContentRestriction)
						{
							XmlSchemaComplexContentRestriction xmlSchemaComplexContentRestriction = (XmlSchemaComplexContentRestriction)xmlSchemaComplexContent.Content;
							if (xmlSchemaComplexContentRestriction.BaseTypeName == SchemaExporter.AnytypeQualifiedName)
							{
								dataContract = ImportType(typeName, xmlSchemaComplexContentRestriction.Particle, xmlSchemaComplexContentRestriction.Attributes, xmlSchemaComplexContentRestriction.AnyAttribute, null, xmlSchemaComplexType.Annotation);
							}
							else
							{
								ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString("XML schema complexType restriction is not supported."));
							}
						}
					}
				}
			}
			if (dataContract == null)
			{
				ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, string.Empty);
			}
			if (type.QualifiedName != XmlQualifiedName.Empty)
			{
				ImportTopLevelElement(typeName);
			}
			ImportDataContractExtension(type, dataContract);
			ImportGenericInfo(type, dataContract);
			ImportKnownTypes(typeName);
			return dataContract;
		}
		catch (InvalidDataContractException ex)
		{
			ex2 = ex;
		}
		if (importXmlDataType)
		{
			RemoveFailedContract(typeName);
			return ImportXmlDataType(typeName, type, isAnonymous);
		}
		if ((dataContractSet.TryGetReferencedType(typeName, dataContract, out var type2) || (string.IsNullOrEmpty(type.Name) && dataContractSet.TryGetReferencedType(ImportActualType(type.Annotation, typeName, typeName), dataContract, out type2))) && Globals.TypeOfIXmlSerializable.IsAssignableFrom(type2))
		{
			RemoveFailedContract(typeName);
			return ImportXmlDataType(typeName, type, isAnonymous);
		}
		XmlDataContract xmlDataContract = ImportSpecialXmlDataType(type, isAnonymous);
		if (xmlDataContract != null)
		{
			dataContractSet.Remove(typeName);
			return xmlDataContract;
		}
		throw ex2;
	}

	private void RemoveFailedContract(XmlQualifiedName typeName)
	{
		ClassDataContract classDataContract = dataContractSet[typeName] as ClassDataContract;
		dataContractSet.Remove(typeName);
		if (classDataContract != null)
		{
			for (ClassDataContract baseContract = classDataContract.BaseContract; baseContract != null; baseContract = baseContract.BaseContract)
			{
				baseContract.KnownDataContracts.Remove(typeName);
			}
			if (dataContractSet.KnownTypesForObject != null)
			{
				dataContractSet.KnownTypesForObject.Remove(typeName);
			}
		}
	}

	private bool CheckIfEnum(XmlSchemaSimpleTypeRestriction restriction)
	{
		foreach (XmlSchemaFacet facet in restriction.Facets)
		{
			if (!(facet is XmlSchemaEnumerationFacet))
			{
				return false;
			}
		}
		XmlQualifiedName stringQualifiedName = SchemaExporter.StringQualifiedName;
		if (restriction.BaseTypeName != XmlQualifiedName.Empty)
		{
			if (!(restriction.BaseTypeName == stringQualifiedName) || restriction.Facets.Count <= 0)
			{
				return ImportType(restriction.BaseTypeName) is EnumDataContract;
			}
			return true;
		}
		if (restriction.BaseType != null)
		{
			DataContract dataContract = ImportType(restriction.BaseType);
			if (!(dataContract.StableName == stringQualifiedName))
			{
				return dataContract is EnumDataContract;
			}
			return true;
		}
		return false;
	}

	private bool CheckIfCollection(XmlSchemaSequence rootSequence)
	{
		if (rootSequence.Items == null || rootSequence.Items.Count == 0)
		{
			return false;
		}
		RemoveOptionalUnknownSerializationElements(rootSequence.Items);
		if (rootSequence.Items.Count != 1)
		{
			return false;
		}
		XmlSchemaObject xmlSchemaObject = rootSequence.Items[0];
		if (!(xmlSchemaObject is XmlSchemaElement))
		{
			return false;
		}
		XmlSchemaElement xmlSchemaElement = (XmlSchemaElement)xmlSchemaObject;
		if (!(xmlSchemaElement.MaxOccursString == "unbounded"))
		{
			return xmlSchemaElement.MaxOccurs > 1m;
		}
		return true;
	}

	private bool CheckIfISerializable(XmlSchemaSequence rootSequence, XmlSchemaObjectCollection attributes)
	{
		if (rootSequence.Items == null || rootSequence.Items.Count == 0)
		{
			return false;
		}
		RemoveOptionalUnknownSerializationElements(rootSequence.Items);
		if (attributes == null || attributes.Count == 0)
		{
			return false;
		}
		if (rootSequence.Items.Count == 1)
		{
			return rootSequence.Items[0] is XmlSchemaAny;
		}
		return false;
	}

	[SecuritySafeCritical]
	private void RemoveOptionalUnknownSerializationElements(XmlSchemaObjectCollection items)
	{
		for (int i = 0; i < items.Count; i++)
		{
			if (!(items[i] is XmlSchemaElement xmlSchemaElement) || !(xmlSchemaElement.RefName != null) || !(xmlSchemaElement.RefName.Namespace == "http://schemas.microsoft.com/2003/10/Serialization/") || !(xmlSchemaElement.MinOccurs == 0m))
			{
				continue;
			}
			if (serializationSchemaElements == null)
			{
				XmlSchema xmlSchema = XmlSchema.Read(XmlReader.Create(new StringReader("<?xml version='1.0' encoding='utf-8'?>\n<xs:schema elementFormDefault='qualified' attributeFormDefault='qualified' xmlns:tns='http://schemas.microsoft.com/2003/10/Serialization/' targetNamespace='http://schemas.microsoft.com/2003/10/Serialization/' xmlns:xs='http://www.w3.org/2001/XMLSchema'>\n  <xs:element name='anyType' nillable='true' type='xs:anyType' />\n  <xs:element name='anyURI' nillable='true' type='xs:anyURI' />\n  <xs:element name='base64Binary' nillable='true' type='xs:base64Binary' />\n  <xs:element name='boolean' nillable='true' type='xs:boolean' />\n  <xs:element name='byte' nillable='true' type='xs:byte' />\n  <xs:element name='dateTime' nillable='true' type='xs:dateTime' />\n  <xs:element name='decimal' nillable='true' type='xs:decimal' />\n  <xs:element name='double' nillable='true' type='xs:double' />\n  <xs:element name='float' nillable='true' type='xs:float' />\n  <xs:element name='int' nillable='true' type='xs:int' />\n  <xs:element name='long' nillable='true' type='xs:long' />\n  <xs:element name='QName' nillable='true' type='xs:QName' />\n  <xs:element name='short' nillable='true' type='xs:short' />\n  <xs:element name='string' nillable='true' type='xs:string' />\n  <xs:element name='unsignedByte' nillable='true' type='xs:unsignedByte' />\n  <xs:element name='unsignedInt' nillable='true' type='xs:unsignedInt' />\n  <xs:element name='unsignedLong' nillable='true' type='xs:unsignedLong' />\n  <xs:element name='unsignedShort' nillable='true' type='xs:unsignedShort' />\n  <xs:element name='char' nillable='true' type='tns:char' />\n  <xs:simpleType name='char'>\n    <xs:restriction base='xs:int'/>\n  </xs:simpleType>  \n  <xs:element name='duration' nillable='true' type='tns:duration' />\n  <xs:simpleType name='duration'>\n    <xs:restriction base='xs:duration'>\n      <xs:pattern value='\\-?P(\\d*D)?(T(\\d*H)?(\\d*M)?(\\d*(\\.\\d*)?S)?)?' />\n      <xs:minInclusive value='-P10675199DT2H48M5.4775808S' />\n      <xs:maxInclusive value='P10675199DT2H48M5.4775807S' />\n    </xs:restriction>\n  </xs:simpleType>\n  <xs:element name='guid' nillable='true' type='tns:guid' />\n  <xs:simpleType name='guid'>\n    <xs:restriction base='xs:string'>\n      <xs:pattern value='[\\da-fA-F]{8}-[\\da-fA-F]{4}-[\\da-fA-F]{4}-[\\da-fA-F]{4}-[\\da-fA-F]{12}' />\n    </xs:restriction>\n  </xs:simpleType>\n  <xs:attribute name='FactoryType' type='xs:QName' />\n  <xs:attribute name='Id' type='xs:ID' />\n  <xs:attribute name='Ref' type='xs:IDREF' />\n</xs:schema>\n")), null);
				serializationSchemaElements = new Hashtable();
				foreach (XmlSchemaObject item in xmlSchema.Items)
				{
					if (item is XmlSchemaElement xmlSchemaElement2)
					{
						serializationSchemaElements.Add(xmlSchemaElement2.Name, xmlSchemaElement2);
					}
				}
			}
			if (!serializationSchemaElements.ContainsKey(xmlSchemaElement.RefName.Name))
			{
				items.RemoveAt(i);
				i--;
			}
		}
	}

	private DataContract ImportType(XmlQualifiedName typeName, XmlSchemaParticle rootParticle, XmlSchemaObjectCollection attributes, XmlSchemaAnyAttribute anyAttribute, XmlQualifiedName baseTypeName, XmlSchemaAnnotation annotation)
	{
		DataContract result = null;
		bool flag = baseTypeName != null;
		ImportAttributes(typeName, attributes, anyAttribute, out var isReference);
		if (rootParticle == null)
		{
			result = ImportClass(typeName, new XmlSchemaSequence(), baseTypeName, annotation, isReference);
		}
		else if (!(rootParticle is XmlSchemaSequence))
		{
			ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString("Root particle must be sequence to be imported."));
		}
		else
		{
			XmlSchemaSequence xmlSchemaSequence = (XmlSchemaSequence)rootParticle;
			if (xmlSchemaSequence.MinOccurs != 1m)
			{
				ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString("Root sequence must have an item and minOccurs must be 1."));
			}
			if (xmlSchemaSequence.MaxOccurs != 1m)
			{
				ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString("On root sequence, maxOccurs must be 1."));
			}
			result = ((!flag && CheckIfCollection(xmlSchemaSequence)) ? ((DataContract)ImportCollection(typeName, xmlSchemaSequence, attributes, annotation, isReference)) : ((DataContract)((!CheckIfISerializable(xmlSchemaSequence, attributes)) ? ImportClass(typeName, xmlSchemaSequence, baseTypeName, annotation, isReference) : ImportISerializable(typeName, xmlSchemaSequence, baseTypeName, attributes, annotation))));
		}
		return result;
	}

	[SecuritySafeCritical]
	private ClassDataContract ImportClass(XmlQualifiedName typeName, XmlSchemaSequence rootSequence, XmlQualifiedName baseTypeName, XmlSchemaAnnotation annotation, bool isReference)
	{
		ClassDataContract classDataContract = new ClassDataContract();
		classDataContract.StableName = typeName;
		AddDataContract(classDataContract);
		classDataContract.IsValueType = IsValueType(typeName, annotation);
		classDataContract.IsReference = isReference;
		if (baseTypeName != null)
		{
			ImportBaseContract(baseTypeName, classDataContract);
			if (classDataContract.BaseContract.IsISerializable)
			{
				if (IsISerializableDerived(typeName, rootSequence))
				{
					classDataContract.IsISerializable = true;
				}
				else
				{
					ThrowTypeCannotBeImportedException(classDataContract.StableName.Name, classDataContract.StableName.Namespace, SR.GetString("On type '{0}' in '{1}' namespace, derived type is not ISerializable.", baseTypeName.Name, baseTypeName.Namespace));
				}
			}
			if (classDataContract.BaseContract.IsReference)
			{
				classDataContract.IsReference = true;
			}
		}
		if (!classDataContract.IsISerializable)
		{
			classDataContract.Members = new List<DataMember>();
			RemoveOptionalUnknownSerializationElements(rootSequence.Items);
			for (int i = 0; i < rootSequence.Items.Count; i++)
			{
				XmlSchemaElement xmlSchemaElement = rootSequence.Items[i] as XmlSchemaElement;
				if (xmlSchemaElement == null)
				{
					ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString("Only local elements can be imported."));
				}
				ImportClassMember(xmlSchemaElement, classDataContract);
			}
		}
		return classDataContract;
	}

	[SecuritySafeCritical]
	private DataContract ImportXmlDataType(XmlQualifiedName typeName, XmlSchemaType xsdType, bool isAnonymous)
	{
		DataContract dataContract = dataContractSet[typeName];
		if (dataContract != null)
		{
			return dataContract;
		}
		XmlDataContract xmlDataContract = ImportSpecialXmlDataType(xsdType, isAnonymous);
		if (xmlDataContract != null)
		{
			return xmlDataContract;
		}
		xmlDataContract = new XmlDataContract();
		xmlDataContract.StableName = typeName;
		xmlDataContract.IsValueType = false;
		AddDataContract(xmlDataContract);
		if (xsdType != null)
		{
			ImportDataContractExtension(xsdType, xmlDataContract);
			xmlDataContract.IsValueType = IsValueType(typeName, xsdType.Annotation);
			xmlDataContract.IsTypeDefinedOnImport = true;
			xmlDataContract.XsdType = (isAnonymous ? xsdType : null);
			xmlDataContract.HasRoot = !IsXmlAnyElementType(xsdType as XmlSchemaComplexType);
		}
		else
		{
			xmlDataContract.IsValueType = true;
			xmlDataContract.IsTypeDefinedOnImport = false;
			xmlDataContract.HasRoot = true;
			if (DiagnosticUtility.ShouldTraceVerbose)
			{
				TraceUtility.Trace(TraceEventType.Verbose, 196623, SR.GetString("XSD import annotation failed"), new StringTraceRecord("Type", typeName.Namespace + ":" + typeName.Name));
			}
		}
		if (!isAnonymous)
		{
			xmlDataContract.SetTopLevelElementName(SchemaHelper.GetGlobalElementDeclaration(schemaSet, typeName, out var isNullable));
			xmlDataContract.IsTopLevelElementNullable = isNullable;
		}
		return xmlDataContract;
	}

	private XmlDataContract ImportSpecialXmlDataType(XmlSchemaType xsdType, bool isAnonymous)
	{
		if (!isAnonymous)
		{
			return null;
		}
		if (!(xsdType is XmlSchemaComplexType xsdType2))
		{
			return null;
		}
		if (IsXmlAnyElementType(xsdType2))
		{
			XmlQualifiedName stableName = new XmlQualifiedName("XElement", "http://schemas.datacontract.org/2004/07/System.Xml.Linq");
			if (dataContractSet.TryGetReferencedType(stableName, null, out var type) && Globals.TypeOfIXmlSerializable.IsAssignableFrom(type))
			{
				XmlDataContract xmlDataContract = new XmlDataContract(type);
				AddDataContract(xmlDataContract);
				return xmlDataContract;
			}
			return (XmlDataContract)DataContract.GetBuiltInDataContract(Globals.TypeOfXmlElement);
		}
		if (IsXmlAnyType(xsdType2))
		{
			return (XmlDataContract)DataContract.GetBuiltInDataContract(Globals.TypeOfXmlNodeArray);
		}
		return null;
	}

	private bool IsXmlAnyElementType(XmlSchemaComplexType xsdType)
	{
		if (xsdType == null)
		{
			return false;
		}
		if (!(xsdType.Particle is XmlSchemaSequence xmlSchemaSequence))
		{
			return false;
		}
		if (xmlSchemaSequence.Items == null || xmlSchemaSequence.Items.Count != 1)
		{
			return false;
		}
		if (!(xmlSchemaSequence.Items[0] is XmlSchemaAny { Namespace: null }))
		{
			return false;
		}
		if (xsdType.AnyAttribute != null || (xsdType.Attributes != null && xsdType.Attributes.Count > 0))
		{
			return false;
		}
		return true;
	}

	private bool IsXmlAnyType(XmlSchemaComplexType xsdType)
	{
		if (xsdType == null)
		{
			return false;
		}
		if (!(xsdType.Particle is XmlSchemaSequence xmlSchemaSequence))
		{
			return false;
		}
		if (xmlSchemaSequence.Items == null || xmlSchemaSequence.Items.Count != 1)
		{
			return false;
		}
		if (!(xmlSchemaSequence.Items[0] is XmlSchemaAny { Namespace: null } xmlSchemaAny))
		{
			return false;
		}
		if (xmlSchemaAny.MaxOccurs != decimal.MaxValue)
		{
			return false;
		}
		if (xsdType.AnyAttribute == null || xsdType.Attributes.Count > 0)
		{
			return false;
		}
		return true;
	}

	private bool IsValueType(XmlQualifiedName typeName, XmlSchemaAnnotation annotation)
	{
		string innerText = GetInnerText(typeName, ImportAnnotation(annotation, SchemaExporter.IsValueTypeName));
		if (innerText != null)
		{
			try
			{
				return XmlConvert.ToBoolean(innerText);
			}
			catch (FormatException ex)
			{
				ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString("IsValueType is formatted incorrectly as '{0}': {1}", innerText, ex.Message));
			}
		}
		return false;
	}

	[SecuritySafeCritical]
	private ClassDataContract ImportISerializable(XmlQualifiedName typeName, XmlSchemaSequence rootSequence, XmlQualifiedName baseTypeName, XmlSchemaObjectCollection attributes, XmlSchemaAnnotation annotation)
	{
		ClassDataContract classDataContract = new ClassDataContract();
		classDataContract.StableName = typeName;
		classDataContract.IsISerializable = true;
		AddDataContract(classDataContract);
		classDataContract.IsValueType = IsValueType(typeName, annotation);
		if (baseTypeName == null)
		{
			CheckISerializableBase(typeName, rootSequence, attributes);
		}
		else
		{
			ImportBaseContract(baseTypeName, classDataContract);
			if (!classDataContract.BaseContract.IsISerializable)
			{
				ThrowISerializableTypeCannotBeImportedException(classDataContract.StableName.Name, classDataContract.StableName.Namespace, SR.GetString("Base type '{0}' in '{1}' namespace is not ISerializable.", baseTypeName.Name, baseTypeName.Namespace));
			}
			if (!IsISerializableDerived(typeName, rootSequence))
			{
				ThrowISerializableTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString("Type derived from ISerializable cannot contain more than one item."));
			}
		}
		return classDataContract;
	}

	private void CheckISerializableBase(XmlQualifiedName typeName, XmlSchemaSequence rootSequence, XmlSchemaObjectCollection attributes)
	{
		if (rootSequence == null)
		{
			ThrowISerializableTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString("ISerializable does not contain any element."));
		}
		if (rootSequence.Items == null || rootSequence.Items.Count < 1)
		{
			ThrowISerializableTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString("ISerializable does not contain any element."));
		}
		else if (rootSequence.Items.Count > 1)
		{
			ThrowISerializableTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString("ISerializable cannot contain more than one item."));
		}
		XmlSchemaObject xmlSchemaObject = rootSequence.Items[0];
		if (!(xmlSchemaObject is XmlSchemaAny))
		{
			ThrowISerializableTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString("ISerializable does not contain any element."));
		}
		XmlSchemaAny xmlSchemaAny = (XmlSchemaAny)xmlSchemaObject;
		XmlSchemaAny iSerializableWildcardElement = SchemaExporter.ISerializableWildcardElement;
		if (xmlSchemaAny.MinOccurs != iSerializableWildcardElement.MinOccurs)
		{
			ThrowISerializableTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString("ISerializable wildcard maxOccurs must be '{0}'.", iSerializableWildcardElement.MinOccurs));
		}
		if (xmlSchemaAny.MaxOccursString != iSerializableWildcardElement.MaxOccursString)
		{
			ThrowISerializableTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString("ISerializable wildcard maxOccurs must be '{0}'.", iSerializableWildcardElement.MaxOccursString));
		}
		if (xmlSchemaAny.Namespace != iSerializableWildcardElement.Namespace)
		{
			ThrowISerializableTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString("ISerializable wildcard namespace is invalid: '{0}'.", iSerializableWildcardElement.Namespace));
		}
		if (xmlSchemaAny.ProcessContents != iSerializableWildcardElement.ProcessContents)
		{
			ThrowISerializableTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString("ISerializable wildcard processContents is invalid: '{0}'.", iSerializableWildcardElement.ProcessContents));
		}
		XmlQualifiedName refName = SchemaExporter.ISerializableFactoryTypeAttribute.RefName;
		bool flag = false;
		if (attributes != null)
		{
			for (int i = 0; i < attributes.Count; i++)
			{
				xmlSchemaObject = attributes[i];
				if (xmlSchemaObject is XmlSchemaAttribute && ((XmlSchemaAttribute)xmlSchemaObject).RefName == refName)
				{
					flag = true;
					break;
				}
			}
		}
		if (!flag)
		{
			ThrowISerializableTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString("ISerializable must have ref attribute that points to its factory type.", refName.Name, refName.Namespace));
		}
	}

	private bool IsISerializableDerived(XmlQualifiedName typeName, XmlSchemaSequence rootSequence)
	{
		if (rootSequence != null && rootSequence.Items != null)
		{
			return rootSequence.Items.Count == 0;
		}
		return true;
	}

	[SecuritySafeCritical]
	private void ImportBaseContract(XmlQualifiedName baseTypeName, ClassDataContract dataContract)
	{
		ClassDataContract classDataContract = ImportType(baseTypeName) as ClassDataContract;
		if (classDataContract == null)
		{
			ThrowTypeCannotBeImportedException(dataContract.StableName.Name, dataContract.StableName.Namespace, SR.GetString(dataContract.IsISerializable ? "Invalid ISerializable derivation from '{0}' in '{1}' namespace." : "Invalid class derivation from '{0}' in '{1}' namespace.", baseTypeName.Name, baseTypeName.Namespace));
		}
		if (classDataContract.IsValueType)
		{
			classDataContract.IsValueType = false;
		}
		for (ClassDataContract classDataContract2 = classDataContract; classDataContract2 != null; classDataContract2 = classDataContract2.BaseContract)
		{
			Dictionary<XmlQualifiedName, DataContract> dictionary = classDataContract2.KnownDataContracts;
			if (dictionary == null)
			{
				dictionary = (classDataContract2.KnownDataContracts = new Dictionary<XmlQualifiedName, DataContract>());
			}
			dictionary.Add(dataContract.StableName, dataContract);
		}
		dataContract.BaseContract = classDataContract;
	}

	private void ImportTopLevelElement(XmlQualifiedName typeName)
	{
		XmlSchemaElement schemaElement = SchemaHelper.GetSchemaElement(SchemaObjects, typeName);
		if (schemaElement == null)
		{
			return;
		}
		XmlQualifiedName xmlQualifiedName = schemaElement.SchemaTypeName;
		if (xmlQualifiedName.IsEmpty)
		{
			if (schemaElement.SchemaType != null)
			{
				ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString("Anonymous type is not supported. Type is '{0}' in '{1}' namespace.", typeName.Name, typeName.Namespace));
			}
			else
			{
				xmlQualifiedName = SchemaExporter.AnytypeQualifiedName;
			}
		}
		if (xmlQualifiedName != typeName)
		{
			ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString("Top-level element represents a different type. Expected '{0}' type in '{1}' namespace.", schemaElement.SchemaTypeName.Name, schemaElement.SchemaTypeName.Namespace));
		}
		CheckIfElementUsesUnsupportedConstructs(typeName, schemaElement);
	}

	private void ImportClassMember(XmlSchemaElement element, ClassDataContract dataContract)
	{
		XmlQualifiedName stableName = dataContract.StableName;
		if (element.MinOccurs > 1m)
		{
			ThrowTypeCannotBeImportedException(stableName.Name, stableName.Namespace, SR.GetString("On element '{0}', schema element minOccurs must be less or equal to 1.", element.Name));
		}
		if (element.MaxOccurs != 1m)
		{
			ThrowTypeCannotBeImportedException(stableName.Name, stableName.Namespace, SR.GetString("On element '{0}', schema element maxOccurs must be 1.", element.Name));
		}
		DataContract dataContract2 = null;
		string name = element.Name;
		bool isRequired = element.MinOccurs > 0m;
		bool isNillable = element.IsNillable;
		int order = 0;
		if (((element.Form == XmlSchemaForm.None) ? SchemaHelper.GetSchemaWithType(SchemaObjects, schemaSet, stableName).ElementFormDefault : element.Form) != XmlSchemaForm.Qualified)
		{
			ThrowTypeCannotBeImportedException(stableName.Name, stableName.Namespace, SR.GetString("On schema element '{0}', form must be qualified.", element.Name));
		}
		CheckIfElementUsesUnsupportedConstructs(stableName, element);
		if (element.SchemaTypeName.IsEmpty)
		{
			if (element.SchemaType != null)
			{
				dataContract2 = ImportAnonymousElement(element, new XmlQualifiedName(string.Format(CultureInfo.InvariantCulture, "{0}.{1}Type", stableName.Name, element.Name), stableName.Namespace));
			}
			else if (!element.RefName.IsEmpty)
			{
				ThrowTypeCannotBeImportedException(stableName.Name, stableName.Namespace, SR.GetString("For local element, ref is not supported. The referenced name is '{0}' in '{1}' namespace.", element.RefName.Name, element.RefName.Namespace));
			}
			else
			{
				dataContract2 = ImportType(SchemaExporter.AnytypeQualifiedName);
			}
		}
		else
		{
			XmlQualifiedName typeName = ImportActualType(element.Annotation, element.SchemaTypeName, stableName);
			dataContract2 = ImportType(typeName);
			if (IsObjectContract(dataContract2))
			{
				needToImportKnownTypesForObject = true;
			}
		}
		bool? flag = ImportEmitDefaultValue(element.Annotation, stableName);
		bool emitDefaultValue;
		if (!dataContract2.IsValueType && !isNillable)
		{
			if (flag.HasValue && flag.Value)
			{
				throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString("Invalid EmilDefault annotation for '{0}' in type '{1}' in '{2}' namespace.", name, stableName.Name, stableName.Namespace)));
			}
			emitDefaultValue = false;
		}
		else
		{
			emitDefaultValue = !flag.HasValue || flag.Value;
		}
		int num = dataContract.Members.Count - 1;
		if (num >= 0)
		{
			DataMember dataMember = dataContract.Members[num];
			if (dataMember.Order > 0)
			{
				order = dataContract.Members.Count;
			}
			DataMember y = new DataMember(dataContract2, name, isNillable, isRequired, emitDefaultValue, order);
			int num2 = ClassDataContract.DataMemberComparer.Singleton.Compare(dataMember, y);
			if (num2 == 0)
			{
				ThrowTypeCannotBeImportedException(stableName.Name, stableName.Namespace, SR.GetString("Cannot have duplicate element names '{0}'.", name));
			}
			else if (num2 > 0)
			{
				order = dataContract.Members.Count;
			}
		}
		DataMember dataMember2 = new DataMember(dataContract2, name, isNillable, isRequired, emitDefaultValue, order);
		XmlQualifiedName surrogateDataAnnotationName = SchemaExporter.SurrogateDataAnnotationName;
		dataContractSet.SetSurrogateData(dataMember2, ImportSurrogateData(ImportAnnotation(element.Annotation, surrogateDataAnnotationName), surrogateDataAnnotationName.Name, surrogateDataAnnotationName.Namespace));
		dataContract.Members.Add(dataMember2);
	}

	private bool? ImportEmitDefaultValue(XmlSchemaAnnotation annotation, XmlQualifiedName typeName)
	{
		XmlElement xmlElement = ImportAnnotation(annotation, SchemaExporter.DefaultValueAnnotation);
		if (xmlElement == null)
		{
			return null;
		}
		string text = xmlElement.Attributes.GetNamedItem("EmitDefaultValue")?.Value;
		if (text == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString("Annotation attribute was not found: default value annotation is '{0}', type is '{1}' in '{2}' namespace, emit default value is {3}.", SchemaExporter.DefaultValueAnnotation.Name, typeName.Name, typeName.Namespace, "EmitDefaultValue")));
		}
		return XmlConvert.ToBoolean(text);
	}

	internal static XmlQualifiedName ImportActualType(XmlSchemaAnnotation annotation, XmlQualifiedName defaultTypeName, XmlQualifiedName typeName)
	{
		XmlElement xmlElement = ImportAnnotation(annotation, SchemaExporter.ActualTypeAnnotationName);
		if (xmlElement == null)
		{
			return defaultTypeName;
		}
		string text = xmlElement.Attributes.GetNamedItem("Name")?.Value;
		if (text == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString("Annotation attribute was not found: default value annotation is '{0}', type is '{1}' in '{2}' namespace, emit default value is {3}.", SchemaExporter.ActualTypeAnnotationName.Name, typeName.Name, typeName.Namespace, "Name")));
		}
		string text2 = xmlElement.Attributes.GetNamedItem("Namespace")?.Value;
		if (text2 == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString("Annotation attribute was not found: default value annotation is '{0}', type is '{1}' in '{2}' namespace, emit default value is {3}.", SchemaExporter.ActualTypeAnnotationName.Name, typeName.Name, typeName.Namespace, "Namespace")));
		}
		return new XmlQualifiedName(text, text2);
	}

	[SecuritySafeCritical]
	private CollectionDataContract ImportCollection(XmlQualifiedName typeName, XmlSchemaSequence rootSequence, XmlSchemaObjectCollection attributes, XmlSchemaAnnotation annotation, bool isReference)
	{
		CollectionDataContract collectionDataContract = new CollectionDataContract(CollectionKind.Array);
		collectionDataContract.StableName = typeName;
		AddDataContract(collectionDataContract);
		collectionDataContract.IsReference = isReference;
		XmlSchemaElement xmlSchemaElement = (XmlSchemaElement)rootSequence.Items[0];
		collectionDataContract.IsItemTypeNullable = xmlSchemaElement.IsNillable;
		collectionDataContract.ItemName = xmlSchemaElement.Name;
		if (((xmlSchemaElement.Form == XmlSchemaForm.None) ? SchemaHelper.GetSchemaWithType(SchemaObjects, schemaSet, typeName).ElementFormDefault : xmlSchemaElement.Form) != XmlSchemaForm.Qualified)
		{
			ThrowArrayTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString("For array item, element 'form' must be {0}.", xmlSchemaElement.Name));
		}
		CheckIfElementUsesUnsupportedConstructs(typeName, xmlSchemaElement);
		if (xmlSchemaElement.SchemaTypeName.IsEmpty)
		{
			if (xmlSchemaElement.SchemaType != null)
			{
				XmlQualifiedName xmlQualifiedName = new XmlQualifiedName(xmlSchemaElement.Name, typeName.Namespace);
				if (dataContractSet[xmlQualifiedName] == null)
				{
					collectionDataContract.ItemContract = ImportAnonymousElement(xmlSchemaElement, xmlQualifiedName);
				}
				else
				{
					XmlQualifiedName typeQName = new XmlQualifiedName(string.Format(CultureInfo.InvariantCulture, "{0}.{1}Type", typeName.Name, xmlSchemaElement.Name), typeName.Namespace);
					collectionDataContract.ItemContract = ImportAnonymousElement(xmlSchemaElement, typeQName);
				}
			}
			else if (!xmlSchemaElement.RefName.IsEmpty)
			{
				ThrowArrayTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString("For local element, ref is not supported. The referenced name is '{0}' in '{1}' namespace.", xmlSchemaElement.RefName.Name, xmlSchemaElement.RefName.Namespace));
			}
			else
			{
				collectionDataContract.ItemContract = ImportType(SchemaExporter.AnytypeQualifiedName);
			}
		}
		else
		{
			collectionDataContract.ItemContract = ImportType(xmlSchemaElement.SchemaTypeName);
		}
		if (IsDictionary(typeName, annotation))
		{
			ClassDataContract classDataContract = collectionDataContract.ItemContract as ClassDataContract;
			DataMember dataMember = null;
			DataMember dataMember2 = null;
			if (classDataContract == null || classDataContract.Members == null || classDataContract.Members.Count != 2 || !(dataMember = classDataContract.Members[0]).IsRequired || !(dataMember2 = classDataContract.Members[1]).IsRequired)
			{
				ThrowArrayTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString("'{0}' is an invalid key value type.", xmlSchemaElement.Name));
			}
			if (classDataContract.Namespace != collectionDataContract.Namespace)
			{
				ThrowArrayTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString("'{0}' in '{1}' namespace is an invalid key value type.", xmlSchemaElement.Name, classDataContract.Namespace));
			}
			classDataContract.IsValueType = true;
			collectionDataContract.KeyName = dataMember.Name;
			collectionDataContract.ValueName = dataMember2.Name;
			if (xmlSchemaElement.SchemaType != null)
			{
				dataContractSet.Remove(classDataContract.StableName);
				GenericInfo genericInfo = new GenericInfo(DataContract.GetStableName(Globals.TypeOfKeyValue), Globals.TypeOfKeyValue.FullName);
				genericInfo.Add(GetGenericInfoForDataMember(dataMember));
				genericInfo.Add(GetGenericInfoForDataMember(dataMember2));
				genericInfo.AddToLevel(0, 2);
				collectionDataContract.ItemContract.StableName = new XmlQualifiedName(genericInfo.GetExpandedStableName().Name, typeName.Namespace);
			}
		}
		return collectionDataContract;
	}

	private GenericInfo GetGenericInfoForDataMember(DataMember dataMember)
	{
		GenericInfo genericInfo = null;
		if (dataMember.MemberTypeContract.IsValueType && dataMember.IsNullable)
		{
			genericInfo = new GenericInfo(DataContract.GetStableName(Globals.TypeOfNullable), Globals.TypeOfNullable.FullName);
			genericInfo.Add(new GenericInfo(dataMember.MemberTypeContract.StableName, null));
		}
		else
		{
			genericInfo = new GenericInfo(dataMember.MemberTypeContract.StableName, null);
		}
		return genericInfo;
	}

	private bool IsDictionary(XmlQualifiedName typeName, XmlSchemaAnnotation annotation)
	{
		string innerText = GetInnerText(typeName, ImportAnnotation(annotation, SchemaExporter.IsDictionaryAnnotationName));
		if (innerText != null)
		{
			try
			{
				return XmlConvert.ToBoolean(innerText);
			}
			catch (FormatException ex)
			{
				ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString("IsDictionary formatted value '{0}' is incorrect: {1}", innerText, ex.Message));
			}
		}
		return false;
	}

	private EnumDataContract ImportFlagsEnum(XmlQualifiedName typeName, XmlSchemaSimpleTypeList list, XmlSchemaAnnotation annotation)
	{
		XmlSchemaSimpleType itemType = list.ItemType;
		if (itemType == null)
		{
			ThrowEnumTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString("Enum list must contain an anonymous type."));
		}
		XmlSchemaSimpleTypeContent content = itemType.Content;
		if (content is XmlSchemaSimpleTypeUnion)
		{
			ThrowEnumTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString("Enum union in anonymous type is not supported."));
		}
		else if (content is XmlSchemaSimpleTypeList)
		{
			ThrowEnumTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString("Enum list in anonymous type is not supported."));
		}
		else if (content is XmlSchemaSimpleTypeRestriction)
		{
			XmlSchemaSimpleTypeRestriction restriction = (XmlSchemaSimpleTypeRestriction)content;
			if (CheckIfEnum(restriction))
			{
				return ImportEnum(typeName, restriction, isFlags: true, annotation);
			}
			ThrowEnumTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString("For simpleType restriction, only enum is supported and this type could not be convert to enum."));
		}
		return null;
	}

	[SecuritySafeCritical]
	private EnumDataContract ImportEnum(XmlQualifiedName typeName, XmlSchemaSimpleTypeRestriction restriction, bool isFlags, XmlSchemaAnnotation annotation)
	{
		EnumDataContract enumDataContract = new EnumDataContract();
		enumDataContract.StableName = typeName;
		enumDataContract.BaseContractName = ImportActualType(annotation, SchemaExporter.DefaultEnumBaseTypeName, typeName);
		enumDataContract.IsFlags = isFlags;
		AddDataContract(enumDataContract);
		enumDataContract.Values = new List<long>();
		enumDataContract.Members = new List<DataMember>();
		foreach (XmlSchemaFacet facet in restriction.Facets)
		{
			XmlSchemaEnumerationFacet xmlSchemaEnumerationFacet = facet as XmlSchemaEnumerationFacet;
			if (xmlSchemaEnumerationFacet == null)
			{
				ThrowEnumTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString("For schema facets, only enumeration is supported."));
			}
			if (xmlSchemaEnumerationFacet.Value == null)
			{
				ThrowEnumTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString("Schema enumeration facet must have values."));
			}
			string innerText = GetInnerText(typeName, ImportAnnotation(xmlSchemaEnumerationFacet.Annotation, SchemaExporter.EnumerationValueAnnotationName));
			if (innerText == null)
			{
				enumDataContract.Values.Add(SchemaExporter.GetDefaultEnumValue(isFlags, enumDataContract.Members.Count));
			}
			else
			{
				enumDataContract.Values.Add(enumDataContract.GetEnumValueFromString(innerText));
			}
			DataMember item = new DataMember(xmlSchemaEnumerationFacet.Value);
			enumDataContract.Members.Add(item);
		}
		return enumDataContract;
	}

	private DataContract ImportSimpleTypeRestriction(XmlQualifiedName typeName, XmlSchemaSimpleTypeRestriction restriction)
	{
		DataContract result = null;
		if (!restriction.BaseTypeName.IsEmpty)
		{
			result = ImportType(restriction.BaseTypeName);
		}
		else if (restriction.BaseType != null)
		{
			result = ImportType(restriction.BaseType);
		}
		else
		{
			ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString("This simpleType restriction does not specify the base type."));
		}
		return result;
	}

	private void ImportDataContractExtension(XmlSchemaType type, DataContract dataContract)
	{
		if (type.Annotation == null || type.Annotation.Items == null)
		{
			return;
		}
		foreach (XmlSchemaObject item in type.Annotation.Items)
		{
			if (!(item is XmlSchemaAppInfo { Markup: not null, Markup: var markup }))
			{
				continue;
			}
			for (int i = 0; i < markup.Length; i++)
			{
				XmlElement xmlElement = markup[i] as XmlElement;
				XmlQualifiedName surrogateDataAnnotationName = SchemaExporter.SurrogateDataAnnotationName;
				if (xmlElement != null && xmlElement.NamespaceURI == surrogateDataAnnotationName.Namespace && xmlElement.LocalName == surrogateDataAnnotationName.Name)
				{
					object surrogateData = ImportSurrogateData(xmlElement, surrogateDataAnnotationName.Name, surrogateDataAnnotationName.Namespace);
					dataContractSet.SetSurrogateData(dataContract, surrogateData);
				}
			}
		}
	}

	[SecuritySafeCritical]
	private void ImportGenericInfo(XmlSchemaType type, DataContract dataContract)
	{
		if (type.Annotation == null || type.Annotation.Items == null)
		{
			return;
		}
		foreach (XmlSchemaObject item in type.Annotation.Items)
		{
			if (!(item is XmlSchemaAppInfo { Markup: not null, Markup: var markup }))
			{
				continue;
			}
			for (int i = 0; i < markup.Length; i++)
			{
				if (markup[i] is XmlElement { NamespaceURI: "http://schemas.microsoft.com/2003/10/Serialization/", LocalName: "GenericType" } xmlElement)
				{
					dataContract.GenericInfo = ImportGenericInfo(xmlElement, type);
				}
			}
		}
	}

	private GenericInfo ImportGenericInfo(XmlElement typeElement, XmlSchemaType type)
	{
		string text = typeElement.Attributes.GetNamedItem("Name")?.Value;
		if (text == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString("On type '{0}' Generic annotation attribute '{1}' was not found.", type.Name, "Name")));
		}
		string text2 = typeElement.Attributes.GetNamedItem("Namespace")?.Value;
		if (text2 == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString("On type '{0}' Generic annotation attribute '{1}' was not found.", type.Name, "Namespace")));
		}
		if (typeElement.ChildNodes.Count > 0)
		{
			text = DataContract.EncodeLocalName(text);
		}
		int num = 0;
		GenericInfo genericInfo = new GenericInfo(new XmlQualifiedName(text, text2), type.Name);
		foreach (XmlNode childNode in typeElement.ChildNodes)
		{
			if (childNode is XmlElement xmlElement)
			{
				if (xmlElement.LocalName != "GenericParameter" || xmlElement.NamespaceURI != "http://schemas.microsoft.com/2003/10/Serialization/")
				{
					throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString("On type '{2}', generic annotation has invalid element. Argument element is '{0}' in '{1}' namespace.", xmlElement.LocalName, xmlElement.NamespaceURI, type.Name)));
				}
				XmlNode namedItem = xmlElement.Attributes.GetNamedItem("NestedLevel");
				int result = 0;
				if (namedItem != null && !int.TryParse(namedItem.Value, out result))
				{
					throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString("On type '{2}', generic annotation has invalid attribute value '{3}'. Argument element is '{0}' in '{1}' namespace. Nested level attribute attribute name is '{4}'. Type is '{5}'.", xmlElement.LocalName, xmlElement.NamespaceURI, type.Name, namedItem.Value, namedItem.LocalName, Globals.TypeOfInt.Name)));
				}
				if (result < num)
				{
					throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString("On type '{2}', generic annotation for nested level must be increasing. Argument element is '{0}' in '{1}' namespace.", xmlElement.LocalName, xmlElement.NamespaceURI, type.Name)));
				}
				genericInfo.Add(ImportGenericInfo(xmlElement, type));
				genericInfo.AddToLevel(result, 1);
				num = result;
			}
		}
		XmlNode namedItem2 = typeElement.Attributes.GetNamedItem("NestedLevel");
		if (namedItem2 != null)
		{
			int result2 = 0;
			if (!int.TryParse(namedItem2.Value, out result2))
			{
				throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString("On type '{2}', generic annotation has invalid attribute value '{3}'. Argument element is '{0}' in '{1}' namespace. Nested level attribute attribute name is '{4}'. Type is '{5}'.", typeElement.LocalName, typeElement.NamespaceURI, type.Name, namedItem2.Value, namedItem2.LocalName, Globals.TypeOfInt.Name)));
			}
			if (result2 - 1 > num)
			{
				genericInfo.AddToLevel(result2 - 1, 0);
			}
		}
		return genericInfo;
	}

	private object ImportSurrogateData(XmlElement typeElement, string name, string ns)
	{
		if (dataContractSet.DataContractSurrogate != null && typeElement != null)
		{
			Collection<Type> collection = new Collection<Type>();
			DataContractSurrogateCaller.GetKnownCustomDataTypes(dataContractSet.DataContractSurrogate, collection);
			return new DataContractSerializer(Globals.TypeOfObject, name, ns, collection, int.MaxValue, ignoreExtensionDataObject: false, preserveObjectReferences: true, null).ReadObject(new XmlNodeReader(typeElement));
		}
		return null;
	}

	private void CheckComplexType(XmlQualifiedName typeName, XmlSchemaComplexType type)
	{
		if (type.IsAbstract)
		{
			ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString("Abstract type is not supported"));
		}
		if (type.IsMixed)
		{
			ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString("Mixed content is not supported."));
		}
	}

	private void CheckIfElementUsesUnsupportedConstructs(XmlQualifiedName typeName, XmlSchemaElement element)
	{
		if (element.IsAbstract)
		{
			ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString("Abstract element '{0}' is not supported.", element.Name));
		}
		if (element.DefaultValue != null)
		{
			ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString("On element '{0}', default value is not supported.", element.Name));
		}
		if (element.FixedValue != null)
		{
			ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString("On schema element '{0}', fixed value is not supported.", element.Name));
		}
		if (!element.SubstitutionGroup.IsEmpty)
		{
			ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString("substitutionGroups on elements are not supported.", element.Name));
		}
	}

	private void ImportAttributes(XmlQualifiedName typeName, XmlSchemaObjectCollection attributes, XmlSchemaAnyAttribute anyAttribute, out bool isReference)
	{
		if (anyAttribute != null)
		{
			ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString("XML Schema 'any' attribute is not supported"));
		}
		isReference = false;
		if (attributes == null)
		{
			return;
		}
		bool foundAttribute = false;
		bool foundAttribute2 = false;
		for (int i = 0; i < attributes.Count; i++)
		{
			XmlSchemaObject xmlSchemaObject = attributes[i];
			if (xmlSchemaObject is XmlSchemaAttribute)
			{
				XmlSchemaAttribute xmlSchemaAttribute = (XmlSchemaAttribute)xmlSchemaObject;
				if (xmlSchemaAttribute.Use != XmlSchemaUse.Prohibited && !TryCheckIfAttribute(typeName, xmlSchemaAttribute, Globals.IdQualifiedName, ref foundAttribute) && !TryCheckIfAttribute(typeName, xmlSchemaAttribute, Globals.RefQualifiedName, ref foundAttribute2) && (xmlSchemaAttribute.RefName.IsEmpty || xmlSchemaAttribute.RefName.Namespace != "http://schemas.microsoft.com/2003/10/Serialization/" || xmlSchemaAttribute.Use == XmlSchemaUse.Required))
				{
					ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString("Type should not contain attributes. Serialization namespace: '{0}'.", "http://schemas.microsoft.com/2003/10/Serialization/"));
				}
			}
		}
		isReference = foundAttribute && foundAttribute2;
	}

	private bool TryCheckIfAttribute(XmlQualifiedName typeName, XmlSchemaAttribute attribute, XmlQualifiedName refName, ref bool foundAttribute)
	{
		if (attribute.RefName != refName)
		{
			return false;
		}
		if (foundAttribute)
		{
			ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString("Cannot have duplicate attribute names '{0}'.", refName.Name));
		}
		foundAttribute = true;
		return true;
	}

	private void AddDataContract(DataContract dataContract)
	{
		dataContractSet.Add(dataContract.StableName, dataContract);
	}

	private string GetInnerText(XmlQualifiedName typeName, XmlElement xmlElement)
	{
		if (xmlElement != null)
		{
			for (XmlNode xmlNode = xmlElement.FirstChild; xmlNode != null; xmlNode = xmlNode.NextSibling)
			{
				if (xmlNode.NodeType == XmlNodeType.Element)
				{
					ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, SR.GetString("For annotation element '{0}' in namespace '{1}', expected text but got element '{2}' in '{3}' namespace.", xmlElement.LocalName, xmlElement.NamespaceURI, xmlNode.LocalName, xmlNode.NamespaceURI));
				}
			}
			return xmlElement.InnerText;
		}
		return null;
	}

	private static XmlElement ImportAnnotation(XmlSchemaAnnotation annotation, XmlQualifiedName annotationQualifiedName)
	{
		if (annotation != null && annotation.Items != null && annotation.Items.Count > 0 && annotation.Items[0] is XmlSchemaAppInfo)
		{
			XmlNode[] markup = ((XmlSchemaAppInfo)annotation.Items[0]).Markup;
			if (markup != null)
			{
				for (int i = 0; i < markup.Length; i++)
				{
					if (markup[i] is XmlElement xmlElement && xmlElement.LocalName == annotationQualifiedName.Name && xmlElement.NamespaceURI == annotationQualifiedName.Namespace)
					{
						return xmlElement;
					}
				}
			}
		}
		return null;
	}

	private static void ThrowTypeCannotBeImportedException(string name, string ns, string message)
	{
		ThrowTypeCannotBeImportedException(SR.GetString("Type '{0}' in '{1}' namespace cannot be imported: {2}", name, ns, message));
	}

	private static void ThrowArrayTypeCannotBeImportedException(string name, string ns, string message)
	{
		ThrowTypeCannotBeImportedException(SR.GetString("Array type cannot be imported for '{0}' in '{1}' namespace: {2}.", name, ns, message));
	}

	private static void ThrowEnumTypeCannotBeImportedException(string name, string ns, string message)
	{
		ThrowTypeCannotBeImportedException(SR.GetString("For '{0}' in '{1}' namespace, enum type cannot be imported: {2}", name, ns, message));
	}

	private static void ThrowISerializableTypeCannotBeImportedException(string name, string ns, string message)
	{
		ThrowTypeCannotBeImportedException(SR.GetString("ISerializable type '{0}' in '{1}' namespace cannot be imported: {2}", name, ns, message));
	}

	private static void ThrowTypeCannotBeImportedException(string message)
	{
		throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString("Type cannot be imported: {0}", message)));
	}
}
