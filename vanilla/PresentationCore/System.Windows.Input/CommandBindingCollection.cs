using System.Collections;
using System.Collections.Generic;
using MS.Internal.PresentationCore;

namespace System.Windows.Input;

/// <summary>Represents a collection of <see cref="T:System.Windows.Input.CommandBinding" /> objects.</summary>
public sealed class CommandBindingCollection : IList, ICollection, IEnumerable
{
	private List<CommandBinding> _innerCBList;

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
			if (!(value is CommandBinding value2))
			{
				throw new NotSupportedException(SR.CollectionOnlyAcceptsCommandBindings);
			}
			this[index] = value2;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Input.CommandBinding" /> at the specified index. </summary>
	/// <returns>The binding at the specified index.</returns>
	/// <param name="index">The position in the collection.</param>
	public CommandBinding this[int index]
	{
		get
		{
			return _innerCBList?[index];
		}
		set
		{
			if (_innerCBList != null)
			{
				_innerCBList[index] = value;
			}
		}
	}

	/// <summary>Gets a value indicating whether this <see cref="T:System.Windows.Input.CommandBindingCollection" /> has a fixed size. </summary>
	/// <returns>true if the collection has a fixed size; otherwise, false.  The default value is false.</returns>
	public bool IsFixedSize => IsReadOnly;

	/// <summary>Gets a value indicating whether access to this <see cref="T:System.Windows.Input.CommandBindingCollection" /> is synchronized (thread-safe). </summary>
	/// <returns>true if the collection is thread-safe; otherwise, false.  The default value is false.</returns>
	public bool IsSynchronized
	{
		get
		{
			if (_innerCBList != null)
			{
				return ((ICollection)_innerCBList).IsSynchronized;
			}
			return false;
		}
	}

	/// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Windows.Input.CommandBindingCollection" />.</summary>
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Windows.Input.CommandBindingCollection" />.</returns>
	public object SyncRoot => this;

	/// <summary>Gets a value indicating whether this <see cref="T:System.Windows.Input.CommandBindingCollection" /> is read-only. </summary>
	/// <returns>true if the collection is read-only; otherwise, false. The default value is false.</returns>
	public bool IsReadOnly => false;

	/// <summary>Gets the number of <see cref="T:System.Windows.Input.CommandBinding" /> items in this <see cref="T:System.Windows.Input.CommandBindingCollection" />.</summary>
	/// <returns>The number of bindings in the collection.</returns>
	public int Count => _innerCBList?.Count ?? 0;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.CommandBindingCollection" /> class.</summary>
	public CommandBindingCollection()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.CommandBindingCollection" /> class using the items in the specified <see cref="T:System.Collections.IList" />.</summary>
	/// <param name="commandBindings">The collection whose items are copied to the new <see cref="T:System.Windows.Input.CommandBindingCollection" />.</param>
	public CommandBindingCollection(IList commandBindings)
	{
		if (commandBindings != null && commandBindings.Count > 0)
		{
			AddRange(commandBindings);
		}
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.ICollection.CopyTo(System.Array,System.Int32)" />.</summary>
	/// <param name="array">A zero-based <see cref="T:System.Array" /> that receives the copied items from the <see cref="T:System.Windows.Input.CommandBindingCollection" />.</param>
	/// <param name="index">The first position in the specified <see cref="T:System.Array" /> to receive the copied contents.</param>
	void ICollection.CopyTo(Array array, int index)
	{
		((ICollection)_innerCBList)?.CopyTo(array, index);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Contains(System.Object)" />.</summary>
	/// <returns>true if the <see cref="T:System.Object" /> is found in the <see cref="T:System.Windows.Input.CommandBindingCollection" />; otherwise, false.</returns>
	/// <param name="key">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.Media.TextEffectCollection" />.</param>
	bool IList.Contains(object key)
	{
		return Contains(key as CommandBinding);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.IndexOf(System.Object)" />.</summary>
	/// <returns>The index of <paramref name="value" /> if found in the list; otherwise, -1.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.Input.CommandBindingCollection" />.</param>
	int IList.IndexOf(object value)
	{
		if (!(value is CommandBinding value2))
		{
			return -1;
		}
		return IndexOf(value2);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Insert(System.Int32,System.Object)" />.</summary>
	/// <param name="index">The zero-based index at which to insert the <see cref="T:System.Object" />.</param>
	/// <param name="value">The <see cref="T:System.Object" /> to insert into the <see cref="T:System.Windows.Input.CommandBindingCollection" />.</param>
	void IList.Insert(int index, object value)
	{
		Insert(index, value as CommandBinding);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Add(System.Object)" />.</summary>
	/// <returns>The position into which the new element was inserted.</returns>
	/// <param name="commandBinding">The <see cref="T:System.Object" /> to add to the <see cref="T:System.Windows.Input.CommandBindingCollection" />.</param>
	int IList.Add(object commandBinding)
	{
		return Add(commandBinding as CommandBinding);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Remove(System.Object)" />.</summary>
	/// <param name="commandBinding">The <see cref="T:System.Object" /> to remove from the <see cref="T:System.Windows.Input.CommandBindingCollection" />.</param>
	void IList.Remove(object commandBinding)
	{
		Remove(commandBinding as CommandBinding);
	}

	/// <summary>Adds the specified <see cref="T:System.Windows.Input.CommandBinding" /> to this <see cref="T:System.Windows.Input.CommandBindingCollection" />.</summary>
	/// <returns>0, if the operation was successful (note that this is not the index of the added item).</returns>
	/// <param name="commandBinding">The binding to add to the collection.</param>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="commandBinding" /> is null.</exception>
	public int Add(CommandBinding commandBinding)
	{
		if (commandBinding != null)
		{
			if (_innerCBList == null)
			{
				_innerCBList = new List<CommandBinding>(1);
			}
			_innerCBList.Add(commandBinding);
			return 0;
		}
		throw new NotSupportedException(SR.CollectionOnlyAcceptsCommandBindings);
	}

	/// <summary>Adds the items of the specified <see cref="T:System.Collections.ICollection" /> to the end of this <see cref="T:System.Windows.Input.CommandBindingCollection" />. </summary>
	/// <param name="collection">The collection of items to add to the end of this <see cref="T:System.Windows.Input.CommandBindingCollection" />.</param>
	/// <exception cref="T:System.NotSupportedException">Any of the items in the collection to add are null.</exception>
	/// <exception cref="T:System.NotSupportedException">The collection to add is null.</exception>
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
		if (_innerCBList == null)
		{
			_innerCBList = new List<CommandBinding>(collection.Count);
		}
		IEnumerator enumerator = collection.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (enumerator.Current is CommandBinding item)
			{
				_innerCBList.Add(item);
				continue;
			}
			throw new NotSupportedException(SR.CollectionOnlyAcceptsCommandBindings);
		}
	}

	/// <summary>Inserts the specified <see cref="T:System.Windows.Input.CommandBinding" /> into this <see cref="T:System.Windows.Input.CommandBindingCollection" /> at the specified index.</summary>
	/// <param name="index">The zero-based index at which to insert <paramref name="commandBinding" /></param>
	/// <param name="commandBinding">The binding to insert.</param>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="commandBinding" /> is null.</exception>
	public void Insert(int index, CommandBinding commandBinding)
	{
		if (commandBinding != null)
		{
			_innerCBList?.Insert(index, commandBinding);
			return;
		}
		throw new NotSupportedException(SR.CollectionOnlyAcceptsCommandBindings);
	}

	/// <summary>Removes the first occurrence of the specified <see cref="T:System.Windows.Input.CommandBinding" /> from this <see cref="T:System.Windows.Input.CommandBindingCollection" />.</summary>
	/// <param name="commandBinding">The binding to remove.</param>
	public void Remove(CommandBinding commandBinding)
	{
		if (_innerCBList != null && commandBinding != null)
		{
			_innerCBList.Remove(commandBinding);
		}
	}

	/// <summary>Removes the specified <see cref="T:System.Windows.Input.CommandBinding" /> at the specified index of this <see cref="T:System.Windows.Input.CommandBindingCollection" />.</summary>
	/// <param name="index">The zero-based index of the <see cref="T:System.Windows.Input.CommandBinding" /> to remove.</param>
	public void RemoveAt(int index)
	{
		_innerCBList?.RemoveAt(index);
	}

	/// <summary>Removes all items from this <see cref="T:System.Windows.Input.CommandBindingCollection" />.</summary>
	public void Clear()
	{
		if (_innerCBList != null)
		{
			_innerCBList.Clear();
			_innerCBList = null;
		}
	}

	/// <summary>Searches for the first occurrence of the specified <see cref="T:System.Windows.Input.CommandBinding" /> in this <see cref="T:System.Windows.Input.CommandBindingCollection" />. </summary>
	/// <returns>The index of the first occurrence of <paramref name="value" />, if found; otherwise, -1.</returns>
	/// <param name="value">The binding to locate in the collection. </param>
	public int IndexOf(CommandBinding value)
	{
		return _innerCBList?.IndexOf(value) ?? (-1);
	}

	/// <summary>Determines whether the specified <see cref="T:System.Windows.Input.CommandBinding" /> is in this <see cref="T:System.Windows.Input.CommandBindingCollection" />.</summary>
	/// <returns>true if the specified <see cref="T:System.Windows.Input.CommandBinding" /> is in the collection; otherwise, false.</returns>
	/// <param name="commandBinding">The binding to locate in the collection.</param>
	public bool Contains(CommandBinding commandBinding)
	{
		if (_innerCBList != null && commandBinding != null)
		{
			return _innerCBList.Contains(commandBinding);
		}
		return false;
	}

	/// <summary>Copies all of the items in the <see cref="T:System.Windows.Input.CommandBindingCollection" /> to the specified one-dimensional array, starting at the specified index of the target array.</summary>
	/// <param name="commandBindings">The array into which the collection is copied.</param>
	/// <param name="index">The index position in <paramref name="commandBindings" /> at which copying starts.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="commandBindings" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than 0.</exception>
	public void CopyTo(CommandBinding[] commandBindings, int index)
	{
		_innerCBList?.CopyTo(commandBindings, index);
	}

	/// <summary>Gets an enumerator that iterates through this <see cref="T:System.Windows.Input.CommandBindingCollection" />. </summary>
	/// <returns>The enumerator for this collection.</returns>
	public IEnumerator GetEnumerator()
	{
		if (_innerCBList != null)
		{
			return _innerCBList.GetEnumerator();
		}
		return new List<CommandBinding>(0).GetEnumerator();
	}

	internal ICommand FindMatch(object targetElement, InputEventArgs inputEventArgs)
	{
		for (int i = 0; i < Count; i++)
		{
			if (this[i].Command is RoutedCommand routedCommand && routedCommand.InputGesturesInternal?.FindMatch(targetElement, inputEventArgs) != null)
			{
				return routedCommand;
			}
		}
		return null;
	}

	internal CommandBinding FindMatch(ICommand command, ref int index)
	{
		while (index < Count)
		{
			CommandBinding commandBinding = this[index++];
			if (commandBinding.Command == command)
			{
				return commandBinding;
			}
		}
		return null;
	}
}
