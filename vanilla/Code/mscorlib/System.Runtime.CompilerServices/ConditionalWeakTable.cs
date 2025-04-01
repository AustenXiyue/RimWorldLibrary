using System.Collections;
using System.Collections.Generic;
using System.Security;

namespace System.Runtime.CompilerServices;

/// <summary>Enables compilers to dynamically attach object fields to managed objects.</summary>
/// <typeparam name="TKey">The reference type to which the field is attached. </typeparam>
/// <typeparam name="TValue">The field's type. This must be a reference type.</typeparam>
public sealed class ConditionalWeakTable<TKey, TValue> where TKey : class where TValue : class
{
	/// <summary>Represents a method that creates a non-default value to add as part of a key/value pair to a <see cref="T:System.Runtime.CompilerServices.ConditionalWeakTable`2" /> object. </summary>
	/// <returns>An instance of a reference type that represents the value to attach to the specified key.</returns>
	/// <param name="key">The key that belongs to the value to create.</param>
	public delegate TValue CreateValueCallback(TKey key);

	private const int INITIAL_SIZE = 13;

	private const float LOAD_FACTOR = 0.7f;

	private const float COMPACT_FACTOR = 0.5f;

	private const float EXPAND_FACTOR = 1.1f;

	private Ephemeron[] data;

	private object _lock = new object();

	private int size;

	internal ICollection<TKey> Keys
	{
		[SecuritySafeCritical]
		get
		{
			object ePHEMERON_TOMBSTONE = GC.EPHEMERON_TOMBSTONE;
			List<TKey> list = new List<TKey>(data.Length);
			lock (_lock)
			{
				for (int i = 0; i < data.Length; i++)
				{
					TKey val = (TKey)data[i].key;
					if (val != null && val != ePHEMERON_TOMBSTONE)
					{
						list.Add(val);
					}
				}
				return list;
			}
		}
	}

	internal ICollection<TValue> Values
	{
		[SecuritySafeCritical]
		get
		{
			object ePHEMERON_TOMBSTONE = GC.EPHEMERON_TOMBSTONE;
			List<TValue> list = new List<TValue>(data.Length);
			lock (_lock)
			{
				for (int i = 0; i < data.Length; i++)
				{
					Ephemeron ephemeron = data[i];
					if (ephemeron.key != null && ephemeron.key != ePHEMERON_TOMBSTONE)
					{
						list.Add((TValue)ephemeron.value);
					}
				}
				return list;
			}
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.CompilerServices.ConditionalWeakTable`2" /> class.</summary>
	public ConditionalWeakTable()
	{
		data = new Ephemeron[13];
		GC.register_ephemeron_array(data);
	}

	~ConditionalWeakTable()
	{
	}

	private void RehashWithoutResize()
	{
		int num = data.Length;
		for (int i = 0; i < num; i++)
		{
			if (data[i].key == GC.EPHEMERON_TOMBSTONE)
			{
				data[i].key = null;
			}
		}
		for (int j = 0; j < num; j++)
		{
			object key = data[j].key;
			if (key == null)
			{
				continue;
			}
			int num2 = (RuntimeHelpers.GetHashCode(key) & 0x7FFFFFFF) % num;
			while (true)
			{
				if (data[num2].key == null)
				{
					data[num2].key = key;
					data[num2].value = data[j].value;
					data[j].key = null;
					data[j].value = null;
					break;
				}
				if (data[num2].key == key)
				{
					break;
				}
				if (++num2 == num)
				{
					num2 = 0;
				}
			}
		}
	}

	private void RecomputeSize()
	{
		size = 0;
		for (int i = 0; i < data.Length; i++)
		{
			if (data[i].key != null)
			{
				size++;
			}
		}
	}

	private void Rehash()
	{
		RecomputeSize();
		uint prime = (uint)HashHelpers.GetPrime(((int)((float)size / 0.7f) << 1) | 1);
		if ((float)prime > (float)data.Length * 0.5f && (float)prime < (float)data.Length * 1.1f)
		{
			RehashWithoutResize();
			return;
		}
		Ephemeron[] array = new Ephemeron[prime];
		GC.register_ephemeron_array(array);
		size = 0;
		for (int i = 0; i < data.Length; i++)
		{
			object key = data[i].key;
			object value = data[i].value;
			if (key == null || key == GC.EPHEMERON_TOMBSTONE)
			{
				continue;
			}
			int num = array.Length;
			int num2 = -1;
			int num3;
			int num4 = (num3 = (RuntimeHelpers.GetHashCode(key) & 0x7FFFFFFF) % num);
			do
			{
				object key2 = array[num4].key;
				if (key2 == null || key2 == GC.EPHEMERON_TOMBSTONE)
				{
					num2 = num4;
					break;
				}
				if (++num4 == num)
				{
					num4 = 0;
				}
			}
			while (num4 != num3);
			array[num2].key = key;
			array[num2].value = value;
			size++;
		}
		data = array;
	}

	/// <summary>Adds a key to the table.</summary>
	/// <param name="key">The key to add. <paramref name="key" /> represents the object to which the property is attached.</param>
	/// <param name="value">The key's property value.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="key" /> already exists.</exception>
	public void Add(TKey key, TValue value)
	{
		if (key == null)
		{
			throw new ArgumentNullException("Null key", "key");
		}
		lock (_lock)
		{
			if ((float)size >= (float)data.Length * 0.7f)
			{
				Rehash();
			}
			int num = data.Length;
			int num2 = -1;
			int num3;
			int num4 = (num3 = (RuntimeHelpers.GetHashCode(key) & 0x7FFFFFFF) % num);
			do
			{
				object key2 = data[num4].key;
				if (key2 == null)
				{
					if (num2 == -1)
					{
						num2 = num4;
					}
					break;
				}
				if (key2 == GC.EPHEMERON_TOMBSTONE && num2 == -1)
				{
					num2 = num4;
				}
				else if (key2 == key)
				{
					throw new ArgumentException("Key already in the list", "key");
				}
				if (++num4 == num)
				{
					num4 = 0;
				}
			}
			while (num4 != num3);
			data[num2].key = key;
			data[num2].value = value;
			size++;
		}
	}

	/// <summary>Removes a key and its value from the table.</summary>
	/// <returns>true if the key is found and removed; otherwise, false.</returns>
	/// <param name="key">The key to remove. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	public bool Remove(TKey key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("Null key", "key");
		}
		lock (_lock)
		{
			int num = data.Length;
			int num2;
			int num3 = (num2 = (RuntimeHelpers.GetHashCode(key) & 0x7FFFFFFF) % num);
			do
			{
				object key2 = data[num3].key;
				if (key2 == key)
				{
					data[num3].key = GC.EPHEMERON_TOMBSTONE;
					data[num3].value = null;
					size--;
					return true;
				}
				if (key2 == null)
				{
					break;
				}
				if (++num3 == num)
				{
					num3 = 0;
				}
			}
			while (num3 != num2);
		}
		return false;
	}

	/// <summary>Gets the value of the specified key.</summary>
	/// <returns>true if <paramref name="key" /> is found; otherwise, false.</returns>
	/// <param name="key">The key that represents an object with an attached property.</param>
	/// <param name="value">When this method returns, contains the attached property value. If <paramref name="key" /> is not found, <paramref name="value" /> contains the default value.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	public bool TryGetValue(TKey key, out TValue value)
	{
		if (key == null)
		{
			throw new ArgumentNullException("Null key", "key");
		}
		value = null;
		lock (_lock)
		{
			int num = data.Length;
			int num2;
			int num3 = (num2 = (RuntimeHelpers.GetHashCode(key) & 0x7FFFFFFF) % num);
			do
			{
				object key2 = data[num3].key;
				if (key2 == key)
				{
					value = (TValue)data[num3].value;
					return true;
				}
				if (key2 == null)
				{
					break;
				}
				if (++num3 == num)
				{
					num3 = 0;
				}
			}
			while (num3 != num2);
		}
		return false;
	}

	/// <summary>Atomically searches for a specified key in the table and returns the corresponding value. If the key does not exist in the table, the method invokes the default constructor of the class that represents the table's value to create a value that is bound to the specified key. </summary>
	/// <returns>The value that corresponds to <paramref name="key" />, if <paramref name="key" /> already exists in the table; otherwise, a new value created by the default constructor of the class defined by the <paramref name="TValue" /> generic type parameter.</returns>
	/// <param name="key">The key to search for. <paramref name="key" /> represents the object to which the property is attached.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	/// <exception cref="T:System.MissingMethodException">NoteIn the .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.MissingMemberException" />, instead.The class that represents the table's value does not define a default constructor.</exception>
	public TValue GetOrCreateValue(TKey key)
	{
		return GetValue(key, (TKey k) => Activator.CreateInstance<TValue>());
	}

	/// <summary>Atomically searches for a specified key in the table and returns the corresponding value. If the key does not exist in the table, the method invokes a callback method to create a value that is bound to the specified key.</summary>
	/// <returns>The value attached to <paramref name="key" />, if <paramref name="key" /> already exists in the table; otherwise, the new value returned by the <paramref name="createValueCallback" /> delegate.</returns>
	/// <param name="key">The key to search for. <paramref name="key" /> represents the object to which the property is attached.</param>
	/// <param name="createValueCallback">A delegate to a method that can create a value for the given <paramref name="key" />. It has a single parameter of type <paramref name="TKey" />, and returns a value of type <paramref name="TValue" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> or <paramref name="createValueCallback" /> is null.</exception>
	public TValue GetValue(TKey key, CreateValueCallback createValueCallback)
	{
		if (createValueCallback == null)
		{
			throw new ArgumentNullException("Null create delegate", "createValueCallback");
		}
		lock (_lock)
		{
			if (TryGetValue(key, out var value))
			{
				return value;
			}
			value = createValueCallback(key);
			Add(key, value);
			return value;
		}
	}

	[FriendAccessAllowed]
	[SecuritySafeCritical]
	internal TKey FindEquivalentKeyUnsafe(TKey key, out TValue value)
	{
		lock (_lock)
		{
			for (int i = 0; i < data.Length; i++)
			{
				Ephemeron ephemeron = data[i];
				if (object.Equals(ephemeron.key, key))
				{
					value = (TValue)ephemeron.value;
					return (TKey)ephemeron.key;
				}
			}
		}
		value = null;
		return null;
	}

	[SecuritySafeCritical]
	internal void Clear()
	{
		lock (_lock)
		{
			for (int i = 0; i < data.Length; i++)
			{
				data[i].key = GC.EPHEMERON_TOMBSTONE;
				data[i].value = null;
			}
			size = 0;
		}
	}
}
