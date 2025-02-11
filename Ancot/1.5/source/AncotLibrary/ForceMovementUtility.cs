using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace AncotLibrary;

public static class ForceMovementUtility
{
	public static void ApplyRepulsiveForce(IntVec3 origin, Pawn victim, float distance, List<HediffDef> removeHediffsAffected = null, bool ignoreResistance = false, float fieldStrength = 1f)
	{
		Vector3 vector = (victim.Position - origin).ToVector3();
		vector.Normalize();
		IntVec3 position = victim.Position;
		if (!ignoreResistance)
		{
			distance = Math.Max(0f, distance * (fieldStrength - victim.GetStatValue(AncotDefOf.Ancot_FieldForceResistance)));
		}
		for (int i = 0; (float)i < distance; i++)
		{
			Vector3 vect = i * vector;
			IntVec3 intVec = victim.Position + vect.ToIntVec3();
			if (!intVec.InBounds(victim.Map) || !intVec.Walkable(victim.Map) || !GenSight.LineOfSight(origin, intVec, victim.Map))
			{
				break;
			}
			position = intVec;
		}
		bool flag = true;
		victim.Position = position;
		if (!removeHediffsAffected.NullOrEmpty())
		{
			for (int j = 0; j < removeHediffsAffected.Count; j++)
			{
				Hediff firstHediffOfDef = victim.health.hediffSet.GetFirstHediffOfDef(removeHediffsAffected[j]);
				if (firstHediffOfDef != null)
				{
					victim.health.RemoveHediff(firstHediffOfDef);
				}
			}
		}
		victim.pather.StopDead();
		victim.jobs.StopAll();
	}

	public static void ApplyGravitationalForce(IntVec3 origin, Pawn victim, float distance, List<HediffDef> removeHediffsAffected = null, bool ignoreResistance = false, float fieldStrength = 1f)
	{
		Vector3 vector = (origin - victim.Position).ToVector3();
		vector.Normalize();
		IntVec3 position = victim.Position;
		if (!ignoreResistance)
		{
			distance = Math.Max(0f, distance * (fieldStrength - victim.GetStatValue(AncotDefOf.Ancot_FieldForceResistance)));
		}
		for (int i = 0; (float)i < distance; i++)
		{
			Vector3 vect = i * vector;
			IntVec3 intVec = victim.Position + vect.ToIntVec3();
			if (!intVec.InBounds(victim.Map) || !intVec.Walkable(victim.Map) || !GenSight.LineOfSight(victim.PositionHeld, intVec, victim.Map))
			{
				break;
			}
			if (intVec == origin)
			{
				position = origin;
				break;
			}
			position = intVec;
		}
		bool flag = true;
		victim.Position = position;
		if (!removeHediffsAffected.NullOrEmpty())
		{
			for (int j = 0; j < removeHediffsAffected.Count; j++)
			{
				Hediff firstHediffOfDef = victim.health.hediffSet.GetFirstHediffOfDef(removeHediffsAffected[j]);
				if (firstHediffOfDef != null)
				{
					victim.health.RemoveHediff(firstHediffOfDef);
				}
			}
		}
		victim.pather.StopDead();
		victim.jobs.StopAll();
	}
}
