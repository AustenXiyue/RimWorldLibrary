using RimWorld;
using UnityEngine;
using Verse;

namespace AncotLibrary;

public class CompCombatPlatform : ThingComp
{
	public float floatOffset_xAxis = 0f;

	public float floatOffset_yAxis = 0f;

	public float randTime = Rand.Range(0f, 300f);

	public CompProperties_CombatPlatform Props => (CompProperties_CombatPlatform)props;

	public Pawn PawnOwner
	{
		get
		{
			if (!(parent is Apparel { Wearer: var wearer }))
			{
				if (parent is Pawn result)
				{
					return result;
				}
				return null;
			}
			return wearer;
		}
	}

	public override void CompTick()
	{
		base.CompTick();
		if (Props.float_xAxis != null)
		{
			floatOffset_xAxis = Mathf.Sin(((float)Find.TickManager.TicksGame + randTime) * Props.float_xAxis.floatSpeed) * Props.float_xAxis.floatAmplitude;
		}
		if (Props.float_yAxis != null)
		{
			floatOffset_yAxis = Mathf.Sin(((float)Find.TickManager.TicksGame + randTime) * Props.float_yAxis.floatSpeed) * Props.float_yAxis.floatAmplitude;
		}
	}
}
