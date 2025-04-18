using System.Collections;
using System.Runtime.InteropServices;
using Unity;

namespace System.Security.Permissions;

/// <summary>Represents the enumerator for <see cref="T:System.Security.Permissions.KeyContainerPermissionAccessEntry" /> objects in a <see cref="T:System.Security.Permissions.KeyContainerPermissionAccessEntryCollection" />.</summary>
[Serializable]
[ComVisible(true)]
public sealed class KeyContainerPermissionAccessEntryEnumerator : IEnumerator
{
	private IEnumerator e;

	/// <summary>Gets the current entry in the collection.</summary>
	/// <returns>The current <see cref="T:System.Security.Permissions.KeyContainerPermissionAccessEntry" /> object in the collection.</returns>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Security.Permissions.KeyContainerPermissionAccessEntryEnumerator.Current" /> property is accessed before first calling the <see cref="M:System.Security.Permissions.KeyContainerPermissionAccessEntryEnumerator.MoveNext" /> method. The cursor is located before the first object in the collection.-or- The <see cref="P:System.Security.Permissions.KeyContainerPermissionAccessEntryEnumerator.Current" /> property is accessed after a call to the <see cref="M:System.Security.Permissions.KeyContainerPermissionAccessEntryEnumerator.MoveNext" /> method returns false, which indicates that the cursor is located after the last object in the collection. </exception>
	public KeyContainerPermissionAccessEntry Current => (KeyContainerPermissionAccessEntry)e.Current;

	/// <summary>Gets the current object in the collection.</summary>
	/// <returns>The current object in the collection.</returns>
	object IEnumerator.Current => e.Current;

	internal KeyContainerPermissionAccessEntryEnumerator(ArrayList list)
	{
		e = list.GetEnumerator();
	}

	/// <summary>Moves to the next element in the collection.</summary>
	/// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
	public bool MoveNext()
	{
		return e.MoveNext();
	}

	/// <summary>Resets the enumerator to the beginning of the collection.</summary>
	public void Reset()
	{
		e.Reset();
	}

	internal KeyContainerPermissionAccessEntryEnumerator()
	{
		ThrowStub.ThrowNotSupportedException();
	}
}
