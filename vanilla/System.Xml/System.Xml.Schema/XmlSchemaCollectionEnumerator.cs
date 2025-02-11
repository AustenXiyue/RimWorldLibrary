using System.Collections;
using Unity;

namespace System.Xml.Schema;

/// <summary>Supports a simple iteration over a collection. This class cannot be inherited. </summary>
public sealed class XmlSchemaCollectionEnumerator : IEnumerator
{
	private IDictionaryEnumerator enumerator;

	/// <summary>For a description of this member, see <see cref="P:System.Xml.Schema.XmlSchemaCollectionEnumerator.Current" />.</summary>
	/// <returns>Returns the current node.</returns>
	object IEnumerator.Current => Current;

	/// <summary>Gets the current <see cref="T:System.Xml.Schema.XmlSchema" /> in the collection.</summary>
	/// <returns>The current XmlSchema in the collection.</returns>
	public XmlSchema Current => ((XmlSchemaCollectionNode)enumerator.Value)?.Schema;

	internal XmlSchemaCollectionNode CurrentNode => (XmlSchemaCollectionNode)enumerator.Value;

	internal XmlSchemaCollectionEnumerator(Hashtable collection)
	{
		enumerator = collection.GetEnumerator();
	}

	/// <summary>For a description of this member, see <see cref="M:System.Xml.Schema.XmlSchemaCollectionEnumerator.System.Collections.IEnumerator.Reset" />.</summary>
	void IEnumerator.Reset()
	{
		enumerator.Reset();
	}

	/// <summary>For a description of this member, see <see cref="M:System.Xml.Schema.XmlSchemaCollectionEnumerator.MoveNext" />.</summary>
	/// <returns>Returns the next node.</returns>
	bool IEnumerator.MoveNext()
	{
		return enumerator.MoveNext();
	}

	/// <summary>Advances the enumerator to the next schema in the collection.</summary>
	/// <returns>true if the move was successful; false if the enumerator has passed the end of the collection.</returns>
	public bool MoveNext()
	{
		return enumerator.MoveNext();
	}

	internal XmlSchemaCollectionEnumerator()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
