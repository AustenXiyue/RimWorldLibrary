using RimWorld;
using Verse.AI;

namespace CombatExtended;

public class JobDriver_WaitKnockdown : JobDriver_Wait
{
	public override void SetInitialPosture()
	{
		pawn.jobs.posture = PawnPosture.LayingOnGroundNormal;
	}
}
