using System.Collections;

namespace System.CodeDom;

/// <summary>Represents a collection of <see cref="T:System.CodeDom.CodeTypeParameter" /> objects.</summary>
[Serializable]
public class CodeTypeParameterCollection : CollectionBase
{
	/// <summary>Gets or sets the <see cref="T:System.CodeDom.CodeTypeParameter" /> object at the specified index in the collection.</summary>
	/// <returns>The <see cref="T:System.CodeDom.CodeTypeParameter" /> object at the specified index.</returns>
	/// <param name="index">The zero-based index of the collection object to access.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is outside the valid range of indexes for the collection. </exception>
	public CodeTypeParameter this[int index]
	{
		get
		{
			return (CodeTypeParameter)base.List[index];
		}
		set
		{
			base.List[index] = value;
		}
	}

	/// <summary>Initializes a new, empty instance of the <see cref="T:System.CodeDom.CodeTypeParameterCollection" /> class. </summary>
	public CodeTypeParameterCollection()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeTypeParameterCollection" /> class containing the elements of the specified source collection.</summary>
	/// <param name="value">A <see cref="T:System.CodeDom.CodeTypeParameterCollection" /> with which to initialize the collection.</param>
	public CodeTypeParameterCollection(CodeTypeParameterCollection value)
	{
		AddRange(value);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeTypeParameterCollection" /> class containing the specified array of <see cref="T:System.CodeDom.CodeTypeParameter" /> objects. </summary>
	/// <param name="value">An array of <see cref="T:System.CodeDom.CodeTypeParameter" /> objects with which to initialize the collection.</param>
	public CodeTypeParameterCollection(CodeTypeParameter[] value)
	{
		AddRange(value);
	}

	/// <summary>Adds the specified <see cref="T:System.CodeDom.CodeTypeParameter" /> object to the collection.</summary>
	/// <returns>The zero-based index at which the new element was inserted.</returns>
	/// <param name="value">The <see cref="T:System.CodeDom.CodeTypeParameter" /> to add.</param>
	public int Add(CodeTypeParameter value)
	{
		return base.List.Add(value);
	}

	/// <summary>Adds the specified <see cref="T:System.CodeDom.CodeTypeParameter" /> object to the collection using the specified data type name.</summary>
	/// <param name="value">The name of a data type for which to add the <see cref="T:System.CodeDom.CodeTypeParameter" /> object to the collection.</param>
	public void Add(string value)
	{
		Add(new CodeTypeParameter(value));
	}

	/// <summary>Copies the elements of the specified <see cref="T:System.CodeDom.CodeTypeParameter" /> array to the end of the collection.</summary>
	/// <param name="value">An array of type <see cref="T:System.CodeDom.CodeTypeParameter" /> containing the objects to add to the collection.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null.</exception>
	public void AddRange(CodeTypeParameter[] value)
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

	/// <summary>Copies the elements of the specified <see cref="T:System.CodeDom.CodeTypeParameterCollection" /> to the end of the collection.</summary>
	/// <param name="value">A <see cref="T:System.CodeDom.CodeTypeParameterCollection" /> containing the <see cref="T:System.CodeDom.CodeTypeParameter" /> objects to add to the collection.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null.</exception>
	public void AddRange(CodeTypeParameterCollection value)
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

	/// <summary>Determines whether the collection contains the specified <see cref="T:System.CodeDom.CodeTypeParameter" /> object.</summary>
	/// <returns>true if the <see cref="T:System.CodeDom.CodeTypeParameter" /> object is contained in the collection; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.CodeDom.CodeTypeParameter" /> object to search for in the collection.</param>
	public bool Contains(CodeTypeParameter value)
	{
		return base.List.Contains(value);
	}

	/// <summary>Copies the items in the collection to the specified one-dimensional <see cref="T:System.Array" /> at the specified index.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the values copied from the collection. </param>
	/// <param name="index">The index of the array at which to begin inserting. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="array" /> is multidimensional.-or- The number of elements in the <see cref="T:System.CodeDom.CodeTypeParameterCollection" /> is greater than the available space between the index of the target array specified by <paramref name="index" /> and the end of the target array. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than the target array's lowest index. </exception>
	public void CopyTo(CodeTypeParameter[] array, int index)
	{
		base.List.CopyTo(array, index);
	}

	/// <summary>Gets the index in the collection of the specified <see cref="T:System.CodeDom.CodeTypeParameter" /> object, if it exists in the collection.</summary>
	/// <returns>The zero-based index of the specified <see cref="T:System.CodeDom.CodeTypeParameter" /> object in the collection if found; otherwise, -1.</returns>
	/// <param name="value">The <see cref="T:System.CodeDom.CodeTypeParameter" /> object to locate in the collection.</param>
	public int IndexOf(CodeTypeParameter value)
	{
		return base.List.IndexOf(value);
	}

	/// <summary>Inserts the specified <see cref="T:System.CodeDom.CodeTypeParameter" /> object into the collection at the specified index.</summary>
	/// <param name="index">The zero-based index at which to insert the item. </param>
	/// <param name="value">The <see cref="T:System.CodeDom.CodeTypeParameter" /> object to insert. </param>
	public void Insert(int index, CodeTypeParameter value)
	{
		base.List.Insert(index, value);
	}

	/// <summary>Removes the specified <see cref="T:System.CodeDom.CodeTypeParameter" /> object from the collection.</summary>
	/// <param name="value">The <see cref="T:System.CodeDom.CodeTypeParameter" /> object to remove from the collection.</param>
	/// <exception cref="T:System.ArgumentException">The specified object is not found in the collection. </exception>
	public void Remove(CodeTypeParameter value)
	{
		base.List.Remove(value);
	}
}
