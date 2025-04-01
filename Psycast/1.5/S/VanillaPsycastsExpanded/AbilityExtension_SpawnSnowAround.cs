using RimWorld.Planet;
using Verse;
using VFECore.Abilities;

namespace VanillaPsycastsExpanded;

public class AbilityExtension_SpawnSnowAround : AbilityExtension_AbilityMod
{
	public float radius;

	public float depth;

	public override void Cast(GlobalTargetInfo[] targets, Ability ability)
	{
		((AbilityExtension_AbilityMod)this).Cast(targets, ability);
		foreach (GlobalTargetInfo globalTargetInfo in targets)
		{
			foreach (IntVec3 item in GenRadial.RadialCellsAround(globalTargetInfo.Cell, radius, useCenter: true))
			{
				ability.pawn.Map.snowGrid.AddDepth(item, depth);
			}
		}
	}
}
