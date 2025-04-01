using System.Collections;
using System.Globalization;
using System.Text;

namespace System.Xml.Schema;

/// <summary>The <see cref="T:System.Xml.Schema.XmlSchemaDatatype" /> class is an abstract class for mapping XML Schema definition language (XSD) types to Common Language Runtime (CLR) types.</summary>
public abstract class XmlSchemaDatatype
{
	/// <summary>When overridden in a derived class, gets the Common Language Runtime (CLR) type of the item.</summary>
	/// <returns>The Common Language Runtime (CLR) type of the item.</returns>
	public abstract Type ValueType { get; }

	/// <summary>When overridden in a derived class, gets the type for the string as specified in the World Wide Web Consortium (W3C) XML 1.0 specification.</summary>
	/// <returns>An <see cref="T:System.Xml.XmlTokenizedType" /> value for the string.</returns>
	public abstract XmlTokenizedType TokenizedType { get; }

	/// <summary>Gets the <see cref="T:System.Xml.Schema.XmlSchemaDatatypeVariety" /> value for the simple type.</summary>
	/// <returns>The <see cref="T:System.Xml.Schema.XmlSchemaDatatypeVariety" /> value for the simple type.</returns>
	public virtual XmlSchemaDatatypeVariety Variety => XmlSchemaDatatypeVariety.Atomic;

	/// <summary>Gets the <see cref="T:System.Xml.Schema.XmlTypeCode" /> value for the simple type.</summary>
	/// <returns>The <see cref="T:System.Xml.Schema.XmlTypeCode" /> value for the simple type.</returns>
	public virtual XmlTypeCode TypeCode => XmlTypeCode.None;

	internal abstract bool HasLexicalFacets { get; }

	internal abstract bool HasValueFacets { get; }

	internal abstract XmlValueConverter ValueConverter { get; }

	internal abstract RestrictionFacets Restriction { get; set; }

	internal abstract FacetsChecker FacetsChecker { get; }

	internal abstract XmlSchemaWhiteSpace BuiltInWhitespaceFacet { get; }

	internal string TypeCodeString
	{
		get
		{
			string result = string.Empty;
			XmlTypeCode typeCode = TypeCode;
			switch (Variety)
			{
			case XmlSchemaDatatypeVariety.List:
				result = ((typeCode != XmlTypeCode.AnyAtomicType) ? ("List of " + TypeCodeToString(typeCode)) : "List of Union");
				break;
			case XmlSchemaDatatypeVariety.Union:
				result = "Union";
				break;
			case XmlSchemaDatatypeVariety.Atomic:
				result = ((typeCode != XmlTypeCode.AnyAtomicType) ? TypeCodeToString(typeCode) : "anySimpleType");
				break;
			}
			return result;
		}
	}

	/// <summary>When overridden in a derived class, validates the string specified against a built-in or user-defined simple type.</summary>
	/// <returns>An <see cref="T:System.Object" /> that can be cast safely to the type returned by the <see cref="P:System.Xml.Schema.XmlSchemaDatatype.ValueType" /> property.</returns>
	/// <param name="s">The string to validate against the simple type.</param>
	/// <param name="nameTable">The <see cref="T:System.Xml.XmlNameTable" /> to use for atomization while parsing the string if this <see cref="T:System.Xml.Schema.XmlSchemaDatatype" /> object represents the xs:NCName type. </param>
	/// <param name="nsmgr">The <see cref="T:System.Xml.IXmlNamespaceResolver" /> object to use while parsing the string if this <see cref="T:System.Xml.Schema.XmlSchemaDatatype" /> object represents the xs:QName type.</param>
	/// <exception cref="T:System.Xml.Schema.XmlSchemaValidationException">The input value is not a valid instance of this W3C XML Schema type.</exception>
	/// <exception cref="T:System.ArgumentNullException">The value to parse cannot be null.</exception>
	public abstract object ParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr);

	/// <summary>Converts the value specified, whose type is one of the valid Common Language Runtime (CLR) representations of the XML schema type represented by the <see cref="T:System.Xml.Schema.XmlSchemaDatatype" />, to the CLR type specified.</summary>
	/// <returns>The converted input value.</returns>
	/// <param name="value">The input value to convert to the specified type.</param>
	/// <param name="targetType">The target type to convert the input value to.</param>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="T:System.Object" /> or <see cref="T:System.Type" /> parameter is null.</exception>
	/// <exception cref="T:System.InvalidCastException">The type represented by the <see cref="T:System.Xml.Schema.XmlSchemaDatatype" />   does not support a conversion from type of the value specified to the type specified.</exception>
	public virtual object ChangeType(object value, Type targetType)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (targetType == null)
		{
			throw new ArgumentNullException("targetType");
		}
		return ValueConverter.ChangeType(value, targetType);
	}

	/// <summary>Converts the value specified, whose type is one of the valid Common Language Runtime (CLR) representations of the XML schema type represented by the <see cref="T:System.Xml.Schema.XmlSchemaDatatype" />, to the CLR type specified using the <see cref="T:System.Xml.IXmlNamespaceResolver" /> if the <see cref="T:System.Xml.Schema.XmlSchemaDatatype" /> represents the xs:QName type or a type derived from it.</summary>
	/// <returns>The converted input value.</returns>
	/// <param name="value">The input value to convert to the specified type.</param>
	/// <param name="targetType">The target type to convert the input value to.</param>
	/// <param name="namespaceResolver">An <see cref="T:System.Xml.IXmlNamespaceResolver" /> used for resolving namespace prefixes. This is only of use if the <see cref="T:System.Xml.Schema.XmlSchemaDatatype" />  represents the xs:QName type or a type derived from it.</param>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="T:System.Object" /> or <see cref="T:System.Type" /> parameter is null.</exception>
	/// <exception cref="T:System.InvalidCastException">The type represented by the <see cref="T:System.Xml.Schema.XmlSchemaDatatype" />   does not support a conversion from type of the value specified to the type specified.</exception>
	public virtual object ChangeType(object value, Type targetType, IXmlNamespaceResolver namespaceResolver)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (targetType == null)
		{
			throw new ArgumentNullException("targetType");
		}
		if (namespaceResolver == null)
		{
			throw new ArgumentNullException("namespaceResolver");
		}
		return ValueConverter.ChangeType(value, targetType, namespaceResolver);
	}

	/// <summary>The <see cref="M:System.Xml.Schema.XmlSchemaDatatype.IsDerivedFrom(System.Xml.Schema.XmlSchemaDatatype)" /> method always returns false.</summary>
	/// <returns>Always returns false.</returns>
	/// <param name="datatype">The <see cref="T:System.Xml.Schema.XmlSchemaDatatype" />.</param>
	public virtual bool IsDerivedFrom(XmlSchemaDatatype datatype)
	{
		return false;
	}

	internal abstract int Compare(object value1, object value2);

	internal abstract object ParseValue(string s, Type typDest, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr);

	internal abstract object ParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, bool createAtomicValue);

	internal abstract Exception TryParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, out object typedValue);

	internal abstract Exception TryParseValue(object value, XmlNameTable nameTable, IXmlNamespaceResolver namespaceResolver, out object typedValue);

	internal abstract XmlSchemaDatatype DeriveByRestriction(XmlSchemaObjectCollection facets, XmlNameTable nameTable, XmlSchemaType schemaType);

	internal abstract XmlSchemaDatatype DeriveByList(XmlSchemaType schemaType);

	internal abstract void VerifySchemaValid(XmlSchemaObjectTable notations, XmlSchemaObject caller);

	internal abstract bool IsEqual(object o1, object o2);

	internal abstract bool IsComparable(XmlSchemaDatatype dtype);

	internal string TypeCodeToString(XmlTypeCode typeCode)
	{
		return typeCode switch
		{
			XmlTypeCode.None => "None", 
			XmlTypeCode.Item => "AnyType", 
			XmlTypeCode.AnyAtomicType => "AnyAtomicType", 
			XmlTypeCode.String => "String", 
			XmlTypeCode.Boolean => "Boolean", 
			XmlTypeCode.Decimal => "Decimal", 
			XmlTypeCode.Float => "Float", 
			XmlTypeCode.Double => "Double", 
			XmlTypeCode.Duration => "Duration", 
			XmlTypeCode.DateTime => "DateTime", 
			XmlTypeCode.Time => "Time", 
			XmlTypeCode.Date => "Date", 
			XmlTypeCode.GYearMonth => "GYearMonth", 
			XmlTypeCode.GYear => "GYear", 
			XmlTypeCode.GMonthDay => "GMonthDay", 
			XmlTypeCode.GDay => "GDay", 
			XmlTypeCode.GMonth => "GMonth", 
			XmlTypeCode.HexBinary => "HexBinary", 
			XmlTypeCode.Base64Binary => "Base64Binary", 
			XmlTypeCode.AnyUri => "AnyUri", 
			XmlTypeCode.QName => "QName", 
			XmlTypeCode.Notation => "Notation", 
			XmlTypeCode.NormalizedString => "NormalizedString", 
			XmlTypeCode.Token => "Token", 
			XmlTypeCode.Language => "Language", 
			XmlTypeCode.NmToken => "NmToken", 
			XmlTypeCode.Name => "Name", 
			XmlTypeCode.NCName => "NCName", 
			XmlTypeCode.Id => "Id", 
			XmlTypeCode.Idref => "Idref", 
			XmlTypeCode.Entity => "Entity", 
			XmlTypeCode.Integer => "Integer", 
			XmlTypeCode.NonPositiveInteger => "NonPositiveInteger", 
			XmlTypeCode.NegativeInteger => "NegativeInteger", 
			XmlTypeCode.Long => "Long", 
			XmlTypeCode.Int => "Int", 
			XmlTypeCode.Short => "Short", 
			XmlTypeCode.Byte => "Byte", 
			XmlTypeCode.NonNegativeInteger => "NonNegativeInteger", 
			XmlTypeCode.UnsignedLong => "UnsignedLong", 
			XmlTypeCode.UnsignedInt => "UnsignedInt", 
			XmlTypeCode.UnsignedShort => "UnsignedShort", 
			XmlTypeCode.UnsignedByte => "UnsignedByte", 
			XmlTypeCode.PositiveInteger => "PositiveInteger", 
			_ => typeCode.ToString(), 
		};
	}

	internal static string ConcatenatedToString(object value)
	{
		Type type = value.GetType();
		string result = string.Empty;
		if (!(type == typeof(IEnumerable)) || !(type != typeof(string)))
		{
			result = ((!(value is IFormattable)) ? value.ToString() : ((IFormattable)value).ToString("", CultureInfo.InvariantCulture));
		}
		else
		{
			StringBuilder stringBuilder = new StringBuilder();
			IEnumerator enumerator = (value as IEnumerable).GetEnumerator();
			if (enumerator.MoveNext())
			{
				stringBuilder.Append("{");
				object current = enumerator.Current;
				if (current is IFormattable)
				{
					stringBuilder.Append(((IFormattable)current).ToString("", CultureInfo.InvariantCulture));
				}
				else
				{
					stringBuilder.Append(current.ToString());
				}
				while (enumerator.MoveNext())
				{
					stringBuilder.Append(" , ");
					current = enumerator.Current;
					if (current is IFormattable)
					{
						stringBuilder.Append(((IFormattable)current).ToString("", CultureInfo.InvariantCulture));
					}
					else
					{
						stringBuilder.Append(current.ToString());
					}
				}
				stringBuilder.Append("}");
				result = stringBuilder.ToString();
			}
		}
		return result;
	}

	internal static XmlSchemaDatatype FromXmlTokenizedType(XmlTokenizedType token)
	{
		return DatatypeImplementation.FromXmlTokenizedType(token);
	}

	internal static XmlSchemaDatatype FromXmlTokenizedTypeXsd(XmlTokenizedType token)
	{
		return DatatypeImplementation.FromXmlTokenizedTypeXsd(token);
	}

	internal static XmlSchemaDatatype FromXdrName(string name)
	{
		return DatatypeImplementation.FromXdrName(name);
	}

	internal static XmlSchemaDatatype DeriveByUnion(XmlSchemaSimpleType[] types, XmlSchemaType schemaType)
	{
		return DatatypeImplementation.DeriveByUnion(types, schemaType);
	}

	internal static string XdrCanonizeUri(string uri, XmlNameTable nameTable, SchemaNames schemaNames)
	{
		int num = 5;
		bool flag = false;
		if (uri.Length > 5 && uri.StartsWith("uuid:", StringComparison.Ordinal))
		{
			flag = true;
		}
		else if (uri.Length > 9 && uri.StartsWith("urn:uuid:", StringComparison.Ordinal))
		{
			flag = true;
			num = 9;
		}
		string text = ((!flag) ? uri : nameTable.Add(uri.Substring(0, num) + uri.Substring(num, uri.Length - num).ToUpper(CultureInfo.InvariantCulture)));
		if (Ref.Equal(schemaNames.NsDataTypeAlias, text) || Ref.Equal(schemaNames.NsDataTypeOld, text))
		{
			text = schemaNames.NsDataType;
		}
		else if (Ref.Equal(schemaNames.NsXdrAlias, text))
		{
			text = schemaNames.NsXdr;
		}
		return text;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaDatatype" /> class.</summary>
	protected XmlSchemaDatatype()
	{
	}
}
