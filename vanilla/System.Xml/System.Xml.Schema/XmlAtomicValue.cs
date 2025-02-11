using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml.XPath;
using Unity;

namespace System.Xml.Schema;

/// <summary>Represents the typed value of a validated XML element or attribute. The <see cref="T:System.Xml.Schema.XmlAtomicValue" /> class cannot be inherited.</summary>
public sealed class XmlAtomicValue : XPathItem, ICloneable
{
	[StructLayout(LayoutKind.Explicit, Size = 8)]
	private struct Union
	{
		[FieldOffset(0)]
		public bool boolVal;

		[FieldOffset(0)]
		public double dblVal;

		[FieldOffset(0)]
		public long i64Val;

		[FieldOffset(0)]
		public int i32Val;

		[FieldOffset(0)]
		public DateTime dtVal;
	}

	private class NamespacePrefixForQName : IXmlNamespaceResolver
	{
		public string prefix;

		public string ns;

		public NamespacePrefixForQName(string prefix, string ns)
		{
			this.ns = ns;
			this.prefix = prefix;
		}

		public string LookupNamespace(string prefix)
		{
			if (prefix == this.prefix)
			{
				return ns;
			}
			return null;
		}

		public string LookupPrefix(string namespaceName)
		{
			if (ns == namespaceName)
			{
				return prefix;
			}
			return null;
		}

		public IDictionary<string, string> GetNamespacesInScope(XmlNamespaceScope scope)
		{
			return new Dictionary<string, string>(1) { [prefix] = ns };
		}
	}

	private XmlSchemaType xmlType;

	private object objVal;

	private TypeCode clrType;

	private Union unionVal;

	private NamespacePrefixForQName nsPrefix;

	/// <summary>Gets a value indicating whether the validated XML element or attribute is an XPath node or an atomic value.</summary>
	/// <returns>true if the validated XML element or attribute is an XPath node; false if the validated XML element or attribute is an atomic value.</returns>
	public override bool IsNode => false;

	/// <summary>Gets the <see cref="T:System.Xml.Schema.XmlSchemaType" /> for the validated XML element or attribute.</summary>
	/// <returns>The <see cref="T:System.Xml.Schema.XmlSchemaType" /> for the validated XML element or attribute.</returns>
	public override XmlSchemaType XmlType => xmlType;

	/// <summary>Gets the Microsoft .NET Framework type of the validated XML element or attribute.</summary>
	/// <returns>The .NET Framework type of the validated XML element or attribute. The default value is <see cref="T:System.String" />.</returns>
	public override Type ValueType => xmlType.Datatype.ValueType;

	/// <summary>Gets the current validated XML element or attribute as a boxed object of the most appropriate Microsoft .NET Framework type according to its schema type.</summary>
	/// <returns>The current validated XML element or attribute as a boxed object of the most appropriate .NET Framework type.</returns>
	public override object TypedValue
	{
		get
		{
			XmlValueConverter valueConverter = xmlType.ValueConverter;
			if (objVal == null)
			{
				switch (clrType)
				{
				case TypeCode.Boolean:
					return valueConverter.ChangeType(unionVal.boolVal, ValueType);
				case TypeCode.Int32:
					return valueConverter.ChangeType(unionVal.i32Val, ValueType);
				case TypeCode.Int64:
					return valueConverter.ChangeType(unionVal.i64Val, ValueType);
				case TypeCode.Double:
					return valueConverter.ChangeType(unionVal.dblVal, ValueType);
				case TypeCode.DateTime:
					return valueConverter.ChangeType(unionVal.dtVal, ValueType);
				}
			}
			return valueConverter.ChangeType(objVal, ValueType, nsPrefix);
		}
	}

	/// <summary>Gets the validated XML element or attribute's value as a <see cref="T:System.Boolean" />.</summary>
	/// <returns>The validated XML element or attribute's value as a <see cref="T:System.Boolean" />.</returns>
	/// <exception cref="T:System.FormatException">The validated XML element or attribute's value is not in the correct format for the <see cref="T:System.Boolean" /> type.</exception>
	/// <exception cref="T:System.InvalidCastException">The attempted cast to <see cref="T:System.Boolean" /> is not valid.</exception>
	public override bool ValueAsBoolean
	{
		get
		{
			XmlValueConverter valueConverter = xmlType.ValueConverter;
			if (objVal == null)
			{
				switch (clrType)
				{
				case TypeCode.Boolean:
					return unionVal.boolVal;
				case TypeCode.Int32:
					return valueConverter.ToBoolean(unionVal.i32Val);
				case TypeCode.Int64:
					return valueConverter.ToBoolean(unionVal.i64Val);
				case TypeCode.Double:
					return valueConverter.ToBoolean(unionVal.dblVal);
				case TypeCode.DateTime:
					return valueConverter.ToBoolean(unionVal.dtVal);
				}
			}
			return valueConverter.ToBoolean(objVal);
		}
	}

	/// <summary>Gets the validated XML element or attribute's value as a <see cref="T:System.DateTime" />.</summary>
	/// <returns>The validated XML element or attribute's value as a <see cref="T:System.DateTime" />.</returns>
	/// <exception cref="T:System.FormatException">The validated XML element or attribute's value is not in the correct format for the <see cref="T:System.DateTime" /> type.</exception>
	/// <exception cref="T:System.InvalidCastException">The attempted cast to <see cref="T:System.DateTime" /> is not valid.</exception>
	public override DateTime ValueAsDateTime
	{
		get
		{
			XmlValueConverter valueConverter = xmlType.ValueConverter;
			if (objVal == null)
			{
				switch (clrType)
				{
				case TypeCode.Boolean:
					return valueConverter.ToDateTime(unionVal.boolVal);
				case TypeCode.Int32:
					return valueConverter.ToDateTime(unionVal.i32Val);
				case TypeCode.Int64:
					return valueConverter.ToDateTime(unionVal.i64Val);
				case TypeCode.Double:
					return valueConverter.ToDateTime(unionVal.dblVal);
				case TypeCode.DateTime:
					return unionVal.dtVal;
				}
			}
			return valueConverter.ToDateTime(objVal);
		}
	}

	/// <summary>Gets the validated XML element or attribute's value as a <see cref="T:System.Double" />.</summary>
	/// <returns>The validated XML element or attribute's value as a <see cref="T:System.Double" />.</returns>
	/// <exception cref="T:System.FormatException">The validated XML element or attribute's value is not in the correct format for the <see cref="T:System.Double" /> type.</exception>
	/// <exception cref="T:System.InvalidCastException">The attempted cast to <see cref="T:System.Double" /> is not valid.</exception>
	/// <exception cref="T:System.OverflowException">The attempted cast resulted in an overflow.</exception>
	public override double ValueAsDouble
	{
		get
		{
			XmlValueConverter valueConverter = xmlType.ValueConverter;
			if (objVal == null)
			{
				switch (clrType)
				{
				case TypeCode.Boolean:
					return valueConverter.ToDouble(unionVal.boolVal);
				case TypeCode.Int32:
					return valueConverter.ToDouble(unionVal.i32Val);
				case TypeCode.Int64:
					return valueConverter.ToDouble(unionVal.i64Val);
				case TypeCode.Double:
					return unionVal.dblVal;
				case TypeCode.DateTime:
					return valueConverter.ToDouble(unionVal.dtVal);
				}
			}
			return valueConverter.ToDouble(objVal);
		}
	}

	/// <summary>Gets the validated XML element or attribute's value as an <see cref="T:System.Int32" />.</summary>
	/// <returns>The validated XML element or attribute's value as an <see cref="T:System.Int32" />.</returns>
	/// <exception cref="T:System.FormatException">The validated XML element or attribute's value is not in the correct format for the <see cref="T:System.Int32" /> type.</exception>
	/// <exception cref="T:System.InvalidCastException">The attempted cast to <see cref="T:System.Int32" /> is not valid.</exception>
	/// <exception cref="T:System.OverflowException">The attempted cast resulted in an overflow.</exception>
	public override int ValueAsInt
	{
		get
		{
			XmlValueConverter valueConverter = xmlType.ValueConverter;
			if (objVal == null)
			{
				switch (clrType)
				{
				case TypeCode.Boolean:
					return valueConverter.ToInt32(unionVal.boolVal);
				case TypeCode.Int32:
					return unionVal.i32Val;
				case TypeCode.Int64:
					return valueConverter.ToInt32(unionVal.i64Val);
				case TypeCode.Double:
					return valueConverter.ToInt32(unionVal.dblVal);
				case TypeCode.DateTime:
					return valueConverter.ToInt32(unionVal.dtVal);
				}
			}
			return valueConverter.ToInt32(objVal);
		}
	}

	/// <summary>Gets the validated XML element or attribute's value as an <see cref="T:System.Int64" />.</summary>
	/// <returns>The validated XML element or attribute's value as an <see cref="T:System.Int64" />.</returns>
	/// <exception cref="T:System.FormatException">The validated XML element or attribute's value is not in the correct format for the <see cref="T:System.Int64" /> type.</exception>
	/// <exception cref="T:System.InvalidCastException">The attempted cast to <see cref="T:System.Int64" /> is not valid.</exception>
	/// <exception cref="T:System.OverflowException">The attempted cast resulted in an overflow.</exception>
	public override long ValueAsLong
	{
		get
		{
			XmlValueConverter valueConverter = xmlType.ValueConverter;
			if (objVal == null)
			{
				switch (clrType)
				{
				case TypeCode.Boolean:
					return valueConverter.ToInt64(unionVal.boolVal);
				case TypeCode.Int32:
					return valueConverter.ToInt64(unionVal.i32Val);
				case TypeCode.Int64:
					return unionVal.i64Val;
				case TypeCode.Double:
					return valueConverter.ToInt64(unionVal.dblVal);
				case TypeCode.DateTime:
					return valueConverter.ToInt64(unionVal.dtVal);
				}
			}
			return valueConverter.ToInt64(objVal);
		}
	}

	/// <summary>Gets the string value of the validated XML element or attribute.</summary>
	/// <returns>The string value of the validated XML element or attribute.</returns>
	public override string Value
	{
		get
		{
			XmlValueConverter valueConverter = xmlType.ValueConverter;
			if (objVal == null)
			{
				switch (clrType)
				{
				case TypeCode.Boolean:
					return valueConverter.ToString(unionVal.boolVal);
				case TypeCode.Int32:
					return valueConverter.ToString(unionVal.i32Val);
				case TypeCode.Int64:
					return valueConverter.ToString(unionVal.i64Val);
				case TypeCode.Double:
					return valueConverter.ToString(unionVal.dblVal);
				case TypeCode.DateTime:
					return valueConverter.ToString(unionVal.dtVal);
				}
			}
			return valueConverter.ToString(objVal, nsPrefix);
		}
	}

	internal XmlAtomicValue(XmlSchemaType xmlType, bool value)
	{
		if (xmlType == null)
		{
			throw new ArgumentNullException("xmlType");
		}
		this.xmlType = xmlType;
		clrType = TypeCode.Boolean;
		unionVal.boolVal = value;
	}

	internal XmlAtomicValue(XmlSchemaType xmlType, DateTime value)
	{
		if (xmlType == null)
		{
			throw new ArgumentNullException("xmlType");
		}
		this.xmlType = xmlType;
		clrType = TypeCode.DateTime;
		unionVal.dtVal = value;
	}

	internal XmlAtomicValue(XmlSchemaType xmlType, double value)
	{
		if (xmlType == null)
		{
			throw new ArgumentNullException("xmlType");
		}
		this.xmlType = xmlType;
		clrType = TypeCode.Double;
		unionVal.dblVal = value;
	}

	internal XmlAtomicValue(XmlSchemaType xmlType, int value)
	{
		if (xmlType == null)
		{
			throw new ArgumentNullException("xmlType");
		}
		this.xmlType = xmlType;
		clrType = TypeCode.Int32;
		unionVal.i32Val = value;
	}

	internal XmlAtomicValue(XmlSchemaType xmlType, long value)
	{
		if (xmlType == null)
		{
			throw new ArgumentNullException("xmlType");
		}
		this.xmlType = xmlType;
		clrType = TypeCode.Int64;
		unionVal.i64Val = value;
	}

	internal XmlAtomicValue(XmlSchemaType xmlType, string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (xmlType == null)
		{
			throw new ArgumentNullException("xmlType");
		}
		this.xmlType = xmlType;
		objVal = value;
	}

	internal XmlAtomicValue(XmlSchemaType xmlType, string value, IXmlNamespaceResolver nsResolver)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (xmlType == null)
		{
			throw new ArgumentNullException("xmlType");
		}
		this.xmlType = xmlType;
		objVal = value;
		if (nsResolver != null && (this.xmlType.TypeCode == XmlTypeCode.QName || this.xmlType.TypeCode == XmlTypeCode.Notation))
		{
			string prefixFromQName = GetPrefixFromQName(value);
			nsPrefix = new NamespacePrefixForQName(prefixFromQName, nsResolver.LookupNamespace(prefixFromQName));
		}
	}

	internal XmlAtomicValue(XmlSchemaType xmlType, object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (xmlType == null)
		{
			throw new ArgumentNullException("xmlType");
		}
		this.xmlType = xmlType;
		objVal = value;
	}

	internal XmlAtomicValue(XmlSchemaType xmlType, object value, IXmlNamespaceResolver nsResolver)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (xmlType == null)
		{
			throw new ArgumentNullException("xmlType");
		}
		this.xmlType = xmlType;
		objVal = value;
		if (nsResolver != null && (this.xmlType.TypeCode == XmlTypeCode.QName || this.xmlType.TypeCode == XmlTypeCode.Notation))
		{
			string @namespace = (objVal as XmlQualifiedName).Namespace;
			nsPrefix = new NamespacePrefixForQName(nsResolver.LookupPrefix(@namespace), @namespace);
		}
	}

	/// <summary>Returns a copy of this <see cref="T:System.Xml.Schema.XmlAtomicValue" /> object.</summary>
	/// <returns>An <see cref="T:System.Xml.Schema.XmlAtomicValue" /> object copy of this <see cref="T:System.Xml.Schema.XmlAtomicValue" /> object.</returns>
	public XmlAtomicValue Clone()
	{
		return this;
	}

	/// <summary>For a description of this member, see <see cref="M:System.Xml.Schema.XmlAtomicValue.Clone" />.</summary>
	/// <returns>Returns a copy of this <see cref="T:System.Xml.Schema.XmlAtomicValue" /> object.</returns>
	object ICloneable.Clone()
	{
		return this;
	}

	/// <summary>Returns the validated XML element or attribute's value as the type specified using the <see cref="T:System.Xml.IXmlNamespaceResolver" /> object specified to resolve namespace prefixes.</summary>
	/// <returns>The value of the validated XML element or attribute as the type requested.</returns>
	/// <param name="type">The type to return the validated XML element or attribute's value as.</param>
	/// <param name="nsResolver">The <see cref="T:System.Xml.IXmlNamespaceResolver" /> object used to resolve namespace prefixes.</param>
	/// <exception cref="T:System.FormatException">The validated XML element or attribute's value is not in the correct format for the target type.</exception>
	/// <exception cref="T:System.InvalidCastException">The attempted cast is not valid.</exception>
	/// <exception cref="T:System.OverflowException">The attempted cast resulted in an overflow.</exception>
	public override object ValueAs(Type type, IXmlNamespaceResolver nsResolver)
	{
		XmlValueConverter valueConverter = xmlType.ValueConverter;
		if (type == typeof(XPathItem) || type == typeof(XmlAtomicValue))
		{
			return this;
		}
		if (objVal == null)
		{
			switch (clrType)
			{
			case TypeCode.Boolean:
				return valueConverter.ChangeType(unionVal.boolVal, type);
			case TypeCode.Int32:
				return valueConverter.ChangeType(unionVal.i32Val, type);
			case TypeCode.Int64:
				return valueConverter.ChangeType(unionVal.i64Val, type);
			case TypeCode.Double:
				return valueConverter.ChangeType(unionVal.dblVal, type);
			case TypeCode.DateTime:
				return valueConverter.ChangeType(unionVal.dtVal, type);
			}
		}
		return valueConverter.ChangeType(objVal, type, nsResolver);
	}

	/// <summary>Gets the string value of the validated XML element or attribute.</summary>
	/// <returns>The string value of the validated XML element or attribute.</returns>
	public override string ToString()
	{
		return Value;
	}

	private string GetPrefixFromQName(string value)
	{
		int colonOffset;
		int num = ValidateNames.ParseQName(value, 0, out colonOffset);
		if (num == 0 || num != value.Length)
		{
			return null;
		}
		if (colonOffset != 0)
		{
			return value.Substring(0, colonOffset);
		}
		return string.Empty;
	}

	internal XmlAtomicValue()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
