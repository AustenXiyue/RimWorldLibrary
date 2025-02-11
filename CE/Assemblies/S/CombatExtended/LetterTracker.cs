using RimWorld;
using Verse;

namespace CombatExtended;

public class LetterTracker : MapComponent
{
	private static bool _sentMechWarning;

	public LetterTracker(Map map)
		: base(map)
	{
	}

	public override void MapComponentTick()
	{
		base.MapComponentTick();
		if (Find.TickManager.TicksGame % 2000 == 0 && Faction.OfMechanoids != null && !_sentMechWarning && (float)GenDate.DaysPassed >= Faction.OfMechanoids.def.earliestRaidDays * 0.75f && Find.AnyPlayerHomeMap != null)
		{
			Pawn pawn = ((Find.AnyPlayerHomeMap.mapPawns.FreeColonistsSpawnedCount != 0) ? Find.AnyPlayerHomeMap.mapPawns.FreeColonistsSpawned.RandomElement() : PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists.RandomElement());
			TaggedString label = "CE_MechWarningLabel".Translate();
			TaggedString text = "CE_MechWarningText".Translate(pawn?.LabelShort ?? ((string)"CE_MechWarningText_UnnamedColonist".Translate()), CE_ThingDefOf.Mech_CentipedeBlaster.GetStatValueAbstract(StatDefOf.ArmorRating_Sharp));
			Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NegativeEvent);
			_sentMechWarning = true;
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref _sentMechWarning, "sentMechWarning", defaultValue: false);
	}
}
