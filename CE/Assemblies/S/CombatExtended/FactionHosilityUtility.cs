using System;
using RimWorld;

namespace CombatExtended;

public static class FactionHosilityUtility
{
	public static ShellingResponseDef GetShellingResponseDef(this Faction faction)
	{
		return (faction?.def ?? null).GetShellingResponseDef();
	}

	public static ShellingResponseDef GetShellingResponseDef(this FactionDef factionDef)
	{
		if (factionDef == null)
		{
			return CE_ShellingResponseDefOf.CE_ShellingPreset_Undefined;
		}
		FactionDefExtensionCE factionDefExtensionCE = null;
		if (factionDef.HasModExtension<FactionDefExtensionCE>())
		{
			factionDefExtensionCE = factionDef.GetModExtension<FactionDefExtensionCE>();
			if (factionDefExtensionCE.shellingResponse != null)
			{
				return factionDefExtensionCE.shellingResponse;
			}
		}
		return factionDef.techLevel switch
		{
			TechLevel.Undefined => CE_ShellingResponseDefOf.CE_ShellingPreset_Undefined, 
			TechLevel.Animal => CE_ShellingResponseDefOf.CE_ShellingPreset_Animal, 
			TechLevel.Neolithic => CE_ShellingResponseDefOf.CE_ShellingPreset_Neolithic, 
			TechLevel.Medieval => CE_ShellingResponseDefOf.CE_ShellingPreset_Medieval, 
			TechLevel.Industrial => CE_ShellingResponseDefOf.CE_ShellingPreset_Industrial, 
			TechLevel.Spacer => CE_ShellingResponseDefOf.CE_ShellingPreset_Spacer, 
			TechLevel.Ultra => CE_ShellingResponseDefOf.CE_ShellingPreset_Ultra, 
			TechLevel.Archotech => CE_ShellingResponseDefOf.CE_ShellingPreset_Archotech, 
			_ => throw new NotImplementedException($"CE: GetShellingResponseDef() {factionDef.label} tech level {factionDef.techLevel} has no preset!"), 
		};
	}
}
