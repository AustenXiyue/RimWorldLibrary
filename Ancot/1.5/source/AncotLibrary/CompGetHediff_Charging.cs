using System.Linq;
using RimWorld;
using Verse;

namespace AncotLibrary;

public class CompGetHediff_Charging : ThingComp
{
	private CompProperties_GetHediff_Charging Props => (CompProperties_GetHediff_Charging)props;

	protected Pawn PawnOwner
	{
		get
		{
			if (!(parent is Apparel { Wearer: var wearer }))
			{
				if (parent is Pawn result)
				{
					return result;
				}
				return null;
			}
			return wearer;
		}
	}

	private bool staggering => PawnOwner.stances.stagger.Staggered;

	private CompMechAutoFight compMechAutoFight => PawnOwner.TryGetComp<CompMechAutoFight>();

	public bool autoFightForPlayer
	{
		get
		{
			if (compMechAutoFight != null && PawnOwner != null && PawnOwner.Faction.IsPlayer)
			{
				return compMechAutoFight.autoFight;
			}
			return false;
		}
	}

	private float movingSpeed => PawnOwner.GetStatValue(StatDefOf.MoveSpeed);

	public override void CompTick()
	{
		base.CompTick();
		if (PawnOwner == null || PawnOwner.Downed || !PawnOwner.Spawned)
		{
			return;
		}
		Hediff hediff = PawnOwner.health.hediffSet.GetFirstHediffOfDef(Props.hediffDef);
		BodyPartRecord part = PawnOwner.health.hediffSet.GetNotMissingParts().FirstOrDefault((BodyPartRecord x) => x.def == Props.bodyPartDef);
		if (PawnOwner.pather.MovingNow && (PawnOwner.Drafted || autoFightForPlayer || PawnOwner.InAggroMentalState || (PawnOwner.Faction != Faction.OfPlayer && !PawnOwner.IsPrisoner)) && movingSpeed >= Props.minSpeed)
		{
			if (hediff == null)
			{
				hediff = PawnOwner.health.AddHediff(Props.hediffDef, part, null);
				hediff.Severity = Props.initialSeverity;
			}
			else if (Props.speedSeverityFactor.HasValue)
			{
				hediff.Severity += movingSpeed * Props.speedSeverityFactor.Value * Props.severityPerTick_Job;
			}
			else
			{
				hediff.Severity += Props.severityPerTick_Job;
			}
		}
		else if (hediff != null)
		{
			hediff.Severity -= Props.severityPerTick_Stop;
		}
		if (hediff != null && (float)PawnOwner.Map.pathing.For(PawnOwner).pathGrid.PerceivedPathCostAt(PawnOwner.Position) > Props.pathCostThreshold)
		{
			hediff.Severity -= Props.blockedSeverityFactor * Props.severityPerTick_Stop;
		}
		if (hediff != null && staggering)
		{
			hediff.Severity -= Props.staggeredSeverityFactor * Props.severityPerTick_Stop;
		}
	}
}
