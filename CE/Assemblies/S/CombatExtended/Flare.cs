using CombatExtended.Compatibility;
using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended;

public class Flare : ThingWithComps
{
	public enum FlareDrawMode
	{
		Unknown,
		FlyOver,
		Direct
	}

	public const float BURN_TICKS = 2100f;

	public const float ALTITUDE_DRAW_FACTOR = 0.7f;

	public const float DEFAULT_FLYOVER_START_ALT = 30f;

	public const float DEFAULT_FLYOVER_FINAL_ALT = 4f;

	public const float DEFAULT_DIRECT_ALT = 0f;

	public const float LANDGLOW_MIN_ALTITUDE = 20f;

	public const float LANDGLOW_SCALE = 4f;

	public const float WATERSPLASH_MIN_ALTITUDE = 6f;

	public const float WATERSPLASH_VELOCITY = 5f;

	public const float WATERSPLASH_SIZE = 2.5f;

	private const int SMOKE_MIN_INTERVAL = 7;

	private const int SMOKE_MAX_INTERVAL = 21;

	private const float SMOKE_MIN_SIZE = 0.15f;

	private const float SMOKE_MAX_SIZE = 1.25f;

	private float _startingAltitude = -1f;

	private float _finalAltitude = -1f;

	private FlareDrawMode _drawMode = FlareDrawMode.Unknown;

	private Vector3 _directDrawPos = new Vector3(0f, 0f, -1f);

	private int spawnTick = -1;

	private int nextSmokePuff = -1;

	private static readonly Vector3 _moteDrawOffset = new Vector3(0f, 0f, -0.5f);

	public float StartingAltitude
	{
		get
		{
			if (_startingAltitude == -1f)
			{
				Log.Error("CE: Called StartingAltitude_get before setting a DrawMode");
				_ = DrawMode;
			}
			return _startingAltitude;
		}
		set
		{
			_startingAltitude = value;
		}
	}

	public float FinalAltitude
	{
		get
		{
			if (_finalAltitude == -1f)
			{
				Log.Error("CE: Called FinalAltitude_get before setting a DrawMode");
				_ = DrawMode;
			}
			return _finalAltitude;
		}
		set
		{
			_finalAltitude = value;
		}
	}

	public FlareDrawMode DrawMode
	{
		get
		{
			return _drawMode;
		}
		set
		{
			if (_drawMode != value)
			{
				_drawMode = value;
				switch (_drawMode)
				{
				case FlareDrawMode.FlyOver:
					_startingAltitude = ((_startingAltitude <= 0f) ? 30f : _startingAltitude);
					_finalAltitude = ((_finalAltitude <= 0f) ? 4f : _finalAltitude);
					break;
				case FlareDrawMode.Direct:
					_startingAltitude = 0f;
					_finalAltitude = 0f;
					break;
				}
			}
		}
	}

	public float Progress => (float)(GenTicks.TicksGame - spawnTick) / 2100f;

	public Vector3 DirectDrawPos
	{
		get
		{
			if (_directDrawPos.z == -1f)
			{
				Log.Error($"CE: Tried to draw a miss configured flare {this} at {base.Position} with unknown DirectDrawPos. Defaulting to base.DirectDrawPos");
				_directDrawPos = base.DrawPos;
			}
			return _directDrawPos;
		}
		set
		{
			_directDrawPos = value;
		}
	}

	public override Vector3 DrawPos
	{
		get
		{
			if (DrawMode == FlareDrawMode.FlyOver)
			{
				Vector3 drawPos = base.DrawPos;
				drawPos.y = CurAltitude;
				drawPos.z += CurAltitude * 0.7f;
				return drawPos;
			}
			return DirectDrawPos;
		}
	}

	private float CurAltitude => StartingAltitude * (1f - Progress) + FinalAltitude * Progress;

	private float HeightDrawScale => 1f - (Progress * 2f - 0.5f) / 2f;

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		if (spawnTick == -1)
		{
			spawnTick = GenTicks.TicksGame;
		}
		if (_drawMode == FlareDrawMode.Unknown)
		{
			Log.Error($"CE: Tried to draw a miss configured flare {this} at {base.Position} with unknown FlareDrawMode. Defaulting to flyover");
			DrawMode = FlareDrawMode.FlyOver;
		}
		base.SpawnSetup(map, respawningAfterLoad);
	}

	public override void ExposeData()
	{
		Scribe_Values.Look(ref _drawMode, "drawMode", FlareDrawMode.Unknown);
		Scribe_Values.Look(ref _finalAltitude, "_finalAltitude", 0f);
		Scribe_Values.Look(ref _startingAltitude, "_startingAltitude", 0f);
		Scribe_Values.Look(ref spawnTick, "spawnTick", -1);
		base.ExposeData();
	}

	public override void Tick()
	{
		base.Tick();
		if (nextSmokePuff <= 0)
		{
			if (Multiplayer.InMultiplayer)
			{
				Rand.PushState();
			}
			MoteThrownCE moteThrownCE = (MoteThrownCE)ThingMaker.MakeThing(CE_ThingDefOf.Mote_FlareSmoke);
			moteThrownCE.Scale = Rand.Range(1.5f, 2.5f) * Rand.Range(0.15f, 1.25f) * HeightDrawScale;
			moteThrownCE.rotationRate = Rand.Range(-30f, 30f);
			moteThrownCE.SetVelocity(Rand.Range(30, 40), Rand.Range(0.5f, 0.7f));
			((Thing)moteThrownCE).positionInt = base.Position;
			moteThrownCE.exactPosition = DrawPos;
			moteThrownCE.drawOffset = _moteDrawOffset;
			moteThrownCE.attachedAltitudeThing = this;
			moteThrownCE.SpawnSetup(base.Map, respawningAfterLoad: false);
			MoteThrownCE moteThrownCE2 = (MoteThrownCE)ThingMaker.MakeThing(CE_ThingDefOf.Mote_FlareGlow);
			moteThrownCE2.Scale = Rand.Range(4f, 6f) * 0.6f * HeightDrawScale;
			moteThrownCE2.rotationRate = Rand.Range(-3f, 3f);
			moteThrownCE2.SetVelocity(Rand.Range(0, 360), 0.12f);
			((Thing)moteThrownCE2).positionInt = base.Position;
			moteThrownCE2.drawOffset = _moteDrawOffset;
			moteThrownCE2.exactPosition = DrawPos;
			moteThrownCE2.attachedAltitudeThing = this;
			moteThrownCE2.SpawnSetup(base.Map, respawningAfterLoad: false);
			if (CurAltitude < 20f)
			{
				FleckMaker.ThrowFireGlow(DrawPos, base.Map, 4f * (1f - (CurAltitude - FinalAltitude) / (20f - FinalAltitude)));
			}
			if (CurAltitude < 6f)
			{
				FleckMaker.WaterSplash(base.Position.ToVector3Shifted(), base.Map, Rand.Range(0.8f, 1.2f) * 2.5f * (1f - (CurAltitude - FinalAltitude) / (6f - FinalAltitude)), 5f);
			}
			if (Multiplayer.InMultiplayer)
			{
				Rand.PopState();
			}
			nextSmokePuff = Rand.Range(7, 21);
		}
		nextSmokePuff--;
		if (Progress >= 0.99f)
		{
			Destroy();
		}
	}

	public override void DrawAt(Vector3 drawLoc, bool flip = false)
	{
		float heightDrawScale = HeightDrawScale;
		Matrix4x4 matrix = default(Matrix4x4);
		Rot4 rotation = base.Rotation;
		matrix.SetTRS(DrawPos + Graphic.DrawOffset(rotation), Graphic.QuatFromRot(rotation), new Vector3(heightDrawScale, 0f, heightDrawScale));
		Graphics.DrawMesh(Graphic.MeshAt(rotation), matrix, Graphic.MatAt(rotation), 0);
	}
}
