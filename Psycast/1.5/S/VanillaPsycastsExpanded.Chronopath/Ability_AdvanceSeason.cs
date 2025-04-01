using RimWorld.Planet;
using UnityEngine;
using Verse;
using VFECore.Abilities;

namespace VanillaPsycastsExpanded.Chronopath;

public class Ability_AdvanceSeason : Ability
{
	private int ticksAdvanceLeft;

	public override void Cast(params GlobalTargetInfo[] targets)
	{
		((Ability)this).Cast(targets);
		ticksAdvanceLeft = Mathf.CeilToInt(360f);
	}

	public override void Tick()
	{
		((Ability)this).Tick();
		if (ticksAdvanceLeft > 0)
		{
			ticksAdvanceLeft--;
			Find.TickManager.DebugSetTicksGame(Find.TickManager.TicksGame + 2500);
		}
	}

	public override void ExposeData()
	{
		((Ability)this).ExposeData();
		Scribe_Values.Look(ref ticksAdvanceLeft, "ticksAdvanceLeft", 0);
	}
}
