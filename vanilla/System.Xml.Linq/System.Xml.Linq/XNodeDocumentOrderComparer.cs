using System.Collections;
using System.Collections.Generic;

namespace System.Xml.Linq;

/// <summary>Contains functionality to compare nodes for their document order. This class cannot be inherited. </summary>
public sealed class XNodeDocumentOrderComparer : IComparer, IComparer<XNode>
{
	/// <summary>Compares two nodes to determine their relative document order.</summary>
	/// <returns>An <see cref="T:System.Int32" /> that contains 0 if the nodes are equal; -1 if <paramref name="x" /> is before <paramref name="y" />; 1 if <paramref name="x" /> is after <paramref name="y" />.</returns>
	/// <param name="x">The first <see cref="T:System.Xml.Linq.XNode" /> to compare.</param>
	/// <param name="y">The second <see cref="T:System.Xml.Linq.XNode" /> to compare.</param>
	/// <exception cref="T:System.InvalidOperationException">The two nodes do not share a common ancestor.</exception>
	public int Compare(XNode x, XNode y)
	{
		return XNode.CompareDocumentOrder(x, y);
	}

	/// <summary>Compares two nodes to determine their relative document order.</summary>
	/// <returns>An <see cref="T:System.Int32" /> that contains 0 if the nodes are equal; -1 if <paramref name="x" /> is before <paramref name="y" />; 1 if <paramref name="x" /> is after <paramref name="y" />.</returns>
	/// <param name="x">The first <see cref="T:System.Xml.Linq.XNode" /> to compare.</param>
	/// <param name="y">The second <see cref="T:System.Xml.Linq.XNode" /> to compare.</param>
	/// <exception cref="T:System.InvalidOperationException">The two nodes do not share a common ancestor.</exception>
	/// <exception cref="T:System.ArgumentException">The two nodes are not derived from <see cref="T:System.Xml.Linq.XNode" />.</exception>
	int IComparer.Compare(object x, object y)
	{
		XNode xNode = x as XNode;
		if (xNode == null && x != null)
		{
			throw new ArgumentException(Res.GetString("Argument_MustBeDerivedFrom", typeof(XNode)), "x");
		}
		XNode xNode2 = y as XNode;
		if (xNode2 == null && y != null)
		{
			throw new ArgumentException(Res.GetString("Argument_MustBeDerivedFrom", typeof(XNode)), "y");
		}
		return Compare(xNode, xNode2);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Linq.XNodeDocumentOrderComparer" /> class. </summary>
	public XNodeDocumentOrderComparer()
	{
	}
}
