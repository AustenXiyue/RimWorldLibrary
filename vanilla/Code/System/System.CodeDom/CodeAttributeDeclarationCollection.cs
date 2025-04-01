using System.Collections;

namespace System.CodeDom;

/// <summary>Represents a collection of <see cref="T:System.CodeDom.CodeAttributeDeclaration" /> objects.</summary>
[Serializable]
public class CodeAttributeDeclarationCollection : CollectionBase
{
	/// <summary>Gets or sets the <see cref="T:System.CodeDom.CodeAttributeDeclaration" /> object at the specified index.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeAttributeDeclaration" /> at each valid index.</returns>
	/// <param name="index">The index of the collection to access. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="index" /> parameter is outside the valid range of indexes for the collection. </exception>
	public CodeAttributeDeclaration this[int index]
	{
		get
		{
			return (CodeAttributeDeclaration)base.List[index];
		}
		set
		{
			base.List[index] = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeAttributeDeclarationCollection" /> class.</summary>
	public CodeAttributeDeclarationCollection()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeAttributeDeclarationCollection" /> class containing the elements of the specified source collection.</summary>
	/// <param name="value">A <see cref="T:System.CodeDom.CodeAttributeDeclarationCollection" /> with which to initialize the collection. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null.</exception>
	public CodeAttributeDeclarationCollection(CodeAttributeDeclarationCollection value)
	{
		AddRange(value);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeAttributeDeclarationCollection" /> class containing the specified array of <see cref="T:System.CodeDom.CodeAttributeDeclaration" /> objects.</summary>
	/// <param name="value">An array of <see cref="T:System.CodeDom.CodeAttributeDeclaration" /> objects with which to initialize the collection. </param>
	/// <exception cref="T:System.ArgumentNullException">One or more objects in the array are null.</exception>
	public CodeAttributeDeclarationCollection(CodeAttributeDeclaration[] value)
	{
		AddRange(value);
	}

	/// <summary>Adds a <see cref="T:System.CodeDom.CodeAttributeDeclaration" /> object with the specified value to the collection.</summary>
	/// <returns>The index at which the new element was inserted.</returns>
	/// <param name="value">The <see cref="T:System.CodeDom.CodeAttributeDeclaration" /> object to add. </param>
	public int Add(CodeAttributeDeclaration value)
	{
		return base.List.Add(value);
	}

	/// <summary>Copies the elements of the specified <see cref="T:System.CodeDom.CodeAttributeDeclaration" /> array to the end of the collection.</summary>
	/// <param name="value">An array of type <see cref="T:System.CodeDom.CodeAttributeDeclaration" /> that contains the objects to add to the collection. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null.</exception>
	public void AddRange(CodeAttributeDeclaration[] value)
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

	/// <summary>Copies the contents of another <see cref="T:System.CodeDom.CodeAttributeDeclarationCollection" /> object to the end of the collection.</summary>
	/// <param name="value">A <see cref="T:System.CodeDom.CodeAttributeDeclarationCollection" /> that contains the objects to add to the collection. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null.</exception>
	public void AddRange(CodeAttributeDeclarationCollection value)
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

	/// <summary>Gets or sets a value that indicates whether the collection contains the specified <see cref="T:System.CodeDom.CodeAttributeDeclaration" /> object.</summary>
	/// <returns>true if the collection contains the specified object; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.CodeDom.CodeAttributeDeclaration" /> object to locate. </param>
	public bool Contains(CodeAttributeDeclaration value)
	{
		return base.List.Contains(value);
	}

	/// <summary>Copies the collection objects to a one-dimensional <see cref="T:System.Array" /> instance beginning at the specified index.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the values copied from the collection. </param>
	/// <param name="index">The index of the array at which to begin inserting. </param>
	/// <exception cref="T:System.ArgumentException">The destination array is multidimensional.-or- The number of elements in the <see cref="T:System.CodeDom.CodeAttributeDeclarationCollection" /> is greater than the available space between the index of the target array specified by the <paramref name="index" /> parameter and the end of the target array. </exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="array" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="index" /> parameter is less than the target array's minimum index. </exception>
	public void CopyTo(CodeAttributeDeclaration[] array, int index)
	{
		base.List.CopyTo(array, index);
	}

	/// <summary>Gets the index of the specified <see cref="T:System.CodeDom.CodeAttributeDeclaration" /> object in the collection, if it exists in the collection.</summary>
	/// <returns>The index in the collection of the specified object, if found; otherwise, -1.</returns>
	/// <param name="value">The <see cref="T:System.CodeDom.CodeAttributeDeclaration" /> object to locate in the collection. </param>
	public int IndexOf(CodeAttributeDeclaration value)
	{
		return base.List.IndexOf(value);
	}

	/// <summary>Inserts the specified <see cref="T:System.CodeDom.CodeAttributeDeclaration" /> object into the collection at the specified index.</summary>
	/// <param name="index">The zero-based index where the specified object should be inserted. </param>
	/// <param name="value">The <see cref="T:System.CodeDom.CodeAttributeDeclaration" /> object to insert. </param>
	public void Insert(int index, CodeAttributeDeclaration value)
	{
		base.List.Insert(index, value);
	}

	/// <summary>Removes the specified <see cref="T:System.CodeDom.CodeAttributeDeclaration" /> object from the collection.</summary>
	/// <param name="value">The <see cref="T:System.CodeDom.CodeAttributeDeclaration" /> object to remove from the collection. </param>
	/// <exception cref="T:System.ArgumentException">The specified object is not found in the collection. </exception>
	public void Remove(CodeAttributeDeclaration value)
	{
		base.List.Remove(value);
	}
}
