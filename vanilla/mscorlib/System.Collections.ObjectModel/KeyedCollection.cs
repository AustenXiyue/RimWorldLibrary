using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace System.Collections.ObjectModel;

/// <summary>Provides the abstract base class for a collection whose keys are embedded in the values.</summary>
/// <typeparam name="TKey">The type of keys in the collection.</typeparam>
/// <typeparam name="TItem">The type of items in the collection.</typeparam>
[Serializable]
[DebuggerDisplay("Count = {Count}")]
[ComVisible(false)]
[DebuggerTypeProxy(typeof(Mscorlib_KeyedCollectionDebugView<, >))]
public abstract class KeyedCollection<TKey, TItem> : Collection<TItem>
{
	private const int defaultThreshold = 0;

	private IEqualityComparer<TKey> comparer;

	private Dictionary<TKey, TItem> dict;

	private int keyCount;

	private int threshold;

	/// <summary>Gets the generic equality comparer that is used to determine equality of keys in the collection.</summary>
	/// <returns>The implementation of the <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> generic interface that is used to determine equality of keys in the collection.</returns>
	public IEqualityComparer<TKey> Comparer => comparer;

	/// <summary>Gets the element with the specified key. </summary>
	/// <returns>The element with the specified key. If an element with the specified key is not found, an exception is thrown.</returns>
	/// <param name="key">The key of the element to get.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">An element with the specified key does not exist in the collection.</exception>
	public TItem this[TKey key]
	{
		get
		{
			if (key == null)
			{
				ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
			}
			if (dict != null)
			{
				return dict[key];
			}
			foreach (TItem item in base.Items)
			{
				if (comparer.Equals(GetKeyForItem(item), key))
				{
					return item;
				}
			}
			ThrowHelper.ThrowKeyNotFoundException();
			return default(TItem);
		}
	}

	/// <summary>Gets the lookup dictionary of the <see cref="T:System.Collections.ObjectModel.KeyedCollection`2" />.</summary>
	/// <returns>The lookup dictionary of the <see cref="T:System.Collections.ObjectModel.KeyedCollection`2" />, if it exists; otherwise, null.</returns>
	protected IDictionary<TKey, TItem> Dictionary => dict;

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.ObjectModel.KeyedCollection`2" /> class that uses the default equality comparer.</summary>
	protected KeyedCollection()
		: this((IEqualityComparer<TKey>)null, 0)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.ObjectModel.KeyedCollection`2" /> class that uses the specified equality comparer.</summary>
	/// <param name="comparer">The implementation of the <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> generic interface to use when comparing keys, or null to use the default equality comparer for the type of the key, obtained from <see cref="P:System.Collections.Generic.EqualityComparer`1.Default" />.</param>
	protected KeyedCollection(IEqualityComparer<TKey> comparer)
		: this(comparer, 0)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.ObjectModel.KeyedCollection`2" /> class that uses the specified equality comparer and creates a lookup dictionary when the specified threshold is exceeded.</summary>
	/// <param name="comparer">The implementation of the <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> generic interface to use when comparing keys, or null to use the default equality comparer for the type of the key, obtained from <see cref="P:System.Collections.Generic.EqualityComparer`1.Default" />.</param>
	/// <param name="dictionaryCreationThreshold">The number of elements the collection can hold without creating a lookup dictionary (0 creates the lookup dictionary when the first item is added), or –1 to specify that a lookup dictionary is never created.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="dictionaryCreationThreshold" /> is less than –1.</exception>
	protected KeyedCollection(IEqualityComparer<TKey> comparer, int dictionaryCreationThreshold)
	{
		if (comparer == null)
		{
			comparer = EqualityComparer<TKey>.Default;
		}
		if (dictionaryCreationThreshold == -1)
		{
			dictionaryCreationThreshold = int.MaxValue;
		}
		if (dictionaryCreationThreshold < -1)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.dictionaryCreationThreshold, ExceptionResource.ArgumentOutOfRange_InvalidThreshold);
		}
		this.comparer = comparer;
		threshold = dictionaryCreationThreshold;
	}

	/// <summary>Determines whether the collection contains an element with the specified key.</summary>
	/// <returns>true if the <see cref="T:System.Collections.ObjectModel.KeyedCollection`2" /> contains an element with the specified key; otherwise, false.</returns>
	/// <param name="key">The key to locate in the <see cref="T:System.Collections.ObjectModel.KeyedCollection`2" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	public bool Contains(TKey key)
	{
		if (key == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
		}
		if (dict != null)
		{
			return dict.ContainsKey(key);
		}
		if (key != null)
		{
			foreach (TItem item in base.Items)
			{
				if (comparer.Equals(GetKeyForItem(item), key))
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool ContainsItem(TItem item)
	{
		TKey keyForItem;
		if (dict == null || (keyForItem = GetKeyForItem(item)) == null)
		{
			return base.Items.Contains(item);
		}
		if (dict.TryGetValue(keyForItem, out var value))
		{
			return EqualityComparer<TItem>.Default.Equals(value, item);
		}
		return false;
	}

	/// <summary>Removes the element with the specified key from the <see cref="T:System.Collections.ObjectModel.KeyedCollection`2" />.</summary>
	/// <returns>true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key" /> is not found in the <see cref="T:System.Collections.ObjectModel.KeyedCollection`2" />.</returns>
	/// <param name="key">The key of the element to remove.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	public bool Remove(TKey key)
	{
		if (key == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
		}
		if (dict != null)
		{
			if (dict.ContainsKey(key))
			{
				return Remove(dict[key]);
			}
			return false;
		}
		if (key != null)
		{
			for (int i = 0; i < base.Items.Count; i++)
			{
				if (comparer.Equals(GetKeyForItem(base.Items[i]), key))
				{
					RemoveItem(i);
					return true;
				}
			}
		}
		return false;
	}

	/// <summary>Changes the key associated with the specified element in the lookup dictionary.</summary>
	/// <param name="item">The element to change the key of.</param>
	/// <param name="newKey">The new key for <paramref name="item" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="item" /> is null.-or-<paramref name="key" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="item" /> is not found.-or-<paramref name="key" /> already exists in the <see cref="T:System.Collections.ObjectModel.KeyedCollection`2" />.</exception>
	protected void ChangeItemKey(TItem item, TKey newKey)
	{
		if (!ContainsItem(item))
		{
			ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_ItemNotExist);
		}
		TKey keyForItem = GetKeyForItem(item);
		if (!comparer.Equals(keyForItem, newKey))
		{
			if (newKey != null)
			{
				AddKey(newKey, item);
			}
			if (keyForItem != null)
			{
				RemoveKey(keyForItem);
			}
		}
	}

	/// <summary>Removes all elements from the <see cref="T:System.Collections.ObjectModel.KeyedCollection`2" />.</summary>
	protected override void ClearItems()
	{
		base.ClearItems();
		if (dict != null)
		{
			dict.Clear();
		}
		keyCount = 0;
	}

	/// <summary>When implemented in a derived class, extracts the key from the specified element.</summary>
	/// <returns>The key for the specified element.</returns>
	/// <param name="item">The element from which to extract the key.</param>
	protected abstract TKey GetKeyForItem(TItem item);

	/// <summary>Inserts an element into the <see cref="T:System.Collections.ObjectModel.KeyedCollection`2" /> at the specified index.</summary>
	/// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
	/// <param name="item">The object to insert.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than 0.-or-<paramref name="index" /> is greater than <see cref="P:System.Collections.ObjectModel.Collection`1.Count" />.</exception>
	protected override void InsertItem(int index, TItem item)
	{
		TKey keyForItem = GetKeyForItem(item);
		if (keyForItem != null)
		{
			AddKey(keyForItem, item);
		}
		base.InsertItem(index, item);
	}

	/// <summary>Removes the element at the specified index of the <see cref="T:System.Collections.ObjectModel.KeyedCollection`2" />.</summary>
	/// <param name="index">The index of the element to remove.</param>
	protected override void RemoveItem(int index)
	{
		TKey keyForItem = GetKeyForItem(base.Items[index]);
		if (keyForItem != null)
		{
			RemoveKey(keyForItem);
		}
		base.RemoveItem(index);
	}

	/// <summary>Replaces the item at the specified index with the specified item.</summary>
	/// <param name="index">The zero-based index of the item to be replaced.</param>
	/// <param name="item">The new item.</param>
	protected override void SetItem(int index, TItem item)
	{
		TKey keyForItem = GetKeyForItem(item);
		TKey keyForItem2 = GetKeyForItem(base.Items[index]);
		if (comparer.Equals(keyForItem2, keyForItem))
		{
			if (keyForItem != null && dict != null)
			{
				dict[keyForItem] = item;
			}
		}
		else
		{
			if (keyForItem != null)
			{
				AddKey(keyForItem, item);
			}
			if (keyForItem2 != null)
			{
				RemoveKey(keyForItem2);
			}
		}
		base.SetItem(index, item);
	}

	private void AddKey(TKey key, TItem item)
	{
		if (dict != null)
		{
			dict.Add(key, item);
			return;
		}
		if (keyCount == threshold)
		{
			CreateDictionary();
			dict.Add(key, item);
			return;
		}
		if (Contains(key))
		{
			ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_AddingDuplicate);
		}
		keyCount++;
	}

	private void CreateDictionary()
	{
		dict = new Dictionary<TKey, TItem>(comparer);
		foreach (TItem item in base.Items)
		{
			TKey keyForItem = GetKeyForItem(item);
			if (keyForItem != null)
			{
				dict.Add(keyForItem, item);
			}
		}
	}

	private void RemoveKey(TKey key)
	{
		if (dict != null)
		{
			dict.Remove(key);
		}
		else
		{
			keyCount--;
		}
	}
}
