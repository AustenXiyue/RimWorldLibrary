using System.Security;

namespace System.Runtime.InteropServices;

/// <summary>Encapsulates an array and an offset within the specified array.</summary>
[Serializable]
[ComVisible(true)]
public struct ArrayWithOffset
{
	private object m_array;

	private int m_offset;

	private int m_count;

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.InteropServices.ArrayWithOffset" /> structure.</summary>
	/// <param name="array">A managed array. </param>
	/// <param name="offset">The offset in bytes, of the element to be passed through platform invoke. </param>
	/// <exception cref="T:System.ArgumentException">The array is larger than 2 gigabytes (GB).</exception>
	[SecuritySafeCritical]
	public ArrayWithOffset(object array, int offset)
	{
		m_array = array;
		m_offset = offset;
		m_count = 0;
		m_count = CalculateCount();
	}

	/// <summary>Returns the managed array referenced by this <see cref="T:System.Runtime.InteropServices.ArrayWithOffset" />.</summary>
	/// <returns>The managed array this instance references.</returns>
	public object GetArray()
	{
		return m_array;
	}

	/// <summary>Returns the offset provided when this <see cref="T:System.Runtime.InteropServices.ArrayWithOffset" /> was constructed.</summary>
	/// <returns>The offset for this instance.</returns>
	public int GetOffset()
	{
		return m_offset;
	}

	/// <summary>Returns a hash code for this value type.</summary>
	/// <returns>The hash code for this instance.</returns>
	public override int GetHashCode()
	{
		return m_count + m_offset;
	}

	/// <summary>Indicates whether the specified object matches the current <see cref="T:System.Runtime.InteropServices.ArrayWithOffset" /> object.</summary>
	/// <returns>true if the object matches this <see cref="T:System.Runtime.InteropServices.ArrayWithOffset" />; otherwise, false.</returns>
	/// <param name="obj">Object to compare with this instance. </param>
	public override bool Equals(object obj)
	{
		if (obj is ArrayWithOffset)
		{
			return Equals((ArrayWithOffset)obj);
		}
		return false;
	}

	/// <summary>Indicates whether the specified <see cref="T:System.Runtime.InteropServices.ArrayWithOffset" /> object matches the current instance.</summary>
	/// <returns>true if the specified <see cref="T:System.Runtime.InteropServices.ArrayWithOffset" /> object matches the current instance; otherwise, false.</returns>
	/// <param name="obj">An <see cref="T:System.Runtime.InteropServices.ArrayWithOffset" /> object to compare with this instance.</param>
	public bool Equals(ArrayWithOffset obj)
	{
		if (obj.m_array == m_array && obj.m_offset == m_offset)
		{
			return obj.m_count == m_count;
		}
		return false;
	}

	/// <summary>Determines whether two specified <see cref="T:System.Runtime.InteropServices.ArrayWithOffset" /> objects have the same value.</summary>
	/// <returns>true if the value of <paramref name="a" /> is the same as the value of <paramref name="b" />; otherwise, false.</returns>
	/// <param name="a">An <see cref="T:System.Runtime.InteropServices.ArrayWithOffset" /> object to compare with the <paramref name="b" /> parameter. </param>
	/// <param name="b">An <see cref="T:System.Runtime.InteropServices.ArrayWithOffset" /> object to compare with the <paramref name="a" /> parameter.</param>
	public static bool operator ==(ArrayWithOffset a, ArrayWithOffset b)
	{
		return a.Equals(b);
	}

	/// <summary>Determines whether two specified <see cref="T:System.Runtime.InteropServices.ArrayWithOffset" /> objects no not have the same value.</summary>
	/// <returns>true if the value of <paramref name="a" /> is not the same as the value of <paramref name="b" />; otherwise, false.</returns>
	/// <param name="a">An <see cref="T:System.Runtime.InteropServices.ArrayWithOffset" /> object to compare with the <paramref name="b" /> parameter. </param>
	/// <param name="b">An <see cref="T:System.Runtime.InteropServices.ArrayWithOffset" /> object to compare with the <paramref name="a" /> parameter.</param>
	public static bool operator !=(ArrayWithOffset a, ArrayWithOffset b)
	{
		return !(a == b);
	}

	private int CalculateCount()
	{
		if (!(m_array is Array array))
		{
			throw new ArgumentException();
		}
		return array.Rank * array.Length - m_offset;
	}
}
