namespace System.Xml.Schema;

internal class Datatype_unsignedInt : Datatype_unsignedLong
{
	private static readonly Numeric10FacetsChecker s_numeric10FacetsChecker = new Numeric10FacetsChecker(0m, 4294967295m);

	internal override FacetsChecker FacetsChecker => s_numeric10FacetsChecker;

	public override XmlTypeCode TypeCode => XmlTypeCode.UnsignedInt;

	public override Type ValueType => typeof(uint);

	internal override Type ListValueType => typeof(uint[]);

	internal override int Compare(object value1, object value2)
	{
		return ((uint)value1).CompareTo((uint)value2);
	}

	internal override Exception TryParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, out object typedValue)
	{
		typedValue = null;
		Exception ex = s_numeric10FacetsChecker.CheckLexicalFacets(ref s, this);
		if (ex == null)
		{
			ex = XmlConvert.TryToUInt32(s, out var result);
			if (ex == null)
			{
				ex = s_numeric10FacetsChecker.CheckValueFacets(result, this);
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
