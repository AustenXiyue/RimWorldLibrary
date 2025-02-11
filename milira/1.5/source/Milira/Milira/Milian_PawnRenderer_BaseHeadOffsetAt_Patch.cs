using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace Milira;

[HarmonyPatch(typeof(PawnRenderer))]
[HarmonyPatch("BaseHeadOffsetAt")]
public static class Milian_PawnRenderer_BaseHeadOffsetAt_Patch
{
	[HarmonyPrefix]
	public static bool Prefix(PawnRenderer __instance, Rot4 rotation, ref Vector3 __result)
	{
		FieldInfo fieldInfo = AccessTools.Field(typeof(PawnRenderer), "pawn");
		Pawn pawn = (Pawn)fieldInfo.GetValue(__instance);
		if (MilianUtility.IsMilian(pawn))
		{
			Vector2 vector = new Vector2(0f, 0.4f);
			Vector2 vector2 = vector * Mathf.Sqrt(pawn.ageTracker.CurLifeStage.bodySizeFactor);
			switch (rotation.AsInt)
			{
			case 0:
				__result = new Vector3(0f, 0f, vector2.y);
				break;
			case 1:
				__result = new Vector3(vector2.x, 0f, vector2.y);
				break;
			case 2:
				__result = new Vector3(0f, 0f, vector2.y);
				break;
			case 3:
				__result = new Vector3(0f - vector2.x, 0f, vector2.y);
				break;
			default:
				Log.Error("BaseHeadOffsetAt error in " + pawn);
				__result = Vector3.zero;
				break;
			}
			return false;
		}
		return true;
	}
}
