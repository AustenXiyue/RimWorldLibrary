using System;
using UnityEngine;
using Verse;

namespace CombatExtended;

[StaticConstructorOnStartup]
public class CompCharges : ThingComp
{
	private static float MaxRangeAngle = (float)Math.PI / 4f;

	public CompProperties_Charges Props => (CompProperties_Charges)props;

	public bool GetChargeBracket(float range, float shotHeight, float gravityFactor, out Vector2 bracket)
	{
		bracket = new Vector2(0f, 0f);
		if (Props.chargeSpeeds.Count <= 0)
		{
			Log.Error("Tried getting charge bracket from empty list.");
			return false;
		}
		foreach (int chargeSpeed in Props.chargeSpeeds)
		{
			float num = CE_Utility.MaxProjectileRange(shotHeight, chargeSpeed, MaxRangeAngle, gravityFactor);
			if (range <= num)
			{
				bracket = new Vector2(chargeSpeed, num);
				return true;
			}
		}
		return false;
	}
}
