using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace MS.Internal.Xml.XPath;

[DebuggerDisplay("{ToString()}")]
internal abstract class Query : ResetableIterator
{
	public const XPathResultType XPathResultType_Navigator = (XPathResultType)4;

	public override int Count
	{
		get
		{
			if (count == -1)
			{
				Query query = (Query)Clone();
				query.Reset();
				count = 0;
				while (query.MoveNext())
				{
					count++;
				}
			}
			return count;
		}
	}

	public virtual double XsltDefaultPriority => 0.5;

	public abstract XPathResultType StaticType { get; }

	public virtual QueryProps Properties => QueryProps.Merge;

	public Query()
	{
	}

	protected Query(Query other)
		: base(other)
	{
	}

	public override bool MoveNext()
	{
		return Advance() != null;
	}

	public virtual void SetXsltContext(XsltContext context)
	{
	}

	public abstract object Evaluate(XPathNodeIterator nodeIterator);

	public abstract XPathNavigator Advance();

	public virtual XPathNavigator MatchNode(XPathNavigator current)
	{
		throw XPathException.Create("'{0}' is an invalid XSLT pattern.");
	}

	public static Query Clone(Query input)
	{
		if (input != null)
		{
			return (Query)input.Clone();
		}
		return null;
	}

	protected static XPathNodeIterator Clone(XPathNodeIterator input)
	{
		return input?.Clone();
	}

	protected static XPathNavigator Clone(XPathNavigator input)
	{
		return input?.Clone();
	}

	public bool Insert(List<XPathNavigator> buffer, XPathNavigator nav)
	{
		int num = 0;
		int num2 = buffer.Count;
		if (num2 != 0)
		{
			switch (CompareNodes(buffer[num2 - 1], nav))
			{
			case XmlNodeOrder.Same:
				return false;
			case XmlNodeOrder.Before:
				buffer.Add(nav.Clone());
				return true;
			}
			num2--;
		}
		while (num < num2)
		{
			int median = GetMedian(num, num2);
			switch (CompareNodes(buffer[median], nav))
			{
			case XmlNodeOrder.Same:
				return false;
			case XmlNodeOrder.Before:
				num = median + 1;
				break;
			default:
				num2 = median;
				break;
			}
		}
		buffer.Insert(num, nav.Clone());
		return true;
	}

	private static int GetMedian(int l, int r)
	{
		return l + r >>> 1;
	}

	public static XmlNodeOrder CompareNodes(XPathNavigator l, XPathNavigator r)
	{
		XmlNodeOrder xmlNodeOrder = l.ComparePosition(r);
		if (xmlNodeOrder == XmlNodeOrder.Unknown)
		{
			XPathNavigator xPathNavigator = l.Clone();
			xPathNavigator.MoveToRoot();
			string baseURI = xPathNavigator.BaseURI;
			if (!xPathNavigator.MoveTo(r))
			{
				xPathNavigator = r.Clone();
			}
			xPathNavigator.MoveToRoot();
			string baseURI2 = xPathNavigator.BaseURI;
			int num = string.CompareOrdinal(baseURI, baseURI2);
			xmlNodeOrder = ((num >= 0) ? ((num > 0) ? XmlNodeOrder.After : XmlNodeOrder.Unknown) : XmlNodeOrder.Before);
		}
		return xmlNodeOrder;
	}

	[Conditional("DEBUG")]
	private void AssertDOD(List<XPathNavigator> buffer, XPathNavigator nav, int pos)
	{
		if (!(nav.GetType().ToString() == "Microsoft.VisualStudio.Modeling.StoreNavigator") && !(nav.GetType().ToString() == "System.Xml.DataDocumentXPathNavigator"))
		{
			if (0 < pos)
			{
				CompareNodes(buffer[pos - 1], nav);
			}
			if (pos < buffer.Count)
			{
				CompareNodes(nav, buffer[pos]);
			}
		}
	}

	[Conditional("DEBUG")]
	public static void AssertQuery(Query query)
	{
		if (query is FunctionQuery)
		{
			return;
		}
		query = Clone(query);
		XPathNavigator xPathNavigator = null;
		_ = query.Clone().Count;
		int num = 0;
		XPathNavigator xPathNavigator2;
		while ((xPathNavigator2 = query.Advance()) != null && !(xPathNavigator2.GetType().ToString() == "Microsoft.VisualStudio.Modeling.StoreNavigator") && !(xPathNavigator2.GetType().ToString() == "System.Xml.DataDocumentXPathNavigator"))
		{
			if (xPathNavigator != null && (xPathNavigator.NodeType != XPathNodeType.Namespace || xPathNavigator2.NodeType != XPathNodeType.Namespace))
			{
				CompareNodes(xPathNavigator, xPathNavigator2);
			}
			xPathNavigator = xPathNavigator2.Clone();
			num++;
		}
	}

	protected XPathResultType GetXPathType(object value)
	{
		if (value is XPathNodeIterator)
		{
			return XPathResultType.NodeSet;
		}
		if (value is string)
		{
			return XPathResultType.String;
		}
		if (value is double)
		{
			return XPathResultType.Number;
		}
		if (value is bool)
		{
			return XPathResultType.Boolean;
		}
		return (XPathResultType)4;
	}

	public virtual void PrintQuery(XmlWriter w)
	{
		w.WriteElementString(GetType().Name, string.Empty);
	}
}
