using System.Collections.Generic;
using Verse;

namespace CombatExtended;

public class CompProperties_ArmorDurability : CompProperties
{
	public float Durability;

	public bool Regenerates;

	public float RegenInterval;

	public float RegenValue;

	public bool Repairable;

	public List<ThingDefCountClass> RepairIngredients;

	public int RepairTime;

	public float RepairValue;

	public bool CanOverHeal;

	public float MaxOverHeal;

	public float MinArmorValueSharp = -1f;

	public float MinArmorValueBlunt = -1f;

	public float MinArmorValueHeat = -1f;

	public float MinArmorValueElectric = -1f;

	public float MinArmorPct = 0.25f;

	public CompProperties_ArmorDurability()
	{
		compClass = typeof(CompArmorDurability);
	}
}
