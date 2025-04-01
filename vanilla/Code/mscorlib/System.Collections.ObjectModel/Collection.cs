using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace System.Collections.ObjectModel;

/// <summary>Provides the base class for a generic collection.</summary>
/// <typeparam name="T">The type of elements in the collection.</typeparam>
[Serializable]
[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(Mscorlib_CollectionDebugView<>))]
[ComVisible(false)]
public class Collection<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IList, ICollection, IReadOnlyList<T>, IReadOnlyCollection<T>
{
	private IList<T> items;

	[NonSerialized]
	private object _syncRoot;

	/// <summary>Gets the number of elements actually contained in the <see cref="T:System.Collections.ObjectModel.Collection`1" />.</summary>
	/// <returns>The number of elements actually contained in the <see cref="T:System.Collections.ObjectModel.Collection`1" />.</returns>
	public int Count => items.Count;

	/// <summary>Gets a <see cref="T:System.Collections.Generic.IList`1" /> wrapper around the <see cref="T:System.Collections.ObjectModel.Collection`1" />.</summary>
	/// <returns>A <see cref="T:System.Collections.Generic.IList`1" /> wrapper around the <see cref="T:System.Collections.ObjectModel.Collection`1" />.</returns>
	protected IList<T> Items => items;

	/// <summary>Gets or sets the element at the specified index.</summary>
	/// <returns>The element at the specified index.</returns>
	/// <param name="index">The zero-based index of the element to get or set.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.-or-<paramref name="index" /> is equal to or greater than <see cref="P:System.Collections.ObjectModel.Collection`1.Count" />. </exception>
	public T this[int index]
	{
		get
		{
			return items[index];
		}
		set
		{
			if (items.IsReadOnly)
			{
				ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
			}
			if (index < 0 || index >= items.Count)
			{
				ThrowHelper.ThrowArgumentOutOfRangeException();
			}
			SetItem(index, value);
		}
	}

	/// <summary>Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</summary>
	/// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.  In the default implementation of <see cref="T:System.Collections.ObjectModel.Collection`1" />, this property always returns false.</returns>
	bool ICollection<T>.IsReadOnly => items.IsReadOnly;

	/// <summary>Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe).</summary>
	/// <returns>true if access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe); otherwise, false.  In the default implementation of <see cref="T:System.Collections.ObjectModel.Collection`1" />, this property always returns false.</returns>
	bool ICollection.IsSynchronized => false;

	/// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.</summary>
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.  In the default implementation of <see cref="T:System.Collections.ObjectModel.Collection`1" />, this property always returns the current instance.</returns>
	object ICollection.SyncRoot
	{
		get
		{
			if (_syncRoot == null)
			{
				if (items is ICollection collection)
				{
					_syncRoot = collection.SyncRoot;
				}
				else
				{
					Interlocked.CompareExchange<object>(ref _syncRoot, new object(), (object)null);
				}
			}
			return _syncRoot;
		}
	}

	/// <summary>Gets or sets the element at the specified index.</summary>
	/// <returns>The element at the specified index.</returns>
	/// <param name="index">The zero-based index of the element to get or set.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is not a valid index in the <see cref="T:System.Collections.IList" />.</exception>
	/// <exception cref="T:System.ArgumentException">The property is set and <paramref name="value" /> is of a type that is not assignable to the <see cref="T:System.Collections.IList" />.</exception>
	object IList.this[int index]
	{
		get
		{
			return items[index];
		}
		set
		{
			ThrowHelper.IfNullAndNullsAreIllegalThenThrow<T>(value, ExceptionArgument.value);
			try
			{
				this[index] = (T)value;
			}
			catch (InvalidCastException)
			{
				ThrowHelper.ThrowWrongValueTypeArgumentException(value, typeof(T));
			}
		}
	}

	/// <summary>Gets a value indicating whether the <see cref="T:System.Collections.IList" /> is read-only.</summary>
	/// <returns>true if the <see cref="T:System.Collections.IList" /> is read-only; otherwise, false.  In the default implementation of <see cref="T:System.Collections.ObjectModel.Collection`1" />, this property always returns false.</returns>
	bool IList.IsReadOnly => items.IsReadOnly;

	/// <summary>Gets a value indicating whether the <see cref="T:System.Collections.IList" /> has a fixed size.</summary>
	/// <returns>true if the <see cref="T:System.Collections.IList" /> has a fixed size; otherwise, false.  In the default implementation of <see cref="T:System.Collections.ObjectModel.Collection`1" />, this property always returns false.</returns>
	bool IList.IsFixedSize
	{
		get
		{
			if (items is IList list)
			{
				return list.IsFixedSize;
			}
			return items.IsReadOnly;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.ObjectModel.Collection`1" /> class that is empty.</summary>
	public Collection()
	{
		items = new List<T>();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.ObjectModel.Collection`1" /> class as a wrapper for the specified list.</summary>
	/// <param name="list">The list that is wrapped by the new collection.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="list" /> is null.</exception>
	public Collection(IList<T> list)
	{
		if (list == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.list);
		}
		items = list;
	}

	/// <summary>Adds an object to the end of the <see cref="T:System.Collections.ObjectModel.Collection`1" />. </summary>
	/// <param name="item">The object to be added to the end of the <see cref="T:System.Collections.ObjectModel.Collection`1" />. The value can be null for reference types.</param>
	public void Add(T item)
	{
		if (items.IsReadOnly)
		{
			ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
		}
		int count = items.Count;
		InsertItem(count, item);
	}

	/// <summary>Removes all elements from the <see cref="T:System.Collections.ObjectModel.Collection`1" />.</summary>
	public void Clear()
	{
		if (items.IsReadOnly)
		{
			ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
		}
		ClearItems();
	}

	/// <summary>Copies the entire <see cref="T:System.Collections.ObjectModel.Collection`1" /> to a compatible one-dimensional <see cref="T:System.Array" />, starting at the specified index of the target array.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.ObjectModel.Collection`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
	/// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.</exception>
	/// <exception cref="T:System.ArgumentException">The number of elements in the source <see cref="T:System.Collections.ObjectModel.Collection`1" /> is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />.</exception>
	public void CopyTo(T[] array, int index)
	{
		items.CopyTo(array, index);
	}

	/// <summary>Determines whether an element is in the <see cref="T:System.Collections.ObjectModel.Collection`1" />.</summary>
	/// <returns>true if <paramref name="item" /> is found in the <see cref="T:System.Collections.ObjectModel.Collection`1" />; otherwise, false.</returns>
	/// <param name="item">The object to locate in the <see cref="T:System.Collections.ObjectModel.Collection`1" />. The value can be null for reference types.</param>
	public bool Contains(T item)
	{
		return items.Contains(item);
	}

	/// <summary>Returns an enumerator that iterates through the <see cref="T:System.Collections.ObjectModel.Collection`1" />.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerator`1" /> for the <see cref="T:System.Collections.ObjectModel.Collection`1" />.</returns>
	public IEnumerator<T> GetEnumerator()
	{
		return items.GetEnumerator();
	}

	/// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within the entire <see cref="T:System.Collections.ObjectModel.Collection`1" />.</summary>
	/// <returns>The zero-based index of the first occurrence of <paramref name="item" /> within the entire <see cref="T:System.Collections.ObjectModel.Collection`1" />, if found; otherwise, -1.</returns>
	/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.List`1" />. The value can be null for reference types.</param>
	public int IndexOf(T item)
	{
		return items.IndexOf(item);
	}

	/// <summary>Inserts an element into the <see cref="T:System.Collections.ObjectModel.Collection`1" /> at the specified index.</summary>
	/// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
	/// <param name="item">The object to insert. The value can be null for reference types.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.-or-<paramref name="index" /> is greater than <see cref="P:System.Collections.ObjectModel.Collection`1.Count" />.</exception>
	public void Insert(int index, T item)
	{
		if (items.IsReadOnly)
		{
			ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
		}
		if (index < 0 || index > items.Count)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_ListInsert);
		}
		InsertItem(index, item);
	}

	/// <summary>Removes the first occurrence of a specific object from the <see cref="T:System.Collections.ObjectModel.Collection`1" />.</summary>
	/// <returns>true if <paramref name="item" /> is successfully removed; otherwise, false.  This method also returns false if <paramref name="item" /> was not found in the original <see cref="T:System.Collections.ObjectModel.Collection`1" />.</returns>
	/// <param name="item">The object to remove from the <see cref="T:System.Collections.ObjectModel.Collection`1" />. The value can be null for reference types.</param>
	public bool Remove(T item)
	{
		if (items.IsReadOnly)
		{
			ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
		}
		int num = items.IndexOf(item);
		if (num < 0)
		{
			return false;
		}
		RemoveItem(num);
		return true;
	}

	/// <summary>Removes the element at the specified index of the <see cref="T:System.Collections.ObjectModel.Collection`1" />.</summary>
	/// <param name="index">The zero-based index of the element to remove.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.-or-<paramref name="index" /> is equal to or greater than <see cref="P:System.Collections.ObjectModel.Collection`1.Count" />.</exception>
	public void RemoveAt(int index)
	{
		if (items.IsReadOnly)
		{
			ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
		}
		if (index < 0 || index >= items.Count)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException();
		}
		RemoveItem(index);
	}

	/// <summary>Removes all elements from the <see cref="T:System.Collections.ObjectModel.Collection`1" />.</summary>
	protected virtual void ClearItems()
	{
		items.Clear();
	}

	/// <summary>Inserts an element into the <see cref="T:System.Collections.ObjectModel.Collection`1" /> at the specified index.</summary>
	/// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
	/// <param name="item">The object to insert. The value can be null for reference types.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.-or-<paramref name="index" /> is greater than <see cref="P:System.Collections.ObjectModel.Collection`1.Count" />.</exception>
	protected virtual void InsertItem(int index, T item)
	{
		items.Insert(index, item);
	}

	/// <summary>Removes the element at the specified index of the <see cref="T:System.Collections.ObjectModel.Collection`1" />.</summary>
	/// <param name="index">The zero-based index of the element to remove.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.-or-<paramref name="index" /> is equal to or greater than <see cref="P:System.Collections.ObjectModel.Collection`1.Count" />.</exception>
	protected virtual void RemoveItem(int index)
	{
		items.RemoveAt(index);
	}

	/// <summary>Replaces the element at the specified index.</summary>
	/// <param name="index">The zero-based index of the element to replace.</param>
	/// <param name="item">The new value for the element at the specified index. The value can be null for reference types.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.-or-<paramref name="index" /> is greater than <see cref="P:System.Collections.ObjectModel.Collection`1.Count" />.</exception>
	protected virtual void SetItem(int index, T item)
	{
		items[index] = item;
	}

	/// <summary>Returns an enumerator that iterates through a collection.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> that can be used to iterate through the collection.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)items).GetEnumerator();
	}

	/// <summary>Copies the elements of the <see cref="T:System.Collections.ICollection" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.ICollection" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
	/// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="array" /> is multidimensional.-or-<paramref name="array" /> does not have zero-based indexing.-or-The number of elements in the source <see cref="T:System.Collections.ICollection" /> is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />.-or-The type of the source <see cref="T:System.Collections.ICollection" /> cannot be cast automatically to the type of the destination <paramref name="array" />.</exception>
	void ICollection.CopyTo(Array array, int index)
	{
		if (array == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
		}
		if (array.Rank != 1)
		{
			ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RankMultiDimNotSupported);
		}
		if (array.GetLowerBound(0) != 0)
		{
			ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_NonZeroLowerBound);
		}
		if (index < 0)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
		}
		if (array.Length - index < Count)
		{
			ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
		}
		if (array is T[] array2)
		{
			items.CopyTo(array2, index);
			return;
		}
		Type elementType = array.GetType().GetElementType();
		Type typeFromHandle = typeof(T);
		if (!elementType.IsAssignableFrom(typeFromHandle) && !typeFromHandle.IsAssignableFrom(elementType))
		{
			ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArrayType);
		}
		object[] array3 = array as object[];
		if (array3 == null)
		{
			ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArrayType);
		}
		int count = items.Count;
		try
		{
			for (int i = 0; i < count; i++)
			{
				array3[index++] = items[i];
			}
		}
		catch (ArrayTypeMismatchException)
		{
			ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArrayType);
		}
	}

	/// <summary>Adds an item to the <see cref="T:System.Collections.IList" />.</summary>
	/// <returns>The position into which the new element was inserted.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to add to the <see cref="T:System.Collections.IList" />.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is of a type that is not assignable to the <see cref="T:System.Collections.IList" />.</exception>
	int IList.Add(object value)
	{
		if (items.IsReadOnly)
		{
			ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
		}
		ThrowHelper.IfNullAndNullsAreIllegalThenThrow<T>(value, ExceptionArgument.value);
		try
		{
			Add((T)value);
		}
		catch (InvalidCastException)
		{
			ThrowHelper.ThrowWrongValueTypeArgumentException(value, typeof(T));
		}
		return Count - 1;
	}

	/// <summary>Determines whether the <see cref="T:System.Collections.IList" /> contains a specific value.</summary>
	/// <returns>true if the <see cref="T:System.Object" /> is found in the <see cref="T:System.Collections.IList" />; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Collections.IList" />.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is of a type that is not assignable to the <see cref="T:System.Collections.IList" />.</exception>
	bool IList.Contains(object value)
	{
		if (IsCompatibleObject(value))
		{
			return Contains((T)value);
		}
		return false;
	}

	/// <summary>Determines the index of a specific item in the <see cref="T:System.Collections.IList" />.</summary>
	/// <returns>The index of <paramref name="value" /> if found in the list; otherwise, -1.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Collections.IList" />.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is of a type that is not assignable to the <see cref="T:System.Collections.IList" />.</exception>
	int IList.IndexOf(object value)
	{
		if (IsCompatibleObject(value))
		{
			return IndexOf((T)value);
		}
		return -1;
	}

	/// <summary>Inserts an item into the <see cref="T:System.Collections.IList" /> at the specified index.</summary>
	/// <param name="index">The zero-based index at which <paramref name="value" /> should be inserted.</param>
	/// <param name="value">The <see cref="T:System.Object" /> to insert into the <see cref="T:System.Collections.IList" />.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is not a valid index in the <see cref="T:System.Collections.IList" />. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is of a type that is not assignable to the <see cref="T:System.Collections.IList" />.</exception>
	void IList.Insert(int index, object value)
	{
		if (items.IsReadOnly)
		{
			ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
		}
		ThrowHelper.IfNullAndNullsAreIllegalThenThrow<T>(value, ExceptionArgument.value);
		try
		{
			Insert(index, (T)value);
		}
		catch (InvalidCastException)
		{
			ThrowHelper.ThrowWrongValueTypeArgumentException(value, typeof(T));
		}
	}

	/// <summary>Removes the first occurrence of a specific object from the <see cref="T:System.Collections.IList" />.</summary>
	/// <param name="value">The <see cref="T:System.Object" /> to remove from the <see cref="T:System.Collections.IList" />.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is of a type that is not assignable to the <see cref="T:System.Collections.IList" />.</exception>
	void IList.Remove(object value)
	{
		if (items.IsReadOnly)
		{
			ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
		}
		if (IsCompatibleObject(value))
		{
			Remove((T)value);
		}
	}

	private static bool IsCompatibleObject(object value)
	{
		if (!(value is T))
		{
			if (value == null)
			{
				return default(T) == null;
			}
			return false;
		}
		return true;
	}
}
