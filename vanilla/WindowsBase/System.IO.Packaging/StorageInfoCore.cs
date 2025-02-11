using System.Collections;
using MS.Internal.IO.Packaging.CompoundFile;

namespace System.IO.Packaging;

internal class StorageInfoCore
{
	internal string storageName;

	internal IStorage safeIStorage;

	internal Hashtable validEnumerators;

	internal Hashtable elementInfoCores;

	internal StorageInfoCore(string nameStorage)
		: this(nameStorage, null)
	{
	}

	internal StorageInfoCore(string nameStorage, IStorage storage)
	{
		storageName = nameStorage;
		safeIStorage = storage;
		validEnumerators = new Hashtable();
		elementInfoCores = new Hashtable(ContainerUtilities.StringCaseInsensitiveComparer);
	}
}
