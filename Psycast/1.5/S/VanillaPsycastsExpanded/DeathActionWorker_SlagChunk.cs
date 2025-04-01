using RimWorld;
using Verse;
using Verse.AI.Group;

namespace VanillaPsycastsExpanded;

public class DeathActionWorker_SlagChunk : DeathActionWorker
{
	public override void PawnDied(Corpse corpse, Lord prevLord)
	{
		if (corpse.Map != null)
		{
			Thing newThing = ThingMaker.MakeThing(ThingDefOf.ChunkSlagSteel);
			GenSpawn.Spawn(newThing, corpse.Position, corpse.Map);
			corpse.Destroy();
		}
	}
}
