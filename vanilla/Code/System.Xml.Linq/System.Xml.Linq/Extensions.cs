using System.Collections.Generic;
using System.Linq;

namespace System.Xml.Linq;

/// <summary>Contains the LINQ to XML extension methods.</summary>
/// <filterpriority>2</filterpriority>
public static class Extensions
{
	/// <summary>Returns a collection of the attributes of every element in the source collection.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XAttribute" /> that contains the attributes of every element in the source collection.</returns>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XElement" /> that contains the source collection.</param>
	/// <filterpriority>2</filterpriority>
	public static IEnumerable<XAttribute> Attributes(this IEnumerable<XElement> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return GetAttributes(source, null);
	}

	/// <summary>Returns a filtered collection of the attributes of every element in the source collection. Only elements that have a matching <see cref="T:System.Xml.Linq.XName" /> are included in the collection.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XAttribute" /> that contains a filtered collection of the attributes of every element in the source collection. Only elements that have a matching <see cref="T:System.Xml.Linq.XName" /> are included in the collection.</returns>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XElement" /> that contains the source collection.</param>
	/// <param name="name">The <see cref="T:System.Xml.Linq.XName" /> to match.</param>
	/// <filterpriority>2</filterpriority>
	public static IEnumerable<XAttribute> Attributes(this IEnumerable<XElement> source, XName name)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (!(name != null))
		{
			return XAttribute.EmptySequence;
		}
		return GetAttributes(source, name);
	}

	/// <summary>Returns a collection of elements that contains the ancestors of every node in the source collection.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XElement" /> that contains the ancestors of every node in the source collection.</returns>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XNode" /> that contains the source collection.</param>
	/// <typeparam name="T">The type of the objects in <paramref name="source" />, constrained to <see cref="T:System.Xml.Linq.XNode" />.</typeparam>
	/// <filterpriority>2</filterpriority>
	public static IEnumerable<XElement> Ancestors<T>(this IEnumerable<T> source) where T : XNode
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return GetAncestors(source, null, self: false);
	}

	/// <summary>Returns a filtered collection of elements that contains the ancestors of every node in the source collection. Only elements that have a matching <see cref="T:System.Xml.Linq.XName" /> are included in the collection.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XElement" /> that contains the ancestors of every node in the source collection. Only elements that have a matching <see cref="T:System.Xml.Linq.XName" /> are included in the collection.</returns>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XNode" /> that contains the source collection.</param>
	/// <param name="name">The <see cref="T:System.Xml.Linq.XName" /> to match.</param>
	/// <typeparam name="T">The type of the objects in <paramref name="source" />, constrained to <see cref="T:System.Xml.Linq.XNode" />.</typeparam>
	/// <filterpriority>2</filterpriority>
	public static IEnumerable<XElement> Ancestors<T>(this IEnumerable<T> source, XName name) where T : XNode
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (!(name != null))
		{
			return XElement.EmptySequence;
		}
		return GetAncestors(source, name, self: false);
	}

	/// <summary>Returns a collection of elements that contains every element in the source collection, and the ancestors of every element in the source collection.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XElement" /> that contains every element in the source collection, and the ancestors of every element in the source collection.</returns>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XElement" /> that contains the source collection.</param>
	/// <filterpriority>2</filterpriority>
	public static IEnumerable<XElement> AncestorsAndSelf(this IEnumerable<XElement> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return GetAncestors(source, null, self: true);
	}

	/// <summary>Returns a filtered collection of elements that contains every element in the source collection, and the ancestors of every element in the source collection. Only elements that have a matching <see cref="T:System.Xml.Linq.XName" /> are included in the collection.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XElement" /> that contains every element in the source collection, and the ancestors of every element in the source collection. Only elements that have a matching <see cref="T:System.Xml.Linq.XName" /> are included in the collection.</returns>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XElement" /> that contains the source collection.</param>
	/// <param name="name">The <see cref="T:System.Xml.Linq.XName" /> to match.</param>
	/// <filterpriority>2</filterpriority>
	public static IEnumerable<XElement> AncestorsAndSelf(this IEnumerable<XElement> source, XName name)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (!(name != null))
		{
			return XElement.EmptySequence;
		}
		return GetAncestors(source, name, self: true);
	}

	/// <summary>Returns a collection of the child nodes of every document and element in the source collection.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XNode" /> of the child nodes of every document and element in the source collection.</returns>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XNode" /> that contains the source collection.</param>
	/// <typeparam name="T">The type of the objects in <paramref name="source" />, constrained to <see cref="T:System.Xml.Linq.XContainer" />.</typeparam>
	/// <filterpriority>2</filterpriority>
	public static IEnumerable<XNode> Nodes<T>(this IEnumerable<T> source) where T : XContainer
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		foreach (T root in source)
		{
			if (root == null)
			{
				continue;
			}
			XNode n = root.LastNode;
			if (n != null)
			{
				do
				{
					n = n.next;
					yield return n;
				}
				while (n.parent == root && n != root.content);
			}
		}
	}

	/// <summary>Returns a collection of the descendant nodes of every document and element in the source collection.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XNode" /> of the descendant nodes of every document and element in the source collection.</returns>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XContainer" /> that contains the source collection.</param>
	/// <typeparam name="T">The type of the objects in <paramref name="source" />, constrained to <see cref="T:System.Xml.Linq.XContainer" />.</typeparam>
	/// <filterpriority>2</filterpriority>
	public static IEnumerable<XNode> DescendantNodes<T>(this IEnumerable<T> source) where T : XContainer
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return GetDescendantNodes(source, self: false);
	}

	/// <summary>Returns a collection of elements that contains the descendant elements of every element and document in the source collection.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XElement" /> that contains the descendant elements of every element and document in the source collection.</returns>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XContainer" /> that contains the source collection.</param>
	/// <typeparam name="T">The type of the objects in <paramref name="source" />, constrained to <see cref="T:System.Xml.Linq.XContainer" />.</typeparam>
	/// <filterpriority>2</filterpriority>
	public static IEnumerable<XElement> Descendants<T>(this IEnumerable<T> source) where T : XContainer
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return GetDescendants(source, null, self: false);
	}

	/// <summary>Returns a filtered collection of elements that contains the descendant elements of every element and document in the source collection. Only elements that have a matching <see cref="T:System.Xml.Linq.XName" /> are included in the collection.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XElement" /> that contains the descendant elements of every element and document in the source collection. Only elements that have a matching <see cref="T:System.Xml.Linq.XName" /> are included in the collection.</returns>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XContainer" /> that contains the source collection.</param>
	/// <param name="name">The <see cref="T:System.Xml.Linq.XName" /> to match.</param>
	/// <typeparam name="T">The type of the objects in <paramref name="source" />, constrained to <see cref="T:System.Xml.Linq.XContainer" />.</typeparam>
	/// <filterpriority>2</filterpriority>
	public static IEnumerable<XElement> Descendants<T>(this IEnumerable<T> source, XName name) where T : XContainer
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (!(name != null))
		{
			return XElement.EmptySequence;
		}
		return GetDescendants(source, name, self: false);
	}

	/// <summary>Returns a collection of nodes that contains every element in the source collection, and the descendant nodes of every element in the source collection.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XNode" /> that contains every element in the source collection, and the descendant nodes of every element in the source collection.</returns>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XElement" /> that contains the source collection.</param>
	/// <filterpriority>2</filterpriority>
	public static IEnumerable<XNode> DescendantNodesAndSelf(this IEnumerable<XElement> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return GetDescendantNodes(source, self: true);
	}

	/// <summary>Returns a collection of elements that contains every element in the source collection, and the descendent elements of every element in the source collection.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XElement" /> that contains every element in the source collection, and the descendent elements of every element in the source collection.</returns>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XElement" /> that contains the source collection.</param>
	/// <filterpriority>2</filterpriority>
	public static IEnumerable<XElement> DescendantsAndSelf(this IEnumerable<XElement> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return GetDescendants(source, null, self: true);
	}

	/// <summary>Returns a filtered collection of elements that contains every element in the source collection, and the descendents of every element in the source collection. Only elements that have a matching <see cref="T:System.Xml.Linq.XName" /> are included in the collection.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XElement" /> that contains every element in the source collection, and the descendents of every element in the source collection. Only elements that have a matching <see cref="T:System.Xml.Linq.XName" /> are included in the collection.</returns>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XElement" /> that contains the source collection.</param>
	/// <param name="name">The <see cref="T:System.Xml.Linq.XName" /> to match.</param>
	/// <filterpriority>2</filterpriority>
	public static IEnumerable<XElement> DescendantsAndSelf(this IEnumerable<XElement> source, XName name)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (!(name != null))
		{
			return XElement.EmptySequence;
		}
		return GetDescendants(source, name, self: true);
	}

	/// <summary>Returns a collection of the child elements of every element and document in the source collection.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XElement" /> of the child elements of every element or document in the source collection.</returns>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XElement" /> that contains the source collection.</param>
	/// <typeparam name="T">The type of the objects in <paramref name="source" />, constrained to <see cref="T:System.Xml.Linq.XContainer" />.</typeparam>
	/// <filterpriority>2</filterpriority>
	public static IEnumerable<XElement> Elements<T>(this IEnumerable<T> source) where T : XContainer
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return GetElements(source, null);
	}

	/// <summary>Returns a filtered collection of the child elements of every element and document in the source collection. Only elements that have a matching <see cref="T:System.Xml.Linq.XName" /> are included in the collection.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XElement" /> of the child elements of every element and document in the source collection. Only elements that have a matching <see cref="T:System.Xml.Linq.XName" /> are included in the collection.</returns>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XElement" /> that contains the source collection.</param>
	/// <param name="name">The <see cref="T:System.Xml.Linq.XName" /> to match.</param>
	/// <typeparam name="T">The type of the objects in <paramref name="source" />, constrained to <see cref="T:System.Xml.Linq.XContainer" />.</typeparam>
	/// <filterpriority>2</filterpriority>
	public static IEnumerable<XElement> Elements<T>(this IEnumerable<T> source, XName name) where T : XContainer
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (!(name != null))
		{
			return XElement.EmptySequence;
		}
		return GetElements(source, name);
	}

	/// <summary>Returns a collection of nodes that contains all nodes in the source collection, sorted in document order.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XNode" /> that contains all nodes in the source collection, sorted in document order.</returns>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XNode" /> that contains the source collection.</param>
	/// <typeparam name="T">The type of the objects in <paramref name="source" />, constrained to <see cref="T:System.Xml.Linq.XNode" />.</typeparam>
	/// <filterpriority>2</filterpriority>
	public static IEnumerable<T> InDocumentOrder<T>(this IEnumerable<T> source) where T : XNode
	{
		return source.OrderBy((T n) => n, XNode.DocumentOrderComparer);
	}

	/// <summary>Removes every attribute in the source collection from its parent element.</summary>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XAttribute" /> that contains the source collection.</param>
	/// <filterpriority>2</filterpriority>
	public static void Remove(this IEnumerable<XAttribute> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		foreach (XAttribute item in new List<XAttribute>(source))
		{
			item?.Remove();
		}
	}

	/// <summary>Removes every node in the source collection from its parent node.</summary>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XNode" /> that contains the source collection.</param>
	/// <typeparam name="T">The type of the objects in <paramref name="source" />, constrained to <see cref="T:System.Xml.Linq.XNode" />.</typeparam>
	/// <filterpriority>2</filterpriority>
	public static void Remove<T>(this IEnumerable<T> source) where T : XNode
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		foreach (T item in new List<T>(source))
		{
			item?.Remove();
		}
	}

	private static IEnumerable<XAttribute> GetAttributes(IEnumerable<XElement> source, XName name)
	{
		foreach (XElement e in source)
		{
			if (e == null)
			{
				continue;
			}
			XAttribute a = e.lastAttr;
			if (a == null)
			{
				continue;
			}
			do
			{
				a = a.next;
				if (name == null || a.name == name)
				{
					yield return a;
				}
			}
			while (a.parent == e && a != e.lastAttr);
		}
	}

	private static IEnumerable<XElement> GetAncestors<T>(IEnumerable<T> source, XName name, bool self) where T : XNode
	{
		foreach (T item in source)
		{
			if (item == null)
			{
				continue;
			}
			for (XElement e = (self ? ((XNode)item) : ((XNode)item.parent)) as XElement; e != null; e = e.parent as XElement)
			{
				if (name == null || e.name == name)
				{
					yield return e;
				}
			}
		}
	}

	private static IEnumerable<XNode> GetDescendantNodes<T>(IEnumerable<T> source, bool self) where T : XContainer
	{
		foreach (T root in source)
		{
			if (root == null)
			{
				continue;
			}
			if (self)
			{
				yield return root;
			}
			XNode n = root;
			while (true)
			{
				XNode firstNode;
				if (n is XContainer xContainer && (firstNode = xContainer.FirstNode) != null)
				{
					n = firstNode;
				}
				else
				{
					while (n != null && n != root && n == n.parent.content)
					{
						n = n.parent;
					}
					if (n == null || n == root)
					{
						break;
					}
					n = n.next;
				}
				yield return n;
			}
		}
	}

	private static IEnumerable<XElement> GetDescendants<T>(IEnumerable<T> source, XName name, bool self) where T : XContainer
	{
		foreach (T root in source)
		{
			if (root == null)
			{
				continue;
			}
			if (self)
			{
				XElement xElement = (XElement)(object)root;
				if (name == null || xElement.name == name)
				{
					yield return xElement;
				}
			}
			XNode n = root;
			XContainer xContainer = root;
			while (true)
			{
				if (xContainer != null && xContainer.content is XNode)
				{
					n = ((XNode)xContainer.content).next;
				}
				else
				{
					while (n != null && n != root && n == n.parent.content)
					{
						n = n.parent;
					}
					if (n == null || n == root)
					{
						break;
					}
					n = n.next;
				}
				XElement e = n as XElement;
				if (e != null && (name == null || e.name == name))
				{
					yield return e;
				}
				xContainer = e;
			}
		}
	}

	private static IEnumerable<XElement> GetElements<T>(IEnumerable<T> source, XName name) where T : XContainer
	{
		foreach (T root in source)
		{
			if (root == null)
			{
				continue;
			}
			XNode n = root.content as XNode;
			if (n == null)
			{
				continue;
			}
			do
			{
				n = n.next;
				if (n is XElement xElement && (name == null || xElement.name == name))
				{
					yield return xElement;
				}
			}
			while (n.parent == root && n != root.content);
		}
	}
}
