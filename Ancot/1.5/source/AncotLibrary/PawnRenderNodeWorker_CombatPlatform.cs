using UnityEngine;
using Verse;

namespace AncotLibrary;

public class PawnRenderNodeWorker_CombatPlatform : PawnRenderNodeWorker
{
	public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
	{
		if (!base.CanDrawNow(node, parms))
		{
			return false;
		}
		PawnRenderNode_CombatPlatform pawnRenderNode_CombatPlatform = node as PawnRenderNode_CombatPlatform;
		PawnRenderNodeProperties_CombatPlatform pawnRenderNodeProperties_CombatPlatform = node.Props as PawnRenderNodeProperties_CombatPlatform;
		if (pawnRenderNodeProperties_CombatPlatform.isApparel)
		{
			pawnRenderNode_CombatPlatform.compCombatPlatform = pawnRenderNode_CombatPlatform.apparel.TryGetComp<CompCombatPlatform>();
		}
		if (pawnRenderNode_CombatPlatform.compCombatPlatform != null)
		{
			if (!pawnRenderNodeProperties_CombatPlatform.drawUndrafted && !pawnRenderNode_CombatPlatform.compCombatPlatform.PawnOwner.Drafted)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public override Quaternion RotationFor(PawnRenderNode node, PawnDrawParms parms)
	{
		Quaternion result = base.RotationFor(node, parms);
		PawnRenderNode_CombatPlatform pawnRenderNode_CombatPlatform = node as PawnRenderNode_CombatPlatform;
		PawnRenderNodeProperties_CombatPlatform pawnRenderNodeProperties_CombatPlatform = node.Props as PawnRenderNodeProperties_CombatPlatform;
		if (pawnRenderNodeProperties_CombatPlatform.isApparel)
		{
			pawnRenderNode_CombatPlatform.compCombatPlatform = pawnRenderNode_CombatPlatform.apparel.TryGetComp<CompCombatPlatform>();
		}
		return result;
	}

	public override Vector3 OffsetFor(PawnRenderNode node, PawnDrawParms parms, out Vector3 pivot)
	{
		PawnRenderNode_CombatPlatform pawnRenderNode_CombatPlatform = node as PawnRenderNode_CombatPlatform;
		PawnRenderNodeProperties_CombatPlatform pawnRenderNodeProperties_CombatPlatform = node.Props as PawnRenderNodeProperties_CombatPlatform;
		if (pawnRenderNodeProperties_CombatPlatform.isApparel)
		{
			pawnRenderNode_CombatPlatform.compCombatPlatform = pawnRenderNode_CombatPlatform.apparel.TryGetComp<CompCombatPlatform>();
		}
		if (pawnRenderNode_CombatPlatform != null && pawnRenderNode_CombatPlatform.compCombatPlatform != null)
		{
			Vector3 vector = new Vector3(pawnRenderNode_CombatPlatform.compCombatPlatform.floatOffset_xAxis, 0f, pawnRenderNode_CombatPlatform.compCombatPlatform.floatOffset_yAxis);
			return base.OffsetFor(node, parms, out pivot) + vector;
		}
		return base.OffsetFor(node, parms, out pivot);
	}
}
