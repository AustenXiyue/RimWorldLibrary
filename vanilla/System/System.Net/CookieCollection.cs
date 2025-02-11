using System.Collections;
using System.Runtime.Serialization;

namespace System.Net;

/// <summary>Provides a collection container for instances of the <see cref="T:System.Net.Cookie" /> class.</summary>
[Serializable]
public class CookieCollection : ICollection, IEnumerable
{
	internal enum Stamp
	{
		Check,
		Set,
		SetToUnused,
		SetToMaxUsed
	}

	private class CookieCollectionEnumerator : IEnumerator
	{
		private CookieCollection m_cookies;

		private int m_count;

		private int m_index = -1;

		private int m_version;

		object IEnumerator.Current
		{
			get
			{
				if (m_index < 0 || m_index >= m_count)
				{
					throw new InvalidOperationException(global::SR.GetString("Enumeration has either not started or has already finished."));
				}
				if (m_version != m_cookies.m_version)
				{
					throw new InvalidOperationException(global::SR.GetString("Collection was modified; enumeration operation may not execute."));
				}
				return m_cookies[m_index];
			}
		}

		internal CookieCollectionEnumerator(CookieCollection cookies)
		{
			m_cookies = cookies;
			m_count = cookies.Count;
			m_version = cookies.m_version;
		}

		bool IEnumerator.MoveNext()
		{
			if (m_version != m_cookies.m_version)
			{
				throw new InvalidOperationException(global::SR.GetString("Collection was modified; enumeration operation may not execute."));
			}
			if (++m_index < m_count)
			{
				return true;
			}
			m_index = m_count;
			return false;
		}

		void IEnumerator.Reset()
		{
			m_index = -1;
		}
	}

	internal int m_version;

	private ArrayList m_list = new ArrayList();

	private DateTime m_TimeStamp = DateTime.MinValue;

	private bool m_has_other_versions;

	[OptionalField]
	private bool m_IsReadOnly;

	/// <summary>Gets a value that indicates whether a <see cref="T:System.Net.CookieCollection" /> is read-only.</summary>
	/// <returns>true if this is a read-only <see cref="T:System.Net.CookieCollection" />; otherwise, false. The default is true.</returns>
	public bool IsReadOnly => m_IsReadOnly;

	/// <summary>Gets the <see cref="T:System.Net.Cookie" /> with a specific index from a <see cref="T:System.Net.CookieCollection" />.</summary>
	/// <returns>A <see cref="T:System.Net.Cookie" /> with a specific index from a <see cref="T:System.Net.CookieCollection" />.</returns>
	/// <param name="index">The zero-based index of the <see cref="T:System.Net.Cookie" /> to be found. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than 0 or <paramref name="index" /> is greater than or equal to <see cref="P:System.Net.CookieCollection.Count" />. </exception>
	public Cookie this[int index]
	{
		get
		{
			if (index < 0 || index >= m_list.Count)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			return (Cookie)m_list[index];
		}
	}

	/// <summary>Gets the <see cref="T:System.Net.Cookie" /> with a specific name from a <see cref="T:System.Net.CookieCollection" />.</summary>
	/// <returns>The <see cref="T:System.Net.Cookie" /> with a specific name from a <see cref="T:System.Net.CookieCollection" />.</returns>
	/// <param name="name">The name of the <see cref="T:System.Net.Cookie" /> to be found. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	public Cookie this[string name]
	{
		get
		{
			foreach (Cookie item in m_list)
			{
				if (string.Compare(item.Name, name, StringComparison.OrdinalIgnoreCase) == 0)
				{
					return item;
				}
			}
			return null;
		}
	}

	/// <summary>Gets the number of cookies contained in a <see cref="T:System.Net.CookieCollection" />.</summary>
	/// <returns>The number of cookies contained in a <see cref="T:System.Net.CookieCollection" />.</returns>
	public int Count => m_list.Count;

	/// <summary>Gets a value that indicates whether access to a <see cref="T:System.Net.CookieCollection" /> is thread safe.</summary>
	/// <returns>true if access to the <see cref="T:System.Net.CookieCollection" /> is thread safe; otherwise, false. The default is false.</returns>
	public bool IsSynchronized => false;

	/// <summary>Gets an object to synchronize access to the <see cref="T:System.Net.CookieCollection" />.</summary>
	/// <returns>An object to synchronize access to the <see cref="T:System.Net.CookieCollection" />.</returns>
	public object SyncRoot => this;

	internal bool IsOtherVersionSeen => m_has_other_versions;

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.CookieCollection" /> class.</summary>
	public CookieCollection()
	{
		m_IsReadOnly = true;
	}

	internal CookieCollection(bool IsReadOnly)
	{
		m_IsReadOnly = IsReadOnly;
	}

	/// <summary>Adds a <see cref="T:System.Net.Cookie" /> to a <see cref="T:System.Net.CookieCollection" />.</summary>
	/// <param name="cookie">The <see cref="T:System.Net.Cookie" /> to be added to a <see cref="T:System.Net.CookieCollection" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="cookie" /> is null. </exception>
	public void Add(Cookie cookie)
	{
		if (cookie == null)
		{
			throw new ArgumentNullException("cookie");
		}
		m_version++;
		int num = IndexOf(cookie);
		if (num == -1)
		{
			m_list.Add(cookie);
		}
		else
		{
			m_list[num] = cookie;
		}
	}

	/// <summary>Adds the contents of a <see cref="T:System.Net.CookieCollection" /> to the current instance.</summary>
	/// <param name="cookies">The <see cref="T:System.Net.CookieCollection" /> to be added. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="cookies" /> is null. </exception>
	public void Add(CookieCollection cookies)
	{
		if (cookies == null)
		{
			throw new ArgumentNullException("cookies");
		}
		foreach (Cookie cookie in cookies)
		{
			Add(cookie);
		}
	}

	/// <summary>Copies the elements of a <see cref="T:System.Net.CookieCollection" /> to an instance of the <see cref="T:System.Array" /> class, starting at a particular index.</summary>
	/// <param name="array">The target <see cref="T:System.Array" /> to which the <see cref="T:System.Net.CookieCollection" /> will be copied. </param>
	/// <param name="index">The zero-based index in the target <see cref="T:System.Array" /> where copying begins. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="array" /> is multidimensional.-or- The number of elements in this <see cref="T:System.Net.CookieCollection" /> is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />. </exception>
	/// <exception cref="T:System.InvalidCastException">The elements in this <see cref="T:System.Net.CookieCollection" /> cannot be cast automatically to the type of the destination <paramref name="array" />. </exception>
	public void CopyTo(Array array, int index)
	{
		m_list.CopyTo(array, index);
	}

	/// <summary>Copies the elements of this <see cref="T:System.Net.CookieCollection" /> to a <see cref="T:System.Net.Cookie" /> array starting at the specified index of the target array.</summary>
	/// <param name="array">The target <see cref="T:System.Net.Cookie" /> array to which the <see cref="T:System.Net.CookieCollection" /> will be copied.</param>
	/// <param name="index">The zero-based index in the target <see cref="T:System.Array" /> where copying begins. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="array" /> is multidimensional.-or- The number of elements in this <see cref="T:System.Net.CookieCollection" /> is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />. </exception>
	/// <exception cref="T:System.InvalidCastException">The elements in this <see cref="T:System.Net.CookieCollection" /> cannot be cast automatically to the type of the destination <paramref name="array" />. </exception>
	public void CopyTo(Cookie[] array, int index)
	{
		m_list.CopyTo(array, index);
	}

	internal DateTime TimeStamp(Stamp how)
	{
		switch (how)
		{
		case Stamp.Set:
			m_TimeStamp = DateTime.Now;
			break;
		case Stamp.SetToMaxUsed:
			m_TimeStamp = DateTime.MaxValue;
			break;
		case Stamp.SetToUnused:
			m_TimeStamp = DateTime.MinValue;
			break;
		}
		return m_TimeStamp;
	}

	internal int InternalAdd(Cookie cookie, bool isStrict)
	{
		int result = 1;
		if (isStrict)
		{
			IComparer comparer = Cookie.GetComparer();
			int num = 0;
			foreach (Cookie item in m_list)
			{
				if (comparer.Compare(cookie, item) == 0)
				{
					result = 0;
					if (item.Variant <= cookie.Variant)
					{
						m_list[num] = cookie;
					}
					break;
				}
				num++;
			}
			if (num == m_list.Count)
			{
				m_list.Add(cookie);
			}
		}
		else
		{
			m_list.Add(cookie);
		}
		if (cookie.Version != 1)
		{
			m_has_other_versions = true;
		}
		return result;
	}

	internal int IndexOf(Cookie cookie)
	{
		IComparer comparer = Cookie.GetComparer();
		int num = 0;
		foreach (Cookie item in m_list)
		{
			if (comparer.Compare(cookie, item) == 0)
			{
				return num;
			}
			num++;
		}
		return -1;
	}

	internal void RemoveAt(int idx)
	{
		m_list.RemoveAt(idx);
	}

	/// <summary>Gets an enumerator that can iterate through a <see cref="T:System.Net.CookieCollection" />.</summary>
	/// <returns>An instance of an implementation of an <see cref="T:System.Collections.IEnumerator" /> interface that can iterate through a <see cref="T:System.Net.CookieCollection" />.</returns>
	public IEnumerator GetEnumerator()
	{
		return new CookieCollectionEnumerator(this);
	}
}
