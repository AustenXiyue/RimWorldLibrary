using System;
using System.Globalization;
using System.Xml;
using System.Xml.XPath;

namespace MS.Internal.Xml.XPath;

internal sealed class OperandQuery : ValueQuery
{
	internal object val;

	public override XPathResultType StaticType => GetXPathType(val);

	public OperandQuery(object val)
	{
		this.val = val;
	}

	public override object Evaluate(XPathNodeIterator nodeIterator)
	{
		return val;
	}

	public override XPathNodeIterator Clone()
	{
		return this;
	}

	public override void PrintQuery(XmlWriter w)
	{
		w.WriteStartElement(GetType().Name);
		w.WriteAttributeString("value", Convert.ToString(val, CultureInfo.InvariantCulture));
		w.WriteEndElement();
	}
}
