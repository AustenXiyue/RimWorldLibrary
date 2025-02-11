using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using CombatExtended.Compatibility;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(FloatMenuMakerMap))]
[HarmonyPatch("AddHumanlikeOrders")]
[HarmonyPatch(new Type[]
{
	typeof(Vector3),
	typeof(Pawn),
	typeof(List<FloatMenuOption>)
})]
internal static class FloatMenuMakerMap_Modify_AddHumanlikeOrders
{
	private static readonly string logPrefix = "Combat Extended :: " + typeof(FloatMenuMakerMap_Modify_AddHumanlikeOrders).Name + " :: ";

	[HarmonyPostfix]
	private static void AddMenuItems(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
	{
		if (pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
		{
			foreach (LocalTargetInfo item2 in GenUI.TargetsAt(clickPos, TargetingParameters.ForRescue(pawn), thingsOnly: true))
			{
				Pawn patient = (Pawn)item2.Thing;
				if (!patient.Downed || !pawn.CanReach(patient, PathEndMode.InteractionCell, Danger.Deadly) || !patient.health.hediffSet.GetHediffsTendable().Any((Hediff h) => h.CanBeStabilized()))
				{
					continue;
				}
				if (pawn.WorkTypeIsDisabled(WorkTypeDefOf.Doctor))
				{
					opts.Add(new FloatMenuOption("CE_CannotStabilize".Translate() + ": " + "IncapableOfCapacity".Translate(WorkTypeDefOf.Doctor.gerundLabel), null));
					continue;
				}
				string label = "CE_Stabilize".Translate(patient.LabelCap);
				opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, delegate
				{
					Stabilize(pawn, patient);
				}, MenuOptionPriority.Default, null, patient), pawn, patient));
			}
		}
		IntVec3 c = IntVec3.FromVector3(clickPos);
		CompInventory compInventory = pawn.TryGetComp<CompInventory>();
		if (compInventory == null)
		{
			return;
		}
		List<Thing> thingList = c.GetThingList(pawn.Map);
		foreach (Thing item in thingList)
		{
			if (item == null || !item.def.alwaysHaulable || item is Corpse)
			{
				continue;
			}
			int count = 0;
			if (!pawn.CanReach(item, PathEndMode.Touch, Danger.Deadly))
			{
				opts.Add(new FloatMenuOption("CannotPickUp".Translate() + " " + item.LabelShort + " (" + "NoPath".Translate() + ")", null));
				continue;
			}
			if (!compInventory.CanFitInInventory(item, out count))
			{
				opts.Add(new FloatMenuOption("CannotPickUp".Translate(item.LabelShort, item) + " (" + "CE_InventoryFull".Translate() + ")", null));
				continue;
			}
			if (count == 1)
			{
				opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("PickUpOne".Translate(item.Label, item), delegate
				{
					Pickup(pawn, item);
				}, MenuOptionPriority.High), pawn, item));
				continue;
			}
			if (count < item.stackCount)
			{
				opts.Add(new FloatMenuOption("CannotPickUpAll".Translate(item.Label, item) + " (" + "CE_InventoryFull".Translate() + ")", null));
			}
			else
			{
				opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("PickUpAll".Translate(item.Label, item), delegate
				{
					PickupAll(pawn, item);
				}, MenuOptionPriority.High), pawn, item));
			}
			opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("PickUpSome".Translate(item.Label, item), delegate
			{
				int to = Mathf.Min(count, item.stackCount);
				Dialog_Slider window = new Dialog_Slider("PickUpCount".Translate(item.LabelShort, item), 1, to, delegate(int selectCount)
				{
					PickupCount(pawn, item, selectCount);
				});
				Find.WindowStack.Add(window);
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(CE_ConceptDefOf.CE_InventoryWeightBulk, KnowledgeAmount.SpecificInteraction);
			}, MenuOptionPriority.High), pawn, item));
		}
	}

	[Multiplayer.SyncMethod]
	private static void Stabilize(Pawn pawn, Pawn patient)
	{
		if (pawn.inventory == null || pawn.inventory.innerContainer == null || !pawn.inventory.innerContainer.Any((Thing t) => t.def.IsMedicine))
		{
			Messages.Message("CE_CannotStabilize".Translate() + ": " + "CE_NoMedicine".Translate(pawn), patient, MessageTypeDefOf.RejectInput);
			return;
		}
		Medicine medicine = (Medicine)pawn.inventory.innerContainer.OrderByDescending((Thing t) => t.GetStatValue(StatDefOf.MedicalPotency)).FirstOrDefault();
		if (medicine != null && pawn.inventory.innerContainer.TryDrop(medicine, pawn.Position, pawn.Map, ThingPlaceMode.Direct, 1, out var resultingThing))
		{
			Job job = JobMaker.MakeJob(CE_JobDefOf.Stabilize, patient, resultingThing);
			job.count = 1;
			pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(CE_ConceptDefOf.CE_Stabilizing, KnowledgeAmount.Total);
		}
	}

	[Multiplayer.SyncMethod]
	private static void Pickup(Pawn pawn, Thing item)
	{
		item.SetForbidden(value: false, warnOnFail: false);
		Job job = JobMaker.MakeJob(JobDefOf.TakeInventory, item);
		job.count = 1;
		pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
		pawn.Notify_HoldTrackerItem(item, 1);
		PlayerKnowledgeDatabase.KnowledgeDemonstrated(CE_ConceptDefOf.CE_InventoryWeightBulk, KnowledgeAmount.SpecificInteraction);
	}

	[Multiplayer.SyncMethod]
	private static void PickupAll(Pawn pawn, Thing item)
	{
		item.SetForbidden(value: false, warnOnFail: false);
		Job job = JobMaker.MakeJob(JobDefOf.TakeInventory, item);
		job.count = item.stackCount;
		pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
		pawn.Notify_HoldTrackerItem(item, item.stackCount);
		PlayerKnowledgeDatabase.KnowledgeDemonstrated(CE_ConceptDefOf.CE_InventoryWeightBulk, KnowledgeAmount.SpecificInteraction);
	}

	[Multiplayer.SyncMethod]
	private static void PickupCount(Pawn pawn, Thing item, int selectCount)
	{
		item.SetForbidden(value: false, warnOnFail: false);
		Job job = JobMaker.MakeJob(JobDefOf.TakeInventory, item);
		job.count = selectCount;
		pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
		pawn.Notify_HoldTrackerItem(item, selectCount);
	}

	private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		string value = "CannotPickUp";
		int num = -1;
		List<CodeInstruction> list = Modify_ForceWear(instructions).ToList();
		for (int i = 0; i < list.Count; i++)
		{
			CodeInstruction val = list[i];
			if (val.opcode == OpCodes.Ldstr && val.operand is string && (val.operand as string).Equals(value))
			{
				num = i;
				break;
			}
		}
		if (num < 0)
		{
			Log.Error("CE failed to patch FloatMenuMakerMap: invalid string index");
		}
		else
		{
			for (int num2 = num; num2 > 0; num2--)
			{
				CodeInstruction val2 = list[num2];
				if (val2.opcode == OpCodes.Callvirt && val2.operand as MethodInfo != null && (val2.operand as MethodInfo).Name == "get_IsPlayerHome")
				{
					val2.opcode = OpCodes.Ldc_I4_1;
					val2.operand = null;
					return list;
				}
			}
			Log.Error("CE failed to patch FloatMenuMakerMap: get_IsPlayerHome not found");
		}
		return instructions;
	}

	private static IEnumerable<CodeInstruction> Modify_ForceWear(IEnumerable<CodeInstruction> instructions)
	{
		int searchPhase = 0;
		bool patched = false;
		string targetString = "ForceWear";
		CodeInstruction previous = null;
		List<Label> startLabel = null;
		CodeInstruction apparelField1 = null;
		CodeInstruction apparelField2 = null;
		List<Label> branchLabel = null;
		foreach (CodeInstruction instruction in instructions)
		{
			if (searchPhase == 3)
			{
				if (instruction.labels != null)
				{
					branchLabel = instruction.labels;
				}
				break;
			}
			if (searchPhase == 2 && instruction.opcode == OpCodes.Callvirt && instruction.operand as MethodInfo != null && (instruction.operand as MethodInfo).Name == "Add" && previous.operand is LocalVariableInfo && (previous.operand as LocalVariableInfo).LocalType == typeof(FloatMenuOption))
			{
				searchPhase = 3;
			}
			if (searchPhase == 1 && instruction.opcode == OpCodes.Ldfld && instruction.operand as FieldInfo != null && (instruction.operand as FieldInfo).Name == "apparel")
			{
				apparelField1 = previous;
				apparelField2 = instruction;
				searchPhase = 2;
			}
			if (searchPhase == 0 && instruction.opcode == OpCodes.Ldstr && instruction.operand is string && (instruction.operand as string).Equals(targetString))
			{
				startLabel = instruction.labels;
				searchPhase = 1;
			}
			if (searchPhase > 0 && searchPhase < 3)
			{
				previous = instruction;
			}
		}
		if (!branchLabel.NullOrEmpty())
		{
			foreach (CodeInstruction instruction2 in instructions)
			{
				if (!patched && instruction2.opcode == OpCodes.Ldstr && instruction2.operand is string && (instruction2.operand as string).Equals(targetString))
				{
					CodeInstruction ldPawn = new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					ldPawn.labels.AddRange(startLabel);
					yield return ldPawn;
					yield return apparelField1;
					yield return apparelField2;
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)typeof(FloatMenuMakerMap_Modify_AddHumanlikeOrders).GetMethod("ForceWearInventoryCheck", AccessTools.all));
					yield return new CodeInstruction(OpCodes.Brfalse, (object)branchLabel.First());
					instruction2.labels.Clear();
				}
				yield return instruction2;
			}
			yield break;
		}
		Log.Error(logPrefix + "Error applying patch to ForceWear, no change.");
		foreach (CodeInstruction instruction3 in instructions)
		{
			yield return instruction3;
		}
	}

	private static bool ForceWearInventoryCheck(Pawn pawn, Apparel apparel, List<FloatMenuOption> opts)
	{
		CompInventory compInventory = pawn.TryGetComp<CompInventory>();
		if (compInventory != null && !compInventory.CanFitInInventory(apparel, out var _, ignoreEquipment: false, useApparelCalculations: true))
		{
			FloatMenuOption item = new FloatMenuOption("CannotWear".Translate(apparel.Label, apparel) + " (" + "CE_InventoryFull".Translate() + ")", null);
			opts.Add(item);
			return false;
		}
		return true;
	}
}
