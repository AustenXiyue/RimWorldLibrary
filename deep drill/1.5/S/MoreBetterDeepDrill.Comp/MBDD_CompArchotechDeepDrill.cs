using System.Collections.Generic;
using MoreBetterDeepDrill.Settings;
using MoreBetterDeepDrill.Types;
using MoreBetterDeepDrill.Utils;
using RimWorld;
using UnityEngine;
using Verse;

namespace MoreBetterDeepDrill.Comp;

public class MBDD_CompArchotechDeepDrill : MBDD_CompDeepDrill
{
	protected DrillableOre selectedOre;

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Deep.Look(ref selectedOre, "selectedOre");
	}

	protected override void TryProducePortion(float yieldPct, Pawn driller = null)
	{
		if (selectedOre == null)
		{
			Messages.Message("DeepDrillExhaustedNoFallback".Translate(), parent, MessageTypeDefOf.TaskCompletion);
			return;
		}
		Thing thing = ThingMaker.MakeThing(selectedOre.OreDef);
		thing.stackCount = Mathf.Max(1, GenMath.RoundRandom((float)selectedOre.amountPerPortion * yieldPct));
		GenPlace.TryPlaceThing(thing, parent.InteractionCell, parent.Map, ThingPlaceMode.Near, null, (IntVec3 p) => p != parent.Position && p != parent.InteractionCell);
		if (driller != null)
		{
			Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.Mined, driller.Named(HistoryEventArgsNames.Doer)));
		}
	}

	protected override void UpdateCanDrillState()
	{
		if (powerComp != null && !powerComp.PowerOn)
		{
			CanDrillNow = false;
		}
		else if (selectedOre == null)
		{
			CanDrillNow = false;
		}
		else
		{
			CanDrillNow = true;
		}
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		foreach (Gizmo item2 in base.CompGetGizmosExtra())
		{
			yield return item2;
		}
		Command_Action action_selectOre = new Command_Action();
		action_selectOre.defaultLabel = "MBDD_ArchotechDeepDrill_Gizmo_SelectOre".Translate();
		if (selectedOre != null)
		{
			action_selectOre.icon = selectedOre.OreDef.uiIcon;
		}
		else
		{
			action_selectOre.icon = ThingDefOf.DeepDrill.uiIcon;
		}
		action_selectOre.Disabled = StaticValues.ModSetting.oreDictionary == null || StaticValues.ModSetting.oreDictionary.Count <= 0;
		action_selectOre.disabledReason = "MBDD_ArchotechDeepDrill_Gizmo_NoOre".Translate();
		action_selectOre.action = delegate
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			using (IEnumerator<DrillableOre> enumerator2 = (object)MBDD_Mod.ModSetting.oreDictionary.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					DrillableOre ore = enumerator2.Current;
					FloatMenuOption item = new FloatMenuOption("MBDD_ArchotechDeepDrill_FloatMenu_SelectOre".Translate() + ore.OreDef.LabelCap, delegate
					{
						selectedOre = ore;
					})
					{
						Disabled = (selectedOre == ore)
					};
					action_selectOre.disabledReason = "MBDD_ArchotechDeepDrill_FloatMenu_SameOre".Translate();
					list.Add(item);
				}
			}
			if (list.Count != 0)
			{
				FloatMenu window = new FloatMenu(list);
				Find.WindowStack.Add(window);
			}
		};
		yield return action_selectOre;
	}

	public override string CompInspectStringExtra()
	{
		if (parent.Spawned && selectedOre != null)
		{
			if (DebugSettings.ShowDevGizmos)
			{
				return "ResourceBelow".Translate() + ": " + selectedOre.OreDef.LabelCap + "\n" + "ProgressToNextPortion".Translate() + ": " + base.ProgressToNextPortionPercent.ToStringPercent("F0") + $"\nPortionYieldPct: {base.PortionYieldPct}\nDrillPower: {base.DrillPower}";
			}
			return "ResourceBelow".Translate() + ": " + selectedOre.OreDef.LabelCap + "\n" + "ProgressToNextPortion".Translate() + ": " + base.ProgressToNextPortionPercent.ToStringPercent("F0");
		}
		return null;
	}
}
