using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Xml.Schema;

namespace System.Xml.Serialization;

/// <summary>Represents an abstract class used for controlling serialization by the <see cref="T:System.Xml.Serialization.XmlSerializer" /> class.</summary>
public abstract class XmlSerializationWriter : XmlSerializationGeneratedCode
{
	internal class TypeEntry
	{
		internal XmlSerializationWriteCallback callback;

		internal string typeNs;

		internal string typeName;

		internal Type type;
	}

	private XmlWriter w;

	private XmlSerializerNamespaces namespaces;

	private int tempNamespacePrefix;

	private Hashtable usedPrefixes;

	private Hashtable references;

	private string idBase;

	private int nextId;

	private Hashtable typeEntries;

	private ArrayList referencesToWrite;

	private Hashtable objectsInUse;

	private string aliasBase = "q";

	private bool soap12;

	private bool escapeName = true;

	/// <summary>Gets or sets a value that indicates whether the <see cref="M:System.Xml.XmlConvert.EncodeName(System.String)" /> method is used to write valid XML.</summary>
	/// <returns>true if the <see cref="M:System.Xml.Serialization.XmlSerializationWriter.FromXmlQualifiedName(System.Xml.XmlQualifiedName)" /> method returns an encoded name; otherwise, false.</returns>
	protected bool EscapeName
	{
		get
		{
			return escapeName;
		}
		set
		{
			escapeName = value;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Xml.XmlWriter" /> that is being used by the <see cref="T:System.Xml.Serialization.XmlSerializationWriter" />.</summary>
	/// <returns>The <see cref="T:System.Xml.XmlWriter" /> used by the class instance.</returns>
	protected XmlWriter Writer
	{
		get
		{
			return w;
		}
		set
		{
			w = value;
		}
	}

	/// <summary>Gets or sets a list of XML qualified name objects that contain the namespaces and prefixes used to produce qualified names in XML documents.</summary>
	/// <returns>An <see cref="T:System.Collections.ArrayList" /> that contains the namespaces and prefix pairs.</returns>
	protected ArrayList Namespaces
	{
		get
		{
			if (namespaces != null)
			{
				return namespaces.NamespaceList;
			}
			return null;
		}
		set
		{
			if (value == null)
			{
				namespaces = null;
				return;
			}
			XmlQualifiedName[] array = (XmlQualifiedName[])value.ToArray(typeof(XmlQualifiedName));
			namespaces = new XmlSerializerNamespaces(array);
		}
	}

	internal void Init(XmlWriter w, XmlSerializerNamespaces namespaces, string encodingStyle, string idBase, TempAssembly tempAssembly)
	{
		this.w = w;
		this.namespaces = namespaces;
		soap12 = encodingStyle == "http://www.w3.org/2003/05/soap-encoding";
		this.idBase = idBase;
		Init(tempAssembly);
	}

	/// <summary>Processes a base-64 byte array.</summary>
	/// <returns>The same byte array that was passed in as an argument.</returns>
	/// <param name="value">A base-64 <see cref="T:System.Byte" /> array.</param>
	protected static byte[] FromByteArrayBase64(byte[] value)
	{
		return value;
	}

	/// <summary>Gets a dynamically generated assembly by name.</summary>
	/// <returns>A dynamically generated assembly.</returns>
	/// <param name="assemblyFullName">The full name of the assembly.</param>
	protected static Assembly ResolveDynamicAssembly(string assemblyFullName)
	{
		return DynamicAssemblies.Get(assemblyFullName);
	}

	/// <summary>Produces a string from an input hexadecimal byte array.</summary>
	/// <returns>The byte array value converted to a string.</returns>
	/// <param name="value">A hexadecimal byte array to translate to a string.</param>
	protected static string FromByteArrayHex(byte[] value)
	{
		return XmlCustomFormatter.FromByteArrayHex(value);
	}

	/// <summary>Produces a string from an input <see cref="T:System.DateTime" />.</summary>
	/// <returns>A string representation of the <see cref="T:System.DateTime" /> that shows the date and time.</returns>
	/// <param name="value">A <see cref="T:System.DateTime" /> to translate to a string.</param>
	protected static string FromDateTime(DateTime value)
	{
		return XmlCustomFormatter.FromDateTime(value);
	}

	/// <summary>Produces a string from a <see cref="T:System.DateTime" /> object.</summary>
	/// <returns>A string representation of the <see cref="T:System.DateTime" /> that shows the date but no time.</returns>
	/// <param name="value">A <see cref="T:System.DateTime" /> to translate to a string.</param>
	protected static string FromDate(DateTime value)
	{
		return XmlCustomFormatter.FromDate(value);
	}

	/// <summary>Produces a string from a <see cref="T:System.DateTime" /> object.</summary>
	/// <returns>A string representation of the <see cref="T:System.DateTime" /> object that shows the time but no date.</returns>
	/// <param name="value">A <see cref="T:System.DateTime" /> that is translated to a string.</param>
	protected static string FromTime(DateTime value)
	{
		return XmlCustomFormatter.FromTime(value);
	}

	/// <summary>Produces a string from an input <see cref="T:System.Char" />.</summary>
	/// <returns>The <see cref="T:System.Char" /> value converted to a string.</returns>
	/// <param name="value">A <see cref="T:System.Char" /> to translate to a string.</param>
	protected static string FromChar(char value)
	{
		return XmlCustomFormatter.FromChar(value);
	}

	/// <summary>Produces a string that consists of delimited identifiers that represent the enumeration members that have been set.</summary>
	/// <returns>A string that consists of delimited identifiers, where each represents a member from the set enumerator list.</returns>
	/// <param name="value">The enumeration value as a series of bitwise OR operations.</param>
	/// <param name="values">The enumeration's name values.</param>
	/// <param name="ids">The enumeration's constant values.</param>
	protected static string FromEnum(long value, string[] values, long[] ids)
	{
		return XmlCustomFormatter.FromEnum(value, values, ids, null);
	}

	/// <summary>Takes a numeric enumeration value and the names and constants from the enumerator list for the enumeration and returns a string that consists of delimited identifiers that represent the enumeration members that have been set.</summary>
	/// <returns>A string that consists of delimited identifiers, where each item is one of the values set by the bitwise operation.</returns>
	/// <param name="value">The enumeration value as a series of bitwise OR operations.</param>
	/// <param name="values">The values of the enumeration.</param>
	/// <param name="ids">The constants of the enumeration.</param>
	/// <param name="typeName">The name of the type </param>
	protected static string FromEnum(long value, string[] values, long[] ids, string typeName)
	{
		return XmlCustomFormatter.FromEnum(value, values, ids, typeName);
	}

	/// <summary>Encodes a valid XML name by replacing characters that are not valid with escape sequences.</summary>
	/// <returns>An encoded string.</returns>
	/// <param name="name">A string to be used as an XML name.</param>
	protected static string FromXmlName(string name)
	{
		return XmlCustomFormatter.FromXmlName(name);
	}

	/// <summary>Encodes a valid XML local name by replacing characters that are not valid with escape sequences.</summary>
	/// <returns>An encoded string.</returns>
	/// <param name="ncName">A string to be used as a local (unqualified) XML name.</param>
	protected static string FromXmlNCName(string ncName)
	{
		return XmlCustomFormatter.FromXmlNCName(ncName);
	}

	/// <summary>Encodes an XML name.</summary>
	/// <returns>An encoded string.</returns>
	/// <param name="nmToken">An XML name to be encoded.</param>
	protected static string FromXmlNmToken(string nmToken)
	{
		return XmlCustomFormatter.FromXmlNmToken(nmToken);
	}

	/// <summary>Encodes a space-delimited sequence of XML names into a single XML name.</summary>
	/// <returns>An encoded string.</returns>
	/// <param name="nmTokens">A space-delimited sequence of XML names to be encoded.</param>
	protected static string FromXmlNmTokens(string nmTokens)
	{
		return XmlCustomFormatter.FromXmlNmTokens(nmTokens);
	}

	/// <summary>Writes an xsi:type attribute for an XML element that is being serialized into a document.</summary>
	/// <param name="name">The local name of an XML Schema data type.</param>
	/// <param name="ns">The namespace of an XML Schema data type.</param>
	protected void WriteXsiType(string name, string ns)
	{
		WriteAttribute("type", "http://www.w3.org/2001/XMLSchema-instance", GetQualifiedName(name, ns));
	}

	private XmlQualifiedName GetPrimitiveTypeName(Type type)
	{
		return GetPrimitiveTypeName(type, throwIfUnknown: true);
	}

	private XmlQualifiedName GetPrimitiveTypeName(Type type, bool throwIfUnknown)
	{
		XmlQualifiedName primitiveTypeNameInternal = GetPrimitiveTypeNameInternal(type);
		if (throwIfUnknown && primitiveTypeNameInternal == null)
		{
			throw CreateUnknownTypeException(type);
		}
		return primitiveTypeNameInternal;
	}

	internal static XmlQualifiedName GetPrimitiveTypeNameInternal(Type type)
	{
		string ns = "http://www.w3.org/2001/XMLSchema";
		string name;
		switch (Type.GetTypeCode(type))
		{
		case TypeCode.String:
			name = "string";
			break;
		case TypeCode.Int32:
			name = "int";
			break;
		case TypeCode.Boolean:
			name = "boolean";
			break;
		case TypeCode.Int16:
			name = "short";
			break;
		case TypeCode.Int64:
			name = "long";
			break;
		case TypeCode.Single:
			name = "float";
			break;
		case TypeCode.Double:
			name = "double";
			break;
		case TypeCode.Decimal:
			name = "decimal";
			break;
		case TypeCode.DateTime:
			name = "dateTime";
			break;
		case TypeCode.Byte:
			name = "unsignedByte";
			break;
		case TypeCode.SByte:
			name = "byte";
			break;
		case TypeCode.UInt16:
			name = "unsignedShort";
			break;
		case TypeCode.UInt32:
			name = "unsignedInt";
			break;
		case TypeCode.UInt64:
			name = "unsignedLong";
			break;
		case TypeCode.Char:
			name = "char";
			ns = "http://microsoft.com/wsdl/types/";
			break;
		default:
			if (type == typeof(XmlQualifiedName))
			{
				name = "QName";
				break;
			}
			if (type == typeof(byte[]))
			{
				name = "base64Binary";
				break;
			}
			if (type == typeof(TimeSpan) && System.LocalAppContextSwitches.EnableTimeSpanSerialization)
			{
				name = "TimeSpan";
				break;
			}
			if (type == typeof(Guid))
			{
				name = "guid";
				ns = "http://microsoft.com/wsdl/types/";
				break;
			}
			if (type == typeof(XmlNode[]))
			{
				name = "anyType";
				break;
			}
			return null;
		}
		return new XmlQualifiedName(name, ns);
	}

	/// <summary>Writes an XML element whose text body is a value of a simple XML Schema data type.</summary>
	/// <param name="name">The local name of the element to write.</param>
	/// <param name="ns">The namespace of the element to write.</param>
	/// <param name="o">The object to be serialized in the element body.</param>
	/// <param name="xsiType">true if the XML element explicitly specifies the text value's type using the xsi:type attribute; otherwise, false.</param>
	protected void WriteTypedPrimitive(string name, string ns, object o, bool xsiType)
	{
		string text = null;
		string ns2 = "http://www.w3.org/2001/XMLSchema";
		bool flag = true;
		bool flag2 = false;
		Type type = o.GetType();
		bool flag3 = false;
		string text2;
		switch (Type.GetTypeCode(type))
		{
		case TypeCode.String:
			text = (string)o;
			text2 = "string";
			flag = false;
			break;
		case TypeCode.Int32:
			text = XmlConvert.ToString((int)o);
			text2 = "int";
			break;
		case TypeCode.Boolean:
			text = XmlConvert.ToString((bool)o);
			text2 = "boolean";
			break;
		case TypeCode.Int16:
			text = XmlConvert.ToString((short)o);
			text2 = "short";
			break;
		case TypeCode.Int64:
			text = XmlConvert.ToString((long)o);
			text2 = "long";
			break;
		case TypeCode.Single:
			text = XmlConvert.ToString((float)o);
			text2 = "float";
			break;
		case TypeCode.Double:
			text = XmlConvert.ToString((double)o);
			text2 = "double";
			break;
		case TypeCode.Decimal:
			text = XmlConvert.ToString((decimal)o);
			text2 = "decimal";
			break;
		case TypeCode.DateTime:
			text = FromDateTime((DateTime)o);
			text2 = "dateTime";
			break;
		case TypeCode.Char:
			text = FromChar((char)o);
			text2 = "char";
			ns2 = "http://microsoft.com/wsdl/types/";
			break;
		case TypeCode.Byte:
			text = XmlConvert.ToString((byte)o);
			text2 = "unsignedByte";
			break;
		case TypeCode.SByte:
			text = XmlConvert.ToString((sbyte)o);
			text2 = "byte";
			break;
		case TypeCode.UInt16:
			text = XmlConvert.ToString((ushort)o);
			text2 = "unsignedShort";
			break;
		case TypeCode.UInt32:
			text = XmlConvert.ToString((uint)o);
			text2 = "unsignedInt";
			break;
		case TypeCode.UInt64:
			text = XmlConvert.ToString((ulong)o);
			text2 = "unsignedLong";
			break;
		default:
			if (type == typeof(XmlQualifiedName))
			{
				text2 = "QName";
				flag3 = true;
				if (name == null)
				{
					w.WriteStartElement(text2, ns2);
				}
				else
				{
					w.WriteStartElement(name, ns);
				}
				text = FromXmlQualifiedName((XmlQualifiedName)o, ignoreEmpty: false);
				break;
			}
			if (type == typeof(byte[]))
			{
				text = string.Empty;
				flag2 = true;
				text2 = "base64Binary";
				break;
			}
			if (type == typeof(Guid))
			{
				text = XmlConvert.ToString((Guid)o);
				text2 = "guid";
				ns2 = "http://microsoft.com/wsdl/types/";
				break;
			}
			if (type == typeof(TimeSpan) && System.LocalAppContextSwitches.EnableTimeSpanSerialization)
			{
				text = XmlConvert.ToString((TimeSpan)o);
				text2 = "TimeSpan";
				break;
			}
			if (typeof(XmlNode[]).IsAssignableFrom(type))
			{
				if (name == null)
				{
					w.WriteStartElement("anyType", "http://www.w3.org/2001/XMLSchema");
				}
				else
				{
					w.WriteStartElement(name, ns);
				}
				XmlNode[] array = (XmlNode[])o;
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i] != null)
					{
						array[i].WriteTo(w);
					}
				}
				w.WriteEndElement();
				return;
			}
			throw CreateUnknownTypeException(type);
		}
		if (!flag3)
		{
			if (name == null)
			{
				w.WriteStartElement(text2, ns2);
			}
			else
			{
				w.WriteStartElement(name, ns);
			}
		}
		if (xsiType)
		{
			WriteXsiType(text2, ns2);
		}
		if (text == null)
		{
			w.WriteAttributeString("nil", "http://www.w3.org/2001/XMLSchema-instance", "true");
		}
		else if (flag2)
		{
			XmlCustomFormatter.WriteArrayBase64(w, (byte[])o, 0, ((byte[])o).Length);
		}
		else if (flag)
		{
			w.WriteRaw(text);
		}
		else
		{
			w.WriteString(text);
		}
		w.WriteEndElement();
	}

	private string GetQualifiedName(string name, string ns)
	{
		if (ns == null || ns.Length == 0)
		{
			return name;
		}
		string text = w.LookupPrefix(ns);
		if (text == null)
		{
			if (ns == "http://www.w3.org/XML/1998/namespace")
			{
				text = "xml";
			}
			else
			{
				text = NextPrefix();
				WriteAttribute("xmlns", text, null, ns);
			}
		}
		else if (text.Length == 0)
		{
			return name;
		}
		return text + ":" + name;
	}

	/// <summary>Returns an XML qualified name, with invalid characters replaced by escape sequences.</summary>
	/// <returns>An XML qualified name, with invalid characters replaced by escape sequences.</returns>
	/// <param name="xmlQualifiedName">An <see cref="T:System.Xml.XmlQualifiedName" /> that represents the XML to be written.</param>
	protected string FromXmlQualifiedName(XmlQualifiedName xmlQualifiedName)
	{
		return FromXmlQualifiedName(xmlQualifiedName, ignoreEmpty: true);
	}

	/// <summary>Produces a string that can be written as an XML qualified name, with invalid characters replaced by escape sequences.</summary>
	/// <returns>An XML qualified name, with invalid characters replaced by escape sequences.</returns>
	/// <param name="xmlQualifiedName">An <see cref="T:System.Xml.XmlQualifiedName" /> that represents the XML to be written.</param>
	/// <param name="ignoreEmpty">true to ignore empty spaces in the string; otherwise, false.</param>
	protected string FromXmlQualifiedName(XmlQualifiedName xmlQualifiedName, bool ignoreEmpty)
	{
		if (xmlQualifiedName == null)
		{
			return null;
		}
		if (xmlQualifiedName.IsEmpty && ignoreEmpty)
		{
			return null;
		}
		return GetQualifiedName(EscapeName ? XmlConvert.EncodeLocalName(xmlQualifiedName.Name) : xmlQualifiedName.Name, xmlQualifiedName.Namespace);
	}

	/// <summary>Writes an opening element tag, including any attributes.</summary>
	/// <param name="name">The local name of the XML element to write.</param>
	protected void WriteStartElement(string name)
	{
		WriteStartElement(name, null, null, writePrefixed: false, null);
	}

	/// <summary>Writes an opening element tag, including any attributes.</summary>
	/// <param name="name">The local name of the XML element to write.</param>
	/// <param name="ns">The namespace of the XML element to write.</param>
	protected void WriteStartElement(string name, string ns)
	{
		WriteStartElement(name, ns, null, writePrefixed: false, null);
	}

	/// <summary>Writes an opening element tag, including any attributes.</summary>
	/// <param name="name">The local name of the XML element to write.</param>
	/// <param name="ns">The namespace of the XML element to write.</param>
	/// <param name="writePrefixed">true to write the element name with a prefix if none is available for the specified namespace; otherwise, false.</param>
	protected void WriteStartElement(string name, string ns, bool writePrefixed)
	{
		WriteStartElement(name, ns, null, writePrefixed, null);
	}

	/// <summary>Writes an opening element tag, including any attributes.</summary>
	/// <param name="name">The local name of the XML element to write.</param>
	/// <param name="ns">The namespace of the XML element to write.</param>
	/// <param name="o">The object being serialized as an XML element.</param>
	protected void WriteStartElement(string name, string ns, object o)
	{
		WriteStartElement(name, ns, o, writePrefixed: false, null);
	}

	/// <summary>Writes an opening element tag, including any attributes.</summary>
	/// <param name="name">The local name of the XML element to write.</param>
	/// <param name="ns">The namespace of the XML element to write.</param>
	/// <param name="o">The object being serialized as an XML element.</param>
	/// <param name="writePrefixed">true to write the element name with a prefix if none is available for the specified namespace; otherwise, false.</param>
	protected void WriteStartElement(string name, string ns, object o, bool writePrefixed)
	{
		WriteStartElement(name, ns, o, writePrefixed, null);
	}

	/// <summary>Writes an opening element tag, including any attributes.</summary>
	/// <param name="name">The local name of the XML element to write.</param>
	/// <param name="ns">The namespace of the XML element to write.</param>
	/// <param name="o">The object being serialized as an XML element.</param>
	/// <param name="writePrefixed">true to write the element name with a prefix if none is available for the specified namespace; otherwise, false.</param>
	/// <param name="xmlns">An instance of the <see cref="T:System.Xml.Serialization.XmlSerializerNamespaces" /> class that contains prefix and namespace pairs to be used in the generated XML.</param>
	protected void WriteStartElement(string name, string ns, object o, bool writePrefixed, XmlSerializerNamespaces xmlns)
	{
		if (o != null && objectsInUse != null)
		{
			if (objectsInUse.ContainsKey(o))
			{
				throw new InvalidOperationException(Res.GetString("A circular reference was detected while serializing an object of type {0}.", o.GetType().FullName));
			}
			objectsInUse.Add(o, o);
		}
		string text = null;
		bool flag = false;
		if (namespaces != null)
		{
			foreach (string key in namespaces.Namespaces.Keys)
			{
				string text3 = (string)namespaces.Namespaces[key];
				if (key.Length > 0 && text3 == ns)
				{
					text = key;
				}
				if (key.Length == 0)
				{
					if (text3 == null || text3.Length == 0)
					{
						flag = true;
					}
					if (ns != text3)
					{
						writePrefixed = true;
					}
				}
			}
			usedPrefixes = ListUsedPrefixes(namespaces.Namespaces, aliasBase);
		}
		if (writePrefixed && text == null && ns != null && ns.Length > 0)
		{
			text = w.LookupPrefix(ns);
			if (text == null || text.Length == 0)
			{
				text = NextPrefix();
			}
		}
		if (text == null && xmlns != null)
		{
			text = xmlns.LookupPrefix(ns);
		}
		if (flag && text == null && ns != null && ns.Length != 0)
		{
			text = NextPrefix();
		}
		w.WriteStartElement(text, name, ns);
		if (namespaces != null)
		{
			foreach (string key2 in namespaces.Namespaces.Keys)
			{
				string text5 = (string)namespaces.Namespaces[key2];
				if (key2.Length == 0 && (text5 == null || text5.Length == 0))
				{
					continue;
				}
				if (text5 == null || text5.Length == 0)
				{
					if (key2.Length > 0)
					{
						throw new InvalidOperationException(Res.GetString("Invalid namespace attribute: xmlns:{0}=\"\".", key2));
					}
					WriteAttribute("xmlns", key2, null, text5);
				}
				else if (w.LookupPrefix(text5) == null)
				{
					if (text == null && key2.Length == 0)
					{
						break;
					}
					WriteAttribute("xmlns", key2, null, text5);
				}
			}
		}
		WriteNamespaceDeclarations(xmlns);
	}

	private Hashtable ListUsedPrefixes(Hashtable nsList, string prefix)
	{
		Hashtable hashtable = new Hashtable();
		int length = prefix.Length;
		foreach (string key in namespaces.Namespaces.Keys)
		{
			if (key.Length <= length)
			{
				continue;
			}
			string text2 = key;
			_ = text2.Length;
			if (text2.Length <= length || text2.Length > length + "2147483647".Length || !text2.StartsWith(prefix, StringComparison.Ordinal))
			{
				continue;
			}
			bool flag = true;
			for (int i = length; i < text2.Length; i++)
			{
				if (!char.IsDigit(text2, i))
				{
					flag = false;
					break;
				}
			}
			if (!flag)
			{
				continue;
			}
			long num = long.Parse(text2.Substring(length), CultureInfo.InvariantCulture);
			if (num <= int.MaxValue)
			{
				int num2 = (int)num;
				if (!hashtable.ContainsKey(num2))
				{
					hashtable.Add(num2, num2);
				}
			}
		}
		if (hashtable.Count > 0)
		{
			return hashtable;
		}
		return null;
	}

	/// <summary>Writes an XML element with an xsi:nil='true' attribute.</summary>
	/// <param name="name">The local name of the XML element to write.</param>
	protected void WriteNullTagEncoded(string name)
	{
		WriteNullTagEncoded(name, null);
	}

	/// <summary>Writes an XML element with an xsi:nil='true' attribute.</summary>
	/// <param name="name">The local name of the XML element to write.</param>
	/// <param name="ns">The namespace of the XML element to write.</param>
	protected void WriteNullTagEncoded(string name, string ns)
	{
		if (name != null && name.Length != 0)
		{
			WriteStartElement(name, ns, null, writePrefixed: true);
			w.WriteAttributeString("nil", "http://www.w3.org/2001/XMLSchema-instance", "true");
			w.WriteEndElement();
		}
	}

	/// <summary>Writes an XML element with an xsi:nil='true' attribute.</summary>
	/// <param name="name">The local name of the XML element to write.</param>
	protected void WriteNullTagLiteral(string name)
	{
		WriteNullTagLiteral(name, null);
	}

	/// <summary>Writes an XML element with an xsi:nil='true' attribute.</summary>
	/// <param name="name">The local name of the XML element to write.</param>
	/// <param name="ns">The namespace of the XML element to write.</param>
	protected void WriteNullTagLiteral(string name, string ns)
	{
		if (name != null && name.Length != 0)
		{
			WriteStartElement(name, ns, null, writePrefixed: false);
			w.WriteAttributeString("nil", "http://www.w3.org/2001/XMLSchema-instance", "true");
			w.WriteEndElement();
		}
	}

	/// <summary>Writes an XML element whose body is empty.</summary>
	/// <param name="name">The local name of the XML element to write.</param>
	protected void WriteEmptyTag(string name)
	{
		WriteEmptyTag(name, null);
	}

	/// <summary>Writes an XML element whose body is empty.</summary>
	/// <param name="name">The local name of the XML element to write.</param>
	/// <param name="ns">The namespace of the XML element to write.</param>
	protected void WriteEmptyTag(string name, string ns)
	{
		if (name != null && name.Length != 0)
		{
			WriteStartElement(name, ns, null, writePrefixed: false);
			w.WriteEndElement();
		}
	}

	/// <summary>Writes a &lt;closing&gt; element tag.</summary>
	protected void WriteEndElement()
	{
		w.WriteEndElement();
	}

	/// <summary>Writes a &lt;closing&gt; element tag.</summary>
	/// <param name="o">The object being serialized.</param>
	protected void WriteEndElement(object o)
	{
		w.WriteEndElement();
		if (o != null && objectsInUse != null)
		{
			objectsInUse.Remove(o);
		}
	}

	/// <summary>Writes an object that uses custom XML formatting as an XML element.</summary>
	/// <param name="serializable">An object that implements the <see cref="T:System.Xml.Serialization.IXmlSerializable" /> interface that uses custom XML formatting.</param>
	/// <param name="name">The local name of the XML element to write.</param>
	/// <param name="ns">The namespace of the XML element to write.</param>
	/// <param name="isNullable">true to write an xsi:nil='true' attribute if the <see cref="T:System.Xml.Serialization.IXmlSerializable" /> class object is null; otherwise, false.</param>
	protected void WriteSerializable(IXmlSerializable serializable, string name, string ns, bool isNullable)
	{
		WriteSerializable(serializable, name, ns, isNullable, wrapped: true);
	}

	/// <summary>Instructs <see cref="T:System.Xml.XmlNode" /> to write an object that uses custom XML formatting as an XML element.</summary>
	/// <param name="serializable">An object that implements the <see cref="T:System.Xml.Serialization.IXmlSerializable" /> interface that uses custom XML formatting.</param>
	/// <param name="name">The local name of the XML element to write.</param>
	/// <param name="ns">The namespace of the XML element to write.</param>
	/// <param name="isNullable">true to write an xsi:nil='true' attribute if the <see cref="T:System.Xml.Serialization.IXmlSerializable" /> object is null; otherwise, false.</param>
	/// <param name="wrapped">true to ignore writing the opening element tag; otherwise, false to write the opening element tag.</param>
	protected void WriteSerializable(IXmlSerializable serializable, string name, string ns, bool isNullable, bool wrapped)
	{
		if (serializable == null)
		{
			if (isNullable)
			{
				WriteNullTagLiteral(name, ns);
			}
			return;
		}
		if (wrapped)
		{
			w.WriteStartElement(name, ns);
		}
		serializable.WriteXml(w);
		if (wrapped)
		{
			w.WriteEndElement();
		}
	}

	/// <summary>Writes an XML element that contains a string as the body. <see cref="T:System.Xml.XmlWriter" /> inserts an xsi:nil='true' attribute if the string's value is null.</summary>
	/// <param name="name">The local name of the XML element to write.</param>
	/// <param name="ns">The namespace of the XML element to write.</param>
	/// <param name="value">The string to write in the body of the XML element.</param>
	/// <param name="xsiType">The name of the XML Schema data type to be written to the xsi:type attribute.</param>
	protected void WriteNullableStringEncoded(string name, string ns, string value, XmlQualifiedName xsiType)
	{
		if (value == null)
		{
			WriteNullTagEncoded(name, ns);
		}
		else
		{
			WriteElementString(name, ns, value, xsiType);
		}
	}

	/// <summary>Writes an XML element that contains a string as the body. <see cref="T:System.Xml.XmlWriter" /> inserts an xsi:nil='true' attribute if the string's value is null.</summary>
	/// <param name="name">The local name of the XML element to write.</param>
	/// <param name="ns">The namespace of the XML element to write.</param>
	/// <param name="value">The string to write in the body of the XML element.</param>
	protected void WriteNullableStringLiteral(string name, string ns, string value)
	{
		if (value == null)
		{
			WriteNullTagLiteral(name, ns);
		}
		else
		{
			WriteElementString(name, ns, value, null);
		}
	}

	/// <summary>Writes an XML element that contains a string as the body. <see cref="T:System.Xml.XmlWriter" /> inserts an xsi:nil='true' attribute if the string's value is null.</summary>
	/// <param name="name">The local name of the XML element to write.</param>
	/// <param name="ns">The namespace of the XML element to write.</param>
	/// <param name="value">The string to write in the body of the XML element.</param>
	/// <param name="xsiType">The name of the XML Schema data type to be written to the xsi:type attribute.</param>
	protected void WriteNullableStringEncodedRaw(string name, string ns, string value, XmlQualifiedName xsiType)
	{
		if (value == null)
		{
			WriteNullTagEncoded(name, ns);
		}
		else
		{
			WriteElementStringRaw(name, ns, value, xsiType);
		}
	}

	/// <summary>Writes a byte array as the body of an XML element. <see cref="T:System.Xml.XmlWriter" /> inserts an xsi:nil='true' attribute if the string's value is null.</summary>
	/// <param name="name">The local name of the XML element to write.</param>
	/// <param name="ns">The namespace of the XML element to write.</param>
	/// <param name="value">The byte array to write in the body of the XML element.</param>
	/// <param name="xsiType">The name of the XML Schema data type to be written to the xsi:type attribute.</param>
	protected void WriteNullableStringEncodedRaw(string name, string ns, byte[] value, XmlQualifiedName xsiType)
	{
		if (value == null)
		{
			WriteNullTagEncoded(name, ns);
		}
		else
		{
			WriteElementStringRaw(name, ns, value, xsiType);
		}
	}

	/// <summary>Writes an XML element that contains a string as the body. <see cref="T:System.Xml.XmlWriter" /> inserts a xsi:nil='true' attribute if the string's value is null.</summary>
	/// <param name="name">The local name of the XML element to write.</param>
	/// <param name="ns">The namespace of the XML element to write.</param>
	/// <param name="value">The string to write in the body of the XML element.</param>
	protected void WriteNullableStringLiteralRaw(string name, string ns, string value)
	{
		if (value == null)
		{
			WriteNullTagLiteral(name, ns);
		}
		else
		{
			WriteElementStringRaw(name, ns, value, null);
		}
	}

	/// <summary>Writes a byte array as the body of an XML element. <see cref="T:System.Xml.XmlWriter" /> inserts an xsi:nil='true' attribute if the string's value is null.</summary>
	/// <param name="name">The local name of the XML element to write.</param>
	/// <param name="ns">The namespace of the XML element to write.</param>
	/// <param name="value">The byte array to write in the body of the XML element.</param>
	protected void WriteNullableStringLiteralRaw(string name, string ns, byte[] value)
	{
		if (value == null)
		{
			WriteNullTagLiteral(name, ns);
		}
		else
		{
			WriteElementStringRaw(name, ns, value, null);
		}
	}

	/// <summary>Writes an XML element whose body contains a valid XML qualified name. <see cref="T:System.Xml.XmlWriter" /> inserts an xsi:nil='true' attribute if the string's value is null.</summary>
	/// <param name="name">The local name of the XML element to write.</param>
	/// <param name="ns">The namespace of the XML element to write.</param>
	/// <param name="value">The XML qualified name to write in the body of the XML element.</param>
	/// <param name="xsiType">The name of the XML Schema data type to be written to the xsi:type attribute.</param>
	protected void WriteNullableQualifiedNameEncoded(string name, string ns, XmlQualifiedName value, XmlQualifiedName xsiType)
	{
		if (value == null)
		{
			WriteNullTagEncoded(name, ns);
		}
		else
		{
			WriteElementQualifiedName(name, ns, value, xsiType);
		}
	}

	/// <summary>Writes an XML element whose body contains a valid XML qualified name. <see cref="T:System.Xml.XmlWriter" /> inserts an xsi:nil='true' attribute if the string's value is null.</summary>
	/// <param name="name">The local name of the XML element to write.</param>
	/// <param name="ns">The namespace of the XML element to write.</param>
	/// <param name="value">The XML qualified name to write in the body of the XML element.</param>
	protected void WriteNullableQualifiedNameLiteral(string name, string ns, XmlQualifiedName value)
	{
		if (value == null)
		{
			WriteNullTagLiteral(name, ns);
		}
		else
		{
			WriteElementQualifiedName(name, ns, value, null);
		}
	}

	/// <summary>Writes an XML node object within the body of a named XML element.</summary>
	/// <param name="node">The XML node to write, possibly a child XML element.</param>
	/// <param name="name">The local name of the parent XML element to write.</param>
	/// <param name="ns">The namespace of the parent XML element to write.</param>
	/// <param name="isNullable">true to write an xsi:nil='true' attribute if the object to serialize is null; otherwise, false.</param>
	/// <param name="any">true to indicate that the node, if an XML element, adheres to an XML Schema any element declaration; otherwise, false.</param>
	protected void WriteElementEncoded(XmlNode node, string name, string ns, bool isNullable, bool any)
	{
		if (node == null)
		{
			if (isNullable)
			{
				WriteNullTagEncoded(name, ns);
			}
		}
		else
		{
			WriteElement(node, name, ns, isNullable, any);
		}
	}

	/// <summary>Instructs an <see cref="T:System.Xml.XmlWriter" /> object to write an <see cref="T:System.Xml.XmlNode" /> object within the body of a named XML element.</summary>
	/// <param name="node">The XML node to write, possibly a child XML element.</param>
	/// <param name="name">The local name of the parent XML element to write.</param>
	/// <param name="ns">The namespace of the parent XML element to write.</param>
	/// <param name="isNullable">true to write an xsi:nil='true' attribute if the object to serialize is null; otherwise, false.</param>
	/// <param name="any">true to indicate that the node, if an XML element, adheres to an XML Schema any element declaration; otherwise, false.</param>
	protected void WriteElementLiteral(XmlNode node, string name, string ns, bool isNullable, bool any)
	{
		if (node == null)
		{
			if (isNullable)
			{
				WriteNullTagLiteral(name, ns);
			}
		}
		else
		{
			WriteElement(node, name, ns, isNullable, any);
		}
	}

	private void WriteElement(XmlNode node, string name, string ns, bool isNullable, bool any)
	{
		if (typeof(XmlAttribute).IsAssignableFrom(node.GetType()))
		{
			throw new InvalidOperationException(Res.GetString("Cannot write a node of type XmlAttribute as an element value. Use XmlAnyAttributeAttribute with an array of XmlNode or XmlAttribute to write the node as an attribute."));
		}
		if (node is XmlDocument)
		{
			node = ((XmlDocument)node).DocumentElement;
			if (node == null)
			{
				if (isNullable)
				{
					WriteNullTagEncoded(name, ns);
				}
				return;
			}
		}
		if (any)
		{
			if (node is XmlElement && name != null && name.Length > 0 && (node.LocalName != name || node.NamespaceURI != ns))
			{
				throw new InvalidOperationException(Res.GetString("This element was named '{0}' from namespace '{1}' but should have been named '{2}' from namespace '{3}'.", node.LocalName, node.NamespaceURI, name, ns));
			}
		}
		else
		{
			w.WriteStartElement(name, ns);
		}
		node.WriteTo(w);
		if (!any)
		{
			w.WriteEndElement();
		}
	}

	/// <summary>Creates an <see cref="T:System.InvalidOperationException" /> that indicates that a type being serialized is not being used in a valid manner or is unexpectedly encountered.</summary>
	/// <returns>The newly created exception.</returns>
	/// <param name="o">The object whose type cannot be serialized.</param>
	protected Exception CreateUnknownTypeException(object o)
	{
		return CreateUnknownTypeException(o.GetType());
	}

	/// <summary>Creates an <see cref="T:System.InvalidOperationException" /> that indicates that a type being serialized is not being used in a valid manner or is unexpectedly encountered.</summary>
	/// <returns>The newly created exception.</returns>
	/// <param name="type">The type that cannot be serialized.</param>
	protected Exception CreateUnknownTypeException(Type type)
	{
		if (typeof(IXmlSerializable).IsAssignableFrom(type))
		{
			return new InvalidOperationException(Res.GetString("The type {0} may not be used in this context. To use {0} as a parameter, return type, or member of a class or struct, the parameter, return type, or member must be declared as type {0} (it cannot be object). Objects of type {0} may not be used in un-typed collections, such as ArrayLists.", type.FullName));
		}
		if (!new TypeScope().GetTypeDesc(type).IsStructLike)
		{
			return new InvalidOperationException(Res.GetString("The type {0} may not be used in this context.", type.FullName));
		}
		return new InvalidOperationException(Res.GetString("The type {0} was not expected. Use the XmlInclude or SoapInclude attribute to specify types that are not known statically.", type.FullName));
	}

	/// <summary>Creates an <see cref="T:System.InvalidOperationException" /> that indicates that a value for an XML element does not match an enumeration type.</summary>
	/// <returns>The newly created exception.</returns>
	/// <param name="value">The value that is not valid.</param>
	/// <param name="elementName">The name of the XML element with an invalid value.</param>
	/// <param name="enumValue">The valid value.</param>
	protected Exception CreateMismatchChoiceException(string value, string elementName, string enumValue)
	{
		return new InvalidOperationException(Res.GetString("Value of {0} mismatches the type of {1}; you need to set it to {2}.", elementName, value, enumValue));
	}

	/// <summary>Creates an <see cref="T:System.InvalidOperationException" /> that indicates that an XML element that should adhere to the XML Schema any element declaration cannot be processed.</summary>
	/// <returns>The newly created exception.</returns>
	/// <param name="name">The XML element that cannot be processed.</param>
	/// <param name="ns">The namespace of the XML element.</param>
	protected Exception CreateUnknownAnyElementException(string name, string ns)
	{
		return new InvalidOperationException(Res.GetString("The XML element '{0}' from namespace '{1}' was not expected. The XML element name and namespace must match those provided via XmlAnyElementAttribute(s).", name, ns));
	}

	/// <summary>Creates an <see cref="T:System.InvalidOperationException" /> that indicates a failure while writing an array where an XML Schema choice element declaration is applied.</summary>
	/// <returns>The newly created exception.</returns>
	/// <param name="type">The type being serialized.</param>
	/// <param name="identifier">A name for the choice element declaration.</param>
	protected Exception CreateInvalidChoiceIdentifierValueException(string type, string identifier)
	{
		return new InvalidOperationException(Res.GetString("Invalid or missing value of the choice identifier '{1}' of type '{0}[]'.", type, identifier));
	}

	/// <summary>Creates an <see cref="T:System.InvalidOperationException" /> that indicates an unexpected name for an element that adheres to an XML Schema choice element declaration.</summary>
	/// <returns>The newly created exception.</returns>
	/// <param name="value">The name that is not valid.</param>
	/// <param name="identifier">The choice element declaration that the name belongs to.</param>
	/// <param name="name">The expected local name of an element.</param>
	/// <param name="ns">The expected namespace of an element.</param>
	protected Exception CreateChoiceIdentifierValueException(string value, string identifier, string name, string ns)
	{
		return new InvalidOperationException(Res.GetString("Value '{0}' of the choice identifier '{1}' does not match element '{2}' from namespace '{3}'.", value, identifier, name, ns));
	}

	/// <summary>Creates an <see cref="T:System.InvalidOperationException" /> for an invalid enumeration value.</summary>
	/// <returns>The newly created exception.</returns>
	/// <param name="value">An object that represents the invalid enumeration.</param>
	/// <param name="typeName">The XML type name.</param>
	protected Exception CreateInvalidEnumValueException(object value, string typeName)
	{
		return new InvalidOperationException(Res.GetString("Instance validation error: '{0}' is not a valid value for {1}.", value, typeName));
	}

	/// <summary>Creates an <see cref="T:System.InvalidOperationException" /> that indicates the <see cref="T:System.Xml.Serialization.XmlAnyElementAttribute" /> which has been invalidly applied to a member; only members that are of type <see cref="T:System.Xml.XmlNode" />, or derived from <see cref="T:System.Xml.XmlNode" />, are valid.</summary>
	/// <returns>The newly created exception.</returns>
	/// <param name="o">The object that represents the invalid member.</param>
	protected Exception CreateInvalidAnyTypeException(object o)
	{
		return CreateInvalidAnyTypeException(o.GetType());
	}

	/// <summary>Creates an <see cref="T:System.InvalidOperationException" /> that indicates the <see cref="T:System.Xml.Serialization.XmlAnyElementAttribute" /> which has been invalidly applied to a member; only members that are of type <see cref="T:System.Xml.XmlNode" />, or derived from <see cref="T:System.Xml.XmlNode" />, are valid.</summary>
	/// <returns>The newly created exception.</returns>
	/// <param name="type">The <see cref="T:System.Type" /> that is invalid.</param>
	protected Exception CreateInvalidAnyTypeException(Type type)
	{
		return new InvalidOperationException(Res.GetString("Cannot serialize member of type {0}: XmlAnyElement can only be used with classes of type XmlNode or a type deriving from XmlNode.", type.FullName));
	}

	/// <summary>Writes a SOAP message XML element that contains a reference to a multiRef element for a given object.</summary>
	/// <param name="n">The local name of the referencing element being written.</param>
	/// <param name="ns">The namespace of the referencing element being written.</param>
	/// <param name="o">The object being serialized.</param>
	protected void WriteReferencingElement(string n, string ns, object o)
	{
		WriteReferencingElement(n, ns, o, isNullable: false);
	}

	/// <summary>Writes a SOAP message XML element that contains a reference to a multiRef element for a given object.</summary>
	/// <param name="n">The local name of the referencing element being written.</param>
	/// <param name="ns">The namespace of the referencing element being written.</param>
	/// <param name="o">The object being serialized.</param>
	/// <param name="isNullable">true to write an xsi:nil='true' attribute if the object to serialize is null; otherwise, false.</param>
	protected void WriteReferencingElement(string n, string ns, object o, bool isNullable)
	{
		if (o == null)
		{
			if (isNullable)
			{
				WriteNullTagEncoded(n, ns);
			}
			return;
		}
		WriteStartElement(n, ns, null, writePrefixed: true);
		if (soap12)
		{
			w.WriteAttributeString("ref", "http://www.w3.org/2003/05/soap-encoding", GetId(o, addToReferencesList: true));
		}
		else
		{
			w.WriteAttributeString("href", "#" + GetId(o, addToReferencesList: true));
		}
		w.WriteEndElement();
	}

	private bool IsIdDefined(object o)
	{
		if (references != null)
		{
			return references.Contains(o);
		}
		return false;
	}

	private string GetId(object o, bool addToReferencesList)
	{
		if (references == null)
		{
			references = new Hashtable();
			referencesToWrite = new ArrayList();
		}
		string text = (string)references[o];
		if (text == null)
		{
			string text2 = idBase;
			int num = ++nextId;
			text = text2 + "id" + num.ToString(CultureInfo.InvariantCulture);
			references.Add(o, text);
			if (addToReferencesList)
			{
				referencesToWrite.Add(o);
			}
		}
		return text;
	}

	/// <summary>Writes an id attribute that appears in a SOAP-encoded multiRef element.</summary>
	/// <param name="o">The object being serialized.</param>
	protected void WriteId(object o)
	{
		WriteId(o, addToReferencesList: true);
	}

	private void WriteId(object o, bool addToReferencesList)
	{
		if (soap12)
		{
			w.WriteAttributeString("id", "http://www.w3.org/2003/05/soap-encoding", GetId(o, addToReferencesList));
		}
		else
		{
			w.WriteAttributeString("id", GetId(o, addToReferencesList));
		}
	}

	/// <summary>Writes the specified <see cref="T:System.Xml.XmlNode" /> as an XML attribute.</summary>
	/// <param name="node">The XML node to write.</param>
	protected void WriteXmlAttribute(XmlNode node)
	{
		WriteXmlAttribute(node, null);
	}

	/// <summary>Writes the specified <see cref="T:System.Xml.XmlNode" /> object as an XML attribute.</summary>
	/// <param name="node">The XML node to write.</param>
	/// <param name="container">An <see cref="T:System.Xml.Schema.XmlSchemaObject" /> object (or null) used to generate a qualified name value for an arrayType attribute from the Web Services Description Language (WSDL) namespace ("http://schemas.xmlsoap.org/wsdl/").</param>
	protected void WriteXmlAttribute(XmlNode node, object container)
	{
		if (!(node is XmlAttribute xmlAttribute))
		{
			throw new InvalidOperationException(Res.GetString("The node must be either type XmlAttribute or a derived type."));
		}
		if (xmlAttribute.Value != null)
		{
			if (xmlAttribute.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/" && xmlAttribute.LocalName == "arrayType")
			{
				string dims;
				XmlQualifiedName xmlQualifiedName = TypeScope.ParseWsdlArrayType(xmlAttribute.Value, out dims, (container is XmlSchemaObject) ? ((XmlSchemaObject)container) : null);
				string value = FromXmlQualifiedName(xmlQualifiedName, ignoreEmpty: true) + dims;
				WriteAttribute("arrayType", "http://schemas.xmlsoap.org/wsdl/", value);
			}
			else
			{
				WriteAttribute(xmlAttribute.Name, xmlAttribute.NamespaceURI, xmlAttribute.Value);
			}
		}
	}

	/// <summary>Writes an XML attribute.</summary>
	/// <param name="localName">The local name of the XML attribute.</param>
	/// <param name="ns">The namespace of the XML attribute.</param>
	/// <param name="value">The value of the XML attribute as a string.</param>
	protected void WriteAttribute(string localName, string ns, string value)
	{
		if (value == null || localName == "xmlns" || localName.StartsWith("xmlns:", StringComparison.Ordinal))
		{
			return;
		}
		int num = localName.IndexOf(':');
		if (num < 0)
		{
			if (ns == "http://www.w3.org/XML/1998/namespace")
			{
				string text = w.LookupPrefix(ns);
				if (text == null || text.Length == 0)
				{
					text = "xml";
				}
				w.WriteAttributeString(text, localName, ns, value);
			}
			else
			{
				w.WriteAttributeString(localName, ns, value);
			}
		}
		else
		{
			string prefix = localName.Substring(0, num);
			w.WriteAttributeString(prefix, localName.Substring(num + 1), ns, value);
		}
	}

	/// <summary>Instructs an <see cref="T:System.Xml.XmlWriter" /> object to write an XML attribute.</summary>
	/// <param name="localName">The local name of the XML attribute.</param>
	/// <param name="ns">The namespace of the XML attribute.</param>
	/// <param name="value">The value of the XML attribute as a byte array.</param>
	protected void WriteAttribute(string localName, string ns, byte[] value)
	{
		if (value == null || localName == "xmlns" || localName.StartsWith("xmlns:", StringComparison.Ordinal))
		{
			return;
		}
		int num = localName.IndexOf(':');
		if (num < 0)
		{
			if (ns == "http://www.w3.org/XML/1998/namespace")
			{
				string text = w.LookupPrefix(ns);
				if (text == null || text.Length == 0)
				{
					text = "xml";
				}
				w.WriteStartAttribute("xml", localName, ns);
			}
			else
			{
				w.WriteStartAttribute(null, localName, ns);
			}
		}
		else
		{
			string text2 = localName.Substring(0, num);
			text2 = w.LookupPrefix(ns);
			w.WriteStartAttribute(text2, localName.Substring(num + 1), ns);
		}
		XmlCustomFormatter.WriteArrayBase64(w, value, 0, value.Length);
		w.WriteEndAttribute();
	}

	/// <summary>Instructs the <see cref="T:System.Xml.XmlWriter" /> to write an XML attribute that has no namespace specified for its name.</summary>
	/// <param name="localName">The local name of the XML attribute.</param>
	/// <param name="value">The value of the XML attribute as a string.</param>
	protected void WriteAttribute(string localName, string value)
	{
		if (value != null)
		{
			w.WriteAttributeString(localName, null, value);
		}
	}

	/// <summary>Instructs an <see cref="T:System.Xml.XmlWriter" /> object to write an XML attribute that has no namespace specified for its name.</summary>
	/// <param name="localName">The local name of the XML attribute.</param>
	/// <param name="value">The value of the XML attribute as a byte array.</param>
	protected void WriteAttribute(string localName, byte[] value)
	{
		if (value != null)
		{
			w.WriteStartAttribute(null, localName, null);
			XmlCustomFormatter.WriteArrayBase64(w, value, 0, value.Length);
			w.WriteEndAttribute();
		}
	}

	/// <summary>Writes an XML attribute where the namespace prefix is provided manually.</summary>
	/// <param name="prefix">The namespace prefix to write.</param>
	/// <param name="localName">The local name of the XML attribute.</param>
	/// <param name="ns">The namespace represented by the prefix.</param>
	/// <param name="value">The value of the XML attribute as a string.</param>
	protected void WriteAttribute(string prefix, string localName, string ns, string value)
	{
		if (value != null)
		{
			w.WriteAttributeString(prefix, localName, null, value);
		}
	}

	/// <summary>Writes a specified string value.</summary>
	/// <param name="value">The value of the string to write.</param>
	protected void WriteValue(string value)
	{
		if (value != null)
		{
			w.WriteString(value);
		}
	}

	/// <summary>Writes a base-64 byte array.</summary>
	/// <param name="value">The byte array to write.</param>
	protected void WriteValue(byte[] value)
	{
		if (value != null)
		{
			XmlCustomFormatter.WriteArrayBase64(w, value, 0, value.Length);
		}
	}

	/// <summary>Writes the XML declaration if the writer is positioned at the start of an XML document.</summary>
	protected void WriteStartDocument()
	{
		if (w.WriteState == WriteState.Start)
		{
			w.WriteStartDocument();
		}
	}

	/// <summary>Writes an XML element with a specified value in its body.</summary>
	/// <param name="localName">The local name of the XML element to be written without namespace qualification.</param>
	/// <param name="value">The text value of the XML element.</param>
	protected void WriteElementString(string localName, string value)
	{
		WriteElementString(localName, null, value, null);
	}

	/// <summary>Writes an XML element with a specified value in its body.</summary>
	/// <param name="localName">The local name of the XML element.</param>
	/// <param name="ns">The namespace of the XML element.</param>
	/// <param name="value">The text value of the XML element.</param>
	protected void WriteElementString(string localName, string ns, string value)
	{
		WriteElementString(localName, ns, value, null);
	}

	/// <summary>Writes an XML element with a specified value in its body.</summary>
	/// <param name="localName">The local name of the XML element.</param>
	/// <param name="value">The text value of the XML element.</param>
	/// <param name="xsiType">The name of the XML Schema data type to be written to the xsi:type attribute.</param>
	protected void WriteElementString(string localName, string value, XmlQualifiedName xsiType)
	{
		WriteElementString(localName, null, value, xsiType);
	}

	/// <summary>Writes an XML element with a specified value in its body.</summary>
	/// <param name="localName">The local name of the XML element.</param>
	/// <param name="ns">The namespace of the XML element.</param>
	/// <param name="value">The text value of the XML element.</param>
	/// <param name="xsiType">The name of the XML Schema data type to be written to the xsi:type attribute.</param>
	protected void WriteElementString(string localName, string ns, string value, XmlQualifiedName xsiType)
	{
		if (value != null)
		{
			if (xsiType == null)
			{
				w.WriteElementString(localName, ns, value);
				return;
			}
			w.WriteStartElement(localName, ns);
			WriteXsiType(xsiType.Name, xsiType.Namespace);
			w.WriteString(value);
			w.WriteEndElement();
		}
	}

	/// <summary>Writes an XML element with a specified value in its body.</summary>
	/// <param name="localName">The local name of the XML element.</param>
	/// <param name="value">The text value of the XML element.</param>
	protected void WriteElementStringRaw(string localName, string value)
	{
		WriteElementStringRaw(localName, null, value, null);
	}

	/// <summary>Writes an XML element with a specified value in its body.</summary>
	/// <param name="localName">The local name of the XML element.</param>
	/// <param name="value">The text value of the XML element.</param>
	protected void WriteElementStringRaw(string localName, byte[] value)
	{
		WriteElementStringRaw(localName, null, value, null);
	}

	/// <summary>Writes an XML element with a specified value in its body.</summary>
	/// <param name="localName">The local name of the XML element.</param>
	/// <param name="ns">The namespace of the XML element.</param>
	/// <param name="value">The text value of the XML element.</param>
	protected void WriteElementStringRaw(string localName, string ns, string value)
	{
		WriteElementStringRaw(localName, ns, value, null);
	}

	/// <summary>Writes an XML element with a specified value in its body.</summary>
	/// <param name="localName">The local name of the XML element.</param>
	/// <param name="ns">The namespace of the XML element.</param>
	/// <param name="value">The text value of the XML element.</param>
	protected void WriteElementStringRaw(string localName, string ns, byte[] value)
	{
		WriteElementStringRaw(localName, ns, value, null);
	}

	/// <summary>Writes an XML element with a specified value in its body.</summary>
	/// <param name="localName">The local name of the XML element.</param>
	/// <param name="value">The text value of the XML element.</param>
	/// <param name="xsiType">The name of the XML Schema data type to be written to the xsi:type attribute.</param>
	protected void WriteElementStringRaw(string localName, string value, XmlQualifiedName xsiType)
	{
		WriteElementStringRaw(localName, null, value, xsiType);
	}

	/// <summary>Writes an XML element with a specified value in its body.</summary>
	/// <param name="localName">The local name of the XML element.</param>
	/// <param name="value">The text value of the XML element.</param>
	/// <param name="xsiType">The name of the XML Schema data type to be written to the xsi:type attribute.</param>
	protected void WriteElementStringRaw(string localName, byte[] value, XmlQualifiedName xsiType)
	{
		WriteElementStringRaw(localName, null, value, xsiType);
	}

	/// <summary>Writes an XML element with a specified value in its body.</summary>
	/// <param name="localName">The local name of the XML element.</param>
	/// <param name="ns">The namespace of the XML element.</param>
	/// <param name="value">The text value of the XML element.</param>
	/// <param name="xsiType">The name of the XML Schema data type to be written to the xsi:type attribute.</param>
	protected void WriteElementStringRaw(string localName, string ns, string value, XmlQualifiedName xsiType)
	{
		if (value != null)
		{
			w.WriteStartElement(localName, ns);
			if (xsiType != null)
			{
				WriteXsiType(xsiType.Name, xsiType.Namespace);
			}
			w.WriteRaw(value);
			w.WriteEndElement();
		}
	}

	/// <summary>Writes an XML element with a specified value in its body.</summary>
	/// <param name="localName">The local name of the XML element.</param>
	/// <param name="ns">The namespace of the XML element.</param>
	/// <param name="value">The text value of the XML element.</param>
	/// <param name="xsiType">The name of the XML Schema data type to be written to the xsi:type attribute.</param>
	protected void WriteElementStringRaw(string localName, string ns, byte[] value, XmlQualifiedName xsiType)
	{
		if (value != null)
		{
			w.WriteStartElement(localName, ns);
			if (xsiType != null)
			{
				WriteXsiType(xsiType.Name, xsiType.Namespace);
			}
			XmlCustomFormatter.WriteArrayBase64(w, value, 0, value.Length);
			w.WriteEndElement();
		}
	}

	/// <summary>Writes a SOAP 1.2 RPC result element with a specified qualified name in its body.</summary>
	/// <param name="name">The local name of the result body.</param>
	/// <param name="ns">The namespace of the result body.</param>
	protected void WriteRpcResult(string name, string ns)
	{
		if (soap12)
		{
			WriteElementQualifiedName("result", "http://www.w3.org/2003/05/soap-rpc", new XmlQualifiedName(name, ns), null);
		}
	}

	/// <summary>Writes an XML element with a specified qualified name in its body.</summary>
	/// <param name="localName">The local name of the XML element.</param>
	/// <param name="value">The name to write, using its prefix if namespace-qualified, in the element text.</param>
	protected void WriteElementQualifiedName(string localName, XmlQualifiedName value)
	{
		WriteElementQualifiedName(localName, null, value, null);
	}

	/// <summary>Writes an XML element with a specified qualified name in its body.</summary>
	/// <param name="localName">The local name of the XML element.</param>
	/// <param name="value">The name to write, using its prefix if namespace-qualified, in the element text.</param>
	/// <param name="xsiType">The name of the XML Schema data type to be written to the xsi:type attribute.</param>
	protected void WriteElementQualifiedName(string localName, XmlQualifiedName value, XmlQualifiedName xsiType)
	{
		WriteElementQualifiedName(localName, null, value, xsiType);
	}

	/// <summary>Writes an XML element with a specified qualified name in its body.</summary>
	/// <param name="localName">The local name of the XML element.</param>
	/// <param name="ns">The namespace of the XML element.</param>
	/// <param name="value">The name to write, using its prefix if namespace-qualified, in the element text.</param>
	protected void WriteElementQualifiedName(string localName, string ns, XmlQualifiedName value)
	{
		WriteElementQualifiedName(localName, ns, value, null);
	}

	/// <summary>Writes an XML element with a specified qualified name in its body.</summary>
	/// <param name="localName">The local name of the XML element.</param>
	/// <param name="ns">The namespace of the XML element.</param>
	/// <param name="value">The name to write, using its prefix if namespace-qualified, in the element text.</param>
	/// <param name="xsiType">The name of the XML Schema data type to be written to the xsi:type attribute.</param>
	protected void WriteElementQualifiedName(string localName, string ns, XmlQualifiedName value, XmlQualifiedName xsiType)
	{
		if (!(value == null))
		{
			if (value.Namespace == null || value.Namespace.Length == 0)
			{
				WriteStartElement(localName, ns, null, writePrefixed: true);
				WriteAttribute("xmlns", "");
			}
			else
			{
				w.WriteStartElement(localName, ns);
			}
			if (xsiType != null)
			{
				WriteXsiType(xsiType.Name, xsiType.Namespace);
			}
			w.WriteString(FromXmlQualifiedName(value, ignoreEmpty: false));
			w.WriteEndElement();
		}
	}

	/// <summary>Stores an implementation of the <see cref="T:System.Xml.Serialization.XmlSerializationWriteCallback" /> delegate and the type it applies to, for a later invocation.</summary>
	/// <param name="type">The <see cref="T:System.Type" /> of objects that are serialized.</param>
	/// <param name="typeName">The name of the type of objects that are serialized.</param>
	/// <param name="typeNs">The namespace of the type of objects that are serialized.</param>
	/// <param name="callback">An instance of the <see cref="T:System.Xml.Serialization.XmlSerializationWriteCallback" /> delegate.</param>
	protected void AddWriteCallback(Type type, string typeName, string typeNs, XmlSerializationWriteCallback callback)
	{
		TypeEntry typeEntry = new TypeEntry();
		typeEntry.typeName = typeName;
		typeEntry.typeNs = typeNs;
		typeEntry.type = type;
		typeEntry.callback = callback;
		typeEntries[type] = typeEntry;
	}

	private void WriteArray(string name, string ns, object o, Type type)
	{
		Type arrayElementType = TypeScope.GetArrayElementType(type, null);
		StringBuilder stringBuilder = new StringBuilder();
		if (!soap12)
		{
			while ((arrayElementType.IsArray || typeof(IEnumerable).IsAssignableFrom(arrayElementType)) && GetPrimitiveTypeName(arrayElementType, throwIfUnknown: false) == null)
			{
				arrayElementType = TypeScope.GetArrayElementType(arrayElementType, null);
				stringBuilder.Append("[]");
			}
		}
		string text;
		string ns2;
		if (arrayElementType == typeof(object))
		{
			text = "anyType";
			ns2 = "http://www.w3.org/2001/XMLSchema";
		}
		else
		{
			TypeEntry typeEntry = GetTypeEntry(arrayElementType);
			if (typeEntry != null)
			{
				text = typeEntry.typeName;
				ns2 = typeEntry.typeNs;
			}
			else if (soap12)
			{
				XmlQualifiedName primitiveTypeName = GetPrimitiveTypeName(arrayElementType, throwIfUnknown: false);
				if (primitiveTypeName != null)
				{
					text = primitiveTypeName.Name;
					ns2 = primitiveTypeName.Namespace;
				}
				else
				{
					Type baseType = arrayElementType.BaseType;
					while (baseType != null)
					{
						typeEntry = GetTypeEntry(baseType);
						if (typeEntry != null)
						{
							break;
						}
						baseType = baseType.BaseType;
					}
					if (typeEntry != null)
					{
						text = typeEntry.typeName;
						ns2 = typeEntry.typeNs;
					}
					else
					{
						text = "anyType";
						ns2 = "http://www.w3.org/2001/XMLSchema";
					}
				}
			}
			else
			{
				XmlQualifiedName primitiveTypeName2 = GetPrimitiveTypeName(arrayElementType);
				text = primitiveTypeName2.Name;
				ns2 = primitiveTypeName2.Namespace;
			}
		}
		if (stringBuilder.Length > 0)
		{
			text += stringBuilder.ToString();
		}
		if (soap12 && name != null && name.Length > 0)
		{
			WriteStartElement(name, ns, null, writePrefixed: false);
		}
		else
		{
			WriteStartElement("Array", "http://schemas.xmlsoap.org/soap/encoding/", null, writePrefixed: true);
		}
		WriteId(o, addToReferencesList: false);
		if (type.IsArray)
		{
			Array array = (Array)o;
			int length = array.Length;
			if (soap12)
			{
				w.WriteAttributeString("itemType", "http://www.w3.org/2003/05/soap-encoding", GetQualifiedName(text, ns2));
				w.WriteAttributeString("arraySize", "http://www.w3.org/2003/05/soap-encoding", length.ToString(CultureInfo.InvariantCulture));
			}
			else
			{
				w.WriteAttributeString("arrayType", "http://schemas.xmlsoap.org/soap/encoding/", GetQualifiedName(text, ns2) + "[" + length.ToString(CultureInfo.InvariantCulture) + "]");
			}
			for (int i = 0; i < length; i++)
			{
				WritePotentiallyReferencingElement("Item", "", array.GetValue(i), arrayElementType, suppressReference: false, isNullable: true);
			}
		}
		else
		{
			int num = (typeof(ICollection).IsAssignableFrom(type) ? ((ICollection)o).Count : (-1));
			if (soap12)
			{
				w.WriteAttributeString("itemType", "http://www.w3.org/2003/05/soap-encoding", GetQualifiedName(text, ns2));
				if (num >= 0)
				{
					w.WriteAttributeString("arraySize", "http://www.w3.org/2003/05/soap-encoding", num.ToString(CultureInfo.InvariantCulture));
				}
			}
			else
			{
				string text2 = ((num >= 0) ? ("[" + num + "]") : "[]");
				w.WriteAttributeString("arrayType", "http://schemas.xmlsoap.org/soap/encoding/", GetQualifiedName(text, ns2) + text2);
			}
			IEnumerator enumerator = ((IEnumerable)o).GetEnumerator();
			if (enumerator != null)
			{
				while (enumerator.MoveNext())
				{
					WritePotentiallyReferencingElement("Item", "", enumerator.Current, arrayElementType, suppressReference: false, isNullable: true);
				}
			}
		}
		w.WriteEndElement();
	}

	/// <summary>Writes a SOAP message XML element that can contain a reference to a &lt;multiRef&gt; XML element for a given object.</summary>
	/// <param name="n">The local name of the XML element to write.</param>
	/// <param name="ns">The namespace of the XML element to write.</param>
	/// <param name="o">The object being serialized either in the current XML element or a multiRef element that is referenced by the current element.</param>
	protected void WritePotentiallyReferencingElement(string n, string ns, object o)
	{
		WritePotentiallyReferencingElement(n, ns, o, null, suppressReference: false, isNullable: false);
	}

	/// <summary>Writes a SOAP message XML element that can contain a reference to a &lt;multiRef&gt; XML element for a given object.</summary>
	/// <param name="n">The local name of the XML element to write.</param>
	/// <param name="ns">The namespace of the XML element to write.</param>
	/// <param name="o">The object being serialized either in the current XML element or a multiRef element that referenced by the current element.</param>
	/// <param name="ambientType">The type stored in the object's type mapping (as opposed to the object's type found directly through the typeof operation).</param>
	protected void WritePotentiallyReferencingElement(string n, string ns, object o, Type ambientType)
	{
		WritePotentiallyReferencingElement(n, ns, o, ambientType, suppressReference: false, isNullable: false);
	}

	/// <summary>Writes a SOAP message XML element that can contain a reference to a &lt;multiRef&gt; XML element for a given object.</summary>
	/// <param name="n">The local name of the XML element to write.</param>
	/// <param name="ns">The namespace of the XML element to write.</param>
	/// <param name="o">The object being serialized either in the current XML element or a multiRef element that is referenced by the current element.</param>
	/// <param name="ambientType">The type stored in the object's type mapping (as opposed to the object's type found directly through the typeof operation).</param>
	/// <param name="suppressReference">true to serialize the object directly into the XML element rather than make the element reference another element that contains the data; otherwise, false.</param>
	protected void WritePotentiallyReferencingElement(string n, string ns, object o, Type ambientType, bool suppressReference)
	{
		WritePotentiallyReferencingElement(n, ns, o, ambientType, suppressReference, isNullable: false);
	}

	/// <summary>Writes a SOAP message XML element that can contain a reference to a multiRef XML element for a given object.</summary>
	/// <param name="n">The local name of the XML element to write.</param>
	/// <param name="ns">The namespace of the XML element to write.</param>
	/// <param name="o">The object being serialized either in the current XML element or a multiRef element that referenced by the current element.</param>
	/// <param name="ambientType">The type stored in the object's type mapping (as opposed to the object's type found directly through the typeof operation).</param>
	/// <param name="suppressReference">true to serialize the object directly into the XML element rather than make the element reference another element that contains the data; otherwise, false.</param>
	/// <param name="isNullable">true to write an xsi:nil='true' attribute if the object to serialize is null; otherwise, false.</param>
	protected void WritePotentiallyReferencingElement(string n, string ns, object o, Type ambientType, bool suppressReference, bool isNullable)
	{
		if (o == null)
		{
			if (isNullable)
			{
				WriteNullTagEncoded(n, ns);
			}
			return;
		}
		Type type = o.GetType();
		if (Convert.GetTypeCode(o) == TypeCode.Object && !(o is Guid) && type != typeof(XmlQualifiedName) && !(o is XmlNode[]) && type != typeof(byte[]))
		{
			if ((suppressReference || soap12) && !IsIdDefined(o))
			{
				WriteReferencedElement(n, ns, o, ambientType);
			}
			else if (n == null)
			{
				TypeEntry typeEntry = GetTypeEntry(type);
				WriteReferencingElement(typeEntry.typeName, typeEntry.typeNs, o, isNullable);
			}
			else
			{
				WriteReferencingElement(n, ns, o, isNullable);
			}
			return;
		}
		bool flag = type != ambientType && !type.IsEnum;
		TypeEntry typeEntry2 = GetTypeEntry(type);
		if (typeEntry2 != null)
		{
			if (n == null)
			{
				WriteStartElement(typeEntry2.typeName, typeEntry2.typeNs, null, writePrefixed: true);
			}
			else
			{
				WriteStartElement(n, ns, null, writePrefixed: true);
			}
			if (flag)
			{
				WriteXsiType(typeEntry2.typeName, typeEntry2.typeNs);
			}
			typeEntry2.callback(o);
			w.WriteEndElement();
		}
		else
		{
			WriteTypedPrimitive(n, ns, o, flag);
		}
	}

	private void WriteReferencedElement(object o, Type ambientType)
	{
		WriteReferencedElement(null, null, o, ambientType);
	}

	private void WriteReferencedElement(string name, string ns, object o, Type ambientType)
	{
		if (name == null)
		{
			name = string.Empty;
		}
		Type type = o.GetType();
		if (type.IsArray || typeof(IEnumerable).IsAssignableFrom(type))
		{
			WriteArray(name, ns, o, type);
			return;
		}
		TypeEntry typeEntry = GetTypeEntry(type);
		if (typeEntry == null)
		{
			throw CreateUnknownTypeException(type);
		}
		WriteStartElement((name.Length == 0) ? typeEntry.typeName : name, (ns == null) ? typeEntry.typeNs : ns, null, writePrefixed: true);
		WriteId(o, addToReferencesList: false);
		if (ambientType != type)
		{
			WriteXsiType(typeEntry.typeName, typeEntry.typeNs);
		}
		typeEntry.callback(o);
		w.WriteEndElement();
	}

	private TypeEntry GetTypeEntry(Type t)
	{
		if (typeEntries == null)
		{
			typeEntries = new Hashtable();
			InitCallbacks();
		}
		return (TypeEntry)typeEntries[t];
	}

	/// <summary>Initializes an instances of the <see cref="T:System.Xml.Serialization.XmlSerializationWriteCallback" /> delegate to serialize SOAP-encoded XML data.</summary>
	protected abstract void InitCallbacks();

	/// <summary>Serializes objects into SOAP-encoded multiRef XML elements in a SOAP message.</summary>
	protected void WriteReferencedElements()
	{
		if (referencesToWrite != null)
		{
			for (int i = 0; i < referencesToWrite.Count; i++)
			{
				WriteReferencedElement(referencesToWrite[i], null);
			}
		}
	}

	/// <summary>Initializes object references only while serializing a SOAP-encoded SOAP message.</summary>
	protected void TopLevelElement()
	{
		objectsInUse = new Hashtable();
	}

	/// <summary>Writes the namespace declaration attributes.</summary>
	/// <param name="xmlns">The XML namespaces to declare.</param>
	protected void WriteNamespaceDeclarations(XmlSerializerNamespaces xmlns)
	{
		if (xmlns != null)
		{
			foreach (DictionaryEntry @namespace in xmlns.Namespaces)
			{
				string text = (string)@namespace.Key;
				string text2 = (string)@namespace.Value;
				if (namespaces != null && namespaces.Namespaces[text] is string text3 && text3 != text2)
				{
					throw new InvalidOperationException(Res.GetString("Illegal namespace declaration xmlns:{0}='{1}'. Namespace alias '{0}' already defined in the current scope.", text, text2));
				}
				string text4 = ((text2 == null || text2.Length == 0) ? null : Writer.LookupPrefix(text2));
				if (text4 == null || text4 != text)
				{
					WriteAttribute("xmlns", text, null, text2);
				}
			}
		}
		namespaces = null;
	}

	private string NextPrefix()
	{
		if (usedPrefixes == null)
		{
			return aliasBase + ++tempNamespacePrefix;
		}
		while (usedPrefixes.ContainsKey(++tempNamespacePrefix))
		{
		}
		return aliasBase + tempNamespacePrefix;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Serialization.XmlSerializationWriter" /> class.</summary>
	protected XmlSerializationWriter()
	{
	}
}
