using HarmonyLib;
using Verse.AI;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(Job), "ExposeData")]
public static class Harmony_Job_ExposeData
{
	public static void Postfix(Job __instance)
	{
		if (__instance is JobCE jobCE)
		{
			jobCE.PostExposeData();
		}
	}
}
