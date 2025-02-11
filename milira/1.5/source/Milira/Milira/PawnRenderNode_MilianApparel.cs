using RimWorld;
using Verse;

namespace Milira;

public class PawnRenderNode_MilianApparel : PawnRenderNode
{
	public PawnRenderNode_MilianApparel(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree, Apparel apparel)
		: base(pawn, props, tree)
	{
		base.apparel = apparel;
	}

	protected override void EnsureMaterialsInitialized()
	{
		if (graphic == null && ApparelGraphicRecordGetter.TryGetGraphicApparel(apparel, BodyTypeDefOf.Female, out var rec))
		{
			graphic = rec.graphic;
		}
	}
}
