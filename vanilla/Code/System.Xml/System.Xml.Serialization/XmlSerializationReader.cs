using System.Collections;
using System.Configuration;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Xml.Serialization.Configuration;

namespace System.Xml.Serialization;

/// <summary>Controls deserialization by the <see cref="T:System.Xml.Serialization.XmlSerializer" /> class. </summary>
public abstract class XmlSerializationReader : XmlSerializationGeneratedCode
{
	private struct SoapArrayInfo
	{
		public string qname;

		public int dimensions;

		public int length;

		public int jaggedDimensions;
	}

	/// <summary>Holds an <see cref="T:System.Xml.Serialization.XmlSerializationFixupCallback" /> delegate instance, plus the method's inputs; also serves as the parameter for the method.</summary>
	protected class Fixup
	{
		private XmlSerializationFixupCallback callback;

		private object source;

		private string[] ids;

		/// <summary>Gets the callback method that creates an instance of the <see cref="T:System.Xml.Serialization.XmlSerializationFixupCallback" /> delegate.</summary>
		/// <returns>The callback method that creates an instance of the <see cref="T:System.Xml.Serialization.XmlSerializationFixupCallback" /> delegate.</returns>
		public XmlSerializationFixupCallback Callback => callback;

		/// <summary>Gets or sets the object that contains other objects whose values get filled in by the callback implementation.</summary>
		/// <returns>The source containing objects with values to fill.</returns>
		public object Source
		{
			get
			{
				return source;
			}
			set
			{
				source = value;
			}
		}

		/// <summary>Gets or sets an array of keys for the objects that belong to the <see cref="P:System.Xml.Serialization.XmlSerializationReader.Fixup.Source" /> property whose values get filled in by the callback implementation.</summary>
		/// <returns>The array of keys.</returns>
		public string[] Ids => ids;

		/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Serialization.XmlSerializationReader.Fixup" /> class.</summary>
		/// <param name="o">The object that contains other objects whose values get filled in by the callback implementation.</param>
		/// <param name="callback">A method that instantiates the <see cref="T:System.Xml.Serialization.XmlSerializationFixupCallback" /> delegate.</param>
		/// <param name="count">The size of the string array obtained through the <see cref="P:System.Xml.Serialization.XmlSerializationReader.Fixup.Ids" /> property.</param>
		public Fixup(object o, XmlSerializationFixupCallback callback, int count)
			: this(o, callback, new string[count])
		{
		}

		/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Serialization.XmlSerializationReader.Fixup" /> class.</summary>
		/// <param name="o">The object that contains other objects whose values get filled in by the callback implementation.</param>
		/// <param name="callback">A method that instantiates the <see cref="T:System.Xml.Serialization.XmlSerializationFixupCallback" /> delegate.</param>
		/// <param name="ids">The string array obtained through the <see cref="P:System.Xml.Serialization.XmlSerializationReader.Fixup.Ids" /> property.</param>
		public Fixup(object o, XmlSerializationFixupCallback callback, string[] ids)
		{
			this.callback = callback;
			Source = o;
			this.ids = ids;
		}
	}

	/// <summary>Holds an <see cref="T:System.Xml.Serialization.XmlSerializationCollectionFixupCallback" /> delegate instance, plus the method's inputs; also supplies the method's parameters. </summary>
	protected class CollectionFixup
	{
		private XmlSerializationCollectionFixupCallback callback;

		private object collection;

		private object collectionItems;

		/// <summary>Gets the callback method that instantiates the <see cref="T:System.Xml.Serialization.XmlSerializationCollectionFixupCallback" /> delegate. </summary>
		/// <returns>The <see cref="T:System.Xml.Serialization.XmlSerializationCollectionFixupCallback" /> delegate that points to the callback method.</returns>
		public XmlSerializationCollectionFixupCallback Callback => callback;

		/// <summary>Gets the <paramref name="object collection" /> for the callback method. </summary>
		/// <returns>The collection that is used for the fixup.</returns>
		public object Collection => collection;

		/// <summary>Gets the array into which the callback method copies a collection. </summary>
		/// <returns>The array into which the callback method copies a collection.</returns>
		public object CollectionItems => collectionItems;

		/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Serialization.XmlSerializationReader.CollectionFixup" /> class with parameters for a callback method. </summary>
		/// <param name="collection">A collection into which the callback method copies the collection items array.</param>
		/// <param name="callback">A method that instantiates the <see cref="T:System.Xml.Serialization.XmlSerializationCollectionFixupCallback" /> delegate.</param>
		/// <param name="collectionItems">An array into which the callback method copies a collection.</param>
		public CollectionFixup(object collection, XmlSerializationCollectionFixupCallback callback, object collectionItems)
		{
			this.callback = callback;
			this.collection = collection;
			this.collectionItems = collectionItems;
		}
	}

	private XmlReader r;

	private XmlCountingReader countingReader;

	private XmlDocument d;

	private Hashtable callbacks;

	private Hashtable types;

	private Hashtable typesReverse;

	private XmlDeserializationEvents events;

	private Hashtable targets;

	private Hashtable referencedTargets;

	private ArrayList targetsWithoutIds;

	private ArrayList fixups;

	private ArrayList collectionFixups;

	private bool soap12;

	private bool isReturnValue;

	private bool decodeName = true;

	private string schemaNsID;

	private string schemaNs1999ID;

	private string schemaNs2000ID;

	private string schemaNonXsdTypesNsID;

	private string instanceNsID;

	private string instanceNs2000ID;

	private string instanceNs1999ID;

	private string soapNsID;

	private string soap12NsID;

	private string schemaID;

	private string wsdlNsID;

	private string wsdlArrayTypeID;

	private string nullID;

	private string nilID;

	private string typeID;

	private string arrayTypeID;

	private string itemTypeID;

	private string arraySizeID;

	private string arrayID;

	private string urTypeID;

	private string stringID;

	private string intID;

	private string booleanID;

	private string shortID;

	private string longID;

	private string floatID;

	private string doubleID;

	private string decimalID;

	private string dateTimeID;

	private string qnameID;

	private string dateID;

	private string timeID;

	private string hexBinaryID;

	private string base64BinaryID;

	private string base64ID;

	private string unsignedByteID;

	private string byteID;

	private string unsignedShortID;

	private string unsignedIntID;

	private string unsignedLongID;

	private string oldDecimalID;

	private string oldTimeInstantID;

	private string anyURIID;

	private string durationID;

	private string ENTITYID;

	private string ENTITIESID;

	private string gDayID;

	private string gMonthID;

	private string gMonthDayID;

	private string gYearID;

	private string gYearMonthID;

	private string IDID;

	private string IDREFID;

	private string IDREFSID;

	private string integerID;

	private string languageID;

	private string NameID;

	private string NCNameID;

	private string NMTOKENID;

	private string NMTOKENSID;

	private string negativeIntegerID;

	private string nonPositiveIntegerID;

	private string nonNegativeIntegerID;

	private string normalizedStringID;

	private string NOTATIONID;

	private string positiveIntegerID;

	private string tokenID;

	private string charID;

	private string guidID;

	private string timeSpanID;

	private static bool checkDeserializeAdvances;

	/// <summary>Gets or sets a value that determines whether XML strings are translated into valid .NET Framework type names.</summary>
	/// <returns>true if XML strings are decoded into valid .NET Framework type names; otherwise, false.</returns>
	protected bool DecodeName
	{
		get
		{
			return decodeName;
		}
		set
		{
			decodeName = value;
		}
	}

	/// <summary>Gets the <see cref="T:System.Xml.XmlReader" /> object that is being used by <see cref="T:System.Xml.Serialization.XmlSerializationReader" />. </summary>
	/// <returns>The <see cref="T:System.Xml.XmlReader" /> that is being used by the <see cref="T:System.Xml.Serialization.XmlSerializationReader" />.</returns>
	protected XmlReader Reader => r;

	/// <summary>Gets the current count of the <see cref="T:System.Xml.XmlReader" />.</summary>
	/// <returns>The current count of an <see cref="T:System.Xml.XmlReader" />.</returns>
	protected int ReaderCount
	{
		get
		{
			if (!checkDeserializeAdvances)
			{
				return 0;
			}
			return countingReader.AdvanceCount;
		}
	}

	/// <summary>Gets the XML document object into which the XML document is being deserialized. </summary>
	/// <returns>An <see cref="T:System.Xml.XmlDocument" /> that represents the deserialized <see cref="T:System.Xml.XmlDocument" /> data.</returns>
	protected XmlDocument Document
	{
		get
		{
			if (d == null)
			{
				d = new XmlDocument(r.NameTable);
				d.SetBaseURI(r.BaseURI);
			}
			return d;
		}
	}

	/// <summary>Gets or sets a value that should be true for a SOAP 1.1 return value.</summary>
	/// <returns>true, if the value is a return value. </returns>
	protected bool IsReturnValue
	{
		get
		{
			if (isReturnValue)
			{
				return !soap12;
			}
			return false;
		}
		set
		{
			isReturnValue = value;
		}
	}

	static XmlSerializationReader()
	{
		checkDeserializeAdvances = ConfigurationManager.GetSection(ConfigurationStrings.XmlSerializerSectionPath) is XmlSerializerSection xmlSerializerSection && xmlSerializerSection.CheckDeserializeAdvances;
	}

	/// <summary>Stores element and attribute names in a <see cref="T:System.Xml.NameTable" /> object. </summary>
	protected abstract void InitIDs();

	internal void Init(XmlReader r, XmlDeserializationEvents events, string encodingStyle, TempAssembly tempAssembly)
	{
		this.events = events;
		if (checkDeserializeAdvances)
		{
			countingReader = new XmlCountingReader(r);
			this.r = countingReader;
		}
		else
		{
			this.r = r;
		}
		d = null;
		soap12 = encodingStyle == "http://www.w3.org/2003/05/soap-encoding";
		Init(tempAssembly);
		schemaNsID = r.NameTable.Add("http://www.w3.org/2001/XMLSchema");
		schemaNs2000ID = r.NameTable.Add("http://www.w3.org/2000/10/XMLSchema");
		schemaNs1999ID = r.NameTable.Add("http://www.w3.org/1999/XMLSchema");
		schemaNonXsdTypesNsID = r.NameTable.Add("http://microsoft.com/wsdl/types/");
		instanceNsID = r.NameTable.Add("http://www.w3.org/2001/XMLSchema-instance");
		instanceNs2000ID = r.NameTable.Add("http://www.w3.org/2000/10/XMLSchema-instance");
		instanceNs1999ID = r.NameTable.Add("http://www.w3.org/1999/XMLSchema-instance");
		soapNsID = r.NameTable.Add("http://schemas.xmlsoap.org/soap/encoding/");
		soap12NsID = r.NameTable.Add("http://www.w3.org/2003/05/soap-encoding");
		schemaID = r.NameTable.Add("schema");
		wsdlNsID = r.NameTable.Add("http://schemas.xmlsoap.org/wsdl/");
		wsdlArrayTypeID = r.NameTable.Add("arrayType");
		nullID = r.NameTable.Add("null");
		nilID = r.NameTable.Add("nil");
		typeID = r.NameTable.Add("type");
		arrayTypeID = r.NameTable.Add("arrayType");
		itemTypeID = r.NameTable.Add("itemType");
		arraySizeID = r.NameTable.Add("arraySize");
		arrayID = r.NameTable.Add("Array");
		urTypeID = r.NameTable.Add("anyType");
		InitIDs();
	}

	/// <summary>Gets a dynamically generated assembly by name.</summary>
	/// <returns>A dynamically generated <see cref="T:System.Reflection.Assembly" />.</returns>
	/// <param name="assemblyFullName">The full name of the assembly.</param>
	protected static Assembly ResolveDynamicAssembly(string assemblyFullName)
	{
		return DynamicAssemblies.Get(assemblyFullName);
	}

	private void InitPrimitiveIDs()
	{
		if (tokenID == null)
		{
			r.NameTable.Add("http://www.w3.org/2001/XMLSchema");
			r.NameTable.Add("http://microsoft.com/wsdl/types/");
			stringID = r.NameTable.Add("string");
			intID = r.NameTable.Add("int");
			booleanID = r.NameTable.Add("boolean");
			shortID = r.NameTable.Add("short");
			longID = r.NameTable.Add("long");
			floatID = r.NameTable.Add("float");
			doubleID = r.NameTable.Add("double");
			decimalID = r.NameTable.Add("decimal");
			dateTimeID = r.NameTable.Add("dateTime");
			qnameID = r.NameTable.Add("QName");
			dateID = r.NameTable.Add("date");
			timeID = r.NameTable.Add("time");
			hexBinaryID = r.NameTable.Add("hexBinary");
			base64BinaryID = r.NameTable.Add("base64Binary");
			unsignedByteID = r.NameTable.Add("unsignedByte");
			byteID = r.NameTable.Add("byte");
			unsignedShortID = r.NameTable.Add("unsignedShort");
			unsignedIntID = r.NameTable.Add("unsignedInt");
			unsignedLongID = r.NameTable.Add("unsignedLong");
			oldDecimalID = r.NameTable.Add("decimal");
			oldTimeInstantID = r.NameTable.Add("timeInstant");
			charID = r.NameTable.Add("char");
			guidID = r.NameTable.Add("guid");
			if (System.LocalAppContextSwitches.EnableTimeSpanSerialization)
			{
				timeSpanID = r.NameTable.Add("TimeSpan");
			}
			base64ID = r.NameTable.Add("base64");
			anyURIID = r.NameTable.Add("anyURI");
			durationID = r.NameTable.Add("duration");
			ENTITYID = r.NameTable.Add("ENTITY");
			ENTITIESID = r.NameTable.Add("ENTITIES");
			gDayID = r.NameTable.Add("gDay");
			gMonthID = r.NameTable.Add("gMonth");
			gMonthDayID = r.NameTable.Add("gMonthDay");
			gYearID = r.NameTable.Add("gYear");
			gYearMonthID = r.NameTable.Add("gYearMonth");
			IDID = r.NameTable.Add("ID");
			IDREFID = r.NameTable.Add("IDREF");
			IDREFSID = r.NameTable.Add("IDREFS");
			integerID = r.NameTable.Add("integer");
			languageID = r.NameTable.Add("language");
			NameID = r.NameTable.Add("Name");
			NCNameID = r.NameTable.Add("NCName");
			NMTOKENID = r.NameTable.Add("NMTOKEN");
			NMTOKENSID = r.NameTable.Add("NMTOKENS");
			negativeIntegerID = r.NameTable.Add("negativeInteger");
			nonNegativeIntegerID = r.NameTable.Add("nonNegativeInteger");
			nonPositiveIntegerID = r.NameTable.Add("nonPositiveInteger");
			normalizedStringID = r.NameTable.Add("normalizedString");
			NOTATIONID = r.NameTable.Add("NOTATION");
			positiveIntegerID = r.NameTable.Add("positiveInteger");
			tokenID = r.NameTable.Add("token");
		}
	}

	/// <summary>Gets the value of the xsi:type attribute for the XML element at the current location of the <see cref="T:System.Xml.XmlReader" />. </summary>
	/// <returns>An XML qualified name that indicates the data type of an XML element.</returns>
	protected XmlQualifiedName GetXsiType()
	{
		string attribute = r.GetAttribute(typeID, instanceNsID);
		if (attribute == null)
		{
			attribute = r.GetAttribute(typeID, instanceNs2000ID);
			if (attribute == null)
			{
				attribute = r.GetAttribute(typeID, instanceNs1999ID);
				if (attribute == null)
				{
					return null;
				}
			}
		}
		return ToXmlQualifiedName(attribute, decodeName: false);
	}

	private Type GetPrimitiveType(XmlQualifiedName typeName, bool throwOnUnknown)
	{
		InitPrimitiveIDs();
		if ((object)typeName.Namespace == schemaNsID || (object)typeName.Namespace == soapNsID || (object)typeName.Namespace == soap12NsID)
		{
			if ((object)typeName.Name == stringID || (object)typeName.Name == anyURIID || (object)typeName.Name == durationID || (object)typeName.Name == ENTITYID || (object)typeName.Name == ENTITIESID || (object)typeName.Name == gDayID || (object)typeName.Name == gMonthID || (object)typeName.Name == gMonthDayID || (object)typeName.Name == gYearID || (object)typeName.Name == gYearMonthID || (object)typeName.Name == IDID || (object)typeName.Name == IDREFID || (object)typeName.Name == IDREFSID || (object)typeName.Name == integerID || (object)typeName.Name == languageID || (object)typeName.Name == NameID || (object)typeName.Name == NCNameID || (object)typeName.Name == NMTOKENID || (object)typeName.Name == NMTOKENSID || (object)typeName.Name == negativeIntegerID || (object)typeName.Name == nonPositiveIntegerID || (object)typeName.Name == nonNegativeIntegerID || (object)typeName.Name == normalizedStringID || (object)typeName.Name == NOTATIONID || (object)typeName.Name == positiveIntegerID || (object)typeName.Name == tokenID)
			{
				return typeof(string);
			}
			if ((object)typeName.Name == intID)
			{
				return typeof(int);
			}
			if ((object)typeName.Name == booleanID)
			{
				return typeof(bool);
			}
			if ((object)typeName.Name == shortID)
			{
				return typeof(short);
			}
			if ((object)typeName.Name == longID)
			{
				return typeof(long);
			}
			if ((object)typeName.Name == floatID)
			{
				return typeof(float);
			}
			if ((object)typeName.Name == doubleID)
			{
				return typeof(double);
			}
			if ((object)typeName.Name == decimalID)
			{
				return typeof(decimal);
			}
			if ((object)typeName.Name == dateTimeID)
			{
				return typeof(DateTime);
			}
			if ((object)typeName.Name == qnameID)
			{
				return typeof(XmlQualifiedName);
			}
			if ((object)typeName.Name == dateID)
			{
				return typeof(DateTime);
			}
			if ((object)typeName.Name == timeID)
			{
				return typeof(DateTime);
			}
			if ((object)typeName.Name == hexBinaryID)
			{
				return typeof(byte[]);
			}
			if ((object)typeName.Name == base64BinaryID)
			{
				return typeof(byte[]);
			}
			if ((object)typeName.Name == unsignedByteID)
			{
				return typeof(byte);
			}
			if ((object)typeName.Name == byteID)
			{
				return typeof(sbyte);
			}
			if ((object)typeName.Name == unsignedShortID)
			{
				return typeof(ushort);
			}
			if ((object)typeName.Name == unsignedIntID)
			{
				return typeof(uint);
			}
			if ((object)typeName.Name == unsignedLongID)
			{
				return typeof(ulong);
			}
			throw CreateUnknownTypeException(typeName);
		}
		if ((object)typeName.Namespace == schemaNs2000ID || (object)typeName.Namespace == schemaNs1999ID)
		{
			if ((object)typeName.Name == stringID || (object)typeName.Name == anyURIID || (object)typeName.Name == durationID || (object)typeName.Name == ENTITYID || (object)typeName.Name == ENTITIESID || (object)typeName.Name == gDayID || (object)typeName.Name == gMonthID || (object)typeName.Name == gMonthDayID || (object)typeName.Name == gYearID || (object)typeName.Name == gYearMonthID || (object)typeName.Name == IDID || (object)typeName.Name == IDREFID || (object)typeName.Name == IDREFSID || (object)typeName.Name == integerID || (object)typeName.Name == languageID || (object)typeName.Name == NameID || (object)typeName.Name == NCNameID || (object)typeName.Name == NMTOKENID || (object)typeName.Name == NMTOKENSID || (object)typeName.Name == negativeIntegerID || (object)typeName.Name == nonPositiveIntegerID || (object)typeName.Name == nonNegativeIntegerID || (object)typeName.Name == normalizedStringID || (object)typeName.Name == NOTATIONID || (object)typeName.Name == positiveIntegerID || (object)typeName.Name == tokenID)
			{
				return typeof(string);
			}
			if ((object)typeName.Name == intID)
			{
				return typeof(int);
			}
			if ((object)typeName.Name == booleanID)
			{
				return typeof(bool);
			}
			if ((object)typeName.Name == shortID)
			{
				return typeof(short);
			}
			if ((object)typeName.Name == longID)
			{
				return typeof(long);
			}
			if ((object)typeName.Name == floatID)
			{
				return typeof(float);
			}
			if ((object)typeName.Name == doubleID)
			{
				return typeof(double);
			}
			if ((object)typeName.Name == oldDecimalID)
			{
				return typeof(decimal);
			}
			if ((object)typeName.Name == oldTimeInstantID)
			{
				return typeof(DateTime);
			}
			if ((object)typeName.Name == qnameID)
			{
				return typeof(XmlQualifiedName);
			}
			if ((object)typeName.Name == dateID)
			{
				return typeof(DateTime);
			}
			if ((object)typeName.Name == timeID)
			{
				return typeof(DateTime);
			}
			if ((object)typeName.Name == hexBinaryID)
			{
				return typeof(byte[]);
			}
			if ((object)typeName.Name == byteID)
			{
				return typeof(sbyte);
			}
			if ((object)typeName.Name == unsignedShortID)
			{
				return typeof(ushort);
			}
			if ((object)typeName.Name == unsignedIntID)
			{
				return typeof(uint);
			}
			if ((object)typeName.Name == unsignedLongID)
			{
				return typeof(ulong);
			}
			throw CreateUnknownTypeException(typeName);
		}
		if ((object)typeName.Namespace == schemaNonXsdTypesNsID)
		{
			if ((object)typeName.Name == charID)
			{
				return typeof(char);
			}
			if ((object)typeName.Name == guidID)
			{
				return typeof(Guid);
			}
			throw CreateUnknownTypeException(typeName);
		}
		if (throwOnUnknown)
		{
			throw CreateUnknownTypeException(typeName);
		}
		return null;
	}

	private bool IsPrimitiveNamespace(string ns)
	{
		if ((object)ns != schemaNsID && (object)ns != schemaNonXsdTypesNsID && (object)ns != soapNsID && (object)ns != soap12NsID && (object)ns != schemaNs2000ID)
		{
			return (object)ns == schemaNs1999ID;
		}
		return true;
	}

	private string ReadStringValue()
	{
		if (r.IsEmptyElement)
		{
			r.Skip();
			return string.Empty;
		}
		r.ReadStartElement();
		string result = r.ReadString();
		ReadEndElement();
		return result;
	}

	private XmlQualifiedName ReadXmlQualifiedName()
	{
		bool flag = false;
		string value;
		if (r.IsEmptyElement)
		{
			value = string.Empty;
			flag = true;
		}
		else
		{
			r.ReadStartElement();
			value = r.ReadString();
		}
		XmlQualifiedName result = ToXmlQualifiedName(value);
		if (flag)
		{
			r.Skip();
			return result;
		}
		ReadEndElement();
		return result;
	}

	private byte[] ReadByteArray(bool isBase64)
	{
		ArrayList arrayList = new ArrayList();
		int num = 1024;
		int num2 = -1;
		int num3 = 0;
		int num4 = 0;
		byte[] array = new byte[num];
		arrayList.Add(array);
		while (num2 != 0)
		{
			if (num3 == array.Length)
			{
				num = Math.Min(num * 2, 65536);
				array = new byte[num];
				num3 = 0;
				arrayList.Add(array);
			}
			num2 = ((!isBase64) ? r.ReadElementContentAsBinHex(array, num3, array.Length - num3) : r.ReadElementContentAsBase64(array, num3, array.Length - num3));
			num3 += num2;
			num4 += num2;
		}
		byte[] array2 = new byte[num4];
		num3 = 0;
		foreach (byte[] item in arrayList)
		{
			num = Math.Min(item.Length, num4);
			if (num > 0)
			{
				Buffer.BlockCopy(item, 0, array2, num3, num);
				num3 += num;
				num4 -= num;
			}
		}
		arrayList.Clear();
		return array2;
	}

	/// <summary>Gets the value of the XML node at which the <see cref="T:System.Xml.XmlReader" /> is currently positioned. </summary>
	/// <returns>The value of the node as a .NET Framework value type, if the value is a simple XML Schema data type.</returns>
	/// <param name="type">The <see cref="T:System.Xml.XmlQualifiedName" /> that represents the simple data type for the current location of the <see cref="T:System.Xml.XmlReader" />.</param>
	protected object ReadTypedPrimitive(XmlQualifiedName type)
	{
		return ReadTypedPrimitive(type, elementCanBeType: false);
	}

	private object ReadTypedPrimitive(XmlQualifiedName type, bool elementCanBeType)
	{
		InitPrimitiveIDs();
		object obj = null;
		if (!IsPrimitiveNamespace(type.Namespace) || (object)type.Name == urTypeID)
		{
			return ReadXmlNodes(elementCanBeType);
		}
		if ((object)type.Namespace == schemaNsID || (object)type.Namespace == soapNsID || (object)type.Namespace == soap12NsID)
		{
			if ((object)type.Name == stringID || (object)type.Name == normalizedStringID)
			{
				return ReadStringValue();
			}
			if ((object)type.Name == anyURIID || (object)type.Name == durationID || (object)type.Name == ENTITYID || (object)type.Name == ENTITIESID || (object)type.Name == gDayID || (object)type.Name == gMonthID || (object)type.Name == gMonthDayID || (object)type.Name == gYearID || (object)type.Name == gYearMonthID || (object)type.Name == IDID || (object)type.Name == IDREFID || (object)type.Name == IDREFSID || (object)type.Name == integerID || (object)type.Name == languageID || (object)type.Name == NameID || (object)type.Name == NCNameID || (object)type.Name == NMTOKENID || (object)type.Name == NMTOKENSID || (object)type.Name == negativeIntegerID || (object)type.Name == nonPositiveIntegerID || (object)type.Name == nonNegativeIntegerID || (object)type.Name == NOTATIONID || (object)type.Name == positiveIntegerID || (object)type.Name == tokenID)
			{
				return CollapseWhitespace(ReadStringValue());
			}
			if ((object)type.Name == intID)
			{
				return XmlConvert.ToInt32(ReadStringValue());
			}
			if ((object)type.Name == booleanID)
			{
				return XmlConvert.ToBoolean(ReadStringValue());
			}
			if ((object)type.Name == shortID)
			{
				return XmlConvert.ToInt16(ReadStringValue());
			}
			if ((object)type.Name == longID)
			{
				return XmlConvert.ToInt64(ReadStringValue());
			}
			if ((object)type.Name == floatID)
			{
				return XmlConvert.ToSingle(ReadStringValue());
			}
			if ((object)type.Name == doubleID)
			{
				return XmlConvert.ToDouble(ReadStringValue());
			}
			if ((object)type.Name == decimalID)
			{
				return XmlConvert.ToDecimal(ReadStringValue());
			}
			if ((object)type.Name == dateTimeID)
			{
				return ToDateTime(ReadStringValue());
			}
			if ((object)type.Name == qnameID)
			{
				return ReadXmlQualifiedName();
			}
			if ((object)type.Name == dateID)
			{
				return ToDate(ReadStringValue());
			}
			if ((object)type.Name == timeID)
			{
				return ToTime(ReadStringValue());
			}
			if ((object)type.Name == unsignedByteID)
			{
				return XmlConvert.ToByte(ReadStringValue());
			}
			if ((object)type.Name == byteID)
			{
				return XmlConvert.ToSByte(ReadStringValue());
			}
			if ((object)type.Name == unsignedShortID)
			{
				return XmlConvert.ToUInt16(ReadStringValue());
			}
			if ((object)type.Name == unsignedIntID)
			{
				return XmlConvert.ToUInt32(ReadStringValue());
			}
			if ((object)type.Name == unsignedLongID)
			{
				return XmlConvert.ToUInt64(ReadStringValue());
			}
			if ((object)type.Name == hexBinaryID)
			{
				return ToByteArrayHex(isNull: false);
			}
			if ((object)type.Name == base64BinaryID)
			{
				return ToByteArrayBase64(isNull: false);
			}
			if ((object)type.Name == base64ID && ((object)type.Namespace == soapNsID || (object)type.Namespace == soap12NsID))
			{
				return ToByteArrayBase64(isNull: false);
			}
			return ReadXmlNodes(elementCanBeType);
		}
		if ((object)type.Namespace == schemaNs2000ID || (object)type.Namespace == schemaNs1999ID)
		{
			if ((object)type.Name == stringID || (object)type.Name == normalizedStringID)
			{
				return ReadStringValue();
			}
			if ((object)type.Name == anyURIID || (object)type.Name == anyURIID || (object)type.Name == durationID || (object)type.Name == ENTITYID || (object)type.Name == ENTITIESID || (object)type.Name == gDayID || (object)type.Name == gMonthID || (object)type.Name == gMonthDayID || (object)type.Name == gYearID || (object)type.Name == gYearMonthID || (object)type.Name == IDID || (object)type.Name == IDREFID || (object)type.Name == IDREFSID || (object)type.Name == integerID || (object)type.Name == languageID || (object)type.Name == NameID || (object)type.Name == NCNameID || (object)type.Name == NMTOKENID || (object)type.Name == NMTOKENSID || (object)type.Name == negativeIntegerID || (object)type.Name == nonPositiveIntegerID || (object)type.Name == nonNegativeIntegerID || (object)type.Name == NOTATIONID || (object)type.Name == positiveIntegerID || (object)type.Name == tokenID)
			{
				return CollapseWhitespace(ReadStringValue());
			}
			if ((object)type.Name == intID)
			{
				return XmlConvert.ToInt32(ReadStringValue());
			}
			if ((object)type.Name == booleanID)
			{
				return XmlConvert.ToBoolean(ReadStringValue());
			}
			if ((object)type.Name == shortID)
			{
				return XmlConvert.ToInt16(ReadStringValue());
			}
			if ((object)type.Name == longID)
			{
				return XmlConvert.ToInt64(ReadStringValue());
			}
			if ((object)type.Name == floatID)
			{
				return XmlConvert.ToSingle(ReadStringValue());
			}
			if ((object)type.Name == doubleID)
			{
				return XmlConvert.ToDouble(ReadStringValue());
			}
			if ((object)type.Name == oldDecimalID)
			{
				return XmlConvert.ToDecimal(ReadStringValue());
			}
			if ((object)type.Name == oldTimeInstantID)
			{
				return ToDateTime(ReadStringValue());
			}
			if ((object)type.Name == qnameID)
			{
				return ReadXmlQualifiedName();
			}
			if ((object)type.Name == dateID)
			{
				return ToDate(ReadStringValue());
			}
			if ((object)type.Name == timeID)
			{
				return ToTime(ReadStringValue());
			}
			if ((object)type.Name == unsignedByteID)
			{
				return XmlConvert.ToByte(ReadStringValue());
			}
			if ((object)type.Name == byteID)
			{
				return XmlConvert.ToSByte(ReadStringValue());
			}
			if ((object)type.Name == unsignedShortID)
			{
				return XmlConvert.ToUInt16(ReadStringValue());
			}
			if ((object)type.Name == unsignedIntID)
			{
				return XmlConvert.ToUInt32(ReadStringValue());
			}
			if ((object)type.Name == unsignedLongID)
			{
				return XmlConvert.ToUInt64(ReadStringValue());
			}
			return ReadXmlNodes(elementCanBeType);
		}
		if ((object)type.Namespace == schemaNonXsdTypesNsID)
		{
			if ((object)type.Name == charID)
			{
				return ToChar(ReadStringValue());
			}
			if ((object)type.Name == guidID)
			{
				return new Guid(CollapseWhitespace(ReadStringValue()));
			}
			if ((object)type.Name == timeSpanID && System.LocalAppContextSwitches.EnableTimeSpanSerialization)
			{
				return XmlConvert.ToTimeSpan(ReadStringValue());
			}
			return ReadXmlNodes(elementCanBeType);
		}
		return ReadXmlNodes(elementCanBeType);
	}

	/// <summary>Reads an XML element that allows null values (xsi:nil = 'true') and returns a generic <see cref="T:System.Nullable`1" /> value. </summary>
	/// <returns>A generic <see cref="T:System.Nullable`1" /> that represents a null XML value.</returns>
	/// <param name="type">The <see cref="T:System.Xml.XmlQualifiedName" /> that represents the simple data type for the current location of the <see cref="T:System.Xml.XmlReader" />.</param>
	protected object ReadTypedNull(XmlQualifiedName type)
	{
		InitPrimitiveIDs();
		object obj = null;
		if (!IsPrimitiveNamespace(type.Namespace) || (object)type.Name == urTypeID)
		{
			return null;
		}
		if ((object)type.Namespace == schemaNsID || (object)type.Namespace == soapNsID || (object)type.Namespace == soap12NsID)
		{
			if ((object)type.Name == stringID || (object)type.Name == anyURIID || (object)type.Name == durationID || (object)type.Name == ENTITYID || (object)type.Name == ENTITIESID || (object)type.Name == gDayID || (object)type.Name == gMonthID || (object)type.Name == gMonthDayID || (object)type.Name == gYearID || (object)type.Name == gYearMonthID || (object)type.Name == IDID || (object)type.Name == IDREFID || (object)type.Name == IDREFSID || (object)type.Name == integerID || (object)type.Name == languageID || (object)type.Name == NameID || (object)type.Name == NCNameID || (object)type.Name == NMTOKENID || (object)type.Name == NMTOKENSID || (object)type.Name == negativeIntegerID || (object)type.Name == nonPositiveIntegerID || (object)type.Name == nonNegativeIntegerID || (object)type.Name == normalizedStringID || (object)type.Name == NOTATIONID || (object)type.Name == positiveIntegerID || (object)type.Name == tokenID)
			{
				return null;
			}
			if ((object)type.Name == intID)
			{
				return null;
			}
			if ((object)type.Name == booleanID)
			{
				return null;
			}
			if ((object)type.Name == shortID)
			{
				return null;
			}
			if ((object)type.Name == longID)
			{
				return null;
			}
			if ((object)type.Name == floatID)
			{
				return null;
			}
			if ((object)type.Name == doubleID)
			{
				return null;
			}
			if ((object)type.Name == decimalID)
			{
				return null;
			}
			if ((object)type.Name == dateTimeID)
			{
				return null;
			}
			if ((object)type.Name == qnameID)
			{
				return null;
			}
			if ((object)type.Name == dateID)
			{
				return null;
			}
			if ((object)type.Name == timeID)
			{
				return null;
			}
			if ((object)type.Name == unsignedByteID)
			{
				return null;
			}
			if ((object)type.Name == byteID)
			{
				return null;
			}
			if ((object)type.Name == unsignedShortID)
			{
				return null;
			}
			if ((object)type.Name == unsignedIntID)
			{
				return null;
			}
			if ((object)type.Name == unsignedLongID)
			{
				return null;
			}
			if ((object)type.Name == hexBinaryID)
			{
				return null;
			}
			if ((object)type.Name == base64BinaryID)
			{
				return null;
			}
			if ((object)type.Name == base64ID && ((object)type.Namespace == soapNsID || (object)type.Namespace == soap12NsID))
			{
				return null;
			}
			return null;
		}
		if ((object)type.Namespace == schemaNonXsdTypesNsID)
		{
			if ((object)type.Name == charID)
			{
				return null;
			}
			if ((object)type.Name == guidID)
			{
				return null;
			}
			if ((object)type.Name == timeSpanID && System.LocalAppContextSwitches.EnableTimeSpanSerialization)
			{
				return null;
			}
			return null;
		}
		return null;
	}

	/// <summary>Determines whether an XML attribute name indicates an XML namespace. </summary>
	/// <returns>true if the XML attribute name indicates an XML namespace; otherwise, false.</returns>
	/// <param name="name">The name of an XML attribute.</param>
	protected bool IsXmlnsAttribute(string name)
	{
		if (!name.StartsWith("xmlns", StringComparison.Ordinal))
		{
			return false;
		}
		if (name.Length == 5)
		{
			return true;
		}
		return name[5] == ':';
	}

	/// <summary>Sets the value of the XML attribute if it is of type arrayType from the Web Services Description Language (WSDL) namespace. </summary>
	/// <param name="attr">An <see cref="T:System.Xml.XmlAttribute" /> that may have the type wsdl:array.</param>
	protected void ParseWsdlArrayType(XmlAttribute attr)
	{
		if ((object)attr.LocalName == wsdlArrayTypeID && (object)attr.NamespaceURI == wsdlNsID)
		{
			int num = attr.Value.LastIndexOf(':');
			if (num < 0)
			{
				attr.Value = r.LookupNamespace("") + ":" + attr.Value;
			}
			else
			{
				attr.Value = r.LookupNamespace(attr.Value.Substring(0, num)) + ":" + attr.Value.Substring(num + 1);
			}
		}
	}

	/// <summary>Instructs the <see cref="T:System.Xml.XmlReader" /> to read the current XML element if the element has a null attribute with the value true. </summary>
	/// <returns>true if the element has a null="true" attribute value and has been read; otherwise, false.</returns>
	protected bool ReadNull()
	{
		if (!GetNullAttr())
		{
			return false;
		}
		if (r.IsEmptyElement)
		{
			r.Skip();
			return true;
		}
		r.ReadStartElement();
		int whileIterations = 0;
		int readerCount = ReaderCount;
		while (r.NodeType != XmlNodeType.EndElement)
		{
			UnknownNode(null);
			CheckReaderCount(ref whileIterations, ref readerCount);
		}
		ReadEndElement();
		return true;
	}

	/// <summary>Determines whether the XML element where the <see cref="T:System.Xml.XmlReader" /> is currently positioned has a null attribute set to the value true.</summary>
	/// <returns>true if <see cref="T:System.Xml.XmlReader" /> is currently positioned over a null attribute with the value true; otherwise, false.</returns>
	protected bool GetNullAttr()
	{
		string attribute = r.GetAttribute(nilID, instanceNsID);
		if (attribute == null)
		{
			attribute = r.GetAttribute(nullID, instanceNsID);
		}
		if (attribute == null)
		{
			attribute = r.GetAttribute(nullID, instanceNs2000ID);
			if (attribute == null)
			{
				attribute = r.GetAttribute(nullID, instanceNs1999ID);
			}
		}
		if (attribute == null || !XmlConvert.ToBoolean(attribute))
		{
			return false;
		}
		return true;
	}

	/// <summary>Instructs the <see cref="T:System.Xml.XmlReader" /> to read a simple, text-only XML element that could be null. </summary>
	/// <returns>The string value; otherwise, null.</returns>
	protected string ReadNullableString()
	{
		if (ReadNull())
		{
			return null;
		}
		return r.ReadElementString();
	}

	/// <summary>Instructs the <see cref="T:System.Xml.XmlReader" /> to read the fully qualified name of the element where it is currently positioned. </summary>
	/// <returns>A <see cref="T:System.Xml.XmlQualifiedName" /> that represents the fully qualified name of the current XML element; otherwise, null if a null="true" attribute value is present.</returns>
	protected XmlQualifiedName ReadNullableQualifiedName()
	{
		if (ReadNull())
		{
			return null;
		}
		return ReadElementQualifiedName();
	}

	/// <summary>Makes the <see cref="T:System.Xml.XmlReader" /> read the fully qualified name of the element where it is currently positioned. </summary>
	/// <returns>The fully qualified name of the current XML element.</returns>
	protected XmlQualifiedName ReadElementQualifiedName()
	{
		if (r.IsEmptyElement)
		{
			XmlQualifiedName result = new XmlQualifiedName(string.Empty, r.LookupNamespace(""));
			r.Skip();
			return result;
		}
		XmlQualifiedName result2 = ToXmlQualifiedName(CollapseWhitespace(r.ReadString()));
		r.ReadEndElement();
		return result2;
	}

	/// <summary>Instructs the <see cref="T:System.Xml.XmlReader" /> to read an XML document root element at its current position.</summary>
	/// <returns>An <see cref="T:System.Xml.XmlDocument" /> that contains the root element that has been read.</returns>
	/// <param name="wrapped">true if the method should read content only after reading the element's start element; otherwise, false.</param>
	protected XmlDocument ReadXmlDocument(bool wrapped)
	{
		XmlNode xmlNode = ReadXmlNode(wrapped);
		if (xmlNode == null)
		{
			return null;
		}
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.AppendChild(xmlDocument.ImportNode(xmlNode, deep: true));
		return xmlDocument;
	}

	/// <summary>Removes all occurrences of white space characters from the beginning and end of the specified string.</summary>
	/// <returns>The trimmed string.</returns>
	/// <param name="value">The string that will have its white space trimmed.</param>
	protected string CollapseWhitespace(string value)
	{
		return value?.Trim();
	}

	/// <summary>Instructs the <see cref="T:System.Xml.XmlReader" /> to read the XML node at its current position. </summary>
	/// <returns>An <see cref="T:System.Xml.XmlNode" /> that represents the XML node that has been read.</returns>
	/// <param name="wrapped">true to read content only after reading the element's start element; otherwise, false.</param>
	protected XmlNode ReadXmlNode(bool wrapped)
	{
		XmlNode result = null;
		if (wrapped)
		{
			if (ReadNull())
			{
				return null;
			}
			r.ReadStartElement();
			r.MoveToContent();
			if (r.NodeType != XmlNodeType.EndElement)
			{
				result = Document.ReadNode(r);
			}
			int whileIterations = 0;
			int readerCount = ReaderCount;
			while (r.NodeType != XmlNodeType.EndElement)
			{
				UnknownNode(null);
				CheckReaderCount(ref whileIterations, ref readerCount);
			}
			r.ReadEndElement();
		}
		else
		{
			result = Document.ReadNode(r);
		}
		return result;
	}

	/// <summary>Produces a base-64 byte array from an input string. </summary>
	/// <returns>A base-64 byte array.</returns>
	/// <param name="value">A string to translate into a base-64 byte array.</param>
	protected static byte[] ToByteArrayBase64(string value)
	{
		return XmlCustomFormatter.ToByteArrayBase64(value);
	}

	/// <summary>Instructs the <see cref="T:System.Xml.XmlReader" /> to read the string value at its current position and return it as a base-64 byte array.</summary>
	/// <returns>A base-64 byte array; otherwise, null if the value of the <paramref name="isNull" /> parameter is true.</returns>
	/// <param name="isNull">true to return null; false to return a base-64 byte array.</param>
	protected byte[] ToByteArrayBase64(bool isNull)
	{
		if (isNull)
		{
			return null;
		}
		return ReadByteArray(isBase64: true);
	}

	/// <summary>Produces a hexadecimal byte array from an input string.</summary>
	/// <returns>A hexadecimal byte array.</returns>
	/// <param name="value">A string to translate into a hexadecimal byte array.</param>
	protected static byte[] ToByteArrayHex(string value)
	{
		return XmlCustomFormatter.ToByteArrayHex(value);
	}

	/// <summary>Instructs the <see cref="T:System.Xml.XmlReader" /> to read the string value at its current position and return it as a hexadecimal byte array.</summary>
	/// <returns>A hexadecimal byte array; otherwise, null if the value of the <paramref name="isNull" /> parameter is true. </returns>
	/// <param name="isNull">true to return null; false to return a hexadecimal byte array.</param>
	protected byte[] ToByteArrayHex(bool isNull)
	{
		if (isNull)
		{
			return null;
		}
		return ReadByteArray(isBase64: false);
	}

	/// <summary>Gets the length of the SOAP-encoded array where the <see cref="T:System.Xml.XmlReader" /> is currently positioned.</summary>
	/// <returns>The length of the SOAP array.</returns>
	/// <param name="name">The local name that the array should have.</param>
	/// <param name="ns">The namespace that the array should have.</param>
	protected int GetArrayLength(string name, string ns)
	{
		if (GetNullAttr())
		{
			return 0;
		}
		string attribute = r.GetAttribute(arrayTypeID, soapNsID);
		SoapArrayInfo soapArrayInfo = ParseArrayType(attribute);
		if (soapArrayInfo.dimensions != 1)
		{
			throw new InvalidOperationException(Res.GetString("SOAP-ENC:arrayType with multidimensional array found at {0}. Only single-dimensional arrays are supported. Consider using an array of arrays instead.", CurrentTag()));
		}
		XmlQualifiedName xmlQualifiedName = ToXmlQualifiedName(soapArrayInfo.qname, decodeName: false);
		if (xmlQualifiedName.Name != name)
		{
			throw new InvalidOperationException(Res.GetString("The SOAP-ENC:arrayType references type is named '{0}'; a type named '{1}' was expected at {2}.", xmlQualifiedName.Name, name, CurrentTag()));
		}
		if (xmlQualifiedName.Namespace != ns)
		{
			throw new InvalidOperationException(Res.GetString("The SOAP-ENC:arrayType references type is from namespace '{0}'; the namespace '{1}' was expected at {2}.", xmlQualifiedName.Namespace, ns, CurrentTag()));
		}
		return soapArrayInfo.length;
	}

	private SoapArrayInfo ParseArrayType(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException(Res.GetString("SOAP-ENC:arrayType was missing at {0}.", CurrentTag()));
		}
		if (value.Length == 0)
		{
			throw new ArgumentException(Res.GetString("SOAP-ENC:arrayType was empty at {0}.", CurrentTag()), "value");
		}
		char[] array = value.ToCharArray();
		int num = array.Length;
		SoapArrayInfo result = default(SoapArrayInfo);
		int num2 = num - 1;
		if (array[num2] != ']')
		{
			throw new ArgumentException(Res.GetString("SOAP-ENC:arrayType must end with a ']' character."), "value");
		}
		num2--;
		while (num2 != -1 && array[num2] != '[')
		{
			if (array[num2] == ',')
			{
				throw new ArgumentException(Res.GetString("SOAP-ENC:arrayType with multidimensional array found at {0}. Only single-dimensional arrays are supported. Consider using an array of arrays instead.", CurrentTag()), "value");
			}
			num2--;
		}
		if (num2 == -1)
		{
			throw new ArgumentException(Res.GetString("SOAP-ENC:arrayType has mismatched brackets."), "value");
		}
		int num3 = num - num2 - 2;
		if (num3 > 0)
		{
			string text = new string(array, num2 + 1, num3);
			try
			{
				result.length = int.Parse(text, CultureInfo.InvariantCulture);
			}
			catch (Exception ex)
			{
				if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
				{
					throw;
				}
				throw new ArgumentException(Res.GetString("SOAP-ENC:arrayType could not handle '{1}' as the length of the array.", text), "value");
			}
		}
		else
		{
			result.length = -1;
		}
		num2--;
		result.jaggedDimensions = 0;
		while (num2 != -1 && array[num2] == ']')
		{
			num2--;
			if (num2 < 0)
			{
				throw new ArgumentException(Res.GetString("SOAP-ENC:arrayType has mismatched brackets."), "value");
			}
			if (array[num2] == ',')
			{
				throw new ArgumentException(Res.GetString("SOAP-ENC:arrayType with multidimensional array found at {0}. Only single-dimensional arrays are supported. Consider using an array of arrays instead.", CurrentTag()), "value");
			}
			if (array[num2] != '[')
			{
				throw new ArgumentException(Res.GetString("SOAP-ENC:arrayType must end with a ']' character."), "value");
			}
			num2--;
			result.jaggedDimensions++;
		}
		result.dimensions = 1;
		result.qname = new string(array, 0, num2 + 1);
		return result;
	}

	private SoapArrayInfo ParseSoap12ArrayType(string itemType, string arraySize)
	{
		SoapArrayInfo result = default(SoapArrayInfo);
		if (itemType != null && itemType.Length > 0)
		{
			result.qname = itemType;
		}
		else
		{
			result.qname = "";
		}
		string[] array = ((arraySize == null || arraySize.Length <= 0) ? new string[0] : arraySize.Split(null));
		result.dimensions = 0;
		result.length = -1;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].Length <= 0)
			{
				continue;
			}
			if (array[i] == "*")
			{
				result.dimensions++;
				continue;
			}
			try
			{
				result.length = int.Parse(array[i], CultureInfo.InvariantCulture);
				result.dimensions++;
			}
			catch (Exception ex)
			{
				if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
				{
					throw;
				}
				throw new ArgumentException(Res.GetString("SOAP-ENC:arrayType could not handle '{1}' as the length of the array.", array[i]), "value");
			}
		}
		if (result.dimensions == 0)
		{
			result.dimensions = 1;
		}
		return result;
	}

	/// <summary>Produces a <see cref="T:System.DateTime" /> object from an input string. </summary>
	/// <returns>A <see cref="T:System.DateTime" /> object.</returns>
	/// <param name="value">A string to translate into a <see cref="T:System.DateTime" /> object.</param>
	protected static DateTime ToDateTime(string value)
	{
		return XmlCustomFormatter.ToDateTime(value);
	}

	/// <summary>Produces a <see cref="T:System.DateTime" /> object from an input string. </summary>
	/// <returns>A <see cref="T:System.DateTime" />object.</returns>
	/// <param name="value">A string to translate into a <see cref="T:System.DateTime" /> class object.</param>
	protected static DateTime ToDate(string value)
	{
		return XmlCustomFormatter.ToDate(value);
	}

	/// <summary>Produces a <see cref="T:System.DateTime" /> from a string that represents the time. </summary>
	/// <returns>A <see cref="T:System.DateTime" /> object.</returns>
	/// <param name="value">A string to translate into a <see cref="T:System.DateTime" /> object.</param>
	protected static DateTime ToTime(string value)
	{
		return XmlCustomFormatter.ToTime(value);
	}

	/// <summary>Produces a <see cref="T:System.Char" /> object from an input string. </summary>
	/// <returns>A <see cref="T:System.Char" /> object.</returns>
	/// <param name="value">A string to translate into a <see cref="T:System.Char" /> object.</param>
	protected static char ToChar(string value)
	{
		return XmlCustomFormatter.ToChar(value);
	}

	/// <summary>Produces a numeric enumeration value from a string that consists of delimited identifiers that represent constants from the enumerator list. </summary>
	/// <returns>A long value that consists of the enumeration value as a series of bitwise OR operations.</returns>
	/// <param name="value">A string that consists of delimited identifiers where each identifier represents a constant from the set enumerator list.</param>
	/// <param name="h">A <see cref="T:System.Collections.Hashtable" /> that consists of the identifiers as keys and the constants as integral numbers.</param>
	/// <param name="typeName">The name of the enumeration type.</param>
	protected static long ToEnum(string value, Hashtable h, string typeName)
	{
		return XmlCustomFormatter.ToEnum(value, h, typeName, validate: true);
	}

	/// <summary>Decodes an XML name.</summary>
	/// <returns>A decoded string.</returns>
	/// <param name="value">An XML name to be decoded.</param>
	protected static string ToXmlName(string value)
	{
		return XmlCustomFormatter.ToXmlName(value);
	}

	/// <summary>Decodes an XML name.</summary>
	/// <returns>A decoded string.</returns>
	/// <param name="value">An XML name to be decoded.</param>
	protected static string ToXmlNCName(string value)
	{
		return XmlCustomFormatter.ToXmlNCName(value);
	}

	/// <summary>Decodes an XML name.</summary>
	/// <returns>A decoded string.</returns>
	/// <param name="value">An XML name to be decoded.</param>
	protected static string ToXmlNmToken(string value)
	{
		return XmlCustomFormatter.ToXmlNmToken(value);
	}

	/// <summary>Decodes an XML name.</summary>
	/// <returns>A decoded string.</returns>
	/// <param name="value">An XML name to be decoded.</param>
	protected static string ToXmlNmTokens(string value)
	{
		return XmlCustomFormatter.ToXmlNmTokens(value);
	}

	/// <summary>Obtains an <see cref="T:System.Xml.XmlQualifiedName" /> from a name that may contain a prefix. </summary>
	/// <returns>An <see cref="T:System.Xml.XmlQualifiedName" /> that represents a namespace-qualified XML name.</returns>
	/// <param name="value">A name that may contain a prefix.</param>
	protected XmlQualifiedName ToXmlQualifiedName(string value)
	{
		return ToXmlQualifiedName(value, DecodeName);
	}

	internal XmlQualifiedName ToXmlQualifiedName(string value, bool decodeName)
	{
		int num = value?.LastIndexOf(':') ?? (-1);
		string text = ((num < 0) ? null : value.Substring(0, num));
		string text2 = value.Substring(num + 1);
		if (decodeName)
		{
			text = XmlConvert.DecodeName(text);
			text2 = XmlConvert.DecodeName(text2);
		}
		if (text == null || text.Length == 0)
		{
			return new XmlQualifiedName(r.NameTable.Add(value), r.LookupNamespace(string.Empty));
		}
		string text3 = r.LookupNamespace(text);
		if (text3 == null)
		{
			throw new InvalidOperationException(Res.GetString("Namespace prefix '{0}' is not defined.", text));
		}
		return new XmlQualifiedName(r.NameTable.Add(text2), text3);
	}

	/// <summary>Raises an <see cref="E:System.Xml.Serialization.XmlSerializer.UnknownAttribute" /> event for the current position of the <see cref="T:System.Xml.XmlReader" />.</summary>
	/// <param name="o">An object that the <see cref="T:System.Xml.Serialization.XmlSerializer" /> is attempting to deserialize, subsequently accessible through the <see cref="P:System.Xml.Serialization.XmlAttributeEventArgs.ObjectBeingDeserialized" /> property.</param>
	/// <param name="attr">An <see cref="T:System.Xml.XmlAttribute" /> that represents the attribute in question.</param>
	protected void UnknownAttribute(object o, XmlAttribute attr)
	{
		UnknownAttribute(o, attr, null);
	}

	/// <summary>Raises an <see cref="E:System.Xml.Serialization.XmlSerializer.UnknownAttribute" /> event for the current position of the <see cref="T:System.Xml.XmlReader" />.</summary>
	/// <param name="o">An object that the <see cref="T:System.Xml.Serialization.XmlSerializer" /> is attempting to deserialize, subsequently accessible through the <see cref="P:System.Xml.Serialization.XmlAttributeEventArgs.ObjectBeingDeserialized" /> property.</param>
	/// <param name="attr">A <see cref="T:System.Xml.XmlAttribute" /> that represents the attribute in question.</param>
	/// <param name="qnames">A comma-delimited list of XML qualified names.</param>
	protected void UnknownAttribute(object o, XmlAttribute attr, string qnames)
	{
		if (events.OnUnknownAttribute != null)
		{
			GetCurrentPosition(out var lineNumber, out var linePosition);
			XmlAttributeEventArgs e = new XmlAttributeEventArgs(attr, lineNumber, linePosition, o, qnames);
			events.OnUnknownAttribute(events.sender, e);
		}
	}

	/// <summary>Raises an <see cref="E:System.Xml.Serialization.XmlSerializer.UnknownElement" /> event for the current position of the <see cref="T:System.Xml.XmlReader" />.</summary>
	/// <param name="o">The <see cref="T:System.Object" /> that is being deserialized.</param>
	/// <param name="elem">The <see cref="T:System.Xml.XmlElement" /> for which an event is raised.</param>
	protected void UnknownElement(object o, XmlElement elem)
	{
		UnknownElement(o, elem, null);
	}

	/// <summary>Raises an <see cref="E:System.Xml.Serialization.XmlSerializer.UnknownElement" /> event for the current position of the <see cref="T:System.Xml.XmlReader" />.</summary>
	/// <param name="o">An object that the <see cref="T:System.Xml.Serialization.XmlSerializer" /> is attempting to deserialize, subsequently accessible through the <see cref="P:System.Xml.Serialization.XmlAttributeEventArgs.ObjectBeingDeserialized" /> property.</param>
	/// <param name="elem">The <see cref="T:System.Xml.XmlElement" /> for which an event is raised.</param>
	/// <param name="qnames">A comma-delimited list of XML qualified names.</param>
	protected void UnknownElement(object o, XmlElement elem, string qnames)
	{
		if (events.OnUnknownElement != null)
		{
			GetCurrentPosition(out var lineNumber, out var linePosition);
			XmlElementEventArgs e = new XmlElementEventArgs(elem, lineNumber, linePosition, o, qnames);
			events.OnUnknownElement(events.sender, e);
		}
	}

	/// <summary>Raises an <see cref="E:System.Xml.Serialization.XmlSerializer.UnknownNode" /> event for the current position of the <see cref="T:System.Xml.XmlReader" />. </summary>
	/// <param name="o">The object that is being deserialized.</param>
	protected void UnknownNode(object o)
	{
		UnknownNode(o, null);
	}

	/// <summary>Raises an <see cref="E:System.Xml.Serialization.XmlSerializer.UnknownNode" /> event for the current position of the <see cref="T:System.Xml.XmlReader" />.</summary>
	/// <param name="o">The object being deserialized.</param>
	/// <param name="qnames">A comma-delimited list of XML qualified names.</param>
	protected void UnknownNode(object o, string qnames)
	{
		if (r.NodeType == XmlNodeType.None || r.NodeType == XmlNodeType.Whitespace)
		{
			r.Read();
		}
		else
		{
			if (r.NodeType == XmlNodeType.EndElement)
			{
				return;
			}
			if (events.OnUnknownNode != null)
			{
				UnknownNode(Document.ReadNode(r), o, qnames);
			}
			else if (r.NodeType != XmlNodeType.Attribute || events.OnUnknownAttribute != null)
			{
				if (r.NodeType == XmlNodeType.Element && events.OnUnknownElement == null)
				{
					r.Skip();
				}
				else
				{
					UnknownNode(Document.ReadNode(r), o, qnames);
				}
			}
		}
	}

	private void UnknownNode(XmlNode unknownNode, object o, string qnames)
	{
		if (unknownNode != null)
		{
			if (unknownNode.NodeType != 0 && unknownNode.NodeType != XmlNodeType.Whitespace && events.OnUnknownNode != null)
			{
				GetCurrentPosition(out var lineNumber, out var linePosition);
				XmlNodeEventArgs e = new XmlNodeEventArgs(unknownNode, lineNumber, linePosition, o);
				events.OnUnknownNode(events.sender, e);
			}
			if (unknownNode.NodeType == XmlNodeType.Attribute)
			{
				UnknownAttribute(o, (XmlAttribute)unknownNode, qnames);
			}
			else if (unknownNode.NodeType == XmlNodeType.Element)
			{
				UnknownElement(o, (XmlElement)unknownNode, qnames);
			}
		}
	}

	private void GetCurrentPosition(out int lineNumber, out int linePosition)
	{
		if (Reader is IXmlLineInfo)
		{
			IXmlLineInfo xmlLineInfo = (IXmlLineInfo)Reader;
			lineNumber = xmlLineInfo.LineNumber;
			linePosition = xmlLineInfo.LinePosition;
		}
		else
		{
			lineNumber = (linePosition = -1);
		}
	}

	/// <summary>Raises an <see cref="E:System.Xml.Serialization.XmlSerializer.UnreferencedObject" /> event for the current position of the <see cref="T:System.Xml.XmlReader" />.</summary>
	/// <param name="id">A unique string that is used to identify the unreferenced object, subsequently accessible through the <see cref="P:System.Xml.Serialization.UnreferencedObjectEventArgs.UnreferencedId" /> property.</param>
	/// <param name="o">An object that the <see cref="T:System.Xml.Serialization.XmlSerializer" /> is attempting to deserialize, subsequently accessible through the <see cref="P:System.Xml.Serialization.UnreferencedObjectEventArgs.UnreferencedObject" /> property.</param>
	protected void UnreferencedObject(string id, object o)
	{
		if (events.OnUnreferencedObject != null)
		{
			UnreferencedObjectEventArgs e = new UnreferencedObjectEventArgs(o, id);
			events.OnUnreferencedObject(events.sender, e);
		}
	}

	private string CurrentTag()
	{
		return r.NodeType switch
		{
			XmlNodeType.Element => "<" + r.LocalName + " xmlns='" + r.NamespaceURI + "'>", 
			XmlNodeType.EndElement => ">", 
			XmlNodeType.Text => r.Value, 
			XmlNodeType.CDATA => "CDATA", 
			XmlNodeType.Comment => "<--", 
			XmlNodeType.ProcessingInstruction => "<?", 
			_ => "(unknown)", 
		};
	}

	/// <summary>Creates an <see cref="T:System.InvalidOperationException" /> that indicates that a type is unknown. </summary>
	/// <returns>An <see cref="T:System.InvalidOperationException" /> exception.</returns>
	/// <param name="type">An <see cref="T:System.Xml.XmlQualifiedName" /> that represents the name of the unknown type.</param>
	protected Exception CreateUnknownTypeException(XmlQualifiedName type)
	{
		return new InvalidOperationException(Res.GetString("The specified type was not recognized: name='{0}', namespace='{1}', at {2}.", type.Name, type.Namespace, CurrentTag()));
	}

	/// <summary>Creates an <see cref="T:System.InvalidOperationException" /> that indicates that a SOAP-encoded collection type cannot be modified and its values cannot be filled in. </summary>
	/// <returns>An <see cref="T:System.InvalidOperationException" /> exception.</returns>
	/// <param name="name">The fully qualified name of the .NET Framework type for which there is a mapping.</param>
	protected Exception CreateReadOnlyCollectionException(string name)
	{
		return new InvalidOperationException(Res.GetString("Could not deserialize {0}. Parameterless constructor is required for collections and enumerators.", name));
	}

	/// <summary>Creates an <see cref="T:System.InvalidOperationException" /> that indicates that an object being deserialized should be abstract. </summary>
	/// <returns>An <see cref="T:System.InvalidOperationException" /> exception.</returns>
	/// <param name="name">The name of the abstract type.</param>
	/// <param name="ns">The .NET Framework namespace of the abstract type.</param>
	protected Exception CreateAbstractTypeException(string name, string ns)
	{
		return new InvalidOperationException(Res.GetString("The specified type is abstract: name='{0}', namespace='{1}', at {2}.", name, ns, CurrentTag()));
	}

	/// <summary>Creates an <see cref="T:System.InvalidOperationException" /> that indicates that an object being deserialized cannot be instantiated because there is no constructor available.</summary>
	/// <returns>An <see cref="T:System.InvalidOperationException" /> exception.</returns>
	/// <param name="typeName">The name of the type.</param>
	protected Exception CreateInaccessibleConstructorException(string typeName)
	{
		return new InvalidOperationException(Res.GetString("{0} cannot be serialized because it does not have a parameterless constructor.", typeName));
	}

	/// <summary>Creates an <see cref="T:System.InvalidOperationException" /> that indicates that an object being deserialized cannot be instantiated because the constructor throws a security exception.</summary>
	/// <returns>An <see cref="T:System.InvalidOperationException" /> exception.</returns>
	/// <param name="typeName">The name of the type.</param>
	protected Exception CreateCtorHasSecurityException(string typeName)
	{
		return new InvalidOperationException(Res.GetString("The type '{0}' cannot be serialized because its parameterless constructor is decorated with declarative security permission attributes. Consider using imperative asserts or demands in the constructor.", typeName));
	}

	/// <summary>Creates an <see cref="T:System.InvalidOperationException" /> that indicates that the current position of <see cref="T:System.Xml.XmlReader" /> represents an unknown XML node. </summary>
	/// <returns>An <see cref="T:System.InvalidOperationException" /> exception.</returns>
	protected Exception CreateUnknownNodeException()
	{
		return new InvalidOperationException(Res.GetString("{0} was not expected.", CurrentTag()));
	}

	/// <summary>Creates an <see cref="T:System.InvalidOperationException" /> that indicates that an enumeration value is not valid. </summary>
	/// <returns>An <see cref="T:System.InvalidOperationException" /> exception.</returns>
	/// <param name="value">The enumeration value that is not valid.</param>
	/// <param name="enumType">The enumeration type.</param>
	protected Exception CreateUnknownConstantException(string value, Type enumType)
	{
		return new InvalidOperationException(Res.GetString("Instance validation error: '{0}' is not a valid value for {1}.", value, enumType.Name));
	}

	/// <summary>Creates an <see cref="T:System.InvalidCastException" /> that indicates that an explicit reference conversion failed.</summary>
	/// <returns>An <see cref="T:System.InvalidCastException" /> exception.</returns>
	/// <param name="type">The <see cref="T:System.Type" /> that an object cannot be cast to. This type is incorporated into the exception message.</param>
	/// <param name="value">The object that cannot be cast. This object is incorporated into the exception message.</param>
	protected Exception CreateInvalidCastException(Type type, object value)
	{
		return CreateInvalidCastException(type, value, null);
	}

	/// <summary>Creates an <see cref="T:System.InvalidCastException" /> that indicates that an explicit reference conversion failed.</summary>
	/// <returns>An <see cref="T:System.InvalidCastException" /> exception.</returns>
	/// <param name="type">The <see cref="T:System.Type" /> that an object cannot be cast to. This type is incorporated into the exception message.</param>
	/// <param name="value">The object that cannot be cast. This object is incorporated into the exception message.</param>
	/// <param name="id">A string identifier.</param>
	protected Exception CreateInvalidCastException(Type type, object value, string id)
	{
		if (value == null)
		{
			return new InvalidCastException(Res.GetString("Cannot assign null value to an object of type {1}.", type.FullName));
		}
		if (id == null)
		{
			return new InvalidCastException(Res.GetString("Cannot assign object of type {0} to an object of type {1}.", value.GetType().FullName, type.FullName));
		}
		return new InvalidCastException(Res.GetString("Cannot assign object of type {0} to an object of type {1}. The error occurred while reading node with id='{2}'.", value.GetType().FullName, type.FullName, id));
	}

	/// <summary>Populates an object from its XML representation at the current location of the <see cref="T:System.Xml.XmlReader" />, with an option to read the inner element.</summary>
	/// <returns>An <see cref="T:System.InvalidOperationException" /> exception.</returns>
	/// <param name="xsdDerived">The local name of the derived XML Schema data type.</param>
	/// <param name="nsDerived">The namespace of the derived XML Schema data type.</param>
	/// <param name="xsdBase">The local name of the base XML Schema data type.</param>
	/// <param name="nsBase">The namespace of the base XML Schema data type.</param>
	/// <param name="clrDerived">The namespace of the derived .NET Framework type.</param>
	/// <param name="clrBase">The name of the base .NET Framework type.</param>
	protected Exception CreateBadDerivationException(string xsdDerived, string nsDerived, string xsdBase, string nsBase, string clrDerived, string clrBase)
	{
		return new InvalidOperationException(Res.GetString("Type '{0}' from namespace '{1}' declared as derivation of type '{2}' from namespace '{3}, but corresponding CLR types are not compatible.  Cannot convert type '{4}' to '{5}'.", xsdDerived, nsDerived, xsdBase, nsBase, clrDerived, clrBase));
	}

	/// <summary>Creates an <see cref="T:System.InvalidOperationException" /> that indicates that a derived type that is mapped to an XML Schema data type cannot be located.</summary>
	/// <returns>An <see cref="T:System.InvalidOperationException" /> exception.</returns>
	/// <param name="name">The local name of the XML Schema data type that is mapped to the unavailable derived type.</param>
	/// <param name="ns">The namespace of the XML Schema data type that is mapped to the unavailable derived type.</param>
	/// <param name="clrType">The full name of the .NET Framework base type for which a derived type cannot be located.</param>
	protected Exception CreateMissingIXmlSerializableType(string name, string ns, string clrType)
	{
		return new InvalidOperationException(Res.GetString("Type '{0}' from namespace '{1}' does not have corresponding IXmlSerializable type. Please consider adding {2} to '{3}'.", name, ns, typeof(XmlIncludeAttribute).Name, clrType));
	}

	/// <summary>Ensures that a given array, or a copy, is large enough to contain a specified index. </summary>
	/// <returns>The existing <see cref="T:System.Array" />, if it is already large enough; otherwise, a new, larger array that contains the original array's elements.</returns>
	/// <param name="a">The <see cref="T:System.Array" /> that is being checked.</param>
	/// <param name="index">The required index.</param>
	/// <param name="elementType">The <see cref="T:System.Type" /> of the array's elements.</param>
	protected Array EnsureArrayIndex(Array a, int index, Type elementType)
	{
		if (a == null)
		{
			return Array.CreateInstance(elementType, 32);
		}
		if (index < a.Length)
		{
			return a;
		}
		Array array = Array.CreateInstance(elementType, a.Length * 2);
		Array.Copy(a, array, index);
		return array;
	}

	/// <summary>Ensures that a given array, or a copy, is no larger than a specified length. </summary>
	/// <returns>The existing <see cref="T:System.Array" />, if it is already small enough; otherwise, a new, smaller array that contains the original array's elements up to the size of<paramref name=" length" />.</returns>
	/// <param name="a">The array that is being checked.</param>
	/// <param name="length">The maximum length of the array.</param>
	/// <param name="elementType">The <see cref="T:System.Type" /> of the array's elements.</param>
	/// <param name="isNullable">true if null for the array, if present for the input array, can be returned; otherwise, a new, smaller array.</param>
	protected Array ShrinkArray(Array a, int length, Type elementType, bool isNullable)
	{
		if (a == null)
		{
			if (isNullable)
			{
				return null;
			}
			return Array.CreateInstance(elementType, 0);
		}
		if (a.Length == length)
		{
			return a;
		}
		Array array = Array.CreateInstance(elementType, length);
		Array.Copy(a, array, length);
		return array;
	}

	/// <summary>Produces the result of a call to the <see cref="M:System.Xml.XmlReader.ReadString" /> method appended to the input value. </summary>
	/// <returns>The result of call to the <see cref="M:System.Xml.XmlReader.ReadString" /> method appended to the input value.</returns>
	/// <param name="value">A string to prefix to the result of a call to the <see cref="M:System.Xml.XmlReader.ReadString" /> method.</param>
	protected string ReadString(string value)
	{
		return ReadString(value, trim: false);
	}

	/// <summary>Returns the result of a call to the <see cref="M:System.Xml.XmlReader.ReadString" /> method of the <see cref="T:System.Xml.XmlReader" /> class, trimmed of white space if needed, and appended to the input value.</summary>
	/// <returns>The result of the read operation appended to the input value.</returns>
	/// <param name="value">A string that will be appended to.</param>
	/// <param name="trim">true if the result of the read operation should be trimmed; otherwise, false.</param>
	protected string ReadString(string value, bool trim)
	{
		string text = r.ReadString();
		if (text != null && trim)
		{
			text = text.Trim();
		}
		if (value == null || value.Length == 0)
		{
			return text;
		}
		return value + text;
	}

	/// <summary>Populates an object from its XML representation at the current location of the <see cref="T:System.Xml.XmlReader" />. </summary>
	/// <returns>An object that implements the <see cref="T:System.Xml.Serialization.IXmlSerializable" /> interface with its members populated from the location of the <see cref="T:System.Xml.XmlReader" />.</returns>
	/// <param name="serializable">An <see cref="T:System.Xml.Serialization.IXmlSerializable" /> that corresponds to the current position of the <see cref="T:System.Xml.XmlReader" />.</param>
	protected IXmlSerializable ReadSerializable(IXmlSerializable serializable)
	{
		return ReadSerializable(serializable, wrappedAny: false);
	}

	/// <summary>This method supports the .NET Framework infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>An object that implements the IXmlSerializable interface with its members populated from the location of the XmlReader.</returns>
	/// <param name="serializable">An IXmlSerializable object that corresponds to the current position of the XMLReader.</param>
	/// <param name="wrappedAny">Specifies whether the serializable object is wrapped.</param>
	protected IXmlSerializable ReadSerializable(IXmlSerializable serializable, bool wrappedAny)
	{
		string text = null;
		string text2 = null;
		if (wrappedAny)
		{
			text = r.LocalName;
			text2 = r.NamespaceURI;
			r.Read();
			r.MoveToContent();
		}
		serializable.ReadXml(r);
		if (wrappedAny)
		{
			while (r.NodeType == XmlNodeType.Whitespace)
			{
				r.Skip();
			}
			if (r.NodeType == XmlNodeType.None)
			{
				r.Skip();
			}
			if (r.NodeType == XmlNodeType.EndElement && r.LocalName == text && r.NamespaceURI == text2)
			{
				Reader.Read();
			}
		}
		return serializable;
	}

	/// <summary>Reads the value of the href attribute (ref attribute for SOAP 1.2) that is used to refer to an XML element in SOAP encoding. </summary>
	/// <returns>true if the value was read; otherwise, false.</returns>
	/// <param name="fixupReference">An output string into which the href attribute value is read.</param>
	protected bool ReadReference(out string fixupReference)
	{
		string text = (soap12 ? r.GetAttribute("ref", "http://www.w3.org/2003/05/soap-encoding") : r.GetAttribute("href"));
		if (text == null)
		{
			fixupReference = null;
			return false;
		}
		if (!soap12)
		{
			if (!text.StartsWith("#", StringComparison.Ordinal))
			{
				throw new InvalidOperationException(Res.GetString("The referenced element with ID '{0}' is located outside the current document and cannot be retrieved.", text));
			}
			fixupReference = text.Substring(1);
		}
		else
		{
			fixupReference = text;
		}
		if (r.IsEmptyElement)
		{
			r.Skip();
		}
		else
		{
			r.ReadStartElement();
			ReadEndElement();
		}
		return true;
	}

	/// <summary>Stores an object that is being deserialized from a SOAP-encoded multiRef element for later access through the <see cref="M:System.Xml.Serialization.XmlSerializationReader.GetTarget(System.String)" /> method. </summary>
	/// <param name="id">The value of the id attribute of a multiRef element that identifies the element.</param>
	/// <param name="o">The object that is deserialized from the XML element.</param>
	protected void AddTarget(string id, object o)
	{
		if (id == null)
		{
			if (targetsWithoutIds == null)
			{
				targetsWithoutIds = new ArrayList();
			}
			if (o != null)
			{
				targetsWithoutIds.Add(o);
			}
		}
		else
		{
			if (targets == null)
			{
				targets = new Hashtable();
			}
			if (!targets.Contains(id))
			{
				targets.Add(id, o);
			}
		}
	}

	/// <summary>Stores an object that contains a callback method instance that will be called, as necessary, to fill in the objects in a SOAP-encoded array. </summary>
	/// <param name="fixup">An <see cref="T:System.Xml.Serialization.XmlSerializationFixupCallback" /> delegate and the callback method's input data.</param>
	protected void AddFixup(Fixup fixup)
	{
		if (fixups == null)
		{
			fixups = new ArrayList();
		}
		fixups.Add(fixup);
	}

	/// <summary>Stores an object that contains a callback method that will be called, as necessary, to fill in .NET Framework collections or enumerations that map to SOAP-encoded arrays or SOAP-encoded, multi-referenced elements. </summary>
	/// <param name="fixup">A <see cref="T:System.Xml.Serialization.XmlSerializationCollectionFixupCallback" /> delegate and the callback method's input data.</param>
	protected void AddFixup(CollectionFixup fixup)
	{
		if (collectionFixups == null)
		{
			collectionFixups = new ArrayList();
		}
		collectionFixups.Add(fixup);
	}

	/// <summary>Gets an object that is being deserialized from a SOAP-encoded multiRef element and that was stored earlier by <see cref="M:System.Xml.Serialization.XmlSerializationReader.AddTarget(System.String,System.Object)" />.  </summary>
	/// <returns>An object to be deserialized from a SOAP-encoded multiRef element.</returns>
	/// <param name="id">The value of the id attribute of a multiRef element that identifies the element.</param>
	protected object GetTarget(string id)
	{
		object obj = ((targets != null) ? targets[id] : null);
		if (obj == null)
		{
			throw new InvalidOperationException(Res.GetString("The referenced element with ID '{0}' was not found in the document.", id));
		}
		Referenced(obj);
		return obj;
	}

	/// <summary>Stores an object to be deserialized from a SOAP-encoded multiRef element.</summary>
	/// <param name="o">The object to be deserialized.</param>
	protected void Referenced(object o)
	{
		if (o != null)
		{
			if (referencedTargets == null)
			{
				referencedTargets = new Hashtable();
			}
			referencedTargets[o] = o;
		}
	}

	private void HandleUnreferencedObjects()
	{
		if (targets != null)
		{
			foreach (DictionaryEntry target in targets)
			{
				if (referencedTargets == null || !referencedTargets.Contains(target.Value))
				{
					UnreferencedObject((string)target.Key, target.Value);
				}
			}
		}
		if (targetsWithoutIds == null)
		{
			return;
		}
		foreach (object targetsWithoutId in targetsWithoutIds)
		{
			if (referencedTargets == null || !referencedTargets.Contains(targetsWithoutId))
			{
				UnreferencedObject(null, targetsWithoutId);
			}
		}
	}

	private void DoFixups()
	{
		if (fixups == null)
		{
			return;
		}
		for (int i = 0; i < fixups.Count; i++)
		{
			Fixup fixup = (Fixup)fixups[i];
			fixup.Callback(fixup);
		}
		if (collectionFixups != null)
		{
			for (int j = 0; j < collectionFixups.Count; j++)
			{
				CollectionFixup collectionFixup = (CollectionFixup)collectionFixups[j];
				collectionFixup.Callback(collectionFixup.Collection, collectionFixup.CollectionItems);
			}
		}
	}

	/// <summary>Fills in the values of a SOAP-encoded array whose data type maps to a .NET Framework reference type.</summary>
	/// <param name="fixup">An object that contains the array whose values are filled in.</param>
	protected void FixupArrayRefs(object fixup)
	{
		Fixup fixup2 = (Fixup)fixup;
		Array array = (Array)fixup2.Source;
		for (int i = 0; i < array.Length; i++)
		{
			string text = fixup2.Ids[i];
			if (text != null)
			{
				object target = GetTarget(text);
				try
				{
					array.SetValue(target, i);
				}
				catch (InvalidCastException)
				{
					throw new InvalidOperationException(Res.GetString("Invalid reference id='{0}'. Object of type {1} cannot be stored in an array of this type. Details: array index={2}.", text, target.GetType().FullName, i.ToString(CultureInfo.InvariantCulture)));
				}
			}
		}
	}

	private object ReadArray(string typeName, string typeNs)
	{
		Type type = null;
		SoapArrayInfo soapArrayInfo;
		if (soap12)
		{
			string attribute = r.GetAttribute(itemTypeID, soap12NsID);
			string attribute2 = r.GetAttribute(arraySizeID, soap12NsID);
			Type type2 = (Type)types[new XmlQualifiedName(typeName, typeNs)];
			if (attribute == null && attribute2 == null && (type2 == null || !type2.IsArray))
			{
				return null;
			}
			soapArrayInfo = ParseSoap12ArrayType(attribute, attribute2);
			if (type2 != null)
			{
				type = TypeScope.GetArrayElementType(type2, null);
			}
		}
		else
		{
			string attribute3 = r.GetAttribute(arrayTypeID, soapNsID);
			if (attribute3 == null)
			{
				return null;
			}
			soapArrayInfo = ParseArrayType(attribute3);
		}
		if (soapArrayInfo.dimensions != 1)
		{
			throw new InvalidOperationException(Res.GetString("SOAP-ENC:arrayType with multidimensional array found at {0}. Only single-dimensional arrays are supported. Consider using an array of arrays instead.", CurrentTag()));
		}
		Type type3 = null;
		XmlQualifiedName xmlQualifiedName = new XmlQualifiedName(urTypeID, schemaNsID);
		XmlQualifiedName xmlQualifiedName2;
		if (soapArrayInfo.qname.Length > 0)
		{
			xmlQualifiedName2 = ToXmlQualifiedName(soapArrayInfo.qname, decodeName: false);
			type3 = (Type)types[xmlQualifiedName2];
		}
		else
		{
			xmlQualifiedName2 = xmlQualifiedName;
		}
		if (soap12 && type3 == typeof(object))
		{
			type3 = null;
		}
		bool flag;
		if (type3 == null)
		{
			if (!soap12)
			{
				type3 = GetPrimitiveType(xmlQualifiedName2, throwOnUnknown: true);
				flag = true;
			}
			else
			{
				if (xmlQualifiedName2 != xmlQualifiedName)
				{
					type3 = GetPrimitiveType(xmlQualifiedName2, throwOnUnknown: false);
				}
				if (type3 != null)
				{
					flag = true;
				}
				else if (type == null)
				{
					type3 = typeof(object);
					flag = false;
				}
				else
				{
					type3 = type;
					XmlQualifiedName xmlQualifiedName3 = (XmlQualifiedName)typesReverse[type3];
					if (xmlQualifiedName3 == null)
					{
						xmlQualifiedName3 = XmlSerializationWriter.GetPrimitiveTypeNameInternal(type3);
						flag = true;
					}
					else
					{
						flag = type3.IsPrimitive;
					}
					if (xmlQualifiedName3 != null)
					{
						xmlQualifiedName2 = xmlQualifiedName3;
					}
				}
			}
		}
		else
		{
			flag = type3.IsPrimitive;
		}
		if (!soap12 && soapArrayInfo.jaggedDimensions > 0)
		{
			for (int i = 0; i < soapArrayInfo.jaggedDimensions; i++)
			{
				type3 = type3.MakeArrayType();
			}
		}
		if (r.IsEmptyElement)
		{
			r.Skip();
			return Array.CreateInstance(type3, 0);
		}
		r.ReadStartElement();
		r.MoveToContent();
		int num = 0;
		Array array = null;
		if (type3.IsValueType)
		{
			if (!flag && !type3.IsEnum)
			{
				throw new NotSupportedException(Res.GetString("Cannot serialize {0}. Arrays of structs are not supported with encoded SOAP.", type3.FullName));
			}
			int whileIterations = 0;
			int readerCount = ReaderCount;
			while (r.NodeType != XmlNodeType.EndElement)
			{
				array = EnsureArrayIndex(array, num, type3);
				array.SetValue(ReadReferencedElement(xmlQualifiedName2.Name, xmlQualifiedName2.Namespace), num);
				num++;
				r.MoveToContent();
				CheckReaderCount(ref whileIterations, ref readerCount);
			}
			array = ShrinkArray(array, num, type3, isNullable: false);
		}
		else
		{
			string[] array2 = null;
			int num2 = 0;
			int whileIterations2 = 0;
			int readerCount2 = ReaderCount;
			while (r.NodeType != XmlNodeType.EndElement)
			{
				array = EnsureArrayIndex(array, num, type3);
				array2 = (string[])EnsureArrayIndex(array2, num2, typeof(string));
				string name;
				string ns;
				if (r.NamespaceURI.Length != 0)
				{
					name = r.LocalName;
					ns = (((object)r.NamespaceURI != soapNsID) ? r.NamespaceURI : "http://www.w3.org/2001/XMLSchema");
				}
				else
				{
					name = xmlQualifiedName2.Name;
					ns = xmlQualifiedName2.Namespace;
				}
				array.SetValue(ReadReferencingElement(name, ns, out array2[num2]), num);
				num++;
				num2++;
				r.MoveToContent();
				CheckReaderCount(ref whileIterations2, ref readerCount2);
			}
			if (soap12 && type3 == typeof(object))
			{
				Type type4 = null;
				for (int j = 0; j < num; j++)
				{
					object value = array.GetValue(j);
					if (value != null)
					{
						Type type5 = value.GetType();
						if (type5.IsValueType)
						{
							type4 = null;
							break;
						}
						if (type4 == null || type5.IsAssignableFrom(type4))
						{
							type4 = type5;
						}
						else if (!type4.IsAssignableFrom(type5))
						{
							type4 = null;
							break;
						}
					}
				}
				if (type4 != null)
				{
					type3 = type4;
				}
			}
			array2 = (string[])ShrinkArray(array2, num2, typeof(string), isNullable: false);
			array = ShrinkArray(array, num, type3, isNullable: false);
			Fixup fixup = new Fixup(array, FixupArrayRefs, array2);
			AddFixup(fixup);
		}
		ReadEndElement();
		return array;
	}

	/// <summary>Initializes callback methods that populate objects that map to SOAP-encoded XML data. </summary>
	protected abstract void InitCallbacks();

	/// <summary>Deserializes objects from the SOAP-encoded multiRef elements in a SOAP message. </summary>
	protected void ReadReferencedElements()
	{
		r.MoveToContent();
		int whileIterations = 0;
		int readerCount = ReaderCount;
		while (r.NodeType != XmlNodeType.EndElement && r.NodeType != 0)
		{
			ReadReferencingElement(null, null, elementCanBeType: true, out var _);
			r.MoveToContent();
			CheckReaderCount(ref whileIterations, ref readerCount);
		}
		DoFixups();
		HandleUnreferencedObjects();
	}

	/// <summary>Deserializes an object from a SOAP-encoded multiRef XML element. </summary>
	/// <returns>The value of the referenced element in the document.</returns>
	protected object ReadReferencedElement()
	{
		return ReadReferencedElement(null, null);
	}

	/// <summary>Deserializes an object from a SOAP-encoded multiRef XML element. </summary>
	/// <returns>The value of the referenced element in the document.</returns>
	/// <param name="name">The local name of the element's XML Schema data type.</param>
	/// <param name="ns">The namespace of the element's XML Schema data type.</param>
	protected object ReadReferencedElement(string name, string ns)
	{
		string fixupReference;
		return ReadReferencingElement(name, ns, out fixupReference);
	}

	/// <summary>Deserializes an object from an XML element in a SOAP message that contains a reference to a multiRef element. </summary>
	/// <returns>The deserialized object.</returns>
	/// <param name="fixupReference">An output string into which the href attribute value is read.</param>
	protected object ReadReferencingElement(out string fixupReference)
	{
		return ReadReferencingElement(null, null, out fixupReference);
	}

	/// <summary>Deserializes an object from an XML element in a SOAP message that contains a reference to a multiRef element. </summary>
	/// <returns>The deserialized object.</returns>
	/// <param name="name">The local name of the element's XML Schema data type.</param>
	/// <param name="ns">The namespace of the element's XML Schema data type.</param>
	/// <param name="fixupReference">An output string into which the href attribute value is read.</param>
	protected object ReadReferencingElement(string name, string ns, out string fixupReference)
	{
		return ReadReferencingElement(name, ns, elementCanBeType: false, out fixupReference);
	}

	/// <summary>Deserializes an object from an XML element in a SOAP message that contains a reference to a multiRef element.</summary>
	/// <returns>The deserialized object.</returns>
	/// <param name="name">The local name of the element's XML Schema data type.</param>
	/// <param name="ns">The namespace of the element's XML Schema data type.</param>
	/// <param name="elementCanBeType">true if the element name is also the XML Schema data type name; otherwise, false.</param>
	/// <param name="fixupReference">An output string into which the value of the href attribute is read.</param>
	protected object ReadReferencingElement(string name, string ns, bool elementCanBeType, out string fixupReference)
	{
		object obj = null;
		if (callbacks == null)
		{
			callbacks = new Hashtable();
			types = new Hashtable();
			XmlQualifiedName xmlQualifiedName = new XmlQualifiedName(urTypeID, r.NameTable.Add("http://www.w3.org/2001/XMLSchema"));
			types.Add(xmlQualifiedName, typeof(object));
			typesReverse = new Hashtable();
			typesReverse.Add(typeof(object), xmlQualifiedName);
			InitCallbacks();
		}
		r.MoveToContent();
		if (ReadReference(out fixupReference))
		{
			return null;
		}
		if (ReadNull())
		{
			return null;
		}
		string id = (soap12 ? r.GetAttribute("id", "http://www.w3.org/2003/05/soap-encoding") : r.GetAttribute("id", null));
		if ((obj = ReadArray(name, ns)) == null)
		{
			XmlQualifiedName xmlQualifiedName2 = GetXsiType();
			if (xmlQualifiedName2 == null)
			{
				xmlQualifiedName2 = ((name != null) ? new XmlQualifiedName(r.NameTable.Add(name), r.NameTable.Add(ns)) : new XmlQualifiedName(r.NameTable.Add(r.LocalName), r.NameTable.Add(r.NamespaceURI)));
			}
			XmlSerializationReadCallback xmlSerializationReadCallback = (XmlSerializationReadCallback)callbacks[xmlQualifiedName2];
			obj = ((xmlSerializationReadCallback == null) ? ReadTypedPrimitive(xmlQualifiedName2, elementCanBeType) : xmlSerializationReadCallback());
		}
		AddTarget(id, obj);
		return obj;
	}

	/// <summary>Stores an implementation of the <see cref="T:System.Xml.Serialization.XmlSerializationReadCallback" /> delegate and its input data for a later invocation. </summary>
	/// <param name="name">The name of the .NET Framework type that is being deserialized.</param>
	/// <param name="ns">The namespace of the .NET Framework type that is being deserialized.</param>
	/// <param name="type">The <see cref="T:System.Type" /> to be deserialized.</param>
	/// <param name="read">An <see cref="T:System.Xml.Serialization.XmlSerializationReadCallback" /> delegate.</param>
	protected void AddReadCallback(string name, string ns, Type type, XmlSerializationReadCallback read)
	{
		XmlQualifiedName xmlQualifiedName = new XmlQualifiedName(r.NameTable.Add(name), r.NameTable.Add(ns));
		callbacks[xmlQualifiedName] = read;
		types[xmlQualifiedName] = type;
		typesReverse[type] = xmlQualifiedName;
	}

	/// <summary>Makes the <see cref="T:System.Xml.XmlReader" /> read an XML end tag.</summary>
	protected void ReadEndElement()
	{
		while (r.NodeType == XmlNodeType.Whitespace)
		{
			r.Skip();
		}
		if (r.NodeType == XmlNodeType.None)
		{
			r.Skip();
		}
		else
		{
			r.ReadEndElement();
		}
	}

	private object ReadXmlNodes(bool elementCanBeType)
	{
		ArrayList arrayList = new ArrayList();
		string localName = Reader.LocalName;
		string namespaceURI = Reader.NamespaceURI;
		string name = Reader.Name;
		string text = null;
		string text2 = null;
		int num = 0;
		int lineNumber = -1;
		int linePosition = -1;
		XmlNode xmlNode = null;
		if (Reader.NodeType == XmlNodeType.Attribute)
		{
			XmlAttribute xmlAttribute = Document.CreateAttribute(name, namespaceURI);
			xmlAttribute.Value = Reader.Value;
			xmlNode = xmlAttribute;
		}
		else
		{
			xmlNode = Document.CreateElement(name, namespaceURI);
		}
		GetCurrentPosition(out lineNumber, out linePosition);
		XmlElement xmlElement = xmlNode as XmlElement;
		while (Reader.MoveToNextAttribute())
		{
			if (IsXmlnsAttribute(Reader.Name) || (Reader.Name == "id" && (!soap12 || Reader.NamespaceURI == "http://www.w3.org/2003/05/soap-encoding")))
			{
				num++;
			}
			if ((object)Reader.LocalName == typeID && ((object)Reader.NamespaceURI == instanceNsID || (object)Reader.NamespaceURI == instanceNs2000ID || (object)Reader.NamespaceURI == instanceNs1999ID))
			{
				string value = Reader.Value;
				int num2 = value.LastIndexOf(':');
				text = ((num2 >= 0) ? value.Substring(num2 + 1) : value);
				text2 = Reader.LookupNamespace((num2 >= 0) ? value.Substring(0, num2) : "");
			}
			XmlAttribute xmlAttribute2 = (XmlAttribute)Document.ReadNode(r);
			arrayList.Add(xmlAttribute2);
			xmlElement?.SetAttributeNode(xmlAttribute2);
		}
		if (elementCanBeType && text == null)
		{
			text = localName;
			text2 = namespaceURI;
			XmlAttribute xmlAttribute3 = Document.CreateAttribute(typeID, instanceNsID);
			xmlAttribute3.Value = name;
			arrayList.Add(xmlAttribute3);
		}
		if (text == "anyType" && ((object)text2 == schemaNsID || (object)text2 == schemaNs1999ID || (object)text2 == schemaNs2000ID))
		{
			num++;
		}
		Reader.MoveToElement();
		if (Reader.IsEmptyElement)
		{
			Reader.Skip();
		}
		else
		{
			Reader.ReadStartElement();
			Reader.MoveToContent();
			int whileIterations = 0;
			int readerCount = ReaderCount;
			while (Reader.NodeType != XmlNodeType.EndElement)
			{
				XmlNode xmlNode2 = Document.ReadNode(r);
				arrayList.Add(xmlNode2);
				xmlElement?.AppendChild(xmlNode2);
				Reader.MoveToContent();
				CheckReaderCount(ref whileIterations, ref readerCount);
			}
			ReadEndElement();
		}
		if (arrayList.Count <= num)
		{
			return new object();
		}
		XmlNode[] result = (XmlNode[])arrayList.ToArray(typeof(XmlNode));
		UnknownNode(xmlNode, null, null);
		return result;
	}

	/// <summary>Checks whether the deserializer has advanced.</summary>
	/// <param name="whileIterations">The current count in a while loop.</param>
	/// <param name="readerCount">The current <see cref="P:System.Xml.Serialization.XmlSerializationReader.ReaderCount" />. </param>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Xml.Serialization.XmlSerializationReader.ReaderCount" /> has not advanced. </exception>
	protected void CheckReaderCount(ref int whileIterations, ref int readerCount)
	{
		if (!checkDeserializeAdvances)
		{
			return;
		}
		whileIterations++;
		if ((whileIterations & 0x80) == 128)
		{
			if (readerCount == ReaderCount)
			{
				throw new InvalidOperationException(Res.GetString("Internal error: deserialization failed to advance over underlying stream."));
			}
			readerCount = ReaderCount;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Serialization.XmlSerializationReader" /> class.</summary>
	protected XmlSerializationReader()
	{
	}
}
