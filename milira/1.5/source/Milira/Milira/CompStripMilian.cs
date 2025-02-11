using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Milira;

public class CompStripMilian : ThingComp
{
	public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
	{
		Pawn milian = parent as Pawn;
		if (!selPawn.RaceProps.ToolUser || !milian.IsColonyMechPlayerControlled || !selPawn.CanReserveAndReach(parent, PathEndMode.InteractionCell, Danger.Deadly))
		{
			yield break;
		}
		yield return new FloatMenuOption("Milira_StripMilian".Translate().Formatted(parent), delegate
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			if (milian.equipment.Primary != null)
			{
				ThingWithComps weapon = milian.equipment.Primary;
				list.Add(new FloatMenuOption(weapon.LabelCap, delegate
				{
					Job job = JobMaker.MakeJob(MiliraDefOf.Milira_StripMilian_Weapon, milian, weapon);
					job.count = 1;
					selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
				}));
			}
			List<Apparel> wornApparel = milian.apparel.WornApparel;
			for (int i = 0; i < wornApparel.Count; i++)
			{
				Apparel currentApparel = wornApparel[i];
				list.Add(new FloatMenuOption(currentApparel.LabelCap, delegate
				{
					Job job2 = JobMaker.MakeJob(MiliraDefOf.Milira_StripMilian_Apparel, milian, currentApparel);
					job2.count = 1;
					selPawn.jobs.TryTakeOrderedJob(job2, JobTag.Misc);
				}));
			}
			Find.WindowStack.Add(new FloatMenu(list));
		});
	}
}
