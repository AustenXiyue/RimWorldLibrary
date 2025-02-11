using HarmonyLib;
using RimWorld;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(FireUtility), "TryStartFireIn")]
internal static class Harmony_FireUtility_TryStartFireIn
{
	private const float CatchFireChance = 0.5f;

	internal static void Postfix(IntVec3 c, Map map, float fireSize, bool __result)
	{
		if (__result)
		{
			Pawn firstThing = c.GetFirstThing<Pawn>(map);
			if (firstThing != null && Rand.Chance(0.5f * firstThing.GetStatValue(StatDefOf.Flammability)))
			{
				firstThing.TryAttachFire(fireSize, null);
			}
		}
	}
}
