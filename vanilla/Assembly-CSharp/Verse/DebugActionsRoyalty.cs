using System.Collections.Generic;
using System.Linq;
using System.Text;
using LudeonTK;
using RimWorld;
using RimWorld.QuestGen;

namespace Verse;

public static class DebugActionsRoyalty
{
	private static IEnumerable<Faction> FactionsWithRoyalTitles => Find.FactionManager.AllFactions.Where((Faction f) => f.def.RoyalTitlesAwardableInSeniorityOrderForReading.Count > 0);

	private static bool CheckAnyFactionWithRoyalTitles()
	{
		if (!FactionsWithRoyalTitles.Any())
		{
			Messages.Message("No factions with royal titles found.", MessageTypeDefOf.RejectInput, historical: false);
			return false;
		}
		return true;
	}

	[DebugAction("General", "Award 4 honor", false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap, requiresRoyalty = true)]
	private static void Award4RoyalFavor()
	{
		if (!CheckAnyFactionWithRoyalTitles())
		{
			return;
		}
		List<DebugMenuOption> list = new List<DebugMenuOption>();
		foreach (Faction factionsWithRoyalTitle in FactionsWithRoyalTitles)
		{
			Faction localFaction = factionsWithRoyalTitle;
			list.Add(new DebugMenuOption(localFaction.Name, DebugMenuOptionMode.Tool, delegate
			{
				UI.MouseCell().GetFirstPawn(Find.CurrentMap)?.royalty.GainFavor(localFaction, 4);
			}));
		}
		Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
	}

	[DebugAction("General", "Award 10 honor", false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap, requiresRoyalty = true)]
	private static void Award10RoyalFavor()
	{
		if (!CheckAnyFactionWithRoyalTitles())
		{
			return;
		}
		List<DebugMenuOption> list = new List<DebugMenuOption>();
		foreach (Faction factionsWithRoyalTitle in FactionsWithRoyalTitles)
		{
			Faction localFaction = factionsWithRoyalTitle;
			list.Add(new DebugMenuOption(localFaction.Name, DebugMenuOptionMode.Tool, delegate
			{
				UI.MouseCell().GetFirstPawn(Find.CurrentMap)?.royalty.GainFavor(localFaction, 10);
			}));
		}
		Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
	}

	[DebugAction("General", "Remove 4 honor", false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap, requiresRoyalty = true)]
	private static void Remove4RoyalFavor()
	{
		if (!CheckAnyFactionWithRoyalTitles())
		{
			return;
		}
		List<DebugMenuOption> list = new List<DebugMenuOption>();
		foreach (Faction factionsWithRoyalTitle in FactionsWithRoyalTitles)
		{
			Faction localFaction = factionsWithRoyalTitle;
			list.Add(new DebugMenuOption(localFaction.Name, DebugMenuOptionMode.Tool, delegate
			{
				UI.MouseCell().GetFirstPawn(Find.CurrentMap)?.royalty.TryRemoveFavor(localFaction, 4);
			}));
		}
		Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
	}

	[DebugAction("General", "Reduce royal title", false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap, requiresRoyalty = true)]
	private static void ReduceRoyalTitle()
	{
		if (!CheckAnyFactionWithRoyalTitles())
		{
			return;
		}
		List<DebugMenuOption> list = new List<DebugMenuOption>();
		foreach (Faction factionsWithRoyalTitle in FactionsWithRoyalTitles)
		{
			Faction localFaction = factionsWithRoyalTitle;
			list.Add(new DebugMenuOption(localFaction.Name, DebugMenuOptionMode.Tool, delegate
			{
				UI.MouseCell().GetFirstPawn(Find.CurrentMap)?.royalty.ReduceTitle(localFaction);
			}));
		}
		Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
	}

	[DebugAction("General", "Set royal title", false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap, requiresRoyalty = true)]
	private static void SetTitleForced()
	{
		if (!CheckAnyFactionWithRoyalTitles())
		{
			return;
		}
		List<DebugMenuOption> list = new List<DebugMenuOption>();
		foreach (Faction factionsWithRoyalTitle in FactionsWithRoyalTitles)
		{
			Faction localFaction = factionsWithRoyalTitle;
			list.Add(new DebugMenuOption(localFaction.Name, DebugMenuOptionMode.Action, delegate
			{
				List<DebugMenuOption> list2 = new List<DebugMenuOption>
				{
					new DebugMenuOption("(none)", DebugMenuOptionMode.Tool, delegate
					{
						Pawn firstPawn = UI.MouseCell().GetFirstPawn(Find.CurrentMap);
						if (firstPawn != null && firstPawn.royalty != null && firstPawn.royalty.HasAnyTitleIn(localFaction))
						{
							firstPawn.royalty.SetTitle(localFaction, null, grantRewards: true);
							DebugActionsUtility.DustPuffFrom(firstPawn);
						}
					})
				};
				foreach (RoyalTitleDef item in DefDatabase<RoyalTitleDef>.AllDefsListForReading)
				{
					RoyalTitleDef localTitleDef = item;
					list2.Add(new DebugMenuOption(localTitleDef.defName, DebugMenuOptionMode.Tool, delegate
					{
						UI.MouseCell().GetFirstPawn(Find.CurrentMap)?.royalty.SetTitle(localFaction, localTitleDef, grantRewards: true);
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list2));
			}));
		}
		Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
	}

	[DebugOutput]
	private static void RoyalTitles()
	{
		DebugTables.MakeTablesDialog(DefDatabase<RoyalTitleDef>.AllDefsListForReading, new TableDataGetter<RoyalTitleDef>("defName", (RoyalTitleDef title) => title.defName), new TableDataGetter<RoyalTitleDef>("seniority", (RoyalTitleDef title) => title.seniority), new TableDataGetter<RoyalTitleDef>("favorCost", (RoyalTitleDef title) => title.favorCost), new TableDataGetter<RoyalTitleDef>("Awardable", (RoyalTitleDef title) => title.Awardable), new TableDataGetter<RoyalTitleDef>("minimumExpectationLock", (RoyalTitleDef title) => (title.minExpectation != null) ? title.minExpectation.defName : "NULL"), new TableDataGetter<RoyalTitleDef>("requiredMinimumApparelQuality", (RoyalTitleDef title) => (title.requiredMinimumApparelQuality != 0) ? title.requiredMinimumApparelQuality.ToString() : "None"), new TableDataGetter<RoyalTitleDef>("requireApparel", (RoyalTitleDef title) => (title.requiredApparel != null) ? string.Join(",\r\n", title.requiredApparel.Select((ApparelRequirement a) => a.ToString()).ToArray()) : "NULL"), new TableDataGetter<RoyalTitleDef>("awardThought", (RoyalTitleDef title) => (title.awardThought != null) ? title.awardThought.defName : "NULL"), new TableDataGetter<RoyalTitleDef>("lostThought", (RoyalTitleDef title) => (title.lostThought != null) ? title.lostThought.defName : "NULL"), new TableDataGetter<RoyalTitleDef>("factions", (RoyalTitleDef title) => string.Join(",", (from f in DefDatabase<FactionDef>.AllDefsListForReading
			where f.RoyalTitlesAwardableInSeniorityOrderForReading.Contains(title)
			select f.defName).ToArray())));
	}

	[DebugOutput(name = "Honor Availability (slow)")]
	private static void RoyalFavorAvailability()
	{
		StorytellerCompProperties_OnOffCycle storytellerCompProperties_OnOffCycle2 = (StorytellerCompProperties_OnOffCycle)StorytellerDefOf.Cassandra.comps.Find(delegate(StorytellerCompProperties x)
		{
			if (!(x is StorytellerCompProperties_OnOffCycle storytellerCompProperties_OnOffCycle))
			{
				return false;
			}
			if (storytellerCompProperties_OnOffCycle.IncidentCategory != IncidentCategoryDefOf.GiveQuest)
			{
				return false;
			}
			return (storytellerCompProperties_OnOffCycle.enableIfAnyModActive != null && storytellerCompProperties_OnOffCycle.enableIfAnyModActive.Any((string m) => m.ToLower() == "ludeon.rimworld.royalty")) ? true : false;
		});
		float onDays = storytellerCompProperties_OnOffCycle2.onDays;
		float average = storytellerCompProperties_OnOffCycle2.numIncidentsRange.Average;
		float num = average / onDays;
		SimpleCurve simpleCurve = new SimpleCurve
		{
			new CurvePoint(0f, 35f),
			new CurvePoint(15f, 150f),
			new CurvePoint(150f, 5000f)
		};
		int num2 = 0;
		List<RoyalTitleDef> royalTitlesAwardableInSeniorityOrderForReading = FactionDefOf.Empire.RoyalTitlesAwardableInSeniorityOrderForReading;
		for (int i = 0; i < royalTitlesAwardableInSeniorityOrderForReading.Count; i++)
		{
			num2 += royalTitlesAwardableInSeniorityOrderForReading[i].favorCost;
			if (royalTitlesAwardableInSeniorityOrderForReading[i] == RoyalTitleDefOf.Count)
			{
				break;
			}
		}
		float num3 = 0f;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		int num8 = -1;
		int num9 = -1;
		int num10 = -1;
		int ticksGame = Find.TickManager.TicksGame;
		StoryState storyState = new StoryState(Find.World);
		for (int j = 0; j < 200; j++)
		{
			Find.TickManager.DebugSetTicksGame(j * 60000);
			num3 += num * storytellerCompProperties_OnOffCycle2.acceptFractionByDaysPassedCurve.Evaluate(j);
			while (num3 >= 1f)
			{
				num3 -= 1f;
				num4++;
				float points = simpleCurve.Evaluate(j);
				Slate slate = new Slate();
				slate.Set("points", points);
				QuestScriptDef questScriptDef = DefDatabase<QuestScriptDef>.AllDefsListForReading.Where((QuestScriptDef x) => x.IsRootRandomSelected && x.CanRun(slate)).RandomElementByWeight((QuestScriptDef x) => NaturalRandomQuestChooser.GetNaturalRandomSelectionWeight(x, points, storyState));
				Quest quest = QuestGen.Generate(questScriptDef, slate);
				if (quest.InvolvedFactions.Contains(Faction.OfEmpire))
				{
					num7++;
				}
				QuestPart_GiveRoyalFavor questPart_GiveRoyalFavor = (QuestPart_GiveRoyalFavor)quest.PartsListForReading.Find((QuestPart x) => x is QuestPart_GiveRoyalFavor);
				if (questPart_GiveRoyalFavor != null)
				{
					num5 += questPart_GiveRoyalFavor.amount;
					num6++;
					if (num5 >= num2 && num8 < 0)
					{
						num8 = j;
					}
					if (num9 < 0 || questPart_GiveRoyalFavor.amount < num9)
					{
						num9 = questPart_GiveRoyalFavor.amount;
					}
					if (num10 < 0 || questPart_GiveRoyalFavor.amount > num10)
					{
						num10 = questPart_GiveRoyalFavor.amount;
					}
				}
				storyState.RecordRandomQuestFired(questScriptDef);
				quest.CleanupQuestParts();
			}
		}
		Find.TickManager.DebugSetTicksGame(ticksGame);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Results for: Days=" + 200 + ", intervalDays=" + onDays + ", questsPerInterval=" + average + ":");
		stringBuilder.AppendLine("Quests: " + num4);
		stringBuilder.AppendLine("Quests with honor: " + num6);
		stringBuilder.AppendLine("Quests from Empire: " + num7);
		stringBuilder.AppendLine("Min honor reward: " + num9);
		stringBuilder.AppendLine("Max honor reward: " + num10);
		stringBuilder.AppendLine("Total honor: " + num5);
		stringBuilder.AppendLine("Honor required for Count: " + num2);
		stringBuilder.AppendLine("Count title possible on day: " + num8);
		Log.Message(stringBuilder.ToString());
	}
}
