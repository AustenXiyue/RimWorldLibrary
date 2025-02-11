using System;
using HarmonyLib;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(Game), "LoadGame")]
public static class Harmony_Game_LoadGame
{
	public static void Postfix()
	{
		try
		{
			CE_Scriber.Reset();
		}
		catch (Exception arg)
		{
			Log.Error($"CE: Late scriber is really broken {arg}!!");
		}
	}
}
