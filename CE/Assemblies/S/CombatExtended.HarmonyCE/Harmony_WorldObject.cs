using System;
using CombatExtended.WorldObjects;
using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace CombatExtended.HarmonyCE;

public static class Harmony_WorldObject
{
	[HarmonyPatch(typeof(WorldObject), "SpawnSetup")]
	public static class Harmony_WorldObject_SpawnSetup
	{
		public static void Postfix(WorldObject __instance)
		{
			try
			{
				if (__instance.Spawned)
				{
					Find.World.GetComponent<WorldObjectTrackerCE>().TryRegister(__instance);
				}
			}
			catch (Exception arg)
			{
				Log.Error($"CE: Harmony_WorldObject_SpawnSetup {arg}");
			}
		}
	}

	[HarmonyPatch(typeof(WorldObject), "Destroy")]
	public static class Harmony_WorldObject_Destroy
	{
		public static void Prefix(WorldObject __instance)
		{
			try
			{
				if (__instance.Spawned)
				{
					Find.World.GetComponent<WorldObjectTrackerCE>().TryDeRegister(__instance);
				}
			}
			catch (Exception arg)
			{
				Log.Error($"CE: Harmony_WorldObject_Destroy {arg}");
			}
		}
	}
}
