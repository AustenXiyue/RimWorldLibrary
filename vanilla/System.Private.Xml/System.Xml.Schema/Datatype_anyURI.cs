namespace System.Xml.Schema;

internal sealed class Datatype_anyURI : Datatype_anySimpleType
{
	internal override FacetsChecker FacetsChecker => DatatypeImplementation.stringFacetsChecker;

	public override XmlTypeCode TypeCode => XmlTypeCode.AnyUri;

	public override Type ValueType => typeof(Uri);

	internal override bool HasValueFacets => true;

	internal override Type ListValueType => typeof(Uri[]);

	internal override XmlSchemaWhiteSpace BuiltInWhitespaceFacet => XmlSchemaWhiteSpace.Collapse;

	internal override RestrictionFlags ValidRestrictionFlags => RestrictionFlags.Length | RestrictionFlags.MinLength | RestrictionFlags.MaxLength | RestrictionFlags.Pattern | RestrictionFlags.Enumeration | RestrictionFlags.WhiteSpace;

	internal override XmlValueConverter CreateValueConverter(XmlSchemaType schemaType)
	{
		return XmlMiscConverter.Create(schemaType);
	}

	internal override int Compare(object value1, object value2)
	{
		if (!((Uri)value1).Equals((Uri)value2))
		{
			return -1;
		}
		return 0;
	}

	internal override Exception TryParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, out object typedValue)
	{
		typedValue = null;
		Exception ex = DatatypeImplementation.stringFacetsChecker.CheckLexicalFacets(ref s, this);
		if (ex == null)
		{
			ex = XmlConvert.TryToUri(s, out var result);
			if (ex == null)
			{
				string originalString = result.OriginalString;
				ex = StringFacetsChecker.CheckValueFacets(originalString, this, verifyUri: false);
				if (ex == null)
				{
					typedValue = result;
					return null;
				}
			}
		}
		return ex;
	}
}
