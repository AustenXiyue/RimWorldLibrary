using System.Collections.Generic;
using System.Linq;
using CombatExtended.Compatibility;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CombatExtended;

[StaticConstructorOnStartup]
public class CompMechAmmo : ThingComp
{
	private Pawn _parentPawn;

	private Pawn_InventoryTracker _pawnInventory;

	private CompAmmoUser _ammoUser;

	private ThingWithComps _cachedEquipment;

	private static Texture2D _gizmoIconSetMagCount;

	private static Texture2D _gizmoIconTakeAmmoNow;

	private readonly string _txtSetMagCount = "MTA_SetMagCount".Translate();

	private readonly string _txtTakeAmmoNow = "MTA_TakeAmmoNow".Translate();

	private Dictionary<AmmoDef, int> _loadouts = new Dictionary<AmmoDef, int>();

	public static readonly int REFRESH_INTERVAL = 6000;

	public Texture2D GizmoIcon_SetMagCount
	{
		get
		{
			if (_gizmoIconSetMagCount == null)
			{
				_gizmoIconSetMagCount = ContentFinder<Texture2D>.Get(Props.gizmoIconSetMagCount, reportFailure: false);
			}
			return _gizmoIconSetMagCount;
		}
	}

	public Texture2D GizmoIcon_TakeAmmoNow
	{
		get
		{
			if (_gizmoIconTakeAmmoNow == null)
			{
				_gizmoIconTakeAmmoNow = ContentFinder<Texture2D>.Get(Props.gizmoIconTakeAmmoNow, reportFailure: false);
			}
			return _gizmoIconTakeAmmoNow;
		}
	}

	public Pawn ParentPawn
	{
		get
		{
			if (_parentPawn == null)
			{
				_parentPawn = parent as Pawn;
			}
			return _parentPawn;
		}
	}

	public Pawn_InventoryTracker PawnInventory
	{
		get
		{
			if (_pawnInventory == null)
			{
				_pawnInventory = ParentPawn?.inventory;
			}
			return _pawnInventory;
		}
	}

	public CompAmmoUser AmmoUser
	{
		get
		{
			if (_cachedEquipment != ParentPawn.equipment.Primary)
			{
				_cachedEquipment = ParentPawn.equipment.Primary;
				_ammoUser = _cachedEquipment?.GetComp<CompAmmoUser>();
			}
			if (_ammoUser == null)
			{
				_ammoUser = ParentPawn?.equipment.Primary?.GetComp<CompAmmoUser>();
			}
			return _ammoUser;
		}
	}

	public Dictionary<AmmoDef, int> Loadouts
	{
		get
		{
			if (_loadouts == null)
			{
				_loadouts = new Dictionary<AmmoDef, int>();
			}
			return _loadouts;
		}
	}

	public CompProperties_MechAmmo Props => (CompProperties_MechAmmo)props;

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		if (IsWorkable())
		{
			yield return new Command_Action
			{
				action = SetMagCount,
				defaultLabel = _txtSetMagCount,
				icon = GizmoIcon_SetMagCount
			};
			yield return new Command_Action
			{
				action = TakeAmmoNow,
				defaultLabel = _txtTakeAmmoNow,
				icon = GizmoIcon_TakeAmmoNow
			};
		}
	}

	public override void CompTick()
	{
		if (parent.IsHashIntervalTick(REFRESH_INTERVAL) && IsWorkable())
		{
			TryMakeAmmoJob();
		}
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Collections.Look(ref _loadouts, "MTA_Loadouts");
		if (Scribe.mode == LoadSaveMode.PostLoadInit && _loadouts == null)
		{
			_loadouts = new Dictionary<AmmoDef, int>();
		}
	}

	public void SetMagCount()
	{
		Current.Game.GetComponent<GameComponent_MechLoadoutDialogManger>()?.RegisterCompMechAmmo(this);
	}

	[Multiplayer.SyncMethod]
	public void TakeAmmoNow()
	{
		TryMakeAmmoJob(forced: true);
	}

	public void TryMakeAmmoJob(bool forced = false)
	{
		if (!AmmoUser.UseAmmo || ParentPawn == null || (ParentPawn.Drafted && !forced) || ParentPawn.CurJobDef == MTAJobDefOf.MTA_TakeAmmo || ParentPawn.GetComp<CompMechAmmo>() == null || AmmoUser == null)
		{
			return;
		}
		bool flag = false;
		foreach (AmmoLink ammoType in AmmoUser.Props.ammoSet.ammoTypes)
		{
			AmmoDef ammo = ammoType.ammo;
			int magCount = GetMagCount(ammo);
			int num = AmmoUser.NeedAmmo(ammo, AmmoUser.MagSize * magCount);
			if (ammo == AmmoUser.CurrentAmmo && magCount > 0)
			{
				flag = true;
			}
			if (num <= 0)
			{
				continue;
			}
			Thing thing = ParentPawn.FindBestAmmo(ammo);
			if (thing != null)
			{
				Job job = JobMaker.MakeJob(MTAJobDefOf.MTA_TakeAmmo, thing);
				job.count = num;
				Log.Message(" needs " + num + " " + ammo.label + " ammo.");
				if (ParentPawn.jobs.curJob.def != MTAJobDefOf.MTA_TakeAmmo)
				{
					ParentPawn.jobs.EndCurrentJob(JobCondition.InterruptForced, startNewJob: false);
				}
				ParentPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc, requestQueueing: true);
			}
		}
		if (!flag && AmmoUser.CurrentAmmo != null)
		{
			AmmoDef currentAmmo = AmmoUser.CurrentAmmo;
			AmmoUser.TryUnload(forceUnload: true);
		}
		if (ParentPawn.jobs.AllJobs().Any((Job x) => x.def == MTAJobDefOf.MTA_TakeAmmo))
		{
			ParentPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(MTAJobDefOf.MTA_UnloadAmmo, ParentPawn), JobTag.Misc, requestQueueing: true);
			if (!AmmoUser.FullMagazine && !ParentPawn.Drafted)
			{
				AmmoUser.TryUnload(forceUnload: true);
				ParentPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(CE_JobDefOf.ReloadWeapon, ParentPawn, AmmoUser.parent), JobTag.Misc, requestQueueing: true);
			}
		}
	}

	public int GetMagCount(AmmoDef ammo)
	{
		if (Loadouts.TryGetValue(ammo, out var value))
		{
			return value;
		}
		return 0;
	}

	public bool IsWorkable()
	{
		if (ParentPawn == null)
		{
			return false;
		}
		if (ParentPawn.Faction != Faction.OfPlayer)
		{
			return false;
		}
		if (ParentPawn.equipment.Primary == null)
		{
			return false;
		}
		if (AmmoUser == null)
		{
			return false;
		}
		if (!AmmoUser.UseAmmo)
		{
			return false;
		}
		return true;
	}

	public void DropUnusedAmmo()
	{
		foreach (AmmoLink ammoType in AmmoUser.Props.ammoSet.ammoTypes)
		{
			AmmoDef ammo = ammoType.ammo;
			int magCount = GetMagCount(ammo);
			int num = AmmoUser.NeedAmmo(ammo, AmmoUser.MagSize * magCount);
			if (num < 0)
			{
				ParentPawn.inventory.DropCount(ammo, -num);
			}
		}
	}
}
