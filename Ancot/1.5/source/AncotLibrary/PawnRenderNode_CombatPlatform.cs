using Verse;

namespace AncotLibrary;

public class PawnRenderNode_CombatPlatform : PawnRenderNode
{
	public CompCombatPlatform compCombatPlatform;

	public PawnRenderNode_CombatPlatform(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
		: base(pawn, props, tree)
	{
	}

	public override Graphic GraphicFor(Pawn pawn)
	{
		if (base.Props.texPath != null)
		{
			return GraphicDatabase.Get<Graphic_Multi>(base.Props.texPath, ShaderDatabase.Cutout);
		}
		return null;
	}
}
