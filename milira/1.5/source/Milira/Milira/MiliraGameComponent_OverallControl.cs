using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Milira;

public class MiliraGameComponent_OverallControl : GameComponent
{
	public bool turnToFriend_Pre = false;

	public bool turnToFriend = false;

	public bool goodWillCorrected = false;

	public bool canSendChurchFirstTime = false;

	public bool canSendFallenMilira = false;

	public bool canSendChurchInfo = false;

	public Pawn pawn;

	public Pawn pawnInColony;

	public int miliraThreatPoint = 0;

	private static readonly SimpleCurve MilianClusterChance = new SimpleCurve
	{
		new CurvePoint(0f, 0f),
		new CurvePoint(20f, 0.01f),
		new CurvePoint(100f, 0.8f),
		new CurvePoint(1000f, 1f)
	};

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref turnToFriend_Pre, "turnToFriend_Pre", defaultValue: false);
		Scribe_Values.Look(ref turnToFriend, "turnToFriend", defaultValue: false);
		Scribe_Values.Look(ref goodWillCorrected, "goodWillCorrected", defaultValue: false);
		Scribe_Values.Look(ref canSendChurchFirstTime, "canSendChurchFirstTime", defaultValue: false);
		Scribe_Values.Look(ref canSendFallenMilira, "canSendFallenMilira", defaultValue: false);
		Scribe_Values.Look(ref miliraThreatPoint, "threatPoint", 0);
		Scribe_References.Look(ref pawn, "pawn", saveDestroyedThings: true);
	}

	public MiliraGameComponent_OverallControl(Game game)
	{
	}

	public override void GameComponentTick()
	{
		Map currentMap = Find.CurrentMap;
		if (miliraThreatPoint > 20 && Find.TickManager.TicksGame % 60000 == 0)
		{
			miliraThreatPoint--;
		}
		if (Find.TickManager.TicksGame % 27000 == 0 && Rand.Chance(MilianClusterChance.Evaluate(miliraThreatPoint)) && !turnToFriend)
		{
			if (Rand.Chance(0.3f))
			{
				SendMilianClusterRaid();
			}
			else
			{
				SendMiliraRaid();
			}
		}
		if (miliraThreatPoint < 20 && canSendChurchFirstTime && Find.TickManager.TicksGame % 20000 == 0 && Rand.Chance(0.15f) && pawn != null && pawn.HostFaction != null && pawn.HostFaction.IsPlayer)
		{
			SendChurchFirstInteract();
			canSendChurchFirstTime = false;
		}
		if (Find.TickManager.TicksGame % 12000 == 0 && Rand.Chance(0.4f) && currentMap.gameConditionManager.ConditionIsActive(MiliraDefOf.SolarFlare))
		{
			SolarCrystalMining();
		}
	}

	private void CheckMiliraPermanentEnemyStatus()
	{
		Faction faction = Find.FactionManager.FirstFactionOfDef(MiliraDefOf.Milira_Faction);
		if (turnToFriend_Pre && pawn != null && pawn.HostFaction == null && Faction.OfPlayer.def.defName != "Milira_PlayerFaction" && Faction.OfPlayer.def.defName != "Kiiro_PlayerFaction")
		{
			turnToFriend = true;
			List<FactionDef> permanentEnemyToEveryoneExcept = MiliraDefOf.Milira_Faction.permanentEnemyToEveryoneExcept;
			if (!permanentEnemyToEveryoneExcept.Contains(Faction.OfPlayer.def))
			{
				MiliraDefOf.Milira_Faction.permanentEnemyToEveryoneExcept.Add(Faction.OfPlayer.def);
			}
		}
		if (!turnToFriend && Faction.OfPlayer.def.defName != "Milira_PlayerFaction" && Faction.OfPlayer.def.defName != "Kiiro_PlayerFaction")
		{
			List<FactionDef> permanentEnemyToEveryoneExcept2 = MiliraDefOf.Milira_Faction.permanentEnemyToEveryoneExcept;
			if (permanentEnemyToEveryoneExcept2.Contains(Faction.OfPlayer.def))
			{
				MiliraDefOf.Milira_Faction.permanentEnemyToEveryoneExcept.Remove(Faction.OfPlayer.def);
			}
		}
		if (turnToFriend && !goodWillCorrected && Faction.OfPlayer.def.defName != "Milira_PlayerFaction" && Faction.OfPlayer.def.defName != "Kiiro_PlayerFaction")
		{
			goodWillCorrected = true;
			Find.LetterStack.ReceiveLetter("LetterLabelTest".Translate(), "LetterTest".Translate().Resolve().CapitalizeFirst(), LetterDefOf.NeutralEvent, GlobalTargetInfo.Invalid);
			faction.TryAffectGoodwillWith(Faction.OfPlayer, 100, canSendMessage: true, canSendHostilityLetter: true, null, null);
			IncidentParms parms = new IncidentParms
			{
				target = Find.CurrentMap
			};
			Find.Storyteller.incidentQueue.Add(MiliraDefOf.Milira_FallenAngel_Return, Find.TickManager.TicksGame, parms);
		}
	}

	public override void StartedNewGame()
	{
		canSendFallenMilira = true;
		canSendChurchFirstTime = false;
		turnToFriend_Pre = false;
		turnToFriend = false;
		foreach (Pawn item in MechanitorUtility.MechsInPlayerFaction())
		{
			if (MilianUtility.IsMilian(item))
			{
				item?.Drawer?.renderer?.renderTree?.SetDirty();
			}
		}
		CheckMiliraPermanentEnemyStatus();
		CheckPermanentEnemyChurchIfPlayerIsMilira();
		CheckIfMiliraAndChurchFactionExistWithHostileRelation();
	}

	public override void LoadedGame()
	{
		base.LoadedGame();
		CheckMiliraPermanentEnemyStatus();
		CheckPermanentEnemyChurchIfPlayerIsMilira();
	}

	public void CheckPermanentEnemyChurchIfPlayerIsMilira()
	{
		Faction faction = Find.FactionManager.FirstFactionOfDef(MiliraDefOf.Milira_AngelismChurch);
		if (Faction.OfPlayer.def.defName == "Milira_PlayerFaction" && faction != null)
		{
			faction.def.permanentEnemy = true;
		}
	}

	public void CheckBeFriendInBeginning()
	{
		if (Faction.OfPlayer.def.defName == "Milira_PlayerFaction" || Faction.OfPlayer.def.defName == "Kiiro_PlayerFaction")
		{
			turnToFriend = true;
		}
	}

	public void CheckIfMiliraAndChurchFactionExistWithHostileRelation()
	{
		Faction faction = Find.FactionManager.FirstFactionOfDef(MiliraDefOf.Milira_Faction);
		Faction faction2 = Find.FactionManager.FirstFactionOfDef(MiliraDefOf.Milira_AngelismChurch);
		if (faction == null || faction2 == null || !faction.HostileTo(faction2))
		{
			Find.LetterStack.ReceiveLetter("Milira.StoryDisabled_ConditionFailed".Translate(), "Milira.StoryDisabled_ConditionFailed_Desc".Translate().Resolve().CapitalizeFirst(), LetterDefOf.NeutralEvent, GlobalTargetInfo.Invalid);
		}
	}

	public void SendChurchFirstInteract()
	{
		Map currentMap = Find.CurrentMap;
		IncidentParms parms = new IncidentParms
		{
			target = currentMap
		};
		Find.Storyteller.incidentQueue.Add(MiliraDefOf.Milira_FallenAngel_ToChurch, Find.TickManager.TicksGame, parms);
	}

	public void SendMilianClusterRaid()
	{
		Map currentMap = Find.CurrentMap;
		IncidentParms parms = new IncidentParms
		{
			target = currentMap,
			points = StorytellerUtility.DefaultThreatPointsNow(currentMap)
		};
		Find.Storyteller.incidentQueue.Add(MiliraDefOf.Milira_MilianCluster, Find.TickManager.TicksGame, parms);
	}

	public void SendMiliraRaid()
	{
		Map currentMap = Find.CurrentMap;
		Faction faction = Find.FactionManager.FirstFactionOfDef(MiliraDefOf.Milira_Faction);
		IncidentParms incidentParms = new IncidentParms();
		incidentParms.target = currentMap;
		incidentParms.faction = faction;
		incidentParms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeDrop;
		incidentParms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
		incidentParms.points = Mathf.Max(StorytellerUtility.DefaultThreatPointsNow(currentMap), faction.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Combat));
		Find.Storyteller.incidentQueue.Add(MiliraDefOf.Milira_Raid, Find.TickManager.TicksGame, incidentParms);
	}

	public void SolarCrystalMining()
	{
		Map currentMap = Find.CurrentMap;
		IncidentParms parms = new IncidentParms
		{
			target = currentMap
		};
		Find.Storyteller.incidentQueue.Add(MiliraDefOf.Milira_SolarCrystalMining, Find.TickManager.TicksGame, parms);
	}
}
