using System.Collections;
using System.Collections.Generic;
using MS.Internal;

namespace System.Windows;

/// <summary>Represents a collection of <see cref="T:System.Windows.TriggerAction" /> objects.</summary>
public sealed class TriggerActionCollection : IList, ICollection, IEnumerable, IList<TriggerAction>, ICollection<TriggerAction>, IEnumerable<TriggerAction>
{
	private List<TriggerAction> _rawList;

	private bool _sealed;

	private DependencyObject _owner;

	/// <summary>Gets the number of items in the collection.</summary>
	/// <returns>The number of items that the collection contains.</returns>
	public int Count => _rawList.Count;

	/// <summary>Gets a value that indicates whether the collection is read-only.</summary>
	/// <returns>true if the collection is read-only; otherwise, false.</returns>
	public bool IsReadOnly => _sealed;

	/// <summary>Gets or sets the item that is at the specified index.</summary>
	/// <returns>The <see cref="T:System.Windows.TriggerAction" /> object that is at the specified index.</returns>
	/// <param name="index">The zero-based index of the item to get or set.</param>
	public TriggerAction this[int index]
	{
		get
		{
			return _rawList[index];
		}
		set
		{
			CheckSealed();
			object obj = _rawList[index];
			InheritanceContextHelper.RemoveContextFromObject(Owner, obj as DependencyObject);
			_rawList[index] = value;
		}
	}

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>true if the <see cref="T:System.Collections.IList" /> has a fixed size; otherwise, false.</returns>
	bool IList.IsFixedSize => _sealed;

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>The object that is at the specified index.</returns>
	/// <param name="index">The zero-based index of the item to get or set.</param>
	object IList.this[int index]
	{
		get
		{
			return _rawList[index];
		}
		set
		{
			this[index] = VerifyIsTriggerAction(value);
		}
	}

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.</returns>
	object ICollection.SyncRoot => this;

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>true if access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe); otherwise, false.</returns>
	bool ICollection.IsSynchronized => false;

	internal DependencyObject Owner
	{
		get
		{
			return _owner;
		}
		set
		{
			_owner = value;
		}
	}

	/// <summary>Initializes an instance of the <see cref="T:System.Windows.TriggerActionCollection" /> class.</summary>
	public TriggerActionCollection()
	{
		_rawList = new List<TriggerAction>();
	}

	/// <summary>Initializes an instance of the <see cref="T:System.Windows.TriggerActionCollection" /> class that has the specified initial size.</summary>
	/// <param name="initialSize">The size of the collection.</param>
	public TriggerActionCollection(int initialSize)
	{
		_rawList = new List<TriggerAction>(initialSize);
	}

	/// <summary>Removes all items from the collection.</summary>
	public void Clear()
	{
		CheckSealed();
		for (int num = _rawList.Count - 1; num >= 0; num--)
		{
			InheritanceContextHelper.RemoveContextFromObject(_owner, _rawList[num]);
		}
		_rawList.Clear();
	}

	/// <summary>Removes from the collection the item that is located at the specified index.</summary>
	/// <param name="index">The zero-based index of the item to remove.</param>
	public void RemoveAt(int index)
	{
		CheckSealed();
		TriggerAction oldValue = _rawList[index];
		InheritanceContextHelper.RemoveContextFromObject(_owner, oldValue);
		_rawList.RemoveAt(index);
	}

	/// <summary>Adds an item to the collection.</summary>
	/// <param name="value">The <see cref="T:System.Windows.TriggerAction" /> object to add.</param>
	public void Add(TriggerAction value)
	{
		CheckSealed();
		InheritanceContextHelper.ProvideContextForObject(_owner, value);
		_rawList.Add(value);
	}

	/// <summary>Returns a value that indicates whether the collection contains the specified <see cref="T:System.Windows.TriggerAction" /> object.</summary>
	/// <returns>true if the <see cref="T:System.Windows.TriggerAction" /> object is found in the collection; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.Windows.TriggerAction" /> object to locate in the collection.</param>
	public bool Contains(TriggerAction value)
	{
		return _rawList.Contains(value);
	}

	/// <summary>Begins at the specified index and copies the collection items to the specified array.</summary>
	/// <param name="array">The one-dimensional array that is the destination of the items that are copied from the collection. The array must use zero-based indexing.</param>
	/// <param name="index">The zero-based index in the <paramref name="array" /> where copying starts.</param>
	public void CopyTo(TriggerAction[] array, int index)
	{
		_rawList.CopyTo(array, index);
	}

	/// <summary>Returns the index of the specified item in the collection.</summary>
	/// <returns>The index of <paramref name="value" /> if the <see cref="T:System.Windows.TriggerAction" /> object is found in the collection; otherwise, -1.</returns>
	/// <param name="value">The <see cref="T:System.Windows.TriggerAction" /> object to locate in the collection.</param>
	public int IndexOf(TriggerAction value)
	{
		return _rawList.IndexOf(value);
	}

	/// <summary>Inserts the specified item into the collection at the specified index.</summary>
	/// <param name="index">The zero-based index where the <paramref name="value" /> must be inserted.</param>
	/// <param name="value">The <see cref="T:System.Windows.TriggerAction" /> object to insert into the collection.</param>
	public void Insert(int index, TriggerAction value)
	{
		CheckSealed();
		InheritanceContextHelper.ProvideContextForObject(_owner, value);
		_rawList.Insert(index, value);
	}

	/// <summary>Removes the first occurrence of the specified object from the collection.</summary>
	/// <returns>true if item is successfully removed; otherwise, false. This method also returns false if item was not found in the <see cref="T:System.Windows.TriggerActionCollection" />.</returns>
	/// <param name="value">The <see cref="T:System.Windows.TriggerAction" /> object to remove from the collection.</param>
	public bool Remove(TriggerAction value)
	{
		CheckSealed();
		InheritanceContextHelper.RemoveContextFromObject(_owner, value);
		return _rawList.Remove(value);
	}

	/// <summary>Returns an enumerator that iterates through the collection.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
	[CLSCompliant(false)]
	public IEnumerator<TriggerAction> GetEnumerator()
	{
		return _rawList.GetEnumerator();
	}

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>The position into which the new element was inserted.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to add to the <see cref="T:System.Collections.IList" />.</param>
	int IList.Add(object value)
	{
		CheckSealed();
		InheritanceContextHelper.ProvideContextForObject(_owner, value as DependencyObject);
		return ((IList)_rawList).Add((object?)VerifyIsTriggerAction(value));
	}

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>true if the <see cref="T:System.Object" /> is found in the <see cref="T:System.Collections.IList" />; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Collections.IList" />.</param>
	bool IList.Contains(object value)
	{
		return _rawList.Contains(VerifyIsTriggerAction(value));
	}

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>The index of <paramref name="value" /> if found in the list; otherwise, -1.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Collections.IList" />.</param>
	int IList.IndexOf(object value)
	{
		return _rawList.IndexOf(VerifyIsTriggerAction(value));
	}

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <param name="index">The zero-based index at which <paramref name="value" /> should be inserted.</param>
	/// <param name="value">The <see cref="T:System.Object" /> to insert into the <see cref="T:System.Collections.IList" />.</param>
	void IList.Insert(int index, object value)
	{
		Insert(index, VerifyIsTriggerAction(value));
	}

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <param name="value">The <see cref="T:System.Object" /> to remove from the <see cref="T:System.Collections.IList" />.</param>
	void IList.Remove(object value)
	{
		Remove(VerifyIsTriggerAction(value));
	}

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <param name="array">The one-dimensional array that is the destination of the items that are copied from the collection. The array must use zero-based indexing.</param>
	/// <param name="index">The zero-based index in the <paramref name="array" /> where copying starts.</param>
	void ICollection.CopyTo(Array array, int index)
	{
		((ICollection)_rawList).CopyTo(array, index);
	}

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)_rawList).GetEnumerator();
	}

	internal void Seal(TriggerBase containingTrigger)
	{
		for (int i = 0; i < _rawList.Count; i++)
		{
			_rawList[i].Seal(containingTrigger);
		}
	}

	private void CheckSealed()
	{
		if (_sealed)
		{
			throw new InvalidOperationException(SR.Format(SR.CannotChangeAfterSealed, "TriggerActionCollection"));
		}
	}

	private TriggerAction VerifyIsTriggerAction(object value)
	{
		TriggerAction obj = value as TriggerAction;
		if (obj == null)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			throw new ArgumentException(SR.MustBeTriggerAction);
		}
		return obj;
	}
}
