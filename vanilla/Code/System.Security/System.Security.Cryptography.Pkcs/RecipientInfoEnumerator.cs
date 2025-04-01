using System.Collections;
using Unity;

namespace System.Security.Cryptography.Pkcs;

/// <summary>The <see cref="T:System.Security.Cryptography.Pkcs.RecipientInfoEnumerator" /> class provides enumeration functionality for the <see cref="T:System.Security.Cryptography.Pkcs.RecipientInfoCollection" /> collection. <see cref="T:System.Security.Cryptography.Pkcs.RecipientInfoEnumerator" /> implements the <see cref="T:System.Collections.IEnumerator" /> interface. </summary>
public sealed class RecipientInfoEnumerator : IEnumerator
{
	private IEnumerator enumerator;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.RecipientInfoEnumerator.Current" /> property retrieves the current <see cref="T:System.Security.Cryptography.Pkcs.RecipientInfo" /> object from the <see cref="T:System.Security.Cryptography.Pkcs.RecipientInfoCollection" /> collection.</summary>
	/// <returns>A <see cref="T:System.Security.Cryptography.Pkcs.RecipientInfo" /> object that represents the current recipient information structure in the <see cref="T:System.Security.Cryptography.Pkcs.RecipientInfoCollection" /> collection.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public RecipientInfo Current => (RecipientInfo)enumerator.Current;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.RecipientInfoEnumerator.System#Collections#IEnumerator#Current" /> property retrieves the current <see cref="T:System.Security.Cryptography.Pkcs.RecipientInfo" /> object from the <see cref="T:System.Security.Cryptography.Pkcs.RecipientInfoCollection" /> collection.</summary>
	/// <returns>A <see cref="T:System.Security.Cryptography.Pkcs.RecipientInfo" /> object that represents the current recipient information structure in the <see cref="T:System.Security.Cryptography.Pkcs.RecipientInfoCollection" /> collection.</returns>
	object IEnumerator.Current => enumerator.Current;

	internal RecipientInfoEnumerator(IEnumerable enumerable)
	{
		enumerator = enumerable.GetEnumerator();
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.RecipientInfoEnumerator.MoveNext" /> method advances the enumeration to the next <see cref="T:System.Security.Cryptography.Pkcs.RecipientInfo" /> object in the <see cref="T:System.Security.Cryptography.Pkcs.RecipientInfoCollection" /> collection.</summary>
	/// <returns>This method returns a bool that specifies whether the enumeration successfully advanced. If the enumeration successfully moved to the next <see cref="T:System.Security.Cryptography.Pkcs.RecipientInfo" /> object, the method returns true. If the enumeration moved past the last item in the enumeration, it returns false.</returns>
	public bool MoveNext()
	{
		return enumerator.MoveNext();
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.RecipientInfoEnumerator.Reset" /> method resets the enumeration to the first <see cref="T:System.Security.Cryptography.Pkcs.RecipientInfo" /> object in the <see cref="T:System.Security.Cryptography.Pkcs.RecipientInfoCollection" /> collection.</summary>
	public void Reset()
	{
		enumerator.Reset();
	}

	internal RecipientInfoEnumerator()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
