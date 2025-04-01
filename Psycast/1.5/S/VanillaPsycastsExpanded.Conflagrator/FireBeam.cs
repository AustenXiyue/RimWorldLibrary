using RimWorld;
using Verse;

namespace VanillaPsycastsExpanded.Conflagrator;

public class FireBeam : PowerBeam
{
	public override void StartStrike()
	{
		base.StartStrike();
		Mote firstThing = base.Position.GetFirstThing<Mote>(base.Map);
		firstThing.Destroy();
		firstThing = (Mote)ThingMaker.MakeThing(VPE_DefOf.VPE_Mote_FireBeam);
		firstThing.exactPosition = base.Position.ToVector3Shifted();
		firstThing.Scale = 90f;
		firstThing.rotationRate = 1.2f;
		GenSpawn.Spawn(firstThing, base.Position, base.Map);
	}
}
