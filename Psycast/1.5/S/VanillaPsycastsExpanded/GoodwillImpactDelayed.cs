using RimWorld;
using Verse;

namespace VanillaPsycastsExpanded;

public class GoodwillImpactDelayed : IExposable
{
	public int impactInTicks;

	public int goodwillImpact;

	public Faction factionToImpact;

	public HistoryEventDef historyEvent;

	public string letterLabel;

	public string letterDesc;

	public string relationInfoKey;

	public void DoImpact()
	{
		Faction.OfPlayer.TryAffectGoodwillWith(factionToImpact, goodwillImpact, canSendMessage: true, canSendHostilityLetter: true, historyEvent, null);
		if (!relationInfoKey.NullOrEmpty())
		{
			letterDesc += "\n\n" + relationInfoKey.Translate(factionToImpact.Named("FACTION"), Faction.OfPlayer.GoodwillWith(factionToImpact), goodwillImpact);
		}
		Find.LetterStack.ReceiveLetter(letterLabel, letterDesc, LetterDefOf.ThreatSmall, null, factionToImpact);
	}

	public void ExposeData()
	{
		Scribe_Values.Look(ref impactInTicks, "impactInTicks", 0);
		Scribe_Values.Look(ref goodwillImpact, "goodwillImpact", 0);
		Scribe_Values.Look(ref letterLabel, "letterLabel");
		Scribe_Values.Look(ref letterDesc, "letterDesc");
		Scribe_Values.Look(ref relationInfoKey, "relationInfoKey");
		Scribe_References.Look(ref factionToImpact, "factionToImpact");
		Scribe_Defs.Look(ref historyEvent, "historyEvent");
	}
}
