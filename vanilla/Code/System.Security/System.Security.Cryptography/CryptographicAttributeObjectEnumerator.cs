using System.Collections;
using Unity;

namespace System.Security.Cryptography;

/// <summary>Provides enumeration functionality for the <see cref="T:System.Security.Cryptography.CryptographicAttributeObjectCollection" /> collection. This class cannot be inherited. </summary>
public sealed class CryptographicAttributeObjectEnumerator : IEnumerator
{
	private IEnumerator enumerator;

	/// <summary>Gets the current <see cref="T:System.Security.Cryptography.CryptographicAttributeObject" /> object from the <see cref="T:System.Security.Cryptography.CryptographicAttributeObjectCollection" /> collection.</summary>
	/// <returns>A <see cref="T:System.Security.Cryptography.CryptographicAttributeObject" /> object that represents the current cryptographic attribute in the <see cref="T:System.Security.Cryptography.CryptographicAttributeObjectCollection" /> collection.</returns>
	public CryptographicAttributeObject Current => (CryptographicAttributeObject)enumerator.Current;

	/// <summary>Gets the current <see cref="T:System.Security.Cryptography.CryptographicAttributeObject" /> object from the <see cref="T:System.Security.Cryptography.CryptographicAttributeObjectCollection" /> collection.</summary>
	/// <returns>A <see cref="T:System.Security.Cryptography.CryptographicAttributeObject" /> object that represents the current cryptographic attribute in the <see cref="T:System.Security.Cryptography.CryptographicAttributeObjectCollection" /> collection.</returns>
	object IEnumerator.Current => enumerator.Current;

	internal CryptographicAttributeObjectEnumerator(IEnumerable enumerable)
	{
		enumerator = enumerable.GetEnumerator();
	}

	/// <summary>Advances the enumeration to the next <see cref="T:System.Security.Cryptography.CryptographicAttributeObject" /> object in the <see cref="T:System.Security.Cryptography.CryptographicAttributeObjectCollection" /> collection.</summary>
	/// <returns>true if the enumeration successfully moved to the next <see cref="T:System.Security.Cryptography.CryptographicAttributeObject" /> object; false if the enumerator is at the end of the enumeration.</returns>
	public bool MoveNext()
	{
		return enumerator.MoveNext();
	}

	/// <summary>Resets the enumeration to the first <see cref="T:System.Security.Cryptography.CryptographicAttributeObject" /> object in the <see cref="T:System.Security.Cryptography.CryptographicAttributeObjectCollection" /> collection.</summary>
	public void Reset()
	{
		enumerator.Reset();
	}

	internal CryptographicAttributeObjectEnumerator()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
