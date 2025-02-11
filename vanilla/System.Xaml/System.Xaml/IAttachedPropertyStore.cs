using System.Collections.Generic;

namespace System.Xaml;

/// <summary>Represents an attachable member store for an object where attachable members are set. This attachable member store can then be referenced with <see cref="T:System.Xaml.AttachablePropertyServices" />.</summary>
public interface IAttachedPropertyStore
{
	/// <summary>Gets the count of the attachable member entries in this attachable member store.</summary>
	/// <returns>The integer count of entries in the store.</returns>
	int PropertyCount { get; }

	/// <summary>Copies all attachable member/value pairs from this attachable member store into a destination array.</summary>
	/// <param name="array">The destination array. The array is a generic array, should be passed undimensioned, and should have components of <see cref="T:System.Xaml.AttachableMemberIdentifier" /> and object.</param>
	/// <param name="index">The source index where copying should begin.</param>
	void CopyPropertiesTo(KeyValuePair<AttachableMemberIdentifier, object>[] array, int index);

	/// <summary>Removes the entry for the specified attachable member from this attachable member store.</summary>
	/// <returns>true if an attachable member entry for <paramref name="attachableMemberIdentifier" /> was found in the store and removed; otherwise, false.</returns>
	/// <param name="attachableMemberIdentifier">The XAML type system identifier for the attachable member entry to remove.</param>
	bool RemoveProperty(AttachableMemberIdentifier attachableMemberIdentifier);

	/// <summary>Sets a value for the specified attachable member in the specified store.</summary>
	/// <param name="attachableMemberIdentifier">The XAML type system identifier for the attachable member entry to set.</param>
	/// <param name="value">The value to set.</param>
	void SetProperty(AttachableMemberIdentifier attachableMemberIdentifier, object value);

	/// <summary>Attempts to get a value for the specified attachable member in the specified store. </summary>
	/// <returns>true if an attachable member entry for <paramref name="attachableMemberIdentifier" /> was found in the store and a value was posted to <paramref name="value" />; otherwise, false.</returns>
	/// <param name="attachableMemberIdentifier">The XAML type system identifier for the attachable member entry to get.</param>
	/// <param name="value">Out parameter. When this method returns, contains the destination object for the value if <paramref name="attachableMemberIdentifier" /> exists in the store and has a value.</param>
	bool TryGetProperty(AttachableMemberIdentifier attachableMemberIdentifier, out object value);
}
