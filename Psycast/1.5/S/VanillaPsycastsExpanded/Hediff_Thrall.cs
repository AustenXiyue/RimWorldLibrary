using Verse;

namespace VanillaPsycastsExpanded;

public class Hediff_Thrall : HediffWithComps
{
	public override void Tick()
	{
		base.Tick();
		if (Find.TickManager.TicksGame % 60 != 0)
		{
		}
	}
}
