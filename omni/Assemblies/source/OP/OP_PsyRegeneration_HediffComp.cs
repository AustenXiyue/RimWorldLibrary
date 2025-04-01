using Verse;

namespace OP;

public class OP_PsyRegeneration_HediffComp : HediffComp
{
	public int OP_Tick = 0;

	public OP_RemoveHeddif_HediffCompProperties Props => (OP_RemoveHeddif_HediffCompProperties)props;

	public override void CompPostMake()
	{
		base.CompPostMake();
		OP_Tick = 0;
	}

	public override void CompPostTick(ref float severityAdjustment)
	{
		OP_Tick++;
		if ((double)base.Pawn.psychicEntropy.CurrentPsyfocus >= 0.0 && OP_Tick >= Props.OP_Tick)
		{
			base.Pawn.psychicEntropy?.OffsetPsyfocusDirectly(Props.OP_Psycast);
			if (OP_Mod.ModSetting.EnableClearEntropy)
			{
				base.Pawn.psychicEntropy?.RemoveAllEntropy();
			}
			OP_Tick = 0;
		}
		if ((double)base.Pawn.psychicEntropy.CurrentPsyfocus == 0.0)
		{
			parent.pawn.health.RemoveHediff(parent);
		}
	}

	public override void CompExposeData()
	{
		Scribe_Values.Look(ref OP_Tick, "OP_Tick", 0);
	}
}
