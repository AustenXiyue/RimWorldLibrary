using UnityEngine;
using Verse;

namespace CombatExtended;

public class ProjectileCE_Flare : ProjectileCE_Explosive
{
	private const float FLYOVER_FLARING_CHANCE = 0.2f;

	private const float FLYOVER_FLARING_HEIGHT = 30f;

	private bool decentStarted = false;

	private float _originToTargetDis = -1f;

	private float OriginToTargetDist
	{
		get
		{
			if (_originToTargetDis == -1f)
			{
				_originToTargetDis = Vector3.Distance(intendedTarget.CenterVector3, new Vector3(origin.x, 0f, origin.y));
			}
			return _originToTargetDis;
		}
	}

	private float CurrentDist => Vector3.Distance(intendedTarget.CenterVector3, ExactPosition.Yto0());

	public override void Tick()
	{
		base.Tick();
		if (decentStarted)
		{
			if (ExactPosition.y <= 30f && Rand.Chance(0.2f))
			{
				Impact(null);
			}
		}
		else if (OriginToTargetDist / 2f > CurrentDist)
		{
			decentStarted = true;
		}
	}

	public override void Impact(Thing hitThing)
	{
		landed = true;
		Flare flare = (Flare)ThingMaker.MakeThing(CE_ThingDefOf.Flare);
		flare.DrawMode = Flare.FlareDrawMode.FlyOver;
		flare.StartingAltitude = ExactPosition.y;
		flare.Position = base.Position;
		flare.SpawnSetup(base.Map, respawningAfterLoad: false);
		base.Impact((Thing)null);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref decentStarted, "decentStarted", defaultValue: false);
	}
}
