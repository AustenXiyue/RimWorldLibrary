using UnityEngine;
using Verse;

namespace Milira;

public class PawnRenderNodeWorker_MilianHair : PawnRenderNodeWorker_FlipWhenCrawling
{
	public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
	{
		if (base.CanDrawNow(node, parms))
		{
			CompMilianHairSwitch compMilianHairSwitch = parms.pawn.TryGetComp<CompMilianHairSwitch>();
			return compMilianHairSwitch.drawHair;
		}
		return false;
	}

	public override MaterialPropertyBlock GetMaterialPropertyBlock(PawnRenderNode node, Material material, PawnDrawParms parms)
	{
		if (node.Graphic == null)
		{
			return null;
		}
		MaterialPropertyBlock matPropBlock = node.MatPropBlock;
		matPropBlock.SetColor(ShaderPropertyIDs.Color, parms.tint * material.color);
		PawnRenderUtility.SetMatPropBlockOverlay(matPropBlock, parms.pawn.Faction.MechColor);
		return matPropBlock;
	}
}
