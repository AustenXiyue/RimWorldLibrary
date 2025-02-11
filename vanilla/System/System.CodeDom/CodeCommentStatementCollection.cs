using System.Collections;

namespace System.CodeDom;

/// <summary>Represents a collection of <see cref="T:System.CodeDom.CodeCommentStatement" /> objects.</summary>
[Serializable]
public class CodeCommentStatementCollection : CollectionBase
{
	/// <summary>Gets or sets the <see cref="T:System.CodeDom.CodeCommentStatement" /> object at the specified index in the collection.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeCommentStatement" /> object at each valid index.</returns>
	/// <param name="index">The index of the collection to access. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="index" /> parameter is outside the valid range of indexes for the collection. </exception>
	public CodeCommentStatement this[int index]
	{
		get
		{
			return (CodeCommentStatement)base.List[index];
		}
		set
		{
			base.List[index] = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeCommentStatementCollection" /> class.</summary>
	public CodeCommentStatementCollection()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeCommentStatementCollection" /> class containing the elements of the specified source collection.</summary>
	/// <param name="value">A <see cref="T:System.CodeDom.CodeCommentStatementCollection" /> with which to initialize the collection. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null.</exception>
	public CodeCommentStatementCollection(CodeCommentStatementCollection value)
	{
		AddRange(value);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeCommentStatementCollection" /> class containing the specified array of <see cref="T:System.CodeDom.CodeCommentStatement" /> objects.</summary>
	/// <param name="value">An array of <see cref="T:System.CodeDom.CodeCommentStatement" /> objects with which to initialize the collection. </param>
	/// <exception cref="T:System.ArgumentNullException">One or more objects in the array are null.</exception>
	public CodeCommentStatementCollection(CodeCommentStatement[] value)
	{
		AddRange(value);
	}

	/// <summary>Adds the specified <see cref="T:System.CodeDom.CodeCommentStatement" /> object to the collection.</summary>
	/// <returns>The index at which the new element was inserted.</returns>
	/// <param name="value">The <see cref="T:System.CodeDom.CodeCommentStatement" /> object to add. </param>
	public int Add(CodeCommentStatement value)
	{
		return base.List.Add(value);
	}

	/// <summary>Copies the elements of the specified <see cref="T:System.CodeDom.CodeCommentStatement" /> array to the end of the collection.</summary>
	/// <param name="value">An array of type <see cref="T:System.CodeDom.CodeCommentStatement" /> that contains the objects to add to the collection. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null.</exception>
	public void AddRange(CodeCommentStatement[] value)
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

	/// <summary>Copies the contents of another <see cref="T:System.CodeDom.CodeCommentStatementCollection" /> object to the end of the collection.</summary>
	/// <param name="value">A <see cref="T:System.CodeDom.CodeCommentStatementCollection" /> that contains the objects to add to the collection. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null.</exception>
	public void AddRange(CodeCommentStatementCollection value)
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

	/// <summary>Gets a value that indicates whether the collection contains the specified <see cref="T:System.CodeDom.CodeCommentStatement" /> object.</summary>
	/// <returns>true if the collection contains the specified object; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.CodeDom.CodeCommentStatement" /> to search for in the collection. </param>
	public bool Contains(CodeCommentStatement value)
	{
		return base.List.Contains(value);
	}

	/// <summary>Copies the collection objects to the specified one-dimensional <see cref="T:System.Array" /> beginning at the specified index.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the values copied from the collection. </param>
	/// <param name="index">The index of the array at which to begin inserting. </param>
	/// <exception cref="T:System.ArgumentException">The destination array is multidimensional.-or- The number of elements in the <see cref="T:System.CodeDom.CodeCommentStatementCollection" /> is greater than the available space between the index of the target array specified by the <paramref name="index" /> parameter and the end of the target array. </exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="array" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="index" /> parameter is less than the target array's minimum index. </exception>
	public void CopyTo(CodeCommentStatement[] array, int index)
	{
		base.List.CopyTo(array, index);
	}

	/// <summary>Gets the index of the specified <see cref="T:System.CodeDom.CodeCommentStatement" /> object in the collection, if it exists in the collection.</summary>
	/// <returns>The index of the specified object, if found, in the collection; otherwise, -1.</returns>
	/// <param name="value">The <see cref="T:System.CodeDom.CodeCommentStatement" /> object to locate. </param>
	public int IndexOf(CodeCommentStatement value)
	{
		return base.List.IndexOf(value);
	}

	/// <summary>Inserts a <see cref="T:System.CodeDom.CodeCommentStatement" /> object into the collection at the specified index.</summary>
	/// <param name="index">The zero-based index where the item should be inserted. </param>
	/// <param name="value">The <see cref="T:System.CodeDom.CodeCommentStatement" /> object to insert. </param>
	public void Insert(int index, CodeCommentStatement value)
	{
		base.List.Insert(index, value);
	}

	/// <summary>Removes the specified <see cref="T:System.CodeDom.CodeCommentStatement" /> object from the collection.</summary>
	/// <param name="value">The <see cref="T:System.CodeDom.CodeCommentStatement" /> object to remove from the collection. </param>
	/// <exception cref="T:System.ArgumentException">The specified object is not found in the collection. </exception>
	public void Remove(CodeCommentStatement value)
	{
		base.List.Remove(value);
	}
}
