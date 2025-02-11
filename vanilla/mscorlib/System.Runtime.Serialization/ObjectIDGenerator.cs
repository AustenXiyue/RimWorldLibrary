using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Runtime.Serialization;

/// <summary>Generates IDs for objects.</summary>
[Serializable]
[ComVisible(true)]
public class ObjectIDGenerator
{
	private const int numbins = 4;

	internal int m_currentCount;

	internal int m_currentSize;

	internal long[] m_ids;

	internal object[] m_objs;

	private static readonly int[] sizes = new int[21]
	{
		5, 11, 29, 47, 97, 197, 397, 797, 1597, 3203,
		6421, 12853, 25717, 51437, 102877, 205759, 411527, 823117, 1646237, 3292489,
		6584983
	};

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Serialization.ObjectIDGenerator" /> class.</summary>
	public ObjectIDGenerator()
	{
		m_currentCount = 1;
		m_currentSize = sizes[0];
		m_ids = new long[m_currentSize * 4];
		m_objs = new object[m_currentSize * 4];
	}

	private int FindElement(object obj, out bool found)
	{
		int num = RuntimeHelpers.GetHashCode(obj);
		int num2 = 1 + (num & 0x7FFFFFFF) % (m_currentSize - 2);
		while (true)
		{
			int num3 = (num & 0x7FFFFFFF) % m_currentSize * 4;
			for (int i = num3; i < num3 + 4; i++)
			{
				if (m_objs[i] == null)
				{
					found = false;
					return i;
				}
				if (m_objs[i] == obj)
				{
					found = true;
					return i;
				}
			}
			num += num2;
		}
	}

	/// <summary>Returns the ID for the specified object, generating a new ID if the specified object has not already been identified by the <see cref="T:System.Runtime.Serialization.ObjectIDGenerator" />.</summary>
	/// <returns>The object's ID is used for serialization. <paramref name="firstTime" /> is set to true if this is the first time the object has been identified; otherwise, it is set to false.</returns>
	/// <param name="obj">The object you want an ID for. </param>
	/// <param name="firstTime">true if <paramref name="obj" /> was not previously known to the <see cref="T:System.Runtime.Serialization.ObjectIDGenerator" />; otherwise, false. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="obj" /> parameter is null. </exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">The <see cref="T:System.Runtime.Serialization.ObjectIDGenerator" /> has been asked to keep track of too many objects. </exception>
	public virtual long GetId(object obj, out bool firstTime)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj", Environment.GetResourceString("Object cannot be null."));
		}
		bool found;
		int num = FindElement(obj, out found);
		long result;
		if (!found)
		{
			m_objs[num] = obj;
			m_ids[num] = m_currentCount++;
			result = m_ids[num];
			if (m_currentCount > m_currentSize * 4 / 2)
			{
				Rehash();
			}
		}
		else
		{
			result = m_ids[num];
		}
		firstTime = !found;
		return result;
	}

	/// <summary>Determines whether an object has already been assigned an ID.</summary>
	/// <returns>The object ID of <paramref name="obj" /> if previously known to the <see cref="T:System.Runtime.Serialization.ObjectIDGenerator" />; otherwise, zero.</returns>
	/// <param name="obj">The object you are asking for. </param>
	/// <param name="firstTime">true if <paramref name="obj" /> was not previously known to the <see cref="T:System.Runtime.Serialization.ObjectIDGenerator" />; otherwise, false. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="obj" /> parameter is null. </exception>
	public virtual long HasId(object obj, out bool firstTime)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj", Environment.GetResourceString("Object cannot be null."));
		}
		bool found;
		int num = FindElement(obj, out found);
		if (found)
		{
			firstTime = false;
			return m_ids[num];
		}
		firstTime = true;
		return 0L;
	}

	private void Rehash()
	{
		int i = 0;
		for (int currentSize = m_currentSize; i < sizes.Length && sizes[i] <= currentSize; i++)
		{
		}
		if (i == sizes.Length)
		{
			throw new SerializationException(Environment.GetResourceString("The internal array cannot expand to greater than Int32.MaxValue elements."));
		}
		m_currentSize = sizes[i];
		long[] ids = new long[m_currentSize * 4];
		object[] objs = new object[m_currentSize * 4];
		long[] ids2 = m_ids;
		object[] objs2 = m_objs;
		m_ids = ids;
		m_objs = objs;
		for (int j = 0; j < objs2.Length; j++)
		{
			if (objs2[j] != null)
			{
				bool found;
				int num = FindElement(objs2[j], out found);
				m_objs[num] = objs2[j];
				m_ids[num] = ids2[j];
			}
		}
	}
}
