using System.Collections;
using System.Runtime.CompilerServices;

namespace System.Xml;

/// <summary>Represents an ordered collection of nodes.</summary>
public abstract class XmlNodeList : IEnumerable, IDisposable
{
	/// <summary>Gets the number of nodes in the XmlNodeList.</summary>
	/// <returns>The number of nodes in the XmlNodeList.</returns>
	public abstract int Count { get; }

	/// <summary>Gets a node at the given index.</summary>
	/// <returns>The <see cref="T:System.Xml.XmlNode" /> with the specified index in the collection. If index is greater than or equal to the number of nodes in the list, this returns null.</returns>
	/// <param name="i">The zero-based index into the list of nodes.</param>
	[IndexerName("ItemOf")]
	public virtual XmlNode this[int i] => Item(i);

	/// <summary>Retrieves a node at the given index.</summary>
	/// <returns>The <see cref="T:System.Xml.XmlNode" /> with the specified index in the collection. If <paramref name="index" /> is greater than or equal to the number of nodes in the list, this returns null.</returns>
	/// <param name="index">The zero-based index into the list of nodes.</param>
	public abstract XmlNode Item(int index);

	/// <summary>Gets an enumerator that iterates through the collection of nodes.</summary>
	/// <returns>An enumerator used to iterate through the collection of nodes.</returns>
	public abstract IEnumerator GetEnumerator();

	/// <summary>Releases all resources used by the <see cref="T:System.Xml.XmlNodeList" /> class.</summary>
	void IDisposable.Dispose()
	{
		PrivateDisposeNodeList();
	}

	/// <summary>Disposes resources in the node list privately.</summary>
	protected virtual void PrivateDisposeNodeList()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.XmlNodeList" /> class.</summary>
	protected XmlNodeList()
	{
	}
}
