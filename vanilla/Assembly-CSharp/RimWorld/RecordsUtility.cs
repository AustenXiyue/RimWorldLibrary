using System.Collections.Generic;
using Verse;

namespace RimWorld;

public static class RecordsUtility
{
	public static void Notify_PawnKilled(Pawn killed, Pawn killer)
	{
		killer.records.Increment(RecordDefOf.Kills);
		RaceProperties raceProps = killed.RaceProps;
		if (ModsConfig.AnomalyActive && (killed.IsMutant || raceProps.IsAnomalyEntity))
		{
			killer.records.Increment(RecordDefOf.KillsEntities);
		}
		else if (raceProps.Humanlike)
		{
			killer.records.Increment(RecordDefOf.KillsHumanlikes);
		}
		if (raceProps.Animal)
		{
			killer.records.Increment(RecordDefOf.KillsAnimals);
		}
		if (raceProps.IsMechanoid)
		{
			killer.records.Increment(RecordDefOf.KillsMechanoids);
		}
	}

	public static void Notify_PawnDowned(Pawn downed, Pawn instigator)
	{
		instigator.records.Increment(RecordDefOf.PawnsDowned);
		RaceProperties raceProps = downed.RaceProps;
		if (ModsConfig.AnomalyActive && (downed.IsMutant || raceProps.IsAnomalyEntity))
		{
			instigator.records.Increment(RecordDefOf.PawnsDownedEntities);
		}
		else if (raceProps.Humanlike)
		{
			instigator.records.Increment(RecordDefOf.PawnsDownedHumanlikes);
		}
		if (raceProps.Animal)
		{
			instigator.records.Increment(RecordDefOf.PawnsDownedAnimals);
		}
		if (raceProps.IsMechanoid)
		{
			instigator.records.Increment(RecordDefOf.PawnsDownedMechanoids);
		}
	}

	public static void Notify_BillDone(Pawn billDoer, List<Thing> products)
	{
		for (int i = 0; i < products.Count; i++)
		{
			for (int j = 0; j < products[i].stackCount; j++)
			{
				if (products[i].def.IsNutritionGivingIngestible && (int)products[i].def.ingestible.preferability >= 7)
				{
					billDoer.records.Increment(RecordDefOf.MealsCooked);
				}
				else if (ShouldIncrementThingsCrafted(products[i]))
				{
					billDoer.records.Increment(RecordDefOf.ThingsCrafted);
				}
			}
		}
	}

	private static bool ShouldIncrementThingsCrafted(Thing crafted)
	{
		if (!crafted.def.IsApparel && !crafted.def.IsWeapon && !crafted.def.HasComp(typeof(CompArt)))
		{
			return crafted.def.HasComp(typeof(CompQuality));
		}
		return true;
	}
}
