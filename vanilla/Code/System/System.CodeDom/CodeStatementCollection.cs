using System.Collections;

namespace System.CodeDom;

/// <summary>Represents a collection of <see cref="T:System.CodeDom.CodeStatement" /> objects.</summary>
[Serializable]
public class CodeStatementCollection : CollectionBase
{
	/// <summary>Gets or sets the <see cref="T:System.CodeDom.CodeStatement" /> object at the specified index in the collection.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeStatement" /> at each valid index.</returns>
	/// <param name="index">The index of the collection to access. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="index" /> parameter is outside the valid range of indexes for the collection. </exception>
	public CodeStatement this[int index]
	{
		get
		{
			return (CodeStatement)base.List[index];
		}
		set
		{
			base.List[index] = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeStatementCollection" /> class.</summary>
	public CodeStatementCollection()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeStatementCollection" /> class that contains the elements of the specified source collection.</summary>
	/// <param name="value">A <see cref="T:System.CodeDom.CodeStatementCollection" /> object with which to initialize the collection. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null.</exception>
	public CodeStatementCollection(CodeStatementCollection value)
	{
		AddRange(value);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeStatementCollection" /> class that contains the specified array of <see cref="T:System.CodeDom.CodeStatement" /> objects.</summary>
	/// <param name="value">An array of <see cref="T:System.CodeDom.CodeStatement" /> objects with which to initialize the collection. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null.</exception>
	public CodeStatementCollection(CodeStatement[] value)
	{
		AddRange(value);
	}

	/// <summary>Adds the specified <see cref="T:System.CodeDom.CodeStatement" /> object to the collection.</summary>
	/// <returns>The index at which the new element was inserted.</returns>
	/// <param name="value">The <see cref="T:System.CodeDom.CodeStatement" /> object to add. </param>
	public int Add(CodeStatement value)
	{
		return base.List.Add(value);
	}

	/// <summary>Adds the specified <see cref="T:System.CodeDom.CodeExpression" /> object to the collection.</summary>
	/// <returns>The index at which the new element was inserted.</returns>
	/// <param name="value">The <see cref="T:System.CodeDom.CodeExpression" /> object to add. </param>
	public int Add(CodeExpression value)
	{
		return Add(new CodeExpressionStatement(value));
	}

	/// <summary>Adds a set of <see cref="T:System.CodeDom.CodeStatement" /> objects to the collection.</summary>
	/// <param name="value">An array of <see cref="T:System.CodeDom.CodeStatement" /> objects to add to the collection. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null.</exception>
	public void AddRange(CodeStatement[] value)
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

	/// <summary>Adds the contents of another <see cref="T:System.CodeDom.CodeStatementCollection" /> object to the end of the collection.</summary>
	/// <param name="value">A <see cref="T:System.CodeDom.CodeStatementCollection" /> object that contains the objects to add to the collection. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null.</exception>
	public void AddRange(CodeStatementCollection value)
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

	/// <summary>Gets a value that indicates whether the collection contains the specified <see cref="T:System.CodeDom.CodeStatement" /> object.</summary>
	/// <returns>true if the collection contains the specified object; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.CodeDom.CodeStatement" /> object to search for in the collection. </param>
	public bool Contains(CodeStatement value)
	{
		return base.List.Contains(value);
	}

	/// <summary>Copies the elements of the <see cref="T:System.CodeDom.CodeStatementCollection" /> object to a one-dimensional <see cref="T:System.Array" /> instance, starting at the specified index.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the values copied from the collection. </param>
	/// <param name="index">The index of the array at which to begin inserting. </param>
	/// <exception cref="T:System.ArgumentException">The destination array is multidimensional.-or- The number of elements in the <see cref="T:System.CodeDom.CodeStatementCollection" /> is greater than the available space between the index of the target array specified by the <paramref name="index" /> parameter and the end of the target array. </exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="array" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="index" /> parameter is less than the target array's minimum index. </exception>
	public void CopyTo(CodeStatement[] array, int index)
	{
		base.List.CopyTo(array, index);
	}

	/// <summary>Gets the index of the specified <see cref="T:System.CodeDom.CodeStatement" /> object in the <see cref="T:System.CodeDom.CodeStatementCollection" />, if it exists in the collection.</summary>
	/// <returns>The index of the specified object, if it is found, in the collection; otherwise, -1.</returns>
	/// <param name="value">The <see cref="T:System.CodeDom.CodeStatement" /> to locate in the collection. </param>
	public int IndexOf(CodeStatement value)
	{
		return base.List.IndexOf(value);
	}

	/// <summary>Inserts the specified <see cref="T:System.CodeDom.CodeStatement" /> object into the collection at the specified index.</summary>
	/// <param name="index">The zero-based index where the specified object should be inserted. </param>
	/// <param name="value">The <see cref="T:System.CodeDom.CodeStatement" /> object to insert. </param>
	public void Insert(int index, CodeStatement value)
	{
		base.List.Insert(index, value);
	}

	/// <summary>Removes the specified <see cref="T:System.CodeDom.CodeStatement" /> object from the collection.</summary>
	/// <param name="value">The <see cref="T:System.CodeDom.CodeStatement" /> to remove from the collection. </param>
	/// <exception cref="T:System.ArgumentException">The specified object is not found in the collection. </exception>
	public void Remove(CodeStatement value)
	{
		base.List.Remove(value);
	}
}
