using System.Globalization;
using System.Runtime.Serialization;
using System.Text;

namespace System.Collections.Specialized;

/// <summary>Represents a collection of associated <see cref="T:System.String" /> keys and <see cref="T:System.String" /> values that can be accessed either with the key or with the index.</summary>
[Serializable]
public class NameValueCollection : NameObjectCollectionBase
{
	private string[] _all;

	private string[] _allKeys;

	/// <summary>Gets or sets the entry with the specified key in the <see cref="T:System.Collections.Specialized.NameValueCollection" />.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the comma-separated list of values associated with the specified key, if found; otherwise, null.</returns>
	/// <param name="name">The <see cref="T:System.String" /> key of the entry to locate. The key can be null.</param>
	/// <exception cref="T:System.NotSupportedException">The collection is read-only and the operation attempts to modify the collection. </exception>
	public string this[string name]
	{
		get
		{
			return Get(name);
		}
		set
		{
			Set(name, value);
		}
	}

	/// <summary>Gets the entry at the specified index of the <see cref="T:System.Collections.Specialized.NameValueCollection" />.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the comma-separated list of values at the specified index of the collection.</returns>
	/// <param name="index">The zero-based index of the entry to locate in the collection.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is outside the valid range of indexes for the collection.</exception>
	public string this[int index] => Get(index);

	/// <summary>Gets all the keys in the <see cref="T:System.Collections.Specialized.NameValueCollection" />.</summary>
	/// <returns>A <see cref="T:System.String" /> array that contains all the keys of the <see cref="T:System.Collections.Specialized.NameValueCollection" />.</returns>
	public virtual string[] AllKeys
	{
		get
		{
			if (_allKeys == null)
			{
				_allKeys = BaseGetAllKeys();
			}
			return _allKeys;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Specialized.NameValueCollection" /> class that is empty, has the default initial capacity and uses the default case-insensitive hash code provider and the default case-insensitive comparer.</summary>
	public NameValueCollection()
	{
	}

	/// <summary>Copies the entries from the specified <see cref="T:System.Collections.Specialized.NameValueCollection" /> to a new <see cref="T:System.Collections.Specialized.NameValueCollection" /> with the same initial capacity as the number of entries copied and using the same hash code provider and the same comparer as the source collection.</summary>
	/// <param name="col">The <see cref="T:System.Collections.Specialized.NameValueCollection" /> to copy to the new <see cref="T:System.Collections.Specialized.NameValueCollection" /> instance.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="col" /> is null.</exception>
	public NameValueCollection(NameValueCollection col)
		: base(col?.Comparer)
	{
		Add(col);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Specialized.NameValueCollection" /> class that is empty, has the default initial capacity and uses the specified hash code provider and the specified comparer.</summary>
	/// <param name="hashProvider">The <see cref="T:System.Collections.IHashCodeProvider" /> that will supply the hash codes for all keys in the <see cref="T:System.Collections.Specialized.NameValueCollection" />.</param>
	/// <param name="comparer">The <see cref="T:System.Collections.IComparer" /> to use to determine whether two keys are equal.</param>
	[Obsolete("Please use NameValueCollection(IEqualityComparer) instead.")]
	public NameValueCollection(IHashCodeProvider hashProvider, IComparer comparer)
		: base(hashProvider, comparer)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Specialized.NameValueCollection" /> class that is empty, has the specified initial capacity and uses the default case-insensitive hash code provider and the default case-insensitive comparer.</summary>
	/// <param name="capacity">The initial number of entries that the <see cref="T:System.Collections.Specialized.NameValueCollection" /> can contain.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="capacity" /> is less than zero.</exception>
	public NameValueCollection(int capacity)
		: base(capacity)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Specialized.NameValueCollection" /> class that is empty, has the default initial capacity, and uses the specified <see cref="T:System.Collections.IEqualityComparer" /> object.</summary>
	/// <param name="equalityComparer">The <see cref="T:System.Collections.IEqualityComparer" /> object to use to determine whether two keys are equal and to generate hash codes for the keys in the collection.</param>
	public NameValueCollection(IEqualityComparer equalityComparer)
		: base(equalityComparer)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Specialized.NameValueCollection" /> class that is empty, has the specified initial capacity, and uses the specified <see cref="T:System.Collections.IEqualityComparer" /> object.</summary>
	/// <param name="capacity">The initial number of entries that the <see cref="T:System.Collections.Specialized.NameValueCollection" /> object can contain.</param>
	/// <param name="equalityComparer">The <see cref="T:System.Collections.IEqualityComparer" /> object to use to determine whether two keys are equal and to generate hash codes for the keys in the collection.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="capacity" /> is less than zero.</exception>
	public NameValueCollection(int capacity, IEqualityComparer equalityComparer)
		: base(capacity, equalityComparer)
	{
	}

	/// <summary>Copies the entries from the specified <see cref="T:System.Collections.Specialized.NameValueCollection" /> to a new <see cref="T:System.Collections.Specialized.NameValueCollection" /> with the specified initial capacity or the same initial capacity as the number of entries copied, whichever is greater, and using the default case-insensitive hash code provider and the default case-insensitive comparer.</summary>
	/// <param name="capacity">The initial number of entries that the <see cref="T:System.Collections.Specialized.NameValueCollection" /> can contain.</param>
	/// <param name="col">The <see cref="T:System.Collections.Specialized.NameValueCollection" /> to copy to the new <see cref="T:System.Collections.Specialized.NameValueCollection" /> instance.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="capacity" /> is less than zero.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="col" /> is null.</exception>
	public NameValueCollection(int capacity, NameValueCollection col)
		: base(capacity, col?.Comparer)
	{
		if (col == null)
		{
			throw new ArgumentNullException("col");
		}
		base.Comparer = col.Comparer;
		Add(col);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Specialized.NameValueCollection" /> class that is empty, has the specified initial capacity and uses the specified hash code provider and the specified comparer.</summary>
	/// <param name="capacity">The initial number of entries that the <see cref="T:System.Collections.Specialized.NameValueCollection" /> can contain.</param>
	/// <param name="hashProvider">The <see cref="T:System.Collections.IHashCodeProvider" /> that will supply the hash codes for all keys in the <see cref="T:System.Collections.Specialized.NameValueCollection" />.</param>
	/// <param name="comparer">The <see cref="T:System.Collections.IComparer" /> to use to determine whether two keys are equal.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="capacity" /> is less than zero.</exception>
	[Obsolete("Please use NameValueCollection(Int32, IEqualityComparer) instead.")]
	public NameValueCollection(int capacity, IHashCodeProvider hashProvider, IComparer comparer)
		: base(capacity, hashProvider, comparer)
	{
	}

	internal NameValueCollection(DBNull dummy)
		: base(dummy)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Specialized.NameValueCollection" /> class that is serializable and uses the specified <see cref="T:System.Runtime.Serialization.SerializationInfo" /> and <see cref="T:System.Runtime.Serialization.StreamingContext" />.</summary>
	/// <param name="info">A <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object that contains the information required to serialize the new <see cref="T:System.Collections.Specialized.NameValueCollection" /> instance.</param>
	/// <param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> object that contains the source and destination of the serialized stream associated with the new <see cref="T:System.Collections.Specialized.NameValueCollection" /> instance.</param>
	protected NameValueCollection(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	/// <summary>Resets the cached arrays of the collection to null.</summary>
	protected void InvalidateCachedArrays()
	{
		_all = null;
		_allKeys = null;
	}

	private static string GetAsOneString(ArrayList list)
	{
		int num = list?.Count ?? 0;
		if (num == 1)
		{
			return (string)list[0];
		}
		if (num > 1)
		{
			StringBuilder stringBuilder = new StringBuilder((string)list[0]);
			for (int i = 1; i < num; i++)
			{
				stringBuilder.Append(',');
				stringBuilder.Append((string)list[i]);
			}
			return stringBuilder.ToString();
		}
		return null;
	}

	private static string[] GetAsStringArray(ArrayList list)
	{
		int num = list?.Count ?? 0;
		if (num == 0)
		{
			return null;
		}
		string[] array = new string[num];
		list.CopyTo(0, array, 0, num);
		return array;
	}

	/// <summary>Copies the entries in the specified <see cref="T:System.Collections.Specialized.NameValueCollection" /> to the current <see cref="T:System.Collections.Specialized.NameValueCollection" />.</summary>
	/// <param name="c">The <see cref="T:System.Collections.Specialized.NameValueCollection" /> to copy to the current <see cref="T:System.Collections.Specialized.NameValueCollection" />.</param>
	/// <exception cref="T:System.NotSupportedException">The collection is read-only.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="c" /> is null.</exception>
	public void Add(NameValueCollection c)
	{
		if (c == null)
		{
			throw new ArgumentNullException("c");
		}
		InvalidateCachedArrays();
		int count = c.Count;
		for (int i = 0; i < count; i++)
		{
			string key = c.GetKey(i);
			string[] values = c.GetValues(i);
			if (values != null)
			{
				for (int j = 0; j < values.Length; j++)
				{
					Add(key, values[j]);
				}
			}
			else
			{
				Add(key, null);
			}
		}
	}

	/// <summary>Invalidates the cached arrays and removes all entries from the <see cref="T:System.Collections.Specialized.NameValueCollection" />.</summary>
	/// <exception cref="T:System.NotSupportedException">The collection is read-only.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public virtual void Clear()
	{
		if (base.IsReadOnly)
		{
			throw new NotSupportedException(global::SR.GetString("Collection is read-only."));
		}
		InvalidateCachedArrays();
		BaseClear();
	}

	/// <summary>Copies the entire <see cref="T:System.Collections.Specialized.NameValueCollection" /> to a compatible one-dimensional <see cref="T:System.Array" />, starting at the specified index of the target array.</summary>
	/// <param name="dest">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Specialized.NameValueCollection" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
	/// <param name="index">The zero-based index in <paramref name="dest" /> at which copying begins.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="dest" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="dest" /> is multidimensional.-or- The number of elements in the source <see cref="T:System.Collections.Specialized.NameValueCollection" /> is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="dest" />.</exception>
	/// <exception cref="T:System.InvalidCastException">The type of the source <see cref="T:System.Collections.Specialized.NameValueCollection" /> cannot be cast automatically to the type of the destination <paramref name="dest" />.</exception>
	public void CopyTo(Array dest, int index)
	{
		if (dest == null)
		{
			throw new ArgumentNullException("dest");
		}
		if (dest.Rank != 1)
		{
			throw new ArgumentException(global::SR.GetString("Multi dimension array is not supported on this operation."));
		}
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index", global::SR.GetString("Index {0} is out of range.", index.ToString(CultureInfo.CurrentCulture)));
		}
		if (dest.Length - index < Count)
		{
			throw new ArgumentException(global::SR.GetString("Insufficient space in the target location to copy the information."));
		}
		int count = Count;
		if (_all == null)
		{
			string[] array = new string[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = Get(i);
				dest.SetValue(array[i], i + index);
			}
			_all = array;
		}
		else
		{
			for (int j = 0; j < count; j++)
			{
				dest.SetValue(_all[j], j + index);
			}
		}
	}

	/// <summary>Gets a value indicating whether the <see cref="T:System.Collections.Specialized.NameValueCollection" /> contains keys that are not null.</summary>
	/// <returns>true if the <see cref="T:System.Collections.Specialized.NameValueCollection" /> contains keys that are not null; otherwise, false.</returns>
	public bool HasKeys()
	{
		return InternalHasKeys();
	}

	internal virtual bool InternalHasKeys()
	{
		return BaseHasKeys();
	}

	/// <summary>Adds an entry with the specified name and value to the <see cref="T:System.Collections.Specialized.NameValueCollection" />.</summary>
	/// <param name="name">The <see cref="T:System.String" /> key of the entry to add. The key can be null.</param>
	/// <param name="value">The <see cref="T:System.String" /> value of the entry to add. The value can be null.</param>
	/// <exception cref="T:System.NotSupportedException">The collection is read-only. </exception>
	public virtual void Add(string name, string value)
	{
		if (base.IsReadOnly)
		{
			throw new NotSupportedException(global::SR.GetString("Collection is read-only."));
		}
		InvalidateCachedArrays();
		ArrayList arrayList = (ArrayList)BaseGet(name);
		if (arrayList == null)
		{
			arrayList = new ArrayList(1);
			if (value != null)
			{
				arrayList.Add(value);
			}
			BaseAdd(name, arrayList);
		}
		else if (value != null)
		{
			arrayList.Add(value);
		}
	}

	/// <summary>Gets the values associated with the specified key from the <see cref="T:System.Collections.Specialized.NameValueCollection" /> combined into one comma-separated list.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains a comma-separated list of the values associated with the specified key from the <see cref="T:System.Collections.Specialized.NameValueCollection" />, if found; otherwise, null.</returns>
	/// <param name="name">The <see cref="T:System.String" /> key of the entry that contains the values to get. The key can be null.</param>
	public virtual string Get(string name)
	{
		return GetAsOneString((ArrayList)BaseGet(name));
	}

	/// <summary>Gets the values associated with the specified key from the <see cref="T:System.Collections.Specialized.NameValueCollection" />.</summary>
	/// <returns>A <see cref="T:System.String" /> array that contains the values associated with the specified key from the <see cref="T:System.Collections.Specialized.NameValueCollection" />, if found; otherwise, null.</returns>
	/// <param name="name">The <see cref="T:System.String" /> key of the entry that contains the values to get. The key can be null.</param>
	public virtual string[] GetValues(string name)
	{
		return GetAsStringArray((ArrayList)BaseGet(name));
	}

	/// <summary>Sets the value of an entry in the <see cref="T:System.Collections.Specialized.NameValueCollection" />.</summary>
	/// <param name="name">The <see cref="T:System.String" /> key of the entry to add the new value to. The key can be null.</param>
	/// <param name="value">The <see cref="T:System.Object" /> that represents the new value to add to the specified entry. The value can be null.</param>
	/// <exception cref="T:System.NotSupportedException">The collection is read-only.</exception>
	public virtual void Set(string name, string value)
	{
		if (base.IsReadOnly)
		{
			throw new NotSupportedException(global::SR.GetString("Collection is read-only."));
		}
		InvalidateCachedArrays();
		ArrayList arrayList = new ArrayList(1);
		arrayList.Add(value);
		BaseSet(name, arrayList);
	}

	/// <summary>Removes the entries with the specified key from the <see cref="T:System.Collections.Specialized.NameObjectCollectionBase" /> instance.</summary>
	/// <param name="name">The <see cref="T:System.String" /> key of the entry to remove. The key can be null.</param>
	/// <exception cref="T:System.NotSupportedException">The collection is read-only.</exception>
	public virtual void Remove(string name)
	{
		InvalidateCachedArrays();
		BaseRemove(name);
	}

	/// <summary>Gets the values at the specified index of the <see cref="T:System.Collections.Specialized.NameValueCollection" /> combined into one comma-separated list.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains a comma-separated list of the values at the specified index of the <see cref="T:System.Collections.Specialized.NameValueCollection" />, if found; otherwise, null.</returns>
	/// <param name="index">The zero-based index of the entry that contains the values to get from the collection.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is outside the valid range of indexes for the collection.</exception>
	public virtual string Get(int index)
	{
		return GetAsOneString((ArrayList)BaseGet(index));
	}

	/// <summary>Gets the values at the specified index of the <see cref="T:System.Collections.Specialized.NameValueCollection" />.</summary>
	/// <returns>A <see cref="T:System.String" /> array that contains the values at the specified index of the <see cref="T:System.Collections.Specialized.NameValueCollection" />, if found; otherwise, null.</returns>
	/// <param name="index">The zero-based index of the entry that contains the values to get from the collection.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is outside the valid range of indexes for the collection. </exception>
	public virtual string[] GetValues(int index)
	{
		return GetAsStringArray((ArrayList)BaseGet(index));
	}

	/// <summary>Gets the key at the specified index of the <see cref="T:System.Collections.Specialized.NameValueCollection" />.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the key at the specified index of the <see cref="T:System.Collections.Specialized.NameValueCollection" />, if found; otherwise, null.</returns>
	/// <param name="index">The zero-based index of the key to get from the collection.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is outside the valid range of indexes for the collection. </exception>
	public virtual string GetKey(int index)
	{
		return BaseGetKey(index);
	}
}
