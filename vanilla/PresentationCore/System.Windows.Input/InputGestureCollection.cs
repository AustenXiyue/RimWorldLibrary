using System.Collections;
using System.Collections.Generic;
using MS.Internal.PresentationCore;

namespace System.Windows.Input;

/// <summary>Represents an ordered collection of <see cref="T:System.Windows.Input.InputGesture" /> objects. </summary>
public sealed class InputGestureCollection : IList, ICollection, IEnumerable
{
	private List<InputGesture> _innerGestureList;

	private bool _isReadOnly;

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IList.Item(System.Int32)" />.</summary>
	/// <returns>The element at the specified index.</returns>
	/// <param name="index">The zero-based index of the element to get or set. </param>
	object IList.this[int index]
	{
		get
		{
			return this[index];
		}
		set
		{
			if (!(value is InputGesture value2))
			{
				throw new NotSupportedException(SR.CollectionOnlyAcceptsInputGestures);
			}
			this[index] = value2;
		}
	}

	/// <summary>Gets or set the <see cref="T:System.Windows.Input.InputGesture" /> at the specified index. </summary>
	/// <returns>The gesture at the specified index.</returns>
	/// <param name="index">The position in the collection.</param>
	public InputGesture this[int index]
	{
		get
		{
			if (_innerGestureList == null)
			{
				return null;
			}
			return _innerGestureList[index];
		}
		set
		{
			if (IsReadOnly)
			{
				throw new NotSupportedException(SR.ReadOnlyInputGesturesCollection);
			}
			EnsureList();
			if (_innerGestureList != null)
			{
				_innerGestureList[index] = value;
			}
		}
	}

	/// <summary>Gets a value that indicates whether this <see cref="T:System.Windows.Input.InputGestureCollection" /> is synchronized (thread safe). </summary>
	/// <returns>true if the collection is thread-safe; otherwise, false.  The default value is false.</returns>
	public bool IsSynchronized
	{
		get
		{
			if (_innerGestureList != null)
			{
				return ((ICollection)_innerGestureList).IsSynchronized;
			}
			return false;
		}
	}

	/// <summary>Gets an object that can be used to synchronize access to this <see cref="T:System.Windows.Input.InputGestureCollection" />. </summary>
	/// <returns>The object that can be used to synchronize access to the collection.</returns>
	public object SyncRoot
	{
		get
		{
			if (_innerGestureList == null)
			{
				return this;
			}
			return ((ICollection)_innerGestureList).SyncRoot;
		}
	}

	/// <summary>Gets a value that indicates whether this <see cref="T:System.Windows.Input.InputGestureCollection" /> has a fixed size. </summary>
	/// <returns>true if the collection has a fixed size; otherwise, false.  The default value is false.</returns>
	public bool IsFixedSize => IsReadOnly;

	/// <summary>Gets a value that indicates whether this <see cref="T:System.Windows.Input.InputGestureCollection" /> is read-only.  The default value is false.</summary>
	/// <returns>true if the collection read-only; otherwise, false.</returns>
	public bool IsReadOnly => _isReadOnly;

	/// <summary>Gets the number of <see cref="T:System.Windows.Input.InputGesture" /> items in this <see cref="T:System.Windows.Input.InputGestureCollection" />.</summary>
	/// <returns>The number of gestures in the collection.</returns>
	public int Count
	{
		get
		{
			if (_innerGestureList == null)
			{
				return 0;
			}
			return _innerGestureList.Count;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.InputGestureCollection" /> class. </summary>
	public InputGestureCollection()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.InputGestureCollection" /> class using the elements in the specified <see cref="T:System.Collections.IList" />. </summary>
	/// <param name="inputGestures">The collection whose elements are copied to the new <see cref="T:System.Windows.Input.InputGestureCollection" />.</param>
	public InputGestureCollection(IList inputGestures)
	{
		if (inputGestures != null && inputGestures.Count > 0)
		{
			AddRange(inputGestures);
		}
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.ICollection.CopyTo(System.Array,System.Int32)" />.</summary>
	/// <param name="array">A zero-based <see cref="T:System.Array" /> that receives the copied items from the <see cref="T:System.Windows.Input.InputGestureCollection" />.</param>
	/// <param name="index">The first position in the specified <see cref="T:System.Array" /> to receive the copied contents.</param>
	void ICollection.CopyTo(Array array, int index)
	{
		if (_innerGestureList != null)
		{
			((ICollection)_innerGestureList).CopyTo(array, index);
		}
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Contains(System.Object)" />.</summary>
	/// <returns>true if the <see cref="T:System.Object" /> is found in the <see cref="T:System.Windows.Input.InputGestureCollection" />; otherwise, false.</returns>
	/// <param name="key">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.Media.TextEffectCollection" />.</param>
	bool IList.Contains(object key)
	{
		return Contains(key as InputGesture);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.IndexOf(System.Object)" />.</summary>
	/// <returns>The index of <paramref name="value" /> if found in the list; otherwise, -1.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.Input.InputGestureCollection" />.</param>
	int IList.IndexOf(object value)
	{
		if (!(value is InputGesture value2))
		{
			return -1;
		}
		return IndexOf(value2);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Insert(System.Int32,System.Object)" />.</summary>
	/// <param name="index">The zero-based index at which to insert the <see cref="T:System.Object" />.</param>
	/// <param name="value">The <see cref="T:System.Object" /> to insert into the <see cref="T:System.Windows.Input.InputGestureCollection" />.</param>
	void IList.Insert(int index, object value)
	{
		if (IsReadOnly)
		{
			throw new NotSupportedException(SR.ReadOnlyInputGesturesCollection);
		}
		Insert(index, value as InputGesture);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Add(System.Object)" />.</summary>
	/// <returns>The position into which the new element was inserted.</returns>
	/// <param name="inputGesture">The <see cref="T:System.Object" /> to add to the <see cref="T:System.Windows.Input.InputGestureCollection" />.</param>
	int IList.Add(object inputGesture)
	{
		if (IsReadOnly)
		{
			throw new NotSupportedException(SR.ReadOnlyInputGesturesCollection);
		}
		return Add(inputGesture as InputGesture);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Remove(System.Object)" />.</summary>
	/// <param name="inputGesture">The <see cref="T:System.Object" /> to remove from the <see cref="T:System.Windows.Input.InputGestureCollection" />.</param>
	void IList.Remove(object inputGesture)
	{
		if (IsReadOnly)
		{
			throw new NotSupportedException(SR.ReadOnlyInputGesturesCollection);
		}
		Remove(inputGesture as InputGesture);
	}

	/// <summary>Gets an enumerator that iterates through this <see cref="T:System.Windows.Input.InputGestureCollection" />. </summary>
	/// <returns>The enumerator for this collection.</returns>
	public IEnumerator GetEnumerator()
	{
		if (_innerGestureList != null)
		{
			return _innerGestureList.GetEnumerator();
		}
		return new List<InputGesture>(0).GetEnumerator();
	}

	/// <summary>Searches for the first occurrence of the specified <see cref="T:System.Windows.Input.InputGesture" /> in this <see cref="T:System.Windows.Input.InputGestureCollection" />.</summary>
	/// <returns>The index of the first occurrence of <paramref name="value" />, if found; otherwise, -1. </returns>
	/// <param name="value">The gesture to locate in the collection.</param>
	public int IndexOf(InputGesture value)
	{
		if (_innerGestureList == null)
		{
			return -1;
		}
		return _innerGestureList.IndexOf(value);
	}

	/// <summary>Removes the specified <see cref="T:System.Windows.Input.InputGesture" /> at the specified index of this <see cref="T:System.Windows.Input.InputGestureCollection" />.</summary>
	/// <param name="index">The zero-based index of the gesture to remove.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">index is less than 0.</exception>
	public void RemoveAt(int index)
	{
		if (IsReadOnly)
		{
			throw new NotSupportedException(SR.ReadOnlyInputGesturesCollection);
		}
		if (_innerGestureList != null)
		{
			_innerGestureList.RemoveAt(index);
		}
	}

	/// <summary>Adds the specified <see cref="T:System.Windows.Input.InputGesture" /> to this <see cref="T:System.Windows.Input.InputGestureCollection" />. </summary>
	/// <returns>0, if the operation was successful (note that this is not the index of the added item).  </returns>
	/// <param name="inputGesture">The gesture to add to the collection.</param>
	/// <exception cref="T:System.NotSupportedException">the collection is read-only.</exception>
	/// <exception cref="T:System.ArgumentNullException">the gesture is null.</exception>
	public int Add(InputGesture inputGesture)
	{
		if (IsReadOnly)
		{
			throw new NotSupportedException(SR.ReadOnlyInputGesturesCollection);
		}
		if (inputGesture == null)
		{
			throw new ArgumentNullException("inputGesture");
		}
		EnsureList();
		_innerGestureList.Add(inputGesture);
		return 0;
	}

	/// <summary>Adds the elements of the specified <see cref="T:System.Collections.ICollection" /> to the end of this <see cref="T:System.Windows.Input.InputGestureCollection" />.</summary>
	/// <param name="collection">The collection of items to add to the end of this <see cref="T:System.Windows.Input.InputGestureCollection" />.</param>
	/// <exception cref="T:System.NotSupportedException">Any of the items in the collection to add are null.</exception>
	/// <exception cref="T:System.NotSupportedException">The collection is read-only.</exception>
	/// <exception cref="T:System.ArgumentNullException">The collection to add is null.</exception>
	public void AddRange(ICollection collection)
	{
		if (IsReadOnly)
		{
			throw new NotSupportedException(SR.ReadOnlyInputGesturesCollection);
		}
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		if (collection.Count <= 0)
		{
			return;
		}
		if (_innerGestureList == null)
		{
			_innerGestureList = new List<InputGesture>(collection.Count);
		}
		IEnumerator enumerator = collection.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (enumerator.Current is InputGesture item)
			{
				_innerGestureList.Add(item);
				continue;
			}
			throw new NotSupportedException(SR.CollectionOnlyAcceptsInputGestures);
		}
	}

	/// <summary> Inserts the specified <see cref="T:System.Windows.Input.InputGesture" /> into this <see cref="T:System.Windows.Input.InputGestureCollection" /> at the specified index.</summary>
	/// <param name="index">Index at which to insert <paramref name="inputGesture" />.</param>
	/// <param name="inputGesture">The gesture to insert.</param>
	/// <exception cref="T:System.NotSupportedException">the collection is read-only.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="inputGesture" /> is null.</exception>
	public void Insert(int index, InputGesture inputGesture)
	{
		if (IsReadOnly)
		{
			throw new NotSupportedException(SR.ReadOnlyInputGesturesCollection);
		}
		if (inputGesture == null)
		{
			throw new NotSupportedException(SR.CollectionOnlyAcceptsInputGestures);
		}
		if (_innerGestureList != null)
		{
			_innerGestureList.Insert(index, inputGesture);
		}
	}

	/// <summary>Removes the first occurrence of the specified <see cref="T:System.Windows.Input.InputGesture" /> from this <see cref="T:System.Windows.Input.InputGestureCollection" />.</summary>
	/// <param name="inputGesture">The gesture to remove.</param>
	/// <exception cref="T:System.NotSupportedException">The collection is read-only.</exception>
	/// <exception cref="T:System.ArgumentNullException">The gesture is null.</exception>
	public void Remove(InputGesture inputGesture)
	{
		if (IsReadOnly)
		{
			throw new NotSupportedException(SR.ReadOnlyInputGesturesCollection);
		}
		if (inputGesture == null)
		{
			throw new ArgumentNullException("inputGesture");
		}
		if (_innerGestureList != null && _innerGestureList.Contains(inputGesture))
		{
			_innerGestureList.Remove(inputGesture);
		}
	}

	/// <summary>Removes all elements from the <see cref="T:System.Windows.Input.InputGestureCollection" />.</summary>
	/// <exception cref="T:System.NotSupportedException">The collection is read only.</exception>
	public void Clear()
	{
		if (IsReadOnly)
		{
			throw new NotSupportedException(SR.ReadOnlyInputGesturesCollection);
		}
		if (_innerGestureList != null)
		{
			_innerGestureList.Clear();
			_innerGestureList = null;
		}
	}

	/// <summary>Determines whether the specified <see cref="T:System.Windows.Input.InputGesture" /> is in the collection. </summary>
	/// <returns>true if the gesture is in the collection; otherwise, false.</returns>
	/// <param name="key">The gesture to locate in the collection.</param>
	public bool Contains(InputGesture key)
	{
		if (_innerGestureList != null && key != null)
		{
			return _innerGestureList.Contains(key);
		}
		return false;
	}

	/// <summary>Copies all of the items in the <see cref="T:System.Windows.Input.InputGestureCollection" /> to the specified one-dimensional array, starting at the specified index of the target array.</summary>
	/// <param name="inputGestures">An array into which the collection is copied.</param>
	/// <param name="index">The index position in the <paramref name="inputGestures" /> at which copying begins.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="inputGestures" /> is a null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than 0.</exception>
	public void CopyTo(InputGesture[] inputGestures, int index)
	{
		if (_innerGestureList != null)
		{
			_innerGestureList.CopyTo(inputGestures, index);
		}
	}

	/// <summary>Sets this <see cref="T:System.Windows.Input.InputGestureCollection" /> to read-only. </summary>
	public void Seal()
	{
		_isReadOnly = true;
	}

	private void EnsureList()
	{
		if (_innerGestureList == null)
		{
			_innerGestureList = new List<InputGesture>(1);
		}
	}

	internal InputGesture FindMatch(object targetElement, InputEventArgs inputEventArgs)
	{
		for (int i = 0; i < Count; i++)
		{
			InputGesture inputGesture = this[i];
			if (inputGesture.Matches(targetElement, inputEventArgs))
			{
				return inputGesture;
			}
		}
		return null;
	}
}
