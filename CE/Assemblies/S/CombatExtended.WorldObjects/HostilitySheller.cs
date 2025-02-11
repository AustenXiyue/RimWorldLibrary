using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace CombatExtended.WorldObjects;

public class HostilitySheller : IExposable
{
	private const float SHELLING_FACTOR = 3.5f;

	public const int SHELLER_MINCOOLDOWNTICKS = 300;

	public const int SHELLER_MAXCOOLDOWNTICKS = 1200;

	public const int SHELLER_MAXCOOLDOWNTICKS_TECHMULMAX = 3;

	public const int SHELLER_MESSAGE_TICKSCOOLDOWN = 5000;

	public const int SHELLER_EXPIRYTICKS = 15000;

	public const int SHELLER_MIN_TICKSBETWEENSHOTS = 30;

	public const int SHELLER_MAX_TICKSBETWEENSHOTS = 240;

	public const int SHELLER_MIN_PROJECTILEPOINTS = 100;

	private int lastMessageSentAt = -1;

	private int startedAt;

	private int ticksToNextShot = 0;

	private int shotsFired = 0;

	private int budget = 0;

	private Pawn shooter;

	private Faction targetFaction;

	private GlobalTargetInfo target;

	private int cooldownTicks = -1;

	public HostilityComp comp;

	public virtual bool AbleToShellResponse
	{
		get
		{
			if (comp.AvailableProjectiles.NullOrEmpty())
			{
				return false;
			}
			bool? flag = null;
			if (comp.parent is Site site)
			{
				foreach (SitePart part in site.parts)
				{
					flag = part.def.GetModExtension<WorldObjectHostilityExtension>()?.AbleToShellingResponse;
					if (flag.HasValue)
					{
						return flag.Value;
					}
				}
			}
			flag = comp.parent.Faction?.def.GetModExtension<WorldObjectHostilityExtension>()?.AbleToShellingResponse;
			if (flag.HasValue)
			{
				return flag.Value;
			}
			flag = (comp.props as WorldObjectCompProperties_Hostility).AbleToShellingResponse;
			if (flag.HasValue)
			{
				return flag.Value;
			}
			return true;
		}
	}

	public virtual List<ShellingResponseDef.ShellingResponsePart_Projectile> AvailableProjectiles => comp.AvailableProjectiles;

	public virtual bool Shooting => budget > 0 && GenTicks.TicksGame - startedAt < 15000;

	public virtual void ExposeData()
	{
		Scribe_Values.Look(ref budget, "budget", 0);
		Scribe_Values.Look(ref startedAt, "startedAt", -1);
		Scribe_Values.Look(ref shotsFired, "shotsFired", 0);
		Scribe_Values.Look(ref ticksToNextShot, "ticksToNextShot", 0);
		Scribe_References.Look(ref targetFaction, "targetFaction");
		Scribe_Values.Look(ref cooldownTicks, "cooldownTicks", 0);
		Scribe_TargetInfo.Look(ref target, "target");
	}

	public virtual void ThrottledTick()
	{
		if (cooldownTicks > 0)
		{
			cooldownTicks -= 15;
		}
		else if (Shooting)
		{
			if (ticksToNextShot > 0)
			{
				ticksToNextShot -= 15;
			}
			else if (!(comp.parent is MapParent { HasMap: not false }))
			{
				CastShot();
			}
		}
	}

	public bool TryStartShelling(GlobalTargetInfo targetInfo, float points, Faction targetFaction = null)
	{
		if (Shooting || targetInfo.Tile < 0 || points <= 0f || !AbleToShellResponse)
		{
			return false;
		}
		budget = (int)((float)Mathf.CeilToInt(points) * 3.5f);
		FactionStrengthTracker strengthTracker = comp.parent.Faction.GetStrengthTracker();
		if (strengthTracker != null)
		{
			budget = (int)((float)budget * Mathf.Max(strengthTracker.StrengthPointsMultiplier, 0.5f));
		}
		if (RandomAvailableShell(targetInfo) == null)
		{
			Stop();
			return false;
		}
		if (targetFaction != null && targetFaction.IsPlayer)
		{
			TrySendWarning();
		}
		this.targetFaction = targetFaction;
		if (comp.parent.Faction.def.humanlikeFaction)
		{
			shooter = comp.parent.Faction.GetRandomWorldPawn();
			if (shooter == null)
			{
				Log.Error($"CE: shooter for HostilitySheller from {comp.parent} is null");
			}
		}
		target = targetInfo;
		ticksToNextShot = GetTicksToCooldown();
		startedAt = GenTicks.TicksGame;
		shotsFired = 0;
		return true;
	}

	public void Stop()
	{
		shotsFired = 0;
		ticksToNextShot = 0;
		startedAt = -1;
		budget = -1;
		target = GlobalTargetInfo.Invalid;
		shooter = null;
	}

	protected virtual void CastShot()
	{
		ShellingResponseDef.ShellingResponsePart_Projectile shellingResponsePart_Projectile = RandomAvailableShell(target);
		shotsFired++;
		if (shellingResponsePart_Projectile == null)
		{
			Stop();
			cooldownTicks = GetTicksToCooldown();
			return;
		}
		budget -= (int)shellingResponsePart_Projectile.points;
		LaunchProjectile(shellingResponsePart_Projectile.projectile);
		if ((float)budget <= AvailableProjectiles.Min((ShellingResponseDef.ShellingResponsePart_Projectile p) => p.points))
		{
			Stop();
			cooldownTicks = GetTicksToCooldown();
			return;
		}
		ticksToNextShot = GetTicksToShot();
		if (targetFaction != null && targetFaction.IsPlayer)
		{
			TrySendWarning();
		}
	}

	private void LaunchProjectile(ThingDef projectileDef)
	{
		TravelingShell travelingShell = (TravelingShell)WorldObjectMaker.MakeWorldObject(CE_WorldObjectDefOf.TravelingShell);
		if (comp.parent.Faction != null)
		{
			travelingShell.SetFaction(comp.parent.Faction);
		}
		((WorldObject)travelingShell).tileInt = comp.parent.Tile;
		travelingShell.SpawnSetup();
		Find.World.worldObjects.Add(travelingShell);
		if (shooter == null && comp.parent.Faction.def.humanlikeFaction)
		{
			shooter = comp.parent.Faction.GetRandomWorldPawn();
		}
		travelingShell.launcher = shooter;
		travelingShell.equipmentDef = null;
		travelingShell.globalSource = new GlobalTargetInfo(comp.parent);
		travelingShell.globalSource.worldObjectInt = comp.parent;
		travelingShell.shellDef = projectileDef;
		travelingShell.globalTarget = target;
		if (travelingShell.launcher == null)
		{
			Log.Warning($"CE: Launcher of shell {projectileDef} is null, this may cause targeting issues");
		}
		if (!travelingShell.TryTravel(comp.parent.Tile, target.Tile))
		{
			Stop();
			Log.Error($"CE: Travling shell {projectileDef} failed to launch!");
			travelingShell.Destroy();
		}
	}

	private int GetPointsTotalShots(int points)
	{
		float num = (float)points / AvailableProjectiles.Min((ShellingResponseDef.ShellingResponsePart_Projectile p) => p.points);
		float num2 = AvailableProjectiles.Sum((ShellingResponseDef.ShellingResponsePart_Projectile p) => (float)points / p.points) / (float)AvailableProjectiles.Count;
		if (num2 - num + 100f < 0f)
		{
			num2 = num;
			num = num / 2f + 1f;
		}
		return Rand.Range((int)num, (int)num2);
	}

	protected virtual void TrySendWarning()
	{
		if (GenTicks.TicksGame - lastMessageSentAt > 5000 || lastMessageSentAt == -1)
		{
			lastMessageSentAt = GenTicks.TicksGame;
			ChoiceLetter let = LetterMaker.MakeLetter("CE_CounterShellingLabel".Translate(), "CE_Message_CounterShelling".Translate(comp.parent.Label, comp.parent.Faction.Name), CE_LetterDefOf.CE_ThreatBig, comp.parent, comp.parent.Faction);
			Find.LetterStack.ReceiveLetter(let);
		}
	}

	private ShellingResponseDef.ShellingResponsePart_Projectile RandomAvailableShell(GlobalTargetInfo target)
	{
		return AvailableProjectiles.Where((ShellingResponseDef.ShellingResponsePart_Projectile p) => (float)budget - p.points > 0f && p.projectile.projectile is ProjectilePropertiesCE projectilePropertiesCE && projectilePropertiesCE.shellingProps.range >= (float)Find.WorldGrid.TraversalDistanceBetween(target.Tile, comp.parent.Tile) * 0.5f).RandomElementByWeightWithFallback((ShellingResponseDef.ShellingResponsePart_Projectile p) => p.weight);
	}

	private int GetTicksToCooldown()
	{
		return Rand.Range(300, Mathf.Clamp((int)(7 - comp.parent.Faction.def.techLevel), 1, 3) * 1200) * HealthMultiplier();
	}

	private int GetTicksToShot()
	{
		return Rand.Range(30, 240) * HealthMultiplier();
	}

	private int HealthMultiplier()
	{
		FloatRange retaliationShellingCooldownImpact = comp.parent.Faction.GetShellingResponseDef().retaliationShellingCooldownImpact;
		float num = comp.parent.GetComponent<HealthComp>()?.Health ?? 1f;
		return Mathf.FloorToInt(retaliationShellingCooldownImpact.LerpThroughRange(1f - num));
	}
}
