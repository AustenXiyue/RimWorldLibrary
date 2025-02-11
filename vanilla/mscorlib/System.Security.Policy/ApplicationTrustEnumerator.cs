using System.Collections;
using System.Runtime.InteropServices;
using Unity;

namespace System.Security.Policy;

/// <summary>Represents the enumerator for <see cref="T:System.Security.Policy.ApplicationTrust" /> objects in the <see cref="T:System.Security.Policy.ApplicationTrustCollection" /> collection.</summary>
[ComVisible(true)]
public sealed class ApplicationTrustEnumerator : IEnumerator
{
	private IEnumerator e;

	/// <summary>Gets the current <see cref="T:System.Security.Policy.ApplicationTrust" /> object in the <see cref="T:System.Security.Policy.ApplicationTrustCollection" /> collection.</summary>
	/// <returns>The current <see cref="T:System.Security.Policy.ApplicationTrust" /> in the <see cref="T:System.Security.Policy.ApplicationTrustCollection" />.</returns>
	public ApplicationTrust Current => (ApplicationTrust)e.Current;

	/// <summary>Gets the current <see cref="T:System.Object" /> in the <see cref="T:System.Security.Policy.ApplicationTrustCollection" /> collection.</summary>
	/// <returns>The current <see cref="T:System.Object" /> in the <see cref="T:System.Security.Policy.ApplicationTrustCollection" />.</returns>
	object IEnumerator.Current => e.Current;

	internal ApplicationTrustEnumerator(ApplicationTrustCollection collection)
	{
		e = collection.GetEnumerator();
	}

	/// <summary>Moves to the next element in the <see cref="T:System.Security.Policy.ApplicationTrustCollection" /> collection.</summary>
	/// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[SecuritySafeCritical]
	public bool MoveNext()
	{
		return e.MoveNext();
	}

	/// <summary>Resets the enumerator to the beginning of the <see cref="T:System.Security.Policy.ApplicationTrustCollection" /> collection.</summary>
	public void Reset()
	{
		e.Reset();
	}

	internal ApplicationTrustEnumerator()
	{
		ThrowStub.ThrowNotSupportedException();
	}
}
