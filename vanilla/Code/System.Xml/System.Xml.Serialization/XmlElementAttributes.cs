using System.Collections;

namespace System.Xml.Serialization;

/// <summary>Represents a collection of <see cref="T:System.Xml.Serialization.XmlElementAttribute" /> objects used by the <see cref="T:System.Xml.Serialization.XmlSerializer" /> to override the default way it serializes a class.</summary>
public class XmlElementAttributes : CollectionBase
{
	/// <summary>Gets or sets the element at the specified index.</summary>
	/// <returns>The element at the specified index.</returns>
	/// <param name="index">The zero-based index of the element to get or set. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is not a valid index in the <see cref="T:System.Xml.Serialization.XmlElementAttributes" />. </exception>
	/// <exception cref="T:System.NotSupportedException">The property is set and the <see cref="T:System.Xml.Serialization.XmlElementAttributes" /> is read-only. </exception>
	public XmlElementAttribute this[int index]
	{
		get
		{
			return (XmlElementAttribute)base.List[index];
		}
		set
		{
			base.List[index] = value;
		}
	}

	/// <summary>Adds an <see cref="T:System.Xml.Serialization.XmlElementAttribute" /> to the collection.</summary>
	/// <returns>The zero-based index of the newly added item.</returns>
	/// <param name="attribute">The <see cref="T:System.Xml.Serialization.XmlElementAttribute" /> to add. </param>
	public int Add(XmlElementAttribute attribute)
	{
		return base.List.Add(attribute);
	}

	/// <summary>Inserts an <see cref="T:System.Xml.Serialization.XmlElementAttribute" /> into the collection.</summary>
	/// <param name="index">The zero-based index where the member is inserted. </param>
	/// <param name="attribute">The <see cref="T:System.Xml.Serialization.XmlElementAttribute" /> to insert. </param>
	public void Insert(int index, XmlElementAttribute attribute)
	{
		base.List.Insert(index, attribute);
	}

	/// <summary>Gets the index of the specified <see cref="T:System.Xml.Serialization.XmlElementAttribute" />.</summary>
	/// <returns>The zero-based index of the <see cref="T:System.Xml.Serialization.XmlElementAttribute" />.</returns>
	/// <param name="attribute">The <see cref="T:System.Xml.Serialization.XmlElementAttribute" /> whose index is being retrieved.</param>
	public int IndexOf(XmlElementAttribute attribute)
	{
		return base.List.IndexOf(attribute);
	}

	/// <summary>Determines whether the collection contains the specified object.</summary>
	/// <returns>true if the object exists in the collection; otherwise, false.</returns>
	/// <param name="attribute">The <see cref="T:System.Xml.Serialization.XmlElementAttribute" /> to look for. </param>
	public bool Contains(XmlElementAttribute attribute)
	{
		return base.List.Contains(attribute);
	}

	/// <summary>Removes the specified object from the collection.</summary>
	/// <param name="attribute">The <see cref="T:System.Xml.Serialization.XmlElementAttribute" /> to remove from the collection. </param>
	public void Remove(XmlElementAttribute attribute)
	{
		base.List.Remove(attribute);
	}

	/// <summary>Copies the <see cref="T:System.Xml.Serialization.XmlElementAttributes" />, or a portion of it to a one-dimensional array.</summary>
	/// <param name="array">The <see cref="T:System.Xml.Serialization.XmlElementAttribute" /> array to hold the copied elements. </param>
	/// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins. </param>
	public void CopyTo(XmlElementAttribute[] array, int index)
	{
		base.List.CopyTo(array, index);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Serialization.XmlElementAttributes" /> class. </summary>
	public XmlElementAttributes()
	{
	}
}
