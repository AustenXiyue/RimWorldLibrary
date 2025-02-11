using System;
using System.Windows;
using System.Windows.Markup.Localizer;
using MS.Internal.Hashing.PresentationCore;

namespace MS.Internal.Hashing.PresentationFramework;

internal static class HashHelper
{
	static HashHelper()
	{
		Initialize();
		Type[] types = new Type[2]
		{
			typeof(BamlLocalizableResource),
			typeof(ComponentResourceKey)
		};
		BaseHashHelper.RegisterTypes(typeof(HashHelper).Assembly, types);
		MS.Internal.Hashing.PresentationCore.HashHelper.Initialize();
	}

	internal static bool HasReliableHashCode(object item)
	{
		return BaseHashHelper.HasReliableHashCode(item);
	}

	internal static void Initialize()
	{
	}
}
