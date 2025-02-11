using System;
using CombatExtended.WorldObjects;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace CombatExtended.HarmonyCE;

public static class Harmony_WorldInterface
{
	[HarmonyPatch(typeof(WorldInterface), "WorldInterfaceOnGUI")]
	public static class Harmony_WorldInterface_WorldInterfaceOnGUI
	{
		public static void Postfix()
		{
			try
			{
				if (WorldRendererUtility.WorldRenderedNow && ExpandableWorldObjectsUtility.TransitionPct <= 0.25f)
				{
					WorldHealthGUIUtility.OnGUIWorldObjectHealth();
				}
			}
			catch (Exception arg)
			{
				Log.Error($"CE: Harmony_WorldInterface {arg}");
			}
		}
	}
}
