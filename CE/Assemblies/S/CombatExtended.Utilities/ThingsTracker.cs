using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;

namespace CombatExtended.Utilities;

public class ThingsTracker : MapComponent
{
	private static bool[] validDefs;

	private static Map[] maps = new Map[20];

	private static ThingsTracker[] comps = new ThingsTracker[20];

	private ThingsTrackingModel[][] trackers;

	private ThingsTrackingModel pawnsTracker;

	private ThingsTrackingModel weaponsTracker;

	private ThingsTrackingModel apparelTracker;

	private ThingsTrackingModel ammoTracker;

	private ThingsTrackingModel medicineTracker;

	private ThingsTrackingModel flaresTracker;

	private ThingsTrackingModel attachmentTracker;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ThingsTracker GetTracker(Map map)
	{
		return (map != null) ? GetCachedTracker(map) : null;
	}

	private static ThingsTracker GetCachedTracker(Map map, bool fallbackMode = false)
	{
		int index = map.Index;
		if (index >= 0 && index < comps.Length)
		{
			ThingsTracker result = comps[index];
			if (maps[index] != map)
			{
				maps[index] = map;
				result = (comps[index] = map.GetComponent<ThingsTracker>());
			}
			return result;
		}
		if (fallbackMode)
		{
			throw new Exception($"GetTracker has failed twice for index {index}");
		}
		Map[] destinationArray = new Map[Math.Max(maps.Length * 2, index + 1)];
		ThingsTracker[] destinationArray2 = new ThingsTracker[Math.Max(comps.Length * 2, index + 1)];
		Array.Copy(maps, 0, destinationArray, 0, maps.Length);
		maps = destinationArray;
		Array.Copy(comps, 0, destinationArray2, 0, comps.Length);
		comps = destinationArray2;
		return GetCachedTracker(map, fallbackMode: true);
	}

	public ThingsTracker(Map map)
		: base(map)
	{
		pawnsTracker = new ThingsTrackingModel(null, map, this);
		weaponsTracker = new ThingsTrackingModel(null, map, this);
		ammoTracker = new ThingsTrackingModel(null, map, this);
		apparelTracker = new ThingsTrackingModel(null, map, this);
		medicineTracker = new ThingsTrackingModel(null, map, this);
		flaresTracker = new ThingsTrackingModel(null, map, this);
		attachmentTracker = new ThingsTrackingModel(null, map, this);
		trackers = new ThingsTrackingModel[DefDatabase<ThingDef>.AllDefs.Max((ThingDef def) => def.index) + 1][];
		for (int i = 0; i < trackers.Length; i++)
		{
			trackers[i] = new ThingsTrackingModel[2];
		}
		foreach (ThingDef item in DefDatabase<ThingDef>.AllDefs.Where((ThingDef d) => d.race != null || d.category == ThingCategory.Pawn))
		{
			trackers[item.index][1] = pawnsTracker;
		}
		foreach (ThingDef item2 in DefDatabase<ThingDef>.AllDefs.Where((ThingDef d) => d.IsWeapon))
		{
			trackers[item2.index][1] = weaponsTracker;
		}
		foreach (ThingDef item3 in DefDatabase<ThingDef>.AllDefs.Where((ThingDef d) => d.IsApparel))
		{
			trackers[item3.index][1] = apparelTracker;
		}
		foreach (ThingDef item4 in DefDatabase<ThingDef>.AllDefs.Where((ThingDef d) => d.thingClass == typeof(Flare)))
		{
			trackers[item4.index][1] = flaresTracker;
		}
		foreach (ThingDef item5 in DefDatabase<ThingDef>.AllDefs.Where((ThingDef d) => d.IsMedicine))
		{
			trackers[item5.index][1] = medicineTracker;
		}
		foreach (AmmoDef allDef in DefDatabase<AmmoDef>.AllDefs)
		{
			trackers[allDef.index][1] = ammoTracker;
		}
		foreach (AttachmentDef allDef2 in DefDatabase<AttachmentDef>.AllDefs)
		{
			trackers[allDef2.index][1] = attachmentTracker;
		}
		if (validDefs != null)
		{
			return;
		}
		validDefs = new bool[65535];
		foreach (ThingDef allDef3 in DefDatabase<ThingDef>.AllDefs)
		{
			if (allDef3.thingClass == typeof(Flare))
			{
				validDefs[allDef3.index] = true;
			}
			else if (allDef3.thingClass == typeof(WeaponPlatform))
			{
				validDefs[allDef3.index] = true;
			}
			else if (allDef3.category == ThingCategory.Mote)
			{
				validDefs[allDef3.index] = false;
			}
			else if (allDef3.category == ThingCategory.Filth)
			{
				validDefs[allDef3.index] = false;
			}
			else if (allDef3.category == ThingCategory.Building)
			{
				validDefs[allDef3.index] = false;
			}
			else if (allDef3.category == ThingCategory.Gas)
			{
				validDefs[allDef3.index] = false;
			}
			else if (allDef3.category == ThingCategory.Ethereal)
			{
				validDefs[allDef3.index] = false;
			}
			else if (allDef3.category == ThingCategory.Projectile)
			{
				validDefs[allDef3.index] = false;
			}
			else if (allDef3.category == ThingCategory.Plant)
			{
				validDefs[allDef3.index] = false;
			}
			else if (allDef3.category == ThingCategory.PsychicEmitter)
			{
				validDefs[allDef3.index] = false;
			}
			else if (allDef3.category == ThingCategory.Attachment)
			{
				validDefs[allDef3.index] = false;
			}
			else
			{
				validDefs[allDef3.index] = true;
			}
		}
		foreach (FleckDef allDef4 in DefDatabase<FleckDef>.AllDefs)
		{
			validDefs[allDef4.index] = false;
		}
	}

	public override void MapComponentOnGUI()
	{
		base.MapComponentOnGUI();
		if (!Controller.settings.DebugGenClosetPawn || Find.Selector.SelectedObjects.Count == 0)
		{
			return;
		}
		Thing thing = (from s in Find.Selector.SelectedObjects
			where s is Thing
			select s as Thing).First();
		if (!IsValidTrackableThing(thing))
		{
			return;
		}
		IEnumerable<Thing> enumerable = ((thing is Pawn) ? thing.Position.PawnsInRange(map, 50f) : ((thing is AmmoThing) ? thing.Position.AmmoInRange(map, 50f) : (thing.def.IsApparel ? thing.Position.ApparelInRange(map, 50f) : (thing.def.IsWeapon ? thing.Position.WeaponsInRange(map, 50f) : ((!(thing.def is AttachmentDef)) ? thing.SimilarInRange(50f) : thing.Position.AttachmentsInRange(map, 50f))))));
		Vector2 vector = thing.DrawPos.MapToUIPosition();
		int num = 0;
		foreach (Thing item in enumerable)
		{
			Vector2 vector2 = item.DrawPos.MapToUIPosition();
			Widgets.DrawLine(vector, vector2, Color.red, 1f);
			Vector2 vector3 = (vector + vector2) / 2f;
			Rect rect = new Rect(vector3.x - 25f, vector3.y - 15f, 50f, 30f);
			Widgets.DrawBox(rect);
			Widgets.Label(rect, $"({num++}). {item.Position.DistanceTo(thing.Position)}m");
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IEnumerable<Thing> SimilarInRangeOf(Thing thing, float range)
	{
		return ThingsInRangeOf(thing.def, thing.Position, range);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IEnumerable<Thing> ThingsNearSegment(TrackedThingsRequestCategory category, IntVec3 origin, IntVec3 destination, float range, bool behind = false, bool infront = true)
	{
		ThingsTrackingModel modelFor = GetModelFor(category);
		return modelFor.ThingsNearSegment(origin, destination, range, behind, infront);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IEnumerable<Thing> ThingsInRangeOf(TrackedThingsRequestCategory category, IntVec3 cell, float range)
	{
		ThingsTrackingModel modelFor = GetModelFor(category);
		return modelFor.ThingsInRangeOf(cell, range);
	}

	public IEnumerable<Thing> ThingsInRangeOf(ThingDef def, IntVec3 cell, float range)
	{
		if (!IsValidTrackableDef(def))
		{
			throw new NotSupportedException();
		}
		ThingsTrackingModel[] modelsFor = GetModelsFor(def);
		return modelsFor[0].ThingsInRangeOf(cell, range);
	}

	public void Register(Thing thing)
	{
		if (IsValidTrackableThing(thing))
		{
			ThingsTrackingModel[] modelsFor = GetModelsFor(thing);
			for (int i = 0; i < modelsFor.Length; i++)
			{
				modelsFor[i]?.Register(thing);
			}
		}
	}

	public void Remove(Thing thing)
	{
		if (IsValidTrackableThing(thing))
		{
			ThingsTrackingModel[] modelsFor = GetModelsFor(thing);
			for (int i = 0; i < modelsFor.Length; i++)
			{
				modelsFor[i]?.DeRegister(thing);
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ThingsTrackingModel[] GetModelsFor(Thing thing)
	{
		return GetModelsFor(thing.def);
	}

	public ThingsTrackingModel[] GetModelsFor(ThingDef def)
	{
		ThingsTrackingModel[] array = trackers[def.index];
		if (array[0] != null)
		{
			return array;
		}
		array[0] = new ThingsTrackingModel(def, map, this);
		return array;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ThingsTrackingModel GetModelFor(TrackedThingsRequestCategory category)
	{
		return category switch
		{
			TrackedThingsRequestCategory.Pawns => pawnsTracker, 
			TrackedThingsRequestCategory.Ammo => ammoTracker, 
			TrackedThingsRequestCategory.Apparel => apparelTracker, 
			TrackedThingsRequestCategory.Weapons => weaponsTracker, 
			TrackedThingsRequestCategory.Medicine => medicineTracker, 
			TrackedThingsRequestCategory.Flares => flaresTracker, 
			TrackedThingsRequestCategory.Attachments => attachmentTracker, 
			_ => throw new NotSupportedException(), 
		};
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsValidTrackableThing(Thing thing)
	{
		return IsValidTrackableDef(thing.def);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsValidTrackableDef(ThingDef def)
	{
		return def?.index >= 0 && def.index < validDefs.Length && validDefs[def.index];
	}

	public void Notify_Spawned(Thing thing)
	{
		if (IsValidTrackableThing(thing))
		{
			Register(thing);
		}
	}

	public void Notify_DeSpawned(Thing thing)
	{
		if (IsValidTrackableThing(thing))
		{
			Remove(thing);
		}
	}

	public void Notify_PositionChanged(Thing thing)
	{
		if (IsValidTrackableThing(thing))
		{
			ThingsTrackingModel[] modelsFor = GetModelsFor(thing.def);
			for (int i = 0; i < modelsFor.Length; i++)
			{
				modelsFor[i]?.Notify_ThingPositionChanged(thing);
			}
		}
	}
}
