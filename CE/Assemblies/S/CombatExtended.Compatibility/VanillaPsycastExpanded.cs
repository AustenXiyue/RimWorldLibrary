using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using VanillaPsycastsExpanded;
using Verse;

namespace CombatExtended.Compatibility;

public class VanillaPsycastExpanded : IPatch
{
	private const string ModName = "Vanilla Psycasts Expanded";

	private static Dictionary<Map, IEnumerable<IEnumerable<IntVec3>>> shieldZones;

	private static int shieldZonesCacheTick = -1;

	public bool CanInstall()
	{
		Log.Message("Vanilla Psycasts Expanded loaded: " + ModLister.HasActiveModWithName("Vanilla Psycasts Expanded"));
		return ModLister.HasActiveModWithName("Vanilla Psycasts Expanded");
	}

	public void Install()
	{
		BlockerRegistry.RegisterImpactSomethingCallback(ImpactSomething);
		BlockerRegistry.RegisterBeforeCollideWithCallback(BeforeCollideWith);
		BlockerRegistry.RegisterCheckForCollisionCallback(Hediff_Overshield_InterceptCheck);
		BlockerRegistry.RegisterCheckForCollisionBetweenCallback(AOE_CheckIntercept);
		BlockerRegistry.RegisterShieldZonesCallback(ShieldZones);
		BlockerRegistry.RegisterUnsuppresableFromCallback(Unsuppresable);
	}

	private static IEnumerable<IEnumerable<IntVec3>> ShieldZones(Thing thing)
	{
		IEnumerable<IEnumerable<IntVec3>> value = null;
		int ticksGame = GenTicks.TicksGame;
		if (shieldZonesCacheTick != ticksGame)
		{
			shieldZonesCacheTick = ticksGame;
			shieldZones = new Dictionary<Map, IEnumerable<IEnumerable<IntVec3>>>();
		}
		if (!shieldZones.TryGetValue(thing.Map, out value))
		{
			value = (from x in thing.Map.listerThings.ThingsInGroup(ThingRequestGroup.Pawn).Cast<Pawn>().SelectMany((Pawn x) => x.health.hediffSet.hediffs)
				where x is Hediff_Overshield
				select x).Select(delegate(Hediff x)
			{
				Hediff_Overshield val = (Hediff_Overshield)(object)((x is Hediff_Overshield) ? x : null);
				return GenRadial.RadialCellsAround(((Hediff)(object)val).pawn.Position, ((Hediff_Overlay)val).OverlaySize, useCenter: true);
			}).ToList();
			shieldZones.Add(thing.Map, value);
		}
		return value;
	}

	private static bool Unsuppresable(Pawn pawn, IntVec3 origin)
	{
		return pawn.health.hediffSet.hediffs.Any((Hediff x) => x.GetType() == typeof(Hediff_Overshield));
	}

	private static bool BeforeCollideWith(ProjectileCE projectile, Thing collideWith)
	{
		if (collideWith is Pawn pawn)
		{
			Hediff hediff = pawn.health.hediffSet.hediffs.FirstOrDefault((Hediff x) => x.GetType() == typeof(Hediff_Overshield));
			Hediff_Overshield val = (Hediff_Overshield)(object)((hediff is Hediff_Overshield) ? hediff : null);
			if (val != null)
			{
				OnIntercepted((Hediff)(object)val, projectile, null);
				return true;
			}
		}
		return false;
	}

	private static bool ImpactSomething(ProjectileCE projectile, Thing launcher)
	{
		return Hediff_Overshield_InterceptCheck(projectile, projectile.ExactPosition.ToIntVec3(), launcher);
	}

	public static bool Hediff_Overshield_InterceptCheck(ProjectileCE projectile, IntVec3 cell, Thing launcher)
	{
		foreach (Hediff_Overshield item in (from x in projectile.Map.thingGrid.ThingsListAt(cell).OfType<Pawn>().SelectMany((Pawn x) => x.health.hediffSet.hediffs)
			where x.GetType() == typeof(Hediff_Overshield)
			select x).Cast<Hediff_Overshield>())
		{
			ThingDef def = projectile.def;
			bool flag = ((Hediff)(object)item).pawn != launcher && ((Hediff)(object)item).pawn.Position == cell;
			if (flag)
			{
				OnIntercepted((Hediff)(object)item, projectile, null);
				return flag;
			}
		}
		return false;
	}

	public static bool AOE_CheckIntercept(ProjectileCE projectile, Vector3 from, Vector3 newExactPos)
	{
		ThingDef def = projectile.def;
		if (projectile.def.projectile.flyOverhead)
		{
			return false;
		}
		foreach (Hediff_Overshield item in (from x in projectile.Map.listerThings.ThingsInGroup(ThingRequestGroup.Pawn).Cast<Pawn>().SelectMany((Pawn x) => x.health.hediffSet.hediffs)
			where x is Hediff_Overshield && x.GetType() != typeof(Hediff_Overshield)
			select x).Cast<Hediff_Overshield>())
		{
			Vector3 center = ((Hediff)(object)item).pawn.Position.ToVector3Shifted().Yto0();
			float overlaySize = ((Hediff_Overlay)item).OverlaySize;
			if (CE_Utility.IntersectionPoint(from.Yto0(), newExactPos.Yto0(), center, overlaySize, out var sect, catchOutbound: false, spherical: false, projectile.Map))
			{
				OnIntercepted((Hediff)(object)item, projectile, sect);
				return true;
			}
		}
		return false;
	}

	private static void OnIntercepted(Hediff hediff, ProjectileCE projectile, Vector3[] sect)
	{
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		Hediff_Overshield val = (Hediff_Overshield)(object)((hediff is Hediff_Overshield) ? hediff : null);
		if (val != null)
		{
			Vector3 exactPosition = projectile.ExactPosition;
			if (sect == null)
			{
				CE_Utility.IntersectionPoint(projectile.OriginIV3.ToVector3(), projectile.ExactPosition, ((Hediff)(object)val).pawn.Position.ToVector3(), ((Hediff_Overlay)val).OverlaySize, out sect);
			}
			Vector3 vector = sect.OrderBy((Vector3 x) => (projectile.OriginIV3.ToVector3() - x).sqrMagnitude).First();
			projectile.ExactPosition = vector;
			new Traverse((object)val).Field("lastInterceptAngle").SetValue((object)vector.AngleToFlat(((Hediff)(object)val).pawn.TrueCenter()));
			new Traverse((object)val).Field("lastInterceptTicks").SetValue((object)Find.TickManager.TicksGame);
			new Traverse((object)val).Field("drawInterceptCone").SetValue((object)true);
			FleckMakerCE.ThrowPsycastShieldFleck(vector, projectile.Map, 0.35f);
			projectile.InterceptProjectile(val, projectile.ExactPosition, destroyCompletely: true);
		}
	}
}
