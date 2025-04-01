using System.Collections;

namespace System.Security.Cryptography;

/// <summary>Provides the ability to navigate through an <see cref="T:System.Security.Cryptography.OidCollection" /> object. This class cannot be inherited.</summary>
public sealed class OidEnumerator : IEnumerator
{
	private OidCollection m_oids;

	private int m_current;

	/// <summary>Gets the current <see cref="T:System.Security.Cryptography.Oid" /> object in an <see cref="T:System.Security.Cryptography.OidCollection" /> object.</summary>
	/// <returns>The current <see cref="T:System.Security.Cryptography.Oid" /> object in the collection.</returns>
	public Oid Current => m_oids[m_current];

	/// <summary>Gets the current <see cref="T:System.Security.Cryptography.Oid" /> object in an <see cref="T:System.Security.Cryptography.OidCollection" /> object.</summary>
	/// <returns>The current <see cref="T:System.Security.Cryptography.Oid" /> object.</returns>
	object IEnumerator.Current => m_oids[m_current];

	private OidEnumerator()
	{
	}

	internal OidEnumerator(OidCollection oids)
	{
		m_oids = oids;
		m_current = -1;
	}

	/// <summary>Advances to the next <see cref="T:System.Security.Cryptography.Oid" /> object in an <see cref="T:System.Security.Cryptography.OidCollection" /> object.</summary>
	/// <returns>true, if the enumerator was successfully advanced to the next element; false, if the enumerator has passed the end of the collection.</returns>
	/// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created.</exception>
	public bool MoveNext()
	{
		if (m_current == m_oids.Count - 1)
		{
			return false;
		}
		m_current++;
		return true;
	}

	/// <summary>Sets an enumerator to its initial position.</summary>
	/// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created.</exception>
	public void Reset()
	{
		m_current = -1;
	}
}
