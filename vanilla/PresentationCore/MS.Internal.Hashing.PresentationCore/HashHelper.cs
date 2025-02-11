using System;
using System.Windows.Ink;
using System.Windows.Media;

namespace MS.Internal.Hashing.PresentationCore;

internal static class HashHelper
{
	static HashHelper()
	{
		Initialize();
		Type[] types = new Type[4]
		{
			typeof(CharacterMetrics),
			typeof(ExtendedProperty),
			typeof(FamilyTypeface),
			typeof(NumberSubstitution)
		};
		BaseHashHelper.RegisterTypes(typeof(HashHelper).Assembly, types);
	}

	internal static bool HasReliableHashCode(object item)
	{
		return BaseHashHelper.HasReliableHashCode(item);
	}

	internal static void Initialize()
	{
	}
}
