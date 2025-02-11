using System;
using System.Collections.Generic;
using System.Linq;
using CombatExtended.AI;
using CombatExtended.Compatibility;
using CombatExtended.Utilities;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace CombatExtended;

public class CompAmmoUser : CompRangedGizmoGiver
{
	private int curMagCountInt = 0;

	private AmmoDef currentAmmoInt = null;

	private AmmoDef selectedAmmo;

	private Thing ammoToBeDeleted;

	public Building_Turret turret;

	internal static Type rgStance;

	private int _lastReloadJobTick = -1;

	public CompProperties_AmmoUser Props => (CompProperties_AmmoUser)props;

	public int MagsLeft
	{
		get
		{
			if (CompInventory != null)
			{
				CompInventory.UpdateInventory();
				int num = 0;
				foreach (AmmoLink ammoType in Props.ammoSet.ammoTypes)
				{
					num += CompInventory.AmmoCountOfDef(ammoType.ammo);
				}
				return num;
			}
			return 0;
		}
	}

	public int MagSize => (int)parent.GetStatValue(CE_StatDefOf.MagazineCapacity);

	public int CurAmmoCount => currentAmmoInt.ammoCount;

	public int MagAmmoCount => MagSize / CurAmmoCount;

	public int MagSizeOverride => (int)parent.GetStatValue(CE_StatDefOf.AmmoGenPerMagOverride);

	public int CurMagCount
	{
		get
		{
			return curMagCountInt;
		}
		set
		{
			if (curMagCountInt != value && value >= 0)
			{
				curMagCountInt = value;
				if (CompInventory != null)
				{
					CompInventory.UpdateInventory();
				}
			}
		}
	}

	public CompEquippable CompEquippable => parent.GetComp<CompEquippable>();

	public Pawn Wielder
	{
		get
		{
			if (CompEquippable != null && CompEquippable.PrimaryVerb != null && CompEquippable.PrimaryVerb.caster != null)
			{
				Pawn pawn = (CompEquippable?.parent?.ParentHolder as Pawn_InventoryTracker)?.pawn;
				if (pawn == null || pawn == CompEquippable?.PrimaryVerb?.CasterPawn)
				{
					return CompEquippable.PrimaryVerb.CasterPawn;
				}
			}
			return null;
		}
	}

	public bool IsEquippedGun => Wielder != null;

	private GunDrawExtension gunDrawExt => parent.def.GetModExtension<GunDrawExtension>();

	public Pawn Holder => Wielder ?? (CompEquippable?.parent.ParentHolder as Pawn_InventoryTracker)?.pawn;

	public bool UseAmmo => Props.ammoSet != null && AmmoUtility.IsAmmoSystemActive(Props.ammoSet);

	public bool IsAOEWeapon => parent.def.IsAOEWeapon();

	public bool HasAndUsesAmmoOrMagazine => !UseAmmo || HasAmmoOrMagazine;

	public bool HasAmmoOrMagazine => (HasMagazine && CurMagCount > 0) || HasAmmo;

	public virtual bool CanBeFiredNow => (HasMagazine && CurMagCount > 0) || (!HasMagazine && (HasAmmo || !UseAmmo));

	public bool HasAmmo => CompInventory != null && CompInventory.ammoList.Any((Thing x) => Props.ammoSet.ammoTypes.Any((AmmoLink a) => a.ammo == x.def));

	public bool HasMagazine => MagSize > 0;

	public AmmoDef CurrentAmmo
	{
		get
		{
			return UseAmmo ? currentAmmoInt : null;
		}
		set
		{
			currentAmmoInt = value;
		}
	}

	public bool EmptyMagazine => HasMagazine && CurMagCount == 0;

	public int MissingToFullMagazine
	{
		get
		{
			if (!HasMagazine)
			{
				return 0;
			}
			if (SelectedAmmo == CurrentAmmo)
			{
				return MagSize - CurMagCount;
			}
			return MagSize;
		}
	}

	public bool FullMagazine
	{
		get
		{
			if (UseAmmo)
			{
				return HasMagazine && SelectedAmmo == CurrentAmmo && CurMagCount >= MagSize;
			}
			return CurMagCount >= MagSize;
		}
	}

	public ThingDef CurAmmoProjectile => Props.ammoSet?.ammoTypes?.FirstOrDefault((AmmoLink x) => x.ammo == CurrentAmmo)?.projectile ?? parent.def.Verbs.FirstOrDefault().defaultProjectile;

	public CompInventory CompInventory => Holder.TryGetComp<CompInventory>();

	private IntVec3 Position
	{
		get
		{
			if (IsEquippedGun)
			{
				return Wielder.Position;
			}
			if (turret != null)
			{
				return turret.Position;
			}
			if (Holder != null)
			{
				return Holder.Position;
			}
			return parent.Position;
		}
	}

	private Map Map
	{
		get
		{
			if (Holder != null)
			{
				return Holder.MapHeld;
			}
			if (turret != null)
			{
				return turret.MapHeld;
			}
			return parent.MapHeld;
		}
	}

	public bool ShouldThrowMote => Props.throwMote && MagSize > 1;

	public AmmoDef SelectedAmmo
	{
		get
		{
			return selectedAmmo;
		}
		set
		{
			selectedAmmo = value;
			if (!HasMagazine && CurrentAmmo != value)
			{
				currentAmmoInt = value;
			}
		}
	}

	public override void Initialize(CompProperties vprops)
	{
		base.Initialize(vprops);
		if (!UseAmmo)
		{
			return;
		}
		if (Props.ammoSet.ammoTypes.NullOrEmpty())
		{
			Log.Error(parent.Label + " has no available ammo types");
			return;
		}
		if (currentAmmoInt == null)
		{
			currentAmmoInt = Props.ammoSet.ammoTypes[0].ammo;
		}
		if (selectedAmmo == null)
		{
			selectedAmmo = currentAmmoInt;
		}
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref curMagCountInt, "count", 0);
		Scribe_Defs.Look(ref currentAmmoInt, "currentAmmo");
		Scribe_Defs.Look(ref selectedAmmo, "selectedAmmo");
	}

	private void AssignJobToWielder(Job job)
	{
		if (Wielder.drafter != null)
		{
			Wielder.jobs.TryTakeOrderedJob(job, JobTag.Misc);
		}
		else
		{
			ExternalPawnDrafter.TakeOrderedJob(Wielder, job);
		}
	}

	public void Notify_ShotFired(int ammoConsumedPerShot = 1)
	{
		ammoConsumedPerShot = ((ammoConsumedPerShot <= 0) ? 1 : ammoConsumedPerShot);
		if (!IsEquippedGun && turret == null)
		{
			Log.Error(parent.ToString() + " tried reducing its ammo count without a wielder");
		}
		if (!HasMagazine)
		{
			if (ammoToBeDeleted != null)
			{
				ammoToBeDeleted.Destroy();
				ammoToBeDeleted = null;
				CompInventory.UpdateInventory();
			}
		}
		else
		{
			if (curMagCountInt <= 0)
			{
				Log.Error($"{parent} tried reducing its ammo count when already empty");
			}
			CurMagCount = ((curMagCountInt - ammoConsumedPerShot >= 0) ? (curMagCountInt - ammoConsumedPerShot) : 0);
		}
	}

	public bool Notify_PostShotFired()
	{
		if (!HasAmmoOrMagazine)
		{
			DoOutOfAmmoAction();
			return false;
		}
		return true;
	}

	public bool TryPrepareShot()
	{
		if (HasMagazine)
		{
			if (curMagCountInt <= 0)
			{
				CurMagCount = 0;
				return false;
			}
			return true;
		}
		if (UseAmmo)
		{
			if (!TryFindAmmoInInventory(out ammoToBeDeleted))
			{
				return false;
			}
			if (ammoToBeDeleted.def != CurrentAmmo)
			{
				currentAmmoInt = ammoToBeDeleted.def as AmmoDef;
			}
			if (ammoToBeDeleted.stackCount > 1)
			{
				ammoToBeDeleted = ammoToBeDeleted.SplitOff(1);
			}
		}
		return true;
	}

	[Multiplayer.SyncMethod]
	private void SyncedTryForceReload()
	{
		turret.TryForceReload();
	}

	[Multiplayer.SyncMethod]
	private void SyncedTryStartReload()
	{
		TryStartReload();
	}

	public void TryStartReload()
	{
		if ((DebugSettings.godMode && Wielder != null && (Wielder.IsColonistPlayerControlled || Wielder.IsColonyMech) && selectedAmmo != CurrentAmmo) || Wielder?.jobs.curDriver is IJobDriver_Tactical)
		{
			return;
		}
		if (!HasMagazine)
		{
			if (!CanBeFiredNow)
			{
				DoOutOfAmmoAction();
			}
		}
		else
		{
			if (!IsEquippedGun && turret == null)
			{
				return;
			}
			if (turret != null)
			{
				turret.TryOrderReload();
			}
			else
			{
				if (Wielder.stances.curStance.GetType() == rgStance)
				{
					return;
				}
				if (UseAmmo)
				{
					TryUnload();
					if (IsEquippedGun && !HasAmmo)
					{
						DoOutOfAmmoAction();
						return;
					}
				}
				if ((!Props.reloadOneAtATime || !UseAmmo || selectedAmmo != CurrentAmmo || CurMagCount != MagSize) && IsEquippedGun && _lastReloadJobTick != GenTicks.TicksGame && (Wielder.jobs.curJob?.def ?? null) != CE_JobDefOf.ReloadWeapon)
				{
					Job job = TryMakeReloadJob();
					if (job != null)
					{
						_lastReloadJobTick = GenTicks.TicksGame;
						job.playerForced = true;
						Wielder.jobs.StartJob(job, JobCondition.InterruptForced, null, Wielder.CurJob?.def != job.def, cancelBusyStances: true, null, null, fromQueue: false, canReturnCurJobToPool: false, null);
					}
				}
			}
		}
	}

	public void DropCasing(int count)
	{
		if (gunDrawExt != null && gunDrawExt.DropCasingWhenReload && CompEquippable?.PrimaryVerb is Verb_ShootCE verb_ShootCE && verb_ShootCE.VerbPropsCE.ejectsCasings)
		{
			for (int i = 0; i < count; i++)
			{
				verb_ShootCE.ExternalCallDropCasing(i);
			}
		}
	}

	public bool TryUnload(bool forceUnload = false)
	{
		Thing droppedAmmo;
		return TryUnload(out droppedAmmo, forceUnload);
	}

	public bool TryUnload(out Thing droppedAmmo, bool forceUnload = false)
	{
		droppedAmmo = null;
		if (!HasMagazine || (Holder == null && turret == null))
		{
			return false;
		}
		if (!UseAmmo || curMagCountInt == 0)
		{
			return true;
		}
		if (Props.reloadOneAtATime && !forceUnload && selectedAmmo == CurrentAmmo && turret == null)
		{
			return true;
		}
		Thing thing = ThingMaker.MakeThing(currentAmmoInt);
		thing.stackCount = curMagCountInt / CurAmmoCount;
		Thing thing2 = null;
		int num = curMagCountInt % CurAmmoCount;
		if (num > 0)
		{
			if (currentAmmoInt.HasComp(typeof(CompApparelReloadable)))
			{
				thing2 = ThingMaker.MakeThing(currentAmmoInt);
				((CompApparelVerbOwner_Charged)thing2.TryGetComp<CompApparelReloadable>()).remainingCharges = num;
			}
			else if (currentAmmoInt.partialUnloadAmmoDef != null)
			{
				thing2 = ThingMaker.MakeThing(currentAmmoInt.partialUnloadAmmoDef);
				thing2.stackCount = num;
			}
		}
		bool flag = false;
		if (CompInventory != null)
		{
			flag = curMagCountInt / CurAmmoCount != CompInventory.container.TryAdd(thing, thing.stackCount);
			if (thing2 != null)
			{
				CompInventory.container.TryAdd(thing2, num);
			}
		}
		else
		{
			flag = true;
		}
		if (flag)
		{
			if (!GenThing.TryDropAndSetForbidden(thing, Position, Map, ThingPlaceMode.Near, out droppedAmmo, turret.Faction != Faction.OfPlayer))
			{
				Log.Warning(string.Concat(GetType().Assembly.GetName().Name + " :: " + GetType().Name + " :: ", "Unable to drop ", thing.LabelCap, " on the ground, thing was destroyed."));
			}
			if (thing2 != null && !GenThing.TryDropAndSetForbidden(thing2, Position, Map, ThingPlaceMode.Near, out var _, turret.Faction != Faction.OfPlayer))
			{
				Log.Warning(string.Concat(GetType().Assembly.GetName().Name + " :: " + GetType().Name + " :: ", "Unable to drop ", thing2.LabelCap, " on the ground, thing was destroyed."));
			}
		}
		CurMagCount = 0;
		return true;
	}

	public Job TryMakeReloadJob()
	{
		if (!HasMagazine || (Holder == null && turret == null))
		{
			return null;
		}
		return JobMaker.MakeJob(CE_JobDefOf.ReloadWeapon, Holder, parent);
	}

	private void DoOutOfAmmoAction()
	{
		if (parent.def.weaponTags.Contains("NoSwitch") || (DebugSettings.godMode && Wielder != null && (Wielder.IsColonistPlayerControlled || Wielder.IsColonyMech)))
		{
			return;
		}
		if (ShouldThrowMote)
		{
			MoteMakerCE.ThrowText(Position.ToVector3Shifted(), Map, "CE_OutOfAmmo".Translate() + "!");
		}
		if (IsEquippedGun && CompInventory != null && (Wielder.CurJob == null || Wielder.CurJob.def != JobDefOf.Hunt))
		{
			if (CompInventory.TryFindViableWeapon(out var weapon, !Holder.IsColonist))
			{
				Holder.jobs.StartJob(JobMaker.MakeJob(CE_JobDefOf.EquipFromInventory, weapon), JobCondition.InterruptForced, null, resumeCurJobAfterwards: true, cancelBusyStances: true, null, null, fromQueue: false, canReturnCurJobToPool: false, null);
				return;
			}
			if (!Holder.IsColonist || !parent.def.IsAOEWeapon())
			{
				TryPickupAmmo();
			}
		}
		CompInventory?.SwitchToNextViableWeapon(!parent.def.weaponTags.Contains("NoSwitch"), !Holder.IsColonist, stopJob: false);
		CompAffectedByFacilities compAffectedByFacilities = turret?.TryGetComp<CompAffectedByFacilities>();
		if (compAffectedByFacilities == null)
		{
			return;
		}
		foreach (Thing linkedFacility in compAffectedByFacilities.linkedFacilities)
		{
			if (linkedFacility is Building_AutoloaderCE building_AutoloaderCE && building_AutoloaderCE.StartReload(this))
			{
				break;
			}
		}
	}

	public bool TryPickupAmmo()
	{
		if (!Holder.RaceProps.Humanlike)
		{
			return false;
		}
		if (Holder.MentalState != null)
		{
			return false;
		}
		IEnumerable<AmmoDef> supportedAmmo = Props.ammoSet.ammoTypes.Select((AmmoLink a) => a.ammo);
		foreach (AmmoThing item in Holder.Position.AmmoInRange(Holder.Map, 6f).Where(delegate(AmmoThing t)
		{
			int result;
			if (t != null)
			{
				if (supportedAmmo.Contains(t.AmmoDef))
				{
					result = ((!Holder.IsColonist || (!t.IsForbidden(Holder) && t.Position.AdjacentTo8WayOrInside(Holder))) ? 1 : 0);
					goto IL_005c;
				}
			}
			result = 0;
			goto IL_005c;
			IL_005c:
			return (byte)result != 0;
		}))
		{
			if (!Holder.CanReserve(item) || !Holder.CanReach(item, PathEndMode.InteractionCell, Danger.Unspecified) || !CompInventory.CanFitInInventory(item, out var count))
			{
				continue;
			}
			Thing thing = item;
			if (!thing.Position.AdjacentTo8WayOrInside(Holder))
			{
				Job job = JobMaker.MakeJob(JobDefOf.TakeInventory, thing);
				job.count = count;
				Holder.jobs.StartJob(job, JobCondition.InterruptForced, null, resumeCurJobAfterwards: false, cancelBusyStances: true, null, null, fromQueue: false, canReturnCurJobToPool: false, null);
			}
			else
			{
				thing = item.SplitOff(count);
				CompInventory.container.TryAddOrTransfer(thing);
			}
			Job j = TryMakeReloadJob();
			Holder.jobs.jobQueue.EnqueueFirst(j, null);
			return true;
		}
		return false;
	}

	public void LoadAmmo(Thing ammo = null)
	{
		Building_AutoloaderCE building_AutoloaderCE = null;
		if (parent is Building_AutoloaderCE)
		{
			building_AutoloaderCE = parent as Building_AutoloaderCE;
		}
		if (Holder == null && turret == null && building_AutoloaderCE == null)
		{
			Log.Error(parent.ToString() + " tried loading ammo with no owner");
			return;
		}
		int curMagCount;
		if (UseAmmo)
		{
			bool flag = false;
			Thing ammoThing;
			if (ammo == null)
			{
				if (!TryFindAmmoInInventory(out ammoThing))
				{
					DoOutOfAmmoAction();
					return;
				}
				flag = true;
			}
			else
			{
				ammoThing = ammo;
			}
			currentAmmoInt = (AmmoDef)ammoThing.def;
			CompApparelReloadable compApparelReloadable = ammoThing.TryGetComp<CompApparelReloadable>();
			if ((Props.reloadOneAtATime ? 1 : MagAmmoCount) < ammoThing.stackCount)
			{
				if (Props.reloadOneAtATime)
				{
					curMagCount = curMagCountInt + (((CompApparelVerbOwner_Charged)compApparelReloadable)?.remainingCharges ?? CurAmmoCount);
					ammoThing.stackCount--;
				}
				else
				{
					curMagCount = MagSize;
					ammoThing.stackCount -= MagAmmoCount;
				}
			}
			else
			{
				int num = ammoThing.stackCount;
				if (turret != null || building_AutoloaderCE != null)
				{
					num += curMagCountInt;
				}
				curMagCount = (Props.reloadOneAtATime ? (curMagCountInt + (((CompApparelVerbOwner_Charged)compApparelReloadable)?.remainingCharges ?? CurAmmoCount)) : num);
				if (flag)
				{
					CompInventory.container.Remove(ammoThing);
				}
				else if (!ammoThing.Destroyed)
				{
					ammoThing.Destroy();
				}
			}
		}
		else
		{
			curMagCount = (Props.reloadOneAtATime ? (curMagCountInt + 1) : MagSize);
		}
		CurMagCount = curMagCount;
		if (turret != null)
		{
			turret.SetReloading(reloading: false);
		}
		if (building_AutoloaderCE != null)
		{
			building_AutoloaderCE.isReloading = false;
		}
		if (parent.def.soundInteract != null)
		{
			parent.def.soundInteract.PlayOneShot(new TargetInfo(Position, Map));
		}
	}

	public void ResetAmmoCount(AmmoDef newAmmo = null)
	{
		if (newAmmo != null)
		{
			currentAmmoInt = newAmmo;
			selectedAmmo = newAmmo;
		}
		CurMagCount = MagSize;
	}

	public bool TryFindAmmoInInventory(out Thing ammoThing)
	{
		ammoThing = null;
		if (CompInventory == null)
		{
			return false;
		}
		ammoThing = CompInventory.ammoList.Find((Thing thing) => thing.def == selectedAmmo);
		if (ammoThing != null)
		{
			return true;
		}
		if (Props.reloadOneAtATime && CurMagCount > 0)
		{
			return false;
		}
		foreach (AmmoLink link in Props.ammoSet.ammoTypes)
		{
			ammoThing = CompInventory.ammoList.Find((Thing thing) => thing.def == link.ammo);
			if (ammoThing != null)
			{
				selectedAmmo = link.ammo;
				return true;
			}
		}
		return false;
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		CompMannable mannableComp = turret?.GetMannable();
		if ((Wielder == null || (!Wielder.IsColonistPlayerControlled && !Wielder.IsColonyMech)) && (turret?.Faction != Faction.OfPlayer || (mannableComp == null && !UseAmmo)))
		{
			yield break;
		}
		yield return new GizmoAmmoStatus
		{
			compAmmo = this
		};
		Action action = null;
		if (IsEquippedGun)
		{
			action = SyncedTryStartReload;
		}
		else if (mannableComp != null)
		{
			action = SyncedTryForceReload;
		}
		string tag = ((turret == null) ? ((!HasMagazine) ? "CE_ReloadNoMag" : "CE_Reload") : ((mannableComp != null) ? "CE_ReloadManned" : "CE_ReloadAuto"));
		LessonAutoActivator.TeachOpportunity(ConceptDef.Named(tag), turret, OpportunityType.GoodToKnow);
		yield return new Command_Reload
		{
			compAmmo = this,
			action = action,
			defaultLabel = (HasMagazine ? ((string)"CE_ReloadLabel".Translate()) : ""),
			defaultDesc = "CE_ReloadDesc".Translate(),
			icon = ((CurrentAmmo == null) ? ContentFinder<Texture2D>.Get("UI/Buttons/Reload") : selectedAmmo.IconTexture()),
			tutorTag = tag
		};
		if (DebugSettings.godMode)
		{
			yield return new Command_Action
			{
				action = delegate
				{
					CurMagCount = 0;
				},
				defaultLabel = "DEV: Set ammo to 0"
			};
			yield return new Command_Action
			{
				action = delegate
				{
					CurrentAmmo = SelectedAmmo;
					CurMagCount = MagSize;
				},
				defaultLabel = "DEV: Set ammo to max"
			};
		}
	}

	public override string TransformLabel(string label)
	{
		string text = ((UseAmmo && Controller.settings.ShowCaliberOnGuns) ? string.Concat(" (", Props.ammoSet.LabelCap, ") ") : "");
		return label + text;
	}
}
