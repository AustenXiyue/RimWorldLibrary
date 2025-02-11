using System;
using Verse;

namespace CombatExtended.Loader;

public interface IModPart
{
	void PostLoad(ModContentPack content, ISettingsCE settings);

	Type GetSettingsType();
}
