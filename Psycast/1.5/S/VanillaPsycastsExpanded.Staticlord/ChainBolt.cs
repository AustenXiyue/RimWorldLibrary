using UnityEngine;
using Verse;
using VFECore.Abilities;
using VFEMech;

namespace VanillaPsycastsExpanded.Staticlord;

public class ChainBolt : TeslaProjectile
{
	protected override int MaxBounceCount
	{
		get
		{
			Ability sourceAbility = SourceAbility;
			return (sourceAbility != null) ? Mathf.RoundToInt(sourceAbility.GetPowerForPawn()) : ((TeslaProjectile)this).MaxBounceCount;
		}
	}

	private Ability SourceAbility
	{
		get
		{
			CompAbilityProjectile val = ((Thing)(object)this).TryGetComp<CompAbilityProjectile>();
			if (val != null)
			{
				Ability ability = val.ability;
				if (ability != null)
				{
					return ability;
				}
			}
			int count = base.allProjectiles.Count;
			while (count-- > 0)
			{
				val = ((Thing)(object)base.allProjectiles[count]).TryGetComp<CompAbilityProjectile>();
				if (val != null)
				{
					Ability ability2 = val.ability;
					if (ability2 != null)
					{
						return ability2;
					}
				}
			}
			return null;
		}
	}
}
