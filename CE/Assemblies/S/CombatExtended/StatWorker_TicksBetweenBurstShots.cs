using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended;

public class StatWorker_TicksBetweenBurstShots : StatWorker
{
	public override string ValueToString(float val, bool finalized, ToStringNumberSense numberSense = ToStringNumberSense.Absolute)
	{
		val = Mathf.Ceil(60f / val);
		return base.ValueToString(val, finalized, numberSense);
	}
}
