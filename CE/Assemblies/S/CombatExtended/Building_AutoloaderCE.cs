using System.Collections.Generic;
using System.Text;
using CombatExtended.Compatibility;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace CombatExtended;

[StaticConstructorOnStartup]
public class Building_AutoloaderCE : Building
{
	public CompAmmoUser CompAmmoUser;

	public bool shouldReplaceAmmo;

	public int ticksToCompleteInitial;

	public int ticksToComplete;

	public bool isReloading;

	public Building_Turret TargetTurret;

	private Sustainer reloadingSustainer;

	private static readonly Material UnfilledMat = SolidColorMaterials.NewSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f, 0.65f), ShaderDatabase.MetaOverlay);

	private static readonly Material FilledMat = SolidColorMaterials.NewSolidColorMaterial(new Color(0.9f, 0.85f, 0.2f, 0.65f), ShaderDatabase.MetaOverlay);

	public CompPowerTrader powerComp;

	public CompCanBeDormant dormantComp;

	public CompInitiatable initiatableComp;

	public CompMannable mannableComp;

	public ModExtension_AutoLoaderGraphics graphicsExt;

	public bool isActive => TargetAmmoUser != null;

	public CompAmmoUser TargetAmmoUser => TargetTurret.GetAmmo();

	public bool shouldBeOn => ShouldBeOn();

	public bool manningRequiredButUnmanned => mannableComp != null && !mannableComp.MannedNow;

	public bool powerRequiredButUnpowered => powerComp != null && !powerComp.PowerOn;

	public override Graphic Graphic
	{
		get
		{
			Graphic graphic = null;
			if (graphicsExt != null)
			{
				graphic = (((float)CompAmmoUser.CurMagCount > (float)CompAmmoUser.MagSize * 0.75f) ? graphicsExt.fullGraphic?.GraphicColoredFor(this) : ((!CompAmmoUser.EmptyMagazine) ? graphicsExt.halfFullGraphic?.GraphicColoredFor(this) : graphicsExt.emptyGraphic?.GraphicColoredFor(this)));
			}
			return graphic ?? base.Graphic;
		}
	}

	public bool ShouldBeOn(bool failureNotify = false)
	{
		if (manningRequiredButUnmanned)
		{
			if (failureNotify)
			{
				Messages.Message(string.Format("CE_AutoLoader_Unmanned".Translate(), Label), this, MessageTypeDefOf.RejectInput, historical: false);
			}
			return false;
		}
		if (powerRequiredButUnpowered)
		{
			if (failureNotify)
			{
				Messages.Message(string.Format("CE_AutoLoader_Unpowered".Translate(), Label), this, MessageTypeDefOf.RejectInput, historical: false);
			}
			return false;
		}
		return (dormantComp == null || dormantComp.Awake) && (initiatableComp == null || initiatableComp.Initiated);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref shouldReplaceAmmo, "shouldReplaceAmmo", defaultValue: false);
		Scribe_Values.Look(ref ticksToCompleteInitial, "ticksToCompleteInitial", 0);
		Scribe_Values.Look(ref ticksToComplete, "ticksToComplete", 0);
		Scribe_Values.Look(ref isReloading, "isReloading", defaultValue: false);
		Scribe_References.Look(ref TargetTurret, "Turret");
	}

	public bool CanReplaceAmmo(CompAmmoUser ammoUser)
	{
		return shouldReplaceAmmo && ammoUser.Props.ammoSet == CompAmmoUser.Props.ammoSet && ammoUser.CurrentAmmo != CompAmmoUser.CurrentAmmo;
	}

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		base.SpawnSetup(map, respawningAfterLoad);
		base.Map.GetComponent<AutoLoaderTracker>().Register(this);
		CompAmmoUser = GetComp<CompAmmoUser>();
		dormantComp = GetComp<CompCanBeDormant>();
		initiatableComp = GetComp<CompInitiatable>();
		powerComp = GetComp<CompPowerTrader>();
		mannableComp = GetComp<CompMannable>();
		graphicsExt = def.GetModExtension<ModExtension_AutoLoaderGraphics>();
		if (CompAmmoUser == null)
		{
			Log.Error(GetCustomLabelNoCount() + " Requires CompAmmoUser to function properly.");
		}
		if (def.tickerType != TickerType.Normal)
		{
			Log.Error(GetCustomLabelNoCount() + " Requires normal ticker type to function properly.");
		}
		if (def.drawerType != DrawerType.MapMeshAndRealTime)
		{
			Log.Error(GetCustomLabelNoCount() + " Requires MapMeshAndRealTime drawer type to display progress bar.");
		}
	}

	public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
	{
		TargetTurret?.SetReloading(reloading: false);
		base.Map.GetComponent<AutoLoaderTracker>().Unregister(this);
		DropAmmo(mode == DestroyMode.KillFinalize);
		base.DeSpawn(mode);
	}

	public void DropAmmo(bool forcibly = false)
	{
		if (!CompAmmoUser.EmptyMagazine)
		{
			Thing thing = ThingMaker.MakeThing(CompAmmoUser.CurrentAmmo);
			thing.stackCount = CompAmmoUser.CurMagCount;
			CompAmmoUser.CurMagCount = 0;
			if (!GenThing.TryDropAndSetForbidden(thing, base.Position, base.Map, ThingPlaceMode.Near, out var _, base.Faction != Faction.OfPlayer))
			{
				Log.Warning(string.Concat(GetType().Assembly.GetName().Name + " :: " + GetType().Name + " :: ", "Unable to drop ", thing.LabelCap, " on the ground, thing was destroyed."));
			}
			if (forcibly)
			{
				DamageInfo dinfo = new DamageInfo(DamageDefOf.Bullet, Rand.Range(0, 100));
				thing.TakeDamage(dinfo);
			}
			Notify_ColorChanged();
		}
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		foreach (Gizmo gizmo in base.GetGizmos())
		{
			yield return gizmo;
		}
		if (CompAmmoUser == null || (base.Faction != Faction.OfPlayer && !Prefs.DevMode))
		{
			yield break;
		}
		yield return new GizmoAmmoStatus
		{
			compAmmo = CompAmmoUser
		};
		yield return new Command_Reload
		{
			compAmmo = CompAmmoUser,
			action = null,
			defaultLabel = (CompAmmoUser.HasMagazine ? ((string)"CE_ReloadLabel".Translate()) : ""),
			defaultDesc = "CE_ReloadDesc".Translate(),
			icon = ((CompAmmoUser.CurrentAmmo == null) ? ContentFinder<Texture2D>.Get("UI/Buttons/Reload") : CompAmmoUser.SelectedAmmo.IconTexture()),
			tutorTag = "CE_AutoLoader"
		};
		if (DebugSettings.godMode)
		{
			yield return new Command_Action
			{
				action = delegate
				{
					CompAmmoUser.CurMagCount = 0;
					Notify_ColorChanged();
				},
				defaultLabel = "DEV: Set ammo to 0"
			};
			yield return new Command_Action
			{
				action = delegate
				{
					CompAmmoUser.CurrentAmmo = CompAmmoUser.SelectedAmmo;
					CompAmmoUser.CurMagCount = CompAmmoUser.MagSize;
					Notify_ColorChanged();
				},
				defaultLabel = "DEV: Set ammo to max"
			};
			yield return new Command_Action
			{
				action = delegate
				{
					CompAmmoUser.CurrentAmmo = CompAmmoUser.SelectedAmmo;
					CompAmmoUser.CurMagCount++;
					Notify_ColorChanged();
				},
				defaultLabel = "DEV: Ammo +1"
			};
		}
		if (!CompAmmoUser.EmptyMagazine)
		{
			yield return new Command_Action
			{
				defaultLabel = "CE_AutoLoader_DropAmmo".Translate(),
				defaultDesc = "CE_AutoLoader_DropAmmoDesc".Translate(),
				icon = ContentFinder<Texture2D>.Get("UI/Buttons/CE_AutoLoader_Drop"),
				action = delegate
				{
					DropAmmo();
				}
			};
			yield return new Command_Action
			{
				defaultLabel = "CE_AutoLoader_ForceReload".Translate(),
				defaultDesc = "CE_AutoLoader_ForceReloadDesc".Translate(),
				icon = ContentFinder<Texture2D>.Get("UI/Buttons/CE_AutoLoader_Reload"),
				action = delegate
				{
					List<Thing> list = new List<Thing>();
					GenAdjFast.AdjacentThings8Way(this, list);
					bool flag = false;
					if (ShouldBeOn(failureNotify: true))
					{
						foreach (Thing item in list)
						{
							if (item is Building_TurretGunCE turret && StartReload(turret.GetAmmo()))
							{
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							Messages.Message(string.Format("CE_AutoLoader_NoTurretToReload".Translate(), Label, CompAmmoUser.Props.ammoSet.label), this, MessageTypeDefOf.RejectInput, historical: false);
						}
					}
				}
			};
		}
		yield return new Command_Action
		{
			defaultLabel = (shouldReplaceAmmo ? "CE_AutoLoader_ToggleReplaceOn".Translate() : "CE_AutoLoader_ToggleReplaceOff".Translate()),
			defaultDesc = (shouldReplaceAmmo ? "CE_AutoLoader_ToggleReplaceDescOn".Translate() : "CE_AutoLoader_ToggleReplaceDescOff".Translate()),
			icon = (shouldReplaceAmmo ? ContentFinder<Texture2D>.Get("UI/Buttons/CE_AutoLoader_ReplaceOn") : ContentFinder<Texture2D>.Get("UI/Buttons/CE_AutoLoader_ReplaceOff")),
			action = delegate
			{
				shouldReplaceAmmo = !shouldReplaceAmmo;
			}
		};
	}

	public override void Tick()
	{
		base.Tick();
		if (isActive && shouldBeOn)
		{
			if (!TargetTurret.Spawned || TargetTurret.IsForbidden(base.Faction) || CompAmmoUser.EmptyMagazine)
			{
				TargetTurret?.SetReloading(reloading: false);
				TargetTurret = null;
				ticksToCompleteInitial = 0;
			}
			ticksToComplete--;
			if (reloadingSustainer == null)
			{
				reloadingSustainer = (graphicsExt?.reloadingSustainer ?? CE_SoundDefOf.CE_AutoLoaderAmbient).TrySpawnSustainer(SoundInfo.InMap(this));
			}
			reloadingSustainer.Maintain();
			if (ticksToComplete == 0)
			{
				ticksToCompleteInitial = 0;
				(graphicsExt?.reloadCompleteSound ?? TargetAmmoUser.parent.def.soundInteract).PlayOneShot(this);
				ReloadFinalize();
				Notify_ColorChanged();
			}
		}
		else if (reloadingSustainer != null)
		{
			reloadingSustainer.End();
			reloadingSustainer = null;
		}
	}

	public override void DrawAt(Vector3 drawLoc, bool flip = false)
	{
		base.DrawAt(drawLoc, flip);
		if (isActive)
		{
			GenDraw.FillableBarRequest r = default(GenDraw.FillableBarRequest);
			r.center = DrawPos + Vector3.up * 0.1f;
			r.size = new Vector2(1f, 0.14f);
			r.fillPercent = 1f - (float)ticksToComplete / (float)ticksToCompleteInitial;
			r.filledMat = FilledMat;
			r.unfilledMat = UnfilledMat;
			r.margin = 0.12f;
			GenDraw.DrawFillableBar(r);
		}
	}

	public override string GetInspectString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		string inspectString = base.GetInspectString();
		if (!inspectString.NullOrEmpty())
		{
			stringBuilder.AppendLine(inspectString);
		}
		if (manningRequiredButUnmanned)
		{
			stringBuilder.AppendLine("CE_AutoLoader_ManningRequired".Translate());
		}
		if (isActive)
		{
			stringBuilder.AppendLine("CE_AutoLoader_ReloadTime".Translate(ticksToComplete.TicksToSeconds().ToString("F0")));
		}
		stringBuilder.AppendLine("CE_AutoLoader_ShouldReplace".Translate(shouldReplaceAmmo.ToString()));
		return stringBuilder.ToString().TrimEndNewlines();
	}

	public bool TryActiveReload()
	{
		List<Thing> list = new List<Thing>();
		GenAdjFast.AdjacentThings8Way(this, list);
		foreach (Thing item in list)
		{
			if (item is Building_TurretGunCE building_TurretGunCE && (building_TurretGunCE.GetAmmo().EmptyMagazine || building_TurretGunCE.currentTargetInt == LocalTargetInfo.Invalid) && StartReload(building_TurretGunCE.GetAmmo()))
			{
				return true;
			}
		}
		return false;
	}

	public bool StartReload(CompAmmoUser TurretMagazine, bool continued = false)
	{
		Building_Turret turret = TurretMagazine.turret;
		if (!turret.Spawned || turret.IsForbidden(base.Faction) || turret.GetReloading())
		{
			return false;
		}
		if ((isActive && !continued) || CompAmmoUser.EmptyMagazine || !shouldBeOn)
		{
			return false;
		}
		if (graphicsExt != null)
		{
			bool flag = graphicsExt.allowedTurrets.Any() && graphicsExt.allowedTurrets.Contains(turret.def.defName);
			if (!flag && graphicsExt.allowedTurretTags.Any())
			{
				foreach (string allowedTurretTag in graphicsExt.allowedTurretTags)
				{
					if (turret.def.building.buildingTags.NotNullAndContains(allowedTurretTag))
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		bool flag2 = CanReplaceAmmo(TurretMagazine);
		if (TurretMagazine.FullMagazine && !flag2)
		{
			return false;
		}
		if (TurretMagazine.CurrentAmmo == CompAmmoUser.CurrentAmmo || flag2)
		{
			TargetTurret = TurretMagazine.turret;
			ticksToComplete = Mathf.CeilToInt((float)TurretMagazine.Props.reloadTime.SecondsToTicks() / this.GetStatValue(CE_StatDefOf.ReloadSpeed));
			ticksToCompleteInitial = ticksToComplete;
			turret.SetReloading(reloading: true);
			return true;
		}
		return false;
	}

	public bool ReloadFinalize()
	{
		if (TargetAmmoUser.CurrentAmmo != CompAmmoUser.CurrentAmmo)
		{
			TargetAmmoUser.TryUnload();
			TargetAmmoUser.SelectedAmmo = CompAmmoUser.CurrentAmmo;
			TargetAmmoUser.CurrentAmmo = CompAmmoUser.CurrentAmmo;
		}
		if (TargetAmmoUser.Props.reloadOneAtATime)
		{
			TargetAmmoUser.CurMagCount++;
			CompAmmoUser.CurMagCount--;
			if (StartReload(TargetAmmoUser, continued: true))
			{
				return true;
			}
		}
		else
		{
			int num = Mathf.Min(CompAmmoUser.CurMagCount, TargetAmmoUser.MissingToFullMagazine);
			TargetAmmoUser.CurMagCount += num;
			CompAmmoUser.CurMagCount -= num;
		}
		TargetTurret.SetReloading(reloading: false);
		TargetTurret = null;
		TryActiveReload();
		return true;
	}
}
