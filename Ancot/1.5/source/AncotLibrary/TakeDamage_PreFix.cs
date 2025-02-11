using HarmonyLib;
using Verse;

namespace AncotLibrary;

[StaticConstructorOnStartup]
[HarmonyPatch(typeof(Thing), "TakeDamage")]
internal static class TakeDamage_PreFix
{
	[HarmonyPrefix]
	private static void Prefix(ref DamageInfo dinfo)
	{
		if (dinfo.Instigator != null && dinfo.Instigator is Pawn pawn)
		{
			float num = GameComponent_DamageBuffTracker.Tracker.CheckForDamageBuff(pawn);
			if (num != 0f)
			{
				dinfo.SetAmount(dinfo.Amount * (1f + num));
			}
		}
	}
}
