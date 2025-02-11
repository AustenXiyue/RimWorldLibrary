using System.Collections;
using System.Collections.Generic;
using MS.Internal;
using MS.Internal.PresentationCore;

namespace System.Windows.Input;

/// <summary>Represents an ordered collection of <see cref="T:System.Windows.Input.InputBinding" /> objects. </summary>
public sealed class InputBindingCollection : IList, ICollection, IEnumerable
{
	private List<InputBinding> _innerBindingList;

	private bool _isReadOnly;

	private DependencyObject _owner;

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
			if (!(value is InputBinding value2))
			{
				throw new NotSupportedException(SR.CollectionOnlyAcceptsInputBindings);
			}
			this[index] = value2;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Input.InputBinding" /> at the specified index. </summary>
	/// <returns>The <see cref="T:System.Windows.Input.InputBinding" /> at the specified index.</returns>
	/// <param name="index">The position in the collection.</param>
	public InputBinding this[int index]
	{
		get
		{
			if (_innerBindingList != null)
			{
				return _innerBindingList[index];
			}
			throw new ArgumentOutOfRangeException("index");
		}
		set
		{
			if (_innerBindingList != null)
			{
				InputBinding inputBinding = null;
				if (index >= 0 && index < _innerBindingList.Count)
				{
					inputBinding = _innerBindingList[index];
				}
				_innerBindingList[index] = value;
				if (inputBinding != null)
				{
					InheritanceContextHelper.RemoveContextFromObject(_owner, inputBinding);
				}
				InheritanceContextHelper.ProvideContextForObject(_owner, value);
				return;
			}
			throw new ArgumentOutOfRangeException("index");
		}
	}

	/// <summary>Gets a value indicating whether access to this <see cref="T:System.Windows.Input.InputBindingCollection" /> is synchronized (thread-safe). </summary>
	/// <returns>true if the collection is thread safe; otherwise, false. The default is false.</returns>
	public bool IsSynchronized
	{
		get
		{
			if (_innerBindingList != null)
			{
				return ((ICollection)_innerBindingList).IsSynchronized;
			}
			return false;
		}
	}

	/// <summary>Gets a value that indicates whether this <see cref="T:System.Windows.Input.InputBindingCollection" /> has a fixed size. </summary>
	/// <returns>true if the collection has a fixed size; otherwise, false. The default is false.</returns>
	public bool IsFixedSize => IsReadOnly;

	/// <summary>Gets the number of <see cref="T:System.Windows.Input.InputBinding" /> items in this collection. </summary>
	/// <returns>The number of items in the collection.</returns>
	public int Count
	{
		get
		{
			if (_innerBindingList == null)
			{
				return 0;
			}
			return _innerBindingList.Count;
		}
	}

	/// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Windows.Input.InputBindingCollection" />. </summary>
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Windows.Input.InputBindingCollection" />.</returns>
	public object SyncRoot => this;

	/// <summary>Gets a value that indicates whether this <see cref="T:System.Windows.Input.InputBindingCollection" /> is read-only. </summary>
	/// <returns>true if the collection is read-only; otherwise, false. The default is false.</returns>
	public bool IsReadOnly => _isReadOnly;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.InputBindingCollection" /> class. </summary>
	public InputBindingCollection()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.InputBindingCollection" /> class using the items in the specified <see cref="T:System.Collections.IList" />.  </summary>
	/// <param name="inputBindings">The collection whose items are copied to the new <see cref="T:System.Windows.Input.InputBindingCollection" />.</param>
	public InputBindingCollection(IList inputBindings)
	{
		if (inputBindings != null && inputBindings.Count > 0)
		{
			AddRange(inputBindings);
		}
	}

	internal InputBindingCollection(DependencyObject owner)
	{
		_owner = owner;
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.ICollection.CopyTo(System.Array,System.Int32)" />.</summary>
	/// <param name="array">A zero-based <see cref="T:System.Array" /> that receives the copied items from the <see cref="T:System.Windows.Input.InputBindingCollection" />.</param>
	/// <param name="index">The first position in the specified <see cref="T:System.Array" /> to receive the copied contents.</param>
	void ICollection.CopyTo(Array array, int index)
	{
		if (_innerBindingList != null)
		{
			((ICollection)_innerBindingList).CopyTo(array, index);
		}
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Contains(System.Object)" />.</summary>
	/// <returns>true if the <see cref="T:System.Object" /> is found in the <see cref="T:System.Windows.Input.InputBindingCollection" />; otherwise, false.</returns>
	/// <param name="key">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.Media.TextEffectCollection" />.</param>
	bool IList.Contains(object key)
	{
		return Contains(key as InputBinding);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.IndexOf(System.Object)" />.</summary>
	/// <returns>The index of <paramref name="value" /> if found in the list; otherwise, -1.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.Input.InputBindingCollection" />.</param>
	int IList.IndexOf(object value)
	{
		if (!(value is InputBinding value2))
		{
			return -1;
		}
		return IndexOf(value2);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Insert(System.Int32,System.Object)" />.</summary>
	/// <param name="index">The zero-based index at which to insert the <see cref="T:System.Object" />.</param>
	/// <param name="value">The <see cref="T:System.Object" /> to insert into the <see cref="T:System.Windows.Input.InputBindingCollection" />.</param>
	void IList.Insert(int index, object value)
	{
		Insert(index, value as InputBinding);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Add(System.Object)" />.</summary>
	/// <returns>The position into which the new element was inserted.</returns>
	/// <param name="inputBinding">The <see cref="T:System.Object" /> to add to the <see cref="T:System.Windows.Input.InputBindingCollection" />.</param>
	int IList.Add(object inputBinding)
	{
		Add(inputBinding as InputBinding);
		return 0;
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Remove(System.Object)" />.</summary>
	/// <param name="inputBinding">The <see cref="T:System.Object" /> to remove from the <see cref="T:System.Windows.Input.InputBindingCollection" />.</param>
	void IList.Remove(object inputBinding)
	{
		Remove(inputBinding as InputBinding);
	}

	/// <summary>Adds the specified <see cref="T:System.Windows.Input.InputBinding" /> to this <see cref="T:System.Windows.Input.InputBindingCollection" />. </summary>
	/// <returns>Always returns 0. This deviates from the standard <see cref="T:System.Collections.IList" /> implementation for Add, which should return the index where the new item was added to the collection.</returns>
	/// <param name="inputBinding">The binding to add to the collection.</param>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="inputBinding" /> is null.</exception>
	public int Add(InputBinding inputBinding)
	{
		if (inputBinding != null)
		{
			if (_innerBindingList == null)
			{
				_innerBindingList = new List<InputBinding>(1);
			}
			_innerBindingList.Add(inputBinding);
			InheritanceContextHelper.ProvideContextForObject(_owner, inputBinding);
			return 0;
		}
		throw new NotSupportedException(SR.CollectionOnlyAcceptsInputBindings);
	}

	/// <summary>Searches for the first occurrence of the specified <see cref="T:System.Windows.Input.InputBinding" /> in his <see cref="T:System.Windows.Input.InputBindingCollection" />.</summary>
	/// <returns>The index of the first occurrence of <paramref name="value" />, if found; otherwise, –1.</returns>
	/// <param name="value">The object to locate in the collection.</param>
	public int IndexOf(InputBinding value)
	{
		if (_innerBindingList == null)
		{
			return -1;
		}
		return _innerBindingList.IndexOf(value);
	}

	/// <summary>Adds the items of the specified <see cref="T:System.Collections.ICollection" /> to the end of this <see cref="T:System.Windows.Input.InputBindingCollection" /></summary>
	/// <param name="collection">The collection of items to add to the end of this <see cref="T:System.Windows.Input.InputBindingCollection" />.</param>
	public void AddRange(ICollection collection)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		if (collection.Count <= 0)
		{
			return;
		}
		if (_innerBindingList == null)
		{
			_innerBindingList = new List<InputBinding>(collection.Count);
		}
		IEnumerator enumerator = collection.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (enumerator.Current is InputBinding inputBinding)
			{
				_innerBindingList.Add(inputBinding);
				InheritanceContextHelper.ProvideContextForObject(_owner, inputBinding);
				continue;
			}
			throw new NotSupportedException(SR.CollectionOnlyAcceptsInputBindings);
		}
	}

	/// <summary>Inserts the specified <see cref="T:System.Windows.Input.InputBinding" /> into this <see cref="T:System.Windows.Input.InputBindingCollection" /> at the specified index. </summary>
	/// <param name="index">The zero-based index at which to insert <paramref name="inputBinding" />.</param>
	/// <param name="inputBinding">The binding to insert.</param>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="inputBinding" /> is null.</exception>
	public void Insert(int index, InputBinding inputBinding)
	{
		if (inputBinding == null)
		{
			throw new NotSupportedException(SR.CollectionOnlyAcceptsInputBindings);
		}
		if (_innerBindingList != null)
		{
			_innerBindingList.Insert(index, inputBinding);
			InheritanceContextHelper.ProvideContextForObject(_owner, inputBinding);
		}
	}

	/// <summary>Removes the first occurrence of the specified <see cref="T:System.Windows.Input.InputBinding" /> from this <see cref="T:System.Windows.Input.InputBindingCollection" />. </summary>
	/// <param name="inputBinding">The binding to remove.</param>
	public void Remove(InputBinding inputBinding)
	{
		if (_innerBindingList != null && inputBinding != null && _innerBindingList.Remove(inputBinding))
		{
			InheritanceContextHelper.RemoveContextFromObject(_owner, inputBinding);
		}
	}

	/// <summary>Removes the specified <see cref="T:System.Windows.Input.InputBinding" /> at the specified index of this <see cref="T:System.Windows.Input.InputBindingCollection" />. </summary>
	/// <param name="index">The zero-based index of the <see cref="T:System.Windows.Input.InputBinding" /> to remove.</param>
	public void RemoveAt(int index)
	{
		if (_innerBindingList != null)
		{
			InputBinding inputBinding = null;
			if (index >= 0 && index < _innerBindingList.Count)
			{
				inputBinding = _innerBindingList[index];
			}
			_innerBindingList.RemoveAt(index);
			if (inputBinding != null)
			{
				InheritanceContextHelper.RemoveContextFromObject(_owner, inputBinding);
			}
		}
	}

	/// <summary>Removes all items from this <see cref="T:System.Windows.Input.InputBindingCollection" />. </summary>
	public void Clear()
	{
		if (_innerBindingList == null)
		{
			return;
		}
		List<InputBinding> list = new List<InputBinding>(_innerBindingList);
		_innerBindingList.Clear();
		_innerBindingList = null;
		foreach (InputBinding item in list)
		{
			InheritanceContextHelper.RemoveContextFromObject(_owner, item);
		}
	}

	/// <summary>Gets an enumerator that iterates through this <see cref="T:System.Windows.Input.InputBindingCollection" />. </summary>
	/// <returns>The enumerator for this collection.</returns>
	public IEnumerator GetEnumerator()
	{
		if (_innerBindingList != null)
		{
			return _innerBindingList.GetEnumerator();
		}
		return new List<InputBinding>(0).GetEnumerator();
	}

	/// <summary>Determines whether the specified <see cref="T:System.Windows.Input.InputBinding" /> is in this <see cref="T:System.Windows.Input.InputBindingCollection" /></summary>
	/// <returns>true if the specified <see cref="T:System.Windows.Input.InputBinding" /> is in the collection; otherwise, false.</returns>
	/// <param name="key">The binding to locate in the collection.</param>
	public bool Contains(InputBinding key)
	{
		if (_innerBindingList != null && key != null)
		{
			return _innerBindingList.Contains(key);
		}
		return false;
	}

	/// <summary>Copies all of the items in the <see cref="T:System.Windows.Input.InputBindingCollection" /> to the specified one-dimensional array, starting at the specified index of the target array.</summary>
	/// <param name="inputBindings">The array into which the collection is copied.</param>
	/// <param name="index">The index position in <paramref name="inputBindings" /> at which copying starts.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="inputBindings" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than 0.</exception>
	public void CopyTo(InputBinding[] inputBindings, int index)
	{
		if (_innerBindingList != null)
		{
			_innerBindingList.CopyTo(inputBindings, index);
		}
	}

	internal InputBinding FindMatch(object targetElement, InputEventArgs inputEventArgs)
	{
		for (int num = Count - 1; num >= 0; num--)
		{
			InputBinding inputBinding = this[num];
			if (inputBinding.Command != null && inputBinding.Gesture != null && inputBinding.Gesture.Matches(targetElement, inputEventArgs))
			{
				return inputBinding;
			}
		}
		return null;
	}
}
