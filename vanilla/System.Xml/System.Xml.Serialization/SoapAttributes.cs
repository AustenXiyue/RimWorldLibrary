using System.ComponentModel;
using System.Reflection;

namespace System.Xml.Serialization;

/// <summary>Represents a collection of attribute objects that control how the <see cref="T:System.Xml.Serialization.XmlSerializer" /> serializes and deserializes SOAP methods.</summary>
public class SoapAttributes
{
	private bool soapIgnore;

	private SoapTypeAttribute soapType;

	private SoapElementAttribute soapElement;

	private SoapAttributeAttribute soapAttribute;

	private SoapEnumAttribute soapEnum;

	private object soapDefaultValue;

	internal SoapAttributeFlags SoapFlags
	{
		get
		{
			SoapAttributeFlags soapAttributeFlags = (SoapAttributeFlags)0;
			if (soapElement != null)
			{
				soapAttributeFlags |= SoapAttributeFlags.Element;
			}
			if (soapAttribute != null)
			{
				soapAttributeFlags |= SoapAttributeFlags.Attribute;
			}
			if (soapEnum != null)
			{
				soapAttributeFlags |= SoapAttributeFlags.Enum;
			}
			if (soapType != null)
			{
				soapAttributeFlags |= SoapAttributeFlags.Type;
			}
			return soapAttributeFlags;
		}
	}

	/// <summary>Gets or sets an object that instructs the <see cref="T:System.Xml.Serialization.XmlSerializer" /> how to serialize an object type into encoded SOAP XML.</summary>
	/// <returns>A <see cref="T:System.Xml.Serialization.SoapTypeAttribute" /> that either overrides a <see cref="T:System.Xml.Serialization.SoapTypeAttribute" /> applied to a class declaration, or is applied to a class declaration.</returns>
	public SoapTypeAttribute SoapType
	{
		get
		{
			return soapType;
		}
		set
		{
			soapType = value;
		}
	}

	/// <summary>Gets or sets an object that specifies how the <see cref="T:System.Xml.Serialization.XmlSerializer" /> serializes a SOAP enumeration.</summary>
	/// <returns>An object that specifies how the <see cref="T:System.Xml.Serialization.XmlSerializer" /> serializes an enumeration member.</returns>
	public SoapEnumAttribute SoapEnum
	{
		get
		{
			return soapEnum;
		}
		set
		{
			soapEnum = value;
		}
	}

	/// <summary>Gets or sets a value that specifies whether the <see cref="T:System.Xml.Serialization.XmlSerializer" /> serializes a public field or property as encoded SOAP XML.</summary>
	/// <returns>true if the <see cref="T:System.Xml.Serialization.XmlSerializer" /> must not serialize the field or property; otherwise, false.</returns>
	public bool SoapIgnore
	{
		get
		{
			return soapIgnore;
		}
		set
		{
			soapIgnore = value;
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Xml.Serialization.SoapElementAttribute" /> to override.</summary>
	/// <returns>The <see cref="T:System.Xml.Serialization.SoapElementAttribute" /> to override.</returns>
	public SoapElementAttribute SoapElement
	{
		get
		{
			return soapElement;
		}
		set
		{
			soapElement = value;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Xml.Serialization.SoapAttributeAttribute" /> to override.</summary>
	/// <returns>A <see cref="T:System.Xml.Serialization.SoapAttributeAttribute" /> that overrides the behavior of the <see cref="T:System.Xml.Serialization.XmlSerializer" /> when the member is serialized.</returns>
	public SoapAttributeAttribute SoapAttribute
	{
		get
		{
			return soapAttribute;
		}
		set
		{
			soapAttribute = value;
		}
	}

	/// <summary>Gets or sets the default value of an XML element or attribute.</summary>
	/// <returns>An object that represents the default value of an XML element or attribute.</returns>
	public object SoapDefaultValue
	{
		get
		{
			return soapDefaultValue;
		}
		set
		{
			soapDefaultValue = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Serialization.SoapAttributes" /> class.</summary>
	public SoapAttributes()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Serialization.SoapAttributes" /> class using the specified custom type.</summary>
	/// <param name="provider">Any object that implements the <see cref="T:System.Reflection.ICustomAttributeProvider" /> interface, such as the <see cref="T:System.Type" /> class.</param>
	public SoapAttributes(ICustomAttributeProvider provider)
	{
		object[] customAttributes = provider.GetCustomAttributes(inherit: false);
		for (int i = 0; i < customAttributes.Length; i++)
		{
			if (customAttributes[i] is SoapIgnoreAttribute || customAttributes[i] is ObsoleteAttribute)
			{
				soapIgnore = true;
				break;
			}
			if (customAttributes[i] is SoapElementAttribute)
			{
				soapElement = (SoapElementAttribute)customAttributes[i];
			}
			else if (customAttributes[i] is SoapAttributeAttribute)
			{
				soapAttribute = (SoapAttributeAttribute)customAttributes[i];
			}
			else if (customAttributes[i] is SoapTypeAttribute)
			{
				soapType = (SoapTypeAttribute)customAttributes[i];
			}
			else if (customAttributes[i] is SoapEnumAttribute)
			{
				soapEnum = (SoapEnumAttribute)customAttributes[i];
			}
			else if (customAttributes[i] is DefaultValueAttribute)
			{
				soapDefaultValue = ((DefaultValueAttribute)customAttributes[i]).Value;
			}
		}
		if (soapIgnore)
		{
			soapElement = null;
			soapAttribute = null;
			soapType = null;
			soapEnum = null;
			soapDefaultValue = null;
		}
	}
}
