namespace System.Xml.Schema;

internal class Datatype_day : Datatype_dateTimeBase
{
	public override XmlTypeCode TypeCode => XmlTypeCode.GDay;

	internal Datatype_day()
		: base(XsdDateTimeFlags.GDay)
	{
	}
}
