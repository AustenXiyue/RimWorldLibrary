using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;

namespace System.Xml.Serialization;

/// <summary>Enables iteration over a collection of <see cref="T:System.Xml.Schema.XmlSchema" /> objects. </summary>
public class XmlSchemaEnumerator : IEnumerator<XmlSchema>, IDisposable, IEnumerator
{
	private XmlSchemas list;

	private int idx;

	private int end;

	/// <summary>Gets the current element in the collection.</summary>
	/// <returns>The current <see cref="T:System.Xml.Schema.XmlSchema" /> object in the collection.</returns>
	public XmlSchema Current => list[idx];

	/// <summary>Gets the current element in the collection of <see cref="T:System.Xml.Schema.XmlSchema" /> objects.</summary>
	/// <returns>The current element in the collection of <see cref="T:System.Xml.Schema.XmlSchema" /> objects.</returns>
	object IEnumerator.Current => list[idx];

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Serialization.XmlSchemaEnumerator" /> class. </summary>
	/// <param name="list">The <see cref="T:System.Xml.Serialization.XmlSchemas" /> object you want to iterate over.</param>
	public XmlSchemaEnumerator(XmlSchemas list)
	{
		this.list = list;
		idx = -1;
		end = list.Count - 1;
	}

	/// <summary>Releases all resources used by the <see cref="T:System.Xml.Serialization.XmlSchemaEnumerator" />.</summary>
	public void Dispose()
	{
	}

	/// <summary>Advances the enumerator to the next item in the collection.</summary>
	/// <returns>true if the move is successful; otherwise, false.</returns>
	public bool MoveNext()
	{
		if (idx >= end)
		{
			return false;
		}
		idx++;
		return true;
	}

	/// <summary>Sets the enumerator to its initial position, which is before the first element in the collection of <see cref="T:System.Xml.Schema.XmlSchema" /> objects.</summary>
	void IEnumerator.Reset()
	{
		idx = -1;
	}
}
