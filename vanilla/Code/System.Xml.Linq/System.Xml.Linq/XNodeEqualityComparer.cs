using System.Collections;
using System.Collections.Generic;

namespace System.Xml.Linq;

/// <summary>Compares nodes to determine whether they are equal. This class cannot be inherited. </summary>
public sealed class XNodeEqualityComparer : IEqualityComparer, IEqualityComparer<XNode>
{
	/// <summary>Compares the values of two nodes.</summary>
	/// <returns>A <see cref="T:System.Boolean" /> indicating if the nodes are equal.</returns>
	/// <param name="x">The first <see cref="T:System.Xml.Linq.XNode" /> to compare.</param>
	/// <param name="y">The second <see cref="T:System.Xml.Linq.XNode" /> to compare.</param>
	public bool Equals(XNode x, XNode y)
	{
		return XNode.DeepEquals(x, y);
	}

	/// <summary>Returns a hash code based on an <see cref="T:System.Xml.Linq.XNode" />.</summary>
	/// <returns>A <see cref="T:System.Int32" /> that contains a value-based hash code for the node.</returns>
	/// <param name="obj">The <see cref="T:System.Xml.Linq.XNode" /> to hash.</param>
	public int GetHashCode(XNode obj)
	{
		return obj?.GetDeepHashCode() ?? 0;
	}

	/// <summary>Compares the values of two nodes.</summary>
	/// <returns>true if the nodes are equal; otherwise false.</returns>
	/// <param name="x">The first <see cref="T:System.Xml.Linq.XNode" /> to compare.</param>
	/// <param name="y">The second <see cref="T:System.Xml.Linq.XNode" /> to compare.</param>
	bool IEqualityComparer.Equals(object x, object y)
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
		return Equals(xNode, xNode2);
	}

	/// <summary>Returns a hash code based on the value of a node.</summary>
	/// <returns>A <see cref="T:System.Int32" /> that contains a value-based hash code for the node.</returns>
	/// <param name="obj">The node to hash.</param>
	int IEqualityComparer.GetHashCode(object obj)
	{
		XNode xNode = obj as XNode;
		if (xNode == null && obj != null)
		{
			throw new ArgumentException(Res.GetString("Argument_MustBeDerivedFrom", typeof(XNode)), "obj");
		}
		return GetHashCode(xNode);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Linq.XNodeEqualityComparer" /> class. </summary>
	public XNodeEqualityComparer()
	{
	}
}
