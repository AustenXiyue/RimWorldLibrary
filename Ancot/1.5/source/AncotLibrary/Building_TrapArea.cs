using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace AncotLibrary;

public class Building_TrapArea : Building_Trap
{
	private List<Pawn> touchingPawns = new List<Pawn>();

	public TrapAreaSpringRaduis_Extension Props => def.GetModExtension<TrapAreaSpringRaduis_Extension>();

	public override void Tick()
	{
		if (base.Spawned)
		{
			List<Thing> list = GenRadial.RadialDistinctThingsAround(base.Position, base.Map, Props.springRadius, useCenter: true).ToList();
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] is Pawn pawn && !touchingPawns.Contains(pawn) && pawn.HostileTo(base.Faction))
				{
					touchingPawns.Add(pawn);
					CheckSpring(pawn);
				}
			}
			for (int j = 0; j < touchingPawns.Count; j++)
			{
				Pawn pawn2 = touchingPawns[j];
				if (pawn2 == null || !pawn2.Spawned || pawn2.Position != base.Position)
				{
					touchingPawns.Remove(pawn2);
				}
			}
		}
		base.Tick();
	}

	private void CheckSpring(Pawn p)
	{
		if (Rand.Chance(SpringChance(p)))
		{
			Map map = base.Map;
			Spring(p);
		}
	}

	protected override void SpringSub(Pawn p)
	{
	}
}
