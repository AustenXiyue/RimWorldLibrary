using System.ComponentModel;
using System.Reflection;

namespace System.Xml.Serialization;

/// <summary>Represents a collection of attribute objects that control how the <see cref="T:System.Xml.Serialization.XmlSerializer" /> serializes and deserializes an object.</summary>
public class XmlAttributes
{
	private XmlElementAttributes xmlElements = new XmlElementAttributes();

	private XmlArrayItemAttributes xmlArrayItems = new XmlArrayItemAttributes();

	private XmlAnyElementAttributes xmlAnyElements = new XmlAnyElementAttributes();

	private XmlArrayAttribute xmlArray;

	private XmlAttributeAttribute xmlAttribute;

	private XmlTextAttribute xmlText;

	private XmlEnumAttribute xmlEnum;

	private bool xmlIgnore;

	private bool xmlns;

	private object xmlDefaultValue;

	private XmlRootAttribute xmlRoot;

	private XmlTypeAttribute xmlType;

	private XmlAnyAttributeAttribute xmlAnyAttribute;

	private XmlChoiceIdentifierAttribute xmlChoiceIdentifier;

	private static volatile Type ignoreAttributeType;

	internal XmlAttributeFlags XmlFlags
	{
		get
		{
			XmlAttributeFlags xmlAttributeFlags = (XmlAttributeFlags)0;
			if (xmlElements.Count > 0)
			{
				xmlAttributeFlags |= XmlAttributeFlags.Elements;
			}
			if (xmlArrayItems.Count > 0)
			{
				xmlAttributeFlags |= XmlAttributeFlags.ArrayItems;
			}
			if (xmlAnyElements.Count > 0)
			{
				xmlAttributeFlags |= XmlAttributeFlags.AnyElements;
			}
			if (xmlArray != null)
			{
				xmlAttributeFlags |= XmlAttributeFlags.Array;
			}
			if (xmlAttribute != null)
			{
				xmlAttributeFlags |= XmlAttributeFlags.Attribute;
			}
			if (xmlText != null)
			{
				xmlAttributeFlags |= XmlAttributeFlags.Text;
			}
			if (xmlEnum != null)
			{
				xmlAttributeFlags |= XmlAttributeFlags.Enum;
			}
			if (xmlRoot != null)
			{
				xmlAttributeFlags |= XmlAttributeFlags.Root;
			}
			if (xmlType != null)
			{
				xmlAttributeFlags |= XmlAttributeFlags.Type;
			}
			if (xmlAnyAttribute != null)
			{
				xmlAttributeFlags |= XmlAttributeFlags.AnyAttribute;
			}
			if (xmlChoiceIdentifier != null)
			{
				xmlAttributeFlags |= XmlAttributeFlags.ChoiceIdentifier;
			}
			if (xmlns)
			{
				xmlAttributeFlags |= XmlAttributeFlags.XmlnsDeclarations;
			}
			return xmlAttributeFlags;
		}
	}

	private static Type IgnoreAttribute
	{
		get
		{
			if (ignoreAttributeType == null)
			{
				ignoreAttributeType = typeof(object).Assembly.GetType("System.XmlIgnoreMemberAttribute");
				if (ignoreAttributeType == null)
				{
					ignoreAttributeType = typeof(XmlIgnoreAttribute);
				}
			}
			return ignoreAttributeType;
		}
	}

	/// <summary>Gets a collection of objects that specify how the <see cref="T:System.Xml.Serialization.XmlSerializer" /> serializes a public field or read/write property as an XML element.</summary>
	/// <returns>An <see cref="T:System.Xml.Serialization.XmlElementAttributes" /> that contains a collection of <see cref="T:System.Xml.Serialization.XmlElementAttribute" /> objects.</returns>
	public XmlElementAttributes XmlElements => xmlElements;

	/// <summary>Gets or sets an object that specifies how the <see cref="T:System.Xml.Serialization.XmlSerializer" /> serializes a public field or public read/write property as an XML attribute.</summary>
	/// <returns>An <see cref="T:System.Xml.Serialization.XmlAttributeAttribute" /> that controls the serialization of a public field or read/write property as an XML attribute.</returns>
	public XmlAttributeAttribute XmlAttribute
	{
		get
		{
			return xmlAttribute;
		}
		set
		{
			xmlAttribute = value;
		}
	}

	/// <summary>Gets or sets an object that specifies how the <see cref="T:System.Xml.Serialization.XmlSerializer" /> serializes an enumeration member.</summary>
	/// <returns>An <see cref="T:System.Xml.Serialization.XmlEnumAttribute" /> that specifies how the <see cref="T:System.Xml.Serialization.XmlSerializer" /> serializes an enumeration member.</returns>
	public XmlEnumAttribute XmlEnum
	{
		get
		{
			return xmlEnum;
		}
		set
		{
			xmlEnum = value;
		}
	}

	/// <summary>Gets or sets an object that instructs the <see cref="T:System.Xml.Serialization.XmlSerializer" /> to serialize a public field or public read/write property as XML text.</summary>
	/// <returns>An <see cref="T:System.Xml.Serialization.XmlTextAttribute" /> that overrides the default serialization of a public property or field.</returns>
	public XmlTextAttribute XmlText
	{
		get
		{
			return xmlText;
		}
		set
		{
			xmlText = value;
		}
	}

	/// <summary>Gets or sets an object that specifies how the <see cref="T:System.Xml.Serialization.XmlSerializer" /> serializes a public field or read/write property that returns an array.</summary>
	/// <returns>An <see cref="T:System.Xml.Serialization.XmlArrayAttribute" /> that specifies how the <see cref="T:System.Xml.Serialization.XmlSerializer" /> serializes a public field or read/write property that returns an array.</returns>
	public XmlArrayAttribute XmlArray
	{
		get
		{
			return xmlArray;
		}
		set
		{
			xmlArray = value;
		}
	}

	/// <summary>Gets or sets a collection of objects that specify how the <see cref="T:System.Xml.Serialization.XmlSerializer" /> serializes items inserted into an array returned by a public field or read/write property.</summary>
	/// <returns>An <see cref="T:System.Xml.Serialization.XmlArrayItemAttributes" /> object that contains a collection of <see cref="T:System.Xml.Serialization.XmlArrayItemAttribute" /> objects.</returns>
	public XmlArrayItemAttributes XmlArrayItems => xmlArrayItems;

	/// <summary>Gets or sets the default value of an XML element or attribute.</summary>
	/// <returns>An <see cref="T:System.Object" /> that represents the default value of an XML element or attribute.</returns>
	public object XmlDefaultValue
	{
		get
		{
			return xmlDefaultValue;
		}
		set
		{
			xmlDefaultValue = value;
		}
	}

	/// <summary>Gets or sets a value that specifies whether or not the <see cref="T:System.Xml.Serialization.XmlSerializer" /> serializes a public field or public read/write property.</summary>
	/// <returns>true if the <see cref="T:System.Xml.Serialization.XmlSerializer" /> must not serialize the field or property; otherwise, false.</returns>
	public bool XmlIgnore
	{
		get
		{
			return xmlIgnore;
		}
		set
		{
			xmlIgnore = value;
		}
	}

	/// <summary>Gets or sets an object that specifies how the <see cref="T:System.Xml.Serialization.XmlSerializer" /> serializes a class to which the <see cref="T:System.Xml.Serialization.XmlTypeAttribute" /> has been applied.</summary>
	/// <returns>An <see cref="T:System.Xml.Serialization.XmlTypeAttribute" /> that overrides an <see cref="T:System.Xml.Serialization.XmlTypeAttribute" /> applied to a class declaration.</returns>
	public XmlTypeAttribute XmlType
	{
		get
		{
			return xmlType;
		}
		set
		{
			xmlType = value;
		}
	}

	/// <summary>Gets or sets an object that specifies how the <see cref="T:System.Xml.Serialization.XmlSerializer" /> serializes a class as an XML root element.</summary>
	/// <returns>An <see cref="T:System.Xml.Serialization.XmlRootAttribute" /> that overrides a class attributed as an XML root element.</returns>
	public XmlRootAttribute XmlRoot
	{
		get
		{
			return xmlRoot;
		}
		set
		{
			xmlRoot = value;
		}
	}

	/// <summary>Gets the collection of <see cref="T:System.Xml.Serialization.XmlAnyElementAttribute" /> objects to override.</summary>
	/// <returns>An <see cref="T:System.Xml.Serialization.XmlAnyElementAttributes" /> object that represents the collection of <see cref="T:System.Xml.Serialization.XmlAnyElementAttribute" /> objects.</returns>
	public XmlAnyElementAttributes XmlAnyElements => xmlAnyElements;

	/// <summary>Gets or sets the <see cref="T:System.Xml.Serialization.XmlAnyAttributeAttribute" /> to override.</summary>
	/// <returns>The <see cref="T:System.Xml.Serialization.XmlAnyAttributeAttribute" /> to override.</returns>
	public XmlAnyAttributeAttribute XmlAnyAttribute
	{
		get
		{
			return xmlAnyAttribute;
		}
		set
		{
			xmlAnyAttribute = value;
		}
	}

	/// <summary>Gets or sets an object that allows you to distinguish between a set of choices.</summary>
	/// <returns>An <see cref="T:System.Xml.Serialization.XmlChoiceIdentifierAttribute" /> that can be applied to a class member that is serialized as an xsi:choice element.</returns>
	public XmlChoiceIdentifierAttribute XmlChoiceIdentifier => xmlChoiceIdentifier;

	/// <summary>Gets or sets a value that specifies whether to keep all namespace declarations when an object containing a member that returns an <see cref="T:System.Xml.Serialization.XmlSerializerNamespaces" /> object is overridden.</summary>
	/// <returns>true if the namespace declarations should be kept; otherwise, false.</returns>
	public bool Xmlns
	{
		get
		{
			return xmlns;
		}
		set
		{
			xmlns = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Serialization.XmlAttributes" /> class.</summary>
	public XmlAttributes()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Serialization.XmlAttributes" /> class and customizes how the <see cref="T:System.Xml.Serialization.XmlSerializer" /> serializes and deserializes an object. </summary>
	/// <param name="provider">A class that can provide alternative implementations of attributes that control XML serialization.</param>
	public XmlAttributes(ICustomAttributeProvider provider)
	{
		object[] customAttributes = provider.GetCustomAttributes(inherit: false);
		XmlAnyElementAttribute xmlAnyElementAttribute = null;
		for (int i = 0; i < customAttributes.Length; i++)
		{
			if (customAttributes[i] is XmlIgnoreAttribute || customAttributes[i] is ObsoleteAttribute || customAttributes[i].GetType() == IgnoreAttribute)
			{
				xmlIgnore = true;
				break;
			}
			if (customAttributes[i] is XmlElementAttribute)
			{
				xmlElements.Add((XmlElementAttribute)customAttributes[i]);
			}
			else if (customAttributes[i] is XmlArrayItemAttribute)
			{
				xmlArrayItems.Add((XmlArrayItemAttribute)customAttributes[i]);
			}
			else if (customAttributes[i] is XmlAnyElementAttribute)
			{
				XmlAnyElementAttribute xmlAnyElementAttribute2 = (XmlAnyElementAttribute)customAttributes[i];
				if ((xmlAnyElementAttribute2.Name == null || xmlAnyElementAttribute2.Name.Length == 0) && xmlAnyElementAttribute2.NamespaceSpecified && xmlAnyElementAttribute2.Namespace == null)
				{
					xmlAnyElementAttribute = xmlAnyElementAttribute2;
				}
				else
				{
					xmlAnyElements.Add((XmlAnyElementAttribute)customAttributes[i]);
				}
			}
			else if (customAttributes[i] is DefaultValueAttribute)
			{
				xmlDefaultValue = ((DefaultValueAttribute)customAttributes[i]).Value;
			}
			else if (customAttributes[i] is XmlAttributeAttribute)
			{
				xmlAttribute = (XmlAttributeAttribute)customAttributes[i];
			}
			else if (customAttributes[i] is XmlArrayAttribute)
			{
				xmlArray = (XmlArrayAttribute)customAttributes[i];
			}
			else if (customAttributes[i] is XmlTextAttribute)
			{
				xmlText = (XmlTextAttribute)customAttributes[i];
			}
			else if (customAttributes[i] is XmlEnumAttribute)
			{
				xmlEnum = (XmlEnumAttribute)customAttributes[i];
			}
			else if (customAttributes[i] is XmlRootAttribute)
			{
				xmlRoot = (XmlRootAttribute)customAttributes[i];
			}
			else if (customAttributes[i] is XmlTypeAttribute)
			{
				xmlType = (XmlTypeAttribute)customAttributes[i];
			}
			else if (customAttributes[i] is XmlAnyAttributeAttribute)
			{
				xmlAnyAttribute = (XmlAnyAttributeAttribute)customAttributes[i];
			}
			else if (customAttributes[i] is XmlChoiceIdentifierAttribute)
			{
				xmlChoiceIdentifier = (XmlChoiceIdentifierAttribute)customAttributes[i];
			}
			else if (customAttributes[i] is XmlNamespaceDeclarationsAttribute)
			{
				xmlns = true;
			}
		}
		if (xmlIgnore)
		{
			xmlElements.Clear();
			xmlArrayItems.Clear();
			xmlAnyElements.Clear();
			xmlDefaultValue = null;
			xmlAttribute = null;
			xmlArray = null;
			xmlText = null;
			xmlEnum = null;
			xmlType = null;
			xmlAnyAttribute = null;
			xmlChoiceIdentifier = null;
			xmlns = false;
		}
		else if (xmlAnyElementAttribute != null)
		{
			xmlAnyElements.Add(xmlAnyElementAttribute);
		}
	}

	internal static object GetAttr(ICustomAttributeProvider provider, Type attrType)
	{
		object[] customAttributes = provider.GetCustomAttributes(attrType, inherit: false);
		if (customAttributes.Length == 0)
		{
			return null;
		}
		return customAttributes[0];
	}
}
