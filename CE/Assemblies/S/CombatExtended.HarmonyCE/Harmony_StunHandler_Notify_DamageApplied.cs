using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(StunHandler), "Notify_DamageApplied")]
public static class Harmony_StunHandler_Notify_DamageApplied
{
	public static bool Prefix(StunHandler __instance, DamageInfo dinfo, ref int ___stunTicksLeft, ref bool ___stunFromEMP, ref Dictionary<DamageDef, int> ___adaptationTicksLeft)
	{
		if (!__instance.CanBeStunnedByDamage(dinfo.Def))
		{
			return false;
		}
		Pawn pawn = __instance.parent as Pawn;
		float num = 1f;
		float num2 = 0f;
		if (pawn != null)
		{
			if (pawn.Downed || pawn.Dead)
			{
				return false;
			}
			num = pawn.BodySize;
			StatDef stunResistStat = dinfo.Def.stunResistStat;
			if (stunResistStat != null && !stunResistStat.Worker.IsDisabledFor(pawn))
			{
				num2 = pawn.GetStatValue(dinfo.Def.stunResistStat);
			}
		}
		float num3 = ((float?)dinfo.Def.constantStunDurationTicks) ?? (dinfo.Amount * 30f);
		if (__instance.CanAdaptToDamage(dinfo.Def))
		{
			int num4 = Mathf.RoundToInt(dinfo.Amount * 45f * num);
			int num5 = Mathf.RoundToInt(num3 * Mathf.Clamp01(1f - num2));
			if (___adaptationTicksLeft.TryGetValue(dinfo.Def, out var value) && value > 0)
			{
				float num6 = (float)value / (float)num4 * 15f;
				if (Rand.Value * 100f > num6)
				{
					___adaptationTicksLeft[dinfo.Def] += num4;
					___stunFromEMP = dinfo.Def == DamageDefOf.EMP;
					__instance.StunFor(num5, dinfo.Instigator);
					return false;
				}
				MoteMakerCE.ThrowText(new Vector3((float)__instance.parent.Position.x + 1f, __instance.parent.Position.y, (float)__instance.parent.Position.z + 1f), __instance.parent.Map, "Adapted".Translate(), Color.white);
				int num7 = Mathf.RoundToInt(Mathf.Sqrt(dinfo.Amount * 45f));
				if (num7 < value)
				{
					___adaptationTicksLeft[dinfo.Def] -= num7;
				}
				else
				{
					float num8 = (num7 - value) / num7;
					___adaptationTicksLeft[dinfo.Def] = Mathf.RoundToInt((float)num4 * num8);
					num5 = Mathf.RoundToInt((float)num5 * num8);
					___stunFromEMP = dinfo.Def == DamageDefOf.EMP;
					__instance.StunFor(num5, dinfo.Instigator);
				}
				return false;
			}
			___stunFromEMP = dinfo.Def == DamageDefOf.EMP;
			__instance.StunFor(num5, dinfo.Instigator);
			___adaptationTicksLeft.SetOrAdd(dinfo.Def, num4);
			return false;
		}
		return true;
	}

	public static void Postfix(StunHandler __instance, DamageInfo dinfo)
	{
		if (dinfo.Def != DamageDefOf.EMP)
		{
			return;
		}
		float num = dinfo.Amount;
		if (__instance.parent is Pawn pawn)
		{
			RaceProperties raceProps = pawn.RaceProps;
			if (raceProps != null && raceProps.IsFlesh)
			{
				num = Mathf.RoundToInt(num * 0.25f);
			}
		}
		DamageInfo dinfo2 = new DamageInfo(CE_DamageDefOf.Electrical, num, 9999f, dinfo.Angle, dinfo.Instigator, dinfo.HitPart, dinfo.Weapon, dinfo.Category, null, dinfo.InstigatorGuilty);
		__instance.parent.TakeDamage(dinfo2);
	}
}
