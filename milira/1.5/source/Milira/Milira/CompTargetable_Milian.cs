using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Milira;

public class CompTargetable_Milian : CompTargetable
{
	protected override bool PlayerChoosesTarget => true;

	public CompProperties_TargetableMilian Props_Milian => (CompProperties_TargetableMilian)props;

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		base.PostSpawnSetup(respawningAfterLoad);
	}

	protected override TargetingParameters GetTargetingParameters()
	{
		return new TargetingParameters
		{
			canTargetPawns = true,
			canTargetBuildings = false,
			canTargetMechs = true,
			mapObjectTargetsMustBeAutoAttackable = false,
			validator = (TargetInfo target) => target.Thing is Pawn { Downed: false } pawn && pawn.Awake() && pawn.Faction == Faction.OfPlayer && (!Props_Milian.targetableMilianPawnkinds.NullOrEmpty() || !Props_Milian.availableIfWithHediff.NullOrEmpty()) && (Props_Milian.targetableMilianPawnkinds.Contains(pawn.kindDef) || pawn.health.hediffSet.hediffs.Any((Hediff hediff) => Props_Milian.availableIfWithHediff.Contains(hediff.def)))
		};
	}

	public override IEnumerable<Thing> GetTargets(Thing targetChosenByPlayer = null)
	{
		yield return targetChosenByPlayer;
	}
}
