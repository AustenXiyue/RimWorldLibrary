using System.Collections;
using System.Runtime.InteropServices;
using Unity;

namespace System.Runtime.Serialization;

/// <summary>Provides a formatter-friendly mechanism for parsing the data in <see cref="T:System.Runtime.Serialization.SerializationInfo" />. This class cannot be inherited.</summary>
[ComVisible(true)]
public sealed class SerializationInfoEnumerator : IEnumerator
{
	private string[] m_members;

	private object[] m_data;

	private Type[] m_types;

	private int m_numItems;

	private int m_currItem;

	private bool m_current;

	/// <summary>Gets the current item in the collection.</summary>
	/// <returns>A <see cref="T:System.Runtime.Serialization.SerializationEntry" /> that contains the current serialization data.</returns>
	/// <exception cref="T:System.InvalidOperationException">The enumeration has not started or has already ended. </exception>
	object IEnumerator.Current
	{
		get
		{
			if (!m_current)
			{
				throw new InvalidOperationException(Environment.GetResourceString("Enumeration has either not started or has already finished."));
			}
			return new SerializationEntry(m_members[m_currItem], m_data[m_currItem], m_types[m_currItem]);
		}
	}

	/// <summary>Gets the item currently being examined.</summary>
	/// <returns>The item currently being examined.</returns>
	/// <exception cref="T:System.InvalidOperationException">The enumerator has not started enumerating items or has reached the end of the enumeration. </exception>
	public SerializationEntry Current
	{
		get
		{
			if (!m_current)
			{
				throw new InvalidOperationException(Environment.GetResourceString("Enumeration has either not started or has already finished."));
			}
			return new SerializationEntry(m_members[m_currItem], m_data[m_currItem], m_types[m_currItem]);
		}
	}

	/// <summary>Gets the name for the item currently being examined.</summary>
	/// <returns>The item name.</returns>
	/// <exception cref="T:System.InvalidOperationException">The enumerator has not started enumerating items or has reached the end of the enumeration. </exception>
	public string Name
	{
		get
		{
			if (!m_current)
			{
				throw new InvalidOperationException(Environment.GetResourceString("Enumeration has either not started or has already finished."));
			}
			return m_members[m_currItem];
		}
	}

	/// <summary>Gets the value of the item currently being examined.</summary>
	/// <returns>The value of the item currently being examined.</returns>
	/// <exception cref="T:System.InvalidOperationException">The enumerator has not started enumerating items or has reached the end of the enumeration. </exception>
	public object Value
	{
		get
		{
			if (!m_current)
			{
				throw new InvalidOperationException(Environment.GetResourceString("Enumeration has either not started or has already finished."));
			}
			return m_data[m_currItem];
		}
	}

	/// <summary>Gets the type of the item currently being examined.</summary>
	/// <returns>The type of the item currently being examined.</returns>
	/// <exception cref="T:System.InvalidOperationException">The enumerator has not started enumerating items or has reached the end of the enumeration. </exception>
	public Type ObjectType
	{
		get
		{
			if (!m_current)
			{
				throw new InvalidOperationException(Environment.GetResourceString("Enumeration has either not started or has already finished."));
			}
			return m_types[m_currItem];
		}
	}

	internal SerializationInfoEnumerator(string[] members, object[] info, Type[] types, int numItems)
	{
		m_members = members;
		m_data = info;
		m_types = types;
		m_numItems = numItems - 1;
		m_currItem = -1;
		m_current = false;
	}

	/// <summary>Updates the enumerator to the next item.</summary>
	/// <returns>true if a new element is found; otherwise, false.</returns>
	public bool MoveNext()
	{
		if (m_currItem < m_numItems)
		{
			m_currItem++;
			m_current = true;
		}
		else
		{
			m_current = false;
		}
		return m_current;
	}

	/// <summary>Resets the enumerator to the first item.</summary>
	public void Reset()
	{
		m_currItem = -1;
		m_current = false;
	}

	internal SerializationInfoEnumerator()
	{
		ThrowStub.ThrowNotSupportedException();
	}
}
