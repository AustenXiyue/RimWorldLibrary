using System.Collections.Generic;
using Verse;
using VFECore.Abilities;

namespace VanillaPsycastsExpanded;

public class PawnKindAbilityExtension_Psycasts : PawnKindAbilityExtension
{
	public List<PathUnlockData> unlockedPaths;

	public IntRange statUpgradePoints = IntRange.zero;
}
