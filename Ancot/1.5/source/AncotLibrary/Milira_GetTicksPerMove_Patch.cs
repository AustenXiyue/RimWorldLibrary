using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace AncotLibrary;

[HarmonyPatch(new Type[]
{
	typeof(List<Pawn>),
	typeof(float),
	typeof(float),
	typeof(StringBuilder)
})]
[HarmonyPatch(typeof(CaravanTicksPerMoveUtility))]
[HarmonyPatch("GetTicksPerMove")]
public static class Milira_GetTicksPerMove_Patch
{
	[HarmonyPostfix]
	public static void Postfix(List<Pawn> pawns, ref int __result, float massUsage, float massCapacity, StringBuilder explanation = null)
	{
		float num = 1f;
		foreach (Pawn pawn in pawns)
		{
			num *= pawn.GetStatValue(AncotDefOf.Ancot_CaravanMoveSpeedFactor);
		}
		if (explanation != null && num != 1f)
		{
			explanation.AppendLine("\n\n" + "Ancot.CaravanMoveSpeedFactor".Translate() + ": " + num.ToStringPercent());
		}
		__result = (int)((float)__result / num);
	}
}
