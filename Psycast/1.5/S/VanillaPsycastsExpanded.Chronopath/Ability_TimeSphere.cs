using RimWorld.Planet;
using Verse;
using VFECore.Abilities;

namespace VanillaPsycastsExpanded.Chronopath;

public class Ability_TimeSphere : Ability
{
	public override void Cast(params GlobalTargetInfo[] targets)
	{
		((Ability)this).Cast(targets);
		for (int i = 0; i < targets.Length; i++)
		{
			GlobalTargetInfo globalTargetInfo = targets[i];
			TimeSphere timeSphere = (TimeSphere)ThingMaker.MakeThing(VPE_DefOf.VPE_TimeSphere);
			timeSphere.Duration = ((Ability)this).GetDurationForPawn();
			timeSphere.Radius = ((Ability)this).GetRadiusForPawn();
			GenSpawn.Spawn(timeSphere, globalTargetInfo.Cell, globalTargetInfo.Map);
		}
	}
}
