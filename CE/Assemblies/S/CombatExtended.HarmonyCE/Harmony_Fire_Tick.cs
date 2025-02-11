using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(Fire), "Tick")]
internal static class Harmony_Fire_Tick
{
	private const float SmokeDensityPerInterval = 900f;

	internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		foreach (CodeInstruction code in instructions)
		{
			if (code.opcode == OpCodes.Ldc_R4 && code.operand is float && (float)code.operand == 1f)
			{
				code.operand = 0.6f;
			}
			yield return code;
		}
	}

	internal static void Postfix(Fire __instance)
	{
		if (__instance.Spawned && Controller.settings.SmokeEffects && __instance.IsHashIntervalTick(30) && __instance.Position.Roofed(__instance.Map))
		{
			if (__instance.Position.GetGas(__instance.Map) is Smoke smoke)
			{
				smoke.UpdateDensityBy(900f);
				return;
			}
			Smoke smoke2 = (Smoke)GenSpawn.Spawn(CE_ThingDefOf.Gas_BlackSmoke, __instance.Position, __instance.Map);
			smoke2.UpdateDensityBy(900f);
		}
	}
}
