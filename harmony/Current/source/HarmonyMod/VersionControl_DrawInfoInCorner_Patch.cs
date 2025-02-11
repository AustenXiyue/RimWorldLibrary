using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace HarmonyMod;

[HarmonyPatch(typeof(VersionControl), "DrawInfoInCorner")]
internal static class VersionControl_DrawInfoInCorner_Patch
{
	private static void Postfix()
	{
		string text = $"Harmony v{HarmonyMain.harmonyVersion}";
		Text.Font = GameFont.Small;
		GUI.color = Color.white.ToTransparent(0.5f);
		Vector2 vector = Text.CalcSize(text);
		Rect rect = new Rect(10f, 58f, vector.x, vector.y);
		Widgets.Label(rect, text);
		GUI.color = Color.white;
		if (Mouse.IsOver(rect))
		{
			TipSignal tip = new TipSignal("Harmony Mod v" + HarmonyMain.modVersion);
			TooltipHandler.TipRegion(rect, tip);
			Widgets.DrawHighlight(rect);
		}
	}
}
