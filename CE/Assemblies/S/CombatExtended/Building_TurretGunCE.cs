using System.Collections.Generic;
using System.Linq;
using System.Text;
using CombatExtended.CombatExtended.Jobs.Utils;
using CombatExtended.CombatExtended.LoggerUtils;
using CombatExtended.Compatibility;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace CombatExtended;

[StaticConstructorOnStartup]
public class Building_TurretGunCE : Building_Turret
{
	private const int minTicksBeforeAutoReload = 1800;

	private const int ticksBetweenAmmoChecks = 300;

	private const int ticksBetweenSlowAmmoChecks = 3600;

	public bool isSlow = false;

	public int burstCooldownTicksLeft;

	public int burstWarmupTicksLeft;

	public LocalTargetInfo currentTargetInt = LocalTargetInfo.Invalid;

	protected bool holdFire;

	private Thing gunInt;

	public TurretTop top;

	public CompPowerTrader powerComp;

	public CompCanBeDormant dormantComp;

	public CompInitiatable initiatableComp;

	public CompMannable mannableComp;

	public static Material ForcedTargetLineMat = MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.Transparent, new Color(1f, 0.5f, 0.5f));

	public bool targetingWorldMap = false;

	private CompAmmoUser compAmmo = null;

	private CompFireModes compFireModes = null;

	private CompChangeableProjectile compChangeable = null;

	public bool isReloading = false;

	private int ticksUntilAutoReload = 0;

	private bool everSpawned = false;

	public GlobalTargetInfo globalTargetInfo = GlobalTargetInfo.Invalid;

	private int TicksBetweenAmmoChecks => isSlow ? 3600 : 300;

	public virtual bool Active => (powerComp == null || powerComp.PowerOn) && (dormantComp == null || dormantComp.Awake) && (initiatableComp == null || initiatableComp.Initiated);

	public CompEquippable GunCompEq => Gun.TryGetComp<CompEquippable>();

	public override LocalTargetInfo CurrentTarget => currentTargetInt;

	protected bool WarmingUp => burstWarmupTicksLeft > 0;

	public override Verb AttackVerb => (Gun == null) ? null : GunCompEq.verbTracker.PrimaryVerb;

	public bool IsMannable => mannableComp != null;

	public bool PlayerControlled => (base.Faction == Faction.OfPlayer || MannedByColonist) && !MannedByNonColonist;

	protected virtual bool CanSetForcedTarget => mannableComp != null && PlayerControlled;

	protected bool CanToggleHoldFire => PlayerControlled;

	public bool IsMortar => def.building.IsMortar;

	public bool IsMortarOrProjectileFliesOverhead => Projectile.projectile.flyOverhead || IsMortar;

	private bool MannedByColonist => mannableComp != null && mannableComp.ManningPawn != null && mannableComp.ManningPawn.Faction == Faction.OfPlayer;

	private bool MannedByNonColonist => mannableComp != null && mannableComp.ManningPawn != null && mannableComp.ManningPawn.Faction != Faction.OfPlayer;

	public Thing Gun
	{
		get
		{
			if (gunInt == null && base.Map != null)
			{
				CELogger.Warn("Gun " + ToString() + " was referenced before PostMake. If you're seeing this, please report this to the Combat Extended team!", showOutOfDebugMode: true, "Gun");
				MakeGun();
				if (!everSpawned && (!base.Map.IsPlayerHome || base.Faction != Faction.OfPlayer))
				{
					compAmmo?.ResetAmmoCount();
					everSpawned = true;
				}
			}
			return gunInt;
		}
	}

	public virtual ThingDef Projectile
	{
		get
		{
			if (CompAmmo != null && CompAmmo.CurrentAmmo != null)
			{
				return CompAmmo.CurAmmoProjectile;
			}
			if (CompChangeable != null && CompChangeable.Loaded)
			{
				return CompChangeable.Projectile;
			}
			return GunCompEq.PrimaryVerb.verbProps.defaultProjectile;
		}
	}

	public CompChangeableProjectile CompChangeable
	{
		get
		{
			if (compChangeable == null && Gun != null)
			{
				compChangeable = Gun.TryGetComp<CompChangeableProjectile>();
			}
			return compChangeable;
		}
	}

	public CompAmmoUser CompAmmo
	{
		get
		{
			if (compAmmo == null && Gun != null)
			{
				compAmmo = Gun.TryGetComp<CompAmmoUser>();
			}
			return compAmmo;
		}
	}

	public CompFireModes CompFireModes
	{
		get
		{
			if (compFireModes == null && Gun != null)
			{
				compFireModes = Gun.TryGetComp<CompFireModes>();
			}
			return compFireModes;
		}
	}

	private ProjectilePropertiesCE ProjectileProps => ((ProjectilePropertiesCE)(compAmmo?.CurAmmoProjectile?.projectile)) ?? null;

	public float MaxWorldRange => ProjectileProps?.shellingProps.range ?? (-1f);

	public bool EmptyMagazine => CompAmmo?.EmptyMagazine ?? false;

	public bool FullMagazine => CompAmmo?.FullMagazine ?? false;

	public bool AutoReloadableMagazine => AutoReloadableNow && CompAmmo.CurMagCount <= Mathf.CeilToInt(CompAmmo.MagSize / 6);

	public bool AutoReloadableNow => (mannableComp == null || (!mannableComp.MannedNow && ticksUntilAutoReload == 0)) && Reloadable;

	public bool Reloadable => CompAmmo?.HasMagazine ?? false;

	public CompMannable MannableComp => mannableComp;

	public Building_TurretGunCE()
	{
		top = new TurretTop(this);
	}

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		base.SpawnSetup(map, respawningAfterLoad);
		base.Map.GetComponent<TurretTracker>().Register(this);
		dormantComp = GetComp<CompCanBeDormant>();
		initiatableComp = GetComp<CompInitiatable>();
		powerComp = GetComp<CompPowerTrader>();
		mannableComp = GetComp<CompMannable>();
		if (!everSpawned && (!base.Map.IsPlayerHome || base.Faction != Faction.OfPlayer))
		{
			compAmmo?.ResetAmmoCount();
			everSpawned = true;
		}
		if (!respawningAfterLoad)
		{
			CELogger.Message("top is " + (top?.ToString() ?? "null"), showOutOfDebugMode: false, "SpawnSetup");
			top.SetRotationFromOrientation();
			burstCooldownTicksLeft = def.building.turretInitialCooldownTime.SecondsToTicks();
			if (mannableComp != null)
			{
				ticksUntilAutoReload = 1800;
			}
		}
	}

	public override void PostMake()
	{
		base.PostMake();
		MakeGun();
	}

	private void MakeGun()
	{
		gunInt = ThingMaker.MakeThing(def.building.turretGunDef);
		compAmmo = gunInt.TryGetComp<CompAmmoUser>();
		InitGun();
	}

	public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
	{
		base.Map.GetComponent<TurretTracker>().Unregister(this);
		base.DeSpawn(mode);
		ResetCurrentTarget();
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Deep.Look(ref gunInt, "gunInt");
		InitGun();
		Scribe_Values.Look(ref isReloading, "isReloading", defaultValue: false);
		Scribe_Values.Look(ref ticksUntilAutoReload, "ticksUntilAutoReload", 0);
		Scribe_Values.Look(ref burstCooldownTicksLeft, "burstCooldownTicksLeft", 0);
		Scribe_Values.Look(ref burstWarmupTicksLeft, "burstWarmupTicksLeft", 0);
		Scribe_TargetInfo.Look(ref currentTargetInt, "currentTarget");
		Scribe_Values.Look(ref holdFire, "holdFire", defaultValue: false);
		Scribe_Values.Look(ref everSpawned, "everSpawned", defaultValue: false);
		Scribe_TargetInfo.Look(ref globalTargetInfo, "globalSourceInfo");
		BackCompatibility.PostExposeData(this);
	}

	public override bool ClaimableBy(Faction by, StringBuilder reason = null)
	{
		return base.ClaimableBy(by, reason) && (mannableComp == null || mannableComp.ManningPawn == null) && (!Active || mannableComp != null) && (((dormantComp == null || dormantComp.Awake) && (initiatableComp == null || initiatableComp.Initiated)) || (powerComp != null && !powerComp.PowerOn));
	}

	[Multiplayer.SyncMethod]
	public override void OrderAttack(LocalTargetInfo targ)
	{
		if (globalTargetInfo.IsValid)
		{
			ResetForcedTarget();
		}
		if (!targ.IsValid)
		{
			if (forcedTarget.IsValid)
			{
				ResetForcedTarget();
			}
			return;
		}
		if ((targ.Cell - base.Position).LengthHorizontal < GunCompEq.PrimaryVerb.verbProps.minRange)
		{
			Messages.Message("MessageTargetBelowMinimumRange".Translate(), this, MessageTypeDefOf.RejectInput);
			return;
		}
		if ((targ.Cell - base.Position).LengthHorizontal > GunCompEq.PrimaryVerb.verbProps.range)
		{
			Messages.Message("MessageTargetBeyondMaximumRange".Translate(), this, MessageTypeDefOf.RejectInput);
			return;
		}
		if (forcedTarget != targ)
		{
			forcedTarget = targ;
			if (burstCooldownTicksLeft <= 0)
			{
				TryStartShootSomething(canBeginBurstImmediately: false);
			}
		}
		if (holdFire)
		{
			Messages.Message("MessageTurretWontFireBecauseHoldFire".Translate(def.label), this, MessageTypeDefOf.RejectInput, historical: false);
		}
	}

	public override void Tick()
	{
		base.Tick();
		if (ticksUntilAutoReload > 0)
		{
			ticksUntilAutoReload--;
		}
		if (!isReloading && this.IsHashIntervalTick(TicksBetweenAmmoChecks))
		{
			CompMannable compMannable = MannableComp;
			if (compMannable != null && compMannable.MannedNow)
			{
				TryOrderReload();
			}
			else
			{
				TryReloadViaAutoLoader();
			}
		}
		if (!CanSetForcedTarget && !isReloading && forcedTarget.IsValid && !globalTargetInfo.IsValid && burstCooldownTicksLeft <= 0)
		{
			ResetForcedTarget();
		}
		if (!CanToggleHoldFire)
		{
			holdFire = false;
		}
		if (forcedTarget.ThingDestroyed)
		{
			ResetForcedTarget();
		}
		if (Active && (mannableComp == null || mannableComp.MannedNow) && base.Spawned && (!isReloading || !WarmingUp))
		{
			GunCompEq.verbTracker.VerbsTick();
			if (base.IsStunned || GunCompEq.PrimaryVerb.state == VerbState.Bursting)
			{
				return;
			}
			if (WarmingUp)
			{
				burstWarmupTicksLeft--;
				if (burstWarmupTicksLeft == 0)
				{
					BeginBurst();
				}
			}
			else
			{
				if (burstCooldownTicksLeft > 0)
				{
					burstCooldownTicksLeft--;
				}
				if (burstCooldownTicksLeft <= 0)
				{
					TryStartShootSomething(canBeginBurstImmediately: true);
				}
			}
			top.TurretTopTick();
		}
		else
		{
			ResetCurrentTarget();
		}
	}

	public virtual void TryStartShootSomething(bool canBeginBurstImmediately)
	{
		if (!base.Spawned || (holdFire && CanToggleHoldFire) || (Projectile.projectile.flyOverhead && base.Map.roofGrid.Roofed(base.Position)) || (CompAmmo != null && (isReloading || (mannableComp == null && CompAmmo.CurMagCount <= 0))))
		{
			ResetCurrentTarget();
			return;
		}
		if (!isReloading && (Projectile == null || (CompAmmo != null && !CompAmmo.CanBeFiredNow)))
		{
			ResetCurrentTarget();
			TryOrderReload();
			return;
		}
		bool flag = currentTargetInt.IsValid || globalTargetInfo.IsValid;
		currentTargetInt = (forcedTarget.IsValid ? forcedTarget : TryFindNewTarget());
		if (!flag && (currentTargetInt.IsValid || targetingWorldMap))
		{
			SoundDefOf.TurretAcquireTarget.PlayOneShot(new TargetInfo(base.Position, base.Map));
		}
		if (!targetingWorldMap && !currentTargetInt.IsValid)
		{
			ResetCurrentTarget();
		}
		else if (AttackVerb.verbProps.warmupTime > 0f)
		{
			burstWarmupTicksLeft = AttackVerb.verbProps.warmupTime.SecondsToTicks();
		}
		else if (targetingWorldMap && (!globalTargetInfo.IsValid || globalTargetInfo.WorldObject is DestroyedSettlement))
		{
			ResetForcedTarget();
			ResetCurrentTarget();
		}
		else if (canBeginBurstImmediately)
		{
			BeginBurst();
		}
		else
		{
			burstWarmupTicksLeft = 1;
		}
	}

	public virtual LocalTargetInfo TryFindNewTarget()
	{
		IAttackTargetSearcher attackTargetSearcher = TargSearcher();
		Faction faction = attackTargetSearcher.Thing.Faction;
		float range = AttackVerb.verbProps.range;
		if (Rand.Value < 0.5f && AttackVerb.ProjectileFliesOverhead() && faction.HostileTo(Faction.OfPlayer) && base.Map.listerBuildings.allBuildingsColonist.Where(delegate(Building x)
		{
			float num = AttackVerb.verbProps.EffectiveMinRange(x, this);
			float num2 = x.Position.DistanceToSquared(base.Position);
			return num2 > num * num && num2 < range * range;
		}).TryRandomElement(out var result))
		{
			return result;
		}
		TargetScanFlags targetScanFlags = TargetScanFlags.NeedThreat;
		if (!Projectile.projectile.flyOverhead)
		{
			targetScanFlags |= TargetScanFlags.NeedLOSToAll;
			targetScanFlags |= TargetScanFlags.LOSBlockableByGas;
		}
		else
		{
			targetScanFlags |= TargetScanFlags.NeedNotUnderThickRoof;
		}
		if (AttackVerb.IsIncendiary_Ranged())
		{
			targetScanFlags |= TargetScanFlags.NeedNonBurning;
		}
		return (Thing)AttackTargetFinder.BestShootTargetFromCurrentPosition(attackTargetSearcher, targetScanFlags, IsValidTarget, 0f, range);
	}

	private IAttackTargetSearcher TargSearcher()
	{
		if (mannableComp != null && mannableComp.MannedNow)
		{
			return mannableComp.ManningPawn;
		}
		return this;
	}

	private bool IsValidTarget(Thing t)
	{
		if (t is Pawn pawn)
		{
			if (mannableComp == null)
			{
				return !GenAI.MachinesLike(base.Faction, pawn);
			}
			if (pawn.RaceProps.Animal && pawn.Faction == Faction.OfPlayer)
			{
				return false;
			}
		}
		return true;
	}

	public virtual void BeginBurst()
	{
		ticksUntilAutoReload = 1800;
		if (AttackVerb is Verb_ShootMortarCE verb_ShootMortarCE)
		{
			if (globalTargetInfo.IsValid)
			{
				targetingWorldMap = true;
				GlobalTargetInfo sourceInfo = default(GlobalTargetInfo);
				sourceInfo.tileInt = base.Map.Tile;
				sourceInfo.cellInt = base.Position;
				sourceInfo.mapInt = base.Map;
				sourceInfo.thingInt = (IsMannable ? ((Thing)mannableComp.ManningPawn) : ((Thing)this));
				verb_ShootMortarCE.TryStartShelling(sourceInfo, globalTargetInfo);
			}
			else
			{
				verb_ShootMortarCE.globalTargetInfo = GlobalTargetInfo.Invalid;
				verb_ShootMortarCE.TryStartCastOn(CurrentTarget);
			}
		}
		else
		{
			AttackVerb.TryStartCastOn(CurrentTarget);
		}
		OnAttackedTarget(CurrentTarget);
	}

	public void BurstComplete()
	{
		burstCooldownTicksLeft = BurstCooldownTime().SecondsToTicks();
		if (CompAmmo != null && CompAmmo.CurMagCount <= 0)
		{
			TryForceReload();
		}
	}

	public float BurstCooldownTime()
	{
		if (def.building.turretBurstCooldownTime >= 0f)
		{
			return def.building.turretBurstCooldownTime;
		}
		return AttackVerb.verbProps.defaultCooldownTime;
	}

	public override string GetInspectString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		string inspectString = base.GetInspectString();
		if (!inspectString.NullOrEmpty())
		{
			stringBuilder.AppendLine(inspectString);
		}
		stringBuilder.AppendLine("GunInstalled".Translate() + ": " + Gun.LabelCap);
		if (GunCompEq.PrimaryVerb.verbProps.minRange > 0f)
		{
			stringBuilder.AppendLine("MinimumRange".Translate() + ": " + GunCompEq.PrimaryVerb.verbProps.minRange.ToString("F0"));
		}
		if (isReloading)
		{
			stringBuilder.AppendLine("CE_TurretReloading".Translate());
		}
		else if (base.Spawned && IsMortarOrProjectileFliesOverhead && base.Position.Roofed(base.Map))
		{
			stringBuilder.AppendLine("CannotFire".Translate() + ": " + "Roofed".Translate().CapitalizeFirst());
		}
		else if (base.Spawned && burstCooldownTicksLeft > 0)
		{
			stringBuilder.AppendLine("CanFireIn".Translate() + ": " + burstCooldownTicksLeft.ToStringSecondsFromTicks());
		}
		return stringBuilder.ToString().TrimEndNewlines();
	}

	public override void DrawAt(Vector3 drawLoc, bool flip = false)
	{
		Vector3 drawOffset = Vector3.zero;
		float angleOffset = 0f;
		if (Controller.settings.RecoilAnim)
		{
			CE_Utility.Recoil(def.building.turretGunDef, AttackVerb, out drawOffset, out angleOffset, top.CurRotation, handheld: false);
		}
		top.DrawTurret(drawLoc, drawOffset, angleOffset);
		base.DrawAt(drawLoc, flip);
	}

	public override void DrawExtraSelectionOverlays()
	{
		float range = GunCompEq.PrimaryVerb.verbProps.range;
		if (range < 90f)
		{
			GenDraw.DrawRadiusRing(base.Position, range);
		}
		float minRange = AttackVerb.verbProps.minRange;
		if (minRange < 90f && minRange > 0.1f)
		{
			GenDraw.DrawRadiusRing(base.Position, minRange);
		}
		if (WarmingUp)
		{
			int degreesWide = (int)((float)burstWarmupTicksLeft * 0.5f);
			GenDraw.DrawAimPie(this, CurrentTarget, degreesWide, (float)def.size.x * 0.5f);
		}
		if (forcedTarget.IsValid && (!forcedTarget.HasThing || forcedTarget.Thing.Spawned))
		{
			Vector3 b = ((!forcedTarget.HasThing) ? forcedTarget.Cell.ToVector3Shifted() : forcedTarget.Thing.TrueCenter());
			Vector3 a = this.TrueCenter();
			b.y = AltitudeLayer.MetaOverlays.AltitudeFor();
			a.y = b.y;
			GenDraw.DrawLineBetween(a, b, Building_TurretGun.ForcedTargetLineMat);
		}
	}

	public bool TryAttackWorldTarget(GlobalTargetInfo targetInfo, LocalTargetInfo localTarget)
	{
		ResetCurrentTarget();
		ResetForcedTarget();
		int num = Find.WorldGrid.TraversalDistanceBetween(base.Map.Tile, targetInfo.Tile, passImpassable: true, (int)(MaxWorldRange * 1.5f));
		if ((float)num > MaxWorldRange)
		{
			return false;
		}
		if (!Active)
		{
			return false;
		}
		if (localTarget.IsValid)
		{
			TryOrderAttackWorldTile(targetInfo, localTarget.Cell);
		}
		else
		{
			TryOrderAttackWorldTile(targetInfo, null);
		}
		return true;
	}

	public virtual void TryOrderAttackWorldTile(GlobalTargetInfo targetInf, IntVec3? cell = null)
	{
		int tile = base.Map.Tile;
		int tile2 = targetInf.Tile;
		Vector3 normalized = (Find.WorldGrid.GetTileCenter(tile) - Find.WorldGrid.GetTileCenter(tile2)).normalized;
		Vector3 origin = DrawPos.Yto0();
		Vector3 vector = base.Map.Size.ToVector3();
		vector.y = Mathf.Max(vector.x, vector.z);
		Ray ray = new Ray(origin, normalized);
		new Bounds(vector.Yto0() / 2f, vector).IntersectRay(ray, out var distance);
		Vector3 point = ray.GetPoint(distance);
		point.x = Mathf.Clamp(point.x, 0f, vector.x - 1f);
		point.z = Mathf.Clamp(point.z, 0f, vector.z - 1f);
		point.y = 0f;
		if (cell.HasValue)
		{
			targetInf.cellInt = cell.Value;
		}
		globalTargetInfo = targetInf;
		forcedTarget = new LocalTargetInfo(point.ToIntVec3());
		currentTargetInt = forcedTarget;
		TryStartShootSomething(canBeginBurstImmediately: false);
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		foreach (Gizmo gizmo in base.GetGizmos())
		{
			yield return gizmo;
		}
		if (CompAmmo != null && (PlayerControlled || Prefs.DevMode))
		{
			foreach (Command com in CompAmmo.CompGetGizmosExtra())
			{
				if (!PlayerControlled && Prefs.DevMode && com is GizmoAmmoStatus)
				{
					(com as GizmoAmmoStatus).prefix = "DEV: ";
				}
				yield return com;
			}
		}
		if (IsMortar && Active && base.Faction.IsPlayerSafe() && (compAmmo?.UseAmmo ?? false) && ProjectileProps?.shellingProps != null)
		{
			yield return new Command_ArtilleryTarget
			{
				defaultLabel = "CE_ArtilleryTargetLabel".Translate(),
				defaultDesc = "CE_ArtilleryTargetDesc".Translate(),
				turret = this,
				icon = ContentFinder<Texture2D>.Get("UI/Buttons/AttackWorldTile"),
				hotKey = KeyBindingDefOf.Misc5
			};
		}
		if (!PlayerControlled)
		{
			yield break;
		}
		if (CompFireModes != null)
		{
			foreach (Command item in CompFireModes.GenerateGizmos())
			{
				yield return item;
			}
		}
		if (CanSetForcedTarget)
		{
			Command_VerbTarget vt = new Command_VerbTarget
			{
				defaultLabel = "CommandSetForceAttackTarget".Translate(),
				defaultDesc = "CommandSetForceAttackTargetDesc".Translate(),
				icon = ContentFinder<Texture2D>.Get("UI/Commands/Attack"),
				verb = GunCompEq.PrimaryVerb,
				hotKey = KeyBindingDefOf.Misc4
			};
			if (base.Spawned && IsMortarOrProjectileFliesOverhead && base.Position.Roofed(base.Map))
			{
				vt.Disable("CannotFire".Translate() + ": " + "Roofed".Translate().CapitalizeFirst());
			}
			yield return vt;
		}
		if (forcedTarget.IsValid)
		{
			Command_Action stop = new Command_Action
			{
				defaultLabel = "CommandStopForceAttack".Translate(),
				defaultDesc = "CommandStopForceAttackDesc".Translate(),
				icon = ContentFinder<Texture2D>.Get("UI/Commands/Halt"),
				action = delegate
				{
					SyncedResetForcedTarget();
					SoundDefOf.Tick_Low.PlayOneShotOnCamera();
				}
			};
			if (!forcedTarget.IsValid)
			{
				stop.Disable("CommandStopAttackFailNotForceAttacking".Translate());
			}
			stop.hotKey = KeyBindingDefOf.Misc5;
			yield return stop;
		}
		if (CanToggleHoldFire)
		{
			yield return new Command_Toggle
			{
				defaultLabel = "CommandHoldFire".Translate(),
				defaultDesc = "CommandHoldFireDesc".Translate(),
				icon = ContentFinder<Texture2D>.Get("UI/Commands/HoldFire"),
				hotKey = KeyBindingDefOf.Misc6,
				toggleAction = ToggleHoldFire,
				isActive = () => holdFire
			};
		}
	}

	[Multiplayer.SyncMethod]
	private void ToggleHoldFire()
	{
		holdFire = !holdFire;
		if (holdFire)
		{
			ResetForcedTarget();
		}
	}

	[Multiplayer.SyncMethod]
	private void SyncedResetForcedTarget()
	{
		ResetForcedTarget();
	}

	public virtual void ResetForcedTarget()
	{
		targetingWorldMap = false;
		forcedTarget = LocalTargetInfo.Invalid;
		globalTargetInfo = GlobalTargetInfo.Invalid;
		burstWarmupTicksLeft = 0;
		if (burstCooldownTicksLeft <= 0)
		{
			TryStartShootSomething(canBeginBurstImmediately: false);
		}
	}

	public void ResetCurrentTarget()
	{
		currentTargetInt = LocalTargetInfo.Invalid;
		burstWarmupTicksLeft = 0;
	}

	private void InitGun()
	{
		if (CompAmmo != null)
		{
			CompAmmo.turret = this;
		}
		List<Verb> allVerbs = gunInt.TryGetComp<CompEquippable>().AllVerbs;
		for (int i = 0; i < allVerbs.Count; i++)
		{
			Verb verb = allVerbs[i];
			verb.caster = this;
			verb.castCompleteCallback = BurstComplete;
		}
	}

	public void TryForceReload()
	{
		TryOrderReload(forced: true);
	}

	public Thing InventoryAmmo(CompInventory inventory)
	{
		if (inventory == null)
		{
			return null;
		}
		Thing thing = inventory.container.FirstOrDefault((Thing x) => x.def == CompAmmo.SelectedAmmo);
		if (thing == null)
		{
			thing = inventory.container.FirstOrDefault((Thing x) => CompAmmo.Props.ammoSet.ammoTypes.Any((AmmoLink a) => a.ammo == x.def));
		}
		return thing;
	}

	public void TryOrderReload(bool forced = false)
	{
		if (compAmmo == null || (CompAmmo.CurrentAmmo == CompAmmo.SelectedAmmo && (!CompAmmo.HasMagazine || CompAmmo.CurMagCount == CompAmmo.MagSize)) || TryReloadViaAutoLoader())
		{
			return;
		}
		CompMannable compMannable = mannableComp;
		if (compMannable == null || !compMannable.MannedNow || (!forced && Reloadable && (compAmmo.CurMagCount != 0 || ticksUntilAutoReload > 0)))
		{
			return;
		}
		Pawn manningPawn = mannableComp.ManningPawn;
		if (manningPawn != null && JobGiverUtils_Reload.CanReload(manningPawn, this))
		{
			Job job = JobGiverUtils_Reload.MakeReloadJob(manningPawn, this);
			if (job != null)
			{
				manningPawn.jobs.StartJob(job, JobCondition.Ongoing, null, manningPawn.CurJob?.def != CE_JobDefOf.ReloadTurret, cancelBusyStances: true, null, null, fromQueue: false, canReturnCurJobToPool: false, null);
			}
		}
	}

	public bool TryReloadViaAutoLoader()
	{
		if (base.TargetCurrentlyAimingAt != null)
		{
			return false;
		}
		List<Thing> list = new List<Thing>();
		GenAdjFast.AdjacentThings8Way(this, list);
		foreach (Thing item in list)
		{
			if (item is Building_AutoloaderCE building_AutoloaderCE && building_AutoloaderCE.StartReload(compAmmo))
			{
				return true;
			}
		}
		return false;
	}
}
