using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(Verb), "TryCastNextBurstShot")]
internal static class Harmony_Verb_TryCastNextBurstShot
{
	internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
	{
		List<CodeInstruction> codes = instructions.ToList();
		FieldInfo fverbProps = AccessTools.Field(typeof(Verb), "verbProps");
		FieldInfo fticksBetweenBurstShots = AccessTools.Field(typeof(VerbProperties), "ticksBetweenBurstShots");
		FieldInfo fnonInterruptingSelfCast = AccessTools.Field(typeof(VerbProperties), "nonInterruptingSelfCast");
		MethodInfo pCasterPawn = AccessTools.PropertyGetter(typeof(Verb), "CasterPawn");
		MethodInfo pSpawned = AccessTools.PropertyGetter(typeof(Pawn), "Spawned");
		bool ticksBetweenBurstShotsFinished = false;
		Label? blockEndLabel = default(Label?);
		for (int i = 0; i < codes.Count; i++)
		{
			if (!ticksBetweenBurstShotsFinished && CodeInstructionExtensions.LoadsField(codes[i], fverbProps, false) && CodeInstructionExtensions.LoadsField(codes[i + 1], fticksBetweenBurstShots, false))
			{
				ticksBetweenBurstShotsFinished = true;
				yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(Harmony_Verb_TryCastNextBurstShot), "GetTicksBetweenBurstShots", (Type[])null, (Type[])null));
				i++;
			}
			else if (CodeInstructionExtensions.Branches(codes[i], ref blockEndLabel) && CodeInstructionExtensions.LoadsField(codes[i - 1], fnonInterruptingSelfCast, false))
			{
				yield return codes[i];
				yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
				yield return new CodeInstruction(OpCodes.Callvirt, (object)pCasterPawn);
				yield return new CodeInstruction(OpCodes.Callvirt, (object)pSpawned);
				yield return new CodeInstruction(OpCodes.Brfalse_S, (object)blockEndLabel);
			}
			else
			{
				yield return codes[i];
				blockEndLabel = null;
			}
		}
	}

	private static int GetTicksBetweenBurstShots(Verb verb)
	{
		float num = verb.verbProps.ticksBetweenBurstShots;
		if (verb is Verb_LaunchProjectileCE && verb.EquipmentSource != null)
		{
			float statValue = verb.EquipmentSource.GetStatValue(CE_StatDefOf.TicksBetweenBurstShots);
			if (statValue > 0f)
			{
				num = statValue;
			}
		}
		return (int)num;
	}
}
