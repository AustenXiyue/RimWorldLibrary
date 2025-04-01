using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaPsycastsExpanded.Staticlord;

public class Hediff_Vortexed : Hediff
{
	public Vortex Vortex;

	public override HediffStage CurStage => (Vortex == null) ? base.CurStage : new HediffStage
	{
		capMods = new List<PawnCapacityModifier>
		{
			new PawnCapacityModifier
			{
				capacity = PawnCapacityDefOf.Moving,
				setMax = Mathf.Lerp(0.5f, 0.9f, pawn.Position.DistanceTo(Vortex.Position) / 18.9f)
			},
			new PawnCapacityModifier
			{
				capacity = PawnCapacityDefOf.Manipulation,
				setMax = Mathf.Lerp(0.5f, 0.9f, pawn.Position.DistanceTo(Vortex.Position) / 18.9f)
			}
		}
	};

	public override bool ShouldRemove => Vortex.Destroyed || pawn.Position.DistanceTo(Vortex.Position) >= 18.9f;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref Vortex, "vortex");
	}
}
