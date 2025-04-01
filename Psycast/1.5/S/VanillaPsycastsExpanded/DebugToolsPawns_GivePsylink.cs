using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaPsycastsExpanded;

[HarmonyPatch(typeof(DebugToolsPawns), "GivePsylink")]
public static class DebugToolsPawns_GivePsylink
{
	[HarmonyTranspiler]
	public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		FieldInfo info1 = AccessTools.Field(typeof(HediffDefOf), "PsychicAmplifier");
		FieldInfo info2 = AccessTools.Field(typeof(HediffDef), "maxSeverity");
		foreach (CodeInstruction instruction in instructions)
		{
			if (!CodeInstructionExtensions.LoadsField(instruction, info1, false))
			{
				if (CodeInstructionExtensions.LoadsField(instruction, info2, false))
				{
					yield return new CodeInstruction(OpCodes.Ldsfld, (object)AccessTools.Field(typeof(PsycasterPathDef), "TotalPoints"));
				}
				else
				{
					yield return instruction;
				}
			}
		}
	}
}
