namespace System.Xml.Schema;

internal class Datatype_duration : Datatype_anySimpleType
{
	internal override FacetsChecker FacetsChecker => DatatypeImplementation.durationFacetsChecker;

	public override XmlTypeCode TypeCode => XmlTypeCode.Duration;

	public override Type ValueType => typeof(TimeSpan);

	internal override Type ListValueType => typeof(TimeSpan[]);

	internal override XmlSchemaWhiteSpace BuiltInWhitespaceFacet => XmlSchemaWhiteSpace.Collapse;

	internal override RestrictionFlags ValidRestrictionFlags => RestrictionFlags.Pattern | RestrictionFlags.Enumeration | RestrictionFlags.WhiteSpace | RestrictionFlags.MaxInclusive | RestrictionFlags.MaxExclusive | RestrictionFlags.MinInclusive | RestrictionFlags.MinExclusive;

	internal override XmlValueConverter CreateValueConverter(XmlSchemaType schemaType)
	{
		return XmlMiscConverter.Create(schemaType);
	}

	internal override int Compare(object value1, object value2)
	{
		return ((TimeSpan)value1).CompareTo((TimeSpan)value2);
	}

	internal override Exception TryParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, out object typedValue)
	{
		typedValue = null;
		if (string.IsNullOrEmpty(s))
		{
			return new XmlSchemaException(System.SR.Sch_EmptyAttributeValue, string.Empty);
		}
		Exception ex = DatatypeImplementation.durationFacetsChecker.CheckLexicalFacets(ref s, this);
		if (ex == null)
		{
			ex = XmlConvert.TryToTimeSpan(s, out var result);
			if (ex == null)
			{
				ex = DatatypeImplementation.durationFacetsChecker.CheckValueFacets(result, this);
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
