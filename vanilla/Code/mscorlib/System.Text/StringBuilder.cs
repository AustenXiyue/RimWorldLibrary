using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;

namespace System.Text;

/// <summary>Represents a mutable string of characters. This class cannot be inherited.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[StructLayout(LayoutKind.Sequential)]
[ComVisible(true)]
public sealed class StringBuilder : ISerializable
{
	internal char[] m_ChunkChars;

	internal StringBuilder m_ChunkPrevious;

	internal int m_ChunkLength;

	internal int m_ChunkOffset;

	internal int m_MaxCapacity;

	internal const int DefaultCapacity = 16;

	private const string CapacityField = "Capacity";

	private const string MaxCapacityField = "m_MaxCapacity";

	private const string StringValueField = "m_StringValue";

	private const string ThreadIDField = "m_currentThread";

	internal const int MaxChunkSize = 8000;

	/// <summary>Gets or sets the maximum number of characters that can be contained in the memory allocated by the current instance.</summary>
	/// <returns>The maximum number of characters that can be contained in the memory allocated by the current instance.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value specified for a set operation is less than the current length of this instance.-or- The value specified for a set operation is greater than the maximum capacity. </exception>
	/// <filterpriority>2</filterpriority>
	public int Capacity
	{
		get
		{
			return m_ChunkChars.Length + m_ChunkOffset;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("Capacity must be positive."));
			}
			if (value > MaxCapacity)
			{
				throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("Capacity exceeds maximum capacity."));
			}
			if (value < Length)
			{
				throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("capacity was less than the current size."));
			}
			if (Capacity != value)
			{
				char[] array = new char[value - m_ChunkOffset];
				Array.Copy(m_ChunkChars, array, m_ChunkLength);
				m_ChunkChars = array;
			}
		}
	}

	/// <summary>Gets the maximum capacity of this instance.</summary>
	/// <returns>The maximum number of characters this instance can hold.</returns>
	/// <filterpriority>2</filterpriority>
	public int MaxCapacity => m_MaxCapacity;

	/// <summary>Gets or sets the length of the current <see cref="T:System.Text.StringBuilder" /> object.</summary>
	/// <returns>The length of this instance.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value specified for a set operation is less than zero or greater than <see cref="P:System.Text.StringBuilder.MaxCapacity" />. </exception>
	/// <filterpriority>1</filterpriority>
	public int Length
	{
		get
		{
			return m_ChunkOffset + m_ChunkLength;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("Length cannot be less than zero."));
			}
			if (value > MaxCapacity)
			{
				throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("capacity was less than the current size."));
			}
			int capacity = Capacity;
			if (value == 0 && m_ChunkPrevious == null)
			{
				m_ChunkLength = 0;
				m_ChunkOffset = 0;
				return;
			}
			int num = value - Length;
			if (num > 0)
			{
				Append('\0', num);
				return;
			}
			StringBuilder stringBuilder = FindChunkForIndex(value);
			if (stringBuilder != this)
			{
				char[] array = new char[capacity - stringBuilder.m_ChunkOffset];
				Array.Copy(stringBuilder.m_ChunkChars, array, stringBuilder.m_ChunkLength);
				m_ChunkChars = array;
				m_ChunkPrevious = stringBuilder.m_ChunkPrevious;
				m_ChunkOffset = stringBuilder.m_ChunkOffset;
			}
			m_ChunkLength = value - stringBuilder.m_ChunkOffset;
		}
	}

	/// <summary>Gets or sets the character at the specified character position in this instance.</summary>
	/// <returns>The Unicode character at position <paramref name="index" />.</returns>
	/// <param name="index">The position of the character. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is outside the bounds of this instance while setting a character. </exception>
	/// <exception cref="T:System.IndexOutOfRangeException">
	///   <paramref name="index" /> is outside the bounds of this instance while getting a character. </exception>
	/// <filterpriority>2</filterpriority>
	[IndexerName("Chars")]
	public char this[int index]
	{
		get
		{
			StringBuilder stringBuilder = this;
			do
			{
				int num = index - stringBuilder.m_ChunkOffset;
				if (num >= 0)
				{
					if (num >= stringBuilder.m_ChunkLength)
					{
						throw new IndexOutOfRangeException();
					}
					return stringBuilder.m_ChunkChars[num];
				}
				stringBuilder = stringBuilder.m_ChunkPrevious;
			}
			while (stringBuilder != null);
			throw new IndexOutOfRangeException();
		}
		set
		{
			StringBuilder stringBuilder = this;
			do
			{
				int num = index - stringBuilder.m_ChunkOffset;
				if (num >= 0)
				{
					if (num >= stringBuilder.m_ChunkLength)
					{
						throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
					}
					stringBuilder.m_ChunkChars[num] = value;
					return;
				}
				stringBuilder = stringBuilder.m_ChunkPrevious;
			}
			while (stringBuilder != null);
			throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Text.StringBuilder" /> class.</summary>
	public StringBuilder()
		: this(16)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Text.StringBuilder" /> class using the specified capacity.</summary>
	/// <param name="capacity">The suggested starting size of this instance. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="capacity" /> is less than zero. </exception>
	public StringBuilder(int capacity)
		: this(string.Empty, capacity)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Text.StringBuilder" /> class using the specified string.</summary>
	/// <param name="value">The string used to initialize the value of the instance. If <paramref name="value" /> is null, the new <see cref="T:System.Text.StringBuilder" /> will contain the empty string (that is, it contains <see cref="F:System.String.Empty" />). </param>
	public StringBuilder(string value)
		: this(value, 16)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Text.StringBuilder" /> class using the specified string and capacity.</summary>
	/// <param name="value">The string used to initialize the value of the instance. If <paramref name="value" /> is null, the new <see cref="T:System.Text.StringBuilder" /> will contain the empty string (that is, it contains <see cref="F:System.String.Empty" />). </param>
	/// <param name="capacity">The suggested starting size of the <see cref="T:System.Text.StringBuilder" />. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="capacity" /> is less than zero. </exception>
	public StringBuilder(string value, int capacity)
		: this(value, 0, value?.Length ?? 0, capacity)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Text.StringBuilder" /> class from the specified substring and capacity.</summary>
	/// <param name="value">The string that contains the substring used to initialize the value of this instance. If <paramref name="value" /> is null, the new <see cref="T:System.Text.StringBuilder" /> will contain the empty string (that is, it contains <see cref="F:System.String.Empty" />). </param>
	/// <param name="startIndex">The position within <paramref name="value" /> where the substring begins. </param>
	/// <param name="length">The number of characters in the substring. </param>
	/// <param name="capacity">The suggested starting size of the <see cref="T:System.Text.StringBuilder" />. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="capacity" /> is less than zero.-or- <paramref name="startIndex" /> plus <paramref name="length" /> is not a position within <paramref name="value" />. </exception>
	[SecuritySafeCritical]
	public unsafe StringBuilder(string value, int startIndex, int length, int capacity)
	{
		if (capacity < 0)
		{
			throw new ArgumentOutOfRangeException("capacity", Environment.GetResourceString("'{0}' must be greater than zero.", "capacity"));
		}
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("'{0}' must be non-negative.", "length"));
		}
		if (startIndex < 0)
		{
			throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("StartIndex cannot be less than zero."));
		}
		if (value == null)
		{
			value = string.Empty;
		}
		if (startIndex > value.Length - length)
		{
			throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("Index and length must refer to a location within the string."));
		}
		m_MaxCapacity = int.MaxValue;
		if (capacity == 0)
		{
			capacity = 16;
		}
		if (capacity < length)
		{
			capacity = length;
		}
		m_ChunkChars = new char[capacity];
		m_ChunkLength = length;
		fixed (char* ptr = value)
		{
			ThreadSafeCopy(ptr + startIndex, m_ChunkChars, 0, length);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Text.StringBuilder" /> class that starts with a specified capacity and can grow to a specified maximum.</summary>
	/// <param name="capacity">The suggested starting size of the <see cref="T:System.Text.StringBuilder" />. </param>
	/// <param name="maxCapacity">The maximum number of characters the current string can contain. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="maxCapacity" /> is less than one, <paramref name="capacity" /> is less than zero, or <paramref name="capacity" /> is greater than <paramref name="maxCapacity" />. </exception>
	public StringBuilder(int capacity, int maxCapacity)
	{
		if (capacity > maxCapacity)
		{
			throw new ArgumentOutOfRangeException("capacity", Environment.GetResourceString("Capacity exceeds maximum capacity."));
		}
		if (maxCapacity < 1)
		{
			throw new ArgumentOutOfRangeException("maxCapacity", Environment.GetResourceString("MaxCapacity must be one or greater."));
		}
		if (capacity < 0)
		{
			throw new ArgumentOutOfRangeException("capacity", Environment.GetResourceString("'{0}' must be greater than zero.", "capacity"));
		}
		if (capacity == 0)
		{
			capacity = Math.Min(16, maxCapacity);
		}
		m_MaxCapacity = maxCapacity;
		m_ChunkChars = new char[capacity];
	}

	[SecurityCritical]
	private StringBuilder(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		int num = 0;
		string text = null;
		int num2 = int.MaxValue;
		bool flag = false;
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			switch (enumerator.Name)
			{
			case "m_MaxCapacity":
				num2 = info.GetInt32("m_MaxCapacity");
				break;
			case "m_StringValue":
				text = info.GetString("m_StringValue");
				break;
			case "Capacity":
				num = info.GetInt32("Capacity");
				flag = true;
				break;
			}
		}
		if (text == null)
		{
			text = string.Empty;
		}
		if (num2 < 1 || text.Length > num2)
		{
			throw new SerializationException(Environment.GetResourceString("The serialized MaxCapacity property of StringBuilder must be positive and greater than or equal to the String length."));
		}
		if (!flag)
		{
			num = 16;
			if (num < text.Length)
			{
				num = text.Length;
			}
			if (num > num2)
			{
				num = num2;
			}
		}
		if (num < 0 || num < text.Length || num > num2)
		{
			throw new SerializationException(Environment.GetResourceString("The serialized Capacity property of StringBuilder must be positive, less than or equal to MaxCapacity and greater than or equal to the String length."));
		}
		m_MaxCapacity = num2;
		m_ChunkChars = new char[num];
		text.CopyTo(0, m_ChunkChars, 0, text.Length);
		m_ChunkLength = text.Length;
		m_ChunkPrevious = null;
	}

	/// <summary>Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object with the data necessary to deserialize the current <see cref="T:System.Text.StringBuilder" /> object.</summary>
	/// <param name="info">The object to populate with serialization information.</param>
	/// <param name="context">The place to store and retrieve serialized data. Reserved for future use.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="info" /> is null. </exception>
	[SecurityCritical]
	void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		info.AddValue("m_MaxCapacity", m_MaxCapacity);
		info.AddValue("Capacity", Capacity);
		info.AddValue("m_StringValue", ToString());
		info.AddValue("m_currentThread", 0);
	}

	[Conditional("_DEBUG")]
	private void VerifyClassInvariant()
	{
		StringBuilder stringBuilder = this;
		_ = m_MaxCapacity;
		while (true)
		{
			StringBuilder chunkPrevious = stringBuilder.m_ChunkPrevious;
			if (chunkPrevious != null)
			{
				stringBuilder = chunkPrevious;
				continue;
			}
			break;
		}
	}

	/// <summary>Ensures that the capacity of this instance of <see cref="T:System.Text.StringBuilder" /> is at least the specified value.</summary>
	/// <returns>The new capacity of this instance.</returns>
	/// <param name="capacity">The minimum capacity to ensure. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="capacity" /> is less than zero.-or- Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />. </exception>
	/// <filterpriority>2</filterpriority>
	public int EnsureCapacity(int capacity)
	{
		if (capacity < 0)
		{
			throw new ArgumentOutOfRangeException("capacity", Environment.GetResourceString("Capacity must be positive."));
		}
		if (Capacity < capacity)
		{
			Capacity = capacity;
		}
		return Capacity;
	}

	/// <summary>Converts the value of this instance to a <see cref="T:System.String" />.</summary>
	/// <returns>A string whose value is the same as this instance.</returns>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public unsafe override string ToString()
	{
		if (Length == 0)
		{
			return string.Empty;
		}
		string text = string.FastAllocateString(Length);
		StringBuilder stringBuilder = this;
		fixed (char* ptr = text)
		{
			do
			{
				if (stringBuilder.m_ChunkLength > 0)
				{
					char[] chunkChars = stringBuilder.m_ChunkChars;
					int chunkOffset = stringBuilder.m_ChunkOffset;
					int chunkLength = stringBuilder.m_ChunkLength;
					if ((uint)(chunkLength + chunkOffset) > text.Length || (uint)chunkLength > (uint)chunkChars.Length)
					{
						throw new ArgumentOutOfRangeException("chunkLength", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
					}
					fixed (char* smem = chunkChars)
					{
						string.wstrcpy(ptr + chunkOffset, smem, chunkLength);
					}
				}
				stringBuilder = stringBuilder.m_ChunkPrevious;
			}
			while (stringBuilder != null);
		}
		return text;
	}

	/// <summary>Converts the value of a substring of this instance to a <see cref="T:System.String" />.</summary>
	/// <returns>A string whose value is the same as the specified substring of this instance.</returns>
	/// <param name="startIndex">The starting position of the substring in this instance. </param>
	/// <param name="length">The length of the substring. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> or <paramref name="length" /> is less than zero.-or- The sum of <paramref name="startIndex" /> and <paramref name="length" /> is greater than the length of the current instance. </exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public unsafe string ToString(int startIndex, int length)
	{
		int length2 = Length;
		if (startIndex < 0)
		{
			throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("StartIndex cannot be less than zero."));
		}
		if (startIndex > length2)
		{
			throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("startIndex cannot be larger than length of string."));
		}
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("Length cannot be less than zero."));
		}
		if (startIndex > length2 - length)
		{
			throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("Index and length must refer to a location within the string."));
		}
		StringBuilder stringBuilder = this;
		int num = startIndex + length;
		string text = string.FastAllocateString(length);
		int num2 = length;
		fixed (char* ptr = text)
		{
			while (num2 > 0)
			{
				int num3 = num - stringBuilder.m_ChunkOffset;
				if (num3 >= 0)
				{
					if (num3 > stringBuilder.m_ChunkLength)
					{
						num3 = stringBuilder.m_ChunkLength;
					}
					int num4 = num2;
					int num5 = num4;
					int num6 = num3 - num4;
					if (num6 < 0)
					{
						num5 += num6;
						num6 = 0;
					}
					num2 -= num5;
					if (num5 > 0)
					{
						char[] chunkChars = stringBuilder.m_ChunkChars;
						if ((uint)(num5 + num2) > length || (uint)(num5 + num6) > (uint)chunkChars.Length)
						{
							throw new ArgumentOutOfRangeException("chunkCount", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
						}
						fixed (char* smem = &chunkChars[num6])
						{
							string.wstrcpy(ptr + num2, smem, num5);
						}
					}
				}
				stringBuilder = stringBuilder.m_ChunkPrevious;
			}
		}
		return text;
	}

	/// <summary>Removes all characters from the current <see cref="T:System.Text.StringBuilder" /> instance.</summary>
	/// <returns>An object whose <see cref="P:System.Text.StringBuilder.Length" /> is 0 (zero).</returns>
	public StringBuilder Clear()
	{
		Length = 0;
		return this;
	}

	/// <summary>Appends a specified number of copies of the string representation of a Unicode character to this instance.</summary>
	/// <returns>A reference to this instance after the append operation has completed.</returns>
	/// <param name="value">The character to append. </param>
	/// <param name="repeatCount">The number of times to append <paramref name="value" />. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="repeatCount" /> is less than zero.-or- Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />. </exception>
	/// <exception cref="T:System.OutOfMemoryException">Out of memory.</exception>
	/// <filterpriority>1</filterpriority>
	public StringBuilder Append(char value, int repeatCount)
	{
		if (repeatCount < 0)
		{
			throw new ArgumentOutOfRangeException("repeatCount", Environment.GetResourceString("Count cannot be less than zero."));
		}
		if (repeatCount == 0)
		{
			return this;
		}
		int num = m_ChunkLength;
		while (repeatCount > 0)
		{
			if (num < m_ChunkChars.Length)
			{
				m_ChunkChars[num++] = value;
				repeatCount--;
			}
			else
			{
				m_ChunkLength = num;
				ExpandByABlock(repeatCount);
				num = 0;
			}
		}
		m_ChunkLength = num;
		return this;
	}

	/// <summary>Appends the string representation of a specified subarray of Unicode characters to this instance.</summary>
	/// <returns>A reference to this instance after the append operation has completed.</returns>
	/// <param name="value">A character array. </param>
	/// <param name="startIndex">The starting position in <paramref name="value" />. </param>
	/// <param name="charCount">The number of characters to append. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null, and <paramref name="startIndex" /> and <paramref name="charCount" /> are not zero. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="charCount" /> is less than zero.-or- <paramref name="startIndex" /> is less than zero.-or- <paramref name="startIndex" /> + <paramref name="charCount" /> is greater than the length of <paramref name="value" />.-or- Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />. </exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public unsafe StringBuilder Append(char[] value, int startIndex, int charCount)
	{
		if (startIndex < 0)
		{
			throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("Value must be positive."));
		}
		if (charCount < 0)
		{
			throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Value must be positive."));
		}
		if (CompatibilitySwitches.IsAppEarlierThanWindowsPhone8 && charCount == 0)
		{
			return this;
		}
		if (value == null)
		{
			if (startIndex == 0 && charCount == 0)
			{
				return this;
			}
			throw new ArgumentNullException("value");
		}
		if (charCount > value.Length - startIndex)
		{
			throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		if (charCount == 0)
		{
			return this;
		}
		fixed (char* value2 = &value[startIndex])
		{
			Append(value2, charCount);
		}
		return this;
	}

	/// <summary>Appends a copy of the specified string to this instance.</summary>
	/// <returns>A reference to this instance after the append operation has completed.</returns>
	/// <param name="value">The string to append. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />. </exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public unsafe StringBuilder Append(string value)
	{
		if (value != null)
		{
			char[] chunkChars = m_ChunkChars;
			int chunkLength = m_ChunkLength;
			int length = value.Length;
			int num = chunkLength + length;
			if (num < chunkChars.Length)
			{
				if (length <= 2)
				{
					if (length > 0)
					{
						chunkChars[chunkLength] = value[0];
					}
					if (length > 1)
					{
						chunkChars[chunkLength + 1] = value[1];
					}
				}
				else
				{
					fixed (char* smem = value)
					{
						fixed (char* dmem = &chunkChars[chunkLength])
						{
							string.wstrcpy(dmem, smem, length);
						}
					}
				}
				m_ChunkLength = num;
			}
			else
			{
				AppendHelper(value);
			}
		}
		return this;
	}

	[SecuritySafeCritical]
	private unsafe void AppendHelper(string value)
	{
		fixed (char* value2 = value)
		{
			Append(value2, value.Length);
		}
	}

	/// <summary>Appends a copy of a specified substring to this instance.</summary>
	/// <returns>A reference to this instance after the append operation has completed.</returns>
	/// <param name="value">The string that contains the substring to append. </param>
	/// <param name="startIndex">The starting position of the substring within <paramref name="value" />. </param>
	/// <param name="count">The number of characters in <paramref name="value" /> to append. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null, and <paramref name="startIndex" /> and <paramref name="count" /> are not zero. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> less than zero.-or- <paramref name="startIndex" /> less than zero.-or- <paramref name="startIndex" /> + <paramref name="count" /> is greater than the length of <paramref name="value" />.-or- Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />. </exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public unsafe StringBuilder Append(string value, int startIndex, int count)
	{
		if (startIndex < 0)
		{
			throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Value must be positive."));
		}
		if (CompatibilitySwitches.IsAppEarlierThanWindowsPhone8 && count == 0)
		{
			return this;
		}
		if (value == null)
		{
			if (startIndex == 0 && count == 0)
			{
				return this;
			}
			throw new ArgumentNullException("value");
		}
		if (count == 0)
		{
			return this;
		}
		if (startIndex > value.Length - count)
		{
			throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		fixed (char* ptr = value)
		{
			Append(ptr + startIndex, count);
		}
		return this;
	}

	/// <summary>Appends the default line terminator to the end of the current <see cref="T:System.Text.StringBuilder" /> object.</summary>
	/// <returns>A reference to this instance after the append operation has completed.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />. </exception>
	/// <filterpriority>1</filterpriority>
	[ComVisible(false)]
	public StringBuilder AppendLine()
	{
		return Append(Environment.NewLine);
	}

	/// <summary>Appends a copy of the specified string followed by the default line terminator to the end of the current <see cref="T:System.Text.StringBuilder" /> object.</summary>
	/// <returns>A reference to this instance after the append operation has completed.</returns>
	/// <param name="value">The string to append. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />. </exception>
	/// <filterpriority>1</filterpriority>
	[ComVisible(false)]
	public StringBuilder AppendLine(string value)
	{
		Append(value);
		return Append(Environment.NewLine);
	}

	/// <summary>Copies the characters from a specified segment of this instance to a specified segment of a destination <see cref="T:System.Char" /> array.</summary>
	/// <param name="sourceIndex">The starting position in this instance where characters will be copied from. The index is zero-based.</param>
	/// <param name="destination">The array where characters will be copied.</param>
	/// <param name="destinationIndex">The starting position in <paramref name="destination" /> where characters will be copied. The index is zero-based.</param>
	/// <param name="count">The number of characters to be copied.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="destination" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="sourceIndex" />, <paramref name="destinationIndex" />, or <paramref name="count" />, is less than zero.-or-<paramref name="sourceIndex" /> is greater than the length of this instance.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="sourceIndex" /> + <paramref name="count" /> is greater than the length of this instance.-or-<paramref name="destinationIndex" /> + <paramref name="count" /> is greater than the length of <paramref name="destination" />.</exception>
	/// <filterpriority>1</filterpriority>
	[ComVisible(false)]
	[SecuritySafeCritical]
	public void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
	{
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Argument count must not be negative."));
		}
		if (destinationIndex < 0)
		{
			throw new ArgumentOutOfRangeException("destinationIndex", Environment.GetResourceString("'{0}' must be non-negative.", "destinationIndex"));
		}
		if (destinationIndex > destination.Length - count)
		{
			throw new ArgumentException(Environment.GetResourceString("Either offset did not refer to a position in the string, or there is an insufficient length of destination character array."));
		}
		if ((uint)sourceIndex > (uint)Length)
		{
			throw new ArgumentOutOfRangeException("sourceIndex", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		if (sourceIndex > Length - count)
		{
			throw new ArgumentException(Environment.GetResourceString("Source string was not long enough. Check sourceIndex and count."));
		}
		StringBuilder stringBuilder = this;
		int num = sourceIndex + count;
		int num2 = destinationIndex + count;
		while (count > 0)
		{
			int num3 = num - stringBuilder.m_ChunkOffset;
			if (num3 >= 0)
			{
				if (num3 > stringBuilder.m_ChunkLength)
				{
					num3 = stringBuilder.m_ChunkLength;
				}
				int num4 = count;
				int num5 = num3 - count;
				if (num5 < 0)
				{
					num4 += num5;
					num5 = 0;
				}
				num2 -= num4;
				count -= num4;
				ThreadSafeCopy(stringBuilder.m_ChunkChars, num5, destination, num2, num4);
			}
			stringBuilder = stringBuilder.m_ChunkPrevious;
		}
	}

	/// <summary>Inserts one or more copies of a specified string into this instance at the specified character position.</summary>
	/// <returns>A reference to this instance after insertion has completed.</returns>
	/// <param name="index">The position in this instance where insertion begins. </param>
	/// <param name="value">The string to insert. </param>
	/// <param name="count">The number of times to insert <paramref name="value" />. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero or greater than the current length of this instance.-or- <paramref name="count" /> is less than zero. </exception>
	/// <exception cref="T:System.OutOfMemoryException">The current length of this <see cref="T:System.Text.StringBuilder" /> object plus the length of <paramref name="value" /> times <paramref name="count" /> exceeds <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public unsafe StringBuilder Insert(int index, string value, int count)
	{
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Non-negative number required."));
		}
		int length = Length;
		if ((uint)index > (uint)length)
		{
			throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		if (value == null || value.Length == 0 || count == 0)
		{
			return this;
		}
		long num = (long)value.Length * (long)count;
		if (num > MaxCapacity - Length)
		{
			throw new OutOfMemoryException();
		}
		MakeRoom(index, (int)num, out var chunk, out var indexInChunk, doneMoveFollowingChars: false);
		fixed (char* value2 = value)
		{
			while (count > 0)
			{
				ReplaceInPlaceAtChunk(ref chunk, ref indexInChunk, value2, value.Length);
				count--;
			}
		}
		return this;
	}

	/// <summary>Removes the specified range of characters from this instance.</summary>
	/// <returns>A reference to this instance after the excise operation has completed.</returns>
	/// <param name="startIndex">The zero-based position in this instance where removal begins. </param>
	/// <param name="length">The number of characters to remove. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">If <paramref name="startIndex" /> or <paramref name="length" /> is less than zero, or <paramref name="startIndex" /> + <paramref name="length" /> is greater than the length of this instance. </exception>
	/// <filterpriority>1</filterpriority>
	public StringBuilder Remove(int startIndex, int length)
	{
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("Length cannot be less than zero."));
		}
		if (startIndex < 0)
		{
			throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("StartIndex cannot be less than zero."));
		}
		if (length > Length - startIndex)
		{
			throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		if (Length == length && startIndex == 0)
		{
			Length = 0;
			return this;
		}
		if (length > 0)
		{
			Remove(startIndex, length, out var _, out var _);
		}
		return this;
	}

	/// <summary>Appends the string representation of a specified Boolean value to this instance.</summary>
	/// <returns>A reference to this instance after the append operation has completed.</returns>
	/// <param name="value">The Boolean value to append. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />. </exception>
	/// <filterpriority>1</filterpriority>
	public StringBuilder Append(bool value)
	{
		return Append(value.ToString());
	}

	/// <summary>Appends the string representation of a specified 8-bit signed integer to this instance.</summary>
	/// <returns>A reference to this instance after the append operation has completed.</returns>
	/// <param name="value">The value to append. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public StringBuilder Append(sbyte value)
	{
		return Append(value.ToString(CultureInfo.CurrentCulture));
	}

	/// <summary>Appends the string representation of a specified 8-bit unsigned integer to this instance.</summary>
	/// <returns>A reference to this instance after the append operation has completed.</returns>
	/// <param name="value">The value to append. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />. </exception>
	/// <filterpriority>1</filterpriority>
	public StringBuilder Append(byte value)
	{
		return Append(value.ToString(CultureInfo.CurrentCulture));
	}

	/// <summary>Appends the string representation of a specified Unicode character to this instance.</summary>
	/// <returns>A reference to this instance after the append operation has completed.</returns>
	/// <param name="value">The Unicode character to append. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />. </exception>
	/// <filterpriority>1</filterpriority>
	public StringBuilder Append(char value)
	{
		if (m_ChunkLength < m_ChunkChars.Length)
		{
			m_ChunkChars[m_ChunkLength++] = value;
		}
		else
		{
			Append(value, 1);
		}
		return this;
	}

	/// <summary>Appends the string representation of a specified 16-bit signed integer to this instance.</summary>
	/// <returns>A reference to this instance after the append operation has completed.</returns>
	/// <param name="value">The value to append. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />. </exception>
	/// <filterpriority>1</filterpriority>
	public StringBuilder Append(short value)
	{
		return Append(value.ToString(CultureInfo.CurrentCulture));
	}

	/// <summary>Appends the string representation of a specified 32-bit signed integer to this instance.</summary>
	/// <returns>A reference to this instance after the append operation has completed.</returns>
	/// <param name="value">The value to append. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />. </exception>
	/// <filterpriority>1</filterpriority>
	public StringBuilder Append(int value)
	{
		return Append(value.ToString(CultureInfo.CurrentCulture));
	}

	/// <summary>Appends the string representation of a specified 64-bit signed integer to this instance.</summary>
	/// <returns>A reference to this instance after the append operation has completed.</returns>
	/// <param name="value">The value to append. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />. </exception>
	/// <filterpriority>1</filterpriority>
	public StringBuilder Append(long value)
	{
		return Append(value.ToString(CultureInfo.CurrentCulture));
	}

	/// <summary>Appends the string representation of a specified single-precision floating-point number to this instance.</summary>
	/// <returns>A reference to this instance after the append operation has completed.</returns>
	/// <param name="value">The value to append. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />. </exception>
	/// <filterpriority>1</filterpriority>
	public StringBuilder Append(float value)
	{
		return Append(value.ToString(CultureInfo.CurrentCulture));
	}

	/// <summary>Appends the string representation of a specified double-precision floating-point number to this instance.</summary>
	/// <returns>A reference to this instance after the append operation has completed.</returns>
	/// <param name="value">The value to append. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />. </exception>
	/// <filterpriority>1</filterpriority>
	public StringBuilder Append(double value)
	{
		return Append(value.ToString(CultureInfo.CurrentCulture));
	}

	/// <summary>Appends the string representation of a specified decimal number to this instance.</summary>
	/// <returns>A reference to this instance after the append operation has completed.</returns>
	/// <param name="value">The value to append. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />. </exception>
	/// <filterpriority>1</filterpriority>
	public StringBuilder Append(decimal value)
	{
		return Append(value.ToString(CultureInfo.CurrentCulture));
	}

	/// <summary>Appends the string representation of a specified 16-bit unsigned integer to this instance.</summary>
	/// <returns>A reference to this instance after the append operation has completed.</returns>
	/// <param name="value">The value to append. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public StringBuilder Append(ushort value)
	{
		return Append(value.ToString(CultureInfo.CurrentCulture));
	}

	/// <summary>Appends the string representation of a specified 32-bit unsigned integer to this instance.</summary>
	/// <returns>A reference to this instance after the append operation has completed.</returns>
	/// <param name="value">The value to append. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public StringBuilder Append(uint value)
	{
		return Append(value.ToString(CultureInfo.CurrentCulture));
	}

	/// <summary>Appends the string representation of a specified 64-bit unsigned integer to this instance.</summary>
	/// <returns>A reference to this instance after the append operation has completed.</returns>
	/// <param name="value">The value to append. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public StringBuilder Append(ulong value)
	{
		return Append(value.ToString(CultureInfo.CurrentCulture));
	}

	/// <summary>Appends the string representation of a specified object to this instance.</summary>
	/// <returns>A reference to this instance after the append operation has completed.</returns>
	/// <param name="value">The object to append. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />. </exception>
	/// <filterpriority>1</filterpriority>
	public StringBuilder Append(object value)
	{
		if (value == null)
		{
			return this;
		}
		return Append(value.ToString());
	}

	/// <summary>Appends the string representation of the Unicode characters in a specified array to this instance.</summary>
	/// <returns>A reference to this instance after the append operation has completed.</returns>
	/// <param name="value">The array of characters to append. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />. </exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public unsafe StringBuilder Append(char[] value)
	{
		if (value != null && value.Length != 0)
		{
			fixed (char* value2 = &value[0])
			{
				Append(value2, value.Length);
			}
		}
		return this;
	}

	/// <summary>Inserts a string into this instance at the specified character position.</summary>
	/// <returns>A reference to this instance after the insert operation has completed.</returns>
	/// <param name="index">The position in this instance where insertion begins. </param>
	/// <param name="value">The string to insert. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero or greater than the current length of this instance. -or-The current length of this <see cref="T:System.Text.StringBuilder" /> object plus the length of <paramref name="value" /> exceeds <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public unsafe StringBuilder Insert(int index, string value)
	{
		if ((uint)index > (uint)Length)
		{
			throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		if (value != null)
		{
			fixed (char* value2 = value)
			{
				Insert(index, value2, value.Length);
			}
		}
		return this;
	}

	/// <summary>Inserts the string representation of a Boolean value into this instance at the specified character position.</summary>
	/// <returns>A reference to this instance after the insert operation has completed.</returns>
	/// <param name="index">The position in this instance where insertion begins. </param>
	/// <param name="value">The value to insert. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero or greater than the length of this instance.</exception>
	/// <exception cref="T:System.OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
	/// <filterpriority>1</filterpriority>
	public StringBuilder Insert(int index, bool value)
	{
		return Insert(index, value.ToString(), 1);
	}

	/// <summary>Inserts the string representation of a specified 8-bit signed integer into this instance at the specified character position.</summary>
	/// <returns>A reference to this instance after the insert operation has completed.</returns>
	/// <param name="index">The position in this instance where insertion begins. </param>
	/// <param name="value">The value to insert. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero or greater than the length of this instance. </exception>
	/// <exception cref="T:System.OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public StringBuilder Insert(int index, sbyte value)
	{
		return Insert(index, value.ToString(CultureInfo.CurrentCulture), 1);
	}

	/// <summary>Inserts the string representation of a specified 8-bit unsigned integer into this instance at the specified character position.</summary>
	/// <returns>A reference to this instance after the insert operation has completed.</returns>
	/// <param name="index">The position in this instance where insertion begins. </param>
	/// <param name="value">The value to insert. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero or greater than the length of this instance. </exception>
	/// <exception cref="T:System.OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
	/// <filterpriority>1</filterpriority>
	public StringBuilder Insert(int index, byte value)
	{
		return Insert(index, value.ToString(CultureInfo.CurrentCulture), 1);
	}

	/// <summary>Inserts the string representation of a specified 16-bit signed integer into this instance at the specified character position.</summary>
	/// <returns>A reference to this instance after the insert operation has completed.</returns>
	/// <param name="index">The position in this instance where insertion begins. </param>
	/// <param name="value">The value to insert. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero or greater than the length of this instance. </exception>
	/// <exception cref="T:System.OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
	/// <filterpriority>1</filterpriority>
	public StringBuilder Insert(int index, short value)
	{
		return Insert(index, value.ToString(CultureInfo.CurrentCulture), 1);
	}

	/// <summary>Inserts the string representation of a specified Unicode character into this instance at the specified character position.</summary>
	/// <returns>A reference to this instance after the insert operation has completed.</returns>
	/// <param name="index">The position in this instance where insertion begins. </param>
	/// <param name="value">The value to insert. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero or greater than the length of this instance.-or- Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />. </exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public unsafe StringBuilder Insert(int index, char value)
	{
		Insert(index, &value, 1);
		return this;
	}

	/// <summary>Inserts the string representation of a specified array of Unicode characters into this instance at the specified character position.</summary>
	/// <returns>A reference to this instance after the insert operation has completed.</returns>
	/// <param name="index">The position in this instance where insertion begins. </param>
	/// <param name="value">The character array to insert. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero or greater than the length of this instance.-or- Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />. </exception>
	/// <filterpriority>1</filterpriority>
	public StringBuilder Insert(int index, char[] value)
	{
		if ((uint)index > (uint)Length)
		{
			throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		if (value != null)
		{
			Insert(index, value, 0, value.Length);
		}
		return this;
	}

	/// <summary>Inserts the string representation of a specified subarray of Unicode characters into this instance at the specified character position.</summary>
	/// <returns>A reference to this instance after the insert operation has completed.</returns>
	/// <param name="index">The position in this instance where insertion begins. </param>
	/// <param name="value">A character array. </param>
	/// <param name="startIndex">The starting index within <paramref name="value" />. </param>
	/// <param name="charCount">The number of characters to insert. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null, and <paramref name="startIndex" /> and <paramref name="charCount" /> are not zero. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" />, <paramref name="startIndex" />, or <paramref name="charCount" /> is less than zero.-or- <paramref name="index" /> is greater than the length of this instance.-or- <paramref name="startIndex" /> plus <paramref name="charCount" /> is not a position within <paramref name="value" />.-or- Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />. </exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public unsafe StringBuilder Insert(int index, char[] value, int startIndex, int charCount)
	{
		int length = Length;
		if ((uint)index > (uint)length)
		{
			throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		if (value == null)
		{
			if (startIndex == 0 && charCount == 0)
			{
				return this;
			}
			throw new ArgumentNullException(Environment.GetResourceString("String reference not set to an instance of a String."));
		}
		if (startIndex < 0)
		{
			throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("StartIndex cannot be less than zero."));
		}
		if (charCount < 0)
		{
			throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Value must be positive."));
		}
		if (startIndex > value.Length - charCount)
		{
			throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		if (charCount > 0)
		{
			fixed (char* value2 = &value[startIndex])
			{
				Insert(index, value2, charCount);
			}
		}
		return this;
	}

	/// <summary>Inserts the string representation of a specified 32-bit signed integer into this instance at the specified character position.</summary>
	/// <returns>A reference to this instance after the insert operation has completed.</returns>
	/// <param name="index">The position in this instance where insertion begins. </param>
	/// <param name="value">The value to insert. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero or greater than the length of this instance. </exception>
	/// <exception cref="T:System.OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
	/// <filterpriority>1</filterpriority>
	public StringBuilder Insert(int index, int value)
	{
		return Insert(index, value.ToString(CultureInfo.CurrentCulture), 1);
	}

	/// <summary>Inserts the string representation of a 64-bit signed integer into this instance at the specified character position.</summary>
	/// <returns>A reference to this instance after the insert operation has completed.</returns>
	/// <param name="index">The position in this instance where insertion begins. </param>
	/// <param name="value">The value to insert. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero or greater than the length of this instance. </exception>
	/// <exception cref="T:System.OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
	/// <filterpriority>1</filterpriority>
	public StringBuilder Insert(int index, long value)
	{
		return Insert(index, value.ToString(CultureInfo.CurrentCulture), 1);
	}

	/// <summary>Inserts the string representation of a single-precision floating point number into this instance at the specified character position.</summary>
	/// <returns>A reference to this instance after the insert operation has completed.</returns>
	/// <param name="index">The position in this instance where insertion begins. </param>
	/// <param name="value">The value to insert. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero or greater than the length of this instance. </exception>
	/// <exception cref="T:System.OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
	/// <filterpriority>1</filterpriority>
	public StringBuilder Insert(int index, float value)
	{
		return Insert(index, value.ToString(CultureInfo.CurrentCulture), 1);
	}

	/// <summary>Inserts the string representation of a double-precision floating-point number into this instance at the specified character position.</summary>
	/// <returns>A reference to this instance after the insert operation has completed.</returns>
	/// <param name="index">The position in this instance where insertion begins. </param>
	/// <param name="value">The value to insert. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero or greater than the length of this instance. </exception>
	/// <exception cref="T:System.OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
	/// <filterpriority>1</filterpriority>
	public StringBuilder Insert(int index, double value)
	{
		return Insert(index, value.ToString(CultureInfo.CurrentCulture), 1);
	}

	/// <summary>Inserts the string representation of a decimal number into this instance at the specified character position.</summary>
	/// <returns>A reference to this instance after the insert operation has completed.</returns>
	/// <param name="index">The position in this instance where insertion begins. </param>
	/// <param name="value">The value to insert. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero or greater than the length of this instance. </exception>
	/// <exception cref="T:System.OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
	/// <filterpriority>1</filterpriority>
	public StringBuilder Insert(int index, decimal value)
	{
		return Insert(index, value.ToString(CultureInfo.CurrentCulture), 1);
	}

	/// <summary>Inserts the string representation of a 16-bit unsigned integer into this instance at the specified character position.</summary>
	/// <returns>A reference to this instance after the insert operation has completed.</returns>
	/// <param name="index">The position in this instance where insertion begins. </param>
	/// <param name="value">The value to insert. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero or greater than the length of this instance. </exception>
	/// <exception cref="T:System.OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public StringBuilder Insert(int index, ushort value)
	{
		return Insert(index, value.ToString(CultureInfo.CurrentCulture), 1);
	}

	/// <summary>Inserts the string representation of a 32-bit unsigned integer into this instance at the specified character position.</summary>
	/// <returns>A reference to this instance after the insert operation has completed.</returns>
	/// <param name="index">The position in this instance where insertion begins. </param>
	/// <param name="value">The value to insert. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero or greater than the length of this instance. </exception>
	/// <exception cref="T:System.OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public StringBuilder Insert(int index, uint value)
	{
		return Insert(index, value.ToString(CultureInfo.CurrentCulture), 1);
	}

	/// <summary>Inserts the string representation of a 64-bit unsigned integer into this instance at the specified character position.</summary>
	/// <returns>A reference to this instance after the insert operation has completed.</returns>
	/// <param name="index">The position in this instance where insertion begins. </param>
	/// <param name="value">The value to insert. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero or greater than the length of this instance. </exception>
	/// <exception cref="T:System.OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public StringBuilder Insert(int index, ulong value)
	{
		return Insert(index, value.ToString(CultureInfo.CurrentCulture), 1);
	}

	/// <summary>Inserts the string representation of an object into this instance at the specified character position.</summary>
	/// <returns>A reference to this instance after the insert operation has completed.</returns>
	/// <param name="index">The position in this instance where insertion begins. </param>
	/// <param name="value">The object to insert, or null. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero or greater than the length of this instance. </exception>
	/// <exception cref="T:System.OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
	/// <filterpriority>1</filterpriority>
	public StringBuilder Insert(int index, object value)
	{
		if (value == null)
		{
			return this;
		}
		return Insert(index, value.ToString(), 1);
	}

	/// <summary>Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance. Each format item is replaced by the string representation of a single argument.</summary>
	/// <returns>A reference to this instance with <paramref name="format" /> appended. Each format item in <paramref name="format" /> is replaced by the string representation of <paramref name="arg0" />.</returns>
	/// <param name="format">A composite format string (see Remarks). </param>
	/// <param name="arg0">An object to format. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="format" /> is null. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="format" /> is invalid. -or-The index of a format item is less than 0 (zero), or greater than or equal to 1.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The length of the expanded string would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />. </exception>
	/// <filterpriority>2</filterpriority>
	public StringBuilder AppendFormat(string format, object arg0)
	{
		return AppendFormatHelper(null, format, new ParamsArray(arg0));
	}

	/// <summary>Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance. Each format item is replaced by the string representation of either of two arguments.</summary>
	/// <returns>A reference to this instance with <paramref name="format" /> appended. Each format item in <paramref name="format" /> is replaced by the string representation of the corresponding object argument.</returns>
	/// <param name="format">A composite format string (see Remarks). </param>
	/// <param name="arg0">The first object to format. </param>
	/// <param name="arg1">The second object to format. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="format" /> is null. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="format" /> is invalid.-or-The index of a format item is less than 0 (zero), or greater than or equal to 2. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The length of the expanded string would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />. </exception>
	/// <filterpriority>2</filterpriority>
	public StringBuilder AppendFormat(string format, object arg0, object arg1)
	{
		return AppendFormatHelper(null, format, new ParamsArray(arg0, arg1));
	}

	/// <summary>Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance. Each format item is replaced by the string representation of either of three arguments.</summary>
	/// <returns>A reference to this instance with <paramref name="format" /> appended. Each format item in <paramref name="format" /> is replaced by the string representation of the corresponding object argument.</returns>
	/// <param name="format">A composite format string (see Remarks). </param>
	/// <param name="arg0">The first object to format. </param>
	/// <param name="arg1">The second object to format. </param>
	/// <param name="arg2">The third object to format. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="format" /> is null. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="format" /> is invalid.-or-The index of a format item is less than 0 (zero), or greater than or equal to 3.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The length of the expanded string would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />. </exception>
	/// <filterpriority>2</filterpriority>
	public StringBuilder AppendFormat(string format, object arg0, object arg1, object arg2)
	{
		return AppendFormatHelper(null, format, new ParamsArray(arg0, arg1, arg2));
	}

	/// <summary>Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance. Each format item is replaced by the string representation of a corresponding argument in a parameter array.</summary>
	/// <returns>A reference to this instance with <paramref name="format" /> appended. Each format item in <paramref name="format" /> is replaced by the string representation of the corresponding object argument.</returns>
	/// <param name="format">A composite format string (see Remarks). </param>
	/// <param name="args">An array of objects to format. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="format" /> or <paramref name="args" /> is null. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="format" /> is invalid. -or-The index of a format item is less than 0 (zero), or greater than or equal to the length of the <paramref name="args" /> array.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The length of the expanded string would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />. </exception>
	/// <filterpriority>2</filterpriority>
	public StringBuilder AppendFormat(string format, params object[] args)
	{
		if (args == null)
		{
			throw new ArgumentNullException((format == null) ? "format" : "args");
		}
		return AppendFormatHelper(null, format, new ParamsArray(args));
	}

	public StringBuilder AppendFormat(IFormatProvider provider, string format, object arg0)
	{
		return AppendFormatHelper(provider, format, new ParamsArray(arg0));
	}

	public StringBuilder AppendFormat(IFormatProvider provider, string format, object arg0, object arg1)
	{
		return AppendFormatHelper(provider, format, new ParamsArray(arg0, arg1));
	}

	public StringBuilder AppendFormat(IFormatProvider provider, string format, object arg0, object arg1, object arg2)
	{
		return AppendFormatHelper(provider, format, new ParamsArray(arg0, arg1, arg2));
	}

	/// <summary>Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance. Each format item is replaced by the string representation of a corresponding argument in a parameter array using a specified format provider.</summary>
	/// <returns>A reference to this instance after the append operation has completed. After the append operation, this instance contains any data that existed before the operation, suffixed by a copy of <paramref name="format" /> where any format specification is replaced by the string representation of the corresponding object argument. </returns>
	/// <param name="provider">An object that supplies culture-specific formatting information. </param>
	/// <param name="format">A composite format string (see Remarks). </param>
	/// <param name="args">An array of objects to format.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="format" /> is null. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="format" /> is invalid. -or-The index of a format item is less than 0 (zero), or greater than or equal to the length of the <paramref name="args" /> array.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The length of the expanded string would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />. </exception>
	/// <filterpriority>2</filterpriority>
	public StringBuilder AppendFormat(IFormatProvider provider, string format, params object[] args)
	{
		if (args == null)
		{
			throw new ArgumentNullException((format == null) ? "format" : "args");
		}
		return AppendFormatHelper(provider, format, new ParamsArray(args));
	}

	private static void FormatError()
	{
		throw new FormatException(Environment.GetResourceString("Input string was not in a correct format."));
	}

	internal StringBuilder AppendFormatHelper(IFormatProvider provider, string format, ParamsArray args)
	{
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		int num = 0;
		int length = format.Length;
		char c = '\0';
		ICustomFormatter customFormatter = null;
		if (provider != null)
		{
			customFormatter = (ICustomFormatter)provider.GetFormat(typeof(ICustomFormatter));
		}
		while (true)
		{
			if (num < length)
			{
				c = format[num];
				num++;
				if (c == '}')
				{
					if (num < length && format[num] == '}')
					{
						num++;
					}
					else
					{
						FormatError();
					}
				}
				if (c == '{')
				{
					if (num >= length || format[num] != '{')
					{
						num--;
						goto IL_008d;
					}
					num++;
				}
				Append(c);
				continue;
			}
			goto IL_008d;
			IL_008d:
			if (num == length)
			{
				break;
			}
			num++;
			if (num == length || (c = format[num]) < '0' || c > '9')
			{
				FormatError();
			}
			int num2 = 0;
			do
			{
				num2 = num2 * 10 + c - 48;
				num++;
				if (num == length)
				{
					FormatError();
				}
				c = format[num];
			}
			while (c >= '0' && c <= '9' && num2 < 1000000);
			if (num2 >= args.Length)
			{
				throw new FormatException(Environment.GetResourceString("Index (zero based) must be greater than or equal to zero and less than the size of the argument list."));
			}
			for (; num < length; num++)
			{
				if ((c = format[num]) != ' ')
				{
					break;
				}
			}
			bool flag = false;
			int num3 = 0;
			if (c == ',')
			{
				for (num++; num < length && format[num] == ' '; num++)
				{
				}
				if (num == length)
				{
					FormatError();
				}
				c = format[num];
				if (c == '-')
				{
					flag = true;
					num++;
					if (num == length)
					{
						FormatError();
					}
					c = format[num];
				}
				if (c < '0' || c > '9')
				{
					FormatError();
				}
				do
				{
					num3 = num3 * 10 + c - 48;
					num++;
					if (num == length)
					{
						FormatError();
					}
					c = format[num];
				}
				while (c >= '0' && c <= '9' && num3 < 1000000);
			}
			for (; num < length; num++)
			{
				if ((c = format[num]) != ' ')
				{
					break;
				}
			}
			object obj = args[num2];
			StringBuilder stringBuilder = null;
			if (c == ':')
			{
				num++;
				while (true)
				{
					if (num == length)
					{
						FormatError();
					}
					c = format[num];
					num++;
					if (c == '{')
					{
						if (num < length && format[num] == '{')
						{
							num++;
						}
						else
						{
							FormatError();
						}
					}
					else if (c == '}')
					{
						if (num >= length || format[num] != '}')
						{
							break;
						}
						num++;
					}
					if (stringBuilder == null)
					{
						stringBuilder = new StringBuilder();
					}
					stringBuilder.Append(c);
				}
				num--;
			}
			if (c != '}')
			{
				FormatError();
			}
			num++;
			string text = null;
			string text2 = null;
			if (customFormatter != null)
			{
				if (stringBuilder != null)
				{
					text = stringBuilder.ToString();
				}
				text2 = customFormatter.Format(text, obj, provider);
			}
			if (text2 == null)
			{
				if (obj is IFormattable formattable)
				{
					if (text == null && stringBuilder != null)
					{
						text = stringBuilder.ToString();
					}
					text2 = formattable.ToString(text, provider);
				}
				else if (obj != null)
				{
					text2 = obj.ToString();
				}
			}
			if (text2 == null)
			{
				text2 = string.Empty;
			}
			int num4 = num3 - text2.Length;
			if (!flag && num4 > 0)
			{
				Append(' ', num4);
			}
			Append(text2);
			if (flag && num4 > 0)
			{
				Append(' ', num4);
			}
		}
		return this;
	}

	/// <summary>Replaces all occurrences of a specified string in this instance with another specified string.</summary>
	/// <returns>A reference to this instance with all instances of <paramref name="oldValue" /> replaced by <paramref name="newValue" />.</returns>
	/// <param name="oldValue">The string to replace. </param>
	/// <param name="newValue">The string that replaces <paramref name="oldValue" />, or null. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="oldValue" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">The length of <paramref name="oldValue" /> is zero. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />. </exception>
	/// <filterpriority>1</filterpriority>
	public StringBuilder Replace(string oldValue, string newValue)
	{
		return Replace(oldValue, newValue, 0, Length);
	}

	/// <summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
	/// <returns>true if this instance and <paramref name="sb" /> have equal string, <see cref="P:System.Text.StringBuilder.Capacity" />, and <see cref="P:System.Text.StringBuilder.MaxCapacity" /> values; otherwise, false.</returns>
	/// <param name="sb">An object to compare with this instance, or null. </param>
	/// <filterpriority>2</filterpriority>
	public bool Equals(StringBuilder sb)
	{
		if (sb == null)
		{
			return false;
		}
		if (Capacity != sb.Capacity || MaxCapacity != sb.MaxCapacity || Length != sb.Length)
		{
			return false;
		}
		if (sb == this)
		{
			return true;
		}
		StringBuilder stringBuilder = this;
		int num = stringBuilder.m_ChunkLength;
		StringBuilder stringBuilder2 = sb;
		int num2 = stringBuilder2.m_ChunkLength;
		do
		{
			num--;
			num2--;
			while (num < 0)
			{
				stringBuilder = stringBuilder.m_ChunkPrevious;
				if (stringBuilder == null)
				{
					break;
				}
				num = stringBuilder.m_ChunkLength + num;
			}
			while (num2 < 0)
			{
				stringBuilder2 = stringBuilder2.m_ChunkPrevious;
				if (stringBuilder2 == null)
				{
					break;
				}
				num2 = stringBuilder2.m_ChunkLength + num2;
			}
			if (num < 0)
			{
				return num2 < 0;
			}
			if (num2 < 0)
			{
				return false;
			}
		}
		while (stringBuilder.m_ChunkChars[num] == stringBuilder2.m_ChunkChars[num2]);
		return false;
	}

	/// <summary>Replaces, within a substring of this instance, all occurrences of a specified string with another specified string.</summary>
	/// <returns>A reference to this instance with all instances of <paramref name="oldValue" /> replaced by <paramref name="newValue" /> in the range from <paramref name="startIndex" /> to <paramref name="startIndex" /> + <paramref name="count" /> - 1.</returns>
	/// <param name="oldValue">The string to replace. </param>
	/// <param name="newValue">The string that replaces <paramref name="oldValue" />, or null. </param>
	/// <param name="startIndex">The position in this instance where the substring begins. </param>
	/// <param name="count">The length of the substring. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="oldValue" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">The length of <paramref name="oldValue" /> is zero. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> or <paramref name="count" /> is less than zero.-or- <paramref name="startIndex" /> plus <paramref name="count" /> indicates a character position not within this instance.-or- Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />. </exception>
	/// <filterpriority>1</filterpriority>
	public StringBuilder Replace(string oldValue, string newValue, int startIndex, int count)
	{
		int length = Length;
		if ((uint)startIndex > (uint)length)
		{
			throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		if (count < 0 || startIndex > length - count)
		{
			throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		if (oldValue == null)
		{
			throw new ArgumentNullException("oldValue");
		}
		if (oldValue.Length == 0)
		{
			throw new ArgumentException(Environment.GetResourceString("Empty name is not legal."), "oldValue");
		}
		if (newValue == null)
		{
			newValue = "";
		}
		int[] array = null;
		int num = 0;
		StringBuilder stringBuilder = FindChunkForIndex(startIndex);
		int num2 = startIndex - stringBuilder.m_ChunkOffset;
		while (count > 0)
		{
			if (StartsWith(stringBuilder, num2, count, oldValue))
			{
				if (array == null)
				{
					array = new int[5];
				}
				else if (num >= array.Length)
				{
					int[] array2 = new int[array.Length * 3 / 2 + 4];
					Array.Copy(array, array2, array.Length);
					array = array2;
				}
				array[num++] = num2;
				num2 += oldValue.Length;
				count -= oldValue.Length;
			}
			else
			{
				num2++;
				count--;
			}
			if (num2 >= stringBuilder.m_ChunkLength || count == 0)
			{
				int num3 = num2 + stringBuilder.m_ChunkOffset;
				ReplaceAllInChunk(array, num, stringBuilder, oldValue.Length, newValue);
				num3 += (newValue.Length - oldValue.Length) * num;
				num = 0;
				stringBuilder = FindChunkForIndex(num3);
				num2 = num3 - stringBuilder.m_ChunkOffset;
			}
		}
		return this;
	}

	/// <summary>Replaces all occurrences of a specified character in this instance with another specified character.</summary>
	/// <returns>A reference to this instance with <paramref name="oldChar" /> replaced by <paramref name="newChar" />.</returns>
	/// <param name="oldChar">The character to replace. </param>
	/// <param name="newChar">The character that replaces <paramref name="oldChar" />. </param>
	/// <filterpriority>1</filterpriority>
	public StringBuilder Replace(char oldChar, char newChar)
	{
		return Replace(oldChar, newChar, 0, Length);
	}

	/// <summary>Replaces, within a substring of this instance, all occurrences of a specified character with another specified character.</summary>
	/// <returns>A reference to this instance with <paramref name="oldChar" /> replaced by <paramref name="newChar" /> in the range from <paramref name="startIndex" /> to <paramref name="startIndex" /> + <paramref name="count" /> -1.</returns>
	/// <param name="oldChar">The character to replace. </param>
	/// <param name="newChar">The character that replaces <paramref name="oldChar" />. </param>
	/// <param name="startIndex">The position in this instance where the substring begins. </param>
	/// <param name="count">The length of the substring. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> + <paramref name="count" /> is greater than the length of the value of this instance.-or- <paramref name="startIndex" /> or <paramref name="count" /> is less than zero. </exception>
	/// <filterpriority>1</filterpriority>
	public StringBuilder Replace(char oldChar, char newChar, int startIndex, int count)
	{
		int length = Length;
		if ((uint)startIndex > (uint)length)
		{
			throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		if (count < 0 || startIndex > length - count)
		{
			throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		int num = startIndex + count;
		StringBuilder stringBuilder = this;
		while (true)
		{
			int num2 = num - stringBuilder.m_ChunkOffset;
			int num3 = startIndex - stringBuilder.m_ChunkOffset;
			if (num2 >= 0)
			{
				int i = Math.Max(num3, 0);
				for (int num4 = Math.Min(stringBuilder.m_ChunkLength, num2); i < num4; i++)
				{
					if (stringBuilder.m_ChunkChars[i] == oldChar)
					{
						stringBuilder.m_ChunkChars[i] = newChar;
					}
				}
			}
			if (num3 >= 0)
			{
				break;
			}
			stringBuilder = stringBuilder.m_ChunkPrevious;
		}
		return this;
	}

	[CLSCompliant(false)]
	[SecurityCritical]
	public unsafe StringBuilder Append(char* value, int valueCount)
	{
		if (valueCount < 0)
		{
			throw new ArgumentOutOfRangeException("valueCount", Environment.GetResourceString("Count cannot be less than zero."));
		}
		int num = valueCount + m_ChunkLength;
		if (num <= m_ChunkChars.Length)
		{
			ThreadSafeCopy(value, m_ChunkChars, m_ChunkLength, valueCount);
			m_ChunkLength = num;
		}
		else
		{
			int num2 = m_ChunkChars.Length - m_ChunkLength;
			if (num2 > 0)
			{
				ThreadSafeCopy(value, m_ChunkChars, m_ChunkLength, num2);
				m_ChunkLength = m_ChunkChars.Length;
			}
			int num3 = valueCount - num2;
			ExpandByABlock(num3);
			ThreadSafeCopy(value + num2, m_ChunkChars, 0, num3);
			m_ChunkLength = num3;
		}
		return this;
	}

	[SecurityCritical]
	private unsafe void Insert(int index, char* value, int valueCount)
	{
		if ((uint)index > (uint)Length)
		{
			throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		if (valueCount > 0)
		{
			MakeRoom(index, valueCount, out var chunk, out var indexInChunk, doneMoveFollowingChars: false);
			ReplaceInPlaceAtChunk(ref chunk, ref indexInChunk, value, valueCount);
		}
	}

	[SecuritySafeCritical]
	private unsafe void ReplaceAllInChunk(int[] replacements, int replacementsCount, StringBuilder sourceChunk, int removeCount, string value)
	{
		if (replacementsCount <= 0)
		{
			return;
		}
		fixed (char* value2 = value)
		{
			int num = (value.Length - removeCount) * replacementsCount;
			StringBuilder chunk = sourceChunk;
			int indexInChunk = replacements[0];
			if (num > 0)
			{
				MakeRoom(chunk.m_ChunkOffset + indexInChunk, num, out chunk, out indexInChunk, doneMoveFollowingChars: true);
			}
			int num2 = 0;
			while (true)
			{
				ReplaceInPlaceAtChunk(ref chunk, ref indexInChunk, value2, value.Length);
				int num3 = replacements[num2] + removeCount;
				num2++;
				if (num2 >= replacementsCount)
				{
					break;
				}
				int num4 = replacements[num2];
				if (num != 0)
				{
					fixed (char* value3 = &sourceChunk.m_ChunkChars[num3])
					{
						ReplaceInPlaceAtChunk(ref chunk, ref indexInChunk, value3, num4 - num3);
					}
				}
				else
				{
					indexInChunk += num4 - num3;
				}
			}
			if (num < 0)
			{
				Remove(chunk.m_ChunkOffset + indexInChunk, -num, out chunk, out indexInChunk);
			}
		}
	}

	private bool StartsWith(StringBuilder chunk, int indexInChunk, int count, string value)
	{
		for (int i = 0; i < value.Length; i++)
		{
			if (count == 0)
			{
				return false;
			}
			if (indexInChunk >= chunk.m_ChunkLength)
			{
				chunk = Next(chunk);
				if (chunk == null)
				{
					return false;
				}
				indexInChunk = 0;
			}
			if (value[i] != chunk.m_ChunkChars[indexInChunk])
			{
				return false;
			}
			indexInChunk++;
			count--;
		}
		return true;
	}

	[SecurityCritical]
	private unsafe void ReplaceInPlaceAtChunk(ref StringBuilder chunk, ref int indexInChunk, char* value, int count)
	{
		if (count == 0)
		{
			return;
		}
		while (true)
		{
			int num = Math.Min(chunk.m_ChunkLength - indexInChunk, count);
			ThreadSafeCopy(value, chunk.m_ChunkChars, indexInChunk, num);
			indexInChunk += num;
			if (indexInChunk >= chunk.m_ChunkLength)
			{
				chunk = Next(chunk);
				indexInChunk = 0;
			}
			count -= num;
			if (count != 0)
			{
				value += num;
				continue;
			}
			break;
		}
	}

	[SecurityCritical]
	private unsafe static void ThreadSafeCopy(char* sourcePtr, char[] destination, int destinationIndex, int count)
	{
		if (count > 0)
		{
			if ((uint)destinationIndex > (uint)destination.Length || destinationIndex + count > destination.Length)
			{
				throw new ArgumentOutOfRangeException("destinationIndex", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
			}
			fixed (char* dmem = &destination[destinationIndex])
			{
				string.wstrcpy(dmem, sourcePtr, count);
			}
		}
	}

	[SecurityCritical]
	private unsafe static void ThreadSafeCopy(char[] source, int sourceIndex, char[] destination, int destinationIndex, int count)
	{
		if (count > 0)
		{
			if ((uint)sourceIndex > (uint)source.Length || sourceIndex + count > source.Length)
			{
				throw new ArgumentOutOfRangeException("sourceIndex", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
			}
			fixed (char* sourcePtr = &source[sourceIndex])
			{
				ThreadSafeCopy(sourcePtr, destination, destinationIndex, count);
			}
		}
	}

	private StringBuilder FindChunkForIndex(int index)
	{
		StringBuilder stringBuilder = this;
		while (stringBuilder.m_ChunkOffset > index)
		{
			stringBuilder = stringBuilder.m_ChunkPrevious;
		}
		return stringBuilder;
	}

	private StringBuilder FindChunkForByte(int byteIndex)
	{
		StringBuilder stringBuilder = this;
		while (stringBuilder.m_ChunkOffset * 2 > byteIndex)
		{
			stringBuilder = stringBuilder.m_ChunkPrevious;
		}
		return stringBuilder;
	}

	private StringBuilder Next(StringBuilder chunk)
	{
		if (chunk == this)
		{
			return null;
		}
		return FindChunkForIndex(chunk.m_ChunkOffset + chunk.m_ChunkLength);
	}

	private void ExpandByABlock(int minBlockCharCount)
	{
		if (minBlockCharCount + Length < minBlockCharCount || minBlockCharCount + Length > m_MaxCapacity)
		{
			throw new ArgumentOutOfRangeException("requiredLength", Environment.GetResourceString("capacity was less than the current size."));
		}
		int num = Math.Max(minBlockCharCount, Math.Min(Length, 8000));
		m_ChunkPrevious = new StringBuilder(this);
		m_ChunkOffset += m_ChunkLength;
		m_ChunkLength = 0;
		if (m_ChunkOffset + num < num)
		{
			m_ChunkChars = null;
			throw new OutOfMemoryException();
		}
		m_ChunkChars = new char[num];
	}

	private StringBuilder(StringBuilder from)
	{
		m_ChunkLength = from.m_ChunkLength;
		m_ChunkOffset = from.m_ChunkOffset;
		m_ChunkChars = from.m_ChunkChars;
		m_ChunkPrevious = from.m_ChunkPrevious;
		m_MaxCapacity = from.m_MaxCapacity;
	}

	[SecuritySafeCritical]
	private unsafe void MakeRoom(int index, int count, out StringBuilder chunk, out int indexInChunk, bool doneMoveFollowingChars)
	{
		if (count + Length < count || count + Length > m_MaxCapacity)
		{
			throw new ArgumentOutOfRangeException("requiredLength", Environment.GetResourceString("capacity was less than the current size."));
		}
		chunk = this;
		while (chunk.m_ChunkOffset > index)
		{
			chunk.m_ChunkOffset += count;
			chunk = chunk.m_ChunkPrevious;
		}
		indexInChunk = index - chunk.m_ChunkOffset;
		if (!doneMoveFollowingChars && chunk.m_ChunkLength <= 32 && chunk.m_ChunkChars.Length - chunk.m_ChunkLength >= count)
		{
			int num = chunk.m_ChunkLength;
			while (num > indexInChunk)
			{
				num--;
				chunk.m_ChunkChars[num + count] = chunk.m_ChunkChars[num];
			}
			chunk.m_ChunkLength += count;
			return;
		}
		StringBuilder stringBuilder = new StringBuilder(Math.Max(count, 16), chunk.m_MaxCapacity, chunk.m_ChunkPrevious);
		stringBuilder.m_ChunkLength = count;
		int num2 = Math.Min(count, indexInChunk);
		if (num2 > 0)
		{
			fixed (char* chunkChars = chunk.m_ChunkChars)
			{
				ThreadSafeCopy(chunkChars, stringBuilder.m_ChunkChars, 0, num2);
				int num3 = indexInChunk - num2;
				if (num3 >= 0)
				{
					ThreadSafeCopy(chunkChars + num2, chunk.m_ChunkChars, 0, num3);
					indexInChunk = num3;
				}
			}
		}
		chunk.m_ChunkPrevious = stringBuilder;
		chunk.m_ChunkOffset += count;
		if (num2 < count)
		{
			chunk = stringBuilder;
			indexInChunk = num2;
		}
	}

	private StringBuilder(int size, int maxCapacity, StringBuilder previousBlock)
	{
		m_ChunkChars = new char[size];
		m_MaxCapacity = maxCapacity;
		m_ChunkPrevious = previousBlock;
		if (previousBlock != null)
		{
			m_ChunkOffset = previousBlock.m_ChunkOffset + previousBlock.m_ChunkLength;
		}
	}

	[SecuritySafeCritical]
	private void Remove(int startIndex, int count, out StringBuilder chunk, out int indexInChunk)
	{
		int num = startIndex + count;
		chunk = this;
		StringBuilder stringBuilder = null;
		int num2 = 0;
		while (true)
		{
			if (num - chunk.m_ChunkOffset >= 0)
			{
				if (stringBuilder == null)
				{
					stringBuilder = chunk;
					num2 = num - stringBuilder.m_ChunkOffset;
				}
				if (startIndex - chunk.m_ChunkOffset >= 0)
				{
					break;
				}
			}
			else
			{
				chunk.m_ChunkOffset -= count;
			}
			chunk = chunk.m_ChunkPrevious;
		}
		indexInChunk = startIndex - chunk.m_ChunkOffset;
		int num3 = indexInChunk;
		int count2 = stringBuilder.m_ChunkLength - num2;
		if (stringBuilder != chunk)
		{
			num3 = 0;
			chunk.m_ChunkLength = indexInChunk;
			stringBuilder.m_ChunkPrevious = chunk;
			stringBuilder.m_ChunkOffset = chunk.m_ChunkOffset + chunk.m_ChunkLength;
			if (indexInChunk == 0)
			{
				stringBuilder.m_ChunkPrevious = chunk.m_ChunkPrevious;
				chunk = stringBuilder;
			}
		}
		stringBuilder.m_ChunkLength -= num2 - num3;
		if (num3 != num2)
		{
			ThreadSafeCopy(stringBuilder.m_ChunkChars, num2, stringBuilder.m_ChunkChars, num3, count2);
		}
	}
}
