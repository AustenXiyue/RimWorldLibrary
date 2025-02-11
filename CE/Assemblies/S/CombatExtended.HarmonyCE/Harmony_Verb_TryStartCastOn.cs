using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using CombatExtended.AI;
using HarmonyLib;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(Verb), "TryStartCastOn", new Type[]
{
	typeof(LocalTargetInfo),
	typeof(LocalTargetInfo),
	typeof(bool),
	typeof(bool),
	typeof(bool),
	typeof(bool)
})]
internal static class Harmony_Verb_TryStartCastOn
{
	private static MethodBase mCausesTimeSlowdown = AccessTools.Method(typeof(Verb), "CausesTimeSlowdown", (Type[])null, (Type[])null);

	[HarmonyTranspiler]
	private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
	{
		List<CodeInstruction> codes = instructions.ToList();
		bool finished = false;
		Label l1 = generator.DefineLabel();
		for (int i = 0; i < codes.Count; i++)
		{
			CodeInstruction code = codes[i];
			if (!finished && CodeInstructionExtensions.OperandIs(codes[i], (MemberInfo)mCausesTimeSlowdown))
			{
				finished = true;
				yield return CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldarg_0, (object)null), code);
				yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
				yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
				yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(Harmony_Verb_TryStartCastOn), "CheckReload", (Type[])null, (Type[])null));
				yield return new CodeInstruction(OpCodes.Brtrue_S, (object)l1);
				yield return new CodeInstruction(OpCodes.Pop, (object)null);
				yield return new CodeInstruction(OpCodes.Pop, (object)null);
				yield return new CodeInstruction(OpCodes.Ldc_I4_0, (object)null);
				yield return new CodeInstruction(OpCodes.Ret, (object)null);
				code.labels.Add(l1);
			}
			yield return code;
		}
	}

	private static bool CheckReload(Verb __instance, LocalTargetInfo castTarg, LocalTargetInfo destTarg)
	{
		if (!(__instance is Verb_ShootCE) && !(__instance is Verb_ShootCEOneUse))
		{
			return true;
		}
		if (__instance.CasterIsPawn)
		{
			CompTacticalManager tacticalManager = __instance.CasterPawn.GetTacticalManager();
			if (tacticalManager != null)
			{
				return tacticalManager.TryStartCastChecks(__instance, castTarg, destTarg);
			}
		}
		CompAmmoUser compAmmoUser = __instance.EquipmentSource.TryGetComp<CompAmmoUser>();
		if (compAmmoUser == null || !compAmmoUser.HasMagazine || compAmmoUser.CurMagCount > 0)
		{
			return true;
		}
		compAmmoUser.TryStartReload();
		return false;
	}
}
