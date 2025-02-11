using System.Collections.Generic;
using RimWorld;
using Verse;

namespace CombatExtended;

public class IncendiaryFuel : Filth
{
	private const float maxFireSize = 1.75f;

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		base.SpawnSetup(map, respawningAfterLoad);
		List<Thing> list = new List<Thing>(base.Position.GetThingList(map));
		foreach (Thing item in list)
		{
			if (item is Building_Door { Open: false })
			{
				Destroy();
				break;
			}
			if (item.HasAttachment(ThingDefOf.Fire))
			{
				Fire fire = (Fire)item.GetAttachment(ThingDefOf.Fire);
				if (fire != null)
				{
					fire.fireSize = 1.75f;
				}
			}
			else
			{
				item.TryAttachFire(1.75f, null);
			}
		}
	}

	public override void Tick()
	{
		if (base.Position.GetThingList(base.Map).Any((Thing x) => x.def == ThingDefOf.Filth_FireFoam) || base.Position.GetTerrain(base.Map).IsWater)
		{
			if (!base.Destroyed)
			{
				Destroy();
			}
		}
		else
		{
			FireUtility.TryStartFireIn(base.Position, base.Map, 1.75f, this);
		}
	}
}
