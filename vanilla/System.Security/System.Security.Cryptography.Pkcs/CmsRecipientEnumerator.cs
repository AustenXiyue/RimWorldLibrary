using System.Collections;
using Unity;

namespace System.Security.Cryptography.Pkcs;

/// <summary>The <see cref="T:System.Security.Cryptography.Pkcs.CmsRecipientEnumerator" /> class provides enumeration functionality for the <see cref="T:System.Security.Cryptography.Pkcs.CmsRecipientCollection" /> collection. <see cref="T:System.Security.Cryptography.Pkcs.CmsRecipientEnumerator" /> implements the <see cref="T:System.Collections.IEnumerator" /> interface. </summary>
public sealed class CmsRecipientEnumerator : IEnumerator
{
	private IEnumerator enumerator;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.CmsRecipientEnumerator.Current" /> property retrieves the current <see cref="T:System.Security.Cryptography.Pkcs.CmsRecipient" /> object from the <see cref="T:System.Security.Cryptography.Pkcs.CmsRecipientCollection" /> collection.</summary>
	/// <returns>A <see cref="T:System.Security.Cryptography.Pkcs.CmsRecipient" /> object that represents the current recipient in the <see cref="T:System.Security.Cryptography.Pkcs.CmsRecipientCollection" /> collection.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public CmsRecipient Current => (CmsRecipient)enumerator.Current;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.CmsRecipientEnumerator.System#Collections#IEnumerator#Current" /> property retrieves the current <see cref="T:System.Security.Cryptography.Pkcs.CmsRecipient" /> object from the <see cref="T:System.Security.Cryptography.Pkcs.CmsRecipientCollection" /> collection.</summary>
	/// <returns>A <see cref="T:System.Security.Cryptography.Pkcs.CmsRecipient" /> object that represents the current recipient in the <see cref="T:System.Security.Cryptography.Pkcs.CmsRecipientCollection" /> collection.</returns>
	object IEnumerator.Current => enumerator.Current;

	internal CmsRecipientEnumerator(IEnumerable enumerable)
	{
		enumerator = enumerable.GetEnumerator();
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.CmsRecipientEnumerator.MoveNext" /> method advances the enumeration to the next <see cref="T:System.Security.Cryptography.Pkcs.CmsRecipient" /> object in the <see cref="T:System.Security.Cryptography.Pkcs.CmsRecipientCollection" /> collection.</summary>
	/// <returns>true if the enumeration successfully moved to the next <see cref="T:System.Security.Cryptography.Pkcs.CmsRecipient" /> object; false if the enumeration moved past the last item in the enumeration.</returns>
	public bool MoveNext()
	{
		return enumerator.MoveNext();
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.CmsRecipientEnumerator.Reset" /> method resets the enumeration to the first <see cref="T:System.Security.Cryptography.Pkcs.CmsRecipient" /> object in the <see cref="T:System.Security.Cryptography.Pkcs.CmsRecipientCollection" /> collection.</summary>
	public void Reset()
	{
		enumerator.Reset();
	}

	internal CmsRecipientEnumerator()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
