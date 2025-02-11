using System.Collections;
using Unity;

namespace System.Security.Cryptography.X509Certificates;

/// <summary>Supports a simple iteration over a <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2Collection" /> object. This class cannot be inherited.</summary>
public sealed class X509Certificate2Enumerator : IEnumerator
{
	private IEnumerator enumerator;

	/// <summary>Gets the current element in the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2Collection" /> object.</summary>
	/// <returns>The current element in the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2Collection" /> object.</returns>
	/// <exception cref="T:System.InvalidOperationException">The enumerator is positioned before the first element of the collection or after the last element. </exception>
	public X509Certificate2 Current => (X509Certificate2)enumerator.Current;

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IEnumerator.Current" />.</summary>
	/// <returns>The current element in the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2Collection" /> object.</returns>
	/// <exception cref="T:System.InvalidOperationException">The enumerator is positioned before the first element of the collection or after the last element. </exception>
	object IEnumerator.Current => enumerator.Current;

	internal X509Certificate2Enumerator(X509Certificate2Collection collection)
	{
		enumerator = ((IEnumerable)collection).GetEnumerator();
	}

	/// <summary>Advances the enumerator to the next element in the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2Collection" /> object.</summary>
	/// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
	/// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
	public bool MoveNext()
	{
		return enumerator.MoveNext();
	}

	/// <summary>Sets the enumerator to its initial position, which is before the first element in the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2Collection" /> object.</summary>
	/// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
	public void Reset()
	{
		enumerator.Reset();
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IEnumerator.MoveNext" />.</summary>
	/// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
	/// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
	bool IEnumerator.MoveNext()
	{
		return enumerator.MoveNext();
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IEnumerator.Reset" />.</summary>
	/// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
	void IEnumerator.Reset()
	{
		enumerator.Reset();
	}

	internal X509Certificate2Enumerator()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
