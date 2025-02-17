namespace System.Xml.Schema;

internal sealed class Datatype_char : Datatype_anySimpleType
{
	public override Type ValueType => typeof(char);

	internal override Type ListValueType => typeof(char[]);

	internal override RestrictionFlags ValidRestrictionFlags => (RestrictionFlags)0;

	internal override int Compare(object value1, object value2)
	{
		return ((char)value1).CompareTo((char)value2);
	}

	public override object ParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr)
	{
		try
		{
			return XmlConvert.ToChar(s);
		}
		catch (XmlSchemaException)
		{
			throw;
		}
		catch (Exception innerException)
		{
			throw new XmlSchemaException(System.SR.Format(System.SR.Sch_InvalidValue, s), innerException);
		}
	}

	internal override Exception TryParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, out object typedValue)
	{
		typedValue = null;
		char result;
		Exception ex = XmlConvert.TryToChar(s, out result);
		if (ex == null)
		{
			typedValue = result;
			return null;
		}
		return ex;
	}
}
