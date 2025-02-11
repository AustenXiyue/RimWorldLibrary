using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(TurretGunUtility), "TryFindRandomShellDef")]
public static class Harmony_TurretGunUtility
{
	public static bool Prefix(ThingDef turret, bool allowEMP, bool allowToxGas, TechLevel techLevel, bool allowAntigrainWarhead, ref ThingDef __result)
	{
		if (!TurretGunUtility.NeedsShells(turret))
		{
			__result = null;
			return false;
		}
		CompProperties_AmmoUser compProperties_AmmoUser = turret.building.turretGunDef.comps.OfType<CompProperties_AmmoUser>().FirstOrDefault();
		if (compProperties_AmmoUser == null)
		{
			return true;
		}
		IEnumerable<AmmoDef> source = from ammoLink in compProperties_AmmoUser.ammoSet.ammoTypes
			let ammoDef = ammoLink.ammo
			where ammoDef.spawnAsSiegeAmmo
			let projectileDef = ammoLink.projectile
			let explosiveDamageDef = projectileDef.GetCompProperties<CompProperties_ExplosiveCE>()?.explosiveDamageType ?? projectileDef.GetCompProperties<CompProperties_Explosive>()?.explosiveDamageType
			let projectileDamageDef = projectileDef.projectile.damageDef
			where explosiveDamageDef != null || projectileDamageDef != null
			where allowEMP || (explosiveDamageDef != DamageDefOf.EMP && projectileDamageDef != DamageDefOf.EMP)
			where allowToxGas || !ModsConfig.BiotechActive || (explosiveDamageDef != DamageDefOf.ToxGas && projectileDamageDef != DamageDefOf.ToxGas)
			where allowAntigrainWarhead || ammoDef != ThingDefOf.Shell_AntigrainWarhead
			where techLevel == TechLevel.Undefined || (int)ammoDef.techLevel <= (int)techLevel
			select ammoDef;
		source.TryRandomElementByWeight((ThingDef def) => def.generateAllowChance, out __result);
		return false;
	}
}
