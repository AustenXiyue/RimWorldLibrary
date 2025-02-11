using System.Collections.Generic;
using Verse;

namespace CombatExtended;

public class Verb_LaunchProjectileChangeAble : Verb_ShootCE
{
	public Dictionary<AdditionalWeapon, int> UseManager = null;

	public ThingDef ProjectileInt;

	public bool fireswitch;

	public Pair<AdditionalWeapon, int> burstSwitcherPair;

	public ProjectileChangeExt ChangerExt => CasterPawn?.def.GetModExtension<ProjectileChangeExt>() ?? null;

	public override ThingDef Projectile
	{
		get
		{
			if (fireswitch)
			{
				_ = burstSwitcherPair;
				if (true && burstSwitcherPair.second > 0)
				{
					return burstSwitcherPair.first.projectile;
				}
				return ProjectileInt;
			}
			return base.Projectile;
		}
	}

	public override bool TryCastShot()
	{
		fireswitch = false;
		if (ChangerExt != null)
		{
			if (UseManager.NullOrEmpty())
			{
				if (UseManager == null)
				{
					UseManager = new Dictionary<AdditionalWeapon, int>();
				}
				foreach (AdditionalWeapon item in ChangerExt.additionalEquipment)
				{
					UseManager.Add(item, item.uses);
				}
			}
			if (base.Bursting)
			{
				_ = burstSwitcherPair;
				if (burstSwitcherPair.second < 1)
				{
					foreach (AdditionalWeapon item2 in ChangerExt.additionalEquipment)
					{
						if (Rand.Chance(item2.chanceToUse) && UseManager.TryGetValue(item2, 0) > 0)
						{
							UseManager.SetOrAdd(item2, UseManager.TryGetValue(item2, 0) - 1);
							ProjectileInt = item2.projectile;
							fireswitch = true;
							_ = burstSwitcherPair;
							if (false)
							{
								burstSwitcherPair = default(Pair<AdditionalWeapon, int>);
							}
							burstSwitcherPair.first = item2;
							burstSwitcherPair.second = item2.burstCount - 1;
						}
					}
				}
				else
				{
					burstSwitcherPair.second--;
				}
			}
		}
		return base.TryCastShot();
	}
}
