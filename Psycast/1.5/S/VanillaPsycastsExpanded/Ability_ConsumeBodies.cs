using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using VFECore.Abilities;

namespace VanillaPsycastsExpanded;

public class Ability_ConsumeBodies : Ability_TargetCorpse
{
	public override void WarmupToil(Toil toil)
	{
		((Ability)this).WarmupToil(toil);
		toil.AddPreInitAction(delegate
		{
			GlobalTargetInfo[] currentlyCastingTargets = ((Ability)this).Comp.currentlyCastingTargets;
			for (int i = 0; i < currentlyCastingTargets.Length; i++)
			{
				GlobalTargetInfo globalTargetInfo = currentlyCastingTargets[i];
				if (globalTargetInfo.HasThing && globalTargetInfo.Thing.TryGetComp<CompRottable>() != null)
				{
					((Ability)this).AddEffecterToMaintain(VPE_DefOf.VPE_Liferot.Spawn(globalTargetInfo.Thing.Position, ((Ability)this).pawn.Map), (TargetInfo)globalTargetInfo.Thing, toil.defaultDuration);
				}
			}
		});
		toil.AddPreTickAction(delegate
		{
			GlobalTargetInfo[] currentlyCastingTargets2 = ((Ability)this).Comp.currentlyCastingTargets;
			for (int j = 0; j < currentlyCastingTargets2.Length; j++)
			{
				GlobalTargetInfo globalTargetInfo2 = currentlyCastingTargets2[j];
				if (globalTargetInfo2.HasThing && globalTargetInfo2.Thing.TryGetComp<CompRottable>() != null && globalTargetInfo2.Thing.IsHashIntervalTick(60))
				{
					FilthMaker.TryMakeFilth(globalTargetInfo2.Thing.Position, globalTargetInfo2.Thing.Map, ThingDefOf.Filth_CorpseBile);
					globalTargetInfo2.Thing.TryGetComp<CompRottable>().RotProgress += 60000f;
				}
			}
		});
	}

	public override void Cast(params GlobalTargetInfo[] targets)
	{
		((Ability)this).Cast(targets);
		if (!((Ability)this).pawn.health.hediffSet.HasHediff(VPE_DefOf.VPE_BodiesConsumed))
		{
			((Ability)this).pawn.health.AddHediff(VPE_DefOf.VPE_BodiesConsumed, null, null);
		}
		Hediff_BodiesConsumed hediff_BodiesConsumed = ((Ability)this).pawn.health.hediffSet.GetFirstHediffOfDef(VPE_DefOf.VPE_BodiesConsumed) as Hediff_BodiesConsumed;
		for (int i = 0; i < targets.Length; i++)
		{
			GlobalTargetInfo globalTargetInfo = targets[i];
			MoteBetween moteBetween = (MoteBetween)ThingMaker.MakeThing(VPE_DefOf.VPE_SoulOrbTransfer);
			moteBetween.Attach(globalTargetInfo.Thing, ((Ability)this).pawn);
			moteBetween.exactPosition = globalTargetInfo.Thing.DrawPos;
			GenSpawn.Spawn(moteBetween, globalTargetInfo.Thing.Position, ((Ability)this).pawn.Map);
			hediff_BodiesConsumed.consumedBodies++;
			globalTargetInfo.Thing.Destroy();
		}
	}
}
