using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using MS.Internal.Xml.Linq.ComponentModel;

namespace System.Xml.Linq;

/// <summary>Represents an XML attribute.</summary>
[TypeDescriptionProvider(typeof(XTypeDescriptionProvider<XAttribute>))]
public class XAttribute : XObject
{
	private static IEnumerable<XAttribute> emptySequence;

	internal XAttribute next;

	internal XName name;

	internal string value;

	/// <summary>Gets an empty collection of attributes.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XAttribute" /> containing an empty collection.</returns>
	public static IEnumerable<XAttribute> EmptySequence
	{
		get
		{
			if (emptySequence == null)
			{
				emptySequence = new XAttribute[0];
			}
			return emptySequence;
		}
	}

	/// <summary>Determines if this attribute is a namespace declaration.</summary>
	/// <returns>true if this attribute is a namespace declaration; otherwise false.</returns>
	public bool IsNamespaceDeclaration
	{
		get
		{
			string namespaceName = name.NamespaceName;
			if (namespaceName.Length == 0)
			{
				return name.LocalName == "xmlns";
			}
			return (object)namespaceName == "http://www.w3.org/2000/xmlns/";
		}
	}

	/// <summary>Gets the expanded name of this attribute.</summary>
	/// <returns>An <see cref="T:System.Xml.Linq.XName" /> containing the name of this attribute.</returns>
	public XName Name => name;

	/// <summary>Gets the next attribute of the parent element.</summary>
	/// <returns>An <see cref="T:System.Xml.Linq.XAttribute" /> containing the next attribute of the parent element.</returns>
	/// <filterpriority>2</filterpriority>
	public XAttribute NextAttribute
	{
		get
		{
			if (parent == null || ((XElement)parent).lastAttr == this)
			{
				return null;
			}
			return next;
		}
	}

	/// <summary>Gets the node type for this node.</summary>
	/// <returns>The node type. For <see cref="T:System.Xml.Linq.XAttribute" /> objects, this value is <see cref="F:System.Xml.XmlNodeType.Attribute" />.</returns>
	public override XmlNodeType NodeType => XmlNodeType.Attribute;

	/// <summary>Gets the previous attribute of the parent element.</summary>
	/// <returns>An <see cref="T:System.Xml.Linq.XAttribute" /> containing the previous attribute of the parent element.</returns>
	/// <filterpriority>2</filterpriority>
	public XAttribute PreviousAttribute
	{
		get
		{
			if (parent == null)
			{
				return null;
			}
			XAttribute lastAttr = ((XElement)parent).lastAttr;
			while (lastAttr.next != this)
			{
				lastAttr = lastAttr.next;
			}
			if (lastAttr == ((XElement)parent).lastAttr)
			{
				return null;
			}
			return lastAttr;
		}
	}

	/// <summary>Gets or sets the value of this attribute.</summary>
	/// <returns>A <see cref="T:System.String" /> containing the value of this attribute.</returns>
	/// <exception cref="T:System.ArgumentNullException">When setting, the <paramref name="value" /> is null.</exception>
	public string Value
	{
		get
		{
			return value;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			ValidateAttribute(name, value);
			bool num = NotifyChanging(this, XObjectChangeEventArgs.Value);
			this.value = value;
			if (num)
			{
				NotifyChanged(this, XObjectChangeEventArgs.Value);
			}
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Linq.XAttribute" /> class from the specified name and value. </summary>
	/// <param name="name">The <see cref="T:System.Xml.Linq.XName" /> of the attribute.</param>
	/// <param name="value">An <see cref="T:System.Object" /> containing the value of the attribute.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="name" /> or <paramref name="value" /> parameter is null.</exception>
	public XAttribute(XName name, object value)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		string stringValue = XContainer.GetStringValue(value);
		ValidateAttribute(name, stringValue);
		this.name = name;
		this.value = stringValue;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Linq.XAttribute" /> class from another <see cref="T:System.Xml.Linq.XAttribute" /> object. </summary>
	/// <param name="other">An <see cref="T:System.Xml.Linq.XAttribute" /> object to copy from.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="other" /> parameter is null.</exception>
	public XAttribute(XAttribute other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		name = other.name;
		value = other.value;
	}

	/// <summary>Removes this attribute from its parent element.</summary>
	/// <exception cref="T:System.InvalidOperationException">The parent element is null.</exception>
	public void Remove()
	{
		if (parent == null)
		{
			throw new InvalidOperationException(Res.GetString("InvalidOperation_MissingParent"));
		}
		((XElement)parent).RemoveAttribute(this);
	}

	/// <summary>Sets the value of this attribute.</summary>
	/// <param name="value">The value to assign to this attribute.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="value" /> parameter is null.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="value" /> is an <see cref="T:System.Xml.Linq.XObject" />.</exception>
	public void SetValue(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		Value = XContainer.GetStringValue(value);
	}

	/// <summary>Converts the current <see cref="T:System.Xml.Linq.XAttribute" /> object to a string representation.</summary>
	/// <returns>A <see cref="T:System.String" /> containing the XML text representation of an attribute and its value.</returns>
	public override string ToString()
	{
		using StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
		XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
		xmlWriterSettings.ConformanceLevel = ConformanceLevel.Fragment;
		using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter, xmlWriterSettings))
		{
			xmlWriter.WriteAttributeString(GetPrefixOfNamespace(name.Namespace), name.LocalName, name.NamespaceName, value);
		}
		return stringWriter.ToString().Trim();
	}

	/// <summary>Cast the value of this <see cref="T:System.Xml.Linq.XAttribute" /> to a <see cref="T:System.String" />.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the content of this <see cref="T:System.Xml.Linq.XAttribute" />.</returns>
	/// <param name="attribute">The <see cref="T:System.Xml.Linq.XAttribute" /> to cast to <see cref="T:System.String" />.</param>
	[CLSCompliant(false)]
	public static explicit operator string(XAttribute attribute)
	{
		return attribute?.value;
	}

	/// <summary>Cast the value of this <see cref="T:System.Xml.Linq.XAttribute" /> to a <see cref="T:System.Boolean" />.</summary>
	/// <returns>A <see cref="T:System.Boolean" /> that contains the content of this <see cref="T:System.Xml.Linq.XAttribute" />.</returns>
	/// <param name="attribute">The <see cref="T:System.Xml.Linq.XAttribute" /> to cast to <see cref="T:System.Boolean" />.</param>
	/// <exception cref="T:System.FormatException">The attribute does not contain a valid <see cref="T:System.Boolean" /> value.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="attribute" /> parameter is null.</exception>
	[CLSCompliant(false)]
	public static explicit operator bool(XAttribute attribute)
	{
		if (attribute == null)
		{
			throw new ArgumentNullException("attribute");
		}
		return XmlConvert.ToBoolean(attribute.value.ToLower(CultureInfo.InvariantCulture));
	}

	/// <summary>Cast the value of this <see cref="T:System.Xml.Linq.XAttribute" /> to a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Boolean" />.</summary>
	/// <returns>A <see cref="T:System.Nullable`1" /> of <see cref="T:System.Boolean" /> that contains the content of this <see cref="T:System.Xml.Linq.XAttribute" />.</returns>
	/// <param name="attribute">The <see cref="T:System.Xml.Linq.XAttribute" /> to cast to <see cref="T:System.Nullable`1" /> of <see cref="T:System.Boolean" />.</param>
	/// <exception cref="T:System.FormatException">The attribute does not contain a valid <see cref="T:System.Boolean" /> value.</exception>
	[CLSCompliant(false)]
	public static explicit operator bool?(XAttribute attribute)
	{
		if (attribute == null)
		{
			return null;
		}
		return XmlConvert.ToBoolean(attribute.value.ToLower(CultureInfo.InvariantCulture));
	}

	/// <summary>Cast the value of this <see cref="T:System.Xml.Linq.XAttribute" /> to an <see cref="T:System.Int32" />.</summary>
	/// <returns>A <see cref="T:System.Int32" /> that contains the content of this <see cref="T:System.Xml.Linq.XAttribute" />.</returns>
	/// <param name="attribute">The <see cref="T:System.Xml.Linq.XAttribute" /> to cast to <see cref="T:System.Int32" />.</param>
	/// <exception cref="T:System.FormatException">The attribute does not contain a valid <see cref="T:System.Int32" /> value.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="attribute" /> parameter is null.</exception>
	[CLSCompliant(false)]
	public static explicit operator int(XAttribute attribute)
	{
		if (attribute == null)
		{
			throw new ArgumentNullException("attribute");
		}
		return XmlConvert.ToInt32(attribute.value);
	}

	/// <summary>Cast the value of this <see cref="T:System.Xml.Linq.XAttribute" /> to a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Int32" />.</summary>
	/// <returns>A <see cref="T:System.Nullable`1" /> of <see cref="T:System.Int32" /> that contains the content of this <see cref="T:System.Xml.Linq.XAttribute" />.</returns>
	/// <param name="attribute">The <see cref="T:System.Xml.Linq.XAttribute" /> to cast to a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Int32" />.</param>
	[CLSCompliant(false)]
	public static explicit operator int?(XAttribute attribute)
	{
		if (attribute == null)
		{
			return null;
		}
		return XmlConvert.ToInt32(attribute.value);
	}

	/// <summary>Cast the value of this <see cref="T:System.Xml.Linq.XAttribute" /> to a <see cref="T:System.UInt32" />.</summary>
	/// <returns>A <see cref="T:System.UInt32" /> that contains the content of this <see cref="T:System.Xml.Linq.XAttribute" />.</returns>
	/// <param name="attribute">The <see cref="T:System.Xml.Linq.XAttribute" /> to cast to <see cref="T:System.UInt32" />.</param>
	/// <exception cref="T:System.FormatException">The attribute does not contain a valid <see cref="T:System.UInt32" /> value.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="attribute" /> parameter is null.</exception>
	[CLSCompliant(false)]
	public static explicit operator uint(XAttribute attribute)
	{
		if (attribute == null)
		{
			throw new ArgumentNullException("attribute");
		}
		return XmlConvert.ToUInt32(attribute.value);
	}

	/// <summary>Cast the value of this <see cref="T:System.Xml.Linq.XAttribute" /> to a <see cref="T:System.Nullable`1" /> of <see cref="T:System.UInt32" />.</summary>
	/// <returns>A <see cref="T:System.Nullable`1" /> of <see cref="T:System.UInt32" /> that contains the content of this <see cref="T:System.Xml.Linq.XAttribute" />.</returns>
	/// <param name="attribute">The <see cref="T:System.Xml.Linq.XAttribute" /> to cast to a <see cref="T:System.Nullable`1" /> of <see cref="T:System.UInt32" />.</param>
	/// <exception cref="T:System.FormatException">The attribute does not contain a valid <see cref="T:System.UInt32" /> value.</exception>
	[CLSCompliant(false)]
	public static explicit operator uint?(XAttribute attribute)
	{
		if (attribute == null)
		{
			return null;
		}
		return XmlConvert.ToUInt32(attribute.value);
	}

	/// <summary>Cast the value of this <see cref="T:System.Xml.Linq.XAttribute" /> to an <see cref="T:System.Int64" />.</summary>
	/// <returns>A <see cref="T:System.Int64" /> that contains the content of this <see cref="T:System.Xml.Linq.XAttribute" />.</returns>
	/// <param name="attribute">The <see cref="T:System.Xml.Linq.XAttribute" /> to cast to <see cref="T:System.Int64" />.</param>
	/// <exception cref="T:System.FormatException">The attribute does not contain a valid <see cref="T:System.Int64" /> value.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="attribute" /> parameter is null.</exception>
	[CLSCompliant(false)]
	public static explicit operator long(XAttribute attribute)
	{
		if (attribute == null)
		{
			throw new ArgumentNullException("attribute");
		}
		return XmlConvert.ToInt64(attribute.value);
	}

	/// <summary>Cast the value of this <see cref="T:System.Xml.Linq.XAttribute" /> to a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Int64" />.</summary>
	/// <returns>A <see cref="T:System.Nullable`1" /> of <see cref="T:System.Int64" /> that contains the content of this <see cref="T:System.Xml.Linq.XAttribute" />.</returns>
	/// <param name="attribute">The <see cref="T:System.Xml.Linq.XAttribute" /> to cast to a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Int64" />.</param>
	/// <exception cref="T:System.FormatException">The attribute does not contain a valid <see cref="T:System.Int64" /> value.</exception>
	[CLSCompliant(false)]
	public static explicit operator long?(XAttribute attribute)
	{
		if (attribute == null)
		{
			return null;
		}
		return XmlConvert.ToInt64(attribute.value);
	}

	/// <summary>Cast the value of this <see cref="T:System.Xml.Linq.XAttribute" /> to a <see cref="T:System.UInt64" />.</summary>
	/// <returns>A <see cref="T:System.UInt64" /> that contains the content of this <see cref="T:System.Xml.Linq.XAttribute" />.</returns>
	/// <param name="attribute">The <see cref="T:System.Xml.Linq.XAttribute" /> to cast to <see cref="T:System.UInt64" />.</param>
	/// <exception cref="T:System.FormatException">The attribute does not contain a valid <see cref="T:System.UInt64" /> value.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="attribute" /> parameter is null.</exception>
	[CLSCompliant(false)]
	public static explicit operator ulong(XAttribute attribute)
	{
		if (attribute == null)
		{
			throw new ArgumentNullException("attribute");
		}
		return XmlConvert.ToUInt64(attribute.value);
	}

	/// <summary>Cast the value of this <see cref="T:System.Xml.Linq.XAttribute" /> to a <see cref="T:System.Nullable`1" /> of <see cref="T:System.UInt64" />.</summary>
	/// <returns>A <see cref="T:System.Nullable`1" /> of <see cref="T:System.UInt64" /> that contains the content of this <see cref="T:System.Xml.Linq.XAttribute" />.</returns>
	/// <param name="attribute">The <see cref="T:System.Xml.Linq.XAttribute" /> to cast to a <see cref="T:System.Nullable`1" /> of <see cref="T:System.UInt64" />.</param>
	/// <exception cref="T:System.FormatException">The attribute does not contain a valid <see cref="T:System.UInt64" /> value.</exception>
	[CLSCompliant(false)]
	public static explicit operator ulong?(XAttribute attribute)
	{
		if (attribute == null)
		{
			return null;
		}
		return XmlConvert.ToUInt64(attribute.value);
	}

	/// <summary>Cast the value of this <see cref="T:System.Xml.Linq.XAttribute" /> to a <see cref="T:System.Single" />.</summary>
	/// <returns>A <see cref="T:System.Single" /> that contains the content of this <see cref="T:System.Xml.Linq.XAttribute" />.</returns>
	/// <param name="attribute">The <see cref="T:System.Xml.Linq.XAttribute" /> to cast to <see cref="T:System.Single" />.</param>
	/// <exception cref="T:System.FormatException">The attribute does not contain a valid <see cref="T:System.Single" /> value.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="attribute" /> parameter is null.</exception>
	[CLSCompliant(false)]
	public static explicit operator float(XAttribute attribute)
	{
		if (attribute == null)
		{
			throw new ArgumentNullException("attribute");
		}
		return XmlConvert.ToSingle(attribute.value);
	}

	/// <summary>Cast the value of this <see cref="T:System.Xml.Linq.XAttribute" /> to a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Single" />.</summary>
	/// <returns>A <see cref="T:System.Nullable`1" /> of <see cref="T:System.Single" /> that contains the content of this <see cref="T:System.Xml.Linq.XAttribute" />.</returns>
	/// <param name="attribute">The <see cref="T:System.Xml.Linq.XAttribute" /> to cast to <see cref="T:System.Nullable`1" /> of <see cref="T:System.Single" />.</param>
	/// <exception cref="T:System.FormatException">The attribute does not contain a valid <see cref="T:System.Single" /> value.</exception>
	[CLSCompliant(false)]
	public static explicit operator float?(XAttribute attribute)
	{
		if (attribute == null)
		{
			return null;
		}
		return XmlConvert.ToSingle(attribute.value);
	}

	/// <summary>Cast the value of this <see cref="T:System.Xml.Linq.XAttribute" /> to a <see cref="T:System.Double" />.</summary>
	/// <returns>A <see cref="T:System.Double" /> that contains the content of this <see cref="T:System.Xml.Linq.XAttribute" />.</returns>
	/// <param name="attribute">The <see cref="T:System.Xml.Linq.XAttribute" /> to cast to <see cref="T:System.Double" />.</param>
	/// <exception cref="T:System.FormatException">The attribute does not contain a valid <see cref="T:System.Double" /> value.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="attribute" /> parameter is null.</exception>
	[CLSCompliant(false)]
	public static explicit operator double(XAttribute attribute)
	{
		if (attribute == null)
		{
			throw new ArgumentNullException("attribute");
		}
		return XmlConvert.ToDouble(attribute.value);
	}

	/// <summary>Cast the value of this <see cref="T:System.Xml.Linq.XAttribute" /> to a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Double" />.</summary>
	/// <returns>A <see cref="T:System.Nullable`1" /> of <see cref="T:System.Double" /> that contains the content of this <see cref="T:System.Xml.Linq.XAttribute" />.</returns>
	/// <param name="attribute">The <see cref="T:System.Xml.Linq.XAttribute" /> to cast to <see cref="T:System.Nullable`1" /> of <see cref="T:System.Double" />.</param>
	/// <exception cref="T:System.FormatException">The attribute does not contain a valid <see cref="T:System.Double" /> value.</exception>
	[CLSCompliant(false)]
	public static explicit operator double?(XAttribute attribute)
	{
		if (attribute == null)
		{
			return null;
		}
		return XmlConvert.ToDouble(attribute.value);
	}

	/// <summary>Cast the value of this <see cref="T:System.Xml.Linq.XAttribute" /> to a <see cref="T:System.Decimal" />.</summary>
	/// <returns>A <see cref="T:System.Decimal" /> that contains the content of this <see cref="T:System.Xml.Linq.XAttribute" />.</returns>
	/// <param name="attribute">The <see cref="T:System.Xml.Linq.XAttribute" /> to cast to <see cref="T:System.Decimal" />.</param>
	/// <exception cref="T:System.FormatException">The attribute does not contain a valid <see cref="T:System.Decimal" /> value.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="attribute" /> parameter is null.</exception>
	[CLSCompliant(false)]
	public static explicit operator decimal(XAttribute attribute)
	{
		if (attribute == null)
		{
			throw new ArgumentNullException("attribute");
		}
		return XmlConvert.ToDecimal(attribute.value);
	}

	/// <summary>Cast the value of this <see cref="T:System.Xml.Linq.XAttribute" /> to a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Decimal" />.</summary>
	/// <returns>A <see cref="T:System.Nullable`1" /> of <see cref="T:System.Decimal" /> that contains the content of this <see cref="T:System.Xml.Linq.XAttribute" />.</returns>
	/// <param name="attribute">The <see cref="T:System.Xml.Linq.XAttribute" /> to cast to <see cref="T:System.Nullable`1" /> of <see cref="T:System.Decimal" />.</param>
	/// <exception cref="T:System.FormatException">The attribute does not contain a valid <see cref="T:System.Decimal" /> value.</exception>
	[CLSCompliant(false)]
	public static explicit operator decimal?(XAttribute attribute)
	{
		if (attribute == null)
		{
			return null;
		}
		return XmlConvert.ToDecimal(attribute.value);
	}

	/// <summary>Cast the value of this <see cref="T:System.Xml.Linq.XAttribute" /> to a <see cref="T:System.DateTime" />.</summary>
	/// <returns>A <see cref="T:System.DateTime" /> that contains the content of this <see cref="T:System.Xml.Linq.XAttribute" />.</returns>
	/// <param name="attribute">The <see cref="T:System.Xml.Linq.XAttribute" /> to cast to <see cref="T:System.DateTime" />.</param>
	/// <exception cref="T:System.FormatException">The attribute does not contain a valid <see cref="T:System.DateTime" /> value.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="attribute" /> parameter is null.</exception>
	[CLSCompliant(false)]
	public static explicit operator DateTime(XAttribute attribute)
	{
		if (attribute == null)
		{
			throw new ArgumentNullException("attribute");
		}
		return DateTime.Parse(attribute.value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
	}

	/// <summary>Cast the value of this <see cref="T:System.Xml.Linq.XAttribute" /> to a <see cref="T:System.Nullable`1" /> of <see cref="T:System.DateTime" />.</summary>
	/// <returns>A <see cref="T:System.Nullable`1" /> of <see cref="T:System.DateTime" /> that contains the content of this <see cref="T:System.Xml.Linq.XAttribute" />.</returns>
	/// <param name="attribute">The <see cref="T:System.Xml.Linq.XAttribute" /> to cast to a <see cref="T:System.Nullable`1" /> of <see cref="T:System.DateTime" />.</param>
	/// <exception cref="T:System.FormatException">The attribute does not contain a valid <see cref="T:System.DateTime" /> value.</exception>
	[CLSCompliant(false)]
	public static explicit operator DateTime?(XAttribute attribute)
	{
		if (attribute == null)
		{
			return null;
		}
		return DateTime.Parse(attribute.value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
	}

	/// <summary>Cast the value of this <see cref="T:System.Xml.Linq.XAttribute" /> to a <see cref="T:System.DateTimeOffset" />.</summary>
	/// <returns>A <see cref="T:System.DateTimeOffset" /> that contains the content of this <see cref="T:System.Xml.Linq.XAttribute" />.</returns>
	/// <param name="attribute">The <see cref="T:System.Xml.Linq.XAttribute" /> to cast to <see cref="T:System.DateTimeOffset" />.</param>
	/// <exception cref="T:System.FormatException">The attribute does not contain a valid <see cref="T:System.DateTimeOffset" /> value.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="attribute" /> parameter is null.</exception>
	[CLSCompliant(false)]
	public static explicit operator DateTimeOffset(XAttribute attribute)
	{
		if (attribute == null)
		{
			throw new ArgumentNullException("attribute");
		}
		return XmlConvert.ToDateTimeOffset(attribute.value);
	}

	/// <summary>Cast the value of this <see cref="T:System.Xml.Linq.XAttribute" /> to a <see cref="T:System.Nullable`1" /> of <see cref="T:System.DateTimeOffset" />.</summary>
	/// <returns>A <see cref="T:System.Nullable`1" /> of <see cref="T:System.DateTimeOffset" /> that contains the content of this <see cref="T:System.Xml.Linq.XAttribute" />.</returns>
	/// <param name="attribute">The <see cref="T:System.Xml.Linq.XAttribute" /> to cast to a <see cref="T:System.Nullable`1" /> of <see cref="T:System.DateTimeOffset" />.</param>
	/// <exception cref="T:System.FormatException">The attribute does not contain a valid <see cref="T:System.DateTimeOffset" /> value.</exception>
	[CLSCompliant(false)]
	public static explicit operator DateTimeOffset?(XAttribute attribute)
	{
		if (attribute == null)
		{
			return null;
		}
		return XmlConvert.ToDateTimeOffset(attribute.value);
	}

	/// <summary>Cast the value of this <see cref="T:System.Xml.Linq.XAttribute" /> to a <see cref="T:System.TimeSpan" />.</summary>
	/// <returns>A <see cref="T:System.TimeSpan" /> that contains the content of this <see cref="T:System.Xml.Linq.XAttribute" />.</returns>
	/// <param name="attribute">The <see cref="T:System.Xml.Linq.XAttribute" /> to cast to <see cref="T:System.TimeSpan" />.</param>
	/// <exception cref="T:System.FormatException">The attribute does not contain a valid <see cref="T:System.TimeSpan" /> value.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="attribute" /> parameter is null.</exception>
	[CLSCompliant(false)]
	public static explicit operator TimeSpan(XAttribute attribute)
	{
		if (attribute == null)
		{
			throw new ArgumentNullException("attribute");
		}
		return XmlConvert.ToTimeSpan(attribute.value);
	}

	/// <summary>Cast the value of this <see cref="T:System.Xml.Linq.XAttribute" /> to a <see cref="T:System.Nullable`1" /> of <see cref="T:System.TimeSpan" />.</summary>
	/// <returns>A <see cref="T:System.Nullable`1" /> of <see cref="T:System.TimeSpan" /> that contains the content of this <see cref="T:System.Xml.Linq.XAttribute" />.</returns>
	/// <param name="attribute">The <see cref="T:System.Xml.Linq.XAttribute" /> to cast to a <see cref="T:System.Nullable`1" /> of <see cref="T:System.TimeSpan" />.</param>
	/// <exception cref="T:System.FormatException">The attribute does not contain a valid <see cref="T:System.TimeSpan" /> value.</exception>
	[CLSCompliant(false)]
	public static explicit operator TimeSpan?(XAttribute attribute)
	{
		if (attribute == null)
		{
			return null;
		}
		return XmlConvert.ToTimeSpan(attribute.value);
	}

	/// <summary>Cast the value of this <see cref="T:System.Xml.Linq.XAttribute" /> to a <see cref="T:System.Guid" />.</summary>
	/// <returns>A <see cref="T:System.Guid" /> that contains the content of this <see cref="T:System.Xml.Linq.XAttribute" />.</returns>
	/// <param name="attribute">The <see cref="T:System.Xml.Linq.XAttribute" /> to cast to <see cref="T:System.Guid" />.</param>
	/// <exception cref="T:System.FormatException">The attribute does not contain a valid <see cref="T:System.Guid" /> value.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="attribute" /> parameter is null.</exception>
	[CLSCompliant(false)]
	public static explicit operator Guid(XAttribute attribute)
	{
		if (attribute == null)
		{
			throw new ArgumentNullException("attribute");
		}
		return XmlConvert.ToGuid(attribute.value);
	}

	/// <summary>Cast the value of this <see cref="T:System.Xml.Linq.XAttribute" /> to a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Guid" />.</summary>
	/// <returns>A <see cref="T:System.Nullable`1" /> of <see cref="T:System.Guid" /> that contains the content of this <see cref="T:System.Xml.Linq.XAttribute" />.</returns>
	/// <param name="attribute">The <see cref="T:System.Xml.Linq.XAttribute" /> to cast to a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Guid" />.</param>
	/// <exception cref="T:System.FormatException">The attribute does not contain a valid <see cref="T:System.Guid" /> value.</exception>
	[CLSCompliant(false)]
	public static explicit operator Guid?(XAttribute attribute)
	{
		if (attribute == null)
		{
			return null;
		}
		return XmlConvert.ToGuid(attribute.value);
	}

	internal int GetDeepHashCode()
	{
		return name.GetHashCode() ^ value.GetHashCode();
	}

	internal string GetPrefixOfNamespace(XNamespace ns)
	{
		string namespaceName = ns.NamespaceName;
		if (namespaceName.Length == 0)
		{
			return string.Empty;
		}
		if (parent != null)
		{
			return ((XElement)parent).GetPrefixOfNamespace(ns);
		}
		if ((object)namespaceName == "http://www.w3.org/XML/1998/namespace")
		{
			return "xml";
		}
		if ((object)namespaceName == "http://www.w3.org/2000/xmlns/")
		{
			return "xmlns";
		}
		return null;
	}

	private static void ValidateAttribute(XName name, string value)
	{
		string namespaceName = name.NamespaceName;
		if ((object)namespaceName == "http://www.w3.org/2000/xmlns/")
		{
			if (value.Length == 0)
			{
				throw new ArgumentException(Res.GetString("Argument_NamespaceDeclarationPrefixed", name.LocalName));
			}
			if (value == "http://www.w3.org/XML/1998/namespace")
			{
				if (name.LocalName != "xml")
				{
					throw new ArgumentException(Res.GetString("Argument_NamespaceDeclarationXml"));
				}
				return;
			}
			if (value == "http://www.w3.org/2000/xmlns/")
			{
				throw new ArgumentException(Res.GetString("Argument_NamespaceDeclarationXmlns"));
			}
			string localName = name.LocalName;
			if (localName == "xml")
			{
				throw new ArgumentException(Res.GetString("Argument_NamespaceDeclarationXml"));
			}
			if (localName == "xmlns")
			{
				throw new ArgumentException(Res.GetString("Argument_NamespaceDeclarationXmlns"));
			}
		}
		else if (namespaceName.Length == 0 && name.LocalName == "xmlns")
		{
			if (value == "http://www.w3.org/XML/1998/namespace")
			{
				throw new ArgumentException(Res.GetString("Argument_NamespaceDeclarationXml"));
			}
			if (value == "http://www.w3.org/2000/xmlns/")
			{
				throw new ArgumentException(Res.GetString("Argument_NamespaceDeclarationXmlns"));
			}
		}
	}
}
