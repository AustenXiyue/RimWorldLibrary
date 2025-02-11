using System.Collections.Generic;
using CombatExtended.Compatibility;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CombatExtended;

public class CompUnderBarrel : CompRangedGizmoGiver
{
	public CompEquippable _compEq;

	public CompAmmoUser _compAmmo;

	public CompFireModes _compFireModes;

	public CompProperties_FireModes _compPropsFireModes;

	public VerbProperties _defVerbProps;

	public CompProperties_AmmoUser _compPropsAmmo;

	public AmmoDef mainGunLoadedAmmo;

	public int mainGunMagCount;

	public AmmoDef UnderBarrelLoadedAmmo;

	public int UnderBarrelMagCount;

	public bool usingUnderBarrel;

	public CompProperties_UnderBarrel Props => (CompProperties_UnderBarrel)props;

	public CompEquippable CompEq
	{
		get
		{
			if (_compEq == null)
			{
				_compEq = parent.TryGetComp<CompEquippable>();
			}
			return _compEq;
		}
	}

	public CompAmmoUser CompAmmo
	{
		get
		{
			if (_compAmmo == null)
			{
				_compAmmo = parent.TryGetComp<CompAmmoUser>();
			}
			return _compAmmo;
		}
	}

	public CompFireModes CompFireModes
	{
		get
		{
			if (_compFireModes == null)
			{
				_compFireModes = parent.TryGetComp<CompFireModes>();
			}
			return _compFireModes;
		}
	}

	public CompProperties_FireModes CompPropsFireModes
	{
		get
		{
			if (_compPropsFireModes == null)
			{
				_compPropsFireModes = parent.def.comps.Find((CompProperties x) => x is CompProperties_FireModes) as CompProperties_FireModes;
			}
			return _compPropsFireModes;
		}
	}

	public VerbProperties DefVerbProps
	{
		get
		{
			if (_defVerbProps == null)
			{
				_defVerbProps = parent.def.Verbs.Find((VerbProperties x) => x is VerbPropertiesCE);
			}
			return _defVerbProps;
		}
	}

	public CompProperties_AmmoUser CompPropsAmmo
	{
		get
		{
			if (_compPropsAmmo == null)
			{
				_compPropsAmmo = (CompProperties_AmmoUser)parent.def.comps.Find((CompProperties x) => x is CompProperties_AmmoUser);
			}
			return _compPropsAmmo;
		}
	}

	public bool OneAmmoHolder => Props.oneAmmoHolder;

	[Multiplayer.SyncMethod]
	public void SwitchToUB()
	{
		if (!OneAmmoHolder)
		{
			mainGunLoadedAmmo = CompAmmo.CurrentAmmo;
			mainGunMagCount = CompAmmo.CurMagCount;
			CompAmmo.CurMagCount = UnderBarrelMagCount;
			CompAmmo.CurrentAmmo = UnderBarrelLoadedAmmo;
			CompAmmo.SelectedAmmo = CompAmmo.CurrentAmmo;
		}
		CompAmmo.props = Props.propsUnderBarrel;
		CompEq.PrimaryVerb.verbProps = Props.verbPropsUnderBarrel;
		CompFireModes.props = Props.propsFireModesUnderBarrel;
		if (CompAmmo.Wielder != null)
		{
			CompAmmo.Wielder.TryGetComp<CompInventory>().UpdateInventory();
			if (CompAmmo.Wielder.jobs?.curJob?.def == CE_JobDefOf.ReloadWeapon)
			{
				CompAmmo.Wielder.jobs.EndCurrentJob(JobCondition.InterruptForced);
			}
		}
		CompEq.PrimaryVerb.verbProps.burstShotCount = Props.propsFireModesUnderBarrel.aimedBurstShotCount;
		usingUnderBarrel = true;
		CompFireModes.InitAvailableFireModes();
	}

	[Multiplayer.SyncMethod]
	public void SwithToB()
	{
		if (!OneAmmoHolder)
		{
			UnderBarrelLoadedAmmo = CompAmmo.CurrentAmmo;
			UnderBarrelMagCount = CompAmmo.CurMagCount;
			CompAmmo.CurMagCount = mainGunMagCount;
			CompAmmo.CurrentAmmo = mainGunLoadedAmmo;
			CompAmmo.SelectedAmmo = CompAmmo.CurrentAmmo;
		}
		CompAmmo.props = CompPropsAmmo;
		CompEq.PrimaryVerb.verbProps = DefVerbProps.MemberwiseClone();
		CompFireModes.props = CompPropsFireModes;
		if (CompAmmo.Wielder != null)
		{
			CompAmmo.Wielder.TryGetComp<CompInventory>().UpdateInventory();
			if (CompAmmo.Wielder.jobs?.curJob?.def == CE_JobDefOf.ReloadWeapon)
			{
				CompAmmo.Wielder.jobs.EndCurrentJob(JobCondition.InterruptForced);
			}
		}
		CompEq.PrimaryVerb.verbProps.burstShotCount = DefVerbProps.burstShotCount;
		usingUnderBarrel = false;
		CompFireModes.InitAvailableFireModes();
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		if (CompEq.Holder?.Faction != Faction.OfPlayer && !DebugSettings.godMode)
		{
			yield break;
		}
		if (!usingUnderBarrel)
		{
			yield return new Command_Action
			{
				defaultLabel = (string.IsNullOrEmpty(Props.underBarrelLabel) ? "CE_SwitchAmmmoSetToUnderBarrel".Translate().ToString() : Props.underBarrelLabel),
				icon = ContentFinder<Texture2D>.Get(string.IsNullOrEmpty(Props.underBarrelIconTexPath) ? "UI/Buttons/Reload" : Props.underBarrelIconTexPath),
				defaultDesc = string.Concat(string.Concat("CE_UBGLStats".Translate() + "\n " + "WarmupTime".Translate() + ": ", Props.verbPropsUnderBarrel.warmupTime.ToString(), "\n ") + "Range".Translate() + ": ", Props.verbPropsUnderBarrel.range.ToString(), (Props.oneAmmoHolder || Props.propsUnderBarrel?.ammoSet?.label != null) ? "" : string.Concat("\n " + "CE_AmmoSet".Translate() + ": " + Props.propsUnderBarrel.ammoSet.label + "\n " + "CE_MagazineSize".Translate() + ": ", Props.propsUnderBarrel.magazineSize.ToString())),
				action = delegate
				{
					SwitchToUB();
				}
			};
		}
		else
		{
			yield return new Command_Action
			{
				defaultLabel = (string.IsNullOrEmpty(Props.standardLabel) ? "CE_SwitchAmmmoSetToNormalRifle".Translate().ToString() : Props.standardLabel),
				icon = ContentFinder<Texture2D>.Get(string.IsNullOrEmpty(Props.standardIconTexPath) ? "UI/Buttons/Reload" : Props.standardIconTexPath),
				action = delegate
				{
					SwithToB();
				}
			};
		}
	}

	public override void Initialize(CompProperties props)
	{
		if (parent.def.weaponTags.NullOrEmpty())
		{
			parent.def.weaponTags = new List<string> { "NoSwitch" };
		}
		else if (!parent.def.weaponTags.Contains("NoSwitch"))
		{
			parent.def.weaponTags.Add("NoSwitch");
		}
		base.Initialize(props);
	}

	public override void PostExposeData()
	{
		if (Scribe.mode == LoadSaveMode.Saving && usingUnderBarrel)
		{
			UnderBarrelMagCount = CompAmmo.CurMagCount;
			UnderBarrelLoadedAmmo = CompAmmo.CurrentAmmo;
		}
		Scribe_Values.Look(ref usingUnderBarrel, "usingUnderBarrel", defaultValue: false);
		Scribe_Defs.Look(ref mainGunLoadedAmmo, "mainGunAmmo");
		Scribe_Defs.Look(ref UnderBarrelLoadedAmmo, "UnderBarrelAmmo");
		Scribe_Values.Look(ref mainGunMagCount, "magCountMainGun", 0);
		Scribe_Values.Look(ref UnderBarrelMagCount, "UnderBarrelMagCount", 0);
		if (Scribe.mode == LoadSaveMode.PostLoadInit && usingUnderBarrel)
		{
			if (!OneAmmoHolder)
			{
				CompAmmo.CurMagCount = UnderBarrelMagCount;
				CompAmmo.CurrentAmmo = UnderBarrelLoadedAmmo;
			}
			CompAmmo.props = Props.propsUnderBarrel;
			CompEq.PrimaryVerb.verbProps = Props.verbPropsUnderBarrel;
			CompFireModes.props = Props.propsFireModesUnderBarrel;
			if (CompAmmo.Wielder != null)
			{
				CompAmmo.Wielder.TryGetComp<CompInventory>().UpdateInventory();
			}
		}
		base.PostExposeData();
	}
}
