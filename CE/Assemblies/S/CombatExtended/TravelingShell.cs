using System.Text;
using CombatExtended.WorldObjects;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace CombatExtended;

public class TravelingShell : TravelingThing
{
	public GlobalTargetInfo globalTarget;

	public GlobalTargetInfo globalSource;

	public ThingDef equipmentDef;

	public ThingDef shellDef;

	public Thing launcher;

	private Texture2D expandingIcon;

	public override Texture2D ExpandingIcon
	{
		get
		{
			if (expandingIcon == null)
			{
				string text = (shellDef?.projectile as ProjectilePropertiesCE)?.shellingProps?.iconPath;
				expandingIcon = (text.NullOrEmpty() ? base.ExpandingIcon : ContentFinder<Texture2D>.Get(text));
			}
			return expandingIcon;
		}
	}

	public override string Label => shellDef.label;

	public override float TilesPerTick => (shellDef.projectile as ProjectilePropertiesCE).shellingProps.tilesPerTick;

	public override bool ExpandingIconFlipHorizontal => GenWorldUI.WorldToUIPosition(Start).x > GenWorldUI.WorldToUIPosition(End).x;

	public override float ExpandingIconRotation
	{
		get
		{
			Vector2 vector = GenWorldUI.WorldToUIPosition(Start);
			Vector2 vector2 = GenWorldUI.WorldToUIPosition(End);
			float num = Mathf.Atan2(vector2.y - vector.y, vector2.x - vector.x) * 57.29578f;
			if (num > 180f)
			{
				num -= 180f;
			}
			return num + 90f;
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref equipmentDef, "equipmentDef");
		Scribe_Defs.Look(ref shellDef, "ShellDef");
		Scribe_TargetInfo.Look(ref globalTarget, "globalTarget");
		Scribe_TargetInfo.Look(ref globalSource, "globalSource");
		CE_Scriber.Late(this, delegate(string id)
		{
			Scribe_References.Look(ref launcher, "launcher" + id);
		});
	}

	public override string GetDescription()
	{
		MapParent mapParent = Find.WorldObjects.MapParentAt(globalSource.Tile);
		MapParent mapParent2 = Find.WorldObjects.MapParentAt(globalTarget.Tile);
		StringBuilder stringBuilder = new StringBuilder();
		if (shellDef != null)
		{
			stringBuilder.Append(shellDef.description);
		}
		string format = "CE_TravelingShell_Description".Translate();
		string text = mapParent?.Faction?.NameColored ?? ((TaggedString)"gray");
		string text2 = mapParent2?.Faction?.NameColored ?? ((TaggedString)"gray");
		stringBuilder.AppendFormat(format, "<color=" + text + ">" + (mapParent?.Label ?? $"{globalSource.Tile}") + "</color>", mapParent?.Faction?.Name ?? "Unknown", "<color=" + text + ">" + (mapParent2?.Label ?? $"{globalTarget.Tile}") + "</color>", mapParent2?.Faction?.Name ?? "Unknown");
		return stringBuilder.ToString();
	}

	protected override void Arrived()
	{
		int tile = base.Tile;
		foreach (WorldObject item in Find.World.worldObjects.ObjectsAt(tile))
		{
			if (TryShell(item))
			{
				break;
			}
		}
	}

	private bool TryShell(WorldObject worldObject)
	{
		bool flag = false;
		if (worldObject is MapParent { HasMap: not false } mapParent && Find.Maps.Contains(mapParent.Map))
		{
			flag = true;
			Map map = mapParent.Map;
			IntVec3 intVec = globalTarget.Cell;
			if (!globalTarget.Cell.IsValid || !globalTarget.Cell.InBounds(map))
			{
				intVec = FindRandomImpactCell(map);
			}
			Vector3 normalized = (Find.WorldGrid.GetTileCenter(globalSource.Tile) - Find.WorldGrid.GetTileCenter(globalTarget.Tile)).normalized;
			Vector3 vector = map.Size.ToVector3();
			vector.y = Mathf.Max(vector.x, vector.z);
			Ray ray = new Ray(intVec.ToVector3(), -1f * normalized);
			new Bounds((vector / 2f).Yto0(), vector).IntersectRay(ray, out var num);
			IntVec3 sourceCell = ray.GetPoint(num * 0.75f).ToIntVec3();
			LaunchProjectile(sourceCell, intVec, map, 55f);
		}
		HostilityComp component = worldObject.GetComponent<HostilityComp>();
		HealthComp component2 = worldObject.GetComponent<HealthComp>();
		if (worldObject.Faction != Faction.OfPlayer && component != null && component2 != null)
		{
			if (worldObject.Faction != null)
			{
				component.TryHostilityResponse(base.Faction, new GlobalTargetInfo(base.StartTile));
			}
			if (!flag)
			{
				flag = true;
				component2.ApplyDamage(shellDef, base.Faction, globalSource);
			}
		}
		return flag;
	}

	private void LaunchProjectile(IntVec3 sourceCell, LocalTargetInfo target, Map map, float shotSpeed = 20f, float shotHeight = 200f)
	{
		IntVec3 cell = target.Cell;
		Vector2 vector = new Vector2(sourceCell.x, sourceCell.z);
		Vector2 vector2 = new Vector2(cell.x, cell.z);
		Vector2 vector3 = vector2 - vector;
		ProjectileCE projectileCE = (ProjectileCE)ThingMaker.MakeThing(shellDef);
		ProjectilePropertiesCE projectilePropertiesCE = projectileCE.def.projectile as ProjectilePropertiesCE;
		float shotRotation = (-90f + 57.29578f * Mathf.Atan2(vector3.y, vector3.x)) % 360f;
		float shotAngle = ProjectileCE.GetShotAngle(shotSpeed, (vector2 - vector).magnitude, 0f - shotHeight, flyOverhead: false, projectilePropertiesCE.Gravity);
		projectileCE.canTargetSelf = false;
		projectileCE.Position = sourceCell;
		projectileCE.SpawnSetup(map, respawningAfterLoad: false);
		projectileCE.Launch(launcher, vector, shotAngle, shotRotation, shotHeight, shotSpeed);
	}

	private IntVec3 FindRandomImpactCell(Map map)
	{
		return ShellingUtility.FindRandomImpactCell(map, shellDef);
	}
}
