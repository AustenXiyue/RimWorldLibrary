using System.Collections;

namespace System.Windows.Data;

/// <summary>Represent the method that synchronizes a collection for cross-thread access.</summary>
/// <param name="collection">The collection to access on a thread other than the one that created it.</param>
/// <param name="context">An object used to synchronize the collection.</param>
/// <param name="accessMethod">A delegate to the method that performs the operation on the collection.</param>
/// <param name="writeAccess">true if <paramref name="accessMethod" /> writes to the collection; otherwise, false.</param>
public delegate void CollectionSynchronizationCallback(IEnumerable collection, object context, Action accessMethod, bool writeAccess);
