using UnityEngine;
using Verse;

namespace AncotLibrary;

public class PawnRenderNode_PhysicalShield_Apparel : PawnRenderNode
{
	private PawnDrawParms drawParms;

	public PawnRenderNode_PhysicalShield_Apparel(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
		: base(pawn, props, tree)
	{
	}

	public override GraphicMeshSet MeshSetFor(Pawn pawn)
	{
		if (props.overrideMeshSize.HasValue)
		{
			return MeshPool.GetMeshSetForSize(props.overrideMeshSize.Value.x, props.overrideMeshSize.Value.y);
		}
		return HumanlikeMeshPoolUtility.GetHumanlikeHeadSetForPawn(pawn);
	}

	public override Graphic GraphicFor(Pawn pawn)
	{
		CompPhysicalShield compPhysicalShield = apparel.TryGetComp<CompPhysicalShield>();
		return GraphicDatabase.Get<Graphic_Multi>(compPhysicalShield.ShieldState switch
		{
			An_ShieldState.Active => compPhysicalShield.graphicPath_Holding, 
			An_ShieldState.Ready => compPhysicalShield.graphicPath_Ready, 
			An_ShieldState.Resetting => compPhysicalShield.graphicPath_Ready, 
			_ => compPhysicalShield.graphicPath_Disabled, 
		}, ShaderDatabase.Cutout, Vector2.one, ColorFor(pawn));
	}

	public override Color ColorFor(Pawn pawn)
	{
		Color result = Color.white;
		if (apparel.def.MadeFromStuff)
		{
			result = apparel.Stuff.stuffProps.color;
		}
		return result;
	}
}
