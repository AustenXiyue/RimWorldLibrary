using Verse;
using Verse.AI;

namespace CombatExtended;

public class PawnRenderNodeWorker_Drafted : PawnRenderNodeWorker
{
	public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
	{
		Pawn pawn = node.tree.pawn;
		if (pawn == null || !pawn.Spawned)
		{
			goto IL_0085;
		}
		if (!pawn.Drafted)
		{
			Job curJob = pawn.CurJob;
			if ((curJob == null || !curJob.def.alwaysShowWeapon) && pawn.mindState?.duty?.def.alwaysShowWeapon != true)
			{
				goto IL_0085;
			}
		}
		int result = (base.CanDrawNow(node, parms) ? 1 : 0);
		goto IL_0086;
		IL_0085:
		result = 0;
		goto IL_0086;
		IL_0086:
		return (byte)result != 0;
	}
}
