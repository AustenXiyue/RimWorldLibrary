using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;

namespace RimWorld.Planet;

[StaticConstructorOnStartup]
public static class CaravanFormingUtility
{
	private static readonly Texture2D RemoveFromCaravanCommand = ContentFinder<Texture2D>.Get("UI/Commands/RemoveFromCaravan");

	private static readonly Texture2D AddToCaravanCommand = ContentFinder<Texture2D>.Get("UI/Commands/AddToCaravan");

	private static List<Pawn> tmpPawnsInCancelledCaravan = new List<Pawn>();

	private static List<Pawn> tmpRopers = new List<Pawn>();

	private static List<Pawn> tmpNeedRoping = new List<Pawn>();

	private static List<ThingCount> tmpCaravanPawns = new List<ThingCount>();

	public static void FormAndCreateCaravan(IEnumerable<Pawn> pawns, Faction faction, int exitFromTile, int directionTile, int destinationTile)
	{
		CaravanExitMapUtility.ExitMapAndCreateCaravan(pawns, faction, exitFromTile, directionTile, destinationTile);
	}

	public static void StartFormingCaravan(List<Pawn> pawns, List<Pawn> downedPawns, Faction faction, List<TransferableOneWay> transferables, IntVec3 meetingPoint, IntVec3 exitSpot, int startingTile, int destinationTile)
	{
		if (startingTile < 0)
		{
			Log.Error("Can't start forming caravan because startingTile is invalid.");
			return;
		}
		if (!pawns.Any())
		{
			Log.Error("Can't start forming caravan with 0 pawns.");
			return;
		}
		if (pawns.Any((Pawn x) => x.Downed))
		{
			Log.Warning("Forming a caravan with a downed pawn. This shouldn't happen because we have to create a Lord.");
		}
		List<TransferableOneWay> list = transferables.ToList();
		list.RemoveAll((TransferableOneWay x) => x.CountToTransfer <= 0 || !x.HasAnyThing || x.AnyThing is Pawn);
		for (int i = 0; i < pawns.Count; i++)
		{
			pawns[i].GetLord()?.Notify_PawnLost(pawns[i], PawnLostCondition.ForcedToJoinOtherLord, null);
		}
		LordJob_FormAndSendCaravan lordJob = new LordJob_FormAndSendCaravan(list, downedPawns, meetingPoint, exitSpot, startingTile, destinationTile);
		LordMaker.MakeNewLord(Faction.OfPlayer, lordJob, pawns[0].MapHeld, pawns);
		for (int j = 0; j < pawns.Count; j++)
		{
			Pawn pawn = pawns[j];
			if (pawn.Spawned)
			{
				pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
			}
		}
	}

	public static void StopFormingCaravan(Lord lord)
	{
		tmpPawnsInCancelledCaravan.Clear();
		tmpPawnsInCancelledCaravan.AddRange(lord.ownedPawns);
		bool isPlayerHome = lord.Map.IsPlayerHome;
		SetToUnloadEverything(lord);
		lord.lordManager.RemoveLord(lord);
		if (isPlayerHome)
		{
			LeadAnimalsToPen(tmpPawnsInCancelledCaravan);
		}
		tmpPawnsInCancelledCaravan.Clear();
	}

	public static void LeadAnimalsToPen(List<Pawn> pawns)
	{
		tmpRopers.Clear();
		tmpNeedRoping.Clear();
		foreach (Pawn pawn in pawns)
		{
			if (pawn.Spawned && !pawn.Downed && !pawn.Dead && !pawn.Drafted)
			{
				if (AnimalPenUtility.NeedsToBeManagedByRope(pawn))
				{
					tmpNeedRoping.Add(pawn);
				}
				else if (pawn.IsColonist)
				{
					tmpRopers.Add(pawn);
				}
			}
		}
		if (tmpNeedRoping.Any() && tmpRopers.Any())
		{
			foreach (Pawn ropee in tmpNeedRoping)
			{
				if (!ropee.roping.IsRoped)
				{
					tmpRopers.MinBy((Pawn p) => p.Position.DistanceToSquared(ropee.Position)).roping.RopePawn(ropee);
				}
			}
			StartReturnedLord(tmpRopers.Where((Pawn p) => p.roping.IsRopingOthers).Concat(tmpNeedRoping).ToList());
		}
		tmpRopers.Clear();
		tmpNeedRoping.Clear();
	}

	private static void StartReturnedLord(List<Pawn> pawns)
	{
		foreach (Pawn pawn in pawns)
		{
			pawn.GetLord()?.Notify_PawnLost(pawn, PawnLostCondition.ForcedToJoinOtherLord, null);
		}
		LordJob_ReturnedCaravan lordJob = new LordJob_ReturnedCaravan(pawns[0].Position);
		LordMaker.MakeNewLord(Faction.OfPlayer, lordJob, pawns[0].MapHeld, pawns);
		foreach (Pawn pawn2 in pawns)
		{
			if (pawn2.Spawned)
			{
				pawn2.jobs.EndCurrentJob(JobCondition.InterruptForced);
			}
		}
	}

	public static void RemovePawnFromCaravan(Pawn pawn, Lord lord, bool removeFromDowned = true)
	{
		bool flag = false;
		pawn.inventory.DropAllPackingCaravanThings();
		for (int i = 0; i < lord.ownedPawns.Count; i++)
		{
			Pawn pawn2 = lord.ownedPawns[i];
			if (pawn2 != pawn && CaravanUtility.IsOwner(pawn2, Faction.OfPlayer))
			{
				flag = true;
				break;
			}
		}
		bool flag2 = true;
		TaggedString taggedString = "MessagePawnLostWhileFormingCaravan".Translate(pawn).CapitalizeFirst();
		if (!flag)
		{
			StopFormingCaravan(lord);
			taggedString += " " + "MessagePawnLostWhileFormingCaravan_AllLost".Translate();
		}
		else
		{
			pawn.inventory.UnloadEverything = true;
			if (lord.ownedPawns.Contains(pawn))
			{
				lord.Notify_PawnLost(pawn, PawnLostCondition.ForcedByPlayerAction, null);
				flag2 = false;
			}
			if (lord.LordJob is LordJob_FormAndSendCaravan lordJob_FormAndSendCaravan && lordJob_FormAndSendCaravan.downedPawns.Contains(pawn))
			{
				if (!removeFromDowned)
				{
					flag2 = false;
				}
				else
				{
					lordJob_FormAndSendCaravan.downedPawns.Remove(pawn);
				}
			}
			if (pawn.jobs != null)
			{
				pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
			}
		}
		if (flag2)
		{
			Messages.Message(taggedString, pawn, MessageTypeDefOf.NegativeEvent);
		}
	}

	private static void SetToUnloadEverything(Lord lord)
	{
		for (int i = 0; i < lord.ownedPawns.Count; i++)
		{
			lord.ownedPawns[i].inventory.DropAllPackingCaravanThings();
			lord.ownedPawns[i].inventory.UnloadEverything = true;
		}
	}

	public static void TryAddItemBackToTransferables(Thing item, List<TransferableOneWay> transferables, int count)
	{
		TransferableOneWay transferableOneWay = TransferableUtility.TransferableMatchingDesperate(item, transferables, TransferAsOneMode.PodsOrCaravanPacking);
		if (transferableOneWay != null && !transferableOneWay.things.Contains(item))
		{
			transferableOneWay.things.Add(item);
		}
		transferableOneWay?.AdjustTo(transferableOneWay.ClampAmount(transferableOneWay.CountToTransfer + count));
	}

	public static List<Thing> AllReachableColonyItems(Map map, bool allowEvenIfOutsideHomeArea = false, bool allowEvenIfReserved = false, bool canMinify = false)
	{
		List<Thing> list = new List<Thing>();
		map.listerThings.GetAllThings(in list, IsValidCaravanItem, lookInHaulSources: true);
		return list;
		bool IsValidCaravanItem(Thing t)
		{
			bool flag = canMinify && t.def.Minifiable && t.def.plant == null;
			if (!t.Spawned && !HaulAIUtility.IsInHaulableInventory(t))
			{
				return false;
			}
			if ((flag || t.def.category == ThingCategory.Item) && (allowEvenIfOutsideHomeArea || map.areaManager.Home[t.PositionHeld] || t.IsInAnyStorage()) && !t.PositionHeld.Fogged(t.MapHeld) && (allowEvenIfReserved || !map.reservationManager.IsReservedByAnyoneOf(t, Faction.OfPlayer)) && (flag || t.def.EverHaulable))
			{
				return t.GetInnerIfMinified().def.canLoadIntoCaravan;
			}
			return false;
		}
	}

	public static List<Pawn> AllSendablePawns(Map map, bool allowEvenIfDowned = false, bool allowEvenIfInMentalState = false, bool allowEvenIfPrisonerNotSecure = false, bool allowCapturableDownedPawns = false, bool allowLodgers = false, int allowLoadAndEnterTransportersLordForGroupID = -1)
	{
		List<Pawn> list = new List<Pawn>();
		IReadOnlyList<Pawn> allPawnsSpawned = map.mapPawns.AllPawnsSpawned;
		for (int i = 0; i < allPawnsSpawned.Count; i++)
		{
			Pawn pawn = allPawnsSpawned[i];
			if ((allowEvenIfDowned || !pawn.Downed) && (allowEvenIfInMentalState || !pawn.InMentalState) && (pawn.Faction == Faction.OfPlayer || pawn.IsPrisonerOfColony || (allowCapturableDownedPawns && CanListAsAutoCapturable(pawn))) && pawn.RaceProps.allowedOnCaravan && !pawn.IsQuestHelper() && (!pawn.IsQuestLodger() || allowLodgers) && (allowEvenIfPrisonerNotSecure || !pawn.IsPrisoner || pawn.guest.PrisonerIsSecure) && (pawn.GetLord() == null || pawn.GetLord().LordJob is LordJob_VoluntarilyJoinable || pawn.GetLord().LordJob.IsCaravanSendable || (allowLoadAndEnterTransportersLordForGroupID >= 0 && pawn.GetLord().LordJob is LordJob_LoadAndEnterTransporters && ((LordJob_LoadAndEnterTransporters)pawn.GetLord().LordJob).transportersGroup == allowLoadAndEnterTransportersLordForGroupID)))
			{
				list.Add(pawn);
			}
		}
		return list;
	}

	private static bool CanListAsAutoCapturable(Pawn p)
	{
		if (p.Downed && !p.mindState.WillJoinColonyIfRescued)
		{
			return CaravanUtility.ShouldAutoCapture(p, Faction.OfPlayer);
		}
		return false;
	}

	public static IEnumerable<Gizmo> GetGizmos(Pawn pawn)
	{
		if (IsFormingCaravanOrDownedPawnToBeTakenByCaravan(pawn))
		{
			Lord lord = GetFormAndSendCaravanLord(pawn);
			Command_Action command_Action = new Command_Action();
			command_Action.defaultLabel = "CommandCancelFormingCaravan".Translate();
			command_Action.defaultDesc = "CommandCancelFormingCaravanDesc".Translate();
			command_Action.icon = TexCommand.ClearPrioritizedWork;
			command_Action.activateSound = SoundDefOf.Tick_Low;
			command_Action.action = delegate
			{
				StopFormingCaravan(lord);
			};
			command_Action.hotKey = KeyBindingDefOf.Designator_Cancel;
			yield return command_Action;
			Command_Action command_Action2 = new Command_Action();
			command_Action2.defaultLabel = "CommandRemoveFromCaravan".Translate();
			command_Action2.defaultDesc = "CommandRemoveFromCaravanDesc".Translate();
			command_Action2.icon = RemoveFromCaravanCommand;
			command_Action2.action = delegate
			{
				RemovePawnFromCaravan(pawn, lord);
			};
			command_Action2.hotKey = KeyBindingDefOf.Misc6;
			yield return command_Action2;
		}
		else
		{
			if (!pawn.Spawned)
			{
				yield break;
			}
			bool flag = false;
			for (int i = 0; i < pawn.Map.lordManager.lords.Count; i++)
			{
				Lord lord2 = pawn.Map.lordManager.lords[i];
				if (lord2.faction == Faction.OfPlayer && lord2.LordJob is LordJob_FormAndSendCaravan)
				{
					flag = true;
					break;
				}
			}
			if (!flag || !Dialog_FormCaravan.AllSendablePawns(pawn.Map, reform: false).Contains(pawn))
			{
				yield break;
			}
			Command_Action command_Action3 = new Command_Action();
			command_Action3.defaultLabel = "CommandAddToCaravan".Translate();
			command_Action3.defaultDesc = "CommandAddToCaravanDesc".Translate();
			command_Action3.icon = AddToCaravanCommand;
			command_Action3.action = delegate
			{
				List<Lord> list = new List<Lord>();
				for (int j = 0; j < pawn.Map.lordManager.lords.Count; j++)
				{
					Lord lord3 = pawn.Map.lordManager.lords[j];
					if (lord3.faction == Faction.OfPlayer && lord3.LordJob is LordJob_FormAndSendCaravan)
					{
						list.Add(lord3);
					}
				}
				if (list.Count != 0)
				{
					if (list.Count == 1)
					{
						LateJoinFormingCaravan(pawn, list[0]);
						SoundDefOf.Click.PlayOneShotOnCamera();
					}
					else
					{
						List<FloatMenuOption> list2 = new List<FloatMenuOption>();
						for (int k = 0; k < list.Count; k++)
						{
							Lord caravanLocal = list[k];
							string label = (string)("Caravan".Translate() + " ") + (k + 1);
							list2.Add(new FloatMenuOption(label, delegate
							{
								if (pawn.Spawned && pawn.Map.lordManager.lords.Contains(caravanLocal) && Dialog_FormCaravan.AllSendablePawns(pawn.Map, reform: false).Contains(pawn))
								{
									LateJoinFormingCaravan(pawn, caravanLocal);
								}
							}));
						}
						Find.WindowStack.Add(new FloatMenu(list2));
					}
				}
			};
			command_Action3.hotKey = KeyBindingDefOf.Misc7;
			yield return command_Action3;
		}
	}

	public static void LateJoinFormingCaravan(Pawn pawn, Lord lord)
	{
		pawn.GetLord()?.Notify_PawnLost(pawn, PawnLostCondition.ForcedToJoinOtherLord, null);
		if (pawn.Downed)
		{
			((LordJob_FormAndSendCaravan)lord.LordJob).downedPawns.Add(pawn);
		}
		else
		{
			lord.AddPawn(pawn);
		}
		if (pawn.Spawned)
		{
			pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
		}
	}

	public static bool IsFormingCaravan(this Pawn p)
	{
		Lord lord;
		return p.TryGetFormingCaravanLord(out lord);
	}

	public static bool TryGetFormingCaravanLord(this Pawn p, out Lord lord)
	{
		lord = p.GetLord();
		if (lord != null)
		{
			return lord.LordJob is LordJob_FormAndSendCaravan;
		}
		return false;
	}

	public static bool IsFormingCaravanOrDownedPawnToBeTakenByCaravan(Pawn p)
	{
		return GetFormAndSendCaravanLord(p) != null;
	}

	public static Lord GetFormAndSendCaravanLord(Pawn p)
	{
		if (p.IsFormingCaravan())
		{
			return p.GetLord();
		}
		if (p.SpawnedOrAnyParentSpawned)
		{
			List<Lord> lords = p.MapHeld.lordManager.lords;
			for (int i = 0; i < lords.Count; i++)
			{
				if (lords[i].LordJob is LordJob_FormAndSendCaravan lordJob_FormAndSendCaravan && lordJob_FormAndSendCaravan.downedPawns.Contains(p))
				{
					return lords[i];
				}
			}
		}
		return null;
	}

	public static float CapacityLeft(LordJob_FormAndSendCaravan lordJob)
	{
		float num = CollectionsMassCalculator.MassUsageTransferables(lordJob.transferables, IgnorePawnsInventoryMode.IgnoreIfAssignedToUnload);
		tmpCaravanPawns.Clear();
		for (int i = 0; i < lordJob.lord.ownedPawns.Count; i++)
		{
			Pawn pawn = lordJob.lord.ownedPawns[i];
			tmpCaravanPawns.Add(new ThingCount(pawn, pawn.stackCount));
		}
		num += CollectionsMassCalculator.MassUsage(tmpCaravanPawns, IgnorePawnsInventoryMode.IgnoreIfAssignedToUnload);
		float num2 = CollectionsMassCalculator.Capacity(tmpCaravanPawns);
		tmpCaravanPawns.Clear();
		return num2 - num;
	}

	public static string AppendOverweightInfo(string text, float capacityLeft)
	{
		if (capacityLeft < 0f)
		{
			text += " (" + "OverweightLower".Translate() + ")";
		}
		return text;
	}
}
