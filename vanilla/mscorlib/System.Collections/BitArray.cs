using System.Runtime.InteropServices;
using System.Threading;

namespace System.Collections;

/// <summary>Manages a compact array of bit values, which are represented as Booleans, where true indicates that the bit is on (1) and false indicates the bit is off (0).</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
[ComVisible(true)]
public sealed class BitArray : ICollection, IEnumerable, ICloneable
{
	[Serializable]
	private class BitArrayEnumeratorSimple : IEnumerator, ICloneable
	{
		private BitArray bitarray;

		private int index;

		private int version;

		private bool currentElement;

		public virtual object Current
		{
			get
			{
				if (index == -1)
				{
					throw new InvalidOperationException(Environment.GetResourceString("Enumeration has not started. Call MoveNext."));
				}
				if (index >= bitarray.Count)
				{
					throw new InvalidOperationException(Environment.GetResourceString("Enumeration already finished."));
				}
				return currentElement;
			}
		}

		internal BitArrayEnumeratorSimple(BitArray bitarray)
		{
			this.bitarray = bitarray;
			index = -1;
			version = bitarray._version;
		}

		public object Clone()
		{
			return MemberwiseClone();
		}

		public virtual bool MoveNext()
		{
			if (version != bitarray._version)
			{
				throw new InvalidOperationException(Environment.GetResourceString("Collection was modified; enumeration operation may not execute."));
			}
			if (index < bitarray.Count - 1)
			{
				index++;
				currentElement = bitarray.Get(index);
				return true;
			}
			index = bitarray.Count;
			return false;
		}

		public void Reset()
		{
			if (version != bitarray._version)
			{
				throw new InvalidOperationException(Environment.GetResourceString("Collection was modified; enumeration operation may not execute."));
			}
			index = -1;
		}
	}

	private const int BitsPerInt32 = 32;

	private const int BytesPerInt32 = 4;

	private const int BitsPerByte = 8;

	private int[] m_array;

	private int m_length;

	private int _version;

	[NonSerialized]
	private object _syncRoot;

	private const int _ShrinkThreshold = 256;

	/// <summary>Gets or sets the value of the bit at a specific position in the <see cref="T:System.Collections.BitArray" />.</summary>
	/// <returns>The value of the bit at position <paramref name="index" />.</returns>
	/// <param name="index">The zero-based index of the value to get or set. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.-or- <paramref name="index" /> is equal to or greater than <see cref="P:System.Collections.BitArray.Count" />. </exception>
	/// <filterpriority>2</filterpriority>
	public bool this[int index]
	{
		get
		{
			return Get(index);
		}
		set
		{
			Set(index, value);
		}
	}

	/// <summary>Gets or sets the number of elements in the <see cref="T:System.Collections.BitArray" />.</summary>
	/// <returns>The number of elements in the <see cref="T:System.Collections.BitArray" />.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The property is set to a value that is less than zero. </exception>
	/// <filterpriority>2</filterpriority>
	public int Length
	{
		get
		{
			return m_length;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("Non-negative number required."));
			}
			int arrayLength = GetArrayLength(value, 32);
			if (arrayLength > m_array.Length || arrayLength + 256 < m_array.Length)
			{
				int[] array = new int[arrayLength];
				Array.Copy(m_array, array, (arrayLength > m_array.Length) ? m_array.Length : arrayLength);
				m_array = array;
			}
			if (value > m_length)
			{
				int num = GetArrayLength(m_length, 32) - 1;
				int num2 = m_length % 32;
				if (num2 > 0)
				{
					m_array[num] &= (1 << num2) - 1;
				}
				Array.Clear(m_array, num + 1, arrayLength - num - 1);
			}
			m_length = value;
			_version++;
		}
	}

	/// <summary>Gets the number of elements contained in the <see cref="T:System.Collections.BitArray" />.</summary>
	/// <returns>The number of elements contained in the <see cref="T:System.Collections.BitArray" />.</returns>
	/// <filterpriority>2</filterpriority>
	public int Count => m_length;

	/// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.BitArray" />.</summary>
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.BitArray" />.</returns>
	/// <filterpriority>2</filterpriority>
	public object SyncRoot
	{
		get
		{
			if (_syncRoot == null)
			{
				Interlocked.CompareExchange<object>(ref _syncRoot, new object(), (object)null);
			}
			return _syncRoot;
		}
	}

	/// <summary>Gets a value indicating whether the <see cref="T:System.Collections.BitArray" /> is read-only.</summary>
	/// <returns>This property is always false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsReadOnly => false;

	/// <summary>Gets a value indicating whether access to the <see cref="T:System.Collections.BitArray" /> is synchronized (thread safe).</summary>
	/// <returns>This property is always false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsSynchronized => false;

	private BitArray()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.BitArray" /> class that can hold the specified number of bit values, which are initially set to false.</summary>
	/// <param name="length">The number of bit values in the new <see cref="T:System.Collections.BitArray" />. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="length" /> is less than zero. </exception>
	public BitArray(int length)
		: this(length, defaultValue: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.BitArray" /> class that can hold the specified number of bit values, which are initially set to the specified value.</summary>
	/// <param name="length">The number of bit values in the new <see cref="T:System.Collections.BitArray" />. </param>
	/// <param name="defaultValue">The Boolean value to assign to each bit. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="length" /> is less than zero. </exception>
	public BitArray(int length, bool defaultValue)
	{
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("Non-negative number required."));
		}
		m_array = new int[GetArrayLength(length, 32)];
		m_length = length;
		int num = (defaultValue ? (-1) : 0);
		for (int i = 0; i < m_array.Length; i++)
		{
			m_array[i] = num;
		}
		_version = 0;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.BitArray" /> class that contains bit values copied from the specified array of bytes.</summary>
	/// <param name="bytes">An array of bytes containing the values to copy, where each byte represents eight consecutive bits. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="bytes" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">The length of <paramref name="bytes" /> is greater than <see cref="F:System.Int32.MaxValue" />.</exception>
	public BitArray(byte[] bytes)
	{
		if (bytes == null)
		{
			throw new ArgumentNullException("bytes");
		}
		if (bytes.Length > 268435455)
		{
			throw new ArgumentException(Environment.GetResourceString("The input array length must not exceed Int32.MaxValue / {0}. Otherwise BitArray.Length would exceed Int32.MaxValue.", 8), "bytes");
		}
		m_array = new int[GetArrayLength(bytes.Length, 4)];
		m_length = bytes.Length * 8;
		int num = 0;
		int i;
		for (i = 0; bytes.Length - i >= 4; i += 4)
		{
			m_array[num++] = (bytes[i] & 0xFF) | ((bytes[i + 1] & 0xFF) << 8) | ((bytes[i + 2] & 0xFF) << 16) | ((bytes[i + 3] & 0xFF) << 24);
		}
		switch (bytes.Length - i)
		{
		case 3:
			m_array[num] = (bytes[i + 2] & 0xFF) << 16;
			goto case 2;
		case 2:
			m_array[num] |= (bytes[i + 1] & 0xFF) << 8;
			goto case 1;
		case 1:
			m_array[num] |= bytes[i] & 0xFF;
			break;
		}
		_version = 0;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.BitArray" /> class that contains bit values copied from the specified array of Booleans.</summary>
	/// <param name="values">An array of Booleans to copy. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="values" /> is null. </exception>
	public BitArray(bool[] values)
	{
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		m_array = new int[GetArrayLength(values.Length, 32)];
		m_length = values.Length;
		for (int i = 0; i < values.Length; i++)
		{
			if (values[i])
			{
				m_array[i / 32] |= 1 << i % 32;
			}
		}
		_version = 0;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.BitArray" /> class that contains bit values copied from the specified array of 32-bit integers.</summary>
	/// <param name="values">An array of integers containing the values to copy, where each integer represents 32 consecutive bits. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="values" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">The length of <paramref name="values" /> is greater than <see cref="F:System.Int32.MaxValue" /></exception>
	public BitArray(int[] values)
	{
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		if (values.Length > 67108863)
		{
			throw new ArgumentException(Environment.GetResourceString("The input array length must not exceed Int32.MaxValue / {0}. Otherwise BitArray.Length would exceed Int32.MaxValue.", 32), "values");
		}
		m_array = new int[values.Length];
		m_length = values.Length * 32;
		Array.Copy(values, m_array, values.Length);
		_version = 0;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.BitArray" /> class that contains bit values copied from the specified <see cref="T:System.Collections.BitArray" />.</summary>
	/// <param name="bits">The <see cref="T:System.Collections.BitArray" /> to copy. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="bits" /> is null. </exception>
	public BitArray(BitArray bits)
	{
		if (bits == null)
		{
			throw new ArgumentNullException("bits");
		}
		int arrayLength = GetArrayLength(bits.m_length, 32);
		m_array = new int[arrayLength];
		m_length = bits.m_length;
		Array.Copy(bits.m_array, m_array, arrayLength);
		_version = bits._version;
	}

	/// <summary>Gets the value of the bit at a specific position in the <see cref="T:System.Collections.BitArray" />.</summary>
	/// <returns>The value of the bit at position <paramref name="index" />.</returns>
	/// <param name="index">The zero-based index of the value to get. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.-or- <paramref name="index" /> is greater than or equal to the number of elements in the <see cref="T:System.Collections.BitArray" />. </exception>
	/// <filterpriority>2</filterpriority>
	public bool Get(int index)
	{
		if (index < 0 || index >= Length)
		{
			throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		return (m_array[index / 32] & (1 << index % 32)) != 0;
	}

	/// <summary>Sets the bit at a specific position in the <see cref="T:System.Collections.BitArray" /> to the specified value.</summary>
	/// <param name="index">The zero-based index of the bit to set. </param>
	/// <param name="value">The Boolean value to assign to the bit. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.-or- <paramref name="index" /> is greater than or equal to the number of elements in the <see cref="T:System.Collections.BitArray" />. </exception>
	/// <filterpriority>2</filterpriority>
	public void Set(int index, bool value)
	{
		if (index < 0 || index >= Length)
		{
			throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		if (value)
		{
			m_array[index / 32] |= 1 << index % 32;
		}
		else
		{
			m_array[index / 32] &= ~(1 << index % 32);
		}
		_version++;
	}

	/// <summary>Sets all bits in the <see cref="T:System.Collections.BitArray" /> to the specified value.</summary>
	/// <param name="value">The Boolean value to assign to all bits. </param>
	/// <filterpriority>2</filterpriority>
	public void SetAll(bool value)
	{
		int num = (value ? (-1) : 0);
		int arrayLength = GetArrayLength(m_length, 32);
		for (int i = 0; i < arrayLength; i++)
		{
			m_array[i] = num;
		}
		_version++;
	}

	/// <summary>Performs the bitwise AND operation on the elements in the current <see cref="T:System.Collections.BitArray" /> against the corresponding elements in the specified <see cref="T:System.Collections.BitArray" />.</summary>
	/// <returns>The current instance containing the result of the bitwise AND operation on the elements in the current <see cref="T:System.Collections.BitArray" /> against the corresponding elements in the specified <see cref="T:System.Collections.BitArray" />.</returns>
	/// <param name="value">The <see cref="T:System.Collections.BitArray" /> with which to perform the bitwise AND operation. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> and the current <see cref="T:System.Collections.BitArray" /> do not have the same number of elements. </exception>
	/// <filterpriority>2</filterpriority>
	public BitArray And(BitArray value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (Length != value.Length)
		{
			throw new ArgumentException(Environment.GetResourceString("Array lengths must be the same."));
		}
		int arrayLength = GetArrayLength(m_length, 32);
		for (int i = 0; i < arrayLength; i++)
		{
			m_array[i] &= value.m_array[i];
		}
		_version++;
		return this;
	}

	/// <summary>Performs the bitwise OR operation on the elements in the current <see cref="T:System.Collections.BitArray" /> against the corresponding elements in the specified <see cref="T:System.Collections.BitArray" />.</summary>
	/// <returns>The current instance containing the result of the bitwise OR operation on the elements in the current <see cref="T:System.Collections.BitArray" /> against the corresponding elements in the specified <see cref="T:System.Collections.BitArray" />.</returns>
	/// <param name="value">The <see cref="T:System.Collections.BitArray" /> with which to perform the bitwise OR operation. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> and the current <see cref="T:System.Collections.BitArray" /> do not have the same number of elements. </exception>
	/// <filterpriority>2</filterpriority>
	public BitArray Or(BitArray value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (Length != value.Length)
		{
			throw new ArgumentException(Environment.GetResourceString("Array lengths must be the same."));
		}
		int arrayLength = GetArrayLength(m_length, 32);
		for (int i = 0; i < arrayLength; i++)
		{
			m_array[i] |= value.m_array[i];
		}
		_version++;
		return this;
	}

	/// <summary>Performs the bitwise exclusive OR operation on the elements in the current <see cref="T:System.Collections.BitArray" /> against the corresponding elements in the specified <see cref="T:System.Collections.BitArray" />.</summary>
	/// <returns>The current instance containing the result of the bitwise exclusive OR operation on the elements in the current <see cref="T:System.Collections.BitArray" /> against the corresponding elements in the specified <see cref="T:System.Collections.BitArray" />. </returns>
	/// <param name="value">The <see cref="T:System.Collections.BitArray" /> with which to perform the bitwise exclusive OR operation. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> and the current <see cref="T:System.Collections.BitArray" /> do not have the same number of elements. </exception>
	/// <filterpriority>2</filterpriority>
	public BitArray Xor(BitArray value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (Length != value.Length)
		{
			throw new ArgumentException(Environment.GetResourceString("Array lengths must be the same."));
		}
		int arrayLength = GetArrayLength(m_length, 32);
		for (int i = 0; i < arrayLength; i++)
		{
			m_array[i] ^= value.m_array[i];
		}
		_version++;
		return this;
	}

	/// <summary>Inverts all the bit values in the current <see cref="T:System.Collections.BitArray" />, so that elements set to true are changed to false, and elements set to false are changed to true.</summary>
	/// <returns>The current instance with inverted bit values.</returns>
	/// <filterpriority>2</filterpriority>
	public BitArray Not()
	{
		int arrayLength = GetArrayLength(m_length, 32);
		for (int i = 0; i < arrayLength; i++)
		{
			m_array[i] = ~m_array[i];
		}
		_version++;
		return this;
	}

	/// <summary>Copies the entire <see cref="T:System.Collections.BitArray" /> to a compatible one-dimensional <see cref="T:System.Array" />, starting at the specified index of the target array.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.BitArray" />. The <see cref="T:System.Array" /> must have zero-based indexing. </param>
	/// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="array" /> is multidimensional.-or- The number of elements in the source <see cref="T:System.Collections.BitArray" /> is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />. </exception>
	/// <exception cref="T:System.InvalidCastException">The type of the source <see cref="T:System.Collections.BitArray" /> cannot be cast automatically to the type of the destination <paramref name="array" />. </exception>
	/// <filterpriority>2</filterpriority>
	public void CopyTo(Array array, int index)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("Non-negative number required."));
		}
		if (array.Rank != 1)
		{
			throw new ArgumentException(Environment.GetResourceString("Only single dimensional arrays are supported for the requested action."));
		}
		if (array is int[])
		{
			Array.Copy(m_array, 0, array, index, GetArrayLength(m_length, 32));
			return;
		}
		if (array is byte[])
		{
			int arrayLength = GetArrayLength(m_length, 8);
			if (array.Length - index < arrayLength)
			{
				throw new ArgumentException(Environment.GetResourceString("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection."));
			}
			byte[] array2 = (byte[])array;
			for (int i = 0; i < arrayLength; i++)
			{
				array2[index + i] = (byte)((m_array[i / 4] >> i % 4 * 8) & 0xFF);
			}
			return;
		}
		if (array is bool[])
		{
			if (array.Length - index < m_length)
			{
				throw new ArgumentException(Environment.GetResourceString("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection."));
			}
			bool[] array3 = (bool[])array;
			for (int j = 0; j < m_length; j++)
			{
				array3[index + j] = ((m_array[j / 32] >> j % 32) & 1) != 0;
			}
			return;
		}
		throw new ArgumentException(Environment.GetResourceString("Only supported array types for CopyTo on BitArrays are Boolean[], Int32[] and Byte[]."));
	}

	/// <summary>Creates a shallow copy of the <see cref="T:System.Collections.BitArray" />.</summary>
	/// <returns>A shallow copy of the <see cref="T:System.Collections.BitArray" />.</returns>
	/// <filterpriority>2</filterpriority>
	public object Clone()
	{
		return new BitArray(m_array)
		{
			_version = _version,
			m_length = m_length
		};
	}

	/// <summary>Returns an enumerator that iterates through the <see cref="T:System.Collections.BitArray" />.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> for the entire <see cref="T:System.Collections.BitArray" />.</returns>
	/// <filterpriority>2</filterpriority>
	public IEnumerator GetEnumerator()
	{
		return new BitArrayEnumeratorSimple(this);
	}

	private static int GetArrayLength(int n, int div)
	{
		if (n <= 0)
		{
			return 0;
		}
		return (n - 1) / div + 1;
	}
}
