using System.Linq;
using RimWorld;
using Verse;
using Verse.AI.Group;
using VFECore.Abilities;

namespace VanillaPsycastsExpanded.Harmonist;

public class HediffComp_MindControl : HediffComp_Ability
{
	private Faction oldFaction;

	private Lord oldLord;

	public override void CompPostPostAdd(DamageInfo? dinfo)
	{
		((HediffComp)this).CompPostPostAdd(dinfo);
		oldFaction = ((HediffComp)this).Pawn.Faction;
		oldLord = ((HediffComp)this).Pawn.GetLord();
		oldLord?.RemovePawn(((HediffComp)this).Pawn);
		((HediffComp)this).Pawn.SetFaction(base.ability.pawn.Faction, base.ability.pawn);
	}

	public override void CompPostPostRemoved()
	{
		((HediffComp)this).CompPostPostRemoved();
		((HediffComp)this).Pawn.SetFaction(oldFaction);
		if (!oldFaction.IsPlayer && !(oldLord?.AnyActivePawn ?? false))
		{
			if (((HediffComp)this).Pawn.Map.mapPawns.SpawnedPawnsInFaction(oldFaction).Except(((HediffComp)this).Pawn).Any())
			{
				oldLord = ((Pawn)GenClosest.ClosestThing_Global(((HediffComp)this).Pawn.Position, ((HediffComp)this).Pawn.Map.mapPawns.SpawnedPawnsInFaction(oldFaction), 99999f, (Thing p) => p != ((HediffComp)this).Pawn && ((Pawn)p).GetLord() != null)).GetLord();
			}
			if (oldLord == null)
			{
				LordJob_DefendPoint lordJob = new LordJob_DefendPoint(((HediffComp)this).Pawn.Position, null, null);
				oldLord = LordMaker.MakeNewLord(oldFaction, lordJob, ((HediffComp)this).Pawn.Map);
			}
		}
		oldLord?.AddPawn(((HediffComp)this).Pawn);
	}

	public override void CompExposeData()
	{
		((HediffComp)this).CompExposeData();
		Scribe_References.Look(ref oldFaction, "oldFaction");
		Scribe_References.Look(ref oldLord, "oldLord");
	}
}
