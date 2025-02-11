using Verse;

namespace CombatExtended;

public class ProjectileCE_SpawnsThing : ProjectileCE
{
	public override void Impact(Thing hitThing)
	{
		Map map = base.Map;
		base.Impact(hitThing);
		Thing thing = GenSpawn.Spawn(ThingMaker.MakeThing(def.projectile.spawnsThingDef), base.Position, map);
		if (thing.def.CanHaveFaction && launcher != null)
		{
			thing.SetFaction(launcher.Faction);
		}
	}
}
