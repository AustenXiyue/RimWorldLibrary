namespace System.Xml.Schema;

internal sealed class Datatype_unsignedByte : Datatype_unsignedShort
{
	private static readonly Numeric10FacetsChecker s_numeric10FacetsChecker = new Numeric10FacetsChecker(0m, 255m);

	internal override FacetsChecker FacetsChecker => s_numeric10FacetsChecker;

	public override XmlTypeCode TypeCode => XmlTypeCode.UnsignedByte;

	public override Type ValueType => typeof(byte);

	internal override Type ListValueType => typeof(byte[]);

	internal override int Compare(object value1, object value2)
	{
		return ((byte)value1).CompareTo((byte)value2);
	}

	internal override Exception TryParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, out object typedValue)
	{
		typedValue = null;
		Exception ex = s_numeric10FacetsChecker.CheckLexicalFacets(ref s, this);
		if (ex == null)
		{
			ex = XmlConvert.TryToByte(s, out var result);
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
