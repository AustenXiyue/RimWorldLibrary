using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace CombatExtended;

public class AmmoDef : ThingDef
{
	public AmmoCategoryDef ammoClass;

	public int defaultAmmoCount = 1;

	public float cookOffSpeed = 1f;

	public float cookOffFlashScale = 1f;

	public float worldTilesPerTick = 0.2f;

	public bool menuHidden;

	public ThingDef cookOffProjectile = null;

	public SoundDef cookOffSound = null;

	public SoundDef cookOffTailSound = null;

	public ThingDef detonateProjectile = null;

	public bool isMortarAmmo = false;

	public bool spawnAsSiegeAmmo = true;

	public int ammoCount = 1;

	public ThingDef partialUnloadAmmoDef = null;

	public List<string> ammoTags;

	private List<DefHyperlink> originalHyperlinks;

	private List<ThingDef> users;

	private List<AmmoSetDef> ammoSetDefs;

	[NoTranslate]
	private string oldDescription;

	public List<ThingDef> Users
	{
		get
		{
			if (users == null)
			{
				users = CE_Utility.allWeaponDefs.FindAll(delegate(ThingDef def)
				{
					CompProperties_AmmoUser compProperties = def.GetCompProperties<CompProperties_AmmoUser>();
					return compProperties?.ammoSet?.ammoTypes != null && compProperties.ammoSet.ammoTypes.Any((AmmoLink x) => x.ammo == this);
				});
				if (users != null && !users.Any())
				{
					return users;
				}
				if (descriptionHyperlinks.NullOrEmpty())
				{
					descriptionHyperlinks = new List<DefHyperlink>();
				}
				else if (originalHyperlinks.NullOrEmpty())
				{
					originalHyperlinks = new List<DefHyperlink>();
					foreach (DefHyperlink descriptionHyperlink in descriptionHyperlinks)
					{
						originalHyperlinks.Add(descriptionHyperlink);
					}
				}
				else
				{
					List<DefHyperlink> list = descriptionHyperlinks.Except(originalHyperlinks).ToList();
					foreach (DefHyperlink item in list)
					{
						descriptionHyperlinks.Remove(item);
						item.def.descriptionHyperlinks.Remove(this);
					}
				}
				foreach (ThingDef user in users)
				{
					descriptionHyperlinks.Add(user);
					if (user.descriptionHyperlinks.NullOrEmpty())
					{
						user.descriptionHyperlinks = new List<DefHyperlink>();
					}
					user.descriptionHyperlinks.Add(this);
				}
			}
			return users;
		}
	}

	public List<AmmoSetDef> AmmoSetDefs
	{
		get
		{
			if (ammoSetDefs == null)
			{
				ammoSetDefs = Users.Select((ThingDef x) => x.GetCompProperties<CompProperties_AmmoUser>().ammoSet).Distinct().ToList();
			}
			return ammoSetDefs;
		}
	}

	public void AddDescriptionParts()
	{
		if (ammoClass != null)
		{
			if (oldDescription.NullOrEmpty())
			{
				oldDescription = description;
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(oldDescription);
			stringBuilder.AppendLine("\n" + ammoClass.LabelCap + ":");
			stringBuilder.AppendLine(ammoClass.description);
			if (!Users.NullOrEmpty())
			{
				stringBuilder.AppendLine("\n" + "CE_UsedBy".Translate() + ":");
			}
			description = stringBuilder.ToString().TrimEndNewlines();
		}
	}

	public override IEnumerable<string> ConfigErrors()
	{
		foreach (string item in base.ConfigErrors())
		{
			yield return item;
		}
		if (HasComp(typeof(CompApparelReloadable)) && stackLimit > 1)
		{
			yield return "has compreloadable and a stack limit higher than 1. this is not recommended.";
		}
	}

	public override void ResolveReferences()
	{
		base.ResolveReferences();
		if (detonateProjectile == null)
		{
			return;
		}
		foreach (CompProperties comp in detonateProjectile.comps)
		{
			if (!comps.Any((CompProperties x) => x.compClass == comp.compClass) && (comp.compClass == typeof(CompFragments) || comp.compClass == typeof(CompExplosive) || comp.compClass == typeof(CompExplosiveCE)))
			{
				comps.Add(comp);
			}
		}
	}
}
