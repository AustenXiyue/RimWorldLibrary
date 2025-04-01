using System.Collections;

namespace System.CodeDom;

/// <summary>Represents a collection of <see cref="T:System.CodeDom.CodeDirective" /> objects.</summary>
[Serializable]
public class CodeDirectiveCollection : CollectionBase
{
	/// <summary>Gets or sets the <see cref="T:System.CodeDom.CodeDirective" /> object at the specified index in the collection.</summary>
	/// <returns>The <see cref="T:System.CodeDom.CodeDirective" /> at the index position.</returns>
	/// <param name="index">The index position to access.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is outside the valid range of index positions for the collection. </exception>
	public CodeDirective this[int index]
	{
		get
		{
			return (CodeDirective)base.List[index];
		}
		set
		{
			base.List[index] = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeDirectiveCollection" /> class. </summary>
	public CodeDirectiveCollection()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeDirectiveCollection" /> class with the elements in the specified code directive collection.</summary>
	/// <param name="value">A <see cref="T:System.CodeDom.CodeDirectiveCollection" /> object with which to initialize the collection.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null.</exception>
	public CodeDirectiveCollection(CodeDirectiveCollection value)
	{
		AddRange(value);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeDirectiveCollection" /> class with the code directive objects in the specified array. </summary>
	/// <param name="value">An array of <see cref="T:System.CodeDom.CodeDirective" /> objects with which to initialize the collection.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null.</exception>
	public CodeDirectiveCollection(CodeDirective[] value)
	{
		AddRange(value);
	}

	/// <summary>Adds the specified <see cref="T:System.CodeDom.CodeDirective" /> object to the collection.</summary>
	/// <returns>The index position at which the new element was inserted.</returns>
	/// <param name="value">The <see cref="T:System.CodeDom.CodeDirective" /> object to add.</param>
	public int Add(CodeDirective value)
	{
		return base.List.Add(value);
	}

	/// <summary>Adds an array of <see cref="T:System.CodeDom.CodeDirective" /> objects to the end of the collection.</summary>
	/// <param name="value">An array of <see cref="T:System.CodeDom.CodeDirective" /> objects to add to the collection.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null.</exception>
	public void AddRange(CodeDirective[] value)
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

	/// <summary>Adds the contents of the specified <see cref="T:System.CodeDom.CodeDirectiveCollection" /> object to the end of the collection.</summary>
	/// <param name="value">A <see cref="T:System.CodeDom.CodeDirectiveCollection" /> object containing the <see cref="T:System.CodeDom.CodeDirective" /> objects to add to the collection.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null.</exception>
	public void AddRange(CodeDirectiveCollection value)
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

	/// <summary>Gets a value indicating whether the collection contains the specified <see cref="T:System.CodeDom.CodeDirective" /> object.</summary>
	/// <returns>true if the collection contains the specified object; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.CodeDom.CodeDirective" /> object to search for in the collection.</param>
	public bool Contains(CodeDirective value)
	{
		return base.List.Contains(value);
	}

	/// <summary>Copies the contents of the collection to a one-dimensional array beginning at the specified index.</summary>
	/// <param name="array">An array of type <see cref="T:System.CodeDom.CodeDirective" /> that is the destination of the values copied from the collection. </param>
	/// <param name="index">The index in the array at which to begin inserting collection objects. </param>
	/// <exception cref="T:System.ArgumentException">The destination array is multidimensional.-or- The number of elements in the <see cref="T:System.CodeDom.CodeDirectiveCollection" /> is greater than the available space between the index of the target array specified by <paramref name="index" /> and the end of the target array. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than the target array's minimum index. </exception>
	public void CopyTo(CodeDirective[] array, int index)
	{
		base.List.CopyTo(array, index);
	}

	/// <summary>Gets the index in the collection of the specified <see cref="T:System.CodeDom.CodeDirective" /> object, if it exists in the collection.</summary>
	/// <returns>The index position in the collection of the specified object, if found; otherwise, -1.</returns>
	/// <param name="value">The <see cref="T:System.CodeDom.CodeDirective" /> object to locate in the collection.</param>
	public int IndexOf(CodeDirective value)
	{
		return base.List.IndexOf(value);
	}

	/// <summary>Inserts the specified <see cref="T:System.CodeDom.CodeDirective" /> object into the collection at the specified index.</summary>
	/// <param name="index">The zero-based index position where the specified object should be inserted. </param>
	/// <param name="value">The <see cref="T:System.CodeDom.CodeDirective" /> object to insert. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than 0. -or-<paramref name="index" /> is greater than <see cref="P:System.Collections.CollectionBase.Count" />.</exception>
	public void Insert(int index, CodeDirective value)
	{
		base.List.Insert(index, value);
	}

	/// <summary>Removes the specified <see cref="T:System.CodeDom.CodeDirective" /> object from the collection.</summary>
	/// <param name="value">The <see cref="T:System.CodeDom.CodeDirective" /> object to remove from the collection.</param>
	/// <exception cref="T:System.ArgumentException">The specified object is not found in the collection. </exception>
	public void Remove(CodeDirective value)
	{
		base.List.Remove(value);
	}
}
