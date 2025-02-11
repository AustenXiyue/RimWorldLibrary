using System;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace CombatExtended;

public abstract class TravelingThing : WorldObject
{
	private int startingTile;

	private int destinationTile;

	private int distanceInTiles;

	private float tilesPerTick;

	private float distance;

	private float distanceTraveled;

	private Vector3? _start;

	private Vector3? _end;

	protected virtual Vector3 Start
	{
		get
		{
			Vector3 value;
			if (!_start.HasValue)
			{
				Vector3? vector = (_start = Find.WorldGrid.GetTileCenter(startingTile));
				value = vector.Value;
			}
			else
			{
				value = _start.Value;
			}
			return value;
		}
	}

	protected virtual Vector3 End
	{
		get
		{
			Vector3 value;
			if (!_end.HasValue)
			{
				Vector3? vector = (_end = Find.WorldGrid.GetTileCenter(destinationTile));
				value = vector.Value;
			}
			else
			{
				value = _end.Value;
			}
			return value;
		}
	}

	public virtual float TilesPerTick => 0.03f;

	public int StartTile => startingTile;

	public int DestinationTile => startingTile;

	public float TraveledPtc => distanceTraveled / (float)distanceInTiles;

	public override Vector3 DrawPos => Vector3.Slerp(Start, End, TraveledPtc);

	public TravelingThing()
	{
	}

	public virtual bool TryTravel(int startingTile, int destinationTile)
	{
		if (startingTile <= -1 || destinationTile <= -1 || startingTile == destinationTile)
		{
			Log.Warning($"CE: TryTravel in thing {this} got {startingTile} {destinationTile}");
			return false;
		}
		int num2 = (base.Tile = startingTile);
		this.startingTile = num2;
		this.destinationTile = destinationTile;
		tilesPerTick = TilesPerTick;
		Vector3 tileCenter = Find.WorldGrid.GetTileCenter(startingTile);
		Vector3 tileCenter2 = Find.WorldGrid.GetTileCenter(destinationTile);
		distance = GenMath.SphericalDistance(tileCenter.normalized, tileCenter2.normalized);
		distanceInTiles = (int)Find.World.grid.ApproxDistanceInTiles(distance);
		return true;
	}

	protected abstract void Arrived();

	public override void Tick()
	{
		try
		{
			base.Tick();
			distanceTraveled += tilesPerTick;
			if (TraveledPtc >= 1f)
			{
				base.Tile = destinationTile;
				Arrived();
				Destroy();
			}
		}
		catch (Exception arg)
		{
			Log.Error($"CE: TravelingThing {this} threw an exception {arg}");
			Log.Warning($"CE: TravelingThing {this} is being destroyed to prevent further errors");
			Destroy();
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref startingTile, "startingTile", 0);
		Scribe_Values.Look(ref destinationTile, "destinationTile", 0);
		Scribe_Values.Look(ref tilesPerTick, "tilesPerTick", 0f);
		Scribe_Values.Look(ref distanceInTiles, "distanceInTiles", 0);
		Scribe_Values.Look(ref distance, "distance", 0f);
		Scribe_Values.Look(ref distanceTraveled, "distanceTraveled", 0f);
	}
}
