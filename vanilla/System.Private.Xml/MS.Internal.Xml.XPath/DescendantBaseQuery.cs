using System;
using System.Xml.XPath;

namespace MS.Internal.Xml.XPath;

internal abstract class DescendantBaseQuery : BaseAxisQuery
{
	protected bool matchSelf;

	protected bool abbrAxis;

	public DescendantBaseQuery(Query qyParent, string Name, string Prefix, XPathNodeType Type, bool matchSelf, bool abbrAxis)
		: base(qyParent, Name, Prefix, Type)
	{
		this.matchSelf = matchSelf;
		this.abbrAxis = abbrAxis;
	}

	public DescendantBaseQuery(DescendantBaseQuery other)
		: base(other)
	{
		matchSelf = other.matchSelf;
		abbrAxis = other.abbrAxis;
	}

	public override XPathNavigator MatchNode(XPathNavigator context)
	{
		if (context != null)
		{
			if (!abbrAxis)
			{
				throw XPathException.Create(System.SR.Xp_InvalidPattern);
			}
			if (matches(context))
			{
				XPathNavigator result;
				if (matchSelf && (result = qyInput.MatchNode(context)) != null)
				{
					return result;
				}
				XPathNavigator xPathNavigator = context.Clone();
				while (xPathNavigator.MoveToParent())
				{
					if ((result = qyInput.MatchNode(xPathNavigator)) != null)
					{
						return result;
					}
				}
			}
		}
		return null;
	}
}
