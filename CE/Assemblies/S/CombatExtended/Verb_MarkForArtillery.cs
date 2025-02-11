using System.Linq;
using RimWorld;
using Verse;

namespace CombatExtended;

public class Verb_MarkForArtillery : Verb_LaunchProjectileCE
{
	private int _lastConditionsCheckedAt = int.MinValue;

	private bool _conditionsMet = false;

	public override bool Available()
	{
		return MarkingConditionsMet() && base.Available();
	}

	public override bool IsUsableOn(Thing target)
	{
		return MarkingConditionsMet() && base.IsUsableOn(target);
	}

	public void Dirty()
	{
		_lastConditionsCheckedAt = -1;
	}

	public override void WarmupComplete()
	{
		base.WarmupComplete();
		if (base.ShooterPawn != null && base.ShooterPawn.skills != null)
		{
			base.ShooterPawn.skills.Learn(SkillDefOf.Shooting, 200f);
		}
	}

	public override bool TryCastShot()
	{
		ArtilleryMarker artilleryMarker = ThingMaker.MakeThing(ThingDef.Named("ArtilleryMarker")) as ArtilleryMarker;
		ShiftVecReport shiftVecReport = ShiftVecReportFor(currentTarget);
		artilleryMarker.sightsEfficiency = shiftVecReport.sightsEfficiency;
		artilleryMarker.aimingAccuracy = shiftVecReport.aimingAccuracy;
		artilleryMarker.lightingShift = shiftVecReport.lightingShift;
		artilleryMarker.weatherShift = shiftVecReport.weatherShift;
		if (CasterIsPawn)
		{
			artilleryMarker.caster = base.ShooterPawn;
		}
		GenSpawn.Spawn(artilleryMarker, currentTarget.Cell, caster.Map);
		if (currentTarget.HasThing)
		{
			CompAttachBase compAttachBase = currentTarget.Thing.TryGetComp<CompAttachBase>();
			if (compAttachBase != null)
			{
				artilleryMarker.AttachTo(currentTarget.Thing);
			}
		}
		PlayerKnowledgeDatabase.KnowledgeDemonstrated(CE_ConceptDefOf.CE_Spotting, KnowledgeAmount.SmallInteraction);
		return true;
	}

	public bool MarkingConditionsMet()
	{
		if (_lastConditionsCheckedAt + 250 > GenTicks.TicksGame)
		{
			return _conditionsMet;
		}
		_lastConditionsCheckedAt = GenTicks.TicksGame;
		if (!CasterIsPawn)
		{
			return _conditionsMet = true;
		}
		Pawn_ApparelTracker apparel = base.ShooterPawn.apparel;
		if (apparel != null && apparel.WornApparel.Any((Apparel a) => a.def.IsRadioPack()))
		{
			return _conditionsMet = true;
		}
		TurretTracker component = caster.Map.GetComponent<TurretTracker>();
		if (component.Turrets.Any((Building_Turret t) => t is Building_TurretGunCE { IsMortar: not false } building_TurretGunCE && building_TurretGunCE.Faction == caster.Faction))
		{
			return _conditionsMet = true;
		}
		return _conditionsMet = false;
	}

	public override void VerbTickCE()
	{
		if (CasterPawn != null && CasterPawn.IsColonistPlayerControlled)
		{
			LessonAutoActivator.TeachOpportunity(CE_ConceptDefOf.CE_Spotting, OpportunityType.GoodToKnow);
		}
	}
}
