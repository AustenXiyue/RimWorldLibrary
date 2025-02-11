using System.Collections;

namespace System.CodeDom;

/// <summary>Represents a collection of <see cref="T:System.CodeDom.CodeNamespace" /> objects.</summary>
[Serializable]
public class CodeNamespaceCollection : CollectionBase
{
	/// <summary>Gets or sets the <see cref="T:System.CodeDom.CodeNamespaceCollection" /> object at the specified index in the collection.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeNamespace" /> at each valid index.</returns>
	/// <param name="index">The index of the collection to access. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="index" /> parameter is outside the valid range of indexes for the collection. </exception>
	public CodeNamespace this[int index]
	{
		get
		{
			return (CodeNamespace)base.List[index];
		}
		set
		{
			base.List[index] = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeNamespaceCollection" /> class.</summary>
	public CodeNamespaceCollection()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeNamespaceCollection" /> class that contains the elements of the specified source collection.</summary>
	/// <param name="value">The <see cref="T:System.CodeDom.CodeNamespaceCollection" /> with which to initialize the collection. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null.</exception>
	public CodeNamespaceCollection(CodeNamespaceCollection value)
	{
		AddRange(value);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeNamespaceCollection" /> class that contains the specified array of <see cref="T:System.CodeDom.CodeNamespace" /> objects.</summary>
	/// <param name="value">An array of <see cref="T:System.CodeDom.CodeNamespace" /> objects with which to initialize the collection. </param>
	/// <exception cref="T:System.ArgumentNullException">One or more objects in the array are null.</exception>
	public CodeNamespaceCollection(CodeNamespace[] value)
	{
		AddRange(value);
	}

	/// <summary>Adds the specified <see cref="T:System.CodeDom.CodeNamespace" /> object to the collection.</summary>
	/// <returns>The index at which the new element was inserted.</returns>
	/// <param name="value">The <see cref="T:System.CodeDom.CodeNamespace" /> to add. </param>
	public int Add(CodeNamespace value)
	{
		return base.List.Add(value);
	}

	/// <summary>Copies the elements of the specified <see cref="T:System.CodeDom.CodeNamespace" /> array to the end of the collection.</summary>
	/// <param name="value">An array of type <see cref="T:System.CodeDom.CodeNamespace" /> that contains the objects to add to the collection. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null.</exception>
	public void AddRange(CodeNamespace[] value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		for (int i = 0; i < value.Length; i++)
		{
			Add(value[i]);
		}
	}

	/// <summary>Adds the contents of the specified <see cref="T:System.CodeDom.CodeNamespaceCollection" /> object to the end of the collection.</summary>
	/// <param name="value">A <see cref="T:System.CodeDom.CodeNamespaceCollection" /> that contains the objects to add to the collection. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null.</exception>
	public void AddRange(CodeNamespaceCollection value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		int count = value.Count;
		for (int i = 0; i < count; i++)
		{
			Add(value[i]);
		}
	}

	/// <summary>Gets a value that indicates whether the collection contains the specified <see cref="T:System.CodeDom.CodeNamespace" /> object.</summary>
	/// <returns>true if the <see cref="T:System.CodeDom.CodeNamespace" /> is contained in the collection; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.CodeDom.CodeNamespace" /> to search for in the collection. </param>
	public bool Contains(CodeNamespace value)
	{
		return base.List.Contains(value);
	}

	/// <summary>Copies the collection objects to a one-dimensional <see cref="T:System.Array" /> instance, starting at the specified index.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the values copied from the collection. </param>
	/// <param name="index">The index of the array at which to begin inserting. </param>
	/// <exception cref="T:System.ArgumentException">The destination array is multidimensional.-or- The number of elements in the <see cref="T:System.CodeDom.CodeNamespaceCollection" /> is greater than the available space between the index of the target array specified by the <paramref name="index" /> parameter and the end of the target array. </exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="array" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="index" /> parameter is less than the target array's minimum index. </exception>
	public void CopyTo(CodeNamespace[] array, int index)
	{
		base.List.CopyTo(array, index);
	}

	/// <summary>Gets the index of the specified <see cref="T:System.CodeDom.CodeNamespace" /> object in the <see cref="T:System.CodeDom.CodeNamespaceCollection" />, if it exists in the collection.</summary>
	/// <returns>The index of the specified <see cref="T:System.CodeDom.CodeNamespace" />, if it is found, in the collection; otherwise, -1.</returns>
	/// <param name="value">The <see cref="T:System.CodeDom.CodeNamespace" /> to locate. </param>
	public int IndexOf(CodeNamespace value)
	{
		return base.List.IndexOf(value);
	}

	/// <summary>Inserts the specified <see cref="T:System.CodeDom.CodeNamespace" /> object into the collection at the specified index.</summary>
	/// <param name="index">The zero-based index where the new item should be inserted. </param>
	/// <param name="value">The <see cref="T:System.CodeDom.CodeNamespace" /> to insert. </param>
	public void Insert(int index, CodeNamespace value)
	{
		base.List.Insert(index, value);
	}

	/// <summary>Removes the specified <see cref="T:System.CodeDom.CodeNamespace" /> object from the collection.</summary>
	/// <param name="value">The <see cref="T:System.CodeDom.CodeNamespace" /> to remove from the collection. </param>
	/// <exception cref="T:System.ArgumentException">The specified object is not found in the collection. </exception>
	public void Remove(CodeNamespace value)
	{
		base.List.Remove(value);
	}
}
