using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(ApparelGraphicRecordGetter), "TryGetGraphicApparel")]
internal static class Harmony_ApparelGraphicRecordGetter
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool IsHeadwear(ApparelLayerDef layer)
	{
		return layer.GetModExtension<ApparelLayerExtension>()?.IsHeadwear ?? false;
	}

	internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		bool write = false;
		foreach (CodeInstruction code in instructions)
		{
			if (write)
			{
				write = false;
				code.opcode = OpCodes.Brtrue;
			}
			if (code.opcode == OpCodes.Ldsfld && code.operand == AccessTools.Field(typeof(ApparelLayerDefOf), "Overhead"))
			{
				write = true;
				yield return new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(typeof(Harmony_ApparelGraphicRecordGetter), "IsHeadwear", (Type[])null, (Type[])null));
			}
			else
			{
				yield return code;
			}
		}
	}
}
