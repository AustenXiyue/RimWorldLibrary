using System;
using HarmonyLib;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(Game), "ExposeData")]
public static class Harmony_Game_ExposeData
{
	public static void Postfix()
	{
		try
		{
			CE_Scriber.ExecuteLateScribe();
		}
		catch (Exception arg)
		{
			Log.Error($"CE: Late scriber is really broken {arg}!!");
		}
	}
}
