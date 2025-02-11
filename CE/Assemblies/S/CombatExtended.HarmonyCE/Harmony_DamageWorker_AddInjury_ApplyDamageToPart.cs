using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(DamageWorker_AddInjury), "ApplyDamageToPart")]
internal static class Harmony_DamageWorker_AddInjury_ApplyDamageToPart
{
	private static bool _applyingSecondary = false;

	private static bool shieldAbsorbed = false;

	private static readonly int[] ArmorBlockNullOps = new int[5] { 1, 3, 4, 5, 6 };

	private static void ArmorReroute(Pawn pawn, ref DamageInfo dinfo, out bool deflectedByArmor, out bool diminishedByArmor)
	{
		DamageInfo afterArmorDamage = ArmorUtilityCE.GetAfterArmorDamage(dinfo, pawn, dinfo.HitPart, out deflectedByArmor, out diminishedByArmor, out shieldAbsorbed);
		if (dinfo.HitPart != afterArmorDamage.HitPart && pawn.Spawned)
		{
			LessonAutoActivator.TeachOpportunity(CE_ConceptDefOf.CE_ArmorSystem, OpportunityType.Critical);
		}
		Patch_CheckDuplicateDamageToOuterParts.lastHitPartHealth = pawn.health.hediffSet.GetPartHealth(afterArmorDamage.HitPart);
		dinfo = afterArmorDamage;
	}

	internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Expected O, but got Unknown
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Expected O, but got Unknown
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Expected O, but got Unknown
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Expected O, but got Unknown
		List<CodeInstruction> list = instructions.ToList();
		int num = list.FirstIndexOf((CodeInstruction c) => c.operand == typeof(ArmorUtility).GetMethod("GetPostArmorDamage", AccessTools.all));
		int num2 = -1;
		for (int num3 = num; num3 > 0; num3--)
		{
			if (list[num3].opcode == OpCodes.Ldarg_2)
			{
				num2 = num3;
				break;
			}
		}
		if (num2 == -1)
		{
			Log.Error("CE failed to transpile DamageWorker_AddInjury: could not identify armor block start");
			return instructions;
		}
		List<CodeInstruction> range = list.GetRange(num2, num - num2);
		int[] armorBlockNullOps = ArmorBlockNullOps;
		foreach (int index in armorBlockNullOps)
		{
			range[index].opcode = OpCodes.Nop;
			range[index].operand = null;
		}
		list[num].operand = typeof(Harmony_DamageWorker_AddInjury_ApplyDamageToPart).GetMethod("ArmorReroute", AccessTools.all);
		list[num + 3] = new CodeInstruction(OpCodes.Call, (object)typeof(DamageInfo).GetMethod("get_Def"));
		list[num + 4] = new CodeInstruction(OpCodes.Stloc_S, (object)4);
		list.InsertRange(num + 1, (IEnumerable<CodeInstruction>)(object)new CodeInstruction[2]
		{
			new CodeInstruction(OpCodes.Ldarga_S, (object)1),
			new CodeInstruction(OpCodes.Call, (object)typeof(DamageInfo).GetMethod("get_Amount"))
		});
		return list;
	}

	internal static void Postfix(DamageInfo dinfo, Pawn pawn)
	{
		if (shieldAbsorbed || !(dinfo.Weapon?.projectile is ProjectilePropertiesCE projectilePropertiesCE) || projectilePropertiesCE.secondaryDamage.NullOrEmpty() || _applyingSecondary)
		{
			return;
		}
		_applyingSecondary = true;
		foreach (SecondaryDamage item in projectilePropertiesCE.secondaryDamage)
		{
			if (pawn.Dead || !Rand.Chance(item.chance))
			{
				break;
			}
			DamageInfo dinfo2 = item.GetDinfo(dinfo);
			pawn.TakeDamage(dinfo2);
		}
		_applyingSecondary = false;
	}
}
