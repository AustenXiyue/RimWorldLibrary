using UnityEngine;
using Verse;

namespace AncotLibrary;

public class PawnRenderNodeWorker_PhysicalShield_Apparel : PawnRenderNodeWorker_FlipWhenCrawling
{
	public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
	{
		if (base.CanDrawNow(node, parms))
		{
			return true;
		}
		return false;
	}

	public override Vector3 OffsetFor(PawnRenderNode node, PawnDrawParms parms, out Vector3 pivot)
	{
		pivot = PivotFor(node, parms);
		CompPhysicalShield compPhysicalShield = node.apparel.TryGetComp<CompPhysicalShield>();
		return compPhysicalShield.impactAngleVect * 0.02f;
	}

	public override float LayerFor(PawnRenderNode node, PawnDrawParms parms)
	{
		CompPhysicalShield compPhysicalShield = node.apparel.TryGetComp<CompPhysicalShield>();
		Pawn pawn = parms.pawn;
		float num = 0f;
		if (compPhysicalShield == null)
		{
			return 0f;
		}
		num = ((compPhysicalShield.ShieldState == An_ShieldState.Active) ? ((pawn.Rotation.AsAngle != 0f) ? 75f : (-5f)) : ((compPhysicalShield.ShieldState == An_ShieldState.Ready || compPhysicalShield.ShieldState == An_ShieldState.Resetting) ? ((pawn.Rotation.AsAngle != 180f) ? (-5f) : 75f) : ((pawn.Rotation.AsAngle != 180f) ? 90f : (-5f))));
		return num + node.debugLayerOffset;
	}
}
