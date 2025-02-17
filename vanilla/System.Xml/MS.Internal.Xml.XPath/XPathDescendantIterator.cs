using System.Xml.XPath;

namespace MS.Internal.Xml.XPath;

internal class XPathDescendantIterator : XPathAxisIterator
{
	private int level;

	public XPathDescendantIterator(XPathNavigator nav, XPathNodeType type, bool matchSelf)
		: base(nav, type, matchSelf)
	{
	}

	public XPathDescendantIterator(XPathNavigator nav, string name, string namespaceURI, bool matchSelf)
		: base(nav, name, namespaceURI, matchSelf)
	{
	}

	public XPathDescendantIterator(XPathDescendantIterator it)
		: base(it)
	{
		level = it.level;
	}

	public override XPathNodeIterator Clone()
	{
		return new XPathDescendantIterator(this);
	}

	public override bool MoveNext()
	{
		if (first)
		{
			first = false;
			if (matchSelf && Matches)
			{
				position = 1;
				return true;
			}
		}
		do
		{
			if (nav.MoveToFirstChild())
			{
				level++;
				continue;
			}
			while (true)
			{
				if (level == 0)
				{
					return false;
				}
				if (nav.MoveToNext())
				{
					break;
				}
				nav.MoveToParent();
				level--;
			}
		}
		while (!Matches);
		position++;
		return true;
	}
}
