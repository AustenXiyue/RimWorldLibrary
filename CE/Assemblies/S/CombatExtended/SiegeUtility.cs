using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace CombatExtended;

[StaticConstructorOnStartup]
public class SiegeUtility
{
	public static readonly int MinRequiredConstructionSkill;

	static SiegeUtility()
	{
		MinRequiredConstructionSkill = (from def in DefDatabase<ThingDef>.AllDefsListForReading
			where def.building?.buildingTags.Contains("Artillery_BaseDestroyer") ?? false
			select def.constructionSkillPrerequisite).Max();
	}

	public static bool IsValidShellType(Thing thing, LordToil_Siege siege)
	{
		ThingDef def2 = thing.def;
		AmmoDef ammoDef = def2 as AmmoDef;
		if (ammoDef != null && ammoDef.spawnAsSiegeAmmo)
		{
			return UniqueArtilleryDefs(siege).SelectMany((ThingDef def) => def.building.turretGunDef.comps).OfType<CompProperties_AmmoUser>().SelectMany((CompProperties_AmmoUser props) => props.ammoSet.ammoTypes)
				.Any((AmmoLink ammoLink) => ammoLink.ammo == ammoDef);
		}
		return false;
	}

	public static void DropAdditionalShells(LordToil_Siege siege)
	{
		Lord lord = siege.lord;
		bool allowToxGas = false;
		if (ModsConfig.BiotechActive && lord.faction.def == FactionDefOf.PirateWaster)
		{
			allowToxGas = true;
		}
		foreach (ThingDef item in UniqueArtilleryDefs(siege))
		{
			ThingDef thingDef = TurretGunUtility.TryFindRandomShellDef(item, allowEMP: false, allowToxGas, mustHarmHealth: true, lord.faction.def.techLevel, allowAntigrainWarhead: false, -1f, lord.faction);
			if (thingDef != null)
			{
				siege.DropSupplies(thingDef, 6);
			}
		}
	}

	private static IEnumerable<ThingDef> UniqueArtilleryDefs(LordToil_Siege siege)
	{
		return (from t in siege.lord.ownedBuildings
			select t.def into def
			where def.building.buildingTags.Contains("Artillery_BaseDestroyer")
			select def).Distinct();
	}
}
