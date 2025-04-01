using RimWorld;
using Verse;
using VFECore.Abilities;

namespace VanillaPsycastsExpanded.Wildspeaker;

public class BrambleSpawner : PlantSpawner
{
	protected override void SetupPlant(Plant plant, IntVec3 loc, Map map)
	{
		base.SetupPlant(plant, loc, map);
		CompDuration val = ((Thing)this).TryGetComp<CompDuration>();
		if (val != null)
		{
			int durationTicksLeft = val.durationTicksLeft;
			if (true)
			{
				Current.Game.GetComponent<GameComponent_PsycastsManager>().removeAfterTicks.Add((plant, Find.TickManager.TicksGame + durationTicksLeft));
			}
		}
	}

	protected override ThingDef ChoosePlant(IntVec3 loc, Map map)
	{
		return VPE_DefOf.Plant_Brambles;
	}
}
