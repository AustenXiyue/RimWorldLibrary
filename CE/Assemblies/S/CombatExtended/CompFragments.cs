using System;
using System.Collections;
using CombatExtended.Compatibility;
using UnityEngine;
using Verse;

namespace CombatExtended;

[StaticConstructorOnStartup]
public class CompFragments : ThingComp
{
	private class MonoDummy : MonoBehaviour
	{
	}

	private const int TicksToSpawnAllFrag = 10;

	private static MonoDummy _monoDummy;

	public CompProperties_Fragments PropsCE => (CompProperties_Fragments)props;

	static CompFragments()
	{
		GameObject gameObject = new GameObject();
		UnityEngine.Object.DontDestroyOnLoad(gameObject);
		_monoDummy = gameObject.AddComponent<MonoDummy>();
	}

	public static IEnumerator FragRoutine(Vector3 pos, Map map, float height, Thing instigator, ThingDefCountClass frag, float fragSpeedFactor, float fragShadowChance, FloatRange fragAngleRange, FloatRange fragXZAngleRange, float minCollisionDistance = 0f, bool canTargetSelf = true)
	{
		if (height < 0.001f)
		{
			height = 0.001f;
		}
		IntVec3 cell = pos.ToIntVec3();
		Vector2 exactOrigin = new Vector2(pos.x, pos.z);
		int fragToSpawn = frag.count;
		int fragPerTick = Mathf.CeilToInt((float)fragToSpawn / 10f);
		int fragSpawnedInTick = 0;
		FloatRange fragAngleSinRange = new FloatRange(Mathf.Sin(fragAngleRange.min * ((float)Math.PI / 180f)), Mathf.Sin(fragAngleRange.max * ((float)Math.PI / 180f)));
		while (fragToSpawn-- > 0)
		{
			ProjectileCE projectile = (ProjectileCE)ThingMaker.MakeThing(frag.thingDef);
			GenSpawn.Spawn(projectile, cell, map);
			projectile.canTargetSelf = canTargetSelf;
			projectile.minCollisionDistance = minCollisionDistance;
			projectile.logMisses = false;
			float elevAngle = Mathf.Asin(fragAngleSinRange.RandomInRange);
			projectile.Launch(instigator, exactOrigin, elevAngle, (fragXZAngleRange.RandomInRange + 360f) % 360f, height, fragSpeedFactor * projectile.def.projectile.speed, projectile);
			projectile.castShadow = Rand.Value < fragShadowChance;
			fragSpawnedInTick++;
			if (fragSpawnedInTick >= fragPerTick)
			{
				fragSpawnedInTick = 0;
				yield return new WaitForEndOfFrame();
				if (Find.Maps.IndexOf(map) < 0)
				{
					break;
				}
			}
		}
	}

	public void Throw(Vector3 pos, Map map, Thing instigator, float scaleFactor = 1f)
	{
		if (PropsCE.fragments.NullOrEmpty())
		{
			return;
		}
		if (map == null)
		{
			Log.Warning("CombatExtended :: Tried to throw fragments in a null map.");
			return;
		}
		if (!pos.ToIntVec3().InBounds(map))
		{
			Log.Warning("CombatExtended :: Tried to throw fragments out of bounds");
			return;
		}
		float height;
		FloatRange fragXZAngleRange;
		if (parent is ProjectileCE projectileCE)
		{
			height = projectileCE.ExactPosition.y;
			fragXZAngleRange = new FloatRange(projectileCE.shotRotation + PropsCE.fragXZAngleRange.min, projectileCE.shotRotation + PropsCE.fragXZAngleRange.max);
		}
		else
		{
			height = 0f;
			fragXZAngleRange = PropsCE.fragXZAngleRange;
		}
		foreach (ThingDefCountClass fragment in PropsCE.fragments)
		{
			ThingDefCountClass thingDefCountClass = fragment;
			thingDefCountClass.count = Mathf.RoundToInt((float)thingDefCountClass.count * scaleFactor);
			IEnumerator enumerator2 = FragRoutine(pos, map, height, instigator, fragment, PropsCE.fragSpeedFactor, PropsCE.fragShadowChance, PropsCE.fragAngleRange, fragXZAngleRange);
			if (!Multiplayer.InMultiplayer)
			{
				_monoDummy.GetComponent<MonoDummy>().StartCoroutine(enumerator2);
			}
			else
			{
				while (enumerator2.MoveNext())
				{
				}
			}
		}
	}
}
