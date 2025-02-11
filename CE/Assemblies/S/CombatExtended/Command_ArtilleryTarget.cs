using System.Collections.Generic;
using System.Linq;
using CombatExtended.WorldObjects;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace CombatExtended;

public class Command_ArtilleryTarget : Command
{
	public Building_TurretGunCE turret;

	public List<Command_ArtilleryTarget> others = null;

	public IEnumerable<Building_TurretGunCE> SelectedTurrets => others?.Select((Command_ArtilleryTarget o) => o.turret) ?? new List<Building_TurretGunCE> { turret };

	public override bool GroupsWith(Gizmo other)
	{
		return other is Command_ArtilleryTarget;
	}

	public override void MergeWith(Gizmo other)
	{
		Command_ArtilleryTarget item = other as Command_ArtilleryTarget;
		if (others == null)
		{
			others = new List<Command_ArtilleryTarget>();
			others.Add(this);
		}
		others.Add(item);
	}

	public override void ProcessInput(Event ev)
	{
		CameraJumper.TryJump(CameraJumper.GetWorldTarget(turret));
		Find.WorldSelector.ClearSelection();
		if (turret == null)
		{
			Log.Error("Command_ArtilleryTarget without turret");
			return;
		}
		if (turret.Active)
		{
			IEnumerable<Building_TurretGunCE> selectedTurrets = SelectedTurrets;
			if (selectedTurrets == null || !selectedTurrets.Any((Building_TurretGunCE t) => t.Destroyed || !t.Active || !t.def.building.IsMortar))
			{
				int turretTile = turret.Map.Tile;
				int radius = (int)turret.MaxWorldRange;
				Find.WorldTargeter.BeginTargeting(delegate(GlobalTargetInfo targetInfo)
				{
					IEnumerable<Building_TurretGunCE> turrets = SelectedTurrets;
					Map map = Find.World.worldObjects.MapParentAt(targetInfo.Tile)?.Map ?? null;
					if (map != null && map.mapPawns.AnyPawnBlockingMapRemoval)
					{
						IntVec3 invalid = IntVec3.Invalid;
						Find.WorldTargeter.StopTargeting();
						CameraJumper.TryJumpInternal(new IntVec3(map.Size.x / 2, 0, map.Size.z / 2), map, CameraJumper.MovementMode.Pan);
						Find.Targeter.BeginTargeting(new TargetingParameters
						{
							canTargetLocations = true,
							canTargetBuildings = true,
							canTargetHumans = true
						}, delegate(LocalTargetInfo target)
						{
							targetInfo.mapInt = map;
							targetInfo.tileInt = map.Tile;
							targetInfo.cellInt = target.cellInt;
							TryAttack(turrets, targetInfo, target);
						}, delegate(LocalTargetInfo target)
						{
							GenDraw.DrawTargetHighlight(target);
						}, delegate(LocalTargetInfo target)
						{
							RoofDef roofDef = map.roofGrid.RoofAt(target.Cell);
							if ((roofDef == null || roofDef == RoofDefOf.RoofConstructed) && target.Cell.GetFirstThing<ArtilleryMarker>(map) != null)
							{
								return true;
							}
							Messages.Message("CE_ArtilleryTarget_MustTargetMark".Translate(), MessageTypeDefOf.RejectInput);
							return false;
						});
						return false;
					}
					if (targetInfo.WorldObject.Destroyed || targetInfo.WorldObject is DestroyedSettlement || targetInfo.WorldObject.def == WorldObjectDefOf.DestroyedSettlement)
					{
						Messages.Message("CE_ArtilleryTarget_AlreadyDestroyed".Translate(), MessageTypeDefOf.CautionInput);
						return false;
					}
					if (targetInfo.WorldObject.Faction != null)
					{
						Faction faction = targetInfo.WorldObject.Faction;
						FactionRelation factionRelation = faction.RelationWith(turret.Faction, allowNull: true);
						if (factionRelation == null)
						{
							faction.TryMakeInitialRelationsWith(turret.Faction);
						}
						if (!faction.HostileTo(turret.Faction) && !faction.Hidden)
						{
							Find.WindowStack.Add(new Dialog_MessageBox("CE_ArtilleryTarget_AttackingAllies".Translate().Formatted(targetInfo.WorldObject.Label, faction.Name), "CE_Yes".Translate(), delegate
							{
								TryAttack(turrets, targetInfo, LocalTargetInfo.Invalid);
								Find.WorldTargeter.StopTargeting();
							}, "CE_No".Translate(), delegate
							{
								Find.WorldTargeter.StopTargeting();
							}, null, buttonADestructive: true));
							return false;
						}
					}
					return TryAttack(turrets, targetInfo, LocalTargetInfo.Invalid);
				}, canTargetTiles: true, null, closeWorldTabWhenFinished: true, delegate
				{
					if (others != null)
					{
						foreach (Building_TurretGunCE selectedTurret in SelectedTurrets)
						{
							if (selectedTurret.MaxWorldRange != (float)radius)
							{
								GenDraw.DrawWorldRadiusRing(turretTile, (int)selectedTurret.MaxWorldRange);
							}
						}
					}
					GenDraw.DrawWorldRadiusRing(turretTile, radius);
				}, delegate(GlobalTargetInfo targetInfo)
				{
					int num = Find.WorldGrid.TraversalDistanceBetween(turretTile, targetInfo.Tile);
					string text = null;
					if (others != null)
					{
						int num2 = 0;
						int num3 = 0;
						foreach (Building_TurretGunCE selectedTurret2 in SelectedTurrets)
						{
							num3++;
							if (selectedTurret2.MaxWorldRange >= (float)num)
							{
								num2++;
							}
						}
						text = "CE_ArtilleryTarget_Distance_Selections".Translate().Formatted(num, num2, num3);
					}
					else
					{
						text = "CE_ArtilleryTarget_Distance".Translate().Formatted(num, radius);
					}
					if (turret.MaxWorldRange > 0f && (float)num > turret.MaxWorldRange)
					{
						GUI.color = ColorLibrary.RedReadable;
						return text + "\n" + "CE_ArtilleryTarget_DestinationBeyondMaximumRange".Translate();
					}
					if (!targetInfo.HasWorldObject || targetInfo.WorldObject is Caravan)
					{
						GUI.color = ColorLibrary.RedReadable;
						return text + "\n" + "CE_ArtilleryTarget_InvalidTarget".Translate();
					}
					string text2 = "";
					if (targetInfo.WorldObject is Settlement settlement)
					{
						text2 = " " + settlement.Name;
						if (settlement.Faction != null && !settlement.Faction.name.NullOrEmpty())
						{
							text2 = text2 + " (" + settlement.Faction.name + ")";
						}
					}
					return text + "\n" + "CE_ArtilleryTarget_ClickToOrderAttack".Translate() + text2;
				}, (GlobalTargetInfo targetInfo) => (targetInfo.HasWorldObject && targetInfo.Tile != turretTile && ((targetInfo.WorldObject as MapParent)?.Map != null || targetInfo.WorldObject.GetComponent<HealthComp>() != null)) ? true : false);
				base.ProcessInput(ev);
				return;
			}
		}
		Log.Error("Command_ArtilleryTarget selected turrets collection is invalid");
	}

	private bool TryAttack(IEnumerable<Building_TurretGunCE> turrets, GlobalTargetInfo targetInfo, LocalTargetInfo localTargetInfo)
	{
		bool flag = false;
		foreach (Building_TurretGunCE turret in turrets)
		{
			if (turret.Active && turret.TryAttackWorldTarget(targetInfo, localTargetInfo))
			{
				flag = flag || true;
			}
		}
		return flag;
	}
}
