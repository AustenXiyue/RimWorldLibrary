using System.Collections;

namespace System.Windows.Documents;

/// <summary>Provides a collection of all of the <see cref="T:System.Windows.Documents.LinkTarget" /> elements in a <see cref="T:System.IO.Packaging.Package" />.</summary>
public sealed class LinkTargetCollection : CollectionBase
{
	/// <summary>Gets or sets the <see cref="T:System.Windows.Documents.LinkTarget" /> at the specified index.</summary>
	/// <returns>The <see cref="T:System.Windows.Documents.LinkTarget" /> at the specified index.</returns>
	/// <param name="index">The index of the target being written or read.</param>
	public LinkTarget this[int index]
	{
		get
		{
			return (LinkTarget)((IList)this)[index];
		}
		set
		{
			((IList)this)[index] = value;
		}
	}

	/// <summary>Adds a specified <see cref="T:System.Windows.Documents.LinkTarget" /> to the collection.</summary>
	/// <returns>The zero-based index in the collection of the <paramref name="value" /> added.</returns>
	/// <param name="value">The link target that is added.</param>
	public int Add(LinkTarget value)
	{
		return ((IList)this).Add((object?)value);
	}

	/// <summary>Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
	/// <param name="value">The link target to remove.</param>
	public void Remove(LinkTarget value)
	{
		((IList)this).Remove((object?)value);
	}

	/// <summary>Specifies a value that indicates whether a particular <see cref="T:System.Windows.Documents.LinkTarget" /> is in the collection.</summary>
	/// <returns>true if <paramref name="value" /> is present; otherwise, false.</returns>
	/// <param name="value">The link to test for.</param>
	public bool Contains(LinkTarget value)
	{
		return ((IList)this).Contains((object?)value);
	}

	/// <summary>Copies the items in the collection to the specified array beginning at the specified index.</summary>
	/// <param name="array">The target array.</param>
	/// <param name="index">The zero-based index of the array position where the first item is copied. </param>
	public void CopyTo(LinkTarget[] array, int index)
	{
		((ICollection)this).CopyTo((Array)array, index);
	}

	/// <summary>Gets the index of the specified item.</summary>
	/// <returns>The index of <paramref name="value" /> if found in the collection; otherwise, -1.</returns>
	/// <param name="value">The object to locate in the collection.</param>
	public int IndexOf(LinkTarget value)
	{
		return ((IList)this).IndexOf((object?)value);
	}

	/// <summary>Inserts the specified item into the collection at the specified index.</summary>
	/// <param name="index">The point where the link target is inserted.</param>
	/// <param name="value">The target to insert.</param>
	public void Insert(int index, LinkTarget value)
	{
		((IList)this).Insert(index, (object?)value);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.LinkTargetCollection" /> class. </summary>
	public LinkTargetCollection()
	{
	}
}
