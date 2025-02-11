using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CombatExtended.HarmonyCE;

internal static class Harmony_PawnGraphicSet
{
	private static bool RenderSpecial(ApparelLayerDef layer)
	{
		ApparelLayerExtension modExtension = layer.GetModExtension<ApparelLayerExtension>();
		return (modExtension != null && modExtension.IsHeadwear) || layer.drawOrder > ApparelLayerDefOf.Shell.drawOrder;
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
				yield return new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(typeof(Harmony_PawnGraphicSet), "RenderSpecial", (Type[])null, (Type[])null));
			}
			else
			{
				yield return code;
			}
		}
	}
}
