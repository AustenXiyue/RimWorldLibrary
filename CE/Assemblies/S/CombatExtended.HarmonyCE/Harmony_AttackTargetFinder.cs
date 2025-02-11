using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using CombatExtended.AI;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CombatExtended.HarmonyCE;

internal static class Harmony_AttackTargetFinder
{
	[HarmonyPatch(typeof(AttackTargetFinder), "BestAttackTarget")]
	internal static class Harmony_AttackTargetFinder_BestAttackTarget
	{
		private static List<IAttackTarget> validTargets = new List<IAttackTarget>();

		private static bool EMPOnlyTargetsMechanoids()
		{
			return false;
		}

		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			//IL_0118: Unknown result type (might be due to invalid IL or missing references)
			//IL_0122: Expected O, but got Unknown
			//IL_012b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0135: Expected O, but got Unknown
			//IL_013e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0148: Expected O, but got Unknown
			//IL_0151: Unknown result type (might be due to invalid IL or missing references)
			//IL_015b: Expected O, but got Unknown
			//IL_0169: Unknown result type (might be due to invalid IL or missing references)
			//IL_0173: Expected O, but got Unknown
			//IL_0181: Unknown result type (might be due to invalid IL or missing references)
			//IL_018b: Expected O, but got Unknown
			//IL_019a: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a4: Expected O, but got Unknown
			//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cc: Expected O, but got Unknown
			//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01df: Expected O, but got Unknown
			List<CodeInstruction> list = instructions.ToList();
			for (int i = 0; i < instructions.Count(); i++)
			{
				if (list[i].opcode == OpCodes.Call && list[i].operand == AccessTools.Method(typeof(VerbUtility), "IsEMP", (Type[])null, (Type[])null))
				{
					list[i - 2].opcode = OpCodes.Nop;
					list[i - 1].opcode = OpCodes.Nop;
					list[i].operand = typeof(Harmony_AttackTargetFinder_BestAttackTarget).GetMethod("EMPOnlyTargetsMechanoids", AccessTools.all);
					break;
				}
			}
			MethodInfo methodInfo = AccessTools.Method(typeof(AttackTargetFinder), "HasRangedAttack", (Type[])null, (Type[])null);
			FieldInfo fieldInfo = AccessTools.FindIncludingInnerTypes<FieldInfo>(typeof(AttackTargetFinder), (Func<Type, FieldInfo>)((Type type) => AccessTools.DeclaredField(type, "innerValidator")));
			List<CodeInstruction> list2 = new List<CodeInstruction>();
			list2.Add(new CodeInstruction(OpCodes.Ldarg_1, (object)null));
			list2.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
			list2.Add(new CodeInstruction(OpCodes.Ldloc_0, (object)null));
			list2.Add(new CodeInstruction(OpCodes.Ldfld, (object)fieldInfo));
			list2.Add(new CodeInstruction(OpCodes.Ldarg_S, (object)4));
			list2.Add(new CodeInstruction(OpCodes.Ldarg_S, (object)7));
			list2.Add(new CodeInstruction(OpCodes.Ldarg_S, (object)9));
			list2.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(Harmony_AttackTargetFinder_BestAttackTarget), "FindAttackTargetForRangedAttack", (Type[])null, (Type[])null)));
			list2.Add(new CodeInstruction(OpCodes.Ret, (object)null));
			List<CodeInstruction> collection = list2;
			Label? label = default(Label?);
			Label? label2 = default(Label?);
			for (int j = 0; j < list.Count; j++)
			{
				if (CodeInstructionExtensions.Branches(list[j], ref label) && CodeInstructionExtensions.Calls(list[j - 1], methodInfo))
				{
					int num = list.FindIndex((CodeInstruction instr) => instr.labels.Contains(label.Value));
					CodeInstruction blockEndInstr = list[num];
					int num2 = 1 + list.FindLastIndex((CodeInstruction instr) => CodeInstructionExtensions.Branches(instr, ref label2) && blockEndInstr.labels.Contains(label2.Value));
					CodeInstruction val = list[num2];
					list.RemoveRange(num2, num - num2 - 1);
					list.InsertRange(num2, collection);
					CodeInstructionExtensions.MoveLabelsTo(val, list[num2]);
					break;
				}
			}
			return list;
		}

		internal static void Prefix(IAttackTargetSearcher searcher)
		{
			map = searcher.Thing?.Map;
			interceptors = (from t in searcher.Thing?.Map.listerThings.ThingsInGroup(ThingRequestGroup.ProjectileInterceptor)
				select t.TryGetComp<CompProjectileInterceptor>()).ToList() ?? new List<CompProjectileInterceptor>();
		}

		private static IAttackTarget FindAttackTargetForRangedAttack(TargetScanFlags targetScanFlags, IAttackTargetSearcher searcher, Predicate<IAttackTarget> attackTargetValidator, float maxDist, bool canBashDoors, bool canBashFences)
		{
			validTargets.Clear();
			Thing searcherThing = searcher.Thing;
			List<IAttackTarget> potentialTargetsFor = searcherThing.Map.attackTargetsCache.GetPotentialTargetsFor(searcher);
			IntVec3 position = searcherThing.Position;
			for (int i = 0; i < potentialTargetsFor.Count; i++)
			{
				IAttackTarget attackTarget = potentialTargetsFor[i];
				if (attackTarget.Thing.Position.InHorDistOf(position, maxDist) && attackTargetValidator(potentialTargetsFor[i]))
				{
					validTargets.Add(attackTarget);
				}
			}
			if (validTargets.Count == 0)
			{
				return null;
			}
			IAttackTarget randomShootingTargetByScore = AttackTargetFinder.GetRandomShootingTargetByScore(validTargets, searcher, searcher.CurrentEffectiveVerb);
			if (randomShootingTargetByScore != null || searcher is Building_Turret || (searcher is Pawn pawn && pawn.CurJobDef == JobDefOf.ManTurret))
			{
				return randomShootingTargetByScore;
			}
			if ((targetScanFlags & TargetScanFlags.NeedReachableIfCantHitFromMyPos) != 0 || (targetScanFlags & TargetScanFlags.NeedReachable) != 0)
			{
				return (IAttackTarget)GenClosest.ClosestThing_Global(position, validTargets, maxDist, (Thing target) => AttackTargetFinder.CanReach(searcherThing, target, canBashDoors, canBashFences));
			}
			return (IAttackTarget)GenClosest.ClosestThing_Global(position, validTargets, maxDist);
		}
	}

	[HarmonyPatch(typeof(AttackTargetFinder), "GetShootingTargetScore")]
	internal static class Harmony_AttackTargetFinder_GetShootingTargetScore
	{
		public static void Postfix(IAttackTarget target, IAttackTargetSearcher searcher, Verb verb, ref float __result)
		{
			float num = target.Thing.Position.DistanceTo(searcher.Thing.Position);
			if (target.Thing is Pawn other && searcher.Thing is Pawn pawn)
			{
				Pawn_PathFollower pather = pawn.pather;
				if (pather != null && pather.moving && pawn.EdgingCloser(other))
				{
					__result += (verb.EffectiveRange - num) / (verb.EffectiveRange + 1f) * 20f;
				}
			}
			LocalTargetInfo targetCurrentlyAimingAt = target.TargetCurrentlyAimingAt;
			if (targetCurrentlyAimingAt.IsValid && targetCurrentlyAimingAt.HasThing && targetCurrentlyAimingAt.Thing is Pawn pawn2)
			{
				Faction faction = pawn2.Faction;
				if (faction != null && faction.HostileTo(searcher.Thing.Faction))
				{
					CompSuppressable compSuppressable = pawn2.TryGetComp<CompSuppressable>();
					if (compSuppressable != null)
					{
						if (compSuppressable.isSuppressed)
						{
							__result += 10f;
						}
						if (compSuppressable.IsHunkering)
						{
							__result += 25f;
						}
					}
					Pawn_HealthTracker health = pawn2.health;
					if (health != null && health.HasHediffsNeedingTend())
					{
						__result += 10f;
					}
				}
			}
			LightingTracker lightingTracker = searcher.Thing.Map?.GetLightingTracker();
			if (lightingTracker != null)
			{
				__result += lightingTracker.CombatGlowAt(target.Thing.Position) * 5f;
			}
			if (map == null)
			{
				return;
			}
			Vector3 lineStart = searcher.Thing.Position.ToVector3();
			Vector3 lineEnd = target.Thing.Position.ToVector3();
			for (int i = 0; i < interceptors.Count; i++)
			{
				CompProjectileInterceptor compProjectileInterceptor = interceptors[i];
				if (!compProjectileInterceptor.Active)
				{
					continue;
				}
				Vector3 closest;
				if (compProjectileInterceptor.parent.Position.DistanceTo(target.Thing.Position) < compProjectileInterceptor.Props.radius)
				{
					if (compProjectileInterceptor.parent.Position.DistanceTo(searcher.Thing.Position) < compProjectileInterceptor.Props.radius)
					{
						__result += 30f;
					}
					else
					{
						__result -= 30f;
					}
				}
				else if (compProjectileInterceptor.parent.Position.DistanceTo(searcher.Thing.Position) > compProjectileInterceptor.Props.radius && compProjectileInterceptor.parent.Position.ToVector3().DistanceToSegment(lineStart, lineEnd, out closest) < compProjectileInterceptor.Props.radius)
				{
					__result -= 30f;
				}
			}
		}
	}

	private static Map map;

	private static List<CompProjectileInterceptor> interceptors;
}
