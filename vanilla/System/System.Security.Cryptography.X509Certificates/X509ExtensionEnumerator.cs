using System.Collections;
using Unity;

namespace System.Security.Cryptography.X509Certificates;

/// <summary>Supports a simple iteration over a <see cref="T:System.Security.Cryptography.X509Certificates.X509ExtensionCollection" />. This class cannot be inherited.</summary>
public sealed class X509ExtensionEnumerator : IEnumerator
{
	private IEnumerator enumerator;

	/// <summary>Gets the current element in the <see cref="T:System.Security.Cryptography.X509Certificates.X509ExtensionCollection" />.</summary>
	/// <returns>The current element in the <see cref="T:System.Security.Cryptography.X509Certificates.X509ExtensionCollection" />.</returns>
	/// <exception cref="T:System.InvalidOperationException">The enumerator is positioned before the first element of the collection or after the last element. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public X509Extension Current => (X509Extension)enumerator.Current;

	/// <summary>Gets an object from a collection.</summary>
	/// <returns>The current element in the <see cref="T:System.Security.Cryptography.X509Certificates.X509ExtensionCollection" />.</returns>
	/// <exception cref="T:System.InvalidOperationException">The enumerator is positioned before the first element of the collection or after the last element. </exception>
	object IEnumerator.Current => enumerator.Current;

	internal X509ExtensionEnumerator(ArrayList list)
	{
		enumerator = list.GetEnumerator();
	}

	/// <summary>Advances the enumerator to the next element in the <see cref="T:System.Security.Cryptography.X509Certificates.X509ExtensionCollection" />.</summary>
	/// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
	/// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
	public bool MoveNext()
	{
		return enumerator.MoveNext();
	}

	/// <summary>Sets the enumerator to its initial position, which is before the first element in the <see cref="T:System.Security.Cryptography.X509Certificates.X509ExtensionCollection" />.</summary>
	/// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
	public void Reset()
	{
		enumerator.Reset();
	}

	internal X509ExtensionEnumerator()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
