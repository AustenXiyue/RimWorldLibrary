using System;

namespace MS.Internal.Hashing.WindowsBase;

internal static class HashHelper
{
	static HashHelper()
	{
		Initialize();
		Type[] emptyTypes = Type.EmptyTypes;
		BaseHashHelper.RegisterTypes(typeof(HashHelper).Assembly, emptyTypes);
	}

	internal static bool HasReliableHashCode(object item)
	{
		return BaseHashHelper.HasReliableHashCode(item);
	}

	internal static void Initialize()
	{
	}
}
